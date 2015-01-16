using System.Drawing;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro.ColorTables;
using Dot42.Graphics;

namespace Dot42.Shared.UI
{
    public static class UIExtensions
    {
        /// <summary>
        /// Initialize the given style manager
        /// </summary>
        public static void Setup(this StyleManager styleManager)
        {
            styleManager.ManagerStyle = eStyle.Office2010Blue;
            styleManager.MetroColorParameters = new MetroColorGeneratorParameters(Color.WhiteSmoke, BackgroundColors.DMetroColor);
        }
    }
}
