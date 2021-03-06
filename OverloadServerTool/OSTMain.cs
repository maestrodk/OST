﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace OverloadServerTool
{
    public partial class OSTMain : Form
    {
        public Theme theme = Theme.GetDarkTheme;

        private bool autoStart = false;

        private ListViewLogger logger = null;

        private olproxy.Program olproxyTask = null;
        private Thread olproxyThread = null;

        private OverloadMapManager mapManager = null;
        private Thread mapManagerThread = null;

        private PaneController paneController = null;

        private OlmodManager olmodManager = null;

        // This matches MJDict defined on Olproxy.
        private Dictionary<string, object> olproxyConfig = new Dictionary<string, object>();

        // Shortcut link for Startup folde (if file exists the autostart is enabled). These are not currently created/used.
        private string shortcutFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "OverLoad Server Tool AutoStart.lnk");

        // Directory for DLC.
        private string dlcLocation = null;

        private string debugFileName = null;

        public void LogDebugMessage(string message)
        {
            if (!Debugging) return;

            message = String.IsNullOrEmpty(message) ? Environment.NewLine : message + Environment.NewLine;
            try { System.IO.File.AppendAllText(debugFileName, String.Format($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} {message}")); } catch { }
        }

        public OSTMain(string[] args, string debugFileName)
        {
            this.debugFileName = debugFileName;

            if (!Debugging)
            {
                try { System.IO.File.Delete(debugFileName); } catch { }
            }

            foreach (string a in args)
            {
                if (a.ToLower().Contains("-launched")) autoStart = true;
            }

            // Init map manager.
            mapManager = new OverloadMapManager(UpdateOnlyExistingMaps);

            // Set a default them (may be changed when reading settings).
            theme = Theme.GetDarkTheme;

            // Initialize controls on main form.
            InitializeComponent();

            // Center main form on Desktop.
            this.StartPosition = FormStartPosition.CenterScreen;
            StatusMessage.Text = "Starting up...";

            // Setup pane control.
            paneController = new PaneController(this, PaneButtonLine, theme);
            paneController.SetupPaneButton(PaneSelectMain, PaneMain);
            paneController.SetupPaneButton(PaneSelectMapManager, PaneMaps);
            paneController.SetupPaneButton(PaneSelectOverload, PaneOverload);
            paneController.SetupPaneButton(PaneSelectOlproxy, PaneOlproxy);
            paneController.SetupPaneButton(PaneSelectOlmod, PaneOlmod);
            paneController.SetupPaneButton(PaneSelectOptions, PaneOptions);

            // Load user preferences.
            LoadSettings();
            
            // Init maps listbox and start monitoring.
            InitMapsListBox();

            // Prepare embedded OlproxyProgram instance before attempting to start thread.
            olproxyTask = new olproxy.Program();

            olproxyTask.SetLogger(Info);

            // Set logging for map manager.
            mapManager.SetLogger(Info, Error);

            // Init Olmod manager.
            olmodManager = new OlmodManager(Info, Error);

            // Create properties for Olproxy thread (will be update from TextBox fields whenever Olproxy is restarted).
            olproxyConfig.Add("isServer", IsServer.Checked);
            olproxyConfig.Add("signOff", SignOff.Checked);
            olproxyConfig.Add("trackerBaseUrl", TrackerBaseUrl.Text);
            olproxyConfig.Add("serverName", ServerName.Text);
            olproxyConfig.Add("notes", ServerNotes.Text);
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
            LogDebugMessage("Main_Load()");

            // The theme colors MUST be set BEFORE attempting to validate settings.
            // This is because ValidateSettings() checks the button colors to see if
            // it is safe to start any of the .exe files.
            theme = (DarkTheme) ? Theme.GetDarkTheme : Theme.GetLightTheme;
            UpdateTheme(theme);

            // Focus the first pane.
            paneController.SwitchToPane(PaneSelectMain);

            // Make sure no text is selected.
            OverloadExecutable.Focus();
            OverloadExecutable.Select(0, 0);

            // Check settings and update buttons.
            ValidateSettings();

            // Locate DLC folder.
            UpdateDLCLocation();

            // Start logging (default is paused state, will be enabled when startup is complete).
            logger = new ListViewLogger(ActivityListView, theme, this);

            // Announce ourself.
            Info("Overload Server Tool " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3) + " by Søren Michélsen.");
            Info("Olproxy 0.3.0 by Arne de Bruijn.");

            // Start background monitor for periodic log updates.
            Thread thread = new Thread(ActivityBackgroundMonitor);
            thread.IsBackground = true;
            thread.Start();

            // Check if we should auto-update maps on startup.
            if (AutoUpdateMapsCheckBox.Checked) MapUpdateButton_Click(null, null);

            // Check if we should auto-update Olmod on startup.
            if (OlmodAutoUpdate) UpdateOlmod_Click(null, null);

            if (OverloadServerToolApplication.ValidFileName(OlmodPath, true)) Info($"{OlmodVersionInfo}");
            else Info("Olmod not foundo.");

            // Check for startup options.
            NotifyIcon.Icon = Properties.Resources.OST;

            this.ShowInTaskbar = !UseTrayIcon.Checked;
            NotifyIcon.Visible = UseTrayIcon.Checked;

            if (autoStart)
            {
                if (UseTrayIcon.Checked) WindowState = FormWindowState.Minimized;
                StartButton_Click(null, null);
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                //OverloadServerToolNotifyIcon.Visible = false;
            }

            Defocus();

            // Check for OST update.
            if (AutoUpdateOST) UpdateCheck(debugFileName, false);
        }

        [DllImport("shell32.dll")]
        static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        public static string SpecialFolderLocalLowPath
        {
            get
            {
                Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");
                IntPtr pszPath = IntPtr.Zero;

                try
                {
                    int hr = SHGetKnownFolderPath(localLowId, 0, IntPtr.Zero, out pszPath);
                    if (hr >= 0) return Marshal.PtrToStringAuto(pszPath);

                    throw Marshal.GetExceptionForHR(hr);
                }
                finally
                {
                    if (pszPath != IntPtr.Zero) Marshal.FreeCoTaskMem(pszPath);
                }
            }
        }

        private void Info(string text)
        {
            logger?.InfoLogMessage(text);
        }

        private void Warning(string text)
        {
            logger?.WarningLogMessage(text);
        }

        private void Verbose(string text)
        {
            logger?.VerboseLogMessage(text);
        }

        private void Error(string text)
        {
            logger?.ErrorLogMessage(text);
        }

        /// <summary>
        /// Update the DLC path if path to Overload gets updated by the user/load of settings.
        /// </summary>
        private void UpdateDLCLocation()
        {
            LogDebugMessage("UpdateDLCLocation()");

            try
            {
                if (System.IO.File.Exists(OverloadExecutable.Text))
                {
                    if (Directory.Exists(Path.GetDirectoryName(OverloadExecutable.Text))) CheckCreateDirectory(Path.Combine(Path.GetDirectoryName(OverloadExecutable.Text), "DLC"));

                    dlcLocation = Path.Combine(Path.GetDirectoryName(OverloadExecutable.Text), "DLC");
                    UseDLCLocationCheckBox.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                LogDebugMessage(String.Format($""));

                dlcLocation = null;
                UseDLCLocationCheckBox.Enabled = false;
                UseDLCLocation = false;
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

        public bool IsOlproxyRunning
        {
            get
            {
                bool running = false;

                try
                {
                    // Check Olproxy.
                    if (UseEmbeddedOlproxy.Checked)
                    {
                        if ((olproxyTask.KillFlag == false) && (olproxyThread != null) && olproxyThread.IsAlive) running = true;
                    }
                    else
                    {
                        try { running = (GetRunningProcess("olproxy") != null); } catch { }
                    }
                }
                catch
                {
                }

                return running;
            }
        }

        public bool IsOverloadRunning
        {
            get
            {
                bool running = false;
                try { if (GetRunningProcess("overload") != null) running = true; } catch { }
                return running;
            }
        }

        public bool IsOverloadOrOlmodRunning
        {
            get { return (IsOverloadRunning || IsOlmodRunning); }
        }

        public bool IsOlmodRunning
        {
            get
            {
                bool running = false;
                try { if (GetRunningProcess("olmod") != null) running = true; } catch { }
                return running;
            }
        }

        /// <summary>
        /// Background logging monitor. Sets GroupBox titles to reflect running status.
        /// </summary>
        private void ActivityBackgroundMonitor()
        {
            while (true)
            {
                Thread.Sleep(2000);

                bool overloadRunning = IsOverloadRunning;
                bool olproxyRunning = IsOlproxyRunning;
                bool olmodRunning = IsOlmodRunning;

                this.UIThread(delegate
                {
                    OverloadLogFileCheck();

                    string statusText = "Ready for some Overload action!";

                    if (overloadRunning && !olproxyRunning && !olmodRunning) statusText = "Overload is running.";
                    else if (overloadRunning && olproxyRunning && !olmodRunning) statusText = "Overload and Olproxy (external) are running.";
                    else if (olmodRunning && !olproxyRunning) statusText = "Overload (Olmod) is running.";
                    else if (olmodRunning && olproxyRunning) statusText = "Overload (Olmod) and Olproxy are running.";
                    else if (olproxyRunning) statusText = "Olproxy is running.";
                    else
                    {
                        bool foundOverload = false;
                        bool foundOlmod = false;

                        try { foundOverload = (OverloadServerToolApplication.ValidFileName(OverloadExecutable.Text) && new FileInfo(OverloadExecutable.Text).Exists); } catch { }
                        try { foundOlmod = (OverloadServerToolApplication.ValidFileName(OlmodExecutable.Text) && new FileInfo(OlmodExecutable.Text).Exists); } catch { }

                        if (UseOlmodCheckBox.Checked && !foundOlmod) statusText = "Cannot find Olmod (check path)!";
                        else if (!UseOlmodCheckBox.Checked && !foundOverload) statusText = "Cannot find Overload (check path)!";
                    }

                    OlproxyRunning.Visible = olproxyRunning;
                    OverloadRunning.Visible = overloadRunning || olmodRunning;

                    StartButton.Text = (olproxyRunning || overloadRunning || olmodRunning) ? "Stop" : "Start";

                    UpdateOlmod.Enabled = !olmodRunning;

                    if (StartButton.Enabled)
                    {
                        StartButton.BackColor = theme.ButtonEnabledBackColor;
                        StartButton.ForeColor = theme.ButtonEnabledForeColor;
                    }
                    else
                    {
                        StartButton.BackColor = theme.ButtonDisabledBackColor;
                        StartButton.ForeColor = theme.ButtonDisabledForeColor;
                    }

                    StatusMessage.Text = statusText;
                });
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Kill embedded Olproxy.
            KillOlproxyThread();

            // Shutdown background workers.
            //StopMapsMonitoring();

            // Save settings for main application.
            try
            {
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save settings: {ex.Message}", "Error");
            }

            try
            {
                // Update config then save as json for standalone Olproxy.
                string alterateFileName = Path.Combine(OlproxyExecutable.Text, "appsettings.json");
                olproxyTask.SaveConfig(UpdateOlproxyConfig(), alterateFileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to save Olpropxy settings: {ex.Message}", "Error");
            }
        }

        private void ValidateSettings()
        {
            TestSetTextBoxColor(OverloadExecutable);
            TestSetTextBoxColor(OlproxyExecutable);
            TestSetTextBoxColor(OlmodExecutable);
            ValidateButton(StartButton, theme);
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

                OverloadPath = OverloadExecutable.Text;
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
            if (StartButton.Text == "Stop")
            {
                StopButton_Click(null, null);
                StartButton.Text = "Start";
            }
            else
            {
                Thread startOverloadThread = new Thread(LaunchOverload);
                startOverloadThread.IsBackground = true;
                startOverloadThread.Start();

                if (UseOlproxyCheckBox.Checked)
                {
                    Thread startOlproxyThread = new Thread(LaunchOlproxy);
                    startOlproxyThread.IsBackground = true;
                    startOlproxyThread.Start();
                }
            }
        }

        private void StartUp(bool useOlmod, bool useOlproxy)
        {
            Thread startOverloadThread = new Thread(LaunchOverload);
            startOverloadThread.IsBackground = true;
            startOverloadThread.Start();

            if (useOlproxy)
            {
                Thread startOlproxyThread = new Thread(LaunchOlproxy);
                startOlproxyThread.IsBackground = true;
                startOlproxyThread.Start();
            }
        }

        private void LaunchOlproxy()
        {
            Verbose("Starting up Olproxy.");

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

            // Make sure Oloroxy.exe exists.
            if (new FileInfo(OlproxyExecutable.Text).Exists == false)
            {
                MessageBox.Show("Missing Olproxy.exe!");
                return;
            }

            // Start Olproxy application.
            Process appStart = new Process();
            appStart.StartInfo = new ProcessStartInfo(OlproxyExecutable.Text, OlproxyArgs.Text);
            appStart.StartInfo.WorkingDirectory = Path.GetDirectoryName(OlproxyExecutable.Text);
            appStart.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            appStart.Start();
        }

        private void LaunchOverload()
        {
            Verbose("Starting up");

            string overloadPath = Path.GetDirectoryName(OverloadExecutable.Text);
            string overloadExe = Path.Combine(overloadPath, "overload.exe");
            string olmodExe = Path.Combine(overloadPath, "olmod.exe");

            string name = null;
            string app = null;

            // If Olmod is enabled check if we should pass Overload install folder.
            string gameDir = "";
            if (OverloadServerToolApplication.ValidDirectoryName(Path.GetDirectoryName(OverloadExecutable.Text), true))
            {
                gameDir = "-gamedir \"" + Path.GetDirectoryName(OverloadExecutable.Text) + "\" ";
            }

            if (UseOlmodCheckBox.Checked)
            {
                if (System.IO.File.Exists(olmodExe))
                {
                    name = Path.GetFileNameWithoutExtension(olmodExe);
                    app = olmodExe;
                }
                else
                {
                    MessageBox.Show("Olmod.exe not found!");
                    return;
                }
            }
            else
            {
                if (System.IO.File.Exists(overloadExe))
                {
                    name = Path.GetFileNameWithoutExtension(overloadExe);
                    app = overloadExe;
                }
                else
                {
                    MessageBox.Show("Overload.exe not found in Overload path!");
                    return;
                }
            }

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
            appStart.StartInfo = new ProcessStartInfo(Path.GetFileName(app), (gameDir + OverloadArgs.Text).Trim());
            appStart.StartInfo.WorkingDirectory = Path.GetDirectoryName(app);
            appStart.Start();
        }

        // Return process if instance is active otherwise return null.
        public Process GetRunningProcess(string name)
        {
            if (String.IsNullOrEmpty(name)) return null;
            foreach (Process process in Process.GetProcesses()) if (process.ProcessName.ToLower() == name.ToLower()) return process;
            return null;
        }

        private void KillRunningProcess(string name)
        {
            foreach (Process process in Process.GetProcesses()) if (process.ProcessName.ToLower() == name.ToLower()) process.Kill();
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
            DarkTheme = DarkThemeCheckBox.Checked;
            theme = (DarkTheme) ? Theme.GetDarkTheme : Theme.GetLightTheme;

            logger?.SetTheme(theme);

            UpdateTheme(theme);

            Info((DarkTheme) ? "Dark theme selected." : "Light theme selected.");
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Verbose("Shutting down active tasks.");

            ValidateButton(StartButton, theme);

            Defocus();

            string overloadPath = Path.GetDirectoryName(OverloadExecutable.Text);
            string overloadExe = Path.Combine(overloadPath, "overload.exe");
            string olmodExe = Path.Combine(overloadPath, "olmod.exe");

            string olproxyName = "olproxy";
            string olmodName = "olmod";
            string overloadName = "overload";

            KillRunningProcess(overloadName);
            KillRunningProcess(olmodName);
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

            Info((UseEmbeddedOlproxy.Checked) ? "Using embedded Olproxy." : "Using standalone Olproxy application.");

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
                if ((olproxyTask != null) && (olproxyTask.KillFlag == false) && ((olproxyThread != null) && olproxyThread.IsAlive))
                {
                    KillOlproxyThread();
                    LaunchOlproxy();
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
                    NotifyIcon.Visible = true;
                }
            }
        }

        private void OverloadServerToolNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void UseOlmod_CheckedChanged(object sender, EventArgs e)
        {
            UseOlmod = UseOlmodCheckBox.Checked;
            Info((UseOlmodCheckBox.Checked) ? "Olmod enabled." : "Olmod disabled.");
        }

        private void UseOlproxy_CheckedChanged(object sender, EventArgs e)
        {
            UseOlproxy = UseOlproxyCheckBox.Checked;
            Verbose((UseOlproxyCheckBox.Checked) ? "Olproxy enabled." : "Olproxy disabled.");
        }

        private void SearchOverloadButton_Click(object sender, EventArgs e)
        {
            FindOverloadInstall();
        }

        private void OlmodExecutable_DoubleClick(object sender, EventArgs e)
        {
            OlmodExecutable.SelectionLength = 0;

            string save = Directory.GetCurrentDirectory();

            if (String.IsNullOrEmpty(OlmodExecutable.Text)) OlmodExecutable.Text = Path.Combine(Path.GetDirectoryName(OverloadExecutable.Text), "olmod.exe");

            SelectExecutable.FileName = Path.GetFileName(OlmodExecutable.Text);
            SelectExecutable.InitialDirectory = Path.GetDirectoryName(OlmodExecutable.Text);

            DialogResult result = SelectExecutable.ShowDialog();

            if (result == DialogResult.OK)
            {
                OlmodExecutable.Text = SelectExecutable.FileName;
                OlmodExecutable.SelectionLength = 0;
            }

            Directory.SetCurrentDirectory(save);
        }

        private void OlmodReleases_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo(OlmodReleases.Text)); } catch { }
        }

        private void OlproxyReleases_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Process.Start(new ProcessStartInfo(OlproxyReleases.Text)); } catch { }
        }

        /// <summary>
        /// Return true only if directory name is valid and exists/is created.
        /// </summary>
        /// <param name="directoryName">Name of directory to check/create./param>
        /// <returns></returns>
        private bool CheckCreateDirectory(string directoryName)
        {
            if (!OverloadServerToolApplication.ValidDirectoryName(directoryName)) return false;
            try { Directory.CreateDirectory(directoryName); } catch { }
            try { return Directory.Exists(directoryName); } catch { return false; }
        }

        private void PlayOverload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                ProcessStartInfo sInfo = new ProcessStartInfo(PLayOverloadLinkLabel.Text);
                Process.Start(sInfo);
            }
            catch
            {
            }
        }

        private void EnableDebugCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Debugging = EnableDebugCheckBox.Checked;
            Info((EnableDebugCheckBox.Checked) ? "Debug logging enabled." : "Debug logging disabled.");
        }

        private void OpenDebugFolder_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (CheckCreateDirectory(Path.GetDirectoryName(debugFileName))) try { Process.Start(new ProcessStartInfo("file:///" + Path.GetDirectoryName(debugFileName))); } catch { MessageBox.Show("Cannot open debug log folder!", "Error"); }
        }

        private void OlmodExecutable_TextChanged(object sender, EventArgs e)
        {
            OlmodPath = OlmodExecutable.Text;
            ValidateSettings();
        }

        private void OlproxyArgs_TextChanged(object sender, EventArgs e)
        {
            OlproxyParameters = OlproxyArgs.Text;
        }

        private void OverloadArgs_TextChanged(object sender, EventArgs e)
        {
            OverloadParameters = OverloadArgs.Text;
        }

        private void OnlineMapJsonUrl_TextChanged(object sender, EventArgs e)
        {
            MapListUrl = OnlineMapJsonUrl.Text;
        }

        private void InfoLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "mailto:mickdk2010@gmail.com?subject=Overload Client Tool";
                proc.Start();
            }
            catch
            {
            }
        }

        #region MapManager

        private object mapChangeLock = new object();

        private static bool SameMap(OverloadMap map1, OverloadMap map2)
        {
            return (map1.InDLCFolder == map2.InDLCFolder) && (map1.ZipName.ToLower() == map2.ZipName.ToLower());
        }

        private void UpdateMapList()
        {
            string appPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Revival");
            appPath = Path.Combine(appPath, "Overload");

            // Construct the paths to where local maps can be stored.
            string dlcFolder = OverloadServerToolApplication.ValidDirectoryName(OverloadPath) ? Path.Combine(Path.GetDirectoryName(OverloadPath), "DLC") : null;
            string appFolder = OverloadServerToolApplication.ValidDirectoryName(appPath) ? appPath : null;

            mapManager.SaveNewMapsToDLCFolder = UseDLCLocation && OverloadServerToolApplication.ValidDirectoryName(dlcFolder, true);
            mapManager.UpdateMapList(MapListUrl, dlcFolder, appFolder);
        }

        private void InitMapsListBox()
        {
            LogDebugMessage("InitMapsListBox()");

            UpdateMapList();
            UpdateListBox();
        }

        private void SetMapButtons()
        {
            if (MapsListBox.SelectedIndex >= 0)
            {
                OverloadMap map = ((KeyValuePair<string, OverloadMap>)MapsListBox.Items[MapsListBox.SelectedIndex]).Value;

                MapHideButton.Text = (map.Hidden) ? "Unhide" : "Hide";

                MapHideButton.Enabled = map.IsLocal;
                MapDeleteButton.Enabled = map.IsLocal;
                MapHideButton.Enabled = map.IsLocal;
                MapRefreshButton.Enabled = map.IsOnline && !map.Hidden;
            }
            else
            {
                MapDeleteButton.Enabled = false;
                MapHideButton.Text = "Hide";
                MapHideButton.Enabled = false;
                MapRefreshButton.Enabled = false;
            }
        }

        private void MapDelete_Click(object sender, EventArgs e)
        {
            if (MapsListBox.SelectedIndex >= 0)
            {
                OverloadMap map = ((KeyValuePair<string, OverloadMap>)MapsListBox.Items[MapsListBox.SelectedIndex]).Value;
                if (!map.IsLocal) return;

                if (MessageBox.Show(String.Format($"Delete map '{map.ZipName}' from disk?"), "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    lock (mapChangeLock)
                    {
                        try
                        {
                            MapsListBox.Items.Remove(map);
                            try { System.IO.File.Delete(map.LocalZipFileName); } catch { }
                            try { System.IO.File.Delete(map.LocalDLCZipFileName); } catch { }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(String.Format($"Whoops! Cannot delete map '{map.ZipName}': {ex.Message}"));
                        }

                        UpdateMapList();
                        UpdateListBox();
                    }
                }
            }
        }

        private void MapsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            lock (mapChangeLock) SetMapButtons();
        }

        private async void MapRefresh_ClickAsync(object sender, EventArgs e)
        {
            OverloadMap map = ((KeyValuePair<string, OverloadMap>)MapsListBox.Items[MapsListBox.SelectedIndex]).Value;

            if (map.Hidden)
            {
                MessageBox.Show("Cannot refresh a hidden map!", "Map is hidden");
                return;
            }

            string dlcFolder = OverloadServerToolApplication.ValidDirectoryName(OverloadPath) ? Path.Combine(Path.GetDirectoryName(OverloadPath), "DLC") : null;
            mapManager.SaveNewMapsToDLCFolder = UseDLCLocation && OverloadServerToolApplication.ValidDirectoryName(dlcFolder, true);

            await mapManager.UpdateMap(map, true);

            UpdateListBox();
        }

        private void MapHideButton_Click(object sender, EventArgs e)
        {
            OverloadMap map = ((KeyValuePair<string, OverloadMap>)MapsListBox.Items[MapsListBox.SelectedIndex]).Value;
            if (!map.IsLocal) return;

            int index = MapsListBox.SelectedIndex;
            try
            {
                map.Hidden = !map.Hidden;
                UpdateListBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format($"Whoops! Cannot {((map.Hidden) ? "unhide" : "hide")} map {map.ZipName}: {ex.Message}"));
                SetMapButtons();
            }

            OnlineMapJsonUrl.Select(0, 0);
            MapsListBox.SetSelected(index, true);
        }

        private void UpdateListBox(string focusName = null)
        {
            MapsListBox.Items.Clear();
            MapsListBox.DisplayMember = "Value";
            MapsListBox.ValueMember = "Value";

            foreach (KeyValuePair<string, OverloadMap> setMap in mapManager.Maps)
            {
                if (setMap.Value.IsLocal) MapsListBox.Items.Add(setMap);
            }

            if (!String.IsNullOrEmpty(focusName))
            {
                for (int i = 0; i < MapsListBox.Items.Count; i++)
                {
                    OverloadMap map = ((KeyValuePair<string, OverloadMap>)MapsListBox.Items[i]).Value;
                    if (map.SameZipFileName(focusName))
                    {
                        MapsListBox.SelectedIndex = i;
                        return;
                    }
                }
            }

            SetMapButtons();
        }

        private void MapUpdateButton_Click(object sender, EventArgs e)
        {
            MapUpdateButton.Enabled = false;

            string dlcFolder = OverloadServerToolApplication.ValidDirectoryName(OverloadPath) ? Path.Combine(Path.GetDirectoryName(OverloadPath), "DLC") : null;
            mapManager.SaveNewMapsToDLCFolder = UseDLCLocation && OverloadServerToolApplication.ValidDirectoryName(dlcFolder, true);

            // Start updating maps in a separate thread.
            mapManagerThread = new Thread(UpdateMapThread);
            mapManagerThread.IsBackground = true;
            mapManagerThread.Start();
        }

        /// <summary>
        /// Update maps in the background.
        /// </summary>
        private void UpdateMapThread()
        {
            this.UIThread(delegate
            {
                UpdatingMaps.Visible = true;
                MapsListBox.Enabled = false;

                MapRefreshButton.Enabled = false;
                MapDeleteButton.Enabled = false;
                MapHideButton.Enabled = false;

                UseDLCLocationCheckBox.Enabled = false;
                AutoUpdateMapsCheckBox.Enabled = false;
                OnlyUpdateExistingMapsCheckBox.Enabled = false;

                if (UpdateOnlyExistingMaps) Verbose(String.Format("Checking for updated maps."));
                else Verbose(String.Format("Checking for new/updated maps."));

                mapManager.SaveNewMapsToDLCFolder = (UseDLCLocation && !String.IsNullOrEmpty(dlcLocation));
                Verbose(String.Format("Overload " + ((UseDLCLocation && !String.IsNullOrEmpty(dlcLocation)) ? "DLC" : "application") + " folder used for new maps."));
            });

            // UpdateAllMaps() must not touch UI elements!
            mapManager.UpdateAllMaps(OnlineMapJsonUrl.Text, dlcLocation, null);

            this.UIThread(delegate
            {
                if (UpdateOnlyExistingMaps) Verbose(String.Format($"Map check finished: {mapManager.Checked} maps, {mapManager.Updated} updated."));
                else Verbose(String.Format($"Map check finished: {mapManager.Checked} maps checked, {mapManager.Created} created, {mapManager.Updated} updated."));

                UpdateListBox();

                UpdatingMaps.Visible = false;
                MapUpdateButton.Enabled = true;
                MapsListBox.Enabled = true;

                UseDLCLocationCheckBox.Enabled = true;
                AutoUpdateMapsCheckBox.Enabled = true;
                OnlyUpdateExistingMapsCheckBox.Enabled = true;

                SetMapButtons();
            });
        }

        /// <summary>
        /// Move maps from either of the two possible directorys.
        /// </summary>
        /// <param name="overloadMapLocation"></param>
        /// <param name="dlcLocation"></param>
        private void MoveMaps(string source, string destination)
        {
            if (!CheckCreateDirectory(source) || !CheckCreateDirectory(destination))
            {
                MessageBox.Show("Something went wrong in checking source/destination folders for map move!", "Unable to move maps");

                source = String.IsNullOrEmpty(source) ? "<null>" : source;
                destination = String.IsNullOrEmpty(destination) ? "<null>" : destination;

                Error(String.Format($"Map move from {source} to {destination} failed!"));

            }
            else
            {
                string[] files = Directory.GetFiles(source, "*.zip");
                foreach (string fileName in files)
                {
                    // Exclude DLC content (only move maps).
                    bool move = true;
                    string test = Path.GetFileNameWithoutExtension(fileName).ToUpper();
                    if (!fileName.ToLower().EndsWith(".zip") || (test.Contains("DLC0") || test.Contains("DLC1"))) move = false;

                    if (move)
                    {
                        string sourceFileName = Path.Combine(source, Path.GetFileName(fileName));
                        string destinationFileName = Path.Combine(destination, Path.GetFileName(fileName));

                        try
                        {
                            FileInfo fiSource = new FileInfo(sourceFileName);

                            if (System.IO.File.Exists(destinationFileName)) System.IO.File.Delete(destinationFileName);
                            System.IO.File.Move(sourceFileName, destinationFileName);

                            // Set local files date and time.
                            System.IO.File.SetCreationTime(destinationFileName, fiSource.CreationTime);
                            System.IO.File.SetLastWriteTime(destinationFileName, fiSource.LastWriteTime);
                        }
                        catch (Exception ex)
                        {
                            Error(String.Format($"Map move {sourceFileName} to {destinationFileName} failed: {ex.Message}"));
                        }
                    }
                }
            }

            UpdateMapList();
            UpdateListBox();
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
                        UseDLCLocation = true;
                        Verbose(String.Format("Overload DLC directory used for maps."));
                        break;

                    default:
                        // TO-DO: Move existing maps.
                        UseDLCLocationCheckBox.Checked = true;
                        UseDLCLocation = true;
                        Verbose(String.Format("Overload DLC directory used for maps."));
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
                        Verbose(String.Format("Overload ProgramData directory used for maps."));
                        UseDLCLocationCheckBox.Checked = false;
                        UseDLCLocation = false;
                        break;

                    default:
                        Verbose(String.Format("Overload ProgramData directory used for maps."));
                        MoveMaps(dlcLocation, overloadMapLocation);
                        UseDLCLocationCheckBox.Checked = false;
                        UseDLCLocation = false;
                        break;
                }
            }
        }

        private void AutoUpdateMaps_Click(object sender, EventArgs e)
        {
            AutoUpdateMaps = AutoUpdateMapsCheckBox.Checked;
        }

        private void OnlyUpdateExistingMapsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateOnlyExistingMaps = OnlyUpdateExistingMapsCheckBox.Checked;
            mapManager.OnlyUpdateExistingMaps = UpdateOnlyExistingMaps;
        }

        private void MapsListBox_MouseMove(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            int index = lb.IndexFromPoint(e.Location);

            if (index >= 0 && index < lb.Items.Count)
            {
                OverloadMap map = ((KeyValuePair<string, OverloadMap>)MapsListBox.Items[index]).Value;
                string toolTipString = map.DisplayMapInfo;

                // Don't do anything tooltip text is the current tooltip .
                if (MainToolTip.GetToolTip(lb) != toolTipString) MainToolTip.SetToolTip(lb, toolTipString);
            }
            else
                MainToolTip.Hide(lb);
        }

        // Log an informational message.
        private void LogMessage(string s)
        {
            this.UIThread(delegate
            {
                LogMessage(s);
            });
        }

        #endregion

        #region Overload Application Settings

        private string outputLogFileName = SpecialFolderLocalLowPath + Path.DirectorySeparatorChar + "Revival" + Path.DirectorySeparatorChar + "Overload" + Path.DirectorySeparatorChar + "output_log.txt";

        private void OverloadLogFileCheck()
        {
            try
            {
                OverloadLog.Visible = System.IO.File.Exists(outputLogFileName);
            }
            catch
            {
                OverloadLog.Visible = false;
            }
        }

        private void OverloadLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                ProcessStartInfo sInfo = new ProcessStartInfo("notepad.exe", outputLogFileName);
                Process.Start(sInfo);
            }
            catch
            {
            }
        }
        #endregion

        private void UseGameDirArg_CheckedChanged(object sender, EventArgs e)
        {
            PassGameDirToOlmod = UseOlmodGameDirArg.Checked;
        }

        #region Olmod updating

        private void UpdateOlmod_Click(object sender, EventArgs e)
        {
            if (IsOlmodRunning)
            {
                Error("Cannot update Olmod when it is running.");
                return;
            }

            // Decide where to unpack Olmod ZIP.
            string olmodInstallFolder = null;

            if (OverloadServerToolApplication.ValidFileName(OlmodPath, true)) olmodInstallFolder = Path.GetDirectoryName(OlmodPath);
            if ((olmodInstallFolder == null) && (OverloadServerToolApplication.ValidFileName(OverloadPath, true))) olmodInstallFolder = Path.GetDirectoryName(OverloadPath);

            if (olmodInstallFolder == null)
            {
                Error("No valid folder found to install/update Olmod.");
                return;
            }

            // Get latest online release info.
            OlmodManager.OlmodRelease latest = olmodManager.GetLastestRelease;
            if (latest == null)
            {
                Error("Unable to get latest Olmod release info from Github.");
                return;
            }

            // Check if update is required. We use the ZIP date to stamp Olmod.exe
            if ((OverloadServerToolApplication.ValidFileName(OlmodPath, true)) && (new FileInfo(OlmodPath).CreationTimeUtc == latest.Created))
            {
                if (sender != null) Info("Already using the latest Olmod version.");
                return;
            }

            Info("Installing latest Olmod release from Github.");

            try
            {
                olmodManager.DownloadAndInstallOlmod(latest, olmodInstallFolder);
            }
            catch
            {
            }
            finally
            {
                ValidateSettings();
            }
        }

        private string OlmodVersionInfo
        {
            get
            {
                string olmodVersion = OverloadServerToolApplication.GetFileVersion(OlmodPath);
                olmodVersion = OverloadServerToolApplication.VersionStringFix(olmodVersion);
                return String.Format($"Olmod {olmodVersion} by Arne de Bruijn.");
            }
        }

        private void AutoUpdateOlmod_CheckedChanged(object sender, EventArgs e)
        {
            OlmodAutoUpdate = AutoUpdateOlmod.Checked;
        }
        #endregion

        private void PayPalLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = "https://paypal.me/SorenM";
                proc.Start();
            }
            catch
            {
            }            
        }

        private void IsServer_CheckedChanged(object sender, EventArgs e)
        {
            OlproxyIsServer = IsServer.Checked;
        }

        private void SignOff_CheckedChanged(object sender, EventArgs e)
        {
            OlproxySignOff = SignOff.Checked;
        }

        private void AutoStartCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetAutoStartup(AutoStartCheckBox.Checked);
            StartWithWindows = AutoStartCheckBox.Checked;
        }

        private void UseTrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            TrayInsteadOfTaskBar = UseTrayIcon.Checked;
            ShowInTaskbar = !UseTrayIcon.Checked;
            NotifyIcon.Visible = UseTrayIcon.Checked;
        }

        private void ServerName_TextChanged(object sender, EventArgs e)
        {
            OlproxyServerName = ServerName.Text;
        }

        private void ServerNotes_TextChanged(object sender, EventArgs e)
        {
            OlproxyNotes = ServerNotes.Text;
        }

        private void TrackerBaseUrl_TextChanged(object sender, EventArgs e)
        {
            OlproxyTrackerBaseUrl = TrackerBaseUrl.Text;
        }
    }
}