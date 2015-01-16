namespace Dot42.VStudio.Wizards
{
    internal static class ProjectConstants
    {
#if ANDROID
        internal const string TargetValue = "Android";
        internal const string PackagExt = "apk";
        internal const string PackageFileNameTag = "ApkFilename";
#elif BB
        internal const string TargetValue = "BlackBerry";
        internal const string PackagExt = "bar";
        internal const string PackageFileNameTag = "BarFilename";
#endif
        internal const string ProjectTypeGuid = GuidList.Strings.guidDot42ProjectFlavorPackage;
    }
}
