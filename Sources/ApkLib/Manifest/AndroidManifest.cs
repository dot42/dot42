using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dot42.Utility;

namespace Dot42.ApkLib.Manifest
{
    public class AndroidManifest
    {
        private readonly XElement root;

        /// <summary>
        /// Default ctor
        /// </summary>
        internal AndroidManifest(XElement root)
        {
            this.root = root;
        }

        /// <summary>
        /// Gets the package name.
        /// </summary>
        public string PackageName
        {
            get { return root.GetAttribute("package"); }
        }

        /// <summary>
        /// Gets the android:versionName value.
        /// </summary>
        public string VersionName
        {
            get { return root.GetAttribute(XName.Get("versionName", AndroidConstants.AndroidNamespace)); }
        }

        /// <summary>
        /// Gets the label of the application element.
        /// </summary>
        public string ApplicationLabel
        {
            get
            {
                var app = root.Element("application");
                if (app == null) return string.Empty;
                return app.GetAttribute(XName.Get("label", AndroidConstants.AndroidNamespace));
            }
        }


        /// <summary>
        /// Gets the icon of the application element.
        /// </summary>
        public string ApplicationIcon
        {
            get
            {
                var app = root.Element("application");
                if (app == null) return string.Empty;
                return app.GetAttribute(XName.Get("icon", AndroidConstants.AndroidNamespace));
            }
        }

        /// <summary>
        /// Try to load the minimum SDK version required for the package.
        /// </summary>
        public bool TryGetMinSdkVersion(out int minSdkVersion)
        {
            minSdkVersion = -1;
            var usesSdk = root.Element("uses-sdk");
            if (usesSdk == null)
                return false;
            var attr = usesSdk.GetAttribute(XName.Get("minSdkVersion", AndroidConstants.AndroidNamespace));
            return !string.IsNullOrEmpty(attr) && int.TryParse(attr, out minSdkVersion);
        }

        /// <summary>
        /// Gets all activities
        /// </summary>
        public IEnumerable<Activity> Activities
        {
            get { return root.Descendants("activity").Select(x => new Activity(x)); }
        }

        /// <summary>
        /// Gets all uses-permission elements
        /// </summary>
        public IEnumerable<UsesPermission> UsesPermissions
        {
            get { return root.Descendants("uses-permission").Select(x => new UsesPermission(x)); }
        }
    }
}
