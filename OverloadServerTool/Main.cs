﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using IWshRuntimeLibrary;

using olproxy;

namespace OverloadServerTool
{
    public partial class OSTMainForm : Form
    {
        private bool DebugLogging = false;

        private bool autoStart = false;
        private ListBoxLog listBoxLog;

        private OlproxyProgram olproxyTask = null;
        private Thread olproxyThread = null;

        private OverloadMapManager mapManager = new OverloadMapManager();
        private Thread mapManagerThread = null;

        // This matches MJDict defined on Olproxy.
        private Dictionary<string, object> olproxyConfig = new Dictionary<string, object>();

        // Shortcut link for Startup folde (if file exists the autostart is enabled).
        private string shortcutFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "OverLoad Server Tool AutoStart.lnk");

        // Directory for DLC.
        private string dlcLocation = null;

        public OSTMainForm(string[] args)
        {
            foreach (string a in args)
            {
                if (a.ToLower().Contains("-launched")) autoStart = true;
            }

            InitializeComponent();

            // Load user preferences.
            LoadSettings();
            ValidateSettings();

            // Reflect autostart option (if shortcut exist then is is enabled).
            AutoStart.Checked = System.IO.File.Exists(shortcutFileName);

            // Prepare embedded OlproxyProgram instance before attempting to start thread.
            olproxyTask = new OlproxyProgram();
            olproxyTask.SetLogger(InfoLogMessage);

            // Set logging for map manager.
            mapManager.SetLogger(InfoLogMessage, ErrorLogMessage);

            // Create properties for Olproxy thread (will be update from TextBox fields whenever Olproxy is restarted).
            olproxyConfig.Add("isServer", IsServer.Checked);
            olproxyConfig.Add("signOff", SignOff.Checked);
            olproxyConfig.Add("trackerBaseUrl", TrackerBaseUrl.Text);
            olproxyConfig.Add("serverName", ServerName.Text);
            olproxyConfig.Add("notes", ServerNotes.Text);
            
            // Start logging (default is paused state, will be enabled when startup is complete).
            InitLogging(ActivityLogListBox, DarkTextBoxBackColor, LightTextBoxBackColor);

            // Reflect selected theme settings.
            SetTheme();
        }

