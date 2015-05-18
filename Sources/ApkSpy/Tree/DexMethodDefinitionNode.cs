using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Dot42.ApkSpy.Disassembly;
using Dot42.DebuggerLib;
using Dot42.DexLib;
using Dot42.DexLib.OpcodeHelp;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace Dot42.ApkSpy.Tree
{
    internal class DexMethodDefinitionNode : TextNode
    {
        private readonly MethodDefinition _methodDef;
        private MethodBodyDisassemblyFormatter _formatter;
        private static readonly DalvikOpcodeHelpLookup Opcodes = new DalvikOpcodeHelpLookup();

        private readonly List<TextMarker> _jumpMarkers= new List<TextMarker>();
        private readonly List<TextMarker> _registerMarkers = new List<TextMarker>();

        private string previousWord = null;

        /// <summary>
        /// Default ctor
        /// </summary>
        public DexMethodDefinitionNode(MethodDefinition methodDef)
        {
            this._methodDef = methodDef;
            Text = methodDef.Name;
            ImageIndex = 6;
        }

        /// <summary>
        /// Create all child nodes
        /// </summary>
        protected override void CreateChildNodes()
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected override string LoadText(ISpyContext settings)
        {
            var nl = Environment.NewLine;
            var sb = new StringBuilder();
            sb.AppendFormat("AccessFlags: {0}{1}", _methodDef.AccessFlags, nl);
            sb.AppendFormat("Prototype:   {0}{1}", _methodDef.Prototype, nl);

            var body = _methodDef.Body;
            if (body != null)
            {
                sb.AppendFormat("#Registers:  {0}{1}", body.Registers.Count, nl);
                sb.AppendFormat("#Incoming:   {0}{1}", body.IncomingArguments, nl);
                sb.AppendFormat("#Outgoing:   {0}{1}", body.OutgoingArguments, nl);
                sb.AppendLine();

                _formatter = _formatter ?? new MethodBodyDisassemblyFormatter(_methodDef, settings.MapFile);
                var code = _formatter.Format(settings.EmbedSourceCodePositions, settings.EmbedSourceCode, settings.ShowControlFlow);
                sb.AppendLine(code);

#if DEBUG
                var debugInfo = body.DebugInfo;
                if (debugInfo != null)
                {
                    sb.AppendLine();
                    sb.AppendLine("DEX Debug info:");
                    sb.AppendFormat("\tStart line: {0}{1}", debugInfo.LineStart, nl);
                    int lineNumber = (int) debugInfo.LineStart;
                    var address = 0;
                    var index = 0;
                    foreach (var ins in debugInfo.DebugInstructions)
                    {
                        if (!ins.IsLowLevel)
                            sb.AppendFormat("\t{0:x4}\tline {1}, address {2:x4} -> {3}{4}", index, lineNumber, address, ins, nl);                        
                        ins.UpdateLineAndAddress(ref lineNumber, ref address);
                        index++;
                    }
                }
#endif
            }

            return sb.ToString();
        }

        protected override void OnToolTipRequest(object sender, ToolTipRequestEventArgs e)
        {
            if (!e.InDocument || e.LogicalPosition.IsEmpty)
                return;
         
            var text = (TextArea) sender;
            if (text.Document == null) return;

            var lineSegment = text.Document.GetLineSegment(e.LogicalPosition.Line);
            var lineText = text.Document.GetText(lineSegment);

            string word = GetWordByWhitespaceAndCommas(lineText, e.LogicalPosition.Column);

            if (word == null)
                return;

            // register-variable?
            if (_formatter != null)
            {
                string varName = _formatter.GetVariableByRegisterName(word);
                if (varName != null)
                {
                    e.ShowToolTip(varName);
                    return;
                }
            }

            // opcode-mnemonic?
            var opcode = Opcodes.Lookup(word);
            if (opcode != null)
            {
                string toolTip = string.Format("{0}\n\n{1}\n\n{2}", opcode.Syntax, opcode.Arguments, opcode.Description);
                e.ShowToolTip(toolTip);
                return;
            }
        }

        protected override void OnTextAreaMouseMove(object sender, MouseEventArgs e)
        {
            var text = sender as TextArea;

            if (text == null || text.Document == null) return;

            var loc = e.Location;
            loc.X -= text.TextView.DrawingPosition.X;
            loc.Y -= text.TextView.DrawingPosition.Y;
            var pos = text.TextView.GetLogicalPosition(loc);
            if (pos.IsEmpty) return;

            var document = text.Document;

            if (pos.Line < 0 || pos.Line >= document.LineSegmentCollection.Count)
                return;

            var lineSegment = document.GetLineSegment(pos.Line);
            var lineText = document.GetText(lineSegment);

            string word = GetWordByWhitespaceAndCommas(lineText, pos.Column);

            if (word == previousWord)
                return;
            
            previousWord = word;

            MarkJumpTarget(word, lineText, pos, document, text);
            MarkRegisterUsage(word, document, text);
        }

        private void MarkJumpTarget(string word, string lineText, TextLocation pos, IDocument document, TextArea text)
        {
            if (_jumpMarkers.Count > 0)
            {
                foreach(var marker in _jumpMarkers)
                    document.MarkerStrategy.RemoveMarker(marker);
                _jumpMarkers.Clear();
                text.Refresh();
            }

            if (word == null)
                return;

            bool couldBeJump = Regex.IsMatch(word, "^[0-9a-fA-F]{3,4}$");
            
            if (!couldBeJump)
                return;
            
            int wordIdx;
            GetWordByWhitespaceAndCommas(lineText, pos.Column, out wordIdx);

            bool isFirstWord = lineText.IndexOf(word, StringComparison.Ordinal) == wordIdx;
            bool isJumpInstruction = false;

            if (!isFirstWord)
            {
                var words = GetWordAndPreviousByWhitespaceAndCommas(lineText, pos.Column);
                isJumpInstruction = words != null && words[0] == MethodDisassembly.JumpMarker;
            }

            if (!isFirstWord && !isJumpInstruction)
                return;

            LineSegment mainLine = null;
            int jumpMarkerLen = MethodDisassembly.JumpMarker.Length;
            // jump target?
            foreach (var line in document.LineSegmentCollection)
            {
                bool isLineFirstWord = true;

                string curLine = document.GetText(line);

                int curOffset = 0;
                foreach (var curWord in SplitAndKeep(curLine, "\n\r \t:,().\\".ToCharArray()))
                {
                    if (curWord.Trim() == "")
                    {
                        curOffset += curWord.Length;
                        continue;
                    }
                    // mark all words matching the jump instruction
                    if (curWord == word)
                    {
                        if (isLineFirstWord && mainLine == null)
                        {
                            mainLine = line;
                        }
                        else
                        {
                            // add marker.
                            if (curOffset > 4 && curLine.Substring(curOffset - jumpMarkerLen - 1, jumpMarkerLen) == MethodDisassembly.JumpMarker)
                            {
                                AddJumpMarker(document, line.Offset + curOffset - jumpMarkerLen - 1,
                                              curWord.Length + jumpMarkerLen + 1);
                            }
                            else
                            {
                                AddJumpMarker(document, line.Offset + curOffset, curWord.Length);
                            }
                        }
                    }
                    
                    curOffset += curWord.Length;
                    isLineFirstWord = false;
                }
            }

            if (mainLine != null && _jumpMarkers.Count > 0)
            {
                // better would be to mark the whole line, not only the words.
                AddJumpMarker(document, mainLine.Offset, mainLine.TotalLength);
            }
            
            if(_jumpMarkers.Count > 0)
                text.Refresh();
        }

        public static IEnumerable<string> SplitAndKeep(string s, char[] delims)
        {
            int start = 0, index;

            while ((index = s.IndexOfAny(delims, start)) != -1)
            {
                if (index - start > 0)
                    yield return s.Substring(start, index - start);
                yield return s.Substring(index, 1);
                start = index + 1;
            }

            if (start < s.Length)
            {
                yield return s.Substring(start);
            }
        }

        private void AddJumpMarker(IDocument document, int offset, int length)
        {
            var marker = new TextMarker(offset, length, TextMarkerType.SolidBlock, Color.Gold);
            document.MarkerStrategy.AddMarker(marker);
            _jumpMarkers.Add(marker);
        }

        private void MarkRegisterUsage(string word, IDocument document, TextArea text)
        {
            if (_formatter == null) return;

            var hadRegisterMarks = _registerMarkers.Count > 0;
            if (hadRegisterMarks)
            {
                foreach(var m in _registerMarkers)
                    document.MarkerStrategy.RemoveMarker(m);
            }

            if (word == null || !Regex.IsMatch(word, "^[rp][0-9]+$"))
            {
                if (hadRegisterMarks)
                    text.Refresh();
                return;
            }

            foreach (var line in document.LineSegmentCollection)
            foreach (var w in line.Words)
            {
                if (w.Word != word)
                    continue;

                var marker = new TextMarker(line.Offset + w.Offset, w.Length, TextMarkerType.SolidBlock,
                                            Color.LightSalmon);
                document.MarkerStrategy.AddMarker(marker);
                _registerMarkers.Add(marker);
            }

            if (_registerMarkers.Count > 0 || hadRegisterMarks)
            {
                text.Refresh();
            }

        }

        private string GetWordByWhitespaceAndCommas(string lineText, int column)
        {
            int wordIdx;
            return GetWordByWhitespaceAndCommas(lineText, column, out wordIdx);
        }

        private string GetWordByWhitespaceAndCommas(string lineText, int column, out int startIdx)
        {
            startIdx = -1;

            if (column >= lineText.Length)
                return null;

            var c = lineText[column];
            if (char.IsWhiteSpace(c) || c == ',')
                return null;

            int idxAfterEnd = column + 1;
            for (; idxAfterEnd < lineText.Length ; ++idxAfterEnd)
            {
                c = lineText[idxAfterEnd];
                if (char.IsWhiteSpace(c) || c == ',')
                    break;
            }
            int idxBeforeStart = column - 1;
            for (; idxBeforeStart >= 0; --idxBeforeStart)
            {
                c = lineText[idxBeforeStart];
                if (char.IsWhiteSpace(c) || c == ',')
                    break;
            }

            startIdx = idxBeforeStart + 1;
            return lineText.Substring(startIdx, idxAfterEnd - idxBeforeStart - 1);
        }

        private string[] GetWordAndPreviousByWhitespaceAndCommas(string lineText, int column)
        {
            int wordIdx, wordIdx2;
            string word = GetWordByWhitespaceAndCommas(lineText, column, out wordIdx);

            if (word == null)
                return null;

            for (--wordIdx; wordIdx >= 0; --wordIdx)
            {
                var c = lineText[wordIdx];
                if (!char.IsWhiteSpace(c) && c != ',' )
                    break;
            }

            if (wordIdx == -1)
                return null;

            return new [] { GetWordByWhitespaceAndCommas(lineText, wordIdx, out wordIdx2), word };
        }

        private string AccessFlagsAsString(AccessFlags accessFlags)
        {
            string result = accessFlags.HasFlag(AccessFlags.Public) ? "public " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Private) ? "private " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Protected) ? "protected " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Static) ? "static " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Final) ? "final " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Synchronized) ? "synchronized " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Bridge) ? "bridge " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.VarArgs) ? "varargs " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Native) ? "native " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Abstract) ? "abstract " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.StrictFp) ? "strictfp " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Synthetic) ? "synthetic " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.Constructor) ? "constructor " : string.Empty;
            result += accessFlags.HasFlag(AccessFlags.DeclaredSynchronized) ? "synchronized " : string.Empty;

            return result.Trim();
        }
    }
}
