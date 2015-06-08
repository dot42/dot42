using System;
using System.Collections.Generic;
using System.IO;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.Java;
using Dot42.DexLib;
using Dot42.DexLib.Instructions;
using Dot42.JvmClassLib;
using Dot42.Utility;
using MethodDefinition = Dot42.DexLib.MethodDefinition;

namespace Dot42.CompilerLib.Target.Dx
{
    /// <summary>
    /// Compiles .jar based method bodies to dex using the 'dx' tool from Android SDK Tools.
    /// 
    /// The implementation has a build in compiler-cache that will speed up incremental builds.
    /// </summary>
    public class DxClassfileMethodBodyCompiler
    {
        private readonly string _temporaryDirectory;
        private readonly bool _generateDebugSymbols;
        private readonly Dictionary<string, DexLookup> _dexes = new Dictionary<string, DexLookup>();



        public DxClassfileMethodBodyCompiler(string temporaryDirectory, bool generateDebugSymbols)
        {
            _temporaryDirectory = temporaryDirectory;
            _generateDebugSymbols = generateDebugSymbols;
        }

        public MethodBody GetMethodBody(MethodDefinition targetMethod, XMethodDefinition sourceMethod)
        {
            var javaType = (XBuilder.JavaTypeDefinition) sourceMethod.DeclaringType;
            var className = javaType.ClassFile.ClassName;
            var source = javaType.ClassFile.Loader.TryGetClassSource(className);

            if (source == null)
                return null;

            DexLookup dex;
            var dexFileName = GetOrCreateDexFilename(source);

            lock (_dexes)
            {
                if (!_dexes.TryGetValue(dexFileName, out dex))
                {
                    dex = new DexLookup(DexLib.Dex.Read(dexFileName));
                    _dexes[dexFileName] = dex;
                }
            }

            var methodDef = dex.GetMethod(targetMethod.Owner.Fullname, targetMethod.Name, targetMethod.Prototype.ToSignature());
            return methodDef == null ? null : methodDef.Body;
        }

        private string GetOrCreateDexFilename(ClassSource source)
        {
            string hash = GetHash(source);

            string jarFileName;

            if (source.IsDiskFile)
            {
                jarFileName = source.FileName;
            }
            else
            {
                string baseName = Path.GetFileName(source.FileName);
                jarFileName = Path.Combine(_temporaryDirectory, baseName + "-" + hash + ".jar");

                if (!File.Exists(jarFileName))
                {
                    Directory.CreateDirectory(_temporaryDirectory);
                    File.WriteAllBytes(jarFileName, source.JarData);
                }
            }

            var dexFileName = Path.Combine(_temporaryDirectory, Path.GetFileNameWithoutExtension(jarFileName) + "." + hash + ".dex");

            if (!File.Exists(dexFileName))
            {
                Directory.CreateDirectory(_temporaryDirectory);
                DxInvoker.CompileToDex(jarFileName, dexFileName, _generateDebugSymbols);
            }

            return dexFileName;
        }

        

        private string GetHash(ClassSource source)
        {
            if (source.JarStreamHash != null)
                return source.JarStreamHash;

            if (source.JarData != null)
                return JarReferenceHash.ComputeJarReferenceHash(source.JarData);

            return JarReferenceHash.ComputeJarReferenceHash(source.FileName);
        }
    }
}
