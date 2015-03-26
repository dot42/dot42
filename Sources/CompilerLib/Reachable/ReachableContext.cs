using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Dot42.CompilerLib.Ast.Extensions;
using Dot42.CompilerLib.Extensions;
using Dot42.CompilerLib.Reachable.DotNet;
using Dot42.CompilerLib.Structure;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.Extensions;
using Dot42.LoaderLib.Java;
using Mono.Cecil;
using FieldDefinition = Mono.Cecil.FieldDefinition;
using IReachableContext = Dot42.Cecil.IReachableContext;
using MethodDefinition = Mono.Cecil.MethodDefinition;
using TypeReference = Mono.Cecil.TypeReference;

namespace Dot42.CompilerLib.Reachable
{
    public sealed class ReachableContext : IReachableContext, JvmClassLib.IReachableContext, IClassLoader
    {
        private readonly AssemblyClassLoader assemblyClassLoader;
        private readonly HashSet<TypeDefinition> reachableTypes = new HashSet<TypeDefinition>();
        private readonly HashSet<ClassFile> reachableClasses = new HashSet<ClassFile>();
        private readonly Dictionary<TypeDefinition, List<IClassBuilder>> dotNetClassBuilders = new Dictionary<TypeDefinition, List<IClassBuilder>>();
        private readonly Dictionary<ClassFile, IClassBuilder> javaClassBuilders = new Dictionary<ClassFile, IClassBuilder>();
        private readonly Dictionary<AssemblyDefinition, List<TypeConditionInclude>> conditionalIncludes = new Dictionary<AssemblyDefinition, List<TypeConditionInclude>>();
        private readonly Dictionary<TypeDefinition, bool> isApplicationRootTypes = new Dictionary<TypeDefinition, bool>();
        private readonly AssemblyCompiler compiler;
        private bool newReachablesDetected;
        private readonly List<AssemblyNameDefinition> assemblyNames;
        private readonly HashSet<string> rootClassNames;

        private static readonly IEnumerable<IIncludeFieldTester> IncludeFieldTesters = CompositionRoot.CompositionContainer.GetExportedValues<IIncludeFieldTester>();
        private static readonly IEnumerable<IIncludeMethodTester> IncludeMethodTesters = CompositionRoot.CompositionContainer.GetExportedValues<IIncludeMethodTester>();

        /// <summary>
        /// Default ctor
        /// </summary>
        internal ReachableContext(AssemblyCompiler compiler, IEnumerable<AssemblyNameDefinition> assemblyNames, IEnumerable<string> rootClassNames)
        {
            this.compiler = compiler;
            this.rootClassNames = new HashSet<string>(rootClassNames.Select(x => x.ToLowerInvariant()));
            this.assemblyClassLoader = compiler.ClassLoader;
            this.assemblyNames = assemblyNames.ToList();
        }

        /// <summary>
        /// Gets the underlying compiler
        /// </summary>
        internal AssemblyCompiler Compiler { get { return compiler; } }

