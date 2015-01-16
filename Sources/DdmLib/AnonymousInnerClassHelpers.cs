

namespace Dot42.DdmLib
{
    internal class MultiLineReceiverAnonymousInnerClassHelper : MultiLineReceiver
    {
        ////public delegate void SetTrimLineDelegate(bool trim);
        ////public SetTrimLineDelegate setTrimLineDelegateInstance;

        ////public override bool trimLine
        ////{
        ////    set { setTrimLineDelegateInstance(value); }
        ////}

        ////public delegate void DoneDelegate();
        ////public DoneDelegate doneDelegateInstance;

        ////public override void done()
        ////{
        ////    doneDelegateInstance();
        ////}

        public delegate void ProcessNewLinesDelegate(string[] lines);
        public ProcessNewLinesDelegate processNewLinesDelegateInstance;

        public override void processNewLines(string[] lines)
        {
            processNewLinesDelegateInstance(lines);
        }
    }
}