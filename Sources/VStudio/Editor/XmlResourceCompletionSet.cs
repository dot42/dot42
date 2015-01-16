using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Dot42.VStudio.Editor
{
    /// <summary>
    /// XML resource specific completion set.
    /// </summary>
    internal sealed class XmlResourceCompletionSet : CompletionSet
    {
        private string filterBufferText;
        private readonly FilteredObservableCollection<Completion> filteredCompletionBuilders;
        private readonly FilteredObservableCollection<Completion> filteredCompletions;

        /// <summary>
        /// Gets or sets the list of completion builders that are part of this completion set.  
        /// </summary>
        /// <remarks>
        /// Completion builders are completions that are displayed separately from the other completions in the completion set.
        ///             In the default presentation, completion builders appear in a non-scrolled list above the scrolled list of completions.
        /// </remarks>
        public override IList<Completion> CompletionBuilders
        {
            get { return filteredCompletionBuilders; }
        }

        /// <summary>
        /// Gets or sets the list of completions that are part of this completion set.
        /// </summary>
        public override IList<Completion> Completions
        {
            get { return filteredCompletions; }
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public XmlResourceCompletionSet(string moniker, string displayName, ITrackingSpan applicableTo, IEnumerable<Completion> completions, IEnumerable<Completion> completionBuilders)
            : base(moniker, displayName, applicableTo, completions, completionBuilders)
        {
            filteredCompletions = new FilteredObservableCollection<Completion>(WritableCompletions);
            filteredCompletionBuilders = new FilteredObservableCollection<Completion>(WritableCompletionBuilders);
        }

        /// <summary>
        /// Filters the set of completions to those that match the applicability text of the completion set and determines the best match.
        /// </summary>
        public override void Filter()
        {
            var currentSnapshot = ApplicableTo.TextBuffer.CurrentSnapshot;
            filterBufferText = ApplicableTo.GetText(currentSnapshot).Trim();
            if (string.IsNullOrEmpty(filterBufferText))
            {
                filteredCompletions.StopFiltering();
                filteredCompletionBuilders.StopFiltering();
            }
            else
            {
                filteredCompletions.Filter(DoesCompletionMatchApplicabilityText);
                filteredCompletionBuilders.Filter(DoesCompletionMatchApplicabilityText);
            }
        }

        private bool DoesCompletionMatchApplicabilityText(Completion completion)
        {
            if (IsUpper(filterBufferText))
            {
                return GetUpperString(completion.DisplayText).StartsWith(this.filterBufferText);
            }

            return completion.DisplayText.ToLowerInvariant().Contains(this.filterBufferText.ToLowerInvariant());
        }

        public static bool IsUpper(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsLower(value[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetUpperString(string value)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]))
                {
                    sb.Append(value[i]);
                }
            }
            return sb.ToString();
        } 
    }
}