        /// <summary>
        /// Mark all assembly roots reachable.
        /// </summary>
        public void MarkRoots(AssemblyDefinition assembly, bool includeAllJavaCode)
        {
#if DEBUG
            //Debugger.Launch();
#endif

            // Mark all public types
            foreach (var typeDef in assembly.MainModule.GetTypes().Where(IsRoot))
            {
                typeDef.MarkReachable(this);
            }

            // Find InstanceOfCondition's
            var instanceOfConditions = FindInstanceOfConditionIncludes(assembly);
            foreach (var typeDef in assembly.MainModule.GetTypes().Where(x => !x.IsReachable))
            {
                foreach (var condition in instanceOfConditions)
                {
                    if (condition.IncludeIfNeeded(this, typeDef))
                    {
                        break;
                    }
                }
            }

            // Mark all includes types (via Dot42.IncludeAttribute)
            foreach (var attr in assembly.GetIncludeAttributes())
            {
                var arg = attr.Properties.Where(x => x.Name == AttributeConstants.IncludeAttributeTypeName).Select(x => (TypeReference) x.Argument.Value).FirstOrDefault();
                if (arg != null)
                {
                    arg.MarkReachable(this);
                }
            }

            // Load all java classes
            if (includeAllJavaCode || (rootClassNames.Count > 0))
            {
                foreach (var className in assemblyClassLoader.GetClassNames(assembly))
                {
                    ClassFile javaClass;
                    if (TryLoadClass(className, out javaClass))
                    {
                        if (includeAllJavaCode || IsRoot(javaClass))
                        {
                            MarkAsRoot(javaClass);
                        }
                        else
                        {
                            foreach (var condition in instanceOfConditions)
                            {
                                if (condition.IncludeIfNeeded(this, javaClass))
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mark all elements of the given class reachable.
        /// </summary>
        internal void MarkAsRoot(ClassFile javaClass)
        {
            javaClass.MarkReachable(this);
            foreach (var m in javaClass.Methods)
            {
                m.MarkReachable(this);
            }
            foreach (var f in javaClass.Fields)
            {
                f.MarkReachable(this);
            }            
        }

        /// <summary>
        /// Is there a C# class with a dex import that maps to the given class?
        /// </summary>
        internal bool HasDexImport(ClassFile javaClass)
        {
            AssemblyClassLoader.DexImport dexImport;
            return assemblyClassLoader.TryGetDexImport(javaClass.ClassName, out dexImport);
        }

        /// <summary>
        /// Finalize reachable detection.
        /// </summary>
        public void Complete()
        {
            while (newReachablesDetected)
            {
                newReachablesDetected = false;

                // Add members with "Include" attribute.
                FindConditionalIncludes();

                // Find .NET overrides and implementations that should be made reachable.
                var nonReachableDotNetMethods = reachableTypes.SelectMany(x => x.Methods).Where(x => !x.IsReachable).ToList();
                foreach (var method in nonReachableDotNetMethods)
                {
                    if (method.IsReachable)
                        continue;

                    // Is an imported java class marked reachable?
                    CustomAttribute javaImportAttr;
                    if ((javaImportAttr = method.GetJavaImportAttribute()) != null)
                    {
                        string className;
                        string memberName;
                        string descriptor;
                        javaImportAttr.GetDexOrJavaImportNames(method, out memberName, out descriptor, out className);
                        ClassFile javaClass;
                        if (TryLoadClass(className, out javaClass))
                        {
                            var javaMethod = javaClass.Methods.FirstOrDefault(x => (x.Name == memberName) && (x.Descriptor == descriptor));
                            if ((javaMethod != null) && (javaMethod.IsReachable))
                            {
                                method.MarkReachable(this);
                                continue;
                            }
                        }
                    }

                    // Is any base method reachable?
                    if (method.GetBaseMethods().Any(x => x.IsReachable))
                    {
                        method.MarkReachable(this);
                        continue;
                    }

                    // Is any base interface method reachable?
                    if (method.GetBaseInterfaceMethods().Any(x => x.IsReachable))
                    {
                        method.MarkReachable(this);
                        continue;
                    }

                    // Is any method from override collection reachable
                    if (method.Overrides.Select(x => x.GetElementMethod().Resolve()).Where(x => x != null).Any(x => x.IsReachable))
                    {
                        method.MarkReachable(this);
                        continue;                        
                    }
                }

                // Make sure all implementations of reachable interface methods are included
                var reachableInterfaceMethods = reachableTypes.Where(x => x.IsInterface).SelectMany(x => x.Methods).Where(x => x.IsReachable).ToList();
                foreach (var method in reachableInterfaceMethods)
                {
                    var interfaceType = method.DeclaringType.GetElementType();
                    var implementedBy = reachableTypes.Where(x => x.Implements(interfaceType)).ToList();
                    foreach (var type in implementedBy)
                    {
                        var implementation = method.GetImplementation(type);
                        implementation.MarkReachable(this);
                    }
                }

                // Find java overrides and implementations that should be made reachable.
                var nonReachableJavaMethods = reachableClasses.SelectMany(x => x.Methods).Where(x => !x.IsReachable).ToList();
                foreach (var method in nonReachableJavaMethods)
                {
                    if (method.IsReachable)
                        continue;

                    // Is any base method reachable?
                    if (method.GetBaseMethods().Any(x => x.IsReachable))
                    {
                        method.MarkReachable(this);
                        continue;
                    }

                    // Is any base interface method reachable?
                    if (method.GetBaseInterfaceMethods().Any(x => x.IsReachable))
                    {
                        method.MarkReachable(this);
                        continue;
                    }
                }
            }

            // create classbuilders (here, because now we know for all types
            // if they are used in Nullable<T>. 
            reachableTypes.ForEach(CreateClassBuilder);

        }

        /// <summary>
        /// Is the given type to be considered a root for the application?
        /// </summary>
        private bool IsRoot(TypeDefinition type)
        {
            if (rootClassNames.Contains(type.FullName.ToLowerInvariant()))
                return true;
            switch (compiler.CompilationMode)
            {
                case CompilationMode.Application:
                    return IsApplicationRoot(type);
                case CompilationMode.ClassLibrary:
                    return type.IsPublic || type.IsNestedPublic || type.IsNestedFamily || type.IsNestedFamilyOrAssembly;
                case CompilationMode.All:
                    var fullName = type.Module.Assembly.Name.FullName;
                    return assemblyNames.Any(x => x.FullName == fullName);
                default:
                    throw new CompilerException("Unknown compilation mode " + (int)compiler.CompilationMode);
            }
        }

        /// <summary>
        /// Is the given attribute type ApplicationRootAttribute or derived of that type?
        /// </summary>
        private bool IsApplicationRoot(TypeDefinition type)
        {
            if (type.HasCustomAttributes && type.CustomAttributes.Select(x => x.AttributeType).Any(IsApplicationRootAttribute))
                return true;

            // Ceck base type for application root attribute with "IncludeDerivedTypes" set to true.
            while (type.BaseType != null)
            {
                type = type.BaseType.Resolve(this);
                if (type == null)
                    return false;

                if (type.HasCustomAttributes)
                {
                    var appRootAttr = type.CustomAttributes.FirstOrDefault(x => IsApplicationRootAttribute(x.AttributeType));
                    if ((appRootAttr != null) && (appRootAttr.HasProperties) &&
                        (appRootAttr.Properties.Any(x => x.Name == AttributeConstants.ApplicationRootAttributeIncludeDerivedTypes)))
                    {
                        var includeDerType = (bool)appRootAttr.Properties.First(x => x.Name == AttributeConstants.ApplicationRootAttributeIncludeDerivedTypes).Argument.Value;
                        if (includeDerType)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Is the given attribute type ApplicationRootAttribute or derived of that type?
        /// </summary>
        private bool IsApplicationRootAttribute(TypeReference attributeType)
        {
            var typeDef = attributeType.Resolve();
            if (typeDef == null)
                return false;

            // Try the cache
            bool result;
            if (isApplicationRootTypes.TryGetValue(typeDef, out result))
                return result;

            // No in cache yet, find out
            if ((typeDef.Namespace == AttributeConstants.Dot42AttributeNamespace) &&
                (typeDef.Name == AttributeConstants.ApplicationRootAttributeName))
            {
                result = true;
            }
            else
            {
                // Try base type
                result = (typeDef.BaseType != null) && IsApplicationRootAttribute(typeDef.BaseType);
            }
            isApplicationRootTypes[typeDef] = result;
            return result;
        }

        /// <summary>
        /// Find the conditional includes in the included assemblies
        /// </summary>
        private void FindConditionalIncludes()
        {
            var reachableAssemblies = reachableTypes.Select(x => x.Module.Assembly).Where(x => x != null).Distinct().ToList();
            foreach (var assembly in reachableAssemblies)
            {
                List<TypeConditionInclude> includes;
                if (!conditionalIncludes.TryGetValue(assembly, out includes))
                {
                    // Load include list
                    includes = FindConditionalIncludes(assembly);
                    conditionalIncludes[assembly] = includes;
                }
                // Go over the list
                foreach (var include in includes)
                {
                    include.IncludeIfNeeded(this);
                }
            }
        }

        /// <summary>
        /// Find the conditional includes in the given assembly.
        /// </summary>
        private List<TypeConditionInclude> FindConditionalIncludes(AssemblyDefinition assembly)
        {
            var target = new List<TypeConditionInclude>();
            foreach (var type in assembly.MainModule.Types)
            {
                FindConditionalIncludes(type, target);
            }
            return target;
        }

        /// <summary>
        /// Find the conditional includes in the given assembly.
        /// </summary>
        private static void FindConditionalIncludes(TypeDefinition type, List<TypeConditionInclude> target)
        {
            FindConditionalIncludes<TypeDefinition>(type, target);
            foreach (var method in type.Methods)
            {
                FindConditionalIncludes(method, target);
            }
            foreach (var field in type.Fields)
            {
                FindConditionalIncludes(field, target);
            }
            foreach (var nestedType in type.NestedTypes)
            {
                FindConditionalIncludes(nestedType, target);
            }
        }

        /// <summary>
        /// Find the conditional includes in the given assembly.
        /// </summary>
        private static void FindConditionalIncludes<T>(T member, List<TypeConditionInclude> target)
            where T : MemberReference, ICustomAttributeProvider, IMemberDefinition
        {
            foreach (var attr in member.GetIncludeAttributes())
            {
                var arg = attr.Properties.Where(x => x.Name == AttributeConstants.IncludeAttributeTypeConditionName).Select(x=> (TypeReference)x.Argument.Value).FirstOrDefault();
                target.Add(new TypeConditionInclude(member, arg));
            }
            if (member.HasEventHandlerAttribute())
            {
                target.Add(new TypeConditionInclude(member, null));
            }
            var type = (member as TypeDefinition) ?? ((IMemberDefinition)member).DeclaringType;
            if ((type != null) && type.HasCustomViewAttribute())
            {
                // Add everything in a custom view
                target.Add(new TypeConditionInclude(member, null));                
            }
            if ((type != null) && (!(member is TypeDefinition)))
            {
                // Look for Include attribute with ApplyToMembers set.
                foreach (var attr in type.GetIncludeAttributes())
                {
                    bool arg = attr.AttributeType.Namespace == AttributeConstants.Dot42AttributeNamespace
                            && attr.AttributeType.Name == AttributeConstants.IncludeTypeAttributeName;

                    arg = arg || attr.Properties.Where(x => x.Name == AttributeConstants.IncludeAttributeApplyToMembersName).Select(x => (bool)x.Argument.Value).FirstOrDefault();
                    if (arg)
                    {
                        target.Add(new TypeConditionInclude(member, null));                        
                    }
                }
            }
        }

        /// <summary>
        /// Find the instanceof includes in the given assembly.
        /// </summary>
        private static List<InstanceOfConditionInclude> FindInstanceOfConditionIncludes(AssemblyDefinition member)
        {
            var target = new List<InstanceOfConditionInclude>();
            foreach (var attr in member.GetIncludeAttributes())
            {
                var arg = attr.Properties.Where(x => x.Name == AttributeConstants.IncludeAttributeInstanceOfConditionName).Select(x => (TypeReference)x.Argument.Value).FirstOrDefault();
                if (arg != null)
                {
                    target.Add(new InstanceOfConditionInclude(arg));
                }
            }
            return target;
        }

        /// <summary>
        /// Is the given type reference part of the "product" that should be included in the reachables search?
        /// </summary>
        public bool Contains(TypeReference typeRef)
        {
            return true;
        }

        /// <summary>
        /// Is the given type reference part of the "product" that should be included in the reachables search?
        /// </summary>
        public bool Contains(JvmClassLib.TypeReference typeRef)
        {
            return true;
        }

        /// <summary>
        /// Is the given type reference part of the "product" that should be included in the reachables search?
        /// </summary>
        public bool Contains(JvmClassLib.ClassFile typeRef)
        {
            return true;
        }

        /// <summary>
        /// Walk over the given member to marks its children reachable.
        /// </summary>
        public void Walk(MemberReference member)
        {
            DotNet.ReachableWalker.Walk(this, member);
        }

        /// <summary>
        /// Walk over the given member to marks its children reachable.
        /// </summary>
        public void Walk(AbstractReference member)
        {
            Java.ReachableWalker.Walk(this, member);
        }

        /// <summary>
        /// Called every time a new reachable member is detected.
        /// </summary>
        void IReachableContext.NewReachableDetected()
        {
            newReachablesDetected = true;
        }

        /// <summary>
        /// Called every time a new reachable member is detected.
        /// </summary>
        void JvmClassLib.IReachableContext.NewReachableDetected()
        {
            newReachablesDetected = true;
        }

        /// <summary>
        /// Gets the type for the given reference.
        /// </summary>
        public TypeDefinition GetTypeDefinition(TypeReference typeRef)
        {
            return typeRef.Resolve();
        }

        /// <summary>
        /// Add the given type to the list of reachable types.
        /// </summary>
        public void RecordReachableType(TypeDefinition type)
        {
            reachableTypes.Add(type);
        }

        /// <summary>
        /// Add the given type to the list of reachable classes.
        /// </summary>
        public void RecordReachableType(ClassFile classFile)
        {
            if (!classFile.IsCreatedByLoader)
            {
                reachableClasses.Add(classFile);
                CreateClassBuilder(classFile);
            }
        }

        /// <summary>
        /// Gets all types recorded as reachable.
        /// </summary>
        public IEnumerable<TypeDefinition> ReachableTypes { get { return reachableTypes; } }

        /// <summary>
        /// Create a class builder for the given type.
        /// </summary>
        private void CreateClassBuilder(TypeDefinition type)
        {
            if ((!type.IsNested) && (!dotNetClassBuilders.ContainsKey(type)))
            {
                dotNetClassBuilders.Add(type, Structure.DotNet.ClassBuilder.Create(this, compiler, type).ToList());
            }
        }

        /// <summary>
        /// Create a class builder for the given type.
        /// </summary>
        private void CreateClassBuilder(ClassFile type)
        {
            if ((!type.IsNested) && (!javaClassBuilders.ContainsKey(type)))
            {
                javaClassBuilders.Add(type, Structure.Java.ClassBuilder.Create(compiler, type));
            }
        }

        /// <summary>
        /// Gets all created type builders.
        /// </summary>
        internal IEnumerable<IClassBuilder> ClassBuilders
        {
            get { return dotNetClassBuilders.Values.SelectMany(x => x).Concat(javaClassBuilders.Values); }
        }

        /// <summary>
        /// Should the given field be included in the APK?
        /// </summary>
        public bool Include(FieldDefinition field)
        {
            switch (compiler.CompilationMode)
            {
                case CompilationMode.Application:
                    break;
                case CompilationMode.ClassLibrary:
                    if (field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly)
                        return true;
                    break;
                case CompilationMode.All:
                    if (IsRoot(field.DeclaringType))
                        return true;
                    break;
                default:
                    throw new CompilerException("Unknown compilation mode " + (int)compiler.CompilationMode);
            }
            return IncludeFieldTesters.Any(x => x.Include(field, this));
        }

        /// <summary>
        /// Should the given event be included in the APK?
        /// </summary>
        public bool Include(EventDefinition evt)
        {
            return false;
        }

        /// <summary>
        /// Should the given property be included in the APK?
        /// </summary>
        public bool Include(PropertyDefinition prop)
        {
            return false;
        }

        /// <summary>
        /// Should the given method be included in the APK?
        /// </summary>
        public bool Include(MethodDefinition method)
        {
            switch (compiler.CompilationMode)
            {
                case CompilationMode.Application:
                    break;
                case CompilationMode.ClassLibrary:
                    if (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly)
                        return true;
                    break;
                case CompilationMode.All:
                    if (IsRoot(method.DeclaringType))
                        return true;
                    break;
                default:
                    throw new CompilerException("Unknown compilation mode " + (int)compiler.CompilationMode);
            }
            return IncludeMethodTesters.Any(x => x.Include(method, this));
        }

        /// <summary>
        /// Should the given class be considered a root?
        /// </summary>
        internal bool IsRoot(ClassFile classFile)
        {
            var className = classFile.ClassName.ToLowerInvariant();
            if (rootClassNames.Contains(className))
                return true;
            className = className.Replace('/', '.');
            if (rootClassNames.Contains(className))
                return true;
            className = className.Replace('$', '.');
            return rootClassNames.Contains(className);
        }

        /// <summary>
        /// Load a java class with given name.
        /// </summary>
        public bool TryLoadClass(string className, out ClassFile result)
        {
            return assemblyClassLoader.TryLoadClass(className, out result);
        }

        /// <summary>
        /// Gets all package names found in this loader.
        /// </summary>
        IEnumerable<string> IClassLoader.Packages
        {
            get { return assemblyClassLoader.Packages; }
        }
    }
}
