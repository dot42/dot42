using System.Xml.Linq;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const string InstrumentationAttribute = "InstrumentationAttribute";

        /// <summary>
        /// Create all instrumentation elements
        /// </summary>
        private void CreateInstrumentation(XElement manifest)
        {
            foreach (var attr in assembly.GetAttributes(InstrumentationAttribute))
            {
                var name = attr.GetValue(-1, "Name", "android.test.InstrumentationTestRunner");
                var functionalTest = attr.GetValue(-1, "FunctionalTest", false);
                var handleProfiling = attr.GetValue(-1, "HandleProfiling", false);
                var label = attr.GetValue<string>(-1, "Label");
                var targetPackage = attr.GetValue(-1, "TargetPackage", packageName);

                var element = new XElement("instrumentation");
                element.Add(new XAttribute(XName.Get("name", Namespace), name));
                element.AddAttrIfNotDefault("functionalTest", Namespace, functionalTest, false);
                element.AddAttrIfNotDefault("handleProfiling", Namespace, handleProfiling, false);
                element.AddAttrIfNotEmpty("label", Namespace, label);
                element.AddAttrIfNotEmpty("targetPackage", Namespace, targetPackage);

                manifest.Add(element);
            }
        }
    }
}
