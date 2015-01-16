namespace Dot42.JvmClassLib
{
    public class LocalVariable
    {
        private readonly int startPC;
        private readonly int length;
        public readonly string Name;
        public readonly TypeReference VariableType;
        public readonly int Index;

        public LocalVariable(int startPc, int length, string name, TypeReference variableType, int index)
        {
            startPC = startPc;
            this.length = length;
            this.Name = name;
            this.VariableType = variableType;
            this.Index = index;
        }

        public bool IsValidForOffset(int offset)
        {
            return ((offset >= startPC) && (offset <= startPC + length));
        }

        public int StartPc { get { return startPC; } }
        public int EndPc { get { return startPC + length; } }
    }
}
