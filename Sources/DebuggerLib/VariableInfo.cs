namespace Dot42.DebuggerLib
{
    public class VariableInfo
    {
        public readonly long CodeIndex;
        public readonly string Name;
        public readonly string Signature;
        public readonly string GenericSignature;
        public readonly int Length;
        public readonly int Slot;

        public VariableInfo(long codeIndex, string name, string signature, string genericSignature, int length, int slot)
        {
            CodeIndex = codeIndex;
            Name = name;
            Signature = signature;
            GenericSignature = genericSignature;
            Length = length;
            Slot = slot;
        }

        public Jdwp.Tag Tag
        {
            get { return (Jdwp.Tag) Signature[0]; }
        }

        /// <summary>
        /// Is this variable valid at the given code offset?
        /// </summary>
        public bool IsValidAt(int offset)
        {
            return (offset >= CodeIndex) && (offset < CodeIndex + Length);
        }
    }
}
