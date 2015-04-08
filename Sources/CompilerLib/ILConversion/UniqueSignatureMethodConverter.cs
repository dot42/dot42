using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Naming;
using Dot42.CompilerLib.Reachable;
using Dot42.LoaderLib.Extensions;
using Dot42.Utility;
using Mono.Cecil;

namespace Dot42.CompilerLib.ILConversion
{
    /// <summary>
    /// this class makes sure all methods have a unique name in the target namespace:
    /// </summary>
    [Export(typeof (ILConverterFactory))]
    internal class UniqueSignatureMethodConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return int.MaxValue; } // Do this at the very latest.
        }

        /// <summary>
        /// Create the converter
        /// </summary>
        public ILConverter Create()
        {
            return new Converter();
        }

        private class Converter : ILConverter
        {
            private List<MethodDefinition> reachableMethods;
            private NameSet methodNames;

            /// <summary>
            /// Rename "new" methods that override a final base method.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                // Only consider 
                //   - methods that don't implement and interface or overwrite another method
                //   - are not getters or setters
                //   - are not event-adders or removers
                //   - do not generics-rename Nullable<T>, as it is used heavily by the compiler.
                //   - don't belong to Dot42.Internal namespace (as these are compiler-used)
                //   - [think about this: ] don't belong to interfaces in the System.* namespace (to reduce clutter; we know there are no clashes there)
                var consideredMethods = reachableContext.ReachableTypes.SelectMany(x => x.Methods)
                                                        .Where(m => !m.IsRuntimeSpecialName
                                                                    && m.GetDexOrJavaImportAttribute() == null
                                                                    &&!m.IsGetter && !m.IsSetter
                                                                    &&!m.IsExplicitImplementation()
                                                                    &&!DontConsiderForMethodRenaming(m.DeclaringType)
                                                                    && m.GetBaseMethod() == null
                                                                    && m.GetBaseInterfaceMethod() == null)
                                                        .ToList();

                // Go over each method, check if it needs renaming
                var methodsToRename = new Dictionary<MethodDefinition, string>();
                foreach (var method in consideredMethods)
                {
                    var postfix = CreateSignaturePostfix(method);
                    if(string.IsNullOrEmpty(postfix))
                        continue;

                    methodsToRename.Add(method, postfix);
                }

                var orderedRenames = methodsToRename.OrderBy(p => p.Key.DeclaringType.FullName)
                                                    .ThenBy(p => p.Key.MemberFullName())
                                                    .ThenBy(p => p.Key.FullName)
                                                    .ToList();
                if (orderedRenames.Count == 0)
                {
                }
                if (methodsToRename.Count == 0)
                    return;

                // Initialize some sets
                reachableMethods = reachableContext.ReachableTypes
                                                   .SelectMany(x => x.Methods)
                                                   .Where(m => m.IsReachable)
                                                   .OrderBy(x => x.FullName)
                                                   .ToList();

                methodNames = new NameSet(reachableMethods.Select(m => m.Name));

                // Rename methods that need renaming
                foreach (var keyVal in methodsToRename)
                {
                    var method = keyVal.Key;
                    var postfix = keyVal.Value;

                    // Create new name
                    var newName = methodNames.GetUniqueName(method.Name + postfix);

                    // Find all methods that derive from method
                    var groupMethods = new HashSet<MethodDefinition> { method };

                    if (method.IsVirtual && !method.IsFinal)
                    {
                        foreach (var otherMethod in reachableMethods.Except(methodsToRename.Keys))
                        {
                            var renamedBaseMethod = otherMethod.GetBaseMethods()
                                            .Concat(otherMethod.GetBaseInterfaceMethods())
                                            .Concat(otherMethod.Overrides)
                                                               .FirstOrDefault(m => m == method);
                            if (renamedBaseMethod != null)
                            {
                                // Rename this other method as well
                                groupMethods.Add(otherMethod);
                            }
                        }
                    }

                    //// Add explicit implementations for interface methods 
                    //foreach (var iMethod in method.GetBaseInterfaceMethods())
                    //{
                    //    var iMethodIsJavaWithGenericParams = iMethod.IsJavaMethodWithGenericParams();
                    //    var explicitName = methodNames.GetUniqueName(method.DeclaringType.Name + "_" + iMethod.Name);
                    //    var stub = InterfaceHelper.CreateExplicitStub(method, explicitName, iMethod, iMethodIsJavaWithGenericParams);
                    //    stub.IsPrivate = true;
                    //    stub.IsFinal = false;
                    //    stub.IsVirtual = true;
                    //}

                    // Rename all methods in the group
                    foreach (var m in groupMethods)
                    {
                        Rename(m, newName);
                    }
                }
            }

            /// <summary>
            /// Rename the given method and all references to it from code.
            /// </summary>
            private void Rename(MethodDefinition method, string newName)
            {
                // Rename reference to method
                foreach (var body in reachableMethods.Select(x => x.Body).Where(x => x != null))
                {
                    var resolver = new GenericsResolver(method.DeclaringType);
                    foreach (var ins in body.Instructions.Where(x => x.Operand is MethodReference))
                    {
                        var methodRef = ((MethodReference) ins.Operand).GetElementMethod();
                        if (!ReferenceEquals(methodRef, method) && methodRef.AreSameIncludingDeclaringType(method, resolver.Resolve))
                        {
                            methodRef.Name = newName;
                        }
                    }
                }
                // Rename method itself
                method.Name = newName;
            }
        }

        /// <summary>
        /// Create a postfix for the name of the given method based on 
        /// - "unsigned" parameter types: these will be mapped to their signed counterparts; 
        /// - on 'ref' our 'out' status, as these will be mapped to arrays; 
        /// - on generic parameters (as these will be mapped to 'object');
        /// - on generic parameters of parameters, as these will be type-erased;
        /// - on generic method parameters, e.g. void Foo[T](), again type-erasure being the reason.
        /// 
        /// See also http://stackoverflow.com/questions/8808703/method-signature-in-c-sharp
        /// </summary>
        private static string CreateSignaturePostfix(MethodReference method)
        {
            var declaringType = method.DeclaringType;

            if (declaringType.IsArray)
                return string.Empty;
            if ((method.Name == ".ctor") || (method.Name == ".cctor"))
                return string.Empty;

            var declType = declaringType.GetElementType().Resolve();
            if ((declType != null) && (declType.IsDelegate()))
                return string.Empty;

            var methodDef = method.Resolve();

            // dont handle events.
            if (methodDef.IsAddOn || methodDef.IsRemoveOn)
                return "";

            var needsPostfix = false;
            var postfix = new StringBuilder("$$");

            const bool processGenerics = true;
            const bool forcePostfixOnParmType = true;

            List<ParameterDefinition> types = new List<ParameterDefinition>(method.Parameters);

            if (ReturnTypeSegregatesOverloads(method)) // Note that for CLR, the return type always segregates overloads.
                                                       // It doesn't for C#, and that's what we are aiming at at the moment.
                types.Add(new ParameterDefinition(method.ReturnType));

            foreach (var parm in types)
            {
                var parmTypeId = "";
                var parameterType = parm.ParameterType;

                if (parameterType.IsByReference)
                {
                    parmTypeId += "r";
                    parameterType = ((ByReferenceType)parameterType).ElementType;
                }

                if (parmTypeId != "" && forcePostfixOnParmType)
                    needsPostfix = true;

                var typeId = GetTypeId(parameterType, processGenerics);

                if (typeId != null)
                    needsPostfix = true;
                else if (parmTypeId == "")
                    typeId = "_";

                postfix.Append(parmTypeId + typeId);
            }

            // add marker for generic method parameters.
            if (processGenerics && methodDef.HasGenericParameters)
            {
                if (!needsPostfix)
                {
                    postfix.Clear();
                    postfix.Append("$$");
                    postfix.Append(methodDef.GenericParameters.Count);
                }
                else
                {
                    postfix.Insert(2, methodDef.GenericParameters.Count + "$");
                }
                needsPostfix = true;
            }


            if (needsPostfix)
            {
                return postfix.ToString();
            }

            return string.Empty;
        }

        private static string GetTypeId(TypeReference type, bool processGenerics)
        {
            var typeId = GetUnsignedPrimitivesTypeId(type);

            if (typeId == null && processGenerics)
            {
                typeId = GetGenericTypeId(type);
            }
            return typeId;
        }

        private static string GetGenericTypeId(TypeReference parameterType)
        {
            if (parameterType.IsGenericParameter)
                return "T"; // this will be represented by "object" in java
            if (parameterType.IsGenericInstance)
            {
                var git = (GenericInstanceType)parameterType;

                StringBuilder shortName = new StringBuilder();

                AppendGenericParameterIds(git, shortName);

                if (shortName.Length > 0)
                    return shortName.ToString();
            }
            return null;
        }

        private static void AppendGenericParameterIds(GenericInstanceType parameterType, StringBuilder shortName)
        {
            // these will be type-erasured.
            foreach (var paramParm in parameterType.GenericArguments)
            {
                if (paramParm.IsGenericParameter)
                {
                    // do nothing for generic parameters of generic types. this is the default.
                    // e.g. IEnumerable<T> will not be specially marked, while IEnumerable<int> will be below
                }
                else
                {
                    var paramElementType = paramParm.Resolve().GetElementType();
                    shortName.Append(paramElementType.Name.TakeWhile(c => c != '`').ToArray());

                    // for eventual names of generic parameters.
                    if (paramParm.IsGenericInstance)
                        AppendGenericParameterIds((GenericInstanceType)paramParm, shortName);
                }
            }
        }

        private static bool DontConsiderForMethodRenaming(TypeDefinition declaringType)
        {
            return declaringType.FullName.StartsWith("Dot42.Internal.")
               ||  declaringType.IsPrimitive// TODO: check this.
               ||  declaringType.GetElementType().IsNullableT()
               ||  declaringType.FullName == "System.String"
               || (declaringType.FullName.StartsWith("System") && declaringType.IsInterface);
        }
        //private static bool IsSystemInterface(TypeReference declaringType)
        //{
        //    return declaringType.FullName.StartsWith("System.") && declaringType.Resolve().IsInterface;
        //}

        //private static bool IsSystemType(TypeReference declaringType)
        //{
        //    return declaringType.FullName.StartsWith("System") || declaringType.FullName.StartsWith("Dot42");
        //}

        private static string GetUnsignedPrimitivesTypeId(TypeReference type)
        {
            if (type.IsPrimitive)
            {
                switch (type.MetadataType)
                {
                    case MetadataType.SByte:
                        return "B";
                    case MetadataType.UInt16:
                        return "S";
                    case MetadataType.UInt32:
                        return "I";
                    case MetadataType.UInt64:
                        return "J";
                }
            }
            return null;
        }

        private static bool ReturnTypeSegregatesOverloads(MethodReference method)
        {
            return method.Name.StartsWith("op_Explicit", StringComparison.OrdinalIgnoreCase);
        }
    }
}