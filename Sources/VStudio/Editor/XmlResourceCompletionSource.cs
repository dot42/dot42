using System;
using System.Collections.Generic;
using System.Linq;
using Dot42.Ide.Descriptors;
using Dot42.VStudio.Shared;
using Dot42.VStudio.XmlEditor;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using CompletionSet = Microsoft.VisualStudio.Language.Intellisense.CompletionSet;

namespace Dot42.VStudio.Editor
{
    /// <summary>
    /// Completion source for specific type of XML resources.
    /// </summary>
    internal abstract class XmlResourceCompletionSource : ICompletionSource
    {
        private bool isDisposed;
        private readonly ITextBuffer textBuffer;
        private readonly IGlyphService glyphService;
        private readonly SVsServiceProvider serviceProvider;
        private readonly IVsEditorAdaptersFactoryService vsEditorAdaptersFactoryService;
        private DescriptorProvider descriptorProvider;

        /// <summary>
        /// Default ctor
        /// </summary>
        protected XmlResourceCompletionSource(ITextBuffer textBuffer, SVsServiceProvider serviceProvider, IVsEditorAdaptersFactoryService vsEditorAdaptersFactoryService, IGlyphService glyphService)
        {
            this.textBuffer = textBuffer;
            this.serviceProvider = serviceProvider;
            this.vsEditorAdaptersFactoryService = vsEditorAdaptersFactoryService;
            this.glyphService = glyphService;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        void IDisposable.Dispose()
        {
            if (!isDisposed)
            {
                GC.SuppressFinalize(this);
                isDisposed = true;
            }
        }

        /// <summary>
        /// Determines which <see cref="T:Microsoft.VisualStudio.Language.Intellisense.CompletionSet"/>s should be part of the specified <see cref="T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSession"/>.
        /// </summary>
        /// <param name="session">The session for which completions are to be computed.</param><param name="completionSets">The set of the completionSets to be added to the session.</param>
        /// <remarks>
        /// Each applicable <see cref="M:Microsoft.VisualStudio.Language.Intellisense.ICompletionSource.AugmentCompletionSession(Microsoft.VisualStudio.Language.Intellisense.ICompletionSession,System.Collections.Generic.IList{Microsoft.VisualStudio.Language.Intellisense.CompletionSet})"/> instance will be called in-order to
        ///             (re)calculate a <see cref="T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSession"/>.  <see cref="T:Microsoft.VisualStudio.Language.Intellisense.CompletionSet"/>s can be added to the session by adding
        ///             them to the completionSets collection passed-in as a parameter.  In addition, by removing items from the collection, a
        ///             source may filter <see cref="T:Microsoft.VisualStudio.Language.Intellisense.CompletionSet"/>s provided by <see cref="T:Microsoft.VisualStudio.Language.Intellisense.ICompletionSource"/>s earlier in the calculation
        ///             chain.
        /// </remarks>
        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            //var context = session.TextView.Properties[typeof(XmlResourceCompletionContext)] as XmlResourceCompletionContext;
            //if (context == null)
                //return;
            var editorService = XmlEditorServiceProvider.GetEditorService(serviceProvider, Dot42Package.DteVersion);
            var xls = editorService.GetLanguageService();
            var vsTextView = vsEditorAdaptersFactoryService.GetViewAdapter(session.TextView);

            int line;
            int column;
            vsTextView.GetCaretPos(out line, out column);
            IVsTextLines buffer;
            ErrorHandler.ThrowOnFailure(vsTextView.GetBuffer(out buffer));
            var source = xls.GetSource(buffer);
            var tokenInfo = source.GetTokenInfo(line, column);
            if ((!tokenInfo.IsStartOfTag) && (!tokenInfo.IsWhitespace) && (!tokenInfo.IsStringLiteral) && (!tokenInfo.IsStartStringLiteral))
                return;

            var doc = xls.GetParseTree(source, vsTextView, line, column);
            var nf = doc.SearchForNodeAtPosition(line, column);

            var attachCommitted = false;
            if (nf.IsXmlStartTag)
            {
                if (tokenInfo.IsStartOfTag)
                {
                    // New element
                    var descriptors = GetElementDescriptors(nf.GetParentPath(), true);
                    completionSets.Clear();
                    completionSets.Add(CreateCompletionSet(CreateElementCompletions(descriptors), session, null));
                    attachCommitted = true;
                }
                else if (tokenInfo.IsWhitespace)
                {
                    // New attribute
                    var descriptor = GetElementDescriptors(nf.GetPath(), false).FirstOrDefault();
                    if (descriptor != null)
                    {
                        completionSets.Clear();
                        completionSets.Add(CreateCompletionSet(CreateAttributeCompletions(descriptor), session, null));
                        attachCommitted = true;
                    }
                }
            }
            else if (nf.IsElement)
            {
                //var element = (XmlElement) nf.Scope;
            }
            else if (nf.IsAttribute)
            {
                // Attribute value
                var descriptor = GetAttributeDescriptor(nf.GetParentPath(), nf.GetLocalName());
                if ((descriptor != null) && (descriptor.HasStandardValues))
                {
                    completionSets.Clear();
                    completionSets.Add(CreateCompletionSet(CreateAttributeValueCompletions(descriptor), session, tokenInfo));
                    attachCommitted = true;
                }
            }

            if (attachCommitted)
            {
                session.Committed += OnSessionCommitted;
            }
        }

        /// <summary>
        /// A completion session has been committed.
        /// </summary>
        private static void OnSessionCommitted(object sender, EventArgs e)
        {
            var session = (ICompletionSession) sender;
            var completion = session.SelectedCompletionSet.SelectionStatus.Completion as XmlResourceCompletion;
            if (completion == null)
                return;
            var textView = session.TextView;
            var moveBackPosition = completion.MoveBackPositions;
            while (moveBackPosition > 0)
            {
                textView.Caret.MoveToPreviousCaretPosition();
                moveBackPosition--;
            }
        }

        /// <summary>
        /// Gets all element descriptors that are valid at the given document node.
        /// </summary>
        private IEnumerable<ElementDescriptor> GetElementDescriptors(List<string> path, bool getChildren)
        {
            IElementDescriptorProvider provider = DescriptorProvider;
            if (provider == null)
                return Enumerable.Empty<ElementDescriptor>();
            if ((path == null) || (path.Count == 0))
                return provider.Descriptors;

            // Walk towards the given node, as element descriptor provider.
            foreach (var name in path)
            {
                provider = provider.Descriptors.FirstOrDefault(x => x.Name == name);
                if (provider == null)
                    return Enumerable.Empty<ElementDescriptor>();
            }
            if (getChildren)
            {
                return provider.Descriptors;
            }
            var descriptor = provider as ElementDescriptor;
            if (descriptor == null)
                return Enumerable.Empty<ElementDescriptor>();
            return new[] { descriptor };
        }

        /// <summary>
        /// Gets the attribute descriptor that is valid for the given attribute.
        /// </summary>
        private AttributeDescriptor GetAttributeDescriptor(List<string> path, string localName)
        {
            IElementDescriptorProvider provider = DescriptorProvider;
            if (provider == null)
                return null;
            var nodeDescriptor = GetElementDescriptors(path, false).FirstOrDefault();
            if (nodeDescriptor == null)
                return null;
            return nodeDescriptor.Attributes.FirstOrDefault(x => x.Name == localName);
        }

        /// <summary>
        /// Gets the descriptor provider to use for this kind of resources.
        /// </summary>
        /// <returns></returns>
        protected abstract DescriptorProvider GetDescriptorProvider(DescriptorProviderSet providerSet);

        /// <summary>
        /// Gets the descriptor provider to use for this kind of resources.
        /// </summary>
        private DescriptorProvider DescriptorProvider
        {
            get
            {
                if (descriptorProvider == null)
                {
                    // Load the provider set
                    var folder = GetFrameworkFolder();
                    var descriptorProviderSet = Descriptors.GetDescriptorProviderSet(folder);
                    descriptorProvider = GetDescriptorProvider(descriptorProviderSet);
                }
                return descriptorProvider;
            }
        }

