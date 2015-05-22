extern alias ilspy;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dot42.CompilerLib;
using Dot42.CompilerLib.Ast;
using Dot42.CompilerLib.Target;
using Dot42.CompilerLib.XModel;
using Dot42.CompilerLib.XModel.DotNet;
using ilspy::Mono.Cecil;
using ICSharpCode.Decompiler;
using ICSharpCode.ILSpy;

namespace Dot42.Compiler.ILSpy
{
    /// <summary>
    /// Shows the ILAst that is used to compile to dex.
    /// </summary>
    [Export(typeof(Language))]
    public class DexInputLanguage : CompiledLanguage
    {
        internal static StopAstConversion StopConversion { get; set; }
        internal static bool ShowHasSeqPoint { get; set; }
        internal static bool BreakExpressionLines { get; set; }
        internal static bool ShowFullNames { get; set; }

        public DexInputLanguage()
        {
            ShowHasSeqPoint = false;
            BreakExpressionLines = true;
        }
        public override string Name
        {
            get { return "Dex Input"; }
        }

        public override string FileExtension
        {
            get { return ".il"; }
        }

        public override void DecompileMethod(ilspy::Mono.Cecil.MethodDefinition method, ICSharpCode.Decompiler.ITextOutput output, DecompilationOptions options)
        {
            var xMethod = GetXMethodDefinition(method);
            DecompileMethod(xMethod, output);
        }

        public override void DecompileType(TypeDefinition type, ITextOutput output, DecompilationOptions options)
        {
            var xType = GetXTypeDefinition(type);
            
            output.WriteLine("class " + type.Name);
            output.WriteLine("{");

            foreach (var field in xType.Fields)
            {
                if (!field.IsReachable)
                    continue;
                output.WriteLine("\t{0} {1};", field.FieldType.Name, field.Name);
            }
                
            output.WriteLine();

            foreach (var method in xType.Methods)
            {
                var ilMethod = method as XBuilder.ILMethodDefinition;
                if (ilMethod != null && !ilMethod.OriginalMethod.IsReachable)
                    continue;

                output.Write("\t{0} {1}(", method.ReturnType.Name, method.Name);
                
                List<string> parms = method.Parameters.Select(p => string.Format("{0}{1} {2}", 
                                                                     KindToStringAndSpace(p.Kind), 
                                                                     p.ParameterType.Name, 
                                                                     p.Name))
                                                      .ToList();

                if (method.NeedsGenericInstanceTypeParameter)
                    parms.Add("Type[] git");
                if (method.NeedsGenericInstanceMethodParameter)
                    parms.Add("Type[] gim");

                output.Write(string.Join(", ", parms));
                output.WriteLine(")");
                output.WriteLine("\t{");
                DecompileMethod(method, output, 2);
                output.WriteLine("\t}");
                output.WriteLine();
            }

            output.WriteLine("}");
            
        }

        private string KindToStringAndSpace(XParameterKind kind)
        {
            if (kind == XParameterKind.ByReference)
                return "ref ";
            if (kind == XParameterKind.Output)
                return "out ";
            return "";
        }

        private void DecompileMethod(XMethodDefinition xMethod, ITextOutput output, int indentation=0)
        {
            var indent = new string(Enumerable.Repeat('\t', indentation).ToArray());

            var ilMethod = xMethod as XBuilder.ILMethodDefinition;
            if (ilMethod == null || !ilMethod.OriginalMethod.HasBody)
            {
                output.Write(indent);
                output.WriteLine("// not an il method or method without body.");
                return;
            }

            var methodSource = new MethodSource(xMethod, ilMethod.OriginalMethod);
            var node = MethodBodyCompiler.CreateOptimizedAst(AssemblyCompiler, methodSource, false, StopConversion);

            if (StopConversion != StopAstConversion.None)
            {
                output.Write(indent);
                output.Write("// Stop " + StopConversion);
                output.WriteLine();
                output.WriteLine();
            }

            var bridge = new TextOutputBridge(output);
            
            for(int i = 0; i < indentation; ++i)
                bridge.Indent();

            FormattingOptions formattingOptions;
            if (StopConversion == StopAstConversion.AfterILConversion || !BreakExpressionLines)
                formattingOptions = FormattingOptions.Default;
            else
                formattingOptions = FormattingOptions.BreakExpressions;
            
            if(!ShowFullNames)
                formattingOptions |= FormattingOptions.SimpleNames;
            if(ShowHasSeqPoint)
                formattingOptions |= FormattingOptions.ShowHasSeqPoint;

            node.WriteTo(bridge, formattingOptions);

            for (int i = 0; i < indentation; ++i)
                bridge.Unindent();

        }
    }
}
