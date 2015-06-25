using System.Linq;
using System.Windows.Input;
using Dot42.CompilerLib.Target;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    public class StopRLMenuCommandAttribute : ExportMainMenuCommandAttribute
    {
        public int StopAfter { get; set; }

        public StopRLMenuCommandAttribute(int iterations)
        {
            Menu = "Dot42 RL Output";
            MenuCategory = "Stop";
            Header = iterations == -1 
                ? "Full Pocessing" 
                : iterations == 0
                ? "No transformations" 
                : "Stop after " + iterations.ToString() + " transformation steps";
            MenuOrder = iterations;
            StopAfter = iterations;
        }
    }

    public class StopRLMenuCommand : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            var attr = GetAttribute(this);
            if (attr == null) return;

            var stopCode = attr.StopAfter;

            RLLanguage.StopOptimizationAfter = stopCode;
            
            MainWindow.Instance.RefreshDecompiledView();
            //UpdateCheckedState(stopCode);
        }

        //private static void UpdateCheckedState(StopAstConversion stopCode)
        //{
        //    // this does not work.
        //    foreach (MenuItem item in MainWindow.Instance.GetMainMenuItems())
        //    {
        //        if (!(item.Command is StopMenuCommand))
        //            continue;
        //        var attr = GetAttribute(item.Command);

        //        if (attr.StopCode == stopCode)
        //        {
        //            item.IsCheckable = true;
        //            item.IsChecked = true;
        //        }
        //        else
        //        {
        //            item.IsChecked = false;
        //        }
        //    }
        //}

        private static StopRLMenuCommandAttribute GetAttribute(ICommand command)
        {
            var attr = command.GetType().GetCustomAttributes(typeof(StopRLMenuCommandAttribute), true)
                              .Cast<StopRLMenuCommandAttribute>()
                              .FirstOrDefault();
            return attr;
        }
    }

    [StopRLMenuCommand(-1)]
    public class RLLanguageCommandFull : StopRLMenuCommand { }
    [StopRLMenuCommand(0)]
    public class RLLanguageCommand00 : StopRLMenuCommand { }
    [StopRLMenuCommand(1)]
    public class RLLanguageCommand01 : StopRLMenuCommand { }
    [StopRLMenuCommand(2)]
    public class RLLanguageCommand02 : StopRLMenuCommand { }
    [StopRLMenuCommand(3)]
    public class RLLanguageCommand03 : StopRLMenuCommand { }
    [StopRLMenuCommand(4)]
    public class RLLanguageCommand04 : StopRLMenuCommand { }
    [StopRLMenuCommand(5)]
    public class RLLanguageCommand05 : StopRLMenuCommand { }
    [StopRLMenuCommand(6)]
    public class RLLanguageCommand06 : StopRLMenuCommand { }
    [StopRLMenuCommand(7)]
    public class RLLanguageCommand07 : StopRLMenuCommand { }
    [StopRLMenuCommand(8)]
    public class RLLanguageCommand08 : StopRLMenuCommand { }
    [StopRLMenuCommand(9)]
    public class RLLanguageCommand09 : StopRLMenuCommand { }
    [StopRLMenuCommand(10)]
    public class RLLanguageCommand10 : StopRLMenuCommand { }
    [StopRLMenuCommand(11)]
    public class RLLanguageCommand11 : StopRLMenuCommand { }
    [StopRLMenuCommand(12)]
    public class RLLanguageCommand12 : StopRLMenuCommand { }
    [StopRLMenuCommand(13)]
    public class RLLanguageCommand13 : StopRLMenuCommand { }
    [StopRLMenuCommand(14)]
    public class RLLanguageCommand14 : StopRLMenuCommand { }
    [StopRLMenuCommand(15)]
    public class RLLangugeCommand15 : StopRLMenuCommand { }
    [StopRLMenuCommand(16)]
    public class RLLanguageCommand16 : StopRLMenuCommand { }
    [StopRLMenuCommand(17)]
    public class RLLanguageCommand17 : StopRLMenuCommand { }
    [StopRLMenuCommand(18)]
    public class RLLanguageCommand18 : StopRLMenuCommand { }
    [StopRLMenuCommand(19)]
    public class RLLanguageCommand19 : StopRLMenuCommand { }
    [StopRLMenuCommand(20)]
    public class RLLanguageCommand20 : StopRLMenuCommand { }
    [StopRLMenuCommand(21)]
    public class RLLanguageCommand21 : StopRLMenuCommand { }
    [StopRLMenuCommand(22)]
    public class RLLanguageCommand22 : StopRLMenuCommand { }
    [StopRLMenuCommand(23)]
    public class RLLanguageCommand23 : StopRLMenuCommand { }

}
