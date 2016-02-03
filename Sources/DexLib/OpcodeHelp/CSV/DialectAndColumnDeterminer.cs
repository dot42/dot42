using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Dot42.DexLib.OpcodeHelp.CSV
{
    /// <summary>
    /// if Quotes is selected, spaces are not allowed as Column Separator
    /// </summary>
    public enum QuoteChar
    {
        None,
        Quotes 
    }

    internal class DialectAndColumnDeterminer
    {
        private readonly QuoteChar quoteChar;
        private Regex splitter;
        private IList<string> columns;
        private SplitType splitType;
        private string headerLine;
        private SplitTypeCount headerCount;
        private int numberOfHeaderLines = 1;
        

        public Regex ColumnSplit { get { return splitter; } }
        public IList<string> Columns { get { return columns; } }
        public int NumberOfHeaderLines { get { return numberOfHeaderLines; } }

        private enum SplitType
        {
            Tab, Komma, Semikolon, Space,/* TabAndSpace*/
        }
        private struct SplitTypeCount
        {
            public int spaces;
            public int tabs;
            public int kommas;
            public int semikolons;
        };

        public DialectAndColumnDeterminer(QuoteChar quoteChar)
        {
            this.quoteChar = quoteChar;
        }

        public void Feed(string line, int lineNo)
        {
            if (lineNo == 0)
            {
                EvaluteHeader(line);
                return;
            }
            
            if (lineNo == 1)
            {
                //int curColCount = splitter.Split(line).Length;

                if (splitType == SplitType.Space)
                {
                    // Resurrect on wrong counted spaces in header
                    SplitTypeCount c = CountChars(line);
                    if (c.spaces == 0)
                    {
                        c = headerCount;
                        if (c.tabs >= c.kommas && c.tabs >= c.semikolons)
                            UsedSplitType = SplitType.Tab;
                        else if (c.kommas >= c.tabs && c.kommas >= c.semikolons)
                            UsedSplitType = SplitType.Komma;
                        else if (c.semikolons > 0)
                            UsedSplitType = SplitType.Semikolon;
                    }
                }
                //// allow wired files with space delimiter on all but first line,
                //// there tab.
                //if (splitter.Split(line).Length <= 1)
                //{
                //    splitType = SplitType.TabAndSpace;
                //    splitter = new Regex("[ \t]+");
                //}
            }
        }

        public void Done()
        {
        }

        private SplitType UsedSplitType
        {
            set
            {
                splitType = value;
                if (splitType == SplitType.Tab)
                    splitter = new Regex("\t");
                else if(splitType == SplitType.Komma)
                    splitter = new Regex(",");
                else if (splitType == SplitType.Semikolon)
                    splitter = new Regex(";");
                else if (splitType == SplitType.Space)
                    splitter = new Regex("[ ]+");
                //else if (splitType == SplitType.TabAndSpace)
                //    splitter = new Regex("[ \t]+");
                
                // determine column-names
                columns = splitter.Split(headerLine);
            }
        }

        public QuoteChar QuoteChar
        {
            get { return quoteChar; }
        }

        private void EvaluteHeader(string line)
        {
            headerLine = line;
            SplitTypeCount c = headerCount = CountChars(line);
            
            if (QuoteChar != QuoteChar.None)
                c.spaces = 0;

            if (c.tabs >= c.kommas && c.tabs >= c.semikolons && c.tabs >= c.spaces)
                UsedSplitType = SplitType.Tab;
            else if (c.kommas >= c.tabs && c.kommas >= c.semikolons && c.kommas >= c.spaces)
                UsedSplitType = SplitType.Komma;
            else if (c.semikolons >= c.spaces)
                UsedSplitType = SplitType.Semikolon;
            else if (c.spaces > 0)
                UsedSplitType = SplitType.Space;
            else
                throw new IOException("unable to determine CSV-dialect");
        }

        private static SplitTypeCount CountChars(string line)
        {
            SplitTypeCount ret = new SplitTypeCount();
            // determine CVS-Dialect
            foreach (char c in line)
            {
                if (c == '\t') ret.tabs += 1;
                else if (c == ',') ret.kommas += 1;
                else if (c == ';') ret.semikolons += 1;
                else if (c == ' ') ret.spaces += 1;
            }
            return ret;
        }

        public string Join(string[] columns)
        {
            if (splitType == SplitType.Komma)
                return string.Join(",", columns);
            else if (splitType == SplitType.Semikolon)
                return string.Join(";", columns);
            else if (splitType == SplitType.Space)
                return string.Join(" ", columns);
            else if (splitType == SplitType.Tab)
                return string.Join("\t", columns);
            else
                throw new Exception("unknown SplitType");
        }
    }
}