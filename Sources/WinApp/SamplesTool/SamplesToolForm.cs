using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dot42.Gui.SamplesTool
{
    public partial class SamplesToolForm : Form
    {
        private readonly string samplesFolder;

        /// <summary>
        /// Designer ctor
        /// </summary>
        public SamplesToolForm()
            : this(".")
        {
        }

        /// <summary>
        /// Default ctor
        /// </summary>
        public SamplesToolForm(string samplesFolder)
        {
            this.samplesFolder = samplesFolder;
            InitializeComponent();
        }

        /// <summary>
        /// Log a message.
        /// </summary>
        public void Log(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(Log), msg);
            }
            else
            {
                tbLog.Text += msg + Environment.NewLine;
            }
        }

        /// <summary>
        /// Perform all sample operations
        /// </summary>
        private bool Run()
        {
            // Create file
            if (!Directory.Exists(samplesFolder))
                Directory.CreateDirectory(samplesFolder);
            if (!Directory.Exists(samplesFolder))
                return false;


            // Create certificate
            var certificateBuilder = new SampleCertificateBuilder(samplesFolder);
            if (!certificateBuilder.Build(Log))
                return false;

            // Unzip samples
            var programsFolder = Path.GetDirectoryName(typeof (SamplesToolForm).Assembly.Location);
            var zipFile = Path.Combine(programsFolder, "Samples.zip");
            if (!File.Exists(zipFile))
                zipFile = Path.Combine(Environment.CurrentDirectory, "Samples.zip");
            if (!File.Exists(zipFile))
            {
                Log("Samples.zip not found");
                return false;
            }
            return SampleUnpacker.Unzip(samplesFolder, zipFile, Log, () => certificateBuilder.UpdateSampleProjects(Log));
        }

        /// <summary>
        /// Start on load
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task = Task.Factory.StartNew(() => Run());
            task.ContinueWith(x => {
                if (x.Result) Close();                                  
                else
                {
                    Log("Failures detected. Please report them to support@dot42.com.");
                    Show();                    
                }
            }, ui);
        }
    }
}
