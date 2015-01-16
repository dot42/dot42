using System.Collections.Generic;

namespace Dot42.DexLib.Instructions
{
    public class DebugInfo
    {
        public DebugInfo(MethodBody body)
        {
            Owner = body;
            Parameters = new List<string>();
            DebugInstructions = new List<DebugInstruction>();
        }

        public List<string> Parameters { get; set; }
        public List<DebugInstruction> DebugInstructions { get; set; }
        public uint LineStart { get; set; }
        public MethodBody Owner { get; set; }
    }
}