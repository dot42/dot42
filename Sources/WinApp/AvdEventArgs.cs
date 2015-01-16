using System;
using Dot42.AvdLib;

namespace Dot42.Gui
{
    public class AvdEventArgs : EventArgs
    {
        private readonly Avd avd;

        public AvdEventArgs(Avd avd)
        {
            this.avd = avd;
        }

        public Avd Avd
        {
            get { return avd; }
        }
    }
}
