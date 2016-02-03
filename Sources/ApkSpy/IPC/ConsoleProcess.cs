using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Dot42.ApkSpy.IPC
{
    public enum CommunicationMode
    {
        None,
        Line,
        Binary,
    }
    /// <summary>
    /// class to communicate with Console Applications
    /// 
    /// All Data events appear asyncronously!
    /// 
    /// When a process is killed, data can arrive (once) after Exited() was called.
    /// </summary>
    public class ConsoleProcess
    {
        public string AppFileName { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public CommunicationMode ModeStdout { get; set; }
        public CommunicationMode ModeStderr { get; set; }

        public int BufferSizeStdout { get; set; }
        public int BufferSizeStderr { get; set; }

        public int ExitCode { get; private set; }

        public Encoding Encoding { get; set; }

        public Stream Stdin
        {
            get
            {
                lock (_sync)
                {
                    if (!IsRunning) return null;
                    return _process.StandardInput.BaseStream;
                }
            }
        }

        private AsyncStreamReader _stderrReader;
        private AsyncStreamReader _stdoutReader;

        private AsyncLineReader _stderrLineReader;
        private AsyncLineReader _stdoutLineReader;

        public event EventHandler Exited;
        public event Action<string> StderrLineRead;
        public event Action<string> StdoutLineRead;
        public event Action<byte[], int> StderrDataRead;
        public event Action<byte[], int> StdoutDataRead;

        private volatile bool _wasClosed;
        private readonly object _sync = new object();
        private Process _process;
        public ProcessPriorityClass PriorityClass { get; set; }

        public ConsoleProcess()
        {
            BufferSizeStdout = 4096;
            BufferSizeStderr = 4096;
            ModeStderr = CommunicationMode.Line;
            ModeStdout = CommunicationMode.Line;
            PriorityClass = ProcessPriorityClass.Normal;
        }

        public bool IsRunning
        {
            get
            {
                var p = _process;
                if (p == null || _wasClosed) return false;
                return true;
            }
        }

        public int ProcessId { get; set; }
        public Process Process { get { return _process; } }

        public void Start()
        {
            if (IsRunning)
                throw new ApplicationException("already running.");

            _wasClosed = false;

            _process = new Process();
            _process.StartInfo.FileName = this.AppFileName;
            if (Arguments != null)
                _process.StartInfo.Arguments = Arguments;
            if (WorkingDirectory != null)
                _process.StartInfo.WorkingDirectory = WorkingDirectory;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = ModeStdout != CommunicationMode.None;
            _process.StartInfo.RedirectStandardError = ModeStderr != CommunicationMode.None;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            _process.StartInfo.CreateNoWindow = true;


            if (ModeStderr == CommunicationMode.None && ModeStdout == CommunicationMode.None)
            {
                _process.Exited += _process_Exited;
                _process.EnableRaisingEvents = true;
            }


            _process.Start();
            ProcessId = _process.Id;
            try { _process.PriorityClass = PriorityClass; }
            catch
            {
            }


            // Stderr
            if (ModeStderr != CommunicationMode.None)
            {
                _stderrReader = new AsyncStreamReader(_process.StandardError.BaseStream, BufferSizeStderr);
                _stderrReader.DataRead += _stderrReader_DataRead;
            }
            if (ModeStderr == CommunicationMode.Line)
            {
                _stderrLineReader = new AsyncLineReader(_stderrReader);
                if (Encoding != null) _stderrLineReader.Encoding = Encoding;
                _stderrLineReader.LineRead += _stderrLineReader_LineRead;
            }
            
            // Stdout
            if (ModeStdout != CommunicationMode.None)
            {
                _stdoutReader = new AsyncStreamReader(_process.StandardOutput.BaseStream, BufferSizeStdout);
                _stdoutReader.DataRead += _stdoutReader_DataRead;
            }
            if (ModeStdout == CommunicationMode.Line)
            {
                _stdoutLineReader = new AsyncLineReader(_stdoutReader);
                if (Encoding != null) _stdoutLineReader.Encoding = Encoding;
                _stdoutLineReader.LineRead += _stdoutLineReader_LineRead;
            }

            if (ModeStderr != CommunicationMode.None)
                _stderrReader.Begin();
            if (ModeStdout != CommunicationMode.None)
                _stdoutReader.Begin();

        }

        private void OnTerminated()
        {
            lock (_sync)
            {
                if (_wasClosed) return;
                // wait for both streams to be closed.
                if (_stderrReader != null && !_stderrReader.Closed) return;
                if (_stdoutReader != null && !_stdoutReader.Closed) return;

                _wasClosed = true;
            }

            CloseStreams();

            try { ExitCode = _process.HasExited ? _process.ExitCode : -1; }
            catch { ExitCode = -1; }
            try { _process.Close(); }
            catch { }

            if (Exited != null)
                Exited(this, new EventArgs());
        }

        void _process_Exited(object sender, EventArgs e)
        {
            OnTerminated();
        }

        private void _stdoutReader_DataRead(byte[] data, int len)
        {
            if (data == null) { OnTerminated(); return; }
            if (ModeStdout == CommunicationMode.Binary && StdoutDataRead != null)
                StdoutDataRead(data, len);
        }

        private void _stderrReader_DataRead(byte[] data, int len)
        {
            if (data == null) { OnTerminated(); return; }
            if (ModeStderr == CommunicationMode.Binary && StderrDataRead != null)
                StderrDataRead(data, len);

        }

        private void _stdoutLineReader_LineRead(string line)
        {
            if (line == null) { return; }
            if (ModeStdout == CommunicationMode.Line && StdoutLineRead != null)
                StdoutLineRead(line);
        }


        private void _stderrLineReader_LineRead(string line)
        {
            if (line == null) return;
            if (ModeStderr == CommunicationMode.Line && StderrLineRead != null)
                StderrLineRead(line);
        }

        /// <summary>
        /// Versucht den Process zu Beenden. Keine Exception bei Fehler.
        /// </summary>
        /// <param name="waitBeforeKillMs"></param>
        /// <param name="waitAsyncronous"></param>
        public void Kill(int waitBeforeKillMs = 0, bool waitAsyncronous = true)
        {
            if (!IsRunning) return;
            try
            {
                if (waitBeforeKillMs > 0 && waitAsyncronous)
                {
                    var p = _process;
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (!p.WaitForExit(waitBeforeKillMs))
                                p.Kill();
                        }
                        catch
                        {
                        }
                    });
                }
                else if (waitBeforeKillMs > 0)
                {
                    if (!_process.WaitForExit(waitBeforeKillMs))
                        if (!_process.HasExited)
                            _process.Kill();
                }
                else
                    _process.Kill();
            }
            catch
            {
            }
            OnTerminated();
        }

        /// <summary>
        /// will close input stdout and stderr streams, 
        /// usually resulting in termination
        /// of console application
        /// 
        /// will not raise exceptions
        /// </summary>
        public void CloseStreams(int msWaitForExitCode = 0)
        {
            if (!IsRunning) return;

            var p = _process;
            try { p.StandardInput.Close(); }
            catch { }
            try { p.StandardOutput.Close(); }
            catch { }
            try { p.StandardError.Close(); }
            catch { }

            if (msWaitForExitCode > 0)
            {
                try { p.WaitForExit(msWaitForExitCode); }
                catch { }
                try { ExitCode = p.ExitCode; }
                catch { }
            }
        }

        public void Wait()
        {
            _process.WaitForExit();
            if (_stderrReader != null)
                _stderrReader.End();
            if (_stdoutReader != null)
                _stdoutReader.End();
        }

        public override string ToString()
        {
            string ret = this.AppFileName ?? "(none)";
            if (Arguments != null) ret += " " + Arguments;
            return ret;
        }

        public bool StdinTryWrite(string msg)
        {
            try
            {
                if (Encoding == null)
                {
                    _process.StandardInput.Write(msg);
                }
                else
                {
                    _process.StandardInput.Flush();
                    var bytes = Encoding.GetBytes(msg);
                    _process.StandardInput.BaseStream.Write(bytes, 0, bytes.Length);
                    _process.StandardInput.BaseStream.Flush();
                }

                _process.StandardInput.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
