using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dot42.BarDeployLib.Entity;
using Dot42.BarDeployLib.Entity.Mime;
using Dot42.BarLib;
using Dot42.DeviceLib;
using Dot42.Utility;

namespace Dot42.BarDeployLib
{
    public class BarDeployer
    {
        private readonly string deviceIp;
        private const int DevicePort = 443;
        private readonly string password;
        private readonly HttpClient httpClient;
        private static readonly Encoding PartEncoding = Encoding.ASCII;
        private static readonly Dictionary<string, string> RootV0 = new Dictionary<string, string> {
            { "PasswdChallenge", "passwd_challenge" },
            { "Success", "success" },
            { "Denied", "denied" },
            {"Error", "syserr" },
            { "Status", "Status" },
            { "RetriesRemaining", "retriesremaining" },
            { "FailedAttempts", "failedattempts" },
            { "Challenge", "challenge" },
            { "Algorithm", "algorithm" },
            { "ICount", "icount" },
            { "Salt", "salt" } };

        /// <summary>
        /// Default ctor
        /// </summary>
        public BarDeployer(IDevice device)
        {
            this.deviceIp = device.UniqueId;
            this.password = device.Password;
            httpClient = new HttpClient();
            UserName = "1000";
        }

        /// <summary>
        /// Name of user to authenticate
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Log response lines via this logger.
        /// </summary>
        public Action<string> Logger { get; set; }

        /// <summary>
        /// Install a bar file with given path.
        /// </summary>
        public void Install(string barFile, bool launchAfterInstall)
        {
            if (barFile == null)
                throw new ArgumentNullException("barFile");
            if (!File.Exists(barFile))
                throw new FileNotFoundException(barFile);
            Login();
            DoCommand(launchAfterInstall ? "Install and Launch" : "Install", barFile, launchAfterInstall, launchAfterInstall, null);
        }

        /// <summary>
        /// Launch a bar file with given path.
        /// </summary>
        public void LaunchFile(string barFile)
        {
            if (barFile == null)
                throw new ArgumentNullException("barFile");
            if (!File.Exists(barFile))
                throw new FileNotFoundException(barFile);
            Login();
            DoCommand("Launch", barFile, true, true, null);
        }

        /// <summary>
        /// Launch a bar with given package id.
        /// </summary>
        public void LaunchId(string packageId)
        {
            if (packageId == null)
                throw new ArgumentNullException("packageId");
            Login();
            DoCommand("Launch", null, true, false, packageId);
        }

        /// <summary>
        /// Uninstall a bar file with given path.
        /// </summary>
        public void UninstallFile(string barFile)
        {
            if (barFile == null)
                throw new ArgumentNullException("barFile");
            if (!File.Exists(barFile))
                throw new FileNotFoundException(barFile);
            Login();
            DoCommand("Uninstall", barFile, true, false, null);
        }

        /// <summary>
        /// Uninstall a bar file with given path.
        /// </summary>
        public void UninstallId(string packageId)
        {
            if (packageId == null)
                throw new ArgumentNullException("packageId");
            Login();
            DoCommand("Uninstall", null, true, false, packageId);
        }

        /// <summary>
        /// Perform a command on the AppInstaller.
        /// </summary>
        private void DoCommand(string command, string barFilePath, bool includePackageId, bool includePackageName, string packageId)
        {
            var url = new UriBuilder("https", deviceIp, DevicePort, "/cgi-bin/appInstaller.cgi").Uri;

            var entity = new MultipartEntity();
            AddParts(entity, command, barFilePath, includePackageId, includePackageName, packageId);
            var resp = httpClient.Post(url, entity);
            PrintResult(resp.GetResponseStream());
        }

        /// <summary>
        /// Add parts to the entity.
        /// </summary>
        private void AddParts(MultipartEntity entity, string command, string barFilePath, bool includePackageId, bool includePackageName, string packageId)
        {
            entity.AddBody(new StringBody(PartEncoding, "command", command));
            if (barFilePath != null)
            {
                entity.AddBody(new FileBody("file", Path.GetFileName(barFilePath), new FileInfo(barFilePath), "application/zip"));
            }
            if (includePackageId || includePackageName)
            {
                string name = null;
                if (packageId == null)
                {
                    if (barFilePath == null)
                        throw new ArgumentNullException("barFilePath");
                    var barFile = new BarFile(barFilePath);
                    packageId = barFile.Manifest.PackageId;
                    name = barFile.Manifest.PackageName;
                }
                if (includePackageId)
                {
                    if (packageId == null)
                        throw new ArgumentException("Missing Package-Id");
                    entity.AddBody(new StringBody(PartEncoding, "package_id", packageId));
                }
                if (includePackageName || (name != null))
                {
                    if (name == null)
                        throw new ArgumentException("Missing Package-Name");
                    entity.AddBody(new StringBody(PartEncoding, "package_name", name));
                }
            }
        }

