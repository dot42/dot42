using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Dot42.DexLib.OpcodeHelp.CSV
{
    internal class CSVFile : IDisposable
    {
        private readonly StreamReader stream;
        private readonly FileStream fileStream;
        private readonly DialectAndColumnDeterminer dialect;
        private bool readSecondLine = false;
        private string secondLine;
        private string[] currentLine;

        public CSVFile(string filename, QuoteChar quoteChar)
        {
            fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }
        
        public CSVFile(string filename)
            : this(filename, QuoteChar.None)
        {
        }

        public CSVFile(Stream stream, QuoteChar quoteChar)
        {
            this.stream = new StreamReader(stream);

            dialect = new DialectAndColumnDeterminer(quoteChar);
            dialect.Feed(this.stream.ReadLine(), 0);
            secondLine = this.stream.ReadLine();
            dialect.Feed(secondLine, 1);
            dialect.Done();
        }

        public IList<string> Columns { get { return dialect.Columns; } }

        public bool NextRow()
        {
            return ReadLine() != null;
        }

        public float Progress
        {
            get
            {
                return ((float)stream.BaseStream.Position) / stream.BaseStream.Length;
            }
        }

        public string[] ReadLine()
        {
            string f = NextLine();
            if(f == null) return null;

            if(dialect.QuoteChar == QuoteChar.None)
                currentLine = dialect.ColumnSplit.Split(f);
            else
            {
                char quoteChar = '"';

                ArrayList list = new ArrayList(dialect.Columns.Count);
                
                int iidx = 0;
                Match m;

                while(f != null && iidx < f.Length)
                {
                    if (f[iidx] == quoteChar)
                    {
                        string currentString = "";
                        iidx += 1;
                        
                        // find end of quote
                        do
                        {
                            int endQuote = iidx;
                            // find ending quote; but skip double quotes.
                            do
                            {
                                endQuote = f.IndexOf(quoteChar, endQuote);
                                if (endQuote == -1 || endQuote == f.Length - 1) break;
                                if (f[endQuote + 1] == quoteChar) 
                                    endQuote += 2; // try next.
                                else break;
                            } while (true);
                            
                            if (endQuote == -1)
                            {
                                // try next line.
                                currentString += f.Substring(iidx) + "\n";
                                iidx = 0;
                                f = NextLine();
                            }
                            else
                            {
                                currentString += f.Substring(iidx, endQuote - iidx);
                                iidx = endQuote + 1;
                                
                                // TODO: what to do, when not at separator?
                                var separatorMatch = dialect.ColumnSplit.Match(f, iidx);
                                if (separatorMatch.Success)
                                    iidx += separatorMatch.Length;
                                break;
                            }
                        } while (f != null);

                        // add, but replace double quotes with single quotes.
                        list.Add(currentString.Replace(quoteChar.ToString() + quoteChar, quoteChar.ToString()));
                    }
                    else
                    {
                        // find field seperator
                        m = dialect.ColumnSplit.Match(f, iidx);
                        if (!m.Success)
                        {
                            // end of line.
                            var currentString = f.Substring(iidx, f.Length - iidx);
                            list.Add(currentString);
                            iidx = f.Length;
                        }
                        else
                        {
                            // separator found
                            var currentString = f.Substring(iidx, m.Index - iidx);
                            list.Add(currentString);
                            iidx = m.Index + m.Length;
                        }
                    }
                }
                currentLine = (string[])list.ToArray(typeof(string));
            }
            return currentLine;
        }

        public double Double(int col)
        {
            if (currentLine == null || currentLine.Length <= col || col < 0) 
                return double.NaN;
            return Convert.ToDouble(currentLine[col], CultureInfo.InvariantCulture);
        }
        public double Double(string colname)
        {
            return Double(Columns.IndexOf(colname));
        }

        public string String(int col)
        {
            if (currentLine == null || currentLine.Length <= col || col < 0) return string.Empty;
            return currentLine[col];
        }
        public string String(string colname)
        {
            return String(Columns.IndexOf(colname));
        }

        public int CurrentRowColCount
        {
            get { return currentLine.Length; }
        }

        private string NextLine()
        {
            string f;
            if(!readSecondLine)
            {
                f = secondLine;
                secondLine = null;
                readSecondLine = true;
            }
            else
                f = stream.ReadLine();
            return f;
        }

        public void Dispose()
        {
            stream.Dispose();
            if(fileStream != null)
                fileStream.Dispose();
        }
    }
}
