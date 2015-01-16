using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

using Dot42.FrameworkDefinitions;
using Dot42.Utility;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to check if the Android version as set on the property
    /// page corresponds with the version in the AndroidManifest.xml file.
    /// </summary>
    public class CheckAndroidVersionInManifest : Task
    {
        /// <summary>
        /// Gets or sets the Minimum SDK Version as defined in the project.
        /// </summary>
        public ITaskItem MinSdkVersion { get; set; }

        /// <summary>
        /// Gets or sets the Target SDK Version as defined in the project.
        /// </summary>
        public ITaskItem TargetSdkVersion { get; set; }

        /// <summary>
        /// Gets or sets the location of the AndroidManifest.xml file to test.
        /// </summary>
        public ITaskItem AndroidManifest { get; set; }

        /// <summary>
        /// Gets or sets the location of the original manifest file as specified in the project.
        /// <remarks>
        /// The one we test is copied to the output folder, where the rest of the toolchain will
        /// add it in the package. So that is also the one we add the (by dot42.com) to the label.
        /// The test on the version should be made on the original file, or at least the error
        /// should point to the original file, otherwise the user will change the wrong file.
        /// Because of how we use MSBuild, this is an array with one entry. This has been checked
        /// in a previous build-step (CreateAndroidManifest).
        /// </remarks>
        /// </summary>
        public ITaskItem[] OriginalAndroidManifest { get; set; }

        /// <summary>
        /// Executes the MS build step for CheckAndroidVersionInManifest.
        /// </summary>
        /// <remarks>
        /// The errors numbers logged in the MSBuild log have the following meaning:
        /// 1: Error in parameters passed to the MSBuild task.
        /// 2: Error in the format of the AndroidManifest.xml file.
        /// 3: Versions in the project are not defined.
        /// 4: Versions in project are not the same as the versions in the manifest.
        /// </remarks>
        /// <returns></returns>
        public override bool Execute()
        {
            // Check AndroidManifest parameter.
            if (null == this.AndroidManifest)
            {
                this.LogError(1, "Android manifest not defined.");
                return false;
            }

            if (null == this.AndroidManifest.ItemSpec)
            {
                this.LogError(1, "Empty Android manifest defined.");
                return false;
            }

            // Check MinSdkVersion parameter.
            if (null == this.MinSdkVersion)
            {
                this.LogError(1, "Minimal SDK version not passed to task.");
                return false;
            }
            
            if (string.IsNullOrEmpty(this.MinSdkVersion.ItemSpec))
            {
                this.LogError(3, "Empty minimal SDK version passed to task.");
                return false;
            }
            
            string minSdkVersion = CheckAndroidVersionInManifest.FormatTargetSdkVersion(this.MinSdkVersion.ItemSpec);
            if (string.IsNullOrEmpty(minSdkVersion))
            {
                this.LogError(3, "Unknown minimal SDK version passed to task.");
                return false;
            }

            // Check TargetSdkVersion parameter.
            if (null == this.TargetSdkVersion)
            {
                this.LogError(1, "Target SDK version not passed to task.");
                return false;
            }
            
            if (string.IsNullOrEmpty(this.TargetSdkVersion.ItemSpec))
            {
                this.LogError(3, "Empty target SDK version passed to task.");
                return false;
            }
            
            string targetSdkVersion = CheckAndroidVersionInManifest.FormatTargetSdkVersion(this.TargetSdkVersion.ItemSpec);
            if (string.IsNullOrEmpty(targetSdkVersion))
            {
                this.LogError(3, "Unknown target SDK version passed to task.");
                return false;
            }

            // From here on all parameters contain correct values.
            // So we read the manifest and get the versions from it.
            XElement root = XElement.Load(this.AndroidManifest.ItemSpec, LoadOptions.SetLineInfo);

            if (null == root)
            {
                // The manifest could not be read, so we set a build error.
                this.LogError(2, "Android manifest could not be read.");
                return false;
            }

            IEnumerable<XElement> manifestVersion =
                from el in root.Elements("uses-sdk")
                select el;

            if (null == manifestVersion)
            {
                // The manifest contains no version info, so we set a build error.
                this.LogError(2, "Android manifest does not define SDK version.");
                return false;
            }

            switch (manifestVersion.Count<XElement>())
            {
                case 0:
                    this.LogError(2, "Android manifest contains no <uses-sdk> element.");
                    return false;
                case 1:
                    {
                        // There is 1 version element in the manifest. So far so good.
                        XElement el = manifestVersion.First<XElement>();
                        if (null == el)
                        {
                            // The only element is null. Very unlikely to occur.
                            this.LogError(2, "Android manifest contains empty version element.");
                            return false;
                        }

                        XAttribute minSdkAttributeInManifest = el.Attribute("{http://schemas.android.com/apk/res/android}minSdkVersion");
                        XAttribute targetSdkAttributeInManifest = el.Attribute("{http://schemas.android.com/apk/res/android}targetSdkVersion");

                        if (null == minSdkAttributeInManifest)
                        {
                            // The element does not contain a minSdkVersion attribute.
                            this.LogError(2, "Android manifest does not define minimal SDK version.");
                            return false;
                        }

                        if (string.IsNullOrEmpty(minSdkAttributeInManifest.Value))
                        {
                            // The element contains an empty minSdkVersion attribute.
                            this.LogError(2, "Android manifest does not define minimal SDK version.");
                            return false;
                        }

                        if (null == targetSdkAttributeInManifest)
                        {
                            // The element does not contain a targetSdkVersion attribute.
                            this.LogError(2, "Android manifest does not define target SDK version.");
                            return false;
                        }

                        if (string.IsNullOrEmpty(targetSdkAttributeInManifest.Value))
                        {
                            // The element contains an empty targetSdkVersion attribute.
                            this.LogError(2, "Android manifest does not define target SDK version.");
                            return false;
                        }

                        // Here we can finally check the versions.
                        if (minSdkAttributeInManifest.Value.Equals(minSdkVersion) && targetSdkAttributeInManifest.Value.Equals(targetSdkVersion))
                        {
                            return true;
                        }

                        // Add a Build Error if the minSdkVersion attribute does not have the correct value.
                        if (!minSdkAttributeInManifest.Value.Equals(minSdkVersion))
                        {
                            this.LogError(4, minSdkAttributeInManifest, "Android version numbers in manifest do not correspond with those in the project.");
                        }

                        // Add a Build Error if the targetSdkVersion attribute does not have the correct value.
                        if (!targetSdkAttributeInManifest.Value.Equals(targetSdkVersion))
                        {
                            this.LogError(4, targetSdkAttributeInManifest, "Android version numbers in manifest do not correspond with those in the project.");
                        }

                        return false;
                    }
                default:
                    // Add a Build Error for each uses-sdk element, so the user can easily find them.
                    foreach (XElement element in manifestVersion)
                    {
                        this.LogError(3, element, "Android manifest contains multiple <uses-sdk> elements.");
                    }

                    return false;
            }
        }

        private void LogError(int errorId, string file, int lineStart, int columnStart, int lineEnd, int columnEnd, string message)
        {
            Log.LogError(
                "Error",
                string.Format("D42-{0:0000}", errorId),
                string.Empty,
                file,
                lineStart,
                columnStart,
                lineEnd,
                columnEnd,
                message);
        }

        private void LogError(int errorId, XAttribute attribute, string message)
        {
            int lineStart = (attribute as IXmlLineInfo).LineNumber;
            int columnStart = (attribute as IXmlLineInfo).LinePosition + attribute.ToString().Length - attribute.Value.Length - 1;
            int lineEnd = lineStart;
            int columnEnd = columnStart + attribute.Value.Length;

            // OriginalAndroidManifest contains exactly one element. This has been checked in a previous build-step.
            this.LogError(errorId, this.OriginalAndroidManifest[0].ItemSpec, lineStart, columnStart, lineEnd, columnEnd, message);
        }

        private void LogError(int errorId, XElement element, string message)
        {
            int lineStart = (element as IXmlLineInfo).LineNumber;
            int columnStart = (element as IXmlLineInfo).LinePosition;
            int lineEnd = lineStart;
            int columnEnd = columnStart + element.ToString().Length;
            this.LogError(errorId, this.AndroidManifest.ItemSpec, lineStart, columnStart, lineEnd, columnEnd, message);
        }

        private void LogError(int errorId, string message)
        {
            this.LogError(errorId, this.AndroidManifest.ItemSpec, 1, 1, 1, 1, message);
        }

        /// <summary>
        /// Format a value specified in PackageAttribute.TargetSdkVersion towards an API level.
        /// </summary>
        private static string FormatTargetSdkVersion(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            
            int result;

            // Try api level first
            if (int.TryParse(value, out result))
                return result.ToString();

            // Match against known framework versions
            var framework = Frameworks.Instance.FirstOrDefault(x => x.Name == value);
            if (framework != null)
            {
                return framework.Descriptor.ApiLevel.ToString();
            }

            return string.Empty;
        }
    }
}
