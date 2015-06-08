
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Dot42.CompilerLib.Target.Dx
{
    public static class DxInvoker
    {
        public static void CompileToDex(string jarFileName, string dexFileName, bool generateDebugSymbols)
        {
            var param = new List<string>
            {
                "--dex",
                "--output",
                dexFileName,
                "--positions",
                (generateDebugSymbols? "lines" : "none"),
            };

            if(!generateDebugSymbols)
                param.Add("--no-locals");

            if (Environment.ProcessorCount > 1)
            {
                param.Add("--num-threads");
                param.Add(Environment.ProcessorCount.ToString(CultureInfo.InvariantCulture));
            }

            param.Add(jarFileName);

            com.android.dx.command.Main.main(param.ToArray());
        }
    }
}
