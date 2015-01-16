using System.Collections.Generic;
using System.IO;
using Dot42.DexLib.IO;

namespace Dot42.DexLib
{
    public sealed class Dex
    {
        public const char NestedClassSeparator = '$';

        private readonly List<ClassDefinition> classes = new List<ClassDefinition>();

        /// <summary>
        /// Gets all top level classes.
        /// </summary>
        internal List<ClassDefinition> Classes { get { return classes; } }

        public static Dex Read(string filename)
        {
            return Read(filename, true);
        }

        public void Write(string filename)
        {
            Write(filename, true);
        }

        public void AddClass(ClassDefinition classDef)
        {
            Classes.Add(classDef);
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

        public IEnumerable<ClassDefinition> GetClasses()
        {
            return Classes;
        }

        public ClassDefinition GetClass(string fullname)
        {
            return GetClass(fullname, Classes);
        }

        internal static ClassDefinition GetClass(string fullname, List<ClassDefinition> container)
        {
            foreach (ClassDefinition item in container)
            {
                if (fullname.Equals(item.Fullname))
                    return item;

                var inner = GetClass(fullname, item.InnerClasses);
                if (inner != null)
                    return inner;
            }
            return null;
        }
    }
}