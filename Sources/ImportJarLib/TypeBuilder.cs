using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Dot42.FrameworkDefinitions;
using Dot42.ImportJarLib.Doxygen;
using Dot42.ImportJarLib.Model;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Java;
using Dot42.Utility;

namespace Dot42.ImportJarLib
{
    /// <summary>
    /// Helper used to build type definitions from ClassFile's
    /// </summary>
    public abstract class TypeBuilder : IBuilderGenericContext
    {
        private readonly List<NestedTypeBuilder> nestedTypeBuilders = new List<NestedTypeBuilder>();
        private List<FieldBuilder> fieldBuilders;
        private List<MethodBuilder> methodBuilders;
        private PropertyBuilder propertyBuilder;

        /// <summary>
        /// Helps in sorting type builders
        /// </summary>
        public virtual int Priority { get { return int.MaxValue/2; } }

        /// <summary>
        /// Create a type defrinition for the given class file and all inner classes.
        /// Add a mapping in the given type name map.
        /// </summary>
        public abstract void CreateType(NetTypeDefinition declaringType, NetModule module, TargetFramework target);

        /// <summary>
        /// Prepare the given type.
        /// This is called just after the type is created and before nested classes are created.
        /// </summary>
        protected void CreateGenericParameters(ClassFile cf, NetTypeDefinition typeDef)
        {
            // Implemented generic attributes
            var signature = cf.Signature;
            if (AddGenericParameters)
            {
                foreach (var tp in signature.TypeParameters)
                {
                    typeDef.GenericParameters.Add(new NetGenericParameter(tp.Name, tp.Name, typeDef));
                }
            }
        }

        /// <summary>
        /// Implement members and setup references now that all types have been created
        /// </summary>
        public virtual void Implement(TargetFramework target)
        {
            nestedTypeBuilders.ForEach(x => x.Implement(target));
        }

        /// <summary>
        /// Implement bindings for the given type
        /// </summary>
        protected void Implement(ClassFile cf, NetTypeDefinition typeDef, bool isEnum, bool isInterfaceConstants, TargetFramework target)
        {
            // Find super class
            var signature = cf.Signature;
            if (!typeDef.IsInterface)
            {
                if (isEnum)
                {
                    // Set base type
                    typeDef.BaseType = target.TypeNameMap.GetByType(typeof(Enum));
                }
                else if ((!isInterfaceConstants) && (signature.SuperClass != null))
                {
                    var notFound = false;
                    try
                    {
                        NetTypeReference baseType;
                        if (signature.SuperClass.TryResolve(target, this, false, out baseType))
                        {
                            if (!typeDef.IsObject())
                            {
                                typeDef.BaseType = baseType;
                            }
                            if (baseType == null)
                            {
                                throw new ArgumentException("Unknown base type: " + signature.SuperClass);
                            }
                        }
                        else
                        {
                            notFound = true;
                        }
                    }
                    catch (ClassNotFoundException)
                    {
                        notFound = true;
                    }
                    if (notFound)
                    {
                        Console.WriteLine("Base type {0} not found in {1}", signature.SuperClass.ClassName, typeDef.FullName);
                    }
                }
                else
                {
                    // Must be java/lang/Object
                }
            }

            // Fix interface or internal base types
            if (!isEnum && !isInterfaceConstants)
            {
                FixBaseType(typeDef, target);
            }

            // Find implemented interfaces
            if (!isEnum && !isInterfaceConstants)
            {
                ImplementInterfaces(typeDef, signature, target);
            }

            // Add JavaRef attribute
            var ca = CreateDexImportAttribute(cf);
            typeDef.CustomAttributes.Add(ca);

            // Build fields
            fieldBuilders = cf.Fields.Where(x => ShouldImplement(x, target)).Select(x => new FieldBuilder(x, this)).ToList();
            fieldBuilders.ForEach(x => x.Create(typeDef, target));
            fieldBuilders.ForEach(x => x.Complete(cf, target));

            // Build methods
            methodBuilders = cf.Methods.Where(x => ShouldImplement(x, target)).SelectMany(x => MethodBuilder.Create(x, this)).ToList();
            if (isEnum && !isInterfaceConstants)
            {
                AddPrivateDefaultCtor(typeDef, target);
            }
            methodBuilders.ForEach(x => x.Create(typeDef, target));
            methodBuilders.ForEach(x => x.Complete(cf, target));

            // Fixup
            RemoveDuplicateMethods(typeDef);
            AddDefaultConstructor(typeDef, target);
            RemoveInterfaceConstructors(typeDef);
        }

