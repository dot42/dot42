using System;
using System.IO;
using System.Linq;
using System.Text;
using Dot42.CompilerLib.RL2DexCompiler;
using Dot42.DebuggerLib.Model;
using Dot42.DexLib;
using Dot42.Mapping;

namespace Dot42.ApkSpy.Disassembly
{
    public class MethodBodyDisassemblyFormatter
    {
        private readonly MethodDefinition _methodDef;
        private readonly MapFileLookup _mapFile;
        private readonly Lazy<string[]> _sourceDocument;
        private readonly MethodDisassembly _dissassembly;

        public MethodBodyDisassemblyFormatter(MethodDefinition methodDef, MapFileLookup mapFile)
        {
            _methodDef = methodDef;
            _mapFile = mapFile;

            TypeEntry typeEntry = null;
            MethodEntry methodEntry = null;

            if (mapFile != null)
            {
                typeEntry = mapFile.GetTypeByNewName(methodDef.Owner.Fullname);
                if (typeEntry != null)
                {
                    methodEntry = typeEntry.FindDexMethod(methodDef.Name, methodDef.Prototype.ToSignature());
                }
            }

            _dissassembly = new MethodDisassembly(methodDef, mapFile, typeEntry, methodEntry);

            _sourceDocument = new Lazy<string[]>(() =>
            {
                if (_dissassembly.MethodEntry == null)
                    return null;
                var pos = _mapFile.GetSourceCodePositions(_dissassembly.MethodEntry).FirstOrDefault();
                if (pos == null)
                    return null;

                try
                {
                    return File.ReadAllLines(pos.Document.Path);
                }
                catch (Exception)
                {
                    return null;
                }
            });
        }

        public string Format(bool embedSourcePositions, bool embedSourceCode, bool showControlFlow)
        {
            var nl = Environment.NewLine;
            var sb = new StringBuilder();
            
            var body = _methodDef.Body;
            var cfg = new ControlFlowGraph(body);
            SourceCodePosition lastSource = null;

            if ((embedSourceCode || embedSourcePositions) && _dissassembly.MethodEntry != null)
            {
                var pos = _mapFile.GetSourceCodePositions(_dissassembly.MethodEntry).FirstOrDefault();
                if (pos != null)
                {
                    sb.Append(" // ----- Source Code: ");
                    sb.Append(pos.Document.Path);
                    sb.Append(nl);
                }
            }

            foreach (var block in cfg)
            {
             if (showControlFlow)
                {
                    sb.AppendFormat(" // ----- Entry [{0}] Exit [{1}]{2}",
                        string.Join(", ", block.EntryBlocks.Select(x => x.Entry.Offset.ToString("X3"))),
                        string.Join(", ", block.ExitBlocks.Select(x => x.Entry.Offset.ToString("X3"))),
                        nl);
                }

                foreach (var i in block.Instructions)
                {
                    if (embedSourcePositions || embedSourceCode)
                    {
                        var source = _dissassembly.FindSourceCode(i.Offset, false);

                        if (source != null && lastSource != null && source.Document.Path != lastSource.Document.Path)
                        {
                            // print document name.
                            sb.Append(" // ----- Source Code: ");
                            sb.Append(source.Document.Path);
                            sb.Append(nl);
                            lastSource = null;
                        }

                        if (embedSourcePositions && source == null && lastSource != null)
                        {
                            sb.AppendLine(" // ----- (no source)");
                        }
                        else if (source != null && (lastSource == null || !source.Position.EqualExceptOffset(lastSource.Position)))
                        {
                            if (embedSourcePositions)
                                sb.AppendFormat(" // ----- Position: {0} - {1}{2}", source.Position.Start, source.Position.End, nl);

                            if (embedSourceCode)
                            {
                                string[] lines = GetSourceCodeLines(source);
                                if (lines != null)
                                    sb.AppendLine(" // " + string.Join(nl + " // ", lines));
                            }
                        }
                        lastSource = source;
                    }
                    
                    sb.AppendLine(_dissassembly.FormatInstruction(i));
                }
            }
            sb.AppendLine();

            if (body.Exceptions.Any())
            {
                sb.AppendLine("Exception handlers:");
                foreach (var handler in body.Exceptions)
                {
                    sb.AppendFormat("\t{0:x4}-{1:x4}{2}", handler.TryStart.Offset, handler.TryEnd.Offset, nl);
                    foreach (var c in handler.Catches)
                    {
                        sb.AppendFormat("\t\t{0} => {1:x4}{2}", c.Type, c.Instruction.Offset, nl);
                    }
                    if (handler.CatchAll != null)
                    {
                        sb.AppendFormat("\t\t{0} => {1:x4}{2}", "<any>", handler.CatchAll.Offset, nl);
                    }
                }
                sb.AppendLine();
            }

            if (_mapFile != null)
            {
                var typeEntry = _mapFile.GetTypeByNewName(_methodDef.Owner.Fullname);
                if (typeEntry != null)
                {
                    var methodEntry = typeEntry.FindDexMethod(_methodDef.Name, _methodDef.Prototype.ToSignature());
                    if (methodEntry != null)
                    {
                        var validParameters = methodEntry.Parameters.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
                        if (validParameters.Any())
                        {
                            sb.AppendLine("Parameters:");
                            foreach (var p in validParameters)
                            {
                                sb.AppendFormat("\t{0} (r{1}) -> {2}{3}", _dissassembly.FormatRegister(p.Register), p.Register, p.Name, nl);
                            }
                            sb.AppendLine();
                        }

                        var validVariables = methodEntry.Variables.Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
                        if (validVariables.Any())
                        {
                            sb.AppendLine("Variables:");
                            foreach (var p in validVariables)
                            {
                                sb.AppendFormat("\t{0} -> {1}{2}", _dissassembly.FormatRegister(p.Register), p.Name, nl);
                            }
                            sb.AppendLine();
                        }

                        sb.AppendLine("Source code positions:");
                        Document lastDocument = null;
                        foreach (var row in _mapFile.GetSourceCodePositions(methodEntry))
                        {
                            if (row.Document != lastDocument)
                            {
                                sb.AppendFormat("\t{0}{1}", row.Document.Path, nl);
                                lastDocument = row.Document;
                            }
                            var pos = row.Position;
                            sb.AppendFormat("\t{0:x4}\t({1},{2}) - ({3},{4}){5}", pos.MethodOffset, pos.Start.Line, pos.Start.Column, pos.End.Line, pos.End.Column, nl);
                        }
                    }
                }
            }
            return sb.ToString();
        }

        private string[] GetSourceCodeLines(SourceCodePosition source)
        {
            // there can be at most one document per method, so don't protect
            // against document path changes.

            var lines = _sourceDocument.Value;
            if (_sourceDocument.Value == null)
                return null;
            var pos = source.Position;
            var startLine = pos.Start.Line - 1;
            if (startLine >= lines.Length)
                return null;

            if (startLine < 0)
                return null;

            int numLines = pos.End.Line - startLine;

            if (numLines < 0)
                numLines = 1;

            if (startLine + numLines >= lines.Length)
                numLines = lines.Length - startLine;

            if (numLines > 3) // show at most 3 lines.
                numLines = 3; 

            string[] ret = new string[numLines];
            for (int i = 0; i < numLines; ++i)
                ret[i] = lines[startLine + i];
            return ret;
        }
    }
}
