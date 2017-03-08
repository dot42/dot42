using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Pdb;
using TallComponents.Dot42;

namespace Dot42.LoaderLib.DotNet
{
    /// <summary>
    /// Symbol provider that does not fail when symbols are not found.
    /// </summary>
    public class SafeSymbolReaderProvider : ISymbolReaderProvider 
    {
        private readonly PdbReaderProvider _baseProvider = new PdbReaderProvider();

        public ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
        {
            try
            {
                var pdbPath = SymbolHelper.GetPdbFileName(fileName);
                if (!File.Exists(pdbPath))
                    return new NullReader();
                return _baseProvider.GetSymbolReader(module, fileName);
            }
            catch (Exception)
            {
                return new NullReader();
            }
        }

        public ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream)
        {
            try
            {
                return _baseProvider.GetSymbolReader(module, symbolStream);
            }
            catch (Exception)
            {
                return new NullReader();
            }
        }

        private sealed class NullReader : ISymbolReader
        {
            public void Dispose()
            {
            }

            public bool ProcessDebugHeader(ImageDebugDirectory directory, byte[] header)
            {
                return true;
            }

            public MethodDebugInformation Read(MethodDefinition method) { return null; }
        }
    }
}
