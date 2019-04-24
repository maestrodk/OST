using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OverloadServerTool
{
    public partial class Main 
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

        public bool TrayOnly
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
            OverloadExecutable.Text = OverloadPath;
            OverloadArgs.Text = OverloadParameters;

            OlproxyExecutable.Text = OlproxyPath;
            OlproxyArgs.Text = OlproxyParameters;

            OlproxyExecutable.Text = OlproxyPath;

            UseEmbeddedOlproxy.Checked = OlproxyEmbedded;

            AutoStart.Checked = StartWithWindows;
            UseTrayIcon.Checked = TrayOnly;

            AutoStart.Checked = StartWithWindows;
            SelectDark.Checked = DarkTheme;

            ServerName.Text = OlproxyServerName;
            ServerNotes.Text = OlproxyNotes;
            TrackerBaseUrl.Text = OlproxyTrackerBaseUrl;
            SignOff.Checked = OlproxySignOff;
            IsServer.Checked = OlproxyIsServer;

            SelectDarkTheme();
        }

        private void SelectDarkTheme()
        {
            if (DarkTheme)
            {
                // Dark theme colors.
                BackColor = Color.DimGray;
                ForeColor = Color.LightGray;

                activeTextBoxColor = Color.White;
                inactiveTextBoxColor = Color.LightCoral;
            }
            else
            {
                // Default textbox colors.
                BackColor = Color.White;
                ForeColor = Color.Black;
            
                activeTextBoxColor =  Color.Black;
                inactiveTextBoxColor = Color.Coral;
            }

            SetDarkThemeColors(this);

            ValidateSettings();
        }

        private void SetDarkThemeColors(Control control)
        {
            if (control.Controls.Count > 0)
            {
                foreach (Control child in control.Controls)
                {
                    SetDarkThemeColors(child);
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
