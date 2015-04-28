using Mono.Cecil.Cil;

namespace Dot42.CompilerLib.Ast
{
    public class SequencePointWrapper : ISourceLocation
    {
        internal const int SpecialLine = 0xFEEFEE;
        private readonly SequencePoint sequencePoint;

        /// <summary>
        /// Default ctor
        /// </summary>
        private SequencePointWrapper(SequencePoint sequencePoint)
        {
            this.sequencePoint = sequencePoint;
        }

        public string Document { get { return (sequencePoint.Document != null) ? sequencePoint.Document.Url : null; } }
        public int StartLine { get { return sequencePoint.StartLine; } }
        public int StartColumn { get { return sequencePoint.StartColumn; } }
        public int EndLine { get { return sequencePoint.EndLine; } }
        public int EndColumn { get { return sequencePoint.EndColumn; } }
        public bool IsSpecial { get { return (sequencePoint.StartLine == SpecialLine); } }

        /// <summary>
        /// Wrap the given sequence point which cn be null.
        /// </summary>
        public static ISourceLocation Wrap(SequencePoint sequencePoint)
        {
            return (sequencePoint != null) ? new SequencePointWrapper(sequencePoint) : null;
        }

        #region Equality
        protected bool Equals(SequencePointWrapper other)
        {
            return Equals(sequencePoint.Document, other.sequencePoint.Document)
                   && Equals(sequencePoint.StartLine, other.sequencePoint.StartLine)
                   && Equals(sequencePoint.StartColumn, other.sequencePoint.StartColumn)
                   && Equals(sequencePoint.EndLine, other.sequencePoint.EndLine)
                   && Equals(sequencePoint.EndColumn, other.sequencePoint.EndColumn);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SequencePointWrapper) obj);
        }

       
        #endregion
    }

    public static class SequencePointExtensions
    {
        public static bool IsSpecial(this SequencePoint sp)
        {
            return (sp != null) && (sp.StartLine == SequencePointWrapper.SpecialLine);
        }
    }
}
