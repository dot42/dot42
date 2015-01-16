using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Dot42.BarDeployLib;

namespace Dot42.DeviceLib.UI
{
    public static class BlackBerryNotifications
    {
        private const int HWND_BROADCAST = 0xFFFF;
        private const string updateNotificationMessageGuid = "F095A8F8-1F16-4534-AC60-19541E50A4D3";
        private static readonly uint updateNotificationMessage;
        private static readonly int myId;

        /// <summary>
        /// Static ctor
        /// </summary>
        static BlackBerryNotifications()
        {
            myId = new Random(Environment.TickCount).Next();
            updateNotificationMessage = RegisterWindowMessage(updateNotificationMessageGuid);
        }

        /// <summary>
        /// Fire an update notification.
        /// </summary>
        internal static void PostUpdateNotification()
        {
            var rc = PostMessage((IntPtr)HWND_BROADCAST, updateNotificationMessage, new IntPtr(myId), IntPtr.Zero);
        }

        /// <summary>
        /// Filter update notifications and handle them.
        /// </summary>
        public static void FilterUpdateNotification(ref Message m)
        {
            if (m.Msg == updateNotificationMessage)
            {
                // Incoming message
                if (m.WParam.ToInt32() != myId)
                {
                    BlackBerryDevices.Instance.Reload();
                }
            }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32")]
        static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam);
    }
}
