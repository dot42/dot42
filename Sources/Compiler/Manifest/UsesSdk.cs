using System;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using Dot42.CompilerLib;
using Dot42.FrameworkDefinitions;
using Mono.Cecil;

namespace Dot42.Compiler.Manifest
{
    /// <summary>
    /// Create AndroidManifest.xml.
    /// </summary>
    partial class ManifestBuilder
    {
        private const int MinApiLevel = 3;

        /// <summary>
        /// Compile the given XML file to a binary XML file in the given output folder.
        /// </summary>
        private void CreateUsesSdk(XElement manifest, CustomAttribute packageAttribute)
        {
            // Create uses-sdk
            var usesSdk = new XElement("uses-sdk", new XAttribute(XName.Get("minSdkVersion", Namespace), FindMinSdkLevel().ToString()));
            if (!string.IsNullOrEmpty(targetSdkVersion))
            {
                usesSdk.Add(new XAttribute(XName.Get("targetSdkVersion", Namespace), FormatTargetSdkVersion(targetSdkVersion)));
            }
            manifest.Add(usesSdk);
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
            throw new CompilerException("Unknown target sdk version " + value);
        }

        /// <summary>
        /// Locate the API level of the targeted SDK.
        /// </summary>
        private int FindMinSdkLevel()
        {
            var sdkRef = assembly.MainModule.AssemblyReferences.FirstOrDefault(x => string.Equals(x.Name, AssemblyConstants.SdkAssemblyName, StringComparison.OrdinalIgnoreCase));
            if (sdkRef == null)
                return MinApiLevel;
            var sdkInfo = Frameworks.Instance.FirstOrDefault(x => x.MatchesFrameworkVersion(sdkRef.Version));
            if (sdkInfo == null)
                return MinApiLevel;
            return sdkInfo.Descriptor.ApiLevel;
        }
    }
}
