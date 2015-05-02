using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Dot42.DexLib.IO;

namespace Dot42.DexLib
{
    /// <summary>
    /// Represents a .dex file. Contains methods for reading, writing, building .dex files
    /// as well as methods or quickly looking classes or methods up.
    /// </summary>
    public sealed class Dex
    {
        public const char NestedClassSeparator = '$';

        private readonly List<ClassDefinition> classes = new List<ClassDefinition>();
        private readonly Dictionary<string, ClassDefinition> innerClassesByName = new Dictionary<string, ClassDefinition>();
        private readonly Dictionary<string, ClassDefinition> classesByName = new Dictionary<string, ClassDefinition>();

        /// <summary>
        /// Gets all top level classes.
        /// </summary>
        public ICollection<ClassDefinition> Classes { get { return classes.AsReadOnly(); } }
        
        /// <summary>
        /// Returns all classes as the original list. The caller must not add or remove 
        /// items from the list, but may change the order of items.
        /// </summary>
        internal List<ClassDefinition> GetClassesList() { return classes;  }

        public static Dex Read(string filename)
        {
            return Read(filename, true);
        }

        public void Write(string filename)
        {
            Write(filename, true);
        }

        public static Dex Read(string filename, bool bufferize)
        {
            using (var filestream = new FileStream(filename, FileMode.Open))
            {
                Stream sourcestream = filestream;
                if (bufferize)
                {
                    var memorystream = new MemoryStream();
                    filestream.CopyTo(memorystream);
                    memorystream.Position = 0;
                    sourcestream = memorystream;
                }

                return Read(sourcestream);
            }
        }

        public static Dex Read(Stream stream)
        {
            using (var binaryReader = new BinaryReader(stream))
            {
                var reader = new DexReader();
                return reader.ReadFrom(binaryReader);
            }
        }

        public void Write(string filename, bool bufferize)
        {
            using (var filestream = new FileStream(filename, FileMode.Create))
            {
                Stream deststream = filestream;
                MemoryStream memorystream = null;

                if (bufferize)
                {
                    memorystream = new MemoryStream();
                    deststream = memorystream;
                }

                using (var binaryWriter = new BinaryWriter(deststream))
                {
                    var writer = new DexWriter(this);
                    writer.WriteTo(binaryWriter);

                    if (bufferize)
                    {
                        memorystream.Position = 0;
                        memorystream.CopyTo(filestream);
                    }
                }
            }
        }

        public void AddClass(ClassDefinition classDef)
        {
            classes.Add(classDef);
            classesByName.Add(classDef.Fullname, classDef);
            classDef.RegisterDex(this);
            RegisterInnerClasses(classDef);
        }

        public ClassDefinition GetClass(string fullname)
        {
            ClassDefinition classDef;

            if (classesByName.TryGetValue(fullname, out classDef))
                return classDef;

            if (innerClassesByName.TryGetValue(fullname, out classDef))
                return classDef;

            return null;
        }

        internal void AddRange(List<ClassDefinition> classes)
        {
            foreach (var @class in classes)
                AddClass(@class);
        }

        private void RegisterInnerClasses(ClassDefinition @class)
        {
            foreach (var inner in @class.InnerClasses)
            {
                innerClassesByName.Add(inner.Fullname, inner);
                inner.RegisterDex(this);
            }
        }

        internal void RegisterInnerClass(ClassDefinition owner, ClassDefinition inner)
        {
            innerClassesByName.Add(inner.Fullname, inner);
            inner.RegisterDex(this);
            RegisterInnerClasses(inner);
        }

        internal void OnNameChanged(ClassDefinition classDefinition, string previousFullName)
        {
            if (classesByName.ContainsKey(previousFullName))
            {
                classesByName.Remove(previousFullName);
                classesByName[classDefinition.Fullname] = classDefinition;
            }
            else if (innerClassesByName.ContainsKey(previousFullName))
            {
                innerClassesByName.Remove(previousFullName);
                innerClassesByName[classDefinition.Fullname] = classDefinition;
            }
            else
            {
                throw new Exception("not a member of this dex: " + classDefinition);
            }
        }

        public MethodDefinition GetMethod(ClassReference ownerRef, string methodName, Prototype prototype)
        {
            // TODO: check in profiler if we should optimize this as well.
            var owner = GetClass(ownerRef.Fullname);
            if (owner == null) return null;
            return owner.Methods.SingleOrDefault(x => (x.Name == methodName) && (x.Prototype.Equals(prototype)));
        }
    }
}