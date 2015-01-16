namespace Dot42.DebuggerLib
{
    public class SlotRequest
    {
        public readonly int Slot;
        public readonly Jdwp.Tag Tag;

        public SlotRequest(int slot, Jdwp.Tag tag)
        {
            Slot = slot;
            Tag = tag;
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", Slot, Tag);
        }
    }
}
