using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dot42.ApkBuilder;
using Dot42.ApkLib;
using Dot42.ApkLib.Manifest;
using Dot42.BarLib;
using Org.BouncyCastle.Utilities.Encoders;

namespace Dot42.Compiler.BarBuilder
{
    /// <summary>
    /// Builder of the META-INF/MANIFEST.MF file
    /// </summary>
    internal sealed class MetaInfManifestBuilder : MetaInfBuilder
    {
        private static readonly Dictionary<string, string> permissionMap = new Dictionary<string, string> {
            { "ANDROID.PERMISSION.ACCESS_COARSE_LOCATION", "read_geolocation" },
            { "ANDROID.PERMISSION.ACCESS_FINE_LOCATION", "read_geolocation" },
            { "ANDROID.PERMISSION.ACCESS_LOCATION_EXTRA_COMMANDS", "read_geolocation" },
            { "ANDROID.PERMISSION.ACCESS_MOCK_LOCATION", "read_geolocation" },
            { "ANDROID.PERMISSION.CONTROL_LOCATION_UPDATES", "read_geolocation" },
            { "ANDROID.PERMISSION.CAMERA", "use_camera" },
            { "ANDROID.PERMISSION.INTERNET", "access_internet" },
            { "ANDROID.PERMISSION.MODIFY_AUDIO_SETTINGS", "set_audio_volume" },
            { "ANDROID.PERMISSION.READ_CONTACTS", "access_pimdomain_contacts" },
            { "ANDROID.PERMISSION.WRITE_CONTACTS", "access_pimdomain_contacts" },
            { "ANDROID.PERMISSION.RECORD_AUDIO", "record_audio" }
        };

        /// <summary>
        /// Default ctor
        /// </summary>
        public MetaInfManifestBuilder(ApkFile apk, string author, string debugTokenPath, Dictionary<string, string> apkPath2barPath)
        {
            // Prepare
            var developmentMode = !string.IsNullOrEmpty(debugTokenPath);            
            var debugToken = developmentMode ? new BarFile(debugTokenPath) : null;
            if (debugToken != null)
            {
                // Verify debug token
                if (!string.Equals(debugToken.Manifest.PackageType, "debug-token", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(string.Format("Debug token {0} has invalid package type", debugTokenPath));
                if (string.IsNullOrEmpty(debugToken.Manifest.PackageAuthor))
                    throw new ArgumentException(string.Format("Debug token {0} has no package author", debugTokenPath));
                if (string.IsNullOrEmpty(debugToken.Manifest.PackageAuthorId))
                    throw new ArgumentException(string.Format("Debug token {0} has no package author id", debugTokenPath));
            }

            // Create manifest
            AppendLine("Archive-Manifest-Version: 1.1");
            AppendLine("Created-By: dot42");
            AppendLine();

            var androidManifest = apk.Manifest;
            if (developmentMode)
            {
                AppendLine("Package-Author: " + debugToken.Manifest.PackageAuthor);
                AppendLine("Package-Author-Id: " + debugToken.Manifest.PackageAuthorId);
            }
            else
            {
                AppendLine("Package-Author: " + author);
                AppendLine("Package-Author-Id: " + FormatID(author));
            }
            AppendLine("Package-Name: " + androidManifest.PackageName);
            AppendLine("Package-Id: " + FormatID(androidManifest.PackageName));
            AppendLine("Package-Version: " + androidManifest.VersionName);
            AppendLine("Package-Version-Id: " + FormatID(androidManifest.VersionName));
            AppendLine("Package-Type: application");
            AppendLine();

            AppendLine("Application-Name: " + androidManifest.ApplicationLabel);
            AppendLine("Application-Id: " + FormatID(androidManifest.ApplicationLabel));
            AppendLine("Application-Version: " + androidManifest.VersionName);
            AppendLine("Application-Version-Id: " + FormatID(androidManifest.VersionName));
            if (developmentMode)
            {
                AppendLine("Application-Development-Mode: true");
            }
            AppendLine();

            // Collect permissions
            var permissions = new HashSet<string> { "access_shared", "play_audio" };
            foreach (var permission in androidManifest.UsesPermissions)
            {
                string bbPerm;
                var key = (permission.Name ?? string.Empty).ToUpperInvariant();
                if (permissionMap.TryGetValue(key, out bbPerm))
                    permissions.Add(bbPerm);
            }
            var userActions = string.Join(",", permissions);

            var entryPoint = GetEntryPoint(androidManifest);
            if (entryPoint != null)
            {
                AppendLine("Entry-Point-Name: " + androidManifest.ApplicationLabel);
                AppendLine("Entry-Point-Type: Qnx/Android");
                AppendLine("Entry-Point: " + string.Format("android://{0}?activity-name={1}", androidManifest.PackageName, entryPoint.Name));
                if (!string.IsNullOrEmpty(userActions))
                {
                    AppendLine("Entry-Point-User-Actions: " + userActions);                    
                }
                var icon = entryPoint.Icon;
                if (string.IsNullOrEmpty(icon)) icon = androidManifest.ApplicationIcon;
                if (!string.IsNullOrEmpty(icon))
                {
                    string iconBarPath;
                    if (apkPath2barPath.TryGetValue(icon, out iconBarPath))
                    {
                        AppendLine("Entry-Point-Icon: " + iconBarPath);
                    }
                }
                AppendLine();
            }
        }


        /// <summary>
        /// Add a Digest entry
        /// </summary>
        public void AddArchiveAsset(string name, Stream stream)
        {
            AppendLine(string.Format("Archive-Asset-Name: {0}", name.Replace('\\', '/')));
            AppendLine(string.Format("Archive-Asset-SHA-512-Digest: {0}", CreateSha512Digest(stream)));
            AppendLine();
        }

        /// <summary>
        /// Gets the entry point activity.
        /// </summary>
        private static Activity GetEntryPoint(AndroidManifest androidManifest)
        {
            return androidManifest.Activities.FirstOrDefault(IsEntryPoint);
        }

        /// <summary>
        /// Is the given activity an entry point?
        /// </summary>
        private static bool IsEntryPoint(Activity activity)
        {
            return activity.IntentFilters.SelectMany(x => x.Actions).Any(x => x.IsMain) &&
                   activity.IntentFilters.SelectMany(x => x.Categories).Any(x => x.IsLauncher);
        }

        private static string FormatID(string val)
        {
            var md = MD5.Create();
            var result = md.ComputeHash(Encoding.UTF8.GetBytes(val));
            var headBytes = Base64.Decode("test");
            var result20 = new byte[20];
            Array.Copy(headBytes, 0, result20, 0, headBytes.Length);
            result20[headBytes.Length] = (byte) result.Length;
            Array.Copy(result, 0, result20, headBytes.Length + 1, result.Length);
            return Convert.ToBase64String(result20).Replace("=", "").Replace('/', '_');
        }
    }
}
