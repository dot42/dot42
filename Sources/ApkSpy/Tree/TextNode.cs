using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dot42.DexLib;
using Dot42.JvmClassLib.Attributes;

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

        private static TextBox CreateTextBox()
        {
            var tb = new TextBox();
            tb.ReadOnly = true;
            tb.MaxLength = 64*1024*1024;
            tb.Multiline = true;
            tb.ScrollBars = ScrollBars.Both;
            tb.WordWrap = false;
            tb.ShortcutsEnabled = true;
            tb.BackColor = Color.White;

            tb.KeyDown += (sender, e) =>
            {
                // as per http://stackoverflow.com/questions/14429445/how-can-i-allow-things-such-as-ctrl-a-and-ctrl-backspace-in-a-c-sharp-textbox
                if (e.Control & e.KeyCode == Keys.A)
                    ((TextBox)sender).SelectAll();
            };

            return tb;
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
