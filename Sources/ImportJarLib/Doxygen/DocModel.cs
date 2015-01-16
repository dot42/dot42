using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Dot42.Utility;

namespace Dot42.ImportJarLib.Doxygen
{
    public class DocModel
    {
        private static readonly Dictionary<string, string> ShortTypeNames = new Dictionary<string, string> {
            { "int", "System.Int32" }
        };
        private readonly Dictionary<string, DocClass> classesById = new Dictionary<string, DocClass>();
        private readonly Dictionary<string, DocClass> classesByName = new Dictionary<string, DocClass>();

        /// <summary>
        /// Try to get a class by it's id.
        /// </summary>
        public bool TryGetClassById(string id, out DocClass @class)
        {
            return classesById.TryGetValue(id, out @class);
        }

        /// <summary>
        /// Try to get a class by it's name.
        /// </summary>
        public bool TryGetClassByName(string name, out DocClass @class)
        {
            return classesByName.TryGetValue(name, out @class);
        }

        /// <summary>
        /// Gets all classes.
        /// </summary>
        public IEnumerable<DocClass>  Classes
        {
            get { return classesById.Values; }
        }

        /// <summary>
        /// Load XML data
        /// </summary>
        public bool Load(string folder)
        {
            if (string.IsNullOrEmpty(folder))
                return false;
            if (!Directory.Exists(folder))
                throw new ArgumentException("Folder does not exist", "folder");

            // Load all files
            using (Profiler.Profile(x => Console.WriteLine("Loaded doxygen files in {0}ms", x.TotalMilliseconds)))
            {
                foreach (var path in Directory.GetFiles(folder, "*.xml"))
                {
                    LoadFile(path);
                }
            }

            // Link
            foreach (var @class in classesById.Values)
            {
                @class.Link(this);
            }

            return true;
        }

        /// <summary>
        /// Load a single XML file
        /// </summary>
        /// <param name="path"></param>
        //[DebuggerNonUserCode]
        private void LoadFile(string path)
        {
            try
            {
                var doc = TryLoadDocument(path);
                if (doc == null)
                    return;

                var compounddef = doc.Root.Element("compounddef");
                if (compounddef == null)
                {
                    // We will never inspect this file, remove it
                    File.Delete(path);
                    return;
                }

                var kind = compounddef.Attribute("kind");
                if (kind == null)
                {
                    // We will never inspect this file, remove it
                    File.Delete(path);
                    return;
                }

                switch (kind.Value)
                {
                    case "class":
                        LoadClass(compounddef);
                        break;
                    case "interface":
                        LoadClass(compounddef);
                        break;
                    default:
                        // We will never inspect this file, remove it
                        //File.Delete(path);
                        break;
                }
            }
            catch
            {
                // Ignore
            }
        }

        /// <summary>
        /// Load the given class
        /// </summary>
        private void LoadClass(XElement compounddef)
        {
            var id = compounddef.GetAttribute("id");
            var name = compounddef.GetElementValue("compoundname");

            var xmlClass = new DocClass(name) { Id = id, Description = GetDescription(compounddef) };
            classesById.Add(xmlClass.Id, xmlClass);
            classesByName.Add(xmlClass.Name, xmlClass);

            var index = xmlClass.Name.IndexOf('<');
            if (index > 0)
            {
                // Generic class
                var elementName = xmlClass.Name.Substring(0, index);
                if (!classesByName.ContainsKey(elementName))
                {
                    classesByName.Add(elementName, xmlClass);
                }
            }

            foreach (var memberdef in compounddef.Descendants("memberdef").Where(x => x.GetAttribute("kind") == "function"))
            {
                LoadMethod(xmlClass, memberdef);
            }

            foreach (var memberdef in compounddef.Descendants("memberdef").Where(x => x.GetAttribute("kind") == "variable"))
            {
                LoadField(xmlClass, memberdef);
            }
        }

        /// <summary>
        /// Load the given method
        /// </summary>
        private void LoadMethod(DocClass @class, XElement memberdef)
        {
            var name = memberdef.GetElementValue("name");

            var xmlMethod = new DocMethod(name) { Description = GetDescription(memberdef) };
            @class.Methods.Add(xmlMethod);

            // Load parameter
            var ns = @class.Namespace;
            foreach (var p in memberdef.Elements("param"))
            {
                LoadParameter(xmlMethod, p, ns);
            }
        }

        /// <summary>
        /// Load the given field
        /// </summary>
        private void LoadField(DocClass @class, XElement memberdef)
        {
            var name = memberdef.GetElementValue("name");

            var xmlField = new DocField(name) { Description = GetDescription(memberdef) };
            @class.Fields.Add(xmlField);
        }

        /// <summary>
        /// Load the given parameter
        /// </summary>
        private void LoadParameter(DocMethod method, XElement param, string @namespace)
        {
            var name = param.GetElementValue("declname");
            var type = LoadType(param.Element("type"), @namespace);

            var p = new DocParameter(name) { ParameterType = type };
            method.Parameters.Add(p);
        }

        /// <summary>
        /// Load a &lt;type&gt; element.
        /// </summary>
        private static DocTypeRef LoadType(XElement type, string @namespace)
        {
            if (type == null)
                return null;
            foreach (var child in type.Nodes())
            {
                if (child.NodeType == XmlNodeType.Element)
                {
                    var childE = (XElement) child;
                    if (childE.Name.LocalName == "ref")
                    {
                        return new DocTypeRef(null, childE.GetAttribute("refid"));
                    }
                }
            }
            if (!type.HasElements)
            {
                // Use name
                var name = type.Value;
                if (!string.IsNullOrEmpty(name))
                {
                    var fullName = name;
                    string tmp;
                    if (ShortTypeNames.TryGetValue(name, out tmp))
                    {
                        fullName = tmp;
                    }
                    else if (!name.Contains('.'))
                    {
                        fullName = @namespace + "." + name;
                    }
                    return new DocTypeRef(fullName, null);
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the description of the element (if any).
        /// </summary>
        private DocDescription GetDescription(XElement e)
        {
            var child = e.Element("detaileddescription");
            return (child != null) ? new DocDescription(child, this) : null;
        }

        /// <summary>
        /// Load the given xML document.
        /// </summary>
        [DebuggerNonUserCode]
        private static XDocument TryLoadDocument(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Ignore;
                    settings.XmlResolver = null;
                    settings.ValidationType = ValidationType.None;
                    settings.ValidationFlags = XmlSchemaValidationFlags.None;
                    using (var reader = XmlReader.Create(stream, settings))
                    {
                        {
                            return XDocument.Load(reader);
                        }
                    }
                }
            }
            catch
            {
                // Ignore
                return null;
            }            
        }
    }
}