        /// <summary>
        /// Print results on the logger.
        /// </summary>
        private void PrintResult(Stream stream)
        {
            var logger = Logger;
            var reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (logger != null)
                    logger(line);
#if DEBUG
                Debug.WriteLine(line);
#endif
            }
        }

        /// <summary>
        /// Login to the device
        /// </summary>
        public void Login()
        {
            var url = new UriBuilder("https", deviceIp, DevicePort, "/cgi-bin/login.cgi") {
                Query = "request_version=1"
            }.Uri;

            XDocument xml;
            try
            {
                xml = httpClient.GetAsXml(url);
            }
            catch (Exception)
            {
                throw new LoginSystemException("No content or invalid xml");
            }

            var status = GetNodeValue(xml, "Status");
            if (status == null)
                throw new LoginSystemException("Cannot determine status of response.");

            if (string.Equals(status, GetKey(xml, "Denied"), StringComparison.OrdinalIgnoreCase))
            {
                var remaining = int.Parse(GetNodeValue(xml, "RetriesRemaining"));
                var failedAttempts = int.Parse(GetNodeValue(xml, "FailedAttempts"));
                throw new LoginAuthException(failedAttempts, remaining);
            }

            if (string.Equals(status, GetKey(xml, "Error"), StringComparison.OrdinalIgnoreCase))
            {
                DLog.Debug(DContext.BarDeploy, "Status: Sys error");
                DoOldLogin();
            }
            else if (string.Equals(status, GetKey(xml, "PasswdChallenge"), StringComparison.OrdinalIgnoreCase))
            {
                var challenge = GetNodeValue(xml, "Challenge");
                var algorithm = int.Parse(GetNodeValue(xml, "Algorithm"));
                var salt = GetNodeValue(xml, "Salt");
                var icount = int.Parse(GetNodeValue(xml, "ICount"));
                DLog.Debug(DContext.CompilerStart, "Status: Challenge");
                if (algorithm == 0)
                {
                    DoOldLogin();
                }
                else
                {
                    var challengeData = PwCrypto.hashAndEncode(this.password, algorithm, salt, icount, challenge);
                    DoV2Login(challengeData);
                }
            }
        }

        /// <summary>
        /// Login with challenge data.
        /// </summary>
        private void DoV2Login(string challengeData)
        {
            var url = new UriBuilder("https", deviceIp, DevicePort,
                                     "/cgi-bin/login.cgi") { Query = "challenge_data=" + challengeData + "&request_version=1" }.Uri;

            XDocument xml;
            try
            {
                xml = httpClient.GetAsXml(url);
            }
            catch (Exception)
            {
                throw new LoginSystemException("No response");
            }

            var status = GetNodeValue(xml, "Status");
            if ((status == null) || (string.Equals(status, GetKey(xml, "Error"), StringComparison.OrdinalIgnoreCase)))
                throw new LoginSystemException("Status error: " + status);
            if (string.Equals(status, GetKey(xml, "Denied"), StringComparison.OrdinalIgnoreCase))
            {
                int remaining = int.Parse(GetNodeValue(xml, "RetriesRemaining"));
                int failedAttempts = int.Parse(GetNodeValue(xml, "FailedAttempts"));
                throw new LoginAuthException(failedAttempts, remaining);
            }
            /*if (string.Equals(status, GetKey(xml, "Success"), StringComparison.OrdinalIgnoreCase))
                checkLoginCookie(loginGet, httpResponse);*/
        }

        /// <summary>
        /// Old style login.
        /// Don't know on which devices this is actually used.
        /// </summary>
        private void DoOldLogin()
        {
            var url = new UriBuilder("https", deviceIp, DevicePort, "/cgi-bin/login.cgi").Uri;

            var loginEntity = new MultipartEntity();
            loginEntity.AddBody(new StringBody(PartEncoding, "user", UserName));
            loginEntity.AddBody(new StringBody(PartEncoding, "passwd", password));
            httpClient.Post(url, loginEntity);
        }

        /// <summary>
        /// Convert key's for older versions
        /// </summary>
        private static string GetKey(XDocument document, string key)
        {
            var root = document.Root;
            if ((root != null) && (root.Name.LocalName == "RIM_tablet_response"))
            {
                return RootV0[key];
            }
            return key;
        }

        /// <summary>
        /// Gets the value of a key in the XML.
        /// </summary>
        private static string GetNodeValue(XDocument document, string key)
        {
            var name = GetKey(document, key);
            var node = document.Descendants(name).FirstOrDefault();
            return (node != null) ? node.Value : null;
        }
    }
}
