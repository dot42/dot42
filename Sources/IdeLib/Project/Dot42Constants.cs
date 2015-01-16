namespace Dot42.Ide.Project
{
    public static class Dot42Constants
    {
        // Target framework
        public const string TargetFrameworkIdentifier = "Dot42";

        // MSBuild item types
        public const string ItemTypeManifestResource = "ManifestResource";
        public const string ItemTypeAnimationResource = "AnimationResource";
        public const string ItemTypeDrawableResource = "DrawableResource";
        public const string ItemTypeLayoutResource = "LayoutResource";
        public const string ItemTypeMenuResource = "MenuResource";
        public const string ItemTypeValuesResource = "ValuesResource";
        public const string ItemTypeXmlResource = "XmlResource";
        public const string ItemTypeRawResource = "RawResource";
        public const string ItemTypeJarReference = "JarReference";

        /// <summary>
        /// All Dot42 specific android resource item types.
        /// </summary>
        public static readonly string[] ResourceItemTypes = new[] {
            ItemTypeAnimationResource,
            ItemTypeDrawableResource,
            ItemTypeLayoutResource,
            ItemTypeMenuResource,
            ItemTypeRawResource,
            ItemTypeValuesResource,
            ItemTypeXmlResource
        };

        // MSBuild property names
        public const string PropApkCertificatePath = "ApkCertificatePath";
        public const string PropApkCertificateThumbprint = "ApkCertificateThumbprint";
        public const string PropApkFilename = "ApkFilename";
        public const string PropApkOutputs = "ApkOutputs";
        public const string PropPackageName = "PackageName";
        public const string PropTargetFrameworkVersion = "TargetFrameworkVersion";
        public const string PropTargetSdkAndroidVersion = "TargetSdkAndroidVersion";
        public const string PropGenerateWcfProxy = "GenerateWcfProxy";
        public const string PropGenerateAndroidManifest = "GenerateAndroidManifest";
        public const string PropAndroidManifestFile = "AndroidManifestFile";
        
        // Project sub type
        public const string Dot42ProjectType = "{337B7DB7-2D1E-448D-BEBF-17E887A46E37}";
    }
}
