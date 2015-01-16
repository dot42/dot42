using System;
using Microsoft.VisualStudio.Package;

namespace Dot42.VStudio.XmlEditor
{
    internal class Source : ISource
    {
        internal readonly Microsoft.VisualStudio.Package.Source source;

        /// <summary>
        /// Default ctor
        /// </summary>
        public Source(Microsoft.VisualStudio.Package.Source source)
        {
            this.source = source;
        }

        void ISource.Save(Action saveAction)
        {
            // Wrap the buffer sync and the formatting in one undo unit.
            using (var ca = new CompoundAction(source, "Synchronize buffer"))
            {
                saveAction();
                /*using (var scope = xmlStore.BeginEditingScope(Resources.SynchronizeBuffer, this))
                {
                    //Replace the existing XDocument with the new one we just generated.
                    document.Root.ReplaceWith(documentFromDesignerState.Root);
                    scope.Complete();
                }*/
                ca.FlushEditActions();
                FormatBuffer(source);
            }
        }

        public ITokenInfo GetTokenInfo(int line, int column)
        {
            var tokenInfo = source.GetTokenInfo(line, column);
            return new TokenInfo(tokenInfo);
        }

        /// <summary>
        /// Reformat the text buffer
        /// </summary>
        private void FormatBuffer(Microsoft.VisualStudio.Package.Source src)
        {
            using (var edits = new EditArray(src, null, false, "Reformat"))
            {
                var span = src.GetDocumentSpan();
                src.ReformatSpan(edits, span);
            }
        }
    }

}
