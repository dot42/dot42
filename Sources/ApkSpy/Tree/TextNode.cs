using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dot42.DexLib;
using Dot42.JvmClassLib.Attributes;
using Dot42.Utility;
using ICSharpCode.TextEditor;

namespace Dot42.ApkSpy.Tree
{
    internal abstract class TextNode : Node
    {
        /// <summary>
        /// Create a view to display the Types of this node.
        /// </summary>
        internal override Control CreateView(ISpyContext settings)
        {
            var tb = CreateTextBox();

            try
            {
                tb.Text = LoadText(settings);
            }
            catch (Exception ex)
            {
                tb.Text = string.Format("Error: {0}\n\n{1}", ex.Message, ex.StackTrace);
            }
            return tb;
        }

        private TextEditorControl CreateTextBox()
        {
            var tb = new TextEditorControl();
            tb.IsReadOnly = true;            
            
            // TODO: set proper highlighting.
            tb.SetHighlighting("C#");

            var high = (ICSharpCode.TextEditor.Document.DefaultHighlightingStrategy)tb.Document.HighlightingStrategy;
            var def = high.Rules.First();
            var delim = " ,().:\t\n\r";
            for(int i = 0; i < def.Delimiters.Length; ++i)
                def.Delimiters[i] = delim.Contains((char)i);

            tb.ShowLineNumbers = false;
            tb.ShowInvalidLines = false;
            tb.ShowVRuler = false;
            tb.ActiveTextAreaControl.TextArea.ToolTipRequest += OnToolTipRequest;
            tb.ActiveTextAreaControl.TextArea.MouseMove += OnTextAreaMouseMove;

            string[] tryFonts = new[] { "Consolas", "Lucida Console" };

            foreach (var fontName in tryFonts)
            {
                tb.Font = new Font(fontName, 9, FontStyle.Regular);
                if (tb.Font.Name == fontName) break;
            }

            if (!tryFonts.Contains(tb.Font.Name))
                tb.Font = new Font(FontFamily.GenericMonospace, 9);
            
            return tb;
        }

        protected virtual void OnTextAreaMouseMove(object sender, MouseEventArgs e)
        {
        }

        protected virtual void OnToolTipRequest(object sender, ToolTipRequestEventArgs e)
        {
        }

        /// <summary>
        /// Load the text to display
        /// </summary>
        protected abstract string LoadText(ISpyContext settings);

        /// <summary>
        /// Convert annotations into text
        /// </summary>
        internal static string LoadAnnotations(IAnnotationProvider provider)
        {
            if (provider.Annotations.Count == 0)
                return string.Empty;
            return string.Join(Environment.NewLine, provider.Annotations.Select(x => "  -  " + x).ToArray());
        }


        /// <summary>
        /// Convert java annotations into text
        /// </summary>
        internal static string LoadAnnotations(IAttributeProvider provider)
        {
            var annAttributes = provider.Attributes.OfType<AnnotationsAttribute>().ToList();
            if (annAttributes.Count == 0)
                return "-";

            var sb = new StringBuilder();
            var nl = Environment.NewLine;
            foreach (var attr in annAttributes)
            {
                sb.Append("    ");
                sb.Append(attr.Name);
                sb.Append(nl);

                foreach (var ann in attr.Annotations)
                {
                    sb.Append("        -");
                    sb.Append(ann.AnnotationTypeName);
                    sb.Append(nl);

                    foreach (var pair in ann.ValuePairs)
                    {
                        sb.Append("            -");
                        sb.AppendFormat("{0} \t - {1}", pair.ElementName, pair.Value);
                        sb.Append(nl);
                    }
                }
            }
            return sb.ToString();
        }
    }
}
