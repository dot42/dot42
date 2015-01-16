using System.Collections.Generic;
using System.Linq;
using Dot42.Utility;
using Microsoft.Build.Framework;

namespace Dot42.MSBuild.Tasks
{
    /// <summary>
    /// Task used to create an APK/BAR file.
    /// </summary>
    public class BuildPackage : Dot42CompilerTask
    {
        /// <summary>
        /// Compiled AndroidManifest.xml
        /// </summary>
        [Required]
        public ITaskItem Manifest { get; set; }

        /// <summary>
        /// Assembly from which to include embedded resources
        /// </summary>
        [Required]
        public ITaskItem[] Assemblies { get; set; }

        /// <summary>
        /// Output package (.apk)
        /// </summary>
        [Required]
        public ITaskItem Package { get; set; }

        /// <summary>
        /// Dex files to include
        /// </summary>
        [Required]
        public ITaskItem[] DexFiles { get; set; }

        /// <summary>
        /// Map files to include
        /// </summary>
        public ITaskItem[] MapFiles { get; set; }

        /// <summary>
        /// Folder containing all precompiled and raw resources
        /// </summary>
        public ITaskItem ResourcesFolder { get; set; }

        /// <summary>
        /// Code signing certificate path
        /// </summary>
        public ITaskItem Certificate { get; set; }

        /// <summary>
        /// Code signing certificate password
        /// </summary>
        public string CertificatePassword { get; set; }

        /// <summary>
        /// Code signing certificate thumbprint
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Path of Free Apps Key file
        /// </summary>
        public ITaskItem FreeAppsKeyPath { get; set; }

        /// <summary>
        /// Package name of the APK
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// Path of the debug token (if any)
        /// </summary>
        public ITaskItem DebugToken { get; set; }

        /// <summary>
        /// All native code libs
        /// </summary>
        public ITaskItem[] NativeCodeLibraries { get; set; }

        protected override string[] GenerateArguments()
        {
            var builder = new List<string>();

            if (Package != null)
            {
                builder.Add(ToolOptions.OutputPackage.AsArg());
                builder.Add(Package.ItemSpec);
            }

            if (Manifest != null)
            {
                builder.Add(ToolOptions.InputManifest.AsArg());
                builder.Add(Manifest.ItemSpec);
            }

            if (Assemblies != null)
            {
                foreach (var asm in Assemblies)
                {
                    builder.Add(ToolOptions.InputAssembly.AsArg());
                    builder.Add(asm.ItemSpec);
                }
            }

            if (ResourcesFolder != null)
            {
                builder.Add(ToolOptions.ResourcesFolder.AsArg());
                builder.Add(ResourcesFolder.ItemSpec);
            }

            if (DexFiles != null)
            {
                foreach (var x in DexFiles.Where(x => x != null))
                {
                    builder.Add(ToolOptions.InputCodeFile.AsArg());
                    builder.Add(x.ItemSpec);
                }
            }

            if (NativeCodeLibraries != null)
            {
                foreach (var x in NativeCodeLibraries.Where(x => x != null))
                {
                    builder.Add(ToolOptions.NativeCodeLibrary.AsArg());
                    builder.Add(x.ItemSpec);
                }
            }

            if (MapFiles != null)
            {
                foreach (var x in MapFiles.Where(x => x != null))
                {
                    builder.Add(ToolOptions.InputMapFile.AsArg());
                    builder.Add(x.ItemSpec);
                }
            }

            if (Certificate != null)
            {
                builder.Add(ToolOptions.CertificatePath.AsArg());
                builder.Add(Certificate.ItemSpec);
            }

            if (!string.IsNullOrEmpty(CertificatePassword))
            {
                builder.Add(ToolOptions.CertificatePassword.AsArg());
                builder.Add(CertificatePassword);
            }

            if (!string.IsNullOrEmpty(CertificateThumbprint))
            {
                builder.Add(ToolOptions.CertificateThumbprint.AsArg());
                builder.Add(CertificateThumbprint);
            }

            if (FreeAppsKeyPath != null)
            {
                builder.Add(ToolOptions.FreeAppsKeyPath.AsArg());
                builder.Add(FreeAppsKeyPath.ItemSpec);
            }

            if (DebugToken != null)
            {
                builder.Add(ToolOptions.DebugToken.AsArg());
                builder.Add(DebugToken.ItemSpec);
            }

            if (!string.IsNullOrEmpty(PackageName))
            {
                builder.Add(ToolOptions.PackageName.AsArg());
                builder.Add(PackageName);
            }

            builder.AddTarget();
            return builder.ToArray();
        }
    }
}
