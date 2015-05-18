using System;
using System.Drawing;
using System.Text;
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
        private TextMarker markedLine;
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

            string word = GetWordByWhitespace(lineText, e.LogicalPosition.Column);

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

            var words = GetWordAndPreviousByWhitespace(lineText, pos.Column);

            //Console.WriteLine("{0} {1} {2}", pos.Line, pos.Column, words==null?"(null)": words[1]);

            var word = words == null ? null : words[1];
            if (word == previousWord)
                return;

            if (markedLine != null)
            {
                document.MarkerStrategy.RemoveMarker(markedLine);
                markedLine = null;
                text.Refresh();
            }

            previousWord = word;

            if (word == null)
                return;

            // jump target?
            if (words[0] == MethodDisassembly.JumpMarker)
            {
                foreach (var line in document.LineSegmentCollection)
                {
                    if (line.Offset + 5 >= document.TextLength)
                        break;

                    var lineBeginning = document.GetText(line.Offset, 5).Trim();
                    if (lineBeginning == word)
                    {
                        // found jump target.
                        // better would be to mark the whole line, not only the words.
                        markedLine = new TextMarker(line.Offset, line.TotalLength, TextMarkerType.SolidBlock, 
                                                    Color.Gold);
                        document.MarkerStrategy.AddMarker(markedLine);
                        text.Refresh();
                        return;
                    }
                }
            }
        }

        private string GetWordByWhitespace(string lineText, int column)
        {
            int wordIdx;
            return GetWordByWhitespace(lineText, column, out wordIdx);
        }

        private string GetWordByWhitespace(string lineText, int column, out int startIdx)
        {
            startIdx = -1;

            if (column >= lineText.Length)
                return null;

            if (char.IsWhiteSpace(lineText[column]))
                return null;

            int idxAfterEnd = column + 1;
            for (; idxAfterEnd < lineText.Length ; ++idxAfterEnd)
            {
                if (char.IsWhiteSpace(lineText[idxAfterEnd]))
                    break;
            }
            int idxBeforeStart = column - 1;
            for (; idxBeforeStart >= 0; --idxBeforeStart)
            {
                if (char.IsWhiteSpace(lineText[idxBeforeStart]))
                    break;
            }

            startIdx = idxBeforeStart + 1;
            return lineText.Substring(startIdx, idxAfterEnd - idxBeforeStart - 1);
        }

        private string[] GetWordAndPreviousByWhitespace(string lineText, int column)
        {
            int wordIdx, wordIdx2;
            string word = GetWordByWhitespace(lineText, column, out wordIdx);

            if (word == null)
                return null;

            for (--wordIdx; wordIdx >= 0; --wordIdx)
            {
                if (!char.IsWhiteSpace(lineText[wordIdx]))
                    break;
            }

            if (wordIdx == -1)
                return null;

            return new [] { GetWordByWhitespace(lineText, wordIdx, out wordIdx2), word };
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
