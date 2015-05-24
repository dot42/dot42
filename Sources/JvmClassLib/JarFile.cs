using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace Dot42.JvmClassLib
{
    public class JarFile : IDisposable, IClassLoader
    {
        private readonly ZipFile zipFile;
        private Dictionary<string, ZipEntry> zipEntries;
        private readonly Dictionary<string, ClassFile> loadedClasses = new Dictionary<string, ClassFile>();
        private readonly string fileName;
        private readonly IClassLoader nextClassLoader;
        private List<string> classNames;
        private List<string> classFileNames;
        private List<string> packages;
        

        /// <summary>
        /// Open a jar file from the given stream
        /// </summary>
        public JarFile(Stream stream, string fileName, IClassLoader nextClassLoader)
        {
            this.fileName = fileName;
            this.nextClassLoader = nextClassLoader;
            zipFile = new ZipFile(stream);
        }

        /// <summary>
        /// Gets the filename of this jar file.
        /// </summary>
        public string Scope
        {
            get { return fileName; }
        }

        /// <summary>
        /// Gets the filenames of all class files
        /// </summary>
        public IEnumerable<string> ClassNames
        {
            get
            {
                if(classNames != null)
                    return classNames;

                classNames = ClassFileNames.Where(x => !x.Contains("$"))
                                           .Select(x => x.Substring(0, x.Length - ".class".Length))
                                           .ToList();
                return classNames;
            }
        }

        /// <summary>
        /// Gets the filenames of all class files
        /// </summary>
        public IEnumerable<string> ClassFileNames
        {
            get
            {
                if(classFileNames != null)
                    return classFileNames;
                classFileNames = (from ZipEntry entry in zipFile 
                                  where entry.Name.EndsWith(".class") 
                                  select entry.Name)
                                 .ToList();
                return classFileNames;
            }
        }

        /// <summary>
        /// Gets all package names found in this loader.
        /// </summary>
        public IEnumerable<string> Packages
        {
            get
            {
                if(packages != null)
                    return packages;
                
                var fileNames = ClassFileNames;
                return packages = fileNames.Select(ClassName.GetPackage)
                                           .Distinct()
                                           .ToList();
            }
        }

        /// <summary>
        /// Gets the filenames of all files in the jar
        /// </summary>
        public IEnumerable<string> FileNames
        {
            get { return from ZipEntry entry in zipFile where entry.IsFile select entry.Name; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            ((IDisposable)zipFile).Dispose();
        }

        /// <summary>
        /// Load a class with the given name
        /// </summary>
        public bool TryLoadClass(string className, out ClassFile result)
        {
            if (loadedClasses.TryGetValue(className, out result))
                return true;
            result = OpenClass(className + ".class");
            if (result != null) return true;
            if (nextClassLoader != null) 
                return nextClassLoader.TryLoadClass(className, out result);
            return false;
        }

        /// <summary>
        /// Load a class with the given name
        /// </summary>
        public ClassFile LoadClass(string className)
        {
            ClassFile result;
            if (TryLoadClass(className, out result))
                return result;
            throw new ResolveException(className);
        }

        /// <summary>
        /// Load an entry with the given filename as raw stream.
        /// </summary>
        public byte[] GetResource(string fileName)
        {
            var entry = zipFile.GetEntry(fileName);
            if (entry == null)
                throw new ArgumentException(string.Format("Resource {0} not found", fileName));
            var data = new byte[entry.Size];
            using (var stream = zipFile.GetInputStream(entry))
            {
                stream.Read(data, 0, data.Length);
            }
            return data;
        }

        /// <summary>
        /// Load the class with the given file name
        /// </summary>
        public ClassFile OpenClass(string fileName)
        {
            if (zipEntries == null)
            {
                var dic = new Dictionary<string, ZipEntry>(StringComparer.InvariantCultureIgnoreCase);
                foreach (ZipEntry zipEntry in zipFile)
                    dic.Add(zipEntry.Name, zipEntry);
                zipEntries = dic;
            }

            ZipEntry entry;

            if (!zipEntries.TryGetValue(fileName, out entry))
                return null;

            using (var stream = zipFile.GetInputStream(entry))
            {
                var result = new ClassFile(stream, this);
                loadedClasses[result.ClassName] = result;
                return result;
            }
        }
    }
}
