using System;

namespace Dot42.CompilerLib.Ast
{
    /// <summary>
    /// Jump target of lookup-switch
    /// </summary>
    public sealed class AstLabelKeyPair : Tuple<AstLabel, int>
    {
        public AstLabelKeyPair(AstLabel label, int key)
            : base(label, key)
        {
        }

        public AstLabel Label { get { return Item1; } }
        public int Key { get { return Item2; } }
    }
}