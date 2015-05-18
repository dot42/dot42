using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dot42.DexLib.OpcodeHelp.CSV;
using Dot42.Utility;

namespace Dot42.DexLib.OpcodeHelp
{
    public class DalvikOpcodeHelpLookup
    {
        private const string DalvikOpcodesCsvRessource = "Dot42.DexLib.OpcodeHelp.DalvikOpcodes.csv";

        private readonly Dictionary<string, DalvikOpcodeHelp> _opcodes = new Dictionary<string, DalvikOpcodeHelp>(StringComparer.InvariantCultureIgnoreCase);

        public DalvikOpcodeHelpLookup()
        {
            try
            {
                using (var csv = new CSVFile(GetType().Assembly.GetManifestResourceStream(DalvikOpcodesCsvRessource), QuoteChar.Quotes))
                {
                    var opAndFormat = "";
                    var mnemonic    = "";
                    var arguments   = "";
                    var description = "";

                    while (csv.NextRow())
                    {
                        var curOpAndFormat = csv.String(0);
                        var curMnemonic = csv.String(1);
                        var curArguments = csv.String(2);
                        var curDescription = csv.String(3);

                        if (string.IsNullOrEmpty(curOpAndFormat) || string.IsNullOrEmpty(curMnemonic))
                        {
                            // this is an addition to the previous line.
                            if (!string.IsNullOrEmpty(curOpAndFormat))
                                opAndFormat += "\n" + curOpAndFormat;
                            if (!string.IsNullOrEmpty(curMnemonic))
                                mnemonic += "\n" + curMnemonic;
                            if (!string.IsNullOrEmpty(curArguments))
                                arguments += "\n" + curArguments;
                            if (!string.IsNullOrEmpty(curDescription))
                                description += curDescription;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(mnemonic))
                                AddEntry(opAndFormat, mnemonic, arguments, description);

                            opAndFormat = curDescription;
                            mnemonic = curMnemonic;
                            arguments = curArguments;
                            description = curDescription;
                        }
                    }

                    if (!string.IsNullOrEmpty(mnemonic))
                        AddEntry(opAndFormat, mnemonic, arguments, description);
                }
            }
            catch (Exception ex)
            {
                DLog.Warning(DContext.CompilerCodeGenerator, "unable to parse opcode help: ", ex.Message);
            }

        }

        private void AddEntry(string opAndFormat, string mnemonic, string arguments, string description)
        {
            var help = new DalvikOpcodeHelp(opAndFormat, mnemonic, arguments, description);
            foreach (var m in mnemonic.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries))
            {
                var mm = m;

                if (Regex.IsMatch(mm, "^[0-9a-fA-F][0-9a-fA-F]: ")) 
                    mm = mm.Substring(4);

                string mnem = mm.Split()[0].Replace("-", "_");

                _opcodes[mnem] = help;
                _opcodes[mnem.Replace("/", "")] = help;
                _opcodes[mnem.Replace("/", "_")] = help;
            }
        }


        public DalvikOpcodeHelp Lookup(string opcode)
        {
            DalvikOpcodeHelp ret;
            _opcodes.TryGetValue(opcode.Replace("-", "_"), out ret);
            return ret;
        }
    }
}