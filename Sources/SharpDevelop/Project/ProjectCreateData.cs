
using System;

namespace Dot42.SharpDevelop
{
	/// <summary>
	/// Description of ProjectCreateData.
	/// </summary>
	internal static class ProjectCreateData
	{
        /// <summary>
        /// Full path of certificate
        /// </summary>
        public static string CertificatePath { get; set; }

        /// <summary>
        /// Thumb print of certificate
        /// </summary>
        public static string CertificateThumbprint { get; set; }

        /// <summary>
        /// Version of target framework
        /// </summary>
        public static string TargetFrameworkVersion { get;set; }

        /// <summary>
        /// Name of additional libraries to include
        /// </summary>
        public static string[] AdditionalLibraryNames { get;set; }
	}
}