        /// <summary>
        /// Create the DexImport that will be attached to the type.
        /// </summary>
        protected virtual NetCustomAttribute CreateDexImportAttribute(ClassFile cf)
        {
            var ca = new NetCustomAttribute(null, cf.ClassName);
            ca.Properties.Add(AttributeConstants.DexImportAttributeAccessFlagsName, (int)cf.AccessFlags);
            var signature = (cf.Signature != null) ? cf.Signature.Original : null;
            if ((signature != null) && (signature != cf.ClassName))
            {
                ca.Properties.Add(AttributeConstants.DexImportAttributeSignature, signature);
            }
            return ca;
        }

        /// <summary>
        /// Fix base type relations.
        /// </summary>
        protected virtual void FixBaseType(NetTypeDefinition typeDef, TargetFramework target)
        {
            if (typeDef.BaseType == null)
                return;

            // If base type is internal, use first public base type
            while ((typeDef.BaseType != null) && (typeDef.BaseType.GetElementType().IsNotPublic))
            {
                // Mark this case
                typeDef.HasInternalBaseClass = true;

                // Internal base type, use first public base type.
                var baseType = typeDef.BaseType.GetElementType();
                // Change base type
                typeDef.BaseType = baseType.BaseType;

                if ((baseType.ClassFile != ClassFile.Empty) && (typeDef.ClassFile != ClassFile.Empty))
                {
                    // Implement interfaces of base type
                    ImplementInterfaces(typeDef, baseType.ClassFile.Signature, target);

                    // Add fields of base type
                    foreach (var field in baseType.ClassFile.Fields.Where(x => x.IsPublic && !x.IsSynthetic))
                    {
                        typeDef.ClassFile.Fields.Add(field.CloneTo(typeDef.ClassFile));
                    }
                    // Add methods of base type
                    foreach (var method in baseType.ClassFile.Methods.Where(x => x.Name[0] != '<'))
                    {
                        typeDef.ClassFile.Methods.Add(method.CloneTo(typeDef.ClassFile));
                    }
                }
            }

            // If base type is interface, move it there
            if ((typeDef.BaseType != null) && (typeDef.BaseType.GetElementType().IsInterface))
            {
                typeDef.Interfaces.Insert(0, typeDef.BaseType);
                typeDef.BaseType = null;
            }
        }

        /// <summary>
        /// All all interfaces in the given signature to the given type definition.
        /// </summary>
        private void ImplementInterfaces(NetTypeDefinition typeDef, ClassSignature signature, TargetFramework target)
        {
            if (signature == null)
                return;
            foreach (var interfaceName in signature.Interfaces)
            {
                try
                {
                    NetTypeDefinition iDef;
                    if (target.TypeNameMap.TryGetByJavaClassName(interfaceName.ClassName, out iDef) && iDef.IsInterface)
                    {
                        var iRef = interfaceName.Resolve(target, this, false);
                        if (!typeDef.Interfaces.Any(x => x.AreSame(iRef)))
                        {
                            typeDef.Interfaces.Add(iRef);
                        }
                    }
                }
                catch (ClassNotFoundException)
                {
                    Console.WriteLine("Interface {0} not found in {1}", interfaceName.ClassName, typeDef.FullName);
                }
            }
        }

