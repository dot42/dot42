using System.Collections.Generic;
using System.Linq;
using Dot42.CecilExtensions;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Target.Dex;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.LoaderLib.Extensions;
using Dot42.Mapping;
using Dot42.Utility;
using Mono.Cecil;
using MethodDefinition = Mono.Cecil.MethodDefinition;

namespace Dot42.CompilerLib.Structure.DotNet
{
    /// <summary>
    /// Build ClassDefinition structures for existing .NET types that have a DexImport attribute.
    /// </summary>
    internal sealed class DexImportClassBuilder : StandardClassBuilder
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        public DexImportClassBuilder(Reachable.ReachableContext context, AssemblyCompiler compiler, TypeDefinition typeDef)
            : base(context, compiler, typeDef)
        {
        }

        /// <summary>
        /// Create the current type as class definition.
        /// </summary>
        protected override void CreateClassDefinition(DexTargetPackage targetPackage, ClassDefinition parent, TypeDefinition parentType, XTypeDefinition parentXType)
        {
            // Do not create a class.
            // It already exists in the framework.
        }

        /// <summary>
        /// Set the super class of the class definition.
        /// </summary>
        protected override void ImplementSuperClass(DexTargetPackage targetPackage)
        {
            // No need to do anything here.
        }

        /// <summary>
        /// Add references to all implemented interfaces.
        /// </summary>
        protected override void ImplementInterfaces(DexTargetPackage targetPackage)
        {
            // No need to do anything here.
        }

        /// <summary>
        /// Create and add GenericInstance field.
        /// </summary>
        protected override void CreateGenericInstanceField(DexTargetPackage targetPackage)
        {
            // No need to do anything here.
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected override bool ShouldImplementField(Mono.Cecil.FieldDefinition field)
        {
            if (!base.ShouldImplementField(field))
                return false;
            if (field.IsStatic)
                return true;
            throw new FrameworkException(
                string.Format("Type {0} should have no non-framework fields ({1})", Type.FullName, field.Name));
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected override bool ShouldImplementMethod(MethodDefinition method)
        {
            if (!base.ShouldImplementMethod(method))
                return false;
            if ((method.Name == ".cctor") || (method.Name == ".ctor"))
                return false;
            if (method.IsPrivate && method.HasOverrides)
                return false;
            return true;
        }

        public override void CreateAnnotations(DexTargetPackage targetPackage)
        {
            // Note: Maybe it makes more sense to not save this property 
            //       information as annotations, but as an asset e.g. in a
            //       simple .ini like format. This would also be true for 
            //       the AssemblyType annotations.

            var compiler = Compiler;

            if (!compiler.AddFrameworkPropertyAnnotations())
                return;

            var propertyClass = compiler.GetDot42InternalType("IProperty").GetClassReference(targetPackage);
            var propertiesClass = compiler.GetDot42InternalType("IProperties").GetClassReference(targetPackage);

            var reflInfoRef = compiler.GetDot42InternalType("Dot42.Internal.ReflectionInfo", "FrameworkPropertyInfoProvider")
                                      .GetClassReference(targetPackage);
            var relfInfo = targetPackage.DexFile.GetClass(reflInfoRef.Fullname);

            var thisRef = XType.GetClassReference(targetPackage);
            var thisrefDexClassName = thisRef.Descriptor.Substring(1, thisRef.Descriptor.Length - 2);

            var properties = relfInfo.Annotations.FirstOrDefault(a => a.Type.Fullname == propertiesClass.Fullname);
            if (properties == null)
            {
                properties = new Annotation(propertiesClass, AnnotationVisibility.Runtime);
                // note we are using a list, undil all class builders have
                properties.Arguments.Add(new AnnotationArgument("Properties", new List<Annotation>()));
                relfInfo.Annotations.Add(properties);
            }
            var propertyList = (List<Annotation>)properties.Arguments.Single().Value;

            foreach (var prop in Type.Properties)
            {
                // don't generate setter-only properties (which don't exist
                // due to the framework generator anyways)
                if (prop.GetMethod == null || prop.GetMethod.HasParameters)
                    continue;

                string propName = prop.Name;

                string getterName = GetMethodName(prop.GetMethod, thisrefDexClassName);
                string setterName = GetMethodName(prop.SetMethod, thisrefDexClassName);

                if (getterName == null && setterName == null)
                    continue;

                var ann = new Annotation(propertyClass, AnnotationVisibility.Runtime,
                                         new AnnotationArgument("DeclaringTypeDescriptor", thisRef.Descriptor),
                                         new AnnotationArgument("Name", propName));
                
                // We might save a few bytes by abbreviating the common pattern "isProperty" with
                // a special code like "is*". This would be interpreted by the PropertyInfoProvider
                // to generate the setter/getter names. 
                if (getterName != null && getterName != "get" + propName)
                    ann.Arguments.Add(new AnnotationArgument("Get", getterName));

                if (setterName != null && setterName != "set" + propName)
                    ann.Arguments.Add(new AnnotationArgument("Set", setterName));

                propertyList.Add(ann);
            }
        }

        internal static void FinalizeFrameworkPropertyAnnotations(AssemblyCompiler compiler, DexTargetPackage targetPackage)
        {
            var reflInfoRef = compiler.GetDot42InternalType("Dot42.Internal.ReflectionInfo", "FrameworkPropertyInfoProvider")
                          .GetClassReference(targetPackage);

            var relfInfo = targetPackage.DexFile.GetClass(reflInfoRef.Fullname);
            var propertiesClass = compiler.GetDot42InternalType("IProperties").GetClassReference(targetPackage);
            var properties = relfInfo.Annotations.FirstOrDefault(a => a.Type.Fullname == propertiesClass.Fullname);

            if (properties == null)
                return;

            var property = properties.Arguments.Single();
            properties.Arguments.Clear();
            properties.Arguments.Add(new AnnotationArgument(property.Name, ((List<Annotation>)property.Value).ToArray()));

        }

        private string GetMethodName(MethodDefinition method, string thisrefDexClassName)
        {
            if (method == null)
                return null;
            
            var attr = method.GetDexImportAttribute();
            if (attr == null) // ignore hybrid properties.
                return null;

            string methodName, descriptor, className;
            attr.GetDexOrJavaImportNames(method, out methodName, out descriptor, out className);

            if (className != thisrefDexClassName)
            {
                return null; // can't reference another type.
            }
            
            return methodName;
        }

        protected override TypeEntry CreateMappingEntry()
        {
            // Create mapping
            var dexName = Type.GetDexImportAttribute().ConstructorArguments[0].Value.ToString();
            var mapFileId = 0;
            var scopeId = Type.MetadataToken.ToScopeId();
            var entry = new TypeEntry(Type.FullName, Type.Scope.Name, dexName, mapFileId, scopeId);
            return entry;
        }
    }
}
