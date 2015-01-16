using System;
using Dot42.CryptoUI;

namespace Dot42.Gui.SamplesTool
{
    internal class SampleCertificateBuilder : ICertificateSettings
    {
        private readonly string samplesFolder;
        private string thumbprint;
        internal const string DefaultPassword = "samplepassword";

        /// <summary>
        /// Default ctor
        /// </summary>
        public SampleCertificateBuilder(string samplesFolder)
        {
            this.samplesFolder = samplesFolder;
        }

        /// <summary>
        /// Build the samples certificate
        /// </summary>
        public bool Build(Action<string> log)
        {
            string errorMessage;
            log("Creating sample certificate...");
            if (!CertificateBuilder.CreateCertificate(this, Console.WriteLine, out thumbprint, out errorMessage))
            {
                log(errorMessage);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Update the certificate in the sample projects.
        /// </summary>
        public bool UpdateSampleProjects(Action<string> log)
        {
            // Update sample projects
            return SampleProjectUpdater.UpdateSampleProjects(samplesFolder, thumbprint, log);            
        }

        public string OrgName
        {
            get { return "Dot42 Samples"; }
        }

        public string OrgUnit
        {
            get { return "Samples"; }
        }

        public string City
        {
            get { return "Unknown"; }
        }

        public string CountryCode
        {
            get { return "NL"; }
        }

        public string State
        {
            get { return string.Empty; }
        }

        public string Email
        {
            get { return "samples@dot42.com"; }
        }

        public string UserName
        {
            get { return Environment.UserName; }
        }

        public string Password
        {
            get { return DefaultPassword; }
        }

        public string Path
        {
            get { return System.IO.Path.Combine(samplesFolder, "Samples.pfx"); }
        }

        public bool ImportInCertificateStore
        {
            get { return true; }
        }

        public int MaxYears
        {
            get { return 2; }
        }

        public bool ValidForPublication
        {
            get { return false; }
        }
    }
}