        private void InitLogging(ListBox listBox, Color darkBackColor, Color lightBackColor)
        {
            listBoxLog = new ListBoxLog(listBox);
            listBoxLog.Paused = true;

            // Make ListBoxLog aware of the selected theme setting.
            listBoxLog.SetDarkTheme(DarkTheme, darkBackColor, lightBackColor);

            // Start background monitor for periodic log updates.
            Thread thread = new Thread(ActivityBackgroundMonitor);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Update the DLC path if path to Overload gets updated by the user/load of settings.
        /// </summary>
        private void UpdateDLCLocation()
        {
            try
            {
                if (System.IO.File.Exists(OverloadExecutable.Text))
                {
                    if (Directory.Exists(Path.GetDirectoryName(OverloadExecutable.Text)))
                    {
                        dlcLocation = Path.Combine(Path.GetDirectoryName(OverloadExecutable.Text), "DLC");
                        Directory.CreateDirectory(dlcLocation);
                    }
                    UseDLCLocationCheckBox.Enabled = true;
                }
            }
            catch
            {
                dlcLocation = null;
                UseDLCLocationCheckBox.Enabled = false;
            }
        }

        /// <summary>
        /// Starts embedded Olproxy.
        /// </summary>
        private void OlproxyThread()
        {
            olproxyTask.Run(new string[1] { OverloadArgs.Text }, UpdateOlproxyConfig());
        }

        /// <summary>
        /// Refresh and return Olproxy configuration object.
        /// </summary>
        /// <returns>A dictionary object matching MJDict</returns>
        private Dictionary<string, object> UpdateOlproxyConfig()
        {
            olproxyConfig["isServer"] = IsServer.Checked;
            olproxyConfig["signOff"] = SignOff.Checked;
            olproxyConfig["trackerBaseUrl"] = TrackerBaseUrl.Text;
            olproxyConfig["serverName"] = ServerName.Text;
            olproxyConfig["notes"] = ServerNotes.Text;
            return olproxyConfig;
        }

        /// <summary>
        /// Start emnbedded Olproxy thread.
        /// </summary>
        private void StartOlproxyThread()
        {
            UpdateOlproxyConfig();
            olproxyThread = new Thread(OlproxyThread);
            olproxyThread.IsBackground = true;
            olproxyThread.Start();
        }

        /// <summary>
        /// Try to gracefully shutdown Olproxy thread.
        /// </summary>
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

        /// <summary>
        /// Background logging monitor. Sets GroupBox titles to reflect running status.
        /// </summary>
        private void ActivityBackgroundMonitor()
        {
            while (true)
            {
                Thread.Sleep(1000);

                if ((listBoxLog != null) && (!listBoxLog.Paused))
                {
                    string olproxyName = Path.GetFileNameWithoutExtension(OlproxyExecutable.Text).ToLower();
                    string overloadName = Path.GetFileNameWithoutExtension(OverloadExecutable.Text).ToLower();

                    if (UseEmbeddedOlproxy.Checked)
                    {
                        if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive))
                        {
                            OlproxyRunning.Invoke(new Action(() => OlproxyRunning.Visible = true));
                        }
                        else
                        {
                            OlproxyRunning.Invoke(new Action(() => OlproxyRunning.Visible = false));
                        }
                    }
                    else
                    {
                        OlproxyRunning.Invoke(new Action(() => OlproxyRunning.Visible = (GetRunningProcess(olproxyName) != null)));
                    }

                    OverloadRunning.Invoke(new Action(() => OverloadRunning.Visible = ((GetRunningProcess(overloadName) != null))));
                }

                if (!OlproxyRunning.Visible || !OverloadRunning.Visible)
                {
                    try { StartButton.Invoke(new Action(() => StartButton.Enabled = true));} catch { }
                }

                try
                {
                    if (StartButton.Enabled)
                    {
                        StartButton.Invoke(new Action(() => StartButton.BackColor = (DarkTheme) ? DarkButtonEnabledBackColor : LightButtonEnabledBackColor));
                        StartButton.Invoke(new Action(() => StartButton.ForeColor = (DarkTheme) ? DarkButtonEnabledForeColor : LightButtonEnabledForeColor));
                    }
                    else
                    {
                        StartButton.Invoke(new Action(() => StartButton.BackColor = (DarkTheme) ? DarkButtonDisabledBackColor : LightButtonDisabledBackColor));
                        StartButton.Invoke(new Action(() => StartButton.ForeColor = (DarkTheme) ? DarkButtonDisabledForeColor : LightButtonDisabledForeColor));
                    }
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Creates or deletes shortcut link to startup OST.
        /// </summary>
        /// <param name="create"></param>
        /// <returns></returns>
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

                    myShortcut.TargetPath = shortcutTarget;                 // Shortcut to OverloadServerTool.exe.
                    myShortcut.IconLocation = shortcutTarget + ",0";        // Use default application icon.
                    myShortcut.WorkingDirectory = Application.StartupPath;  // Working directory.
                    myShortcut.Arguments = "-launched";                     // Parameters sent to OverloadServerTool.exe.
                    myShortcut.Save();                                      // Create shortcut.

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to create autostart shortcut: {ex.Message}");
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
                    MessageBox.Show($"Unable to remove autostart shortcut: {ex.Message}");
                }

                return true;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            // Create/delete autostart shortcut.
            AutoStart_CheckedChanged(null, null);

            // Make sure no text is selected.
            OverloadExecutable.Focus();
            OverloadExecutable.Select(0, 0);

            // Check settings and update buttons.
            ValidateSettings();

            UpdateDLCLocation();

            listBoxLog.Paused = false;                

            InfoLogMessage("Overload Server Tool " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + " by Søren Michélsen.");
            InfoLogMessage("Olproxy by Arne de Bruijn.");

            // Auto-update maps on startup.
            MapUpdateButton_Click(null, null);

            // Check for startup options.
            OverloadServerToolNotifyIcon.Icon = Properties.Resources.OST;
            this.ShowInTaskbar = !UseTrayIcon.Checked;

            if (autoStart)
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

            Defocus();
        }

        /// <summary>
        /// Update maps in the background.
        /// </summary>
        private void UpdateMapThread()
        {
            UpdatingMaps.Invoke(new Action(() => UpdatingMaps.Visible = true));

            VerboseLogMessage(String.Format("Checking for new/updated maps at https://www.overloadmaps.com."));

            if (UseDLCLocationCheckBox.Enabled && UseDLCLocationCheckBox.Checked)
            {
                VerboseLogMessage(String.Format("Overload DLC directory used for maps."));
                mapManager.Update(null, dlcLocation);
            }
            else
            {
                VerboseLogMessage(String.Format("Overload ProgramData directory used for maps."));
                mapManager.Update();
            }

            VerboseLogMessage(String.Format($"Map check finished: {mapManager.Checked} maps, {mapManager.Created} created, {mapManager.Updated} updated."));

            UpdatingMaps.Invoke(new Action(() => UpdatingMaps.Visible = false));
            MapUpdateButton.Invoke(new Action(() => MapUpdateButton.Enabled = true));
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
            TrayInsteadOfTaskBar = UseTrayIcon.Checked;
        }

