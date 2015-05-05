using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.ILConversion;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.LoaderLib.Extensions;
using Mono.Cecil;

namespace Dot42.CompilerLib.XModel.DotNet
{
    partial class XBuilder
    {
        /// <summary>
        /// IL specific type definition.
        /// </summary>
        public sealed class ILTypeDefinition : XTypeDefinition
        {
            private readonly TypeDefinition type;
            private readonly List<XFieldDefinition> fields;
            private readonly List<XMethodDefinition> methods;
            private int addedMethodCount;
            private int addedFieldCount;
            private readonly List<XTypeDefinition> nested;
            private readonly List<XTypeReference> interfaces;
            private XTypeReference baseType;
            private XTypeReference dexImportType;
            private XTypeReference javaImportType;
            private int? priority;
            private bool? isStruct;
            private bool? isImmutableStruct;
            private bool? isGenericClass;
            private string @namespace;

            private Dictionary<MethodDefinition, int> methodToIdx;

            /// <summary>
            /// Default ctor
            /// </summary>
            public ILTypeDefinition(XModule module, XTypeDefinition declaringType, TypeDefinition type)
                : base(module, declaringType, type.IsValueType, type.GenericParameters.Select(x => x.Name))
            {
                this.type = type;
                fields = new List<XFieldDefinition>(type.Fields.Count);
                methods = new List<XMethodDefinition>(type.Methods.Count);
                nested = new List<XTypeDefinition>();
                interfaces = new List<XTypeReference>();

                // Add nested types
                foreach (var source in type.NestedTypes/*.Where(t=>t.IsReachable) should we only consider reachables?*/)
                {
                    var nestedType = new ILTypeDefinition(Module, this, source);
                    nested.Add(nestedType);
                    module.Register(nestedType);
                }

                 methodToIdx = type.Methods.Select((m, idx) => new { m, idx }).ToDictionary(k => k.m, k => k.idx);
            }

            /// <summary>
            /// Gets the underlying type object.
            /// </summary>
            public override object OriginalTypeDefinition { get { return type; } }

            /// <summary>
            /// Sort order priority.
            /// Low values come first
            /// </summary>
            public override int Priority
            {
                get
                {
                    if (!priority.HasValue)
                    {
                        priority = 0;
                        var attr = type.GetDexImportAttribute();
                        if (attr != null)
                        {
                            var value = attr.Properties.Where(x => x.Name == "Priority").Select(x => x.Argument.Value).FirstOrDefault();
                            if (value is int)
                            {
                                priority = (int) value;
                            }
                        }                        
                    }
                    return priority.Value;
                }
            }

            /// <summary>
            /// Name of the member
            /// </summary>
            public override string Name
            {
                get { return type.Name; }
            }

            /// <summary>
            /// Gets the namespace of this type.
            /// Returns the namespace of the declaring type for nested types.
            /// </summary>
            public override string Namespace
            {
                get
                {
                    if (@namespace == null)
                        @namespace = GetScopePrefix(type) + type.Namespace;
                    return @namespace;
                }
            }
            
            /// <summary>
            /// Gets the type this type extends (null if System.Object)
            /// </summary>
            public override XTypeReference BaseType
            {
                get { return baseType ?? (baseType = AsTypeReference(Module, type.BaseType)); }
            }

            /// <summary>
            /// Is this type a type that needs it's generic types implemented at runtime?
            /// </summary>
            public override bool IsGenericClass
            {
                get
                {
                    if (!isGenericClass.HasValue)
                    {
                        isGenericClass = type.HasGenericParameters && !type.HasDexImportAttribute();
                    }
                    return isGenericClass.Value;
                }
            }

            /// <summary>
            /// Gets all fields defined in this type.
            /// </summary>
            public override ReadOnlyCollection<XFieldDefinition> Fields
            {
                get
                {
                    if ((fields.Count - addedFieldCount) != type.Fields.Count)
                    {
                        // Add missing fields
                        foreach (var source in type.Fields)
                        {
                            if (fields.OfType<ILFieldDefinition>().All(x => x.OriginalField != source))
                            {
                                fields.Add(new ILFieldDefinition(this, source));
                            }
                        }    
                        Reset();
                    }
                    return fields.AsReadOnly();
                }
            }

