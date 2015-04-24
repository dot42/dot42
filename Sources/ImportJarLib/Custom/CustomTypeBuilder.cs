using System.Reflection;
using System.Xml.Linq;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;

namespace Dot42.ImportJarLib.Custom
{
    /// <summary>
    /// Base class for newly created basic types.
    /// </summary>
    public abstract class CustomTypeBuilder : TypeBuilder, ICustomTypeBuilder
    {
        private const string Scope = "__custom__";
        private readonly string @namespace;
        private readonly string name;
        private readonly bool isValueType;
        private NetTypeDefinition typeDef;
        
        /// <summary>
        /// Default ctor
        /// </summary>
        protected CustomTypeBuilder(string ns, string name, bool isValueType = false)
        {
            @namespace = ns;
            this.name = name;
            this.isValueType = isValueType;
        }

        /// <summary>
        /// Get this object as type builder.
        /// </summary>
        TypeBuilder ICustomTypeBuilder.AsTypeBuilder()
        {
            return this;
        }

        /// <summary>
        /// Gets the name of the generated type. 
        /// Used for sorting.
        /// </summary>
        string ICustomTypeBuilder.CustomTypeName
        {
            get { return @namespace + "." + name; }
        }

        /// <summary>
        /// Helps in sorting type builders
        /// </summary>
        public override int Priority { get { return 1; } }

        /// <summary>
        /// Create a type defrinition for the given class file and all inner classes.
        /// </summary>
        public override void CreateType(NetTypeDefinition declaringType, NetModule module, TargetFramework target)
        {
            // Create basic type
            typeDef = new NetTypeDefinition(ClassFile.Empty, target, Scope) { Name = name, Namespace = @namespace, Attributes = Attributes };
            module.Types.Add(typeDef);
            target.TypeNameMap.Add("custom/" + name, typeDef);
        }

        /// <summary>
        /// Adds the given nested type to my type declaration.
        /// </summary>
        protected internal override void AddNestedType(NetTypeDefinition nestedType, string namePrefix, NetModule module, ref string fullNestedTypeName)
        {
            TypeDefinition.NestedTypes.Add(nestedType);
        }

        /// <summary>
        /// Gets the attributes needed to create the type.
        /// </summary>
        protected abstract TypeAttributes Attributes { get; }

        /// <summary>
        /// Gets the generated type
        /// </summary>
        protected NetTypeDefinition TypeDefinition { get { return typeDef; } }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public override void Implement(TargetFramework target)
        {
            if (isValueType)
            {
                //typeDef.BaseType = typeNameMap.GetByType(typeof(ValueType));
                typeDef.IsStruct = true;
            }
            base.Implement(target);
        }

        /// <summary>
        /// Update names where needed
        /// </summary>
        public override void FinalizeNames(TargetFramework target, MethodRenamer methodRenamer)
        {
            FinalizeNames(typeDef, target, methodRenamer);
        }

        /// <summary>
        /// Make sure that base types are visible.
        /// </summary>
        public override void FinalizeVisibility(TargetFramework target)
        {
        }

        /// <summary>
        /// Update the attributes of the given method
        /// </summary>
        public override MethodAttributes GetMethodAttributes(MethodDefinition method, MethodAttributes methodAttributes)
        {
            return GetMethodAttributes(typeDef, method, methodAttributes);
        }

        /// <summary>
        /// Full typename of the context without any generic types.
        /// </summary>
        protected override string FullTypeName { get { return @namespace + "." + typeDef.Name; } }

        /// <summary>
        /// Gets the documentation of this type.
        /// Can be null.
        /// </summary>
        public override DocClass Documentation { get { return null; } }

        /// <summary>
        /// Add this type to the layout.xml file if needed.
        /// </summary>
        public override void FillLayoutXml(JarFile jf, XElement parent)
        {
            // Do nothing
        }

        public override void FillTypemapXml(JarFile jf, XElement parent)
        {
        }
    }
}
