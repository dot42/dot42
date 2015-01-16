using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dot42.Documentation;
using Mono.Cecil;

namespace Dot42.BuildTools.Documentation
{
    internal sealed class FrameworkDocumentationSet : DocumentationSet
    {
        private readonly string folder;
        private readonly Version version;

        /// <summary>
        /// Default ctor
        /// </summary>
        public FrameworkDocumentationSet(string folder)
        {
            this.folder = folder;
            version =  Version.Parse(Path.GetFileName(folder).Substring(1));
        }

        public Version Version
        {
            get { return version; }
        }

        /// <summary>
        /// Load all documentation
        /// </summary>
        public void LoadAll()
        {
            foreach (var assemblyPath in Directory.GetFiles(folder, "*.dll"))
            {
                Load(assemblyPath);
            }
        }

        /// <summary>
        /// Load all documentation for the given assembly
        /// </summary>
        private void Load(string assemblyPath)
        {
            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            var summary = new SummaryFile(Path.ChangeExtension(assemblyPath, ".xml"));
            // Load all types
            foreach (var type in assembly.MainModule.Types)
            {
                LoadType(type, null, summary);
            }
            // Load all "empty" namespaces
            AddEmptyNamespaces();
            // Connect parent-children
            AttachChildNamespaces();
        }

        /// <summary>
        /// Create namespaces for each missing parent namespace.
        /// </summary>
        private void AddEmptyNamespaces()
        {
            while (true)
            {
                var allParents = Namespaces.Select(x => x.ParentName).Where(x => x != null).Distinct().ToList();
                var namespacesAdded = false;
                foreach (var parentName in allParents)
                {
                    NamespaceDocumentation ns;
                    if (!TryGetNamespace(parentName, out ns))
                    {
                        namespacesAdded = true;
                        ns = new NamespaceDocumentation(parentName);
                        Add(ns);
                    }
                }
                if (!namespacesAdded)
                    break;
            }
        }

        /// <summary>
        /// Connect the Children of each namespace.
        /// </summary>
        private void AttachChildNamespaces()
        {
            foreach (var ns in Namespaces)
            {
                var parentName = ns.ParentName;
                if (parentName == null)
                    continue;
                NamespaceDocumentation parent;
                if (!TryGetNamespace(parentName, out parent))
                    throw new ArgumentException("Missing namespace: " + parentName);
                parent.Children[ns.Name] = ns;
            }            
        }

        /// <summary>
        /// Load type documentation for the given type and all its members
        /// </summary>
        private void LoadType(TypeDefinition type, TypeDocumentation declaringType, SummaryFile summary)
        {
            // Exclude non-visible types
            if (type.IsNotPublic || type.IsNestedAssembly || type.IsNestedPrivate || type.IsNestedFamilyAndAssembly)
                return;

            // Generate documentation
            var doc = new TypeDocumentation(type, declaringType);

            foreach (var nestedType in type.NestedTypes)
            {
                LoadType(nestedType, doc, summary);
            }
            doc.Events.AddRange(type.Events.Where(IsVisible).OrderBy(Descriptor.Create).Select(x => new EventDocumentation(x, doc)));
            doc.Fields.AddRange(type.Fields.Where(IsVisible).OrderBy(Descriptor.Create).Select(x => new FieldDocumentation(x, doc)));
            doc.Methods.AddRange(type.Methods.Where(IsVisibleAndNormal).OrderBy(x => Descriptor.Create(x, true)).Select(x => new MethodDocumentation(x, doc)));
            doc.Properties.AddRange(type.Properties.Where(IsVisible).OrderBy(x => Descriptor.Create(x, true)).Select(x => new PropertyDocumentation(x, doc)));

            doc.LoadSummary(summary);
            Add(doc);
        }

        /// <summary>
        /// Should the given field be documented?
        /// </summary>
        private static bool IsVisible(FieldDefinition field)
        {
            if (!(field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly))
                return false;
            if (field.DeclaringType.IsEnum)
            {
                return field.IsStatic;
            }
            return true;
        }

        /// <summary>
        /// Should the given method be documented?
        /// </summary>
        private static bool IsVisible(MethodDefinition method)
        {
            return (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }

        /// <summary>
        /// Should the given method be documented?
        /// </summary>
        private static bool IsVisibleAndNormal(MethodDefinition method)
        {
            return IsVisible(method) && (method.SemanticsAttributes == MethodSemanticsAttributes.None);
        }

        /// <summary>
        /// Should the given event be documented?
        /// </summary>
        private static bool IsVisible(EventDefinition evt)
        {
            var method = evt.AddMethod;
            if ((method != null) && IsVisible(method))
                return true;
            method = evt.RemoveMethod;
            if ((method != null) && IsVisible(method))
                return true;
            return false;
        }

        /// <summary>
        /// Should the given property be documented?
        /// </summary>
        private static bool IsVisible(PropertyDefinition prop)
        {
            var method = prop.GetMethod;
            if ((method != null) && IsVisible(method))
                return true;
            method = prop.SetMethod;
            if ((method != null) && IsVisible(method))
                return true;
            return false;
        }

        /// <summary>
        /// Generate XML documentation
        /// </summary>
        internal void Generate(List<FrameworkDocumentationSet> frameworks, string outputFolder)
        {
            var root = new XElement("api");
            var doc = new XDocument(root);

            foreach (var ns in Namespaces)
            {
                // Add namespace element
                root.Add(ns.Generate(frameworks));

                // Add type elements
                root.Add(ns.Types.OrderBy(x => CecilFormat.GetTypeName(x.Type)).Select(x => x.Generate(frameworks)));
            }

            // Save root
            doc.Save(Path.Combine(outputFolder, "api.xml"));
        }
    }
}