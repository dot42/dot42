using System;
using System.Windows.Forms;

namespace Dot42.Gui
{
    internal interface IModalPanelForm
    {
        /// <summary>
        /// Close the current modal panel
        /// </summary>
        void CloseModalPanel();

        /// <summary>
        /// Show the given control as modal panel
        /// </summary>
        void ShowModalPanel<T>(T createControl, Action<T> initialize)
            where T : Control, IProgressControl;
    }
}