        /// <summary>
        /// Add a private default constructor.
        /// </summary>
        private static void AddPrivateDefaultCtor(NetTypeDefinition typeDef, TargetFramework target)
        {
            var ctor = new NetMethodDefinition(".ctor", null, typeDef, target, false, "TypeBuilder.AddPrivateDefaultCtor")
            {
                Attributes = MethodAttributes.Private, 
                AccessFlags = (int)MethodAccessFlags.Private
            };
            typeDef.Methods.Add(ctor);
        }

        /// <summary>
        /// Remove duplicate methods from the type definition.
        /// </summary>
        protected static void RemoveDuplicateMethods(NetTypeDefinition typeDef)
        {
            var methods = typeDef.Methods.ToList();
            for (var i = 0; i < methods.Count; i++)
            {
                var m1 = methods[i];
                for (var j = i + 1; j < methods.Count; )
                {
                    var mj = methods[j];
                    if (m1.IsDuplicate(mj))
                    {
                        // Remove m2
                        methods.RemoveAt(j);
                        typeDef.Methods.Remove(mj);
                        if (m1.Description == null) m1.Description = mj.Description;
                    }
                    else
                    {
                        j++;
                    }
                }
            }
        }
    
        /// <summary>
        /// Rename members that conflict with other members.
        /// </summary>
        protected static void FixMemberNames<T, TConflict>(NetTypeDefinition typeDef, IEnumerable<T> members, IEnumerable<TConflict> conflictingMembers, Action<T, string> renamer)
            where T : INetMemberDefinition
            where TConflict : INetMemberDefinition
        {
            var conflictingMembersList = conflictingMembers.ToList();
            var membersList = members.ToList();
            foreach (var m in membersList)
            {
                var postfix = 0;
                var originalName = m.Name;
                while ((typeDef.Name == m.Name) || (conflictingMembersList.Any(x => !ReferenceEquals(x, m) && (x.Name == m.Name))))
                {
                    renamer(m, originalName + postfix);
                    postfix++;
                }
            }
        }

        /// <summary>
        /// Add a default ctor if there is none, but there are other ctors.
        /// </summary>
        private void AddDefaultConstructor(NetTypeDefinition typeDef, TargetFramework target)
        {
            if (typeDef.IsStruct || typeDef.IsInterface || (this is IInterfaceConstantsTypeBuilder))
                return;
            var ctors = typeDef.Methods.Where(x => x.IsConstructor).ToList();
            if (ctors.Count == 0)
                return;
            // Is there a default ctor?
            if (ctors.Any(x => x.Parameters.Count == 0))
                return;

            // Add default ctor
            var ctor = new NetMethodDefinition(".ctor", null, typeDef, target, false, "TypeBuilder.AddDefaultConstructor")
            {
                Attributes = MethodAttributes.Assembly,
                AccessFlags = (int) MethodAccessFlags.Protected,
                EditorBrowsableState = EditorBrowsableState.Never
            };
            typeDef.Methods.Add(ctor);
        }

        /// <summary>
        /// Remove all ctors from interfaces.
        /// </summary>
        private static void RemoveInterfaceConstructors(NetTypeDefinition typeDef)
        {
            if (!typeDef.IsInterface)
                return;
            foreach (var ctor in typeDef.Methods.Where(x => x.IsConstructor).ToList())
            {
                typeDef.Methods.Remove(ctor);
            }
        }

        /// <summary>
        /// Remove abstract methods that override another abstract method.
        /// </summary>
        private static void RemoveAbstractOverrides(NetTypeDefinition typeDef)
        {
            foreach (var iterator in typeDef.Methods.Where(x => x.IsAbstract).ToList())
            {
                var method = iterator;
                if (typeDef.GetBaseTypesWithoutInterfaces().SelectMany(x => x.GetElementType().Methods).Any(x => x.IsAbstract && x.IsExactDuplicate(method)))
                {
                    typeDef.Methods.Remove(method);                    
                }
            }
        }
        
        /// <summary>
        /// Implement interface members
        /// </summary>
        public virtual void Finalize(TargetFramework target, FinalizeStates state)
        {
            if (methodBuilders != null) methodBuilders.ForEach(x => x.Finalize(target, state));
            if (fieldBuilders != null) fieldBuilders.ForEach(x => x.Finalize(target, state));
            nestedTypeBuilders.ForEach(x => x.Finalize(target, state));
        }

