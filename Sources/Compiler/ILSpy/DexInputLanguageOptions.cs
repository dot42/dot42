using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Dot42.CompilerLib.Target;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    public class StopMenuCommandAttribute : ExportMainMenuCommandAttribute
    {
        public StopAstConversion StopCode { get; set; }

        public StopMenuCommandAttribute(StopAstConversion code)
        {
            Menu = "_Dex Input Language";
            MenuCategory = "Stop";
            Header = code == StopAstConversion.None
                ? "Full Pocessing" : "Stop after " + code.ToString().Replace("After", "");
            MenuOrder = (int) code;
            StopCode = code;
        }
    }

    public class StopMenuCommand : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            var attr = GetAttribute(this);
            if (attr == null) return;

            var stopCode = attr.StopCode;

            DexInputLanguage.StopConversion = stopCode;
            
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

        private static StopMenuCommandAttribute GetAttribute(ICommand command)
        {
            var attr = command.GetType().GetCustomAttributes(typeof (StopMenuCommandAttribute), true)
                              .Cast<StopMenuCommandAttribute>()
                              .FirstOrDefault();
            return attr;
        }
    }

    [StopMenuCommand(StopAstConversion.None)]
    public class DexInputLanguageCommand01 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterILConversion)]
    public class DexInputLanguageCommand02 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterOptimizing)]
    public class DexInputLanguageCommand03 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterIntPtrConverter)]
    public class DexInputLanguageCommand04 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterTypeOfConverter)]
    public class DexInputLanguageCommand05 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterBranchOptimizer)]
    public class DexInputLanguageCommand06 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterCompoundAssignmentConverter)]
    public class DexInputLanguageCommand07 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterInterlockedConverter)]
    public class DexInputLanguageCommand08 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterByReferenceParamConverter)]
    public class DexInputLanguageCommand09 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterCompareUnorderedConverter)]
    public class DexInputLanguageCommand10 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterEnumConverter)]
    public class DexInputLanguageCommand11 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterEnumOptimizer)]
    public class DexInputLanguageCommand12 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterNullableConverter)]
    public class DexInputLanguageCommand13 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterPrimitiveAddressOfConverter)]
    public class DexInputLanguageCommand14 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterStructCallConverter)]
    public class DexInputLanguageCommand15 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterInitializeStructVariablesConverter)]
    public class DexInputLanguageCommand16 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterDelegateConverter)]
    public class DexInputLanguageCommand17 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterLdcWideConverter)]
    public class DexInputLanguageCommand18 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterLdLocWithConversionConverter)]
    public class DexInputLanguageCommand19 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterConvertAfterLoadConversionConverter)]
    public class DexInputLanguageCommand20 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterConvertBeforeStoreConversionConverter)]
    public class DexInputLanguageCommand21 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterCleanupConverter)]
    public class DexInputLanguageCommand22 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterGenericsConverter)]
    public class DexInputLanguageCommand23 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterCastConverter)]
    public class DexInputLanguageCommand24 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterGenericInstanceConverter)]
    public class DexInputLanguageCommand25 : StopMenuCommand { }

    [StopMenuCommand(StopAstConversion.AfterBranchOptimizer2)]
    public class DexInputLanguageCommand26 : StopMenuCommand { }

}
