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
    public class SafeSymbolReaderProvider : PdbReaderProvider, ISymbolReaderProvider 
    {
        public new ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName)
        {
            try
            {
                var pdbPath = SymbolHelper.GetPdbFileName(fileName);
                if (!File.Exists(pdbPath))
                    return new NullReader();
                return base.GetSymbolReader(module, fileName);
            }
            catch (Exception)
            {
                return new NullReader();
            }
        }

        private sealed class NullReader : ISymbolReader
        {
            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
            }

            public bool ProcessDebugHeader(ImageDebugDirectory directory, byte[] header)
            {
                return true;
            }

            public void Read(MethodBody body, InstructionMapper mapper)
            {
            }

            public void Read(MethodSymbols symbols)
            {
            }
        }
    }
}
