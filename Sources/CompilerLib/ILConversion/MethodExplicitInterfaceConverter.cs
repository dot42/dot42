using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Naming;
using Dot42.CompilerLib.Reachable;
using Dot42.CompilerLib.XModel.DotNet;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.ILConversion
{
    [Export(typeof (ILConverterFactory))]
    internal class MethodExplicitInterfaceConverter : ILConverterFactory
    {
        /// <summary>
        /// Low values come first
        /// </summary>
        public int Priority
        {
            get { return 30; }
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
            private ReachableContext reachableContext;
            private NameSet methodNames;
            private List<MethodDefinition> reachableMethods;
            private List<TypeDefinition> interfaces;
            private readonly List<Tuple<MethodDefinition, string>> addedStubs = new List<Tuple<MethodDefinition, string>>();
            private HashSet<MethodReference> methodReferences;

            /// <summary>
            /// Convert interface methods that have an explicit implementation.
            /// </summary>
            public void Convert(ReachableContext reachableContext)
            {
                this.reachableContext = reachableContext;

                // Do we need to convert anything?
                if (!reachableContext.ReachableTypes.SelectMany(x => x.Methods).Any(NeedsConversion))
                    return;

                // Initialize some sets
                reachableMethods = reachableContext.ReachableTypes.SelectMany(x => x.Methods).Where(m => m.IsReachable).OrderBy(x => x.FullName).ToList();
                methodNames = new NameSet(reachableMethods.Select(m => m.Name));
                interfaces = reachableContext.ReachableTypes.Where(x => x.IsInterface).ToList();

                // Go over all interfaces
                foreach (var iType in interfaces)
                {
                    foreach (var iMethod in iType.Methods)
                    {
                        ConvertInterfaceMethod(iType, iMethod);
                    }
                }

                // Remove added stubs that are an override of another added stub.
                foreach (var stubPair in addedStubs)
                {
                    var stub = stubPair.Item1;
                    var oldName = stubPair.Item2;
                    if (stub.GetBaseMethod() != null)
                    {
                        stub.DeclaringType.Methods.Remove(stub);
                    }
                    else
                    {
                        // Check for duplicate methods
                        var resolver = new GenericsResolver(stub.DeclaringType);
                        //var methodsWithSameName = stub.DeclaringType.Methods.Where(x => (x != stub) && (x.Name == stub.Name)).ToList();
                        //var duplicate = methodsWithSameName.FirstOrDefault(x => (x != stub) && x.AreSame(stub, resolver.Resolve));
                        var duplicate = stub.DeclaringType.Methods.FirstOrDefault(x => (x != stub) && x.AreSame(stub, resolver.Resolve));
                        if (duplicate != null)
                        {
                            stub.DeclaringType.Methods.Remove(stub);
                            continue;
                        }

                        if (oldName != stub.Name)
                        {
                            var newName = stub.Name;
                            stub.Name = oldName;
                            duplicate = stub.DeclaringType.Methods.FirstOrDefault(x => (x != stub) && x.AreSame(stub, resolver.Resolve));
                            if (duplicate != null)
                            {
                                stub.DeclaringType.Methods.Remove(stub);
                                continue;
                            }
                            stub.Name = newName;
                        }
                    }
                }
            }

            /// <summary>
            /// Does the given method require some kind of interface conversion
            /// </summary>
            private static bool NeedsConversion(MethodDefinition method)
            {
                if (method.IsExplicitImplementation())
                    return true;
                if (method.DeclaringType.IsInterface)
                {
                    if (method.ContainsGenericParameter)
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Convert the given interface method if it has explicit implementations.
            /// </summary>
            private void ConvertInterfaceMethod(TypeDefinition iType, MethodDefinition iMethod)
            {
                var implementations = GetImplementations(iMethod);
                var iMethodIsJavaWithGenericParams = iMethod.IsJavaMethodWithGenericParams();
                var iMethodContainsGenericParams = iMethod.ContainsGenericParameter;
                if (!iMethodIsJavaWithGenericParams && !iMethodContainsGenericParams && (!implementations.Any(x => x.Item2.IsExplicitImplementation())))
                {
                    // There are no explicit implementation.
                    // No need to convert
                    return;
                }

                // Rename method
                string newName;
                bool createExplicitStubs = true;
                var oldName = iMethod.Name;
                var attr = iMethod.GetDexOrJavaImportAttribute();
                if (attr != null)
                {
                    string className;
                    string memberName;
                    string descriptor;
                    attr.GetDexOrJavaImportNames(iMethod, out memberName, out descriptor, out className);
                    newName = memberName;
                }
                else if ((attr = iMethod.GetDexNameAttribute()) != null)
                {
                    newName = (string) (attr.ConstructorArguments[0].Value);
                    createExplicitStubs = false;
                }
                else
                {
                    var module = reachableContext.Compiler.Module;
                    var xiType = XBuilder.AsTypeReference(module, iType);
                    newName = methodNames.GetUniqueName(NameConverter.GetConvertedName(xiType) + "_" + iMethod.Name);
                    oldName = newName;
                }
                Rename(iMethod, newName);

              
                // Update implementations
                foreach (var typeAndImpl in implementations)
                {
                    var type = typeAndImpl.Item1;
                    var impl = typeAndImpl.Item2;

                    if (impl.IsExplicitImplementation())
                    {
                        // Convert to implicit
                        impl.IsPublic = true;

                        // Rename
                        Rename(impl, newName);
                        // Update names of overrides
                        foreach (var @override in impl.Overrides)
                        {
                            @override.Name = newName;
                        }
                    }
                    else if (!(impl.HasDexImportAttribute() || impl.HasJavaImportAttribute()))
                    {
                        // Add stub redirecting explicit implementation to implicit implementation
                        if (createExplicitStubs)
                        {
                            CreateExplicitStub(type, impl, newName, oldName, iMethod, iMethodIsJavaWithGenericParams /*|| iMethodContainsGenericParams*/);
                        }
                    }
                }
            }

            /// <summary>
            /// Create a new method in the targetType which implements or inherits the implicit implementation 
            /// with the given new name.
            /// This method will call the implicit implementation.
            /// </summary>
            private void CreateExplicitStub(TypeDefinition targetType, MethodDefinition implicitImpl, string newName, string oldName, MethodDefinition iMethod, bool avoidGenericParam)
            {
                // Create method
                var newMethod = InterfaceHelper.CreateExplicitStub(targetType, implicitImpl, newName, iMethod, avoidGenericParam);

                // Record 
                addedStubs.Add(Tuple.Create(newMethod, oldName));
            }

            /// <summary>
            /// Rename the given method and all references to it from code.
            /// </summary>
            private void Rename(MethodDefinition method, string newName)
            {
                methodReferences = methodReferences ?? CollectMethodReferences();
                var resolver = new GenericsResolver(method.DeclaringType);
                foreach (var methodRef in methodReferences)
                {
                    if (ReferenceEquals(method, methodRef))
                        continue;
                    if (methodRef.AreSameIncludingDeclaringType(method, resolver.Resolve))
                    {
                        methodRef.Name = newName;
                    }
                }
                method.Name = newName;
            }

            /// <summary>
            /// Create a set containing all method references found in the reachable methods.
            /// </summary>
            private HashSet<MethodReference> CollectMethodReferences()
            {
                var result = new HashSet<MethodReference>();
                foreach (var body in reachableMethods.Select(x => x.Body).Where(x => x != null))
                {
                    foreach (var ins in body.Instructions)
                    {
                        var methodRef = ins.Operand as MethodReference;
                        if (methodRef == null)
                            continue;
                        methodRef = methodRef.GetElementMethod();
                        result.Add(methodRef);
                    }
                }
                return result;
            }

            /// <summary>
            /// Gets all implementations of the given interface method.
            /// </summary>
            private List<Tuple<TypeDefinition,MethodDefinition>> GetImplementations(MethodDefinition iMethod)
            {
                var iType = iMethod.DeclaringType;
                var typesThatImplement = reachableContext.ReachableTypes.Where(x => x.Implements(iType)).ToList();

                return typesThatImplement.Select(x => Tuple.Create(x, iMethod.GetImplementation(x)))
                                         .Where(x => x.Item2 != null)
                                         .ToList();
            }
        }
    }
}