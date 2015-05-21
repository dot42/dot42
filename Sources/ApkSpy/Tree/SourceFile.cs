using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dot42.ApkLib;
using Dot42.CompilerLib.XModel;
using Dot42.DexLib;
using Dot42.FrameworkDefinitions;
using Dot42.JvmClassLib;
using Dot42.LoaderLib.DotNet;
using Dot42.LoaderLib.Java;
using Dot42.Mapping;
using ICSharpCode.SharpZipLib.Zip;
using Mono.Cecil;

namespace Dot42.ApkSpy.Tree
{
    public class SourceFile : IDisposable, ISpyContext
    {
        private readonly IApkFile apk;
        private readonly JarFile jar;
        private readonly ISpySettings settings;
        private readonly MapFileLookup mapFile;
        private readonly string singleFilePath;

#if DEBUG || ENABLE_SHOW_AST
        private readonly XModule module = new XModule();
        private readonly AssemblyDefinition assembly;
        private readonly AssemblyClassLoader classLoader;
#endif
        private readonly Dictionary<string, ClassFile> classFiles = new Dictionary<string, ClassFile>();

        private SourceFile(IApkFile apk, JarFile jar, ISpySettings settings, MapFileLookup mapFile, string singleFilePath = null)
        {
            this.apk = apk;
            this.jar = jar;
            this.settings = settings;
            this.mapFile = mapFile;
            this.singleFilePath = singleFilePath;

#if DEBUG  || ENABLE_SHOW_AST
            classLoader = new AssemblyClassLoader(module.OnClassLoaded);
            var modParams = new ModuleParameters
            {
                AssemblyResolver = new AssemblyResolver(new[] { Frameworks.Instance.FirstOrDefault().Folder }, classLoader, module.OnAssemblyLoaded),
                Kind = ModuleKind.Dll
            };
            assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("spy", Version.Parse("1.0.0.0")), "main", modParams);
            classLoader.LoadAssembly(assembly);

            var dot42Assembly = modParams.AssemblyResolver.Resolve("dot42");

            // Force loading of classes
            if (jar != null)
            {
                foreach (var fileName in jar.ClassFileNames)
                {
                    OpenClass(fileName);
                }
            }
#endif
        }

        /// <summary>
        /// Is the source an APK?
        /// </summary>
        public bool IsApk { get { return (apk != null); } }

        /// <summary>
        /// Is the source a JAR?
        /// </summary>
        public bool IsJar { get { return (jar != null); } }

        /// <summary>
        /// Is the source a single file?
        /// </summary>
        public bool IsSingleFile { get { return (singleFilePath != null); } }

        /// <summary>
        /// Gets the APK file
        /// </summary>
        public IApkFile Apk { get { return apk; } }

        /// <summary>
        /// Gets the JAR file
        /// </summary>
        public JarFile Jar { get { return jar; } }

        /// <summary>
        /// Gets the single file path
        /// </summary>
        public string SingleFilePath { get { return singleFilePath; } }

        /// <summary>
        /// Gets all filenames with the source.
        /// </summary>
        public IEnumerable<string> FileNames
        {
            get
            {
                if (apk != null)
                    return apk.FileNames;
                if (jar != null)
                    return jar.FileNames;
                if (singleFilePath != null)
                    return new[] { Path.GetFileName(singleFilePath) };
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (apk != null) apk.Dispose();
            if (jar != null) jar.Dispose();
        }

        /// <summary>
        /// Open a source file
        /// </summary>
        internal static SourceFile Open(string path, ISpySettings settings)
        {
            if (path.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
            {
                var mapFilePath = Path.ChangeExtension(path, ".d42map");
                var mapFile = File.Exists(mapFilePath) ? new MapFile(mapFilePath) : null;
                var mapLookup = mapFile == null ? null : new MapFileLookup(mapFile);
                return new SourceFile(new ApkFileOpenOnAccessOnly(path), null, settings, mapLookup);
            }
            if (path.EndsWith(".dex", StringComparison.OrdinalIgnoreCase))
            {
                var mapFilePath = Path.ChangeExtension(path, ".d42map");
                var mapFile = File.Exists(mapFilePath) ? new MapFile(mapFilePath) : null;
                var tempFileName = CreateApkOnTheFly(path);
                var mapLookup = mapFile == null ? null : new MapFileLookup(mapFile);

                return new SourceFile(new ApkFile(tempFileName), null, settings, mapLookup);
            }
            if (path.EndsWith(".jar", StringComparison.OrdinalIgnoreCase))
                return new SourceFile(null, new JarFile(File.OpenRead(path), path, null), settings, null);
            if (path.EndsWith(".bar", StringComparison.OrdinalIgnoreCase))
                return new SourceFile(null, new JarFile(File.OpenRead(path), path, null), settings, null);
            if (path.EndsWith(".p12", StringComparison.OrdinalIgnoreCase))
                return new SourceFile(null, null, settings, null, path);
            throw new ArgumentException("Unknown file type");
        }

        private static string CreateApkOnTheFly(string path)
        {
            var tempFileName = Path.GetTempFileName();
            var file = ZipFile.Create(tempFileName);
            file.BeginUpdate();
            file.Add(path, Path.GetFileName(path));
            file.CommitUpdate();
            file.Close();
            return tempFileName;
        }

        /// <summary>
        /// Open a class by it's filename
        /// </summary>
        public ClassFile OpenClass(string fileName)
        {
            ClassFile result;
            if (classFiles.TryGetValue(fileName, out result))
                return result;
            result = Jar.OpenClass(fileName);
            if (result == null)
                return null;
            classFiles[fileName] = result;
#if DEBUG || ENABLE_SHOW_AST
            module.OnClassLoaded(result);
#endif
            return result;
        }

        /// <summary>
        /// Gets the map file loaded with the current file.
        /// Can return null.
        /// </summary>
        MapFileLookup ISpyContext.MapFile { get { return mapFile; } }

#if DEBUG || ENABLE_SHOW_AST
        /// <summary>
        /// Show abstract syntax tree
        /// </summary>
        bool ISpySettings.ShowAst { get { return settings.ShowAst; } }

        /// <summary>
        /// Gets the XModule
        /// </summary>
        XModule ISpyContext.Module { get { return module; } }

        /// <summary>
        /// Gets the temp assembly.
        /// </summary>
        AssemblyDefinition ISpyContext.Assembly { get { return assembly; } }

        /// <summary>
        /// Gets the classloader
        /// </summary>
        AssemblyClassLoader ISpyContext.ClassLoader { get { return classLoader; } }
#endif
        public bool EnableBaksmali { get { return settings.EnableBaksmali; } }
        public string BaksmaliCommand { get { return settings.BaksmaliCommand; } }
        public string BaksmaliParameters { get { return settings.BaksmaliParameters; }}
        public bool ShowControlFlow { get { return settings.ShowControlFlow; } }
        public bool EmbedSourceCodePositions { get { return settings.EmbedSourceCodePositions; } }
        public bool EmbedSourceCode { get { return settings.EmbedSourceCode; } }
        public bool FullTypeNames { get { return settings.FullTypeNames; } }
    }
}
