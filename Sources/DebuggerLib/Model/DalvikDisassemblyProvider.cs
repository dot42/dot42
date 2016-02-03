using System;
using System.IO;
using System.Linq;
using Dot42.ApkLib;
using Dot42.DexLib;
using Dot42.JvmClassLib;
using Dot42.Mapping;
using TallComponents.Common.Extensions;

namespace Dot42.DebuggerLib.Model
{
    public class DalvikDisassemblyProvider 
    {
        private readonly DalvikProcess _process;
        
        private readonly string _apkPath;
        private readonly Lazy<DexLookup> _dex;
        private readonly MapFileLookup _mapFile;


        public DalvikDisassemblyProvider(DalvikProcess process, string apkPath, MapFileLookup mapFile)
        {
            _process = process;
            _apkPath = apkPath;
            _mapFile = mapFile;
            _dex = new Lazy<DexLookup>(LoadDex);
        }
       
        public MethodDisassembly GetFromLocation(DocumentLocation loc)
        {
            string className = null, methodName= null, methodSignature = null;

            if (loc == null)
                return null;

            if (loc.Method != null)
            {
                var type = loc.Method.DeclaringType.GetSignatureAsync().Await(DalvikProcess.VmTimeout);
                var typeDef = Descriptors.ParseClassType(type);
                className = typeDef.ClassName.Replace("/", ".");

                methodName = loc.Method.Name;
                methodSignature = loc.Method.Signature;
            }

            if (className == null && loc.TypeEntry != null)
                className = loc.TypeEntry.DexName;

            if (methodName == null && loc.MethodEntry != null)
            {
                methodName = loc.MethodEntry.DexName;
                methodSignature = loc.MethodEntry.Signature;
            }

            if (methodName == null || className == null)
                return null;

            var methodDef = _dex.Value.GetMethod(className, methodName, methodSignature);

            if (methodDef == null)
                return null;

            var typeEntry   = loc.TypeEntry   ?? _mapFile.GetTypeByDexName(className);
            var methodEntry = loc.MethodEntry ?? _mapFile.GetMethodByDexSignature(className, methodName, methodSignature);

            return new MethodDisassembly(methodDef, _mapFile, typeEntry, methodEntry);
        }

        private DexLookup LoadDex()
        {
            var apk = new ApkFile(_apkPath);
            var dex = apk.Load("classes.dex");
            return new DexLookup(Dex.Read(new MemoryStream(dex)));
        }
    }
}
