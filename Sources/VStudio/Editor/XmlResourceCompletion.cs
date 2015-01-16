using System;
using System.Windows.Media;
using Microsoft.VisualStudio.Language.Intellisense;

namespace Dot42.VStudio.Editor
{
    /// <summary>
    /// Defines an item in a completion set.
    /// </summary>
    internal sealed class XmlResourceCompletion : Completion, IComparable<XmlResourceCompletion>
    {
        /// <summary>
        /// Default ctor
        /// </summary>
        internal XmlResourceCompletion(IGlyphService glyphService, string displayText, string insertionText, string description, int moveBackPositions, XmlResourceCompletionType type)
            : base(insertionText)
        {
            DisplayText = displayText;
            InsertionText = insertionText;
            Description = description;
            IconSource = GetIconSource(glyphService, type);
            MoveBackPositions = moveBackPositions;
            Type = type;
        }

        /// <summary>
        /// Gets the icon for this completion.
        /// </summary>
        private static ImageSource GetIconSource(IGlyphService glyphService, XmlResourceCompletionType type)
        {
            StandardGlyphGroup stdGlyph;
            switch (type)
            {
                case XmlResourceCompletionType.Element:
                    stdGlyph = StandardGlyphGroup.GlyphXmlChild;
                    break;
                case XmlResourceCompletionType.Attribute:
                    stdGlyph = StandardGlyphGroup.GlyphXmlAttribute;
                    break;
                default:
                    stdGlyph = StandardGlyphGroup.GlyphXmlItem;
                    break;
            }
            return glyphService.GetGlyph(stdGlyph, StandardGlyphItem.GlyphItemPublic);
        }

        /// <summary>
        /// Number of positions to move back when this completion is committed.
        /// </summary>
        public int MoveBackPositions { get; private set; }

        /// <summary>
        /// Type of completion
        /// </summary>
        internal XmlResourceCompletionType Type { get; private set; }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(XmlResourceCompletion other)
        {
            return String.CompareOrdinal(DisplayText, other.DisplayText);
        }
    }
}