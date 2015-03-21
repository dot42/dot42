using System;
using System.Collections.Generic;
using Dot42.ApkLib.Manifest;
using Dot42.ApkLib.Resources;

namespace Dot42.ApkLib
{
    public interface IApkFile : IDisposable
    {
        /// <summary>
        /// Gets the names of all file stored in this APK.
        /// </summary>
        IEnumerable<string> FileNames { get; }

        /// <summary>
        /// Load AndroidManifest.xml
        /// </summary>
        AndroidManifest Manifest { get; }

        /// <summary>
        /// Load a resources table from the given apk.
        /// </summary>
        Table Resources { get; }

        /// <summary>
        /// Load the data for the given filename.
        /// </summary>
        byte[] Load(string fileName);
    }
}