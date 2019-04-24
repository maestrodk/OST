using IWshRuntimeLibrary;
using olproxy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverloadServerTool
{
    public partial class Main : Form
    {
        private bool launched = false;
        private ListBoxLog listBoxLog;

        private OlproxyProgram olproxyTask = null;
        private Thread olproxyThread = null;

        Dictionary<string, object> olproxyConfig = new Dictionary<string, object>();

        private string shortcutFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "OverLoadTool AutoStart.lnk");

        public Main(string[] args)
        {
            foreach (string a in args)
            {
                if (a.ToLower().Contains("-launched")) launched = true;
            }

            InitializeComponent();

            LoadSettings();

            AutoStart.Checked = System.IO.File.Exists(shortcutFileName);

            // Setup OlproxyProgram instance before attempting to start embedded Olproxy thread.
            olproxyTask = new OlproxyProgram();
            olproxyTask.SetLogger(InfoLogMessage);

            olproxyConfig.Add("isServer", IsServer.Checked);
            olproxyConfig.Add("signOff", SignOff.Checked);
            olproxyConfig.Add("trackerBaseUrl", TrackerBaseUrl.Text);
            olproxyConfig.Add("serverName", ServerName.Text);
            olproxyConfig.Add("notes", ServerNotes.Text);
            
            // Start logging.
            InitLogging(ActivityLogListBox);
            listBoxLog.Paused = false;

            SelectDarkTheme();
        }

        private void OlproxyThread()
        {
            olproxyTask.Run(new string[1] { OverloadArgs.Text }, UpdateOlproxyConfig());
        }

        private Dictionary<string, object> UpdateOlproxyConfig()
        {
            olproxyConfig["isServer"] = IsServer.Checked;
            olproxyConfig["signOff"] = SignOff.Checked;
            olproxyConfig["trackerBaseUrl"] = TrackerBaseUrl.Text;
            olproxyConfig["serverName"] = ServerName.Text;
            olproxyConfig["notes"] = ServerNotes.Text;
            return olproxyConfig;
        }

        private void StartOlproxyThread()
        {
            UpdateOlproxyConfig();
            olproxyThread = new Thread(OlproxyThread);
            olproxyThread.IsBackground = true;
            olproxyThread.Start();
        }

        private void KillOlproxyThread()
        {
            if (olproxyThread != null)
            {
                if (olproxyTask.KillFlag == false) olproxyTask.KillFlag = true;

                int n = 25;
                while ((n-- > 0) && (olproxyTask.KillFlag == true))
                {
                    Thread.Sleep(100);
                }

                olproxyThread = null;
            }
        }

        private void ActivityBackgroundMonitor()
        {
            while (true)
            {
                Thread.Sleep(1000);

                if ((listBoxLog != null) && (!listBoxLog.Paused))
                {
                    string olproxyName = Path.GetFileNameWithoutExtension(OlproxyExecutable.Text).ToLower();
                    string overloadName = Path.GetFileNameWithoutExtension(OverloadExecutable.Text).ToLower();

                    //string message = "";

                    if (UseEmbeddedOlproxy.Checked)
                    {
                        if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive))
                        {
                            //message = "Olproxy is running, ";
                            OlproxyGroupBox.Invoke(new Action(() => OlproxyGroupBox.Text = "Olproxy [running]"));
                        }
                        else
                        {
                            // message = "Olproxy is not running, ";
                            OlproxyGroupBox.Invoke(new Action(() => OlproxyGroupBox.Text = "Olproxy [stopped]"));
                        }
                    }
                    else
                    {
                        //message = "Olproxy is " + ((GetRunningProcess(olproxyName) != null) ? "running, " : "not running, ");
                        OlproxyGroupBox.Invoke(new Action(() => OlproxyGroupBox.Text = ((GetRunningProcess(olproxyName) != null) ? "Olproxy [running]" : "Olproxy [stopped]")));
                    }

                    OverloadGroupBox.Invoke(new Action(() => OverloadGroupBox.Text = ((GetRunningProcess(overloadName) != null) ? "Overload [running]" : "Overload [stopped]")));

                    // message += "Overload is " + ((GetRunningProcess(overloadName) != null) ? "running." : "not running.");

                    // if (message.Contains(" not ")) WarningLogMessage(message);
                    // else InfoLogMessage(message);
                }
            }
        }

        private bool IsAutoStartSet()
        {
            return System.IO.File.Exists(shortcutFileName);
        }

        private bool SetAutoStartup(bool create)
        {
            if (create)
            {
                try
                {
                    string appname = Assembly.GetExecutingAssembly().FullName.Remove(Assembly.GetExecutingAssembly().FullName.IndexOf(","));
                    string shortcutTarget = System.IO.Path.Combine(Application.StartupPath, appname + ".exe");

                    WshShell myShell = new WshShell();
                    WshShortcut myShortcut = (WshShortcut)myShell.CreateShortcut(shortcutFileName);

                    myShortcut.TargetPath = shortcutTarget;                 // The exe file this shortcut executes when double clicked.
                    myShortcut.IconLocation = shortcutTarget + ",0";        // Sets the icon of the shortcut to the application icon.
                    myShortcut.WorkingDirectory = Application.StartupPath;  // The working directory..
                    myShortcut.Arguments = "-launched";                     // The arguments used when executing the application.
                    myShortcut.Save();                                      // Creates the shortcut.

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create autostart shortcut!");
                }

                return false;
            }
            else
            {
                try
                {
                    if (System.IO.File.Exists(shortcutFileName)) System.IO.File.Delete(shortcutFileName);
                    else return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to remove autostart shortcut!");
                }

                return true;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            AutoStart_CheckedChanged(null, null);
            OverloadExecutable.Focus();
            OverloadExecutable.Select(0, 0);

            ValidateSettings();

            InfoLogMessage("Overload Server Tool " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + " by Søren Michélsen.");
            InfoLogMessage("Olproxy by Arne de Bruijn.");

            OverloadServerToolNotifyIcon.Icon = SystemIcons.Application;
            this.ShowInTaskbar = !UseTrayIcon.Checked;

            if (launched)
            {
                if (UseTrayIcon.Checked)
                {
                    this.ShowInTaskbar = false;
                    this.WindowState = FormWindowState.Minimized;
                    OverloadServerToolNotifyIcon.Visible = true;
                }
                StartButton_Click(null, null);
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Prevent exception when objects are nulled/destroyed.
            listBoxLog.Paused = true;

            // Kill embedded Olproxy.
            KillOlproxyThread();

            // Save settings for main application.
            try
            {
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error! Unable to save general settings: {ex.Message}");
            }

            try
            {
                // Update config then save as json for standalone Olproxy.
                string alterateFileName = Path.Combine(OlproxyExecutable.Text, "appsettings.json");
                olproxyTask.SaveConfig(UpdateOlproxyConfig(), alterateFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error! Unable to save Olpropxy settings: {ex.Message}");
            }
        }

        private void AutoTray_CheckedChanged(object sender, EventArgs e)
        {
            this.ShowInTaskbar = !UseTrayIcon.Checked;
        }

        private void AutoStart_CheckedChanged(object sender, EventArgs e)
        {
            SetAutoStartup(AutoStart.Checked);
        }

        private void ValidateSettings()
        {
            TestSetTextBoxColor(OverloadExecutable);
            TestSetTextBoxColor(OlproxyExecutable);

            if (((OlproxyExecutable.ForeColor == activeTextBoxColor) || UseEmbeddedOlproxy.Checked) && (OverloadExecutable.ForeColor == activeTextBoxColor))
            {
                // Path to Overload application is OK.
                // And we should use either the embedded Olproxy or the standalone application is OK.
                StartButton.Enabled = true;
            }
            else
            {
                StartButton.Enabled = false;
            }

            ValidateButton(StartButton);
        }

        private void TestSetTextBoxColor(TextBox textBox)
        {
            string path = textBox.Text.Trim();
            try
            {
                if (System.IO.File.Exists(path)) textBox.ForeColor = activeTextBoxColor;
                else textBox.ForeColor = inactiveTextBoxColor;
            }
            catch
            {
                textBox.ForeColor = inactiveTextBoxColor;
            }
        }

        private void OverloadExecutable_TextChanged(object sender, EventArgs e)
        {
            ValidateSettings();
        }

        private void OlproxyExecutable_TextChanged(object sender, EventArgs e)
        {
            ValidateSettings();
        }

        private void OverloadExecutable_MouseDoubleClick(object sender, EventArgs e)
        {
            OverloadExecutable.SelectionLength = 0;

            string save = Directory.GetCurrentDirectory();

            SelectExecutable.FileName = Path.GetFileName(OverloadExecutable.Text);
            SelectExecutable.InitialDirectory = Path.GetDirectoryName(OverloadExecutable.Text);

            DialogResult result = SelectExecutable.ShowDialog();

            if (result == DialogResult.OK)
            {
                OverloadExecutable.Text = SelectExecutable.FileName;
                OverloadExecutable.SelectionLength = 0;
            }

            Directory.SetCurrentDirectory(save);
        }

        private void OlproxyExecutable_DoubleClick(object sender, EventArgs e)
        {
            OlproxyExecutable.SelectionLength = 0;

            string save = Directory.GetCurrentDirectory();

            SelectExecutable.FileName = Path.GetFileName(OlproxyExecutable.Text);
            SelectExecutable.InitialDirectory = Path.GetDirectoryName(OlproxyExecutable.Text);

            DialogResult result = SelectExecutable.ShowDialog();

            if (result == DialogResult.OK)
            {
                OlproxyExecutable.Text = SelectExecutable.FileName;
                OlproxyExecutable.SelectionLength = 0;
            }

            Directory.SetCurrentDirectory(save);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (RestartOlproxy()) RestartOverload();
        }

        private bool RestartOlproxy()
        {
            VerboseLogMessage("Starting up Olproxy.");

            string name = Path.GetFileNameWithoutExtension(OlproxyExecutable.Text).ToLower();
            string args = OlproxyArgs.Text;

            // Update external Olproxy config.
            string alterateFileName = Path.Combine(OlproxyExecutable.Text, "appsettings.json");
            olproxyTask.SaveConfig(UpdateOlproxyConfig(), alterateFileName);

            // Start application it is not already running.
            int running = 0;
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower() == name) running++;
            }

            // If set to use external Olproxy and one instance is running the exit with OK status.
            if ((running == 1) && (UseEmbeddedOlproxy.Checked == false)) return true;

            // If we get here either embedded Olproxy is selected or 0/more than one instance of the external Olproxy is running.
            KillRunningProcess(name);

            // Should have no running instances now.
            if (UseEmbeddedOlproxy.Checked)
            {
                if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive)) KillOlproxyThread();
                StartOlproxyThread();
                return true;
            }

            string olproxyWorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Olproxy");
            if (!Directory.Exists(olproxyWorkingDirectory)) Directory.CreateDirectory(olproxyWorkingDirectory);

            string olproxyExe = Path.Combine(olproxyWorkingDirectory, Path.GetFileName(OlproxyExecutable.Text));
            if (!System.IO.File.Exists(olproxyExe)) System.IO.File.Copy(OlproxyExecutable.Text, olproxyExe);

            // (Re)start application..
            Process appStart = new Process();
            appStart.StartInfo = new ProcessStartInfo(olproxyExe, OlproxyArgs.Text);
            appStart.StartInfo.WorkingDirectory = olproxyWorkingDirectory;
            appStart.Start();

            // Allow some time for the application to initialize.
            int maxWait = 60;
            while (String.IsNullOrEmpty(appStart.MainWindowTitle) && (maxWait-- > 0))
            {
                Thread.Sleep(250);   // 40 x 250 = 15000 = 10 seconds.
                appStart.Refresh();
            }

            // Check if application was started succesfully.
            if (GetRunningProcess(name) != null) return true;

            ErrorLogMessage("Unable to start Olproxy.");
            return false;
        }

        private bool RestartOverload()
        {
            VerboseLogMessage("Starting up Overload");

            string name = Path.GetFileNameWithoutExtension(OverloadExecutable.Text).ToLower();

            // Start application it is not already running.
            int running = 0;
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower() == name) running++;
            }

            if (running == 1) return true;

            // If more than one is running we kill the all and start fresh instance.
            if (running > 1) KillRunningProcess(name);

            // (Re)start application..
            Process appStart = new Process();
            appStart.StartInfo = new ProcessStartInfo(Path.GetFileName(OverloadExecutable.Text), OverloadArgs.Text);
            appStart.StartInfo.WorkingDirectory = Path.GetDirectoryName(OverloadExecutable.Text);
            appStart.Start();

            // Allow some time for the application to initialize.
            int maxWait = 60;
            while (String.IsNullOrEmpty(appStart.MainWindowTitle) && (maxWait-- > 0))
            {
                Thread.Sleep(250);   // 40 x 250 = 15000 = 10 seconds.
                appStart.Refresh();
            }

            // Check if application was started succesfully.
            if (GetRunningProcess(name) != null)
            {
                InfoLogMessage("Overload started.");
                return true;
            }

            ErrorLogMessage("Unable to start Overload!");
            return false;
        }

        // Return process if instance is active otherwise return null.
        private Process GetRunningProcess(string name)
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (!process.ProcessName.ToLower().Contains("overloadservertool"))
                {
                    if (process.ProcessName.ToLower().Contains(name)) return process;
                }
            }
            return null;
        }

        private void KillRunningProcess(string name)
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (!process.ProcessName.ToLower().Contains("overloadservertool"))
                {
                    if (process.ProcessName.ToLower().Contains(name)) process.Kill();
                }
            }
        }

        // listBoxLog.Log(Level.Debug, "A debug level message");
        // listBoxLog.Log(Level.Verbose, "A verbose level message");
        // listBoxLog.Log(Level.Info, "A info level message");
        // listBoxLog.Log(Level.Warning, "A warning level message");
        // listBoxLog.Log(Level.Error, "A error level message");
        // listBoxLog.Log(Level.Critical, "A critical level message");
        // listBoxLog.Paused = !listBoxLog.Paused;

        private void SelectDark_CheckedChanged(object sender, EventArgs e)
        {
            // Must suspend log while changing theme.
            if (listBoxLog != null) listBoxLog.Paused = true;
            if (listBoxLog != null) listBoxLog.SetDarkTheme(DarkTheme = SelectDark.Checked);

            SelectDarkTheme();
            InfoLogMessage((DarkTheme) ? "Dark theme selected." : "Light theme selected selected.");

            if (listBoxLog != null) listBoxLog.Paused = false;
        }

        private void InfoLogMessage(string message)
        {
            if (listBoxLog != null) listBoxLog.Log(Level.Info, message);
        }

        private void VerboseLogMessage(string message)
        {
            if (listBoxLog != null) listBoxLog.Log(Level.Verbose, message);
        }

        private void WarningLogMessage(string message)
        {
            if (listBoxLog != null) listBoxLog.Log(Level.Warning, message);
        }

        private void ErrorLogMessage(string message)
        {
            if (listBoxLog != null) listBoxLog.Log(Level.Error, message);
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            VerboseLogMessage("Stopping active Olproxy and Overload tasks.");

            string olproxyName = Path.GetFileNameWithoutExtension(OlproxyExecutable.Text).ToLower();
            string overloadName = Path.GetFileNameWithoutExtension(OverloadExecutable.Text).ToLower();

            KillRunningProcess(overloadName);
            KillRunningProcess(olproxyName);

            if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive)) KillOlproxyThread();
        }

        private void StopExitButton_Click(object sender, EventArgs e)
        {
            StopButton_Click(null, null);
            Close();
        }

        private void UseEmbeddedOlproxy_CheckedChanged(object sender, EventArgs e)
        {
            InfoLogMessage((UseEmbeddedOlproxy.Checked) ? "Switching to embedded Olproxy." : "Switching to standalone Olproxy application.");

            if (UseEmbeddedOlproxy.Checked == true)
            {
                // Kill Olproxy application if it is running.
                string olproxyName = Path.GetFileNameWithoutExtension(OlproxyExecutable.Text).ToLower();
                Process process = GetRunningProcess(olproxyName);
                if (process != null)
                {
                    // InfoLogMessage("Shutting down Olproxy application.");

                    // Kill Olproxy application then start embedded Olproxy task.
                    process.Kill();

                    // InfoLogMessage("Starting embedded Olproxy task.");

                    StartOlproxyThread();
                }
            }
            else
            {
                // Kill Olproxy task if it is running.
                if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive))
                {
                    KillOlproxyThread();
                    RestartOlproxy();
                }
            }
        }

        private void IsServer_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void ActivityLogListBox_MouseLeave(object sender, EventArgs e)
        {
            ActivityLogListBox.SetSelected(0, false);
        }

        private void Main_MouseEnter(object sender, EventArgs e)
        {
            ActivityLogListBox.SetSelected(0, false);
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                // Send to tray or minimize?
                if (UseTrayIcon.Checked)
                {
                    Hide();
                    OverloadServerToolNotifyIcon.Visible = true;
                }
            }
        }

        private void OverloadServerToolNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            OverloadServerToolNotifyIcon.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void ServerNotes_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
