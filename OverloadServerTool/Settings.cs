using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverloadServerTool
{
    public partial class OSTMainForm
    {
        private Color activeTextBoxColor;
        private Color inactiveTextBoxColor;

        public string OlproxyTrackerBaseUrl
        {
            get { return Properties.Settings.Default.trackerBaseUrl; }
            set { Properties.Settings.Default.trackerBaseUrl = value; }
        }

        public string OlproxyNotes
        {
            get { return Properties.Settings.Default.notes; }
            set { Properties.Settings.Default.notes = value; }
        }

        public string OlproxyServerName
        {
            get { return Properties.Settings.Default.serverName; }
            set { Properties.Settings.Default.serverName = value; }
        }

        public bool OlproxySignOff
        {
            get { return Properties.Settings.Default.signOff; }
            set { Properties.Settings.Default.signOff = value; }
        }

        public bool OlproxyIsServer
        {
            get { return Properties.Settings.Default.isServer; }
            set { Properties.Settings.Default.isServer = value; }
        }

        public string OverloadPath
        {
            get { return Properties.Settings.Default.OverloadPath; }
            set { Properties.Settings.Default.OverloadPath = value; }
        }

        public bool OlproxyEmbedded
        {
            get { return Properties.Settings.Default.EmbeddedOlproxy; }
            set { Properties.Settings.Default.EmbeddedOlproxy = value; }
        }

        public string OverloadParameters
        {
            get { return Properties.Settings.Default.OverloadParameters; }
            set { Properties.Settings.Default.OverloadParameters = value; }
        }

        public string OlproxyPath
        {
            get { return Properties.Settings.Default.OlproxyPath; }
            set { Properties.Settings.Default.OlproxyPath = value; }
        }

        public string OlproxyParameters
        {
            get { return Properties.Settings.Default.OlproxyParameters; }
            set { Properties.Settings.Default.OlproxyParameters = value; }
        }

        public bool DarkTheme
        {
            get { return Properties.Settings.Default.DarkTheme; }
            set { Properties.Settings.Default.DarkTheme = value; }
        }

        public bool StartWithWindows
        {
            get { return Properties.Settings.Default.StartWithWindows; }
            set { Properties.Settings.Default.StartWithWindows = value; }
        }

        public bool StartMinimized
        {
            get { return Properties.Settings.Default.StartMinimized; }
            set { Properties.Settings.Default.StartMinimized = value; }
        }

        public bool TrayInsteadOfTaskBar
        {
            get { return Properties.Settings.Default.TrayOnly; }
            set { Properties.Settings.Default.TrayOnly = value; }
        }

        public void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }

        public void LoadSettings()
        {
            // Attempt to find Overload installation path.
            // First verify the current setting.
            if (!String.IsNullOrEmpty(OverloadPath)) if (!File.Exists(OverloadPath)) OverloadPath = null;
            
            if (String.IsNullOrEmpty(OverloadPath))
            {
                string steamLocation = null;
                string gogLocation = null;
                string dvdLocation = null;

                // Check for a STEAM install of Overload.
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 448850"))
                    {
                        if (key != null)
                        {
                            steamLocation = (string)key.GetValue("InstallLocation");
                            if (!File.Exists(Path.Combine(steamLocation, "overload.exe"))) steamLocation = null;

                            if (String.IsNullOrEmpty(steamLocation))
                            {
                                steamLocation = (string)key.GetValue("UninstallString");
                                if (!String.IsNullOrEmpty(steamLocation))
                                {
                                    string[] parts = steamLocation.Split("\"".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                                    if (parts.Length > 1) steamLocation = Path.Combine(Path.GetDirectoryName(parts[0]), @"steamapps\common\Overload");
                                    else steamLocation = null;
                                }
                            }
                        }
                    }
                }

                // Check for a GOG install of Overload.
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games\1309632191"))
                    {
                        if (key != null) gogLocation = (string)key.GetValue("Path");
                        if (!String.IsNullOrEmpty(gogLocation)) if(!File.Exists(Path.Combine(gogLocation, "overload.exe"))) gogLocation = null;
                    }
                }

                // Check for a DVD install of Overload (KickStarter backer DVD).
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\WOW6432Node\Revival Productions, LLC\Overload"))
                    {
                        if (key != null) dvdLocation = (string)key.GetValue("Path");
                        if (!String.IsNullOrEmpty(dvdLocation)) if (!File.Exists(Path.Combine(dvdLocation, "overload.exe"))) dvdLocation = null;
                    }
                }

                string initPath = steamLocation ?? gogLocation ?? dvdLocation;

                if (String.IsNullOrEmpty(initPath)) initPath = "";

                string olmodFileName = Path.Combine(initPath, "olmod.exe");
                string overloadFileName = Path.Combine(initPath, "overload.exe");
                string olproxyFileName = Path.Combine(initPath, "olproxy.exe");

                // Set Overload/Olmod path.
                if (File.Exists(olmodFileName)) OverloadPath = olmodFileName;
                else OverloadPath = overloadFileName;

                // Set Olproxy path.
                OlproxyPath = olproxyFileName;
            }

            OverloadExecutable.Text = OverloadPath;
            OverloadArgs.Text = OverloadParameters;

            OlproxyExecutable.Text = OlproxyPath;
            OlproxyArgs.Text = OlproxyParameters;

            OlproxyExecutable.Text = OlproxyPath;

            UseEmbeddedOlproxy.Checked = OlproxyEmbedded;

            AutoStart.Checked = StartWithWindows;
            UseTrayIcon.Checked = TrayInsteadOfTaskBar;

            AutoStart.Checked = StartWithWindows;
            SelectDark.Checked = DarkTheme;

            ServerName.Text = OlproxyServerName;
            ServerNotes.Text = OlproxyNotes;
            TrackerBaseUrl.Text = OlproxyTrackerBaseUrl;
            SignOff.Checked = OlproxySignOff;
            IsServer.Checked = OlproxyIsServer;

            // The theme colors MUST be set BEFORE attempting to validate settings.
            // This is because ValidateSettings() checks the button colors to see if
            // it is safe to start any of the .exe files.
            SetTheme();

            ValidateSettings();
        }

        private void SetTheme()
        {
            if (DarkTheme)
            {
                // Dark theme colors.
                BackColor = Color.DimGray;
                ForeColor = Color.LightGray;

                activeTextBoxColor = Color.White;
                inactiveTextBoxColor = Color.LightCoral;

                UpdatingMaps.Image = global::OverloadServerTool.Properties.Resources.arrows_light_blue_on_grey;
                OlproxyRunning.Image = global::OverloadServerTool.Properties.Resources.arrows_light_blue_on_grey;
                OverloadRunning.Image = global::OverloadServerTool.Properties.Resources.arrows_light_blue_on_grey;
            }
            else
            {
                // Default textbox colors.
                BackColor = Color.White;
                ForeColor = Color.Black;
            
                activeTextBoxColor =  Color.Black;
                inactiveTextBoxColor = Color.Coral;

                UpdatingMaps.Image = global::OverloadServerTool.Properties.Resources.arrows_blue_on_white;
                OlproxyRunning.Image = global::OverloadServerTool.Properties.Resources.arrows_blue_on_white;
                OverloadRunning.Image = global::OverloadServerTool.Properties.Resources.arrows_blue_on_white;
            }

            // Set the active theme (recursively).
            ApplyThemeToControl(this);

            ValidateSettings();
        }

        /// <summary>
        /// Recursively set control colors based on type.
        /// </summary>
        /// <param name="control"></param>
        private void ApplyThemeToControl(Control control)
        {
            if (control.Controls.Count > 0)
            {
                foreach (Control child in control.Controls)
                {
                    ApplyThemeToControl(child);
                }
            }

            if (control is GroupBox)
            {
                // Set group box title to blue but keep the color of its children to the theme settings.
                control.ForeColor = (DarkTheme) ? Color.LightSkyBlue : Color.RoyalBlue;
                foreach (Control child in control.Controls)
                {
                    if (SelectDark.Checked) child.ForeColor = activeTextBoxColor;
                    else child.ForeColor = activeTextBoxColor;
                }
            }
            else if ((control is TextBox) || (control is ListBox))
            {
                if (SelectDark.Checked)
                {
                    control.BackColor = Color.FromArgb(64, 64, 64);
                    control.ForeColor = activeTextBoxColor;
                }
                else
                {
                    control.BackColor = Color.White;
                    control.ForeColor = activeTextBoxColor;
                }

            }
            else if (control is CheckBox)
            {
                control.ForeColor = ForeColor;
            }
            else if (control is Button)
            {
                ValidateButton(control);
            }
        }

        /// <summary>
        /// Override default enabled/disabled colors for a Button control.
        /// </summary>
        /// <param name="control"></param>
        void ValidateButton(Control control)
        {
            if (control.Enabled)
            {
                control.BackColor = (DarkTheme) ? Color.FromArgb(128, 128, 128) : Color.FromArgb(200, 200, 200);
                control.ForeColor = (DarkTheme) ? Color.FromArgb(255, 255, 255) : Color.FromArgb(64, 64, 64);
            }
            else
            {
                control.BackColor = (DarkTheme) ? Color.FromArgb(96, 96, 96) : Color.FromArgb(96, 96, 96);
                control.ForeColor = (DarkTheme) ? Color.FromArgb(255, 255, 255) : Color.FromArgb(96, 96, 96);
            }
        }
    }
}