            /// <summary>
            /// Gets a field by it's underlying field object.
            /// </summary>
            public override XFieldDefinition GetByOriginalField(object originalField)
            {
                return Fields.OfType<ILFieldDefinition>().First(x => x.OriginalField == originalField);
            }

            /// <summary>
            /// Gets all methods defined in this type.
            /// </summary>
            public override ReadOnlyCollection<XMethodDefinition> Methods
            {
                get
                {
                    if ((methods.Count - addedMethodCount) != type.Methods.Count)
                    {
                        // Add missing methods
                        foreach (var source in type.Methods)
                        {
                            if (methods.OfType<ILMethodDefinition>().All(x => x.OriginalMethod != source))
                            {
                                methods.Add(new ILMethodDefinition(this, source));
                            }
                        }
                        Reset();
                    }
                    return methods.AsReadOnly();
                }
            }

            /// <summary>
            /// Gets a field by it's underlying method object.
            /// </summary>
            public override XMethodDefinition GetByOriginalMethod(object originalMethod)
            {
                return Methods.OfType<ILMethodDefinition>().First(x => x.OriginalMethod == originalMethod);
            }

            /// <summary>
            /// Gets all types defined in this type.
            /// </summary>
            public override ReadOnlyCollection<XTypeDefinition> NestedTypes
            {
                get { return nested.AsReadOnly(); }
            }

            /// <summary>
            /// Add the given generated method to this type.
            /// </summary>
            internal override void Add(XSyntheticMethodDefinition method)
            {
                methods.Add(method);
                addedMethodCount++;
            }

            /// <summary>
            /// Add the given generated field to this type.
            /// </summary>
            internal override void Add(XSyntheticFieldDefinition field)
            {
                fields.Add(field);
                addedFieldCount++;
            }

            /// <summary>
            /// Add the given generated nested type to this type.
            /// </summary>
            internal override void Add(XSyntheticTypeDefinition nestedType)
            {
                nested.Add(nestedType);
            }

            /// <summary>
            /// Gets all interfaces this type implements.
            /// </summary>
            public override ReadOnlyCollection<XTypeReference> Interfaces
            {
                get
                {
                    if (interfaces.Count != type.Interfaces.Count)
                    {
                        interfaces.Clear();
                        interfaces.AddRange(type.Interfaces.Select(x => AsTypeReference(Module, x.Interface)));
                        Reset();
                    }
                    return interfaces.AsReadOnly();
                }
            }

            /// <summary>
            /// Is this a primitive type?
            /// </summary>
            public override bool IsPrimitive
            {
                get { return type.IsPrimitive; }
            }

            /// <summary>
            /// Is this an interface
            /// </summary>
            public override bool IsInterface
            {
                get { return type.IsInterface; }
            }

            /// <summary>
            /// Is this an enum type?
            /// </summary>
            public override bool IsEnum
            {
                get { return type.IsEnum; }
            }

            /// <summary>
            /// Is this type a struct (non-primitive, non-enum, non-nullableT value type)?
            /// </summary>
            public override bool IsStruct
            {
                get
                {
                    if (!isStruct.HasValue)
                    {
                        isStruct = type.IsValueType && !type.IsPrimitive && !type.IsEnum && !type.IsNullableT() && !type.IsVoid();
                    }
                    return isStruct.Value;
                }
            }

            public override bool IsImmutableStruct
            {
                get
                {
                    if (!isImmutableStruct.HasValue)
                    {
                        isImmutableStruct = IsStruct && StructFields.IsImmutableStruct(type);
                    }
                    return isImmutableStruct.Value;
                }
            }

            /// <summary>
            /// Gets the type of the enum value field.
            /// </summary>
            public override XTypeReference GetEnumUnderlyingType()
            {
                return (!type.IsEnum) ? null : AsTypeReference(Module, type.GetEnumUnderlyingType());
            }

            /// <summary>
            /// Is this class abstract?
            /// </summary>
            public override bool IsAbstract
            {
                get { return type.IsAbstract; }
            }

