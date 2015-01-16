using System;
using System.Linq;
using System.Runtime.InteropServices;
using Dot42.VStudio.XmlEditor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
namespace Dot42.VStudio.Editor
{
    internal class XmlResourceCompletionController : IOleCommandTarget 
    {
        private readonly ITextView textView;
        private readonly ICompletionBroker completionBroker;
        //private ICompletionSession completionSession;
        private readonly IOleCommandTarget nextCommandTarget;
        private readonly System.IServiceProvider serviceProvider;

        internal XmlResourceCompletionController(System.IServiceProvider serviceProvider, IVsTextView vsTextView, ITextView textView, ICompletionBroker completionBroker, ITextStructureNavigatorSelectorService textStructureNavigatorSelectorService)
        {
            this.serviceProvider = serviceProvider;
            this.textView = textView;
            this.completionBroker = completionBroker;

            //add the command to the command chain
            vsTextView.AddCommandFilter(this, out nextCommandTarget);
        }

        private void ShowCompletion()
        {
            // If there is no active session
            if (!completionBroker.IsCompletionActive(textView))
            {
                // Trigger the completion session
                var completionSession = completionBroker.TriggerCompletion(textView);

                // do initial filtering
                if (completionSession != null)
                {
                    completionSession.Filter();
                }
            }
        }

        /// <summary>
        /// Is the given key a completion trigger key?
        /// </summary>
        private bool IsTriggerKey(char key)
        {
            var xls = XmlEditorServiceProvider.GetEditorService(serviceProvider, Dot42Package.DteVersion).GetLanguageService();
            var quoteChar = xls.AutoInsertAttributeQuotes ? '=' : '"';

            return !key.Equals(char.MinValue) &&
                (char.IsLetterOrDigit(key) || char.IsPunctuation(key) || char.IsWhiteSpace(key) || key.Equals(quoteChar));
        }

        /// <summary>
        /// Gets the active non-dismissed completion sessions.
        /// </summary>
        private ICompletionSession GetActiveSession()
        {
            return completionBroker.GetSessions(textView).FirstOrDefault(x => !x.IsDismissed);
        }

        /// <summary>
        /// Filter events
        /// </summary>
        int IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(serviceProvider))
            {
                return nextCommandTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }

            var typedChar = char.MinValue;
            //make sure the input is a char before getting it
            if ((pguidCmdGroup == VSConstants.VSStd2K) && (nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR))
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            // Handle Edit.ListMembers or Edit.CompleteWord commands
            if ((nCmdID == (uint) VSConstants.VSStd2KCmdID.SHOWMEMBERLIST ||
                 nCmdID == (uint) VSConstants.VSStd2KCmdID.COMPLETEWORD))
            {
                if (completionBroker.IsCompletionActive(textView))
                {
                    completionBroker.DismissAllSessions(textView);
                }

                ShowCompletion();

                return VSConstants.S_OK;
            }

            // Handle Enter/Tab commit keys
            if ((nCmdID == (uint) VSConstants.VSStd2KCmdID.RETURN) || (nCmdID == (uint) VSConstants.VSStd2KCmdID.TAB))
            {
                var completionSession = GetActiveSession();
                if (completionSession != null)
                {
                    if (completionSession.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        // Commit
                        completionSession.Commit();
                        // Don't add the character to the buffer
                        return VSConstants.S_OK;
                    }

                    // If there is no selection dismiss
                    completionSession.Dismiss();
                }
            }

            // Pass along command so the character is added to the buffer
            var result = this.nextCommandTarget.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            var handled = false;

            // Hanlde other keys
            if ((typedChar != Char.MinValue) && IsTriggerKey(typedChar))
            {
                var completionSession = GetActiveSession();
                if (completionSession == null)
                {
                    // Handle trigger keys
                    // Check if the typed char is a trigger
                    if (IsTriggerKey(typedChar))
                    {
                        ShowCompletion();
                        return result;
                    }
                }
                else
                {
                    // Session is active, filter.
                    completionSession.Filter();
                }
                handled = true;
            }
            // redo the filter if there is a deletion
            else if ((nCmdID == (uint) VSConstants.VSStd2KCmdID.BACKSPACE) || (nCmdID == (uint) VSConstants.VSStd2KCmdID.DELETE))
            {
                var completionSession = GetActiveSession();
                if (completionSession != null)
                {
                    completionSession.Filter();
                }
                handled = true;
            }
            return handled ? VSConstants.S_OK : result;
        }

        int IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == VSConstants.VSStd2K && cCmds > 0)
            {
                // completion commands should be available
                if (((uint)VSConstants.VSStd2KCmdID.SHOWMEMBERLIST == (uint)prgCmds[0].cmdID || (uint)VSConstants.VSStd2KCmdID.COMPLETEWORD == (uint)prgCmds[0].cmdID))
                {
                    prgCmds[0].cmdf = (int)Constants.MSOCMDF_ENABLED | (int)Constants.MSOCMDF_SUPPORTED;
                    return VSConstants.S_OK;
                }
            }

            return this.nextCommandTarget.QueryStatus(pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

    }
}
