using System.Linq;
using System.Windows.Input;
using Dot42.CompilerLib.Ast.Optimizer;
using Dot42.CompilerLib.Target;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    public class StopOptimizeMenuCommandAttribute : ExportMainMenuCommandAttribute
    {
        public AstOptimizationStep StopCode { get; set; }

        public StopOptimizeMenuCommandAttribute(AstOptimizationStep code)
        {
            Menu = "Dot42 DexInput/_Optimization";
            MenuCategory = "Stop";
            Header = code == AstOptimizationStep.None
                ? "Full Pocessing" : "Stop before " + code.ToString().Replace("Before", "");
            MenuOrder = (code == AstOptimizationStep.None)? -1 : (int) code;
            StopCode = code;
        }
    }

    public class StopOptimizeMenuCommand : SimpleCommand
    {
        public override void Execute(object parameter)
        {
            var attr = GetAttribute(this);
            if (attr == null) return;

            var stopCode = attr.StopCode;

            DexInputLanguage.StopConversion = StopAstConversion.AfterOptimizing;
            DexInputLanguage.StopOptimizing = stopCode;
            
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

        private static StopOptimizeMenuCommandAttribute GetAttribute(ICommand command)
        {
            var attr = command.GetType().GetCustomAttributes(typeof(StopOptimizeMenuCommandAttribute), true)
                              .Cast<StopOptimizeMenuCommandAttribute>()
                              .FirstOrDefault();
            return attr;
        }
    }

    [StopOptimizeMenuCommand(AstOptimizationStep.None)]
    public class DexInputOptimizeCommand00 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.RemoveRedundantCode)]
    public class DexInputOptimizeCommand02 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.ReduceBranchInstructionSet)]
    public class DexInputOptimizeCommand03 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.InlineVariables)]
    public class DexInputOptimizeCommand04 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.CopyPropagation)]
    public class DexInputOptimizeCommand05 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.SplitToMovableBlocks)]
    public class DexInputOptimizeCommand06 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.TypeInference)]
    public class DexInputOptimizeCommand07 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.SimplifyNullCoalescing)]
    public class DexInputOptimizeCommand08 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.JoinBasicBlocks)]
    public class DexInputOptimizeCommand09 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.SimplifyShiftOperators)]
    public class DexInputOptimizeCommand10 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.TransformDecimalCtorToConstant)]
    public class DexInputOptimizeCommand11 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.SimplifyLdObjAndStObj)]
    public class DexInputOptimizeCommand12 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.TransformArrayInitializers)]
    public class DexInputOptimizeCommand13 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.TransformMultidimensionalArrayInitializers)]
    public class DexInputOptimizeCommand14 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.MakeAssignmentExpression)]
    public class DexInputOptimizeCommand15 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.IntroducePostIncrement)]
    public class DexInputOptimizeCommand16 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.InlineExpressionTreeParameterDeclarations)]
    public class DexInputOptimizeCommand17 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.InlineVariables2)]
    public class DexInputOptimizeCommand18 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.FindLoops)]
    public class DexInputOptimizeCommand19 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.FindConditions)]
    public class DexInputOptimizeCommand20 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.FlattenNestedMovableBlocks)]
    public class DexInputOptimizeCommand21 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.RemoveEndFinally)]
    public class DexInputOptimizeCommand22 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.RemoveRedundantCode2)]
    public class DexInputOptimizeCommand23 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.GotoRemoval)]
    public class DexInputOptimizeCommand24 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.DuplicateReturns)]
    public class DexInputOptimizeCommand25 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.GotoRemoval2)]
    public class DexInputOptimizeCommand26 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.InlineVariables3)]
    public class DexInputOptimizeCommand27 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.RecombineVariables)]
    public class DexInputOptimizeCommand28 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.TypeInference2)]
    public class DexInputOptimizeCommand29 : StopOptimizeMenuCommand { }

    [StopOptimizeMenuCommand(AstOptimizationStep.RemoveRedundantCode3)]
    public class DexInputOptimizeCommand30 : StopOptimizeMenuCommand { }


}