        /// <summary>
        /// Load the framework folder of the current project.
        /// </summary>
        private string GetFrameworkFolder()
        {
            var projectItem = textBuffer.GetProjectItem(serviceProvider);
            if (projectItem == null)
                return string.Empty;
            var project = projectItem.ContainingProject;
            var msBuildProject = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.GetLoadedProjects(project.FullName).FirstOrDefault();
            if (msBuildProject == null)
                return string.Empty;
            var folder = msBuildProject.GetPropertyValue("TargetFrameworkDirectory");
            return folder ?? string.Empty;
        }

        /// <summary>
        /// Create completions for element names.
        /// </summary>
        private IEnumerable<XmlResourceCompletion> CreateElementCompletions(IEnumerable<ElementDescriptor> elementDescriptors)
        {
            var provider = DescriptorProvider;
            if (provider == null)
                return Enumerable.Empty<XmlResourceCompletion>();
            var names = elementDescriptors.Select(x => x.Name);
            return names.OrderBy(x => x).Select(x => new XmlResourceCompletion(glyphService, x, string.Format("<{0}></{0}>", x), x, x.Length + 3, XmlResourceCompletionType.Element));
        }

        /// <summary>
        /// Create completions for attribute names.
        /// </summary>
        private IEnumerable<XmlResourceCompletion> CreateAttributeCompletions(ElementDescriptor elementDescriptor)
        {
            var provider = DescriptorProvider;
            if (provider == null)
                return Enumerable.Empty<XmlResourceCompletion>();
            var names = elementDescriptor.Attributes.Select(x => x.Name);
            var prefix = "android";
            return names.OrderBy(x => x).Select(x => new XmlResourceCompletion(glyphService, x, string.Format("{0}:{1}=\"\"", prefix, x), x, 1, XmlResourceCompletionType.Attribute));
        }

        /// <summary>
        /// Create completions for attribute names.
        /// </summary>
        private IEnumerable<XmlResourceCompletion> CreateAttributeValueCompletions(AttributeDescriptor attributeDescriptor)
        {
            var provider = DescriptorProvider;
            if (provider == null)
                return Enumerable.Empty<XmlResourceCompletion>();
            var names = attributeDescriptor.StandardValues.Select(x => x.Value);
            return names.OrderBy(x => x).Select(x => new XmlResourceCompletion(glyphService, x, x, x, -1, XmlResourceCompletionType.Attribute));
        }

        /// <summary>
        /// Create a new completion set for the given completions.
        /// </summary>
        private CompletionSet CreateCompletionSet(IEnumerable<XmlResourceCompletion> completions, ICompletionSession session, ITokenInfo tokenInfo)
        {
            return new XmlResourceCompletionSet("Dot42", "Dot42",
                CreateTrackingSpan(session, tokenInfo),
                completions,
                null);
        }

        /// <summary>
        /// Create a tracking span that is used in a completesion set in the given session.
        /// </summary>
        private ITrackingSpan CreateTrackingSpan(ICompletionSession session, ITokenInfo tokenInfo)
        {
            var triggerPoint = session.GetTriggerPoint(session.TextView.TextBuffer);
            var currentSnapshot = textBuffer.CurrentSnapshot;
            if (tokenInfo != null)
            {
                var textViewLine = session.TextView.GetTextViewLineContainingBufferPosition(triggerPoint.GetPoint(currentSnapshot));
                var start = textViewLine.Start.Position;
                var first = start + tokenInfo.StartIndex;
                int length;
                if (tokenInfo.IsStartStringLiteral)
                {
                    length = 0;
                    first++;
                }
                else
                {
                    length = tokenInfo.EndIndex - tokenInfo.StartIndex + 1;                    
                }
                return currentSnapshot.CreateTrackingSpan(first, length, SpanTrackingMode.EdgeInclusive);
            }
            else
            {
                var position = triggerPoint.GetPosition(currentSnapshot);
                var separators = new[] {'"', '\'', '.', '>', '<', ' '};
                var text = currentSnapshot.GetText();
                var first = (text[position - 1] == ' ')
                                ? position
                                : text.Substring(0, position).LastIndexOfAny(separators);

                return currentSnapshot.CreateTrackingSpan(first, position - first, SpanTrackingMode.EdgeInclusive);
            }
        }
    }
}
