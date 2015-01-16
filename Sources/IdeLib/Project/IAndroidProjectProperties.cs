using System.Collections.Generic;

namespace Dot42.Ide.Project
{
    public interface IAndroidProjectProperties
    {
        /// <summary>
        /// Name of the android package
        /// </summary>
        string PackageName { get; set; }

        /// <summary>
        /// Filename of apk file.
        /// </summary>
        string ApkFilename { get; set; }

        /// <summary>
        /// Do we generate an APK or a library?
        /// </summary>
        bool ApkOutputs { get; }

        /// <summary>
        /// Path of the signing certificate
        /// </summary>
        string ApkCertificatePath { get; set; }

        /// <summary>
        /// Thumbprint of the signing certificate
        /// </summary>
        string ApkCertificateThumbprint { get; set; }

        /// <summary>
        /// Android target version
        /// </summary>
        string TargetFrameworkVersion { get; set; }

        /// <summary>
        /// Android target SDK version
        /// </summary>
        string TargetSdkAndroidVersion { get; set; }

        /// <summary>
        /// Should WCF proxies be generated?
        /// </summary>
        bool GenerateWcfProxy { get; set; }

        /// <summary>
        /// Name of libraries included in this project.
        /// </summary>
        IEnumerable<string> ReferencedLibraryNames { get; }

        /// <summary>
        /// Add a reference to a library with given name.
        /// </summary>
        void AddReferencedLibrary(string name);

        /// <summary>
        /// Remove a reference to a library with given name.
        /// </summary>
        void RemoveReferencedLibrary(string name);
    }
}
