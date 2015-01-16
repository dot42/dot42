using System;

namespace Dot42.VStudio.XmlEditor
{
    public class EditingScopeEventArgs : EventArgs
    {
        public EditingScopeEventArgs(object editingScopeUserState)
        {
            EditingScopeUserState = editingScopeUserState;
        }

        public object EditingScopeUserState { get; private set; }
    }
}