            /// <summary>
            /// Is this class sealed/final (cannot be extended)?
            /// </summary>
            public override bool IsSealed
            {
                get { return type.IsSealed; }
            }

            /// <summary>
            /// our unique id, constant across builds if the assembly has not changed.
            /// </summary>
            public override string ScopeId { get { return type.Scope.Name + ":" + type.MetadataToken.ToScopeId(); } }

            /// <summary>
            /// Does this type reference point to the same type as the given other reference?
            /// </summary>
            public override bool IsSame(XTypeReference other, bool ignoreSign)
            {
                if (base.IsSame(other, ignoreSign))
                    return true;
                EnsureDexImportType();
                if (dexImportType != Module.TypeSystem.NoType)
                {
                    if (dexImportType.IsSame(other, ignoreSign))
                        return true;
                }
                EnsureJavaImportType();
                if (javaImportType != Module.TypeSystem.NoType)
                {
                    if (javaImportType.IsSame(other, ignoreSign))
                        return true;
                }
                return false;
            }

            /// <summary>
            /// Ensure that dexImportType is loaded.
            /// </summary>
            private void EnsureDexImportType()
            {
                if (dexImportType == null)
                {
                    // Load dex import type
                    string className;
                    dexImportType = TryGetDexImportNames(out className)
                                        ? Java.XBuilder.AsTypeReference(Module, className, XTypeUsageFlags.DeclaringType)
                                        : Module.TypeSystem.NoType;
                }
            }

            /// <summary>
            /// Ensure that javaImportType is loaded.
            /// </summary>
            private void EnsureJavaImportType()
            {
                if (javaImportType == null)
                {
                    // Load java import type
                    string className;
                    javaImportType = TryGetJavaImportNames(out className)
                                         ? Java.XBuilder.AsTypeReference(Module, className, XTypeUsageFlags.DeclaringType)
                                         : Module.TypeSystem.NoType;
                }
            }

            /// <summary>
            /// Is this type reachable?
            /// </summary>
            public override bool IsReachable
            {
                get { return type.IsReachable; }
            }

            /// <summary>
            /// Is there a DexImport attribute on this type?
            /// </summary>
            public override bool HasDexImportAttribute()
            {
                return type.HasDexImportAttribute();
            }

            /// <summary>
            /// Is there a CustomView attribute on this type?
            /// </summary>
            public override bool HasCustomViewAttribute()
            {
                return type.HasCustomViewAttribute();
            }

            /// <summary>
            /// Try to get the classname from the DexImport attribute attached to this method.
            /// </summary>
            public override bool TryGetDexImportNames(out string className)
            {
                var attr = type.GetDexImportAttribute();
                if (attr == null)
                {
                    className = null;
                    return false;
                }
                className = (string) attr.ConstructorArguments[0].Value;
                return true;
            }

            /// <summary>
            /// Try to get the classname from the JavaImport attribute attached to this method.
            /// </summary>
            public override bool TryGetJavaImportNames(out string className)
            {
                var attr = type.GetJavaImportAttribute();
                if (attr == null)
                {
                    className = null;
                    return false;
                }
                className = (string)attr.ConstructorArguments[0].Value;
                return true;                
            }

            /// <summary>
            /// Try to get the enum field that defines the given constant.
            /// </summary>
            public override bool TryGetEnumConstField(object value, out XFieldDefinition field)
            {
                field = null;
                if (!type.IsEnum || (value == null))
                    return false;
                var valueType = value.GetType();
                var ilField = type.Fields.FirstOrDefault(x => x.IsStatic && Equals(XConvert.ChangeType(x.Constant, valueType), value));
                if (ilField == null)
                    return false;
                field = Fields.OfType<ILFieldDefinition>().FirstOrDefault(x => x.OriginalField == ilField);
                return (field != null);
            }

            internal string GetMethodScopeId(MethodDefinition methodDefinition)
            {
                int idx;
                if(methodToIdx.TryGetValue(methodDefinition, out idx))
                    return idx.ToString(CultureInfo.InvariantCulture);
                return null;
            }
        }
    }
}