        private void AutoStart_CheckedChanged(object sender, EventArgs e)
        {
            SetAutoStartup(AutoStart.Checked);
            StartWithWindows = AutoStart.Checked;
        }

        private void ValidateSettings()
        {
            TestSetTextBoxColor(OverloadExecutable);
            TestSetTextBoxColor(OlproxyExecutable);

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
            OverloadPath = OverloadExecutable.Text;
            ValidateSettings();
            UpdateDLCLocation();
        }

        private void OlproxyExecutable_TextChanged(object sender, EventArgs e)
        {
            OlproxyPath = OlproxyExecutable.Text;
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
            StartButton.Enabled = false;

            Thread startOverloadThread = new Thread(RestartOverload);
            startOverloadThread.IsBackground = true;
            startOverloadThread.Start();

            Thread startOlproxyThread = new Thread(RestartOlproxy);
            startOlproxyThread.IsBackground = true;
            startOlproxyThread.Start();
        }

        private void RestartOlproxy()
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
            if ((running == 1) && (UseEmbeddedOlproxy.Checked == false)) return;

            // If we get here either embedded Olproxy is selected or 0/more than one instance of the external Olproxy is running.
            KillRunningProcess(name);

            // Should have no running instances now.
            if (UseEmbeddedOlproxy.Checked)
            {
                if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive)) KillOlproxyThread();
                StartOlproxyThread();
                return;
            }

            // Update JSON configuration file for standalone Olproxy (first check to make sure folder exists).
            string olproxyWorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Olproxy");
            if (!Directory.Exists(olproxyWorkingDirectory)) Directory.CreateDirectory(olproxyWorkingDirectory);

            string olproxyExe = Path.Combine(olproxyWorkingDirectory, Path.GetFileName(OlproxyExecutable.Text));
            if (!System.IO.File.Exists(olproxyExe)) System.IO.File.Copy(OlproxyExecutable.Text, olproxyExe);

            // (Re)start application..
            Process appStart = new Process();
            appStart.StartInfo = new ProcessStartInfo(olproxyExe, OlproxyArgs.Text);
            appStart.StartInfo.WorkingDirectory = olproxyWorkingDirectory;
            appStart.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            appStart.Start();
        }

        private void RestartOverload()
        {
            VerboseLogMessage("Starting up Overload");

            string name = Path.GetFileNameWithoutExtension(OverloadExecutable.Text).ToLower();

            // Start application it is not already running.
            int running = 0;
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName.ToLower() == name) running++;
            }

            if (running == 1) return;

            // If more than one is running we kill the all and start fresh instance.
            if (running > 1) KillRunningProcess(name);

            // (Re)start application..
            Process appStart = new Process();
            appStart.StartInfo = new ProcessStartInfo(Path.GetFileName(OverloadExecutable.Text), OverloadArgs.Text);
            appStart.StartInfo.WorkingDirectory = Path.GetDirectoryName(OverloadExecutable.Text);
            appStart.Start();
        }

        // Return process if instance is active otherwise return null.
        private Process GetRunningProcess(string name)
        {
            if (String.IsNullOrEmpty(name)) return null;

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
            // Must suspend log while changing theme to avoid 'discoloring' 
            // if a background monitor fires an event at the same time.
            if (listBoxLog != null)
            {
                listBoxLog.Paused = true;
                listBoxLog.SetDarkTheme(DarkTheme = SelectDark.Checked, DarkTextBoxBackColor, LightTextBoxBackColor);
            }

            SetTheme();

            InfoLogMessage((DarkTheme) ? "Dark theme selected." : "Light theme selected.");

            // Unpause logging.
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

            ValidateButton(StartButton);

            Defocus();        

            string olproxyName = Path.GetFileNameWithoutExtension(OlproxyExecutable.Text).ToLower();
            string overloadName = Path.GetFileNameWithoutExtension(OverloadExecutable.Text).ToLower();

            KillRunningProcess(overloadName);
            KillRunningProcess(olproxyName);

            if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive)) KillOlproxyThread();
        }

        /// <summary>
        /// Unfocus all controls.
        /// </summary>
        private void Defocus()
        {
            label1.Focus();
        }

        private void StopExitButton_Click(object sender, EventArgs e)
        {
            StopButton_Click(null, null);
            Close();
        }

        private void UseEmbeddedOlproxy_CheckedChanged(object sender, EventArgs e)
        {
            OlproxyEmbedded = UseEmbeddedOlproxy.Checked;

            InfoLogMessage((UseEmbeddedOlproxy.Checked) ? "Switching to embedded Olproxy." : "Switching to standalone Olproxy application.");

            if (UseEmbeddedOlproxy.Checked == true)
            {
                // Kill Olproxy application if it is running.
                string olproxyName = Path.GetFileNameWithoutExtension(OlproxyExecutable.Text).ToLower();
                Process process = GetRunningProcess(olproxyName);
                if (process != null)
                {
                    // Kill running Olproxy application. 
                    process.Kill();

                    // Start embedded Olproxy task.
                    StartOlproxyThread();
                }
            }
            else
            {
                // Kill Olproxy task.
                if ((olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive))
                {
                    KillOlproxyThread();
                    RestartOlproxy();
                }
            }
        }

        private void Main_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                // Tray or minimize to task bar?
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

        private void MapUpdateButton_Click(object sender, EventArgs e)
        {
            MapUpdateButton.Enabled = false;

            // Start updating maps in a separate thread.
            mapManagerThread = new Thread(UpdateMapThread);
            mapManagerThread.IsBackground = true;
            mapManagerThread.Start();
        }

        /// <summary>
        /// Move maps from either of the two possible directorys.
        /// </summary>
        /// <param name="overloadMapLocation"></param>
        /// <param name="dlcLocation"></param>
        private void MoveMaps(string source, string destination)
        {
            string[] files = Directory.GetFiles(source, "*.zip");
            foreach (string fileName in files)
            {
                // Exclude DLC content (only move maps).
                bool move = true;
                string test = Path.GetFileNameWithoutExtension(fileName).ToUpper();
                if (!fileName.ToLower().EndsWith(".zip") || (test.Contains("DLC0") || test.Contains("DLC1"))) move = false;

                if (move) System.IO.File.Move(Path.Combine(source, Path.GetFileName(fileName)), Path.Combine(destination, Path.GetFileName(fileName)));
            }
        }

        private void UseDLCLocationCheckBox_Click(object sender, EventArgs e)
        {
            string overloadMapLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Revival\Overload";

            if (UseDLCLocationCheckBox.Checked == false)
            {
                // Setting the check mark.
                DialogResult result = MessageBox.Show("Move existing maps to the Overload DLC directory?", "Move maps?", MessageBoxButtons.YesNoCancel);
                switch (result)
                {
                    case DialogResult.Cancel:
                        break;

                    case DialogResult.No:
                        UseDLCLocationCheckBox.Checked = true;
                        VerboseLogMessage(String.Format("Overload DLC directory used for maps."));
                        break;

                    default:
                        // TO-DO: Move existing maps.
                        UseDLCLocationCheckBox.Checked = true;
                        VerboseLogMessage(String.Format("Overload DLC directory used for maps."));
                        MoveMaps(overloadMapLocation, dlcLocation);
                        break;
                }
            }
            else
            {
                // Clearing the check mark.
                DialogResult result = MessageBox.Show("Move existing maps to [ProgramData]\\Overload\\Revival directory?", "Move maps?", MessageBoxButtons.YesNoCancel);
                switch (result)
                {
                    case DialogResult.Cancel:
                        break;

                    case DialogResult.No:
                        VerboseLogMessage(String.Format("Overload ProgramData directory used for maps."));
                        UseDLCLocationCheckBox.Checked = false;
                        break;

                    default:
                        VerboseLogMessage(String.Format("Overload ProgramData directory used for maps."));
                        MoveMaps(dlcLocation, overloadMapLocation);
                        UseDLCLocationCheckBox.Checked = false;
                        break;
                }
            }
        }

        private void UseDLCLocationCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UseDLCDirectory = UseDLCLocationCheckBox.Checked;
        }

        private void OverloadArgs_TextChanged(object sender, EventArgs e)
        {
            OverloadParameters = OverloadArgs.Text;
        }

        private void IsServer_CheckedChanged(object sender, EventArgs e)
        {
            OlproxyIsServer = IsServer.Checked;
        }

        private void SignOff_CheckedChanged(object sender, EventArgs e)
        {
            OlproxySignOff = SignOff.Checked;
        }
    }
}