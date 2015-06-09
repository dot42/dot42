using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
        private readonly Dictionary<string, Task<DexLookup>> _dexes = new Dictionary<string, Task<DexLookup>>();



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

            DexLookup dex = GetOrCreateDex(source, waitForResult: true);
            var methodDef = dex.GetMethod(targetMethod.Owner.Fullname, targetMethod.Name, targetMethod.Prototype.ToSignature());
            return methodDef == null ? null : methodDef.Body;
        }

        public void PreloadJar(ClassSource source)
        {
            GetOrCreateDex(source, waitForResult: false);
        }

        private DexLookup GetOrCreateDex(ClassSource source, bool waitForResult)
        {
            string hash = GetHash(source);

            var jarFileName = GetJarFileName(source, hash);

            Task<DexLookup> dex;
            
            lock (_dexes)
            {
                if (_dexes.TryGetValue(jarFileName, out dex))
                {
                    if (!waitForResult)
                        return null;
                    return dex.Result;
                }
                    
                dex = new Task<DexLookup>(()=>
                {
                    var dexFileName = GetOrCreateDexFilename(source, hash);
                    return new DexLookup(DexLib.Dex.Read(dexFileName));
                }, TaskCreationOptions.LongRunning);

                _dexes[jarFileName] = dex;
            }

            dex.Start();

            if (waitForResult)
                return dex.Result;
            return null;
        }


        private string GetJarFileName(ClassSource source, string hash)
        {
            if (source.IsDiskFile)
            {
                return source.FileName;
            }

            string baseName = Path.GetFileName(source.FileName);
            return Path.Combine(_temporaryDirectory, baseName + "-" + hash + ".jar");
        }

        private string GetOrCreateDexFilename(ClassSource source, string hash)
        {
            string jarFileName = GetJarFileName(source, hash);
            
            if (!source.IsDiskFile && !File.Exists(jarFileName))
            {
                Directory.CreateDirectory(_temporaryDirectory);
                File.WriteAllBytes(jarFileName, source.JarData);
            }

            string baseName = Path.GetFileNameWithoutExtension(jarFileName);
            if (!baseName.Contains(hash)) baseName += "." + hash;
            var dexFileName = Path.Combine(_temporaryDirectory, baseName + ".dex");

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
