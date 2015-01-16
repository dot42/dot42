using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Java;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper used to build method definitions from ClassFile's data
    /// </summary>
    internal class MethodBuilder : IBuilderGenericContext
    {
        private readonly MethodDefinition javaMethod;
        private readonly TypeBuilder declaringTypeBuilder;
        private readonly bool addJavaPrefix;
        private readonly bool convertSignedBytes;
        private NetMethodDefinition method;
        private DocMethod docMethod;

        /// <summary>
        /// Create method builders for the given method.
        /// </summary>
        internal static IEnumerable<MethodBuilder> Create(MethodDefinition javaMethod, TypeBuilder declaringTypeBuilder)
        {
            bool onlyInReturnType;
            var convertSignedBytes = ContainsSignedByte(javaMethod, out onlyInReturnType) && !AvoidConvertSignedBytes(javaMethod);
            if (onlyInReturnType && javaMethod.DeclaringClass.IsInterface)
            {
                yield return new MethodBuilder(javaMethod, declaringTypeBuilder, true, false);
            }
            else
            {
                var addJavaPrefix = convertSignedBytes && onlyInReturnType;
                yield return new MethodBuilder(javaMethod, declaringTypeBuilder, false, addJavaPrefix);
                if (convertSignedBytes)
                    yield return new MethodBuilder(javaMethod, declaringTypeBuilder, true, false);
            }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        private MethodBuilder(MethodDefinition javaMethod, TypeBuilder declaringTypeBuilder, bool convertSignedBytes, bool addJavaPrefix)
        {
            this.javaMethod = javaMethod;
            this.declaringTypeBuilder = declaringTypeBuilder;
            this.convertSignedBytes = convertSignedBytes;
            this.addJavaPrefix = addJavaPrefix;
        }

        /// <summary>
        /// Create the method in the given type
        /// </summary>
        public void Create(NetTypeDefinition declaringType, TargetFramework target)
        {
            try
            {
                // Do not add private methods
                if (javaMethod.IsPrivate)
                {
                    if (javaMethod.Name != "<init>")
                    {
                        return;
                    }
                }

                // Do not add useless bridges methods
                if (javaMethod.IsBridge)
                {
                    var targetMethod = javaMethod.DeclaringClass.Methods.FirstOrDefault(x => javaMethod.IsBridgeFor(x));
                    /*if (javaMethod.DeclaringClass.Methods.Any(x =>
                            (x != javaMethod) && (x.Name == javaMethod.Name) &&
                            (x.Parameters.Count == javaMethod.Parameters.Count) &&
                            (x.Descriptor != javaMethod.Descriptor)))*/
                    if (targetMethod != null)
                    {
                        if (!(targetMethod.IsAbstract && !javaMethod.IsAbstract))
                        {
                            return;
                        }
                    }
                }

                // We're using a dummy return type first.
                // Otherwise we cannot resolve generic return types.
                var signature = javaMethod.Signature;
                var nameInfo = declaringTypeBuilder.GetMethodName(javaMethod);

                var name = nameInfo.Name;
                if (nameInfo.IsConstructor)
                {
                    method = new NetMethodDefinition(".ctor", javaMethod, declaringType, target, convertSignedBytes, "MethodBuilder.Create")
                    {
                        AccessFlags = (int) javaMethod.AccessFlags,
                        EditorBrowsableState = nameInfo.EditorBrowsableState
                    };
                }
                else if (nameInfo.IsDeconstructor)
                {
                    method = new NetMethodDefinition("Finalize", javaMethod, declaringType, target, convertSignedBytes, "MethodBuilder.Create")
                    {
                        AccessFlags = (int) javaMethod.AccessFlags,
                        EditorBrowsableState = EditorBrowsableState.Always,
                        IsDeconstructor = true
                    };
                }
                else
                {
                    method = new NetMethodDefinition(name, javaMethod, declaringType, target, convertSignedBytes, "MethodBuilder.Create")
                    {
                        AccessFlags = (int) javaMethod.AccessFlags,
                        EditorBrowsableState = nameInfo.EditorBrowsableState
                    };
                    foreach (var typeParam in signature.TypeParameters)
                    {
                        method.GenericParameters.Add(
                            new NetGenericParameter(
                                TypeBuilder.GetMethodGenericParameterName(declaringType, typeParam.Name),
                                typeParam.Name, method));
                    }
                    var javaReturnType = signature.ReturnType;
                    NetTypeReference returnType;
                    if (!javaReturnType.TryResolve(target, this, convertSignedBytes, out returnType))
                    {
                        method = null;
                        return;
                    }
                    method.ReturnType = returnType;
                }
                method.OriginalJavaName = javaMethod.Name;

                // Find documentation
                var docClass = declaringTypeBuilder.Documentation;
                if (docClass != null)
                {
                    // Look for matches by name and parameter count first.
                    // If there is more then 1 match, we look to the parameter types.
                    var model = target.XmlModel;
                    var matches = docClass.Methods.Where(x => Matches(x, false, model)).ToList();
                    if (matches.Count == 1)
                    {
                        docMethod = matches[0];
                    }
                    else if (matches.Count > 0)
                    {
                        docMethod = matches.FirstOrDefault(x => Matches(x, true, model));
                    }
                }

                method.Attributes = declaringTypeBuilder.GetMethodAttributes(javaMethod, GetAttributes(declaringType, method, javaMethod, name, target.TypeNameMap));
                var paramIndex = 0;
                foreach (var iterator in signature.Parameters)
                {
                    var paramType = iterator;
                    if (paramType.IsJavaLangVoid())
                    {
                        paramType = new ObjectTypeReference("java/lang/Object", null);
                    }
                    NetTypeReference resolvedParamType;
                    if (!paramType.TryResolve(target, this, convertSignedBytes, out resolvedParamType))
                    {
                        method = null;
                        return; // Sometimes public methods used parameters with internal types
                    }
                    var docParam = (docMethod != null)
                                       ? docMethod.Parameters.ElementAtOrDefault(method.Parameters.Count)
                                       : null;
                    var paramName = MakeParameterNameUnique(CreateParameterName(resolvedParamType, docParam, target),
                                                            method);
                    var isParams = javaMethod.IsVarArgs && (paramIndex == signature.Parameters.Count - 1);
                    method.Parameters.Add(new NetParameterDefinition(paramName, resolvedParamType, isParams));
                    paramIndex++;
                }
                method.Description = (docMethod != null) ? docMethod.Description : null;
                declaringType.Methods.Add(method);
                if (!convertSignedBytes)
                {
                    target.MethodMap.Add(javaMethod, method);
                }
            }
            catch (ClassNotFoundException ex)
            {
                Console.WriteLine("Class {0} not found in {1}", ex.ClassName, javaMethod.Descriptor);
                method = null;
            }
        }

        /// <summary>
        /// Finalize all references now that all types have been created
        /// </summary>
        public void Complete(ClassFile declaringClass, TargetFramework target)
        {
            if (method == null)
                return;

            // Add DexImport attribute
            var ca = new NetCustomAttribute(null, javaMethod.Name, javaMethod.Descriptor);
            ca.Properties.Add(AttributeConstants.DexImportAttributeAccessFlagsName, (int)javaMethod.AccessFlags);
            if (convertSignedBytes)
            {
                ca.Properties.Add(AttributeConstants.DexImportAttributeIgnoreFromJavaName, true);
            }
            var signature = (javaMethod.Signature != null) ? javaMethod.Signature.Original : null;
            if ((signature != null) && (signature != javaMethod.Descriptor))
            {
                ca.Properties.Add(AttributeConstants.DexImportAttributeSignature, signature);                
            }
            method.CustomAttributes.Add(ca);
        }

        /// <summary>
        /// Called in the finalize phase of the type builder.
        /// </summary>
        public void Finalize(TargetFramework target, FinalizeStates state)
        {
            if (method == null)
                return;
            if (state == FinalizeStates.FixTypes)
            {
                FixReturnType(target);
            }
        }

        /// <summary>
        /// Called in the finalize phase of the type builder.
        /// </summary>
        public void FinalizeNames(TargetFramework target, MethodRenamer methodRenamer)
        {
            if (method == null)
                return;
            if (addJavaPrefix && (!method.Name.StartsWith("Java")))
            {
                methodRenamer.Rename(method, "Java" + method.Name);
            }
            if (!method.IsConstructor)
            {
                declaringTypeBuilder.ModifyMethodName(method, methodRenamer);
            }

            // Fix visiblity
            FixVisibility();
        }

        /// <summary>
        /// Update the return type (if needed)
        /// </summary>
        private void FixReturnType(TargetFramework target)
        {
            var declaringType = method.DeclaringType;
            if ((declaringType == null) || method.IsSignConverted || (!method.NeedsOverrideKeyword))
                return;

            var returnType = method.ReturnType;
            if ((returnType is NetGenericParameter) && ((NetGenericParameter)returnType).Owner == method)
                return;

            var baseMethod = method.Overrides.First(x => !x.DeclaringType.IsInterface);
            var expectedRetType = GenericParameters.Resolve(baseMethod.ReturnType, declaringType, target.TypeNameMap);
            var retType = GenericParameters.Resolve(method.ReturnType, declaringType, target.TypeNameMap);

            if (!retType.AreSame(expectedRetType))
            {
                if (baseMethod.IsAbstract)
                {
                    method.ReturnType = expectedRetType;
                }
                else
                {
                    method.RequireNewSlot();
                }
            }
        }

        /// <summary>
        /// Fix the visibility of the method
        /// </summary>
        private void FixVisibility()
        {
            // Update visibility/netslot based on base method
            var baseMethod = method.Overrides.FirstOrDefault(x => !x.DeclaringType.IsInterface);
            if ((baseMethod != null) && !method.HasSameVisibility(baseMethod))
            {
                if (baseMethod.IsAbstract)
                {
                    // Limit the visibility to that of the base method
                    method.CopyVisibilityFrom(baseMethod);
                }
                else
                {
                    // Require a new
                    method.RequireNewSlot();
                }
            }

            // Make sure signature types have a high enough visibility
            method.EnsureVisibility();

            // Limit the methods visibility
            method.LimitVisibility();            
        }

        /// <summary>
        /// Create type attributes
        /// </summary>
        private static MethodAttributes GetAttributes(NetTypeDefinition declaringType, NetMethodDefinition method, MethodDefinition javaMethod, string methodName, TypeNameMap typeNameMap)
        {
            var result = (MethodAttributes) 0;
            var isStatic = javaMethod.IsStatic;

            if (javaMethod.IsPublic) result |= MethodAttributes.Public;
            else if (javaMethod.IsProtected) result |= MethodAttributes.FamORAssem;
            else if (javaMethod.IsPrivate) result |= MethodAttributes.Private;
            else if (javaMethod.IsPackagePrivate) result |= MethodAttributes.Assembly;

            if (isStatic) result |= MethodAttributes.Static;
            if (javaMethod.IsAbstract) result |= MethodAttributes.Abstract;
            if (declaringType.IsInterface) result |= MethodAttributes.Abstract;

            if ((!javaMethod.IsFinal) && !isStatic && (methodName != ".ctor") && (!declaringType.IsStruct))
            {
                result |= MethodAttributes.Virtual;
            }
            else
            {
                result |= MethodAttributes.Final;
            }

            if (methodName == ".cctor")
            {
                result |= MethodAttributes.Static;
            }

            if (declaringType.IsSealed)
            {
                result &= ~MethodAttributes.Virtual;
            }

            return result;
        }

        /// <summary>
        /// Resolve the given generic parameter into a type reference.
        /// </summary>
        bool IBuilderGenericContext.TryResolveTypeParameter(string name, TargetFramework target,
                                                            out NetTypeReference type)
        {
            var methodTypeParam = method.GenericParameters.FirstOrDefault(x => (x.Name == name) || (x.OriginalName == name));
            if (methodTypeParam != null)
            {
                type = methodTypeParam;
                return true;
            }

            if (((IBuilderGenericContext) declaringTypeBuilder).TryResolveTypeParameter(name, target, out type))
                return true;
            // Look in parent type
            type = ResolveTypeParameterInBaseClass(javaMethod.DeclaringClass, name, target);
            return (type != null);
        }

        /// <summary>
        /// Try to resolve a type parameter in a base class.
        /// </summary>
        private NetTypeReference ResolveTypeParameterInBaseClass(ClassFile @class, string name, TargetFramework target)
        {
            ClassFile baseClass;
            while (@class.TryGetSuperClass(out baseClass))
            {
                var signature = baseClass.Signature;
                if (signature != null)
                {
                    var typeParam = signature.TypeParameters.FirstOrDefault(x => x.Name == name);
                    if (typeParam != null)
                    {
                        var index = signature.TypeParameters.IndexOf(typeParam);
                        var javaType = @class.Signature.SuperClass.Arguments.ElementAt(index).Signature;
                        return javaType.Resolve(target, this, convertSignedBytes);
                    }
                }
                @class = baseClass;
            }
            return null;
        }

        /// <summary>
        /// Full typename of the context without any generic types.
        /// </summary>
        string IBuilderGenericContext.FullTypeName
        {
            get { return ((IBuilderGenericContext)declaringTypeBuilder).FullTypeName; }
        }

        /// <summary>
        /// Create a name for a parameter of the given type.
        /// </summary>
        private string CreateParameterName(NetTypeReference parameterType, DocParameter doc, TargetFramework target)
        {
            if ((doc != null) && (!string.IsNullOrEmpty(doc.Name)))
                return doc.Name;

            target.ReportMissingParameterName(javaMethod.DeclaringClass.ClassName);

            var name = parameterType.Name;
            var elementType = parameterType.GetElementType();
            if (elementType != null)
            {
                name = elementType.Name.Replace('.', '_');
            }
            if (name == null)
            {
                name = "p";
            }
            if ((name.StartsWith("I")) && (name.Length > 1) && (elementType != null) && (elementType.IsInterface))
            {
                name = name.Substring(1);
            }
            name = name.Substring(0, 1).ToLower() + name.Substring(1);
            return name;
        }

        /// <summary>
        /// Add a postfix to the given parameter name to make it unique.
        /// </summary>
        private static string MakeParameterNameUnique(string name, NetMethodDefinition method)
        {
            var baseName = name;
            if (Keywords.IsKeyword(name))
                name = "@" + name;
            if (method.Parameters.All(x => x.Name != name))
                return name;
            var index = 1;
            while (true)
            {
                var extName = baseName + index;
                if (method.Parameters.All(x => x.Name != extName))
                    return extName;
                index++;
            }
        }

        /// <summary>
        /// Does my method match the given documentation?
        /// </summary>
        private bool Matches(DocMethod arg, bool matchParamTypes, DocModel model)
        {
            if (javaMethod.Name != arg.Name)
            {
                if (method.IsConstructor)
                {
                    if (!javaMethod.DeclaringClass.ClassName.EndsWith("/" + arg.Name))
                        return false;
                }
                else
                {
                    return false;
                }
            }
            var pCount = javaMethod.Parameters.Count;
            if (pCount != arg.Parameters.Count())
                return false;
            if (!matchParamTypes)
            {
                // Param match and name matches, fine for now
                return true;
            }

            // Match on param types
            var pIndex = 0;
            foreach (var docParam in arg.Parameters)
            {
                var p = javaMethod.Parameters[pIndex++];
                var paramType = docParam.ParameterType.Resolve(model);
                if (paramType == null)
                {
                    // Param type cannot be resolved, so no match
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Does the given method contain a parameter or return type that contains a signed byte.
        /// </summary>
        private static bool ContainsSignedByte(MethodDefinition javaMethod, out bool onlyInReturnType)
        {
            onlyInReturnType = false;
            var result = javaMethod.ReturnType.ContainsSignedByte();
            if (javaMethod.Parameters.Any(x => x.ContainsSignedByte()))
            {
                result = true;
            }
            else if (result)
            {
                onlyInReturnType = true;
                return true;
            }
            return result;
        }

        /// <summary>
        /// Should the given method be excluded from an sign converted overload?
        /// </summary>
        private static bool AvoidConvertSignedBytes(MethodDefinition javaMethod)
        {
            var className = javaMethod.DeclaringClass.ClassName;
            switch (className)
            {
                case "java/nio/ByteBuffer":
                    return (javaMethod.Name == "array");
                case "java/lang/Byte":
                case "java/lang/Short":
                case "java/lang/Integer":
                case "java/lang/Long":
                case "java/lang/Float":
                case "java/lang/Double":
                case "java/lang/Number":
                    return true;
            }
            if (javaMethod.DeclaringClass.IsInterface)
                return true;
            return false;
        }
    }
}
