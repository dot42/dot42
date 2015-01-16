namespace Dot42.Gui
{
    internal static class SdkConstants
    {
        public const string NamespaceVersion = "7";

        /// <summary>
        /// Namespace used in Repository file.
        /// </summary>
        public const string Namespace = "http://schemas.android.com/sdk/android/repository/" + NamespaceVersion;

        /// <summary>
        /// URL of the SDK repository download site.
        /// </summary>
        public const string DownloadRoot = "https://dl-ssl.google.com/android/repository/";

        /// <summary>
        /// URL of the SDK repository download site.
        /// </summary>
        public const string RepositoryUrl = DownloadRoot + "repository-" + NamespaceVersion + ".xml";

        /// <summary>
        /// Node name of system image nodes in a repository file.
        /// </summary>
        public const string KeySystemImage = "system-image";
    }
}
