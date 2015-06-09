
using System;
using System.Collections.Generic;
using System.Globalization;
using com.android.dx.command.dexer;

namespace Dot42.CompilerLib.Target.Dx
{
    public static class DxInvoker
    {
        public static void CompileToDex(string jarFileName, string dexFileName, bool generateDebugSymbols)
        {
            var param = new List<string>
            {
                //"--dex",
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

            lock (typeof (Main))
            {
                // After a short glance at the code, it seems to be not multithreading capable,
                // since it makes heavy use of static variables (!).
                // One way to alleviate this would be to start a new AppDomain. For now we just 
                // invoke everything in serial order.

                var args = new Main.Arguments();
                args.parse(param.ToArray());

                int ret = Main.run(args);
                if(ret != 0)
                    throw new Exception("Error running 'dx' on '" + jarFileName + "'. Return code was: " + ret.ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}
