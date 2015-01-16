using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Dot42.ImportJarLib.Model
{
    partial class NetMethodDefinition 
    {
        /// <summary>
        /// Collection with all overrides of a method.
        /// </summary>
        private sealed class OverrideCollection : IEnumerable<NetMethodDefinition>, IFlushable
        {
            private readonly NetMethodDefinition owner;
            private HashSet<NetMethodDefinition> overrides;

            /// <summary>
            /// Default ctor
            /// </summary>
            public OverrideCollection(NetMethodDefinition owner)
            {
                this.owner = owner;
            }

            /// <summary>
            /// Make sure the override collection is re-built.
            /// </summary>
            void IFlushable.Flush()
            {
                overrides = null;
            }

            /// <summary>
            /// Build the list of overrides
            /// </summary>
            private void Build()
            {
                // Collect overrides
                var newOverrides = new HashSet<NetMethodDefinition>();

                if (!owner.IsConstructor && !owner.IsStatic && (owner.DeclaringType != null))
                {
                    var visitedTypes = new HashSet<NetTypeDefinition>();

                    // Collect overrides
                    AddOverrides(owner.DeclaringType, newOverrides, visitedTypes);

                    // Use java overrides
                    if ((owner.javaMethod != null) && (!owner.DeclaringType.IsStruct))
                    {
                        foreach (var x in owner.javaMethod.Overrides())
                        {
                            NetTypeDefinition @class;
                            if (owner.target.TypeNameMap.TryGetByJavaClassName(x.DeclaringClass.ClassName, out @class) && (!@class.IsInterface))
                            {
                                visitedTypes.Add(@class);
                                //@class.Methods.First().
                            }
                        }
                    }

                    // Add flush actions
                    foreach (var type in visitedTypes)
                    {
                        type.AddFlushAction(this);
                    }
                }

                // Save
                overrides = newOverrides;
            }

            /// <summary>
            /// Add all methods from type that owner overrides to the given list.
            /// </summary>
            private void AddOverrides(NetTypeDefinition type, HashSet<NetMethodDefinition> target, HashSet<NetTypeDefinition> visitedTypes)
            {
                // Add the method(s) that my owner overrides
                if (owner.DeclaringType != type)
                {
                    visitedTypes.Add(type);
                    foreach (var m in type.Methods.Where(IsOverride))
                    {
                        target.Add(m);
                        foreach (var o in m.Overrides)
                        {
                            target.Add(o);
                        }
                    }
                }

                // Add interface method(s) that my owner overrides
                foreach (var intf in type.Interfaces.Select(x => x.GetElementType()))
                {
                    foreach (var m in intf.Methods.Where(IsOverride))
                    {
                        target.Add(m);
                        foreach (var o in m.Overrides)
                        {
                            target.Add(o);
                        }
                    }

                    // Recurse
                    AddOverrides(intf, target, visitedTypes);
                }

                // Recurse into base type
                if (type.BaseType != null)
                {
                    AddOverrides(type.BaseType.GetElementType(), target, visitedTypes);
                }
            }

#if OLD
            /// <summary>
            /// Is the given method an override?
            /// </summary>
            private static bool IsOverride(MethodDefinition javaMethod, MethodAttributes attributes, TypeNameMap typeNameMap, out bool isNewSlot)
            {
                isNewSlot = false;
                if (javaMethod.IsOverride())
                    return true;
                if (!typeNameMap.HasImportMappings)
                    return false;
                var superClass = javaMethod.DeclaringClass.SuperClass;
                if (superClass == null)
                    return false;

                // If there are bridge methods calling this method we assume to override in the bridge is an override.
                var bridge = javaMethod.DeclaringClass.Methods.FirstOrDefault(x => IsBridgeFor(x, javaMethod));
                if (bridge != null)
                {
                    bool tmp;
                    if (IsOverride(bridge, attributes, typeNameMap, out tmp))
                        return true;
                }

                // Check typemap
                var superClassName = superClass.ClassName;
                string netName;
                string baseDescriptor;
                string baseSignature;
                MethodAttributes methodAttributes;
                if (!typeNameMap.IsImportedVirtualMethod(superClassName, javaMethod.Name, javaMethod.Descriptor, out methodAttributes, out netName, out baseDescriptor, out baseSignature))
                    return false;
                if ((methodAttributes & MethodAttributes.MemberAccessMask) !=
                    (attributes & MethodAttributes.MemberAccessMask))
                {
                    isNewSlot = true;
                }
                return true;
            }

            /// <summary>
            /// Is the given method a bridge method that calls the given target method?
            /// </summary>
            private static bool IsBridgeFor(MethodDefinition method, MethodDefinition target)
            {
                if ((!method.IsBridge) || (method == target))
                    return false;
                if ((method.Name != target.Name) || (method.Parameters.Count != target.Parameters.Count))
                    return false;
                if (method.Body == null)
                    return false;
                return method.Body.Instructions.Select(x => x.Operand).OfType<ConstantPoolMethodRef>().Any(x => (x.Descriptor == target.Descriptor) && (x.Name == target.Name));
            }
#endif

            /// <summary>
            /// Does my owner override the given method?
            /// </summary>
            private bool IsOverride(NetMethodDefinition other)
            {
                var me = owner;
                if (me.InterfaceMethod == other)
                    return true;
                if (me.Name != other.Name)
                    return false;
                if (me.IsSignConverted != other.IsSignConverted)
                    return false;
                var paramCount = me.Parameters.Count;
                if (paramCount != other.Parameters.Count)
                    return false;
                if (paramCount > 0)
                {
                    var typeNameMap = owner.target.TypeNameMap;
                    var context = owner.DeclaringType;
                    for (var i = 0; i < paramCount; i++)
                    {
                        var myParamType = ImportJarLib.GenericParameters.Resolve(me.Parameters[i].ParameterType, context, typeNameMap);
                        var otherParamType = ImportJarLib.GenericParameters.Resolve(other.Parameters[i].ParameterType, context, typeNameMap);
                        if (!myParamType.AreSame(otherParamType))
                            return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// Gets all overrides.
            /// </summary>
            public IEnumerator<NetMethodDefinition> GetEnumerator()
            {
                if (overrides == null)
                    Build();
                return overrides.GetEnumerator();
            }

            /// <summary>
            /// Gets all overrides.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
