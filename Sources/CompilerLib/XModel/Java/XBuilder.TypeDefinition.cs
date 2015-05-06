using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dot42.CompilerLib.XModel.Synthetic;
using Dot42.JvmClassLib;

namespace Dot42.CompilerLib.XModel.Java
{
    partial class XBuilder
    {
        /// <summary>
        /// Java specific type definition.
        /// </summary>
        internal sealed class JavaTypeDefinition : XTypeDefinition
        {
            private readonly ClassFile type;
            private readonly ReadOnlyCollection<XFieldDefinition> fields;
            private readonly ReadOnlyCollection<XMethodDefinition> methods;
            private readonly ReadOnlyCollection<XTypeDefinition> nested;
            private readonly List<XTypeReference> interfaces;
            private XTypeReference baseType;

            /// <summary>
            /// Default ctor
            /// </summary>
            public JavaTypeDefinition(XModule module, XTypeDefinition declaringType, ClassFile type)
                : base(module, declaringType, false, null)
            {
                this.type = type;
                fields = type.Fields.Select(x => new JavaFieldDefinition(this, x)).Cast<XFieldDefinition>().ToList().AsReadOnly();
                methods = type.Methods.Select(x => new JavaMethodDefinition(this, x)).Cast<XMethodDefinition>().ToList().AsReadOnly();
                interfaces = new List<XTypeReference>();
                nested = type.InnerClasses.Where(x => x.InnerClassFile.DeclaringClass == type).Select(x => new JavaTypeDefinition(module, this, x.InnerClassFile)).Cast<XTypeDefinition>().ToList().AsReadOnly();

                module.Register(this, FullName);
                module.Register(this, type.ClassName);

                foreach(var n in nested)
                    module.Register(n);
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
                get { return 0; }
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
                get { return ClassName.JavaClassNameToClrTypeName(type.Package); }
            }

            /// <summary>
            /// Gets the type this type extends (null if System.Object)
            /// </summary>
            public override XTypeReference BaseType
            {
                get { return baseType ?? (baseType = AsTypeReference(Module, type.SuperClass, XTypeUsageFlags.BaseType)); }
            }

            /// <summary>
            /// Is this type a type that needs it's generic types implemented at runtime?
            /// </summary>
            public override bool IsGenericClass
            {
                get { return false; }
            }

            /// <summary>
            /// Gets all fields defined in this type.
            /// </summary>
            public override ReadOnlyCollection<XFieldDefinition> Fields
            {
                get { return fields; }
            }

            /// <summary>
            /// Gets a field by it's underlying field object.
            /// </summary>
            public override XFieldDefinition GetByOriginalField(object originalField)
            {
                var fieldDef = Fields.Cast<JavaFieldDefinition>().FirstOrDefault(x => x.OriginalField == originalField);
                if (fieldDef == null)
                    throw new ArgumentException(string.Format("Java field {0} not found in {1}", originalField, FullName));
                return fieldDef;
            }

            /// <summary>
            /// Gets all methods defined in this type.
            /// </summary>
            public override ReadOnlyCollection<XMethodDefinition> Methods
            {
                get { return methods; }
            }

            /// <summary>
            /// Gets a field by it's underlying method object.
            /// </summary>
            public override XMethodDefinition GetByOriginalMethod(object originalMethod)
            {
                return Methods.OfType<JavaMethodDefinition>().First(x => x.OriginalMethod == originalMethod);
            }

            /// <summary>
            /// Add the given generated method to this type.
            /// </summary>
            internal override void Add(XSyntheticMethodDefinition method)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Add the given generated field to this type.
            /// </summary>
            internal override void Add(XSyntheticFieldDefinition field)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Add the given generated nested type to this type.
            /// </summary>
            internal override void Add(XSyntheticTypeDefinition nestedType)
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Gets all types defined in this type.
            /// </summary>
            public override ReadOnlyCollection<XTypeDefinition> NestedTypes
            {
                get { return nested; }
            }

            /// <summary>
            /// Gets all interfaces this type implements.
            /// </summary>
            public override ReadOnlyCollection<XTypeReference> Interfaces
            {
                get
                {
                    var typeInterfacesCount = (type.Interfaces != null) ? type.Interfaces.Length : 0;
                    if (interfaces.Count != typeInterfacesCount)
                    {
                        interfaces.Clear();
                        interfaces.AddRange(type.Interfaces.Select(x => AsTypeReference(Module, x, XTypeUsageFlags.Interface)));
                        Reset();
                    }
                    return interfaces.AsReadOnly();
                }
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
                get { return false; }
            }

            /// <summary>
            /// Is this type a struct (non-primitive, non-enum, non-nullableT value type)?
            /// </summary>
            public override bool IsStruct
            {
                get { return false; }
            }

            public override bool IsImmutableStruct { get { return false; } }

            /// <summary>
            /// Gets the type of the enum value field.
            /// </summary>
            public override XTypeReference GetEnumUnderlyingType()
            {
                return null;
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
                get { return type.IsFinal; }
            }

            /// <summary>
            /// our unique id, constant accross builds.
            /// </summary>
            public override string ScopeId { get { return FullName; } }

            /// <summary>
            /// Is this type reachable?
            /// </summary>
            public override bool IsReachable
            {
                get { return false; }
            }

            /// <summary>
            /// Is there a DexImport attribute on this type?
            /// </summary>
            public override bool HasDexImportAttribute()
            {
                return false;
            }

            /// <summary>
            /// Is there a CustomView attribute on this type?
            /// </summary>
            public override bool HasCustomViewAttribute()
            {
                return false;
            }

            /// <summary>
            /// Try to get the classname from the DexImport attribute attached to this method.
            /// </summary>
            public override bool TryGetDexImportNames(out string className)
            {
                className = null;
                return false;
            }

            /// <summary>
            /// Try to get the classname from the JavaImport attribute attached to this method.
            /// </summary>
            public override bool TryGetJavaImportNames(out string className)
            {
                className = null;
                return false;
            }

            /// <summary>
            /// Try to get the enum field that defines the given constant.
            /// </summary>
            public override bool TryGetEnumConstField(object value, out XFieldDefinition field)
            {
                field = null;
                return false;
            }
        }
    }
}