        /// <summary>
        /// Update names where needed
        /// </summary>
        public abstract void FinalizeNames(TargetFramework target, MethodRenamer methodRenamer);

        /// <summary>
        /// Update names where needed
        /// </summary>
        protected void FinalizeNames(NetTypeDefinition typeDef, TargetFramework target, MethodRenamer methodRenamer)
        {
            if (methodBuilders != null) methodBuilders.ForEach(x => x.FinalizeNames(target, methodRenamer));
            nestedTypeBuilders.ForEach(x => x.FinalizeNames(target, methodRenamer));

            //FixMemberNames(typeDef, typeDef.Methods, typeDef.NestedTypes, (m, n) => methodRenamer.Rename(m, n));
            FixMemberNames(typeDef, typeDef.Fields, typeDef.NestedTypes, (m, n) => m.Name = n);
            FixMemberNames(typeDef, typeDef.Fields, typeDef.Methods, (m, n) => m.Name = n);
            FixMemberNames(typeDef, typeDef.Fields, typeDef.Fields, (m, n) => m.Name = n);

            // Build properties
            propertyBuilder = CreatePropertyBuilder(typeDef);
            propertyBuilder.BuildProperties(target, methodRenamer);

            typeDef.EnsureVisibility();
        }

        /// <summary>
        /// Implement interface members
        /// </summary>
        protected void Finalize(NetTypeDefinition typeDef, TargetFramework target, FinalizeStates state)
        {
            if (typeDef.IsInterface) 
                return;

            // Fixup visibility
            typeDef.EnsureVisibility();

            if (state != FinalizeStates.AddRemoveMembers)
                return;

            // Cleanup
            RemoveAbstractOverrides(typeDef);

            // Add interface methods
            AddAbstractInterfaceMethods(typeDef, target);
        }

