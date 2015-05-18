namespace Dot42.DexLib.OpcodeHelp
{
    public class DalvikOpcodeHelp
    {
        public readonly string OpAndFormat;
        public readonly string MnemonicAndSyntax;
        public readonly string Arguments;
        public readonly string Description;

        public string Syntax { get { return MnemonicAndSyntax.Split('\n')[0]; } }

        public DalvikOpcodeHelp(string opAndFormat, string mnemonicAndSyntax, string arguments, string description)
        {
            OpAndFormat = opAndFormat;
            MnemonicAndSyntax = mnemonicAndSyntax;
            Arguments = arguments;
            Description = description;
        }
    }
}
