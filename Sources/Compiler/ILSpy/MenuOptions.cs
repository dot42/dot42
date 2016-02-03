using System;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    [ExportMainMenuCommand(Menu = "_Dot42", Header = "Toggle show full names")]
    public class Option2 : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            DexInputLanguage.ShowFullNames = !DexInputLanguage.ShowFullNames;
            DexLanguage.ShowFullNames = !DexLanguage.ShowFullNames;
            MainWindow.Instance.RefreshDecompiledView();
        }
    }

    [ExportMainMenuCommand(Menu = "_Dot42", Header = "Dex Input/RL: Toggle has sequence points")]
    public class Option1 : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            DexInputLanguage.ShowHasSeqPoint = !DexInputLanguage.ShowHasSeqPoint;
            RLLanguage.ShowHasSeqPoint = !RLLanguage.ShowHasSeqPoint;
            MainWindow.Instance.RefreshDecompiledView();
        }
    }

    [ExportMainMenuCommand(Menu = "_Dot42", Header = "Dex Input: Toggle break expressions")]
    public class OptionDexInput1 : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            DexInputLanguage.BreakExpressionLines = !DexInputLanguage.BreakExpressionLines;
            MainWindow.Instance.RefreshDecompiledView();
        }
    }

    [ExportMainMenuCommand(Menu = "_Dot42", Header = "Dex: Toggle debug operand types")]
    public class Option4 : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            DexLanguage.DebugOperandTypes = !DexLanguage.DebugOperandTypes;
            MainWindow.Instance.RefreshDecompiledView();
        }
    }

    [ExportMainMenuCommand(Menu = "_Dot42", Header = "Toggle generate set next instruction")]
    public class Option3 : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            CompiledLanguage.GenerateSetNextInstructionCode = !CompiledLanguage.GenerateSetNextInstructionCode;
            MainWindow.Instance.RefreshDecompiledView();
        }
    }


}