        /// <summary>
        /// Add abstract method (that do not yet exist) for all interface methods.
        /// </summary>
        private void AddAbstractInterfaceMethods(NetTypeDefinition typeDef, TargetFramework target)
        {
            if ((typeDef.IsInterface) || (this is IInterfaceConstantsTypeBuilder))
                return;

            var typeMethods = typeDef.Methods.ToList();
            foreach (var iterator in typeDef.Interfaces)
            {
                // Collect all interface methods
                var iDef = iterator;
                var iMethods = iDef.GetElementType().Methods.Select(x => Tuple.Create(iDef, x)).ToList();
                iMethods.AddRange(iDef.GetElementType().GetBaseTypes(true).Where(x => x.GetElementType().IsInterface).SelectMany(x => x.GetElementType().Methods.Select(m => Tuple.Create(x, m))));
                foreach (var pair in iMethods)
                {
                    var iMethod = pair.Item2;
                    if (typeMethods.Any(x => x.IsExactDuplicate(iMethod))) 
                        continue;

                    // Add abstract method
                    var method = new NetMethodDefinition(iMethod.Name, null, typeDef, target, iMethod.IsSignConverted, "TypeBuilder.AddAbstractInterfaceMethods");
                    method.AccessFlags = iMethod.AccessFlags;
                    method.Attributes = iMethod.Attributes;
                    method.IsAbstract = false;
                    method.IsVirtual = !typeDef.IsSealed;
                    method.GenericParameters.AddRange(iMethod.GenericParameters.Select(x => new NetGenericParameter(GetMethodGenericParameterName(typeDef, x.Name), x.Name, method)));
                    method.ReturnType = GenericParameters.Resolve(iMethod.ReturnType, typeDef, target.TypeNameMap);
                    method.Parameters.AddRange(iMethod.Parameters.Select(x => new NetParameterDefinition(x.Name, GenericParameters.Resolve(x.ParameterType, typeDef, target.TypeNameMap), x.IsParams)));

                    // Check if there are duplicates now that the generic parameters are resolved.
                    if (typeMethods.Any(x => x.IsExactDuplicate(method)))
                        continue;

                    // Should we ignore this one?
                    if (IgnoreInterfaceMethod(method, iMethod))
                        continue;

                    // Test if the method is a duplicate of an existing method, but with different return type
                    var differentAccessMask = !method.HasSameVisibility(iMethod);
                    if (differentAccessMask || typeMethods.Any(x => x.IsDuplicate(method)) ||
                        ImplementAsExplicitInterfaceMethod(method, iMethod) || typeDef.NestedTypes.Any(x => x.Name == method.Name))
                    {
                        // Method with different return type exists.
                        // Make it an explicit interface method
                        method.SetExplicitImplementation(iMethod, pair.Item1);
                        method.IsAbstract = false;
                        method.IsVirtual = false;
                    }
                    else
                    {
                        // Check for override
                        //method.IsOverride = isOverride;
                        if (method.IsOverride && method.Overrides.All(x => !x.IsVirtual))
                        {
                            // No need to override anything
                            continue;
                        }
                    }

                    // Create DexImport attribute
                    var typeCa = iMethod.DeclaringType.CustomAttributes.First(x => x.AttributeType == null);
                    var methodCa = iMethod.CustomAttributes.First(x => x.AttributeType == null);
                    var ca = new NetCustomAttribute(null, typeCa.ConstructorArguments[0], methodCa.ConstructorArguments[0], methodCa.ConstructorArguments[1]);
                    ca.CopyPropertiesFrom(methodCa);
                    method.CustomAttributes.Add(ca);                    

                    typeDef.Methods.Add(method);
                    typeMethods.Add(method);

                    // Avoid name conflicts
                    var nested = typeDef.NestedTypes.FirstOrDefault(x => x.Name == method.Name);
                    if (nested != null)
                    {
                        nested.Name = "Java" + nested.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Should the given method be implemented as an explicit interface implementation?
        /// </summary>
        protected virtual bool ImplementAsExplicitInterfaceMethod(NetMethodDefinition method, NetMethodDefinition interfaceMethod)
        {
            /*if (method.Name == "Clone")
                return true;*/
            return false;
        }

        /// <summary>
        /// Should the given interface implementation method be left out?
        /// </summary>
        protected virtual bool IgnoreInterfaceMethod(NetMethodDefinition method, NetMethodDefinition interfaceMethod)
        {
            return false;
        }

        /// <summary>
        /// Implement interface members
        /// </summary>
        public void FinalizeProperties(TargetFramework target, MethodRenamer methodRenamer)
        {
            if (propertyBuilder != null) propertyBuilder.Finalize(target, methodRenamer);
            nestedTypeBuilders.ForEach(n => n.FinalizeProperties(target, methodRenamer));
        }

        /// <summary>
        /// Make sure that base types are visible.
        /// </summary>
        public abstract void FinalizeVisibility(TargetFramework target);

        /// <summary>
        /// Adds the given nested type to my type declaration.
        /// </summary>
        protected internal abstract void AddNestedType(NetTypeDefinition nestedType, string namePrefix, NetModule module, ref string fullNestedTypeName);

        /// <summary>
        /// Create nested types
        /// </summary>
        protected virtual void CreateNestedTypes(ClassFile cf, NetTypeDefinition declaringType, string parentFullName, NetModule module, TargetFramework target)
        {
            foreach (var inner in cf.InnerClasses)
            {
                ClassFile innerCf;
                if (cf.Loader.TryLoadClass(inner.Inner.ClassName, out innerCf) && (innerCf.DeclaringClass == cf) && ShouldImplement(inner, target))
                {
                    foreach (var typeBuilder in NestedTypeBuilder.Create(this, parentFullName, innerCf, inner))
                    {
                        typeBuilder.CreateType(declaringType, module, target);
                        nestedTypeBuilders.Add(typeBuilder);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the name of the given field
        /// </summary>
        public virtual string GetFieldName(FieldDefinition field)
        {
            var name = NameConverter.UpperCamelCase(field.Name.Replace('$', '_'));
            return name;
        }

        /// <summary>
        /// Should the given class be implemented?
        /// </summary>
        protected static bool ShouldImplement(ClassFile @class, TargetFramework target)
        {
            if ((target.ImportPublicOnly) && (!@class.IsPublic))
                return false;
            var pkg = @class.Package.Replace('/', '.');
            if (target.IsExcludedPackage(pkg))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Should the given class be implemented?
        /// </summary>
        protected static bool ShouldImplement(InnerClass inner, TargetFramework target)
        {
            if ((target.ImportPublicOnly) && (!(inner.IsPublic || inner.IsProtected)))
                return false;
            return true;
        }

        /// <summary>
        /// Should the given field be implemented?
        /// </summary>
        protected internal virtual bool ShouldImplement(FieldDefinition field, TargetFramework target)
        {
            if ((target.ImportPublicOnly) && (!(field.IsPublic || field.IsProtected)))
                return false;
            return true;
        }

        /// <summary>
        /// Should the given method be implemented?
        /// </summary>
        protected virtual bool ShouldImplement(MethodDefinition method, TargetFramework target)
        {
            if ((target.ImportPublicOnly) && (!(method.IsPublic || method.IsProtected)))
            {
                // Do not import private methods, unless...
                if (method.Name == "<init>")
                {
                    // If there are only private ctor's, implement them
                    if (method.DeclaringClass.Methods.Where(x => x.Name == "<init>").All(x => !(x.IsPublic || x.IsProtected)))
                        return true;
                }
                return false;
            }
            // Is this method abstract and there is another method with the exact same descriptor?
            if (method.IsAbstract)
            {
                if (method.DeclaringClass.Methods.Any(x => (x != method) && (x.Name == method.Name) && (x.Descriptor == method.Descriptor)))
                {
                    return false;
                }
            }

            return (!method.Name.Equals("<clinit>"));
        }

        /// <summary>
        /// Gets the name of the given method.
        /// This is the initial name, a direct conversion from java.
        /// </summary>
        internal MethodNameInfo GetMethodName(MethodDefinition method)
        {
            switch (method.Name)
            {
                case "<init>":
                    return new MethodNameInfo(".ctor", true, false);
                case "<clinit>":
                    return new MethodNameInfo(".cctor", true, false);
                case "finalize":
                    return new MethodNameInfo("Finalize", false, true);
                case "hashCode":
                    return new MethodNameInfo("GetHashCode");
                default:
                    return new MethodNameInfo(NameConverter.UpperCamelCase(method.Name));
            }
        }

        /// <summary>
        /// Modify the name of the given method to another name.
        /// By calling renamer.Rename, all methods in the same group are also updated.
        /// </summary>
        public virtual void ModifyMethodName(NetMethodDefinition method, MethodRenamer renamer)
        {
            var name = method.Name;
            var newName = name;
            if (name.IndexOf('$') >= 0)
            {
                newName = name.Replace('$', '_');
            }
            var typeDef = method.DeclaringType;
            if ((typeDef != null) && ((name == typeDef.Name) || typeDef.GenericParameters.Any(x => x.Name == name)))
            {
                // Name conflict with generic parameter
                newName = "Java" + name;
            }

            if (newName != name)
            {
                renamer.Rename(method, newName);
            }
        }

        /// <summary>
        /// Update the attributes of the given method
        /// </summary>
        public abstract MethodAttributes GetMethodAttributes(MethodDefinition method, MethodAttributes attributes);

        /// <summary>
        /// Update the attributes of the given method
        /// </summary>
        protected static MethodAttributes GetMethodAttributes(NetTypeDefinition typeDef, MethodDefinition method, MethodAttributes attributes)
        {
            if ((typeDef.IsStruct) || ((typeDef.Attributes & TypeAttributes.Sealed) != 0))
                attributes |= MethodAttributes.Final;

            return attributes;
        }

        /// <summary>
        /// Create a property builder for this type builder.
        /// </summary>
        protected virtual PropertyBuilder CreatePropertyBuilder(NetTypeDefinition typeDef)
        {
            return new PropertyBuilder(typeDef, this);
        }

        /// <summary>
        /// Add generic parameters to my type?
        /// </summary>
        protected virtual bool AddGenericParameters { get { return true; } }

        /// <summary>
        /// Resolve the given generic parameter into a type reference.
        /// </summary>
        bool IBuilderGenericContext.TryResolveTypeParameter(string name, TargetFramework target, out NetTypeReference type)
        {
            return TryResolveTypeParameter(name, target, out type);
        }

        /// <summary>
        /// Resolve the given generic parameter into a type reference.
        /// </summary>
        protected virtual bool TryResolveTypeParameter(string name, TargetFramework target, out NetTypeReference type)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Full typename of the context without any generic types.
        /// </summary>
        string IBuilderGenericContext.FullTypeName
        {
            get { return FullTypeName; }
        }

        /// <summary>
        /// Full typename of the context without any generic types.
        /// </summary>
        protected abstract string FullTypeName { get; }

        /// <summary>
        /// Gets the documentation of this type.
        /// Can be null.
        /// </summary>
        public abstract DocClass Documentation { get; }

        /// <summary>
        /// Add this type to the layout.xml file if needed.
        /// </summary>
        public abstract void FillLayoutXml(JarFile jf, XElement parent);

        /// <summary>
        /// Add this type to the typemap.xml file if needed.
        /// </summary>
        public abstract void FillTypemapXml(JarFile jf, XElement parent);

        /// <summary>
        /// Improve the type name for the given class.
        /// </summary>
        protected virtual string CreateTypeName(NetTypeDefinition declaringType, ClassFile classFile, string name, string @namespace)
        {
            // Prefix interfaces
            if (classFile.IsInterface)
            {
                var hasIPrefix = (name.Length >= 2) && (name[0] == 'I') && char.IsUpper(name[1]);
                if (!hasIPrefix)
                {
                    name = "I" + name;
                }
            }

            // Avoid digit prefixes
            if ((name.Length > 0) && char.IsDigit(name[0]))
            {
                name = "_" + name;
            }

            if ((declaringType != null) && (declaringType.Name == name))
            {
                // Alter name
                name = name + '_' + Math.Abs(declaringType.FullName.GetHashCode());
            }

            // Avoid name clashes with namespaces
            var packageNames = classFile.Loader.Packages.Select(x => x.Replace('/', '.').ToLowerInvariant()).ToList();
            var fullNameLower = (string.IsNullOrEmpty(@namespace) ? name : @namespace + '.' + name).ToLowerInvariant();
            if (packageNames.Contains(fullNameLower))
            {
                // Name equals last part of the namespace
                name = name + '_' + Math.Abs(classFile.ClassName.GetHashCode());
            }

            return name;
        }

        /// <summary>
        /// Gets the name of the given field
        /// </summary>
        protected static string FixFieldName(string name, FieldDefinition field, NetTypeDefinition typeDef)
        {
            /*if (name == typeDef.Name)
            {
                // Name conflict, rename myself.
                typeDef.Name = typeDef.Name + Math.Abs(field.DeclaringClass.ClassName.GetHashCode());
            }
            if (typeDef.GenericParameters.Any(x => x.Name == name))
            {
                // Name conflict with generic parameter
                name = "_" + name;
            }*/
            if ((name == typeDef.Name) || typeDef.GenericParameters.Any(x => x.Name == name))
            {
                // Name conflict with generic parameter
                name = "Java" + name;
            }
            return name;
        }

        /// <summary>
        /// Gets the name for a type parameter of a method of my class.
        /// If the name is the same as a type parameter of this class, we rename it, otherwise the given original is returned.
        /// </summary>
        internal static string GetMethodGenericParameterName(NetTypeDefinition typeDef, string original)
        {
            if (typeDef.GenericParameters.Any(x => x.Name == original))
                return "Java" + original;
            return original;
        }
    }
}
