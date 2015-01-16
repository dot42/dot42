using System;
using System.Linq;
using System.Xml.Linq;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string SupportsScreensAttribute = "SupportsScreensAttribute";
        
        /// <summary>
        /// Create the supports-screens element
        /// </summary>
        private void CreateSupportsScreens(XElement manifest)
        {
            // Find supports-screens attribute
            var ssAttributes = assembly.GetAttributes(SupportsScreensAttribute).ToList();
            if (ssAttributes.Count == 0)
                return;
            if (ssAttributes.Count > 1)
                throw new ArgumentException("Multiple SupportsScreens attributes found");

            var attr = ssAttributes[0];
            // Create supports-screens
            var supportsScreens = new XElement("supports-screens");
            manifest.Add(supportsScreens);
            supportsScreens.AddAttrIfFound("smallScreens", Namespace, attr, "SmallScreens");
            supportsScreens.AddAttrIfFound("normalScreens", Namespace, attr, "NormalScreens");
            supportsScreens.AddAttrIfFound("largeScreens", Namespace, attr, "LargeScreens");
            supportsScreens.AddAttrIfFound("xlargeScreens", Namespace, attr, "XLargeScreens");
            supportsScreens.AddAttrIfNotDefault("requiresSmallestWidthDp", Namespace, attr.GetValue<int>("RequiresSmallestWidthDp"), 0, x => x.ToString());
            supportsScreens.AddAttrIfNotDefault("compatibleWidthLimitDp", Namespace, attr.GetValue<int>("CompatibleWidthLimitDp"), 0, x => x.ToString());
            supportsScreens.AddAttrIfNotDefault("largestWidthLimitDp", Namespace, attr.GetValue<int>("LargestWidthLimitDp"), 0, x => x.ToString());
        }
    }
}
