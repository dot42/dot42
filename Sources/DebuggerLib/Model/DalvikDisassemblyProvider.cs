using System;
using System.IO;
using System.Text;
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
        private readonly Lazy<Dex> _dex;
        private readonly MapFile _mapFile;


        public DalvikDisassemblyProvider(DalvikProcess process, string apkPath, MapFile mapFile)
        {
            _process = process;
            _apkPath = apkPath;
            _mapFile = mapFile;
            _dex = new Lazy<Dex>(LoadDex);
        }
       
        public MethodDisassembly GetFromLocation(DocumentLocation loc)
        {
            string className = null, methodName= null;

            if (loc == null)
                return null;

            if (loc.TypeEntry != null)
                className = loc.TypeEntry.DexName;

            if (loc.Method != null)
            {
                if (className == null)
                {
                    var type = loc.Method.DeclaringType.GetSignatureAsync().Await(DalvikProcess.VmTimeout);
                    var typeDef = Descriptors.ParseClassType(type);
                    className = typeDef.ClassName.Replace("/", ".");
                }
                methodName = loc.Method.Name;
            }

            if (methodName == null && loc.MethodEntry != null)
            {
                methodName = loc.MethodEntry.DexName;
            }

            if (methodName == null || className == null)
                return null;

            var classDef = _dex.Value.GetClass(className);
            if (classDef == null)
                return null;

            var methodDef = classDef.GetMethod(methodName);

            if (methodDef == null)
                return null;

            var typeEntry = loc.TypeEntry ?? _mapFile.GetTypeById(classDef.MapFileId);
            var methodEntry = loc.MethodEntry ?? (typeEntry == null ? null : typeEntry.GetMethodById(methodDef.MapFileId));

            return new MethodDisassembly(typeEntry, classDef, methodEntry, methodDef, _mapFile);
        }

        private Dex LoadDex()
        {
            var apk = new ApkFile(_apkPath);
            var dex = apk.Load("classes.dex");
            return Dex.Read(new MemoryStream(dex));
        }
    }
}
