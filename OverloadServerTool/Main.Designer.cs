namespace OverloadServerTool
{
    partial class OSTMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.OverloadExecutable = new System.Windows.Forms.TextBox();
            this.OverloadArgs = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.OlproxyExecutable = new System.Windows.Forms.TextBox();
            this.OlproxyArgs = new System.Windows.Forms.TextBox();
            this.AutoStart = new System.Windows.Forms.CheckBox();
            this.UseTrayIcon = new System.Windows.Forms.CheckBox();
            this.SelectExecutable = new System.Windows.Forms.OpenFileDialog();
            this.StartButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.StopExitButton = new System.Windows.Forms.Button();
            this.SelectDark = new System.Windows.Forms.CheckBox();
            this.ActivityLogListBox = new System.Windows.Forms.ListBox();
            this.UseEmbeddedOlproxy = new System.Windows.Forms.CheckBox();
            this.OverloadGroupBox = new System.Windows.Forms.GroupBox();
            this.OlproxyGroupBox = new System.Windows.Forms.GroupBox();
            this.IsServer = new System.Windows.Forms.CheckBox();
            this.ServerName = new System.Windows.Forms.TextBox();
            this.ServerNotes = new System.Windows.Forms.TextBox();
            this.TrackerBaseUrl = new System.Windows.Forms.TextBox();
            this.SignOff = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.OptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.LoggingGroupBox = new System.Windows.Forms.GroupBox();
            this.ActionsGroupBox = new System.Windows.Forms.GroupBox();
            this.OverloadServerToolNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.OverloadGroupBox.SuspendLayout();
            this.OlproxyGroupBox.SuspendLayout();
            this.OptionsGroupBox.SuspendLayout();
            this.LoggingGroupBox.SuspendLayout();
            this.ActionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Args";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Path";
            // 
            // OverloadExecutable
            // 
            this.OverloadExecutable.BackColor = System.Drawing.Color.White;
            this.OverloadExecutable.Location = new System.Drawing.Point(13, 39);
            this.OverloadExecutable.Margin = new System.Windows.Forms.Padding(1);
            this.OverloadExecutable.Name = "OverloadExecutable";
            this.OverloadExecutable.Size = new System.Drawing.Size(241, 20);
            this.OverloadExecutable.TabIndex = 1;
            this.OverloadExecutable.TextChanged += new System.EventHandler(this.OverloadExecutable_TextChanged);
            this.OverloadExecutable.DoubleClick += new System.EventHandler(this.OverloadExecutable_MouseDoubleClick);
            // 
            // OverloadArgs
            // 
            this.OverloadArgs.Location = new System.Drawing.Point(13, 82);
            this.OverloadArgs.Margin = new System.Windows.Forms.Padding(2);
            this.OverloadArgs.Name = "OverloadArgs";
            this.OverloadArgs.Size = new System.Drawing.Size(241, 20);
            this.OverloadArgs.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Parameters";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Path";
            // 
            // OlproxyExecutable
            // 
            this.OlproxyExecutable.Location = new System.Drawing.Point(13, 38);
            this.OlproxyExecutable.Margin = new System.Windows.Forms.Padding(2);
            this.OlproxyExecutable.Name = "OlproxyExecutable";
            this.OlproxyExecutable.Size = new System.Drawing.Size(222, 20);
            this.OlproxyExecutable.TabIndex = 3;
            this.OlproxyExecutable.TextChanged += new System.EventHandler(this.OlproxyExecutable_TextChanged);
            this.OlproxyExecutable.DoubleClick += new System.EventHandler(this.OlproxyExecutable_DoubleClick);
            // 
            // OlproxyArgs
            // 
            this.OlproxyArgs.Location = new System.Drawing.Point(13, 81);
            this.OlproxyArgs.Margin = new System.Windows.Forms.Padding(2);
            this.OlproxyArgs.Name = "OlproxyArgs";
            this.OlproxyArgs.Size = new System.Drawing.Size(222, 20);
            this.OlproxyArgs.TabIndex = 4;
            // 
            // AutoStart
            // 
            this.AutoStart.AutoSize = true;
            this.AutoStart.Location = new System.Drawing.Point(17, 25);
            this.AutoStart.Name = "AutoStart";
            this.AutoStart.Size = new System.Drawing.Size(136, 17);
            this.AutoStart.TabIndex = 5;
            this.AutoStart.Text = "Start at Windows logon";
            this.AutoStart.UseVisualStyleBackColor = true;
            this.AutoStart.CheckedChanged += new System.EventHandler(this.AutoStart_CheckedChanged);
            // 
            // UseTrayIcon
            // 
            this.UseTrayIcon.AutoSize = true;
            this.UseTrayIcon.Checked = true;
            this.UseTrayIcon.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UseTrayIcon.Location = new System.Drawing.Point(17, 48);
            this.UseTrayIcon.Name = "UseTrayIcon";
            this.UseTrayIcon.Size = new System.Drawing.Size(98, 17);
            this.UseTrayIcon.TabIndex = 7;
            this.UseTrayIcon.Text = "Minimize to tray";
            this.UseTrayIcon.UseVisualStyleBackColor = true;
            this.UseTrayIcon.CheckedChanged += new System.EventHandler(this.AutoTray_CheckedChanged);
            // 
            // SelectExecutable
            // 
            this.SelectExecutable.FileName = "SelectExecutable";
            this.SelectExecutable.Filter = "Applications|*.exe";
            // 
            // StartButton
            // 
            this.StartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartButton.Location = new System.Drawing.Point(12, 26);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(72, 32);
            this.StartButton.TabIndex = 9;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExitButton.Location = new System.Drawing.Point(12, 166);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(74, 30);
            this.ExitButton.TabIndex = 9;
            this.ExitButton.Text = "Exit";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopButton.Location = new System.Drawing.Point(12, 75);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(74, 30);
            this.StopButton.TabIndex = 9;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // StopExitButton
            // 
            this.StopExitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopExitButton.Location = new System.Drawing.Point(12, 122);
            this.StopExitButton.Name = "StopExitButton";
            this.StopExitButton.Size = new System.Drawing.Size(74, 31);
            this.StopExitButton.TabIndex = 9;
            this.StopExitButton.Text = "Stop + Exit";
            this.StopExitButton.UseVisualStyleBackColor = true;
            this.StopExitButton.Click += new System.EventHandler(this.StopExitButton_Click);
            // 
            // SelectDark
            // 
            this.SelectDark.AutoSize = true;
            this.SelectDark.Location = new System.Drawing.Point(173, 25);
            this.SelectDark.Name = "SelectDark";
            this.SelectDark.Size = new System.Drawing.Size(81, 17);
            this.SelectDark.TabIndex = 5;
            this.SelectDark.Text = "Dark theme";
            this.SelectDark.UseVisualStyleBackColor = true;
            this.SelectDark.CheckedChanged += new System.EventHandler(this.SelectDark_CheckedChanged);
            // 
            // ActivityLogListBox
            // 
            this.ActivityLogListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ActivityLogListBox.Font = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ActivityLogListBox.ForeColor = System.Drawing.Color.Black;
            this.ActivityLogListBox.FormattingEnabled = true;
            this.ActivityLogListBox.Location = new System.Drawing.Point(21, 28);
            this.ActivityLogListBox.Margin = new System.Windows.Forms.Padding(0);
            this.ActivityLogListBox.Name = "ActivityLogListBox";
            this.ActivityLogListBox.Size = new System.Drawing.Size(827, 236);
            this.ActivityLogListBox.TabIndex = 0;
            this.ActivityLogListBox.TabStop = false;
            this.ActivityLogListBox.MouseLeave += new System.EventHandler(this.ActivityLogListBox_MouseLeave);
            // 
            // UseEmbeddedOlproxy
            // 
            this.UseEmbeddedOlproxy.AutoSize = true;
            this.UseEmbeddedOlproxy.Location = new System.Drawing.Point(252, 156);
            this.UseEmbeddedOlproxy.Name = "UseEmbeddedOlproxy";
            this.UseEmbeddedOlproxy.Size = new System.Drawing.Size(136, 17);
            this.UseEmbeddedOlproxy.TabIndex = 5;
            this.UseEmbeddedOlproxy.Text = "Use embedded Olproxy";
            this.UseEmbeddedOlproxy.UseVisualStyleBackColor = true;
            this.UseEmbeddedOlproxy.CheckedChanged += new System.EventHandler(this.UseEmbeddedOlproxy_CheckedChanged);
            // 
            // OverloadGroupBox
            // 
            this.OverloadGroupBox.Controls.Add(this.OverloadExecutable);
            this.OverloadGroupBox.Controls.Add(this.label1);
            this.OverloadGroupBox.Controls.Add(this.label2);
            this.OverloadGroupBox.Controls.Add(this.OverloadArgs);
            this.OverloadGroupBox.Location = new System.Drawing.Point(18, 17);
            this.OverloadGroupBox.Name = "OverloadGroupBox";
            this.OverloadGroupBox.Size = new System.Drawing.Size(272, 120);
            this.OverloadGroupBox.TabIndex = 11;
            this.OverloadGroupBox.TabStop = false;
            this.OverloadGroupBox.Text = "Overload";
            // 
            // OlproxyGroupBox
            // 
            this.OlproxyGroupBox.Controls.Add(this.IsServer);
            this.OlproxyGroupBox.Controls.Add(this.UseEmbeddedOlproxy);
            this.OlproxyGroupBox.Controls.Add(this.ServerName);
            this.OlproxyGroupBox.Controls.Add(this.ServerNotes);
            this.OlproxyGroupBox.Controls.Add(this.TrackerBaseUrl);
            this.OlproxyGroupBox.Controls.Add(this.SignOff);
            this.OlproxyGroupBox.Controls.Add(this.OlproxyExecutable);
            this.OlproxyGroupBox.Controls.Add(this.label5);
            this.OlproxyGroupBox.Controls.Add(this.label3);
            this.OlproxyGroupBox.Controls.Add(this.label7);
            this.OlproxyGroupBox.Controls.Add(this.label6);
            this.OlproxyGroupBox.Controls.Add(this.label4);
            this.OlproxyGroupBox.Controls.Add(this.OlproxyArgs);
            this.OlproxyGroupBox.Location = new System.Drawing.Point(305, 17);
            this.OlproxyGroupBox.Name = "OlproxyGroupBox";
            this.OlproxyGroupBox.Size = new System.Drawing.Size(458, 213);
            this.OlproxyGroupBox.TabIndex = 12;
            this.OlproxyGroupBox.TabStop = false;
            this.OlproxyGroupBox.Text = "Olproxy";
            // 
            // IsServer
            // 
            this.IsServer.AutoSize = true;
            this.IsServer.Location = new System.Drawing.Point(252, 109);
            this.IsServer.Name = "IsServer";
            this.IsServer.Size = new System.Drawing.Size(168, 17);
            this.IsServer.TabIndex = 8;
            this.IsServer.Text = "Make server visible on tracker";
            this.IsServer.UseVisualStyleBackColor = true;
            // 
            // ServerName
            // 
            this.ServerName.Location = new System.Drawing.Point(252, 38);
            this.ServerName.Margin = new System.Windows.Forms.Padding(2);
            this.ServerName.Name = "ServerName";
            this.ServerName.Size = new System.Drawing.Size(181, 20);
            this.ServerName.TabIndex = 7;
            // 
            // ServerNotes
            // 
            this.ServerNotes.Location = new System.Drawing.Point(13, 128);
            this.ServerNotes.Margin = new System.Windows.Forms.Padding(2);
            this.ServerNotes.Multiline = true;
            this.ServerNotes.Name = "ServerNotes";
            this.ServerNotes.Size = new System.Drawing.Size(222, 68);
            this.ServerNotes.TabIndex = 7;
            // 
            // TrackerBaseUrl
            // 
            this.TrackerBaseUrl.Location = new System.Drawing.Point(252, 81);
            this.TrackerBaseUrl.Margin = new System.Windows.Forms.Padding(2);
            this.TrackerBaseUrl.Name = "TrackerBaseUrl";
            this.TrackerBaseUrl.Size = new System.Drawing.Size(181, 20);
            this.TrackerBaseUrl.TabIndex = 7;
            // 
            // SignOff
            // 
            this.SignOff.AutoSize = true;
            this.SignOff.Location = new System.Drawing.Point(252, 132);
            this.SignOff.Name = "SignOff";
            this.SignOff.Size = new System.Drawing.Size(197, 17);
            this.SignOff.TabIndex = 6;
            this.SignOff.Text = "Remove inactive server from tracker";
            this.SignOff.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(249, 66);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Tracker URL";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(10, 113);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Notes";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(251, 24);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Server name";
            // 
            // OptionsGroupBox
            // 
            this.OptionsGroupBox.Controls.Add(this.AutoStart);
            this.OptionsGroupBox.Controls.Add(this.SelectDark);
            this.OptionsGroupBox.Controls.Add(this.UseTrayIcon);
            this.OptionsGroupBox.Location = new System.Drawing.Point(18, 148);
            this.OptionsGroupBox.Name = "OptionsGroupBox";
            this.OptionsGroupBox.Size = new System.Drawing.Size(270, 82);
            this.OptionsGroupBox.TabIndex = 13;
            this.OptionsGroupBox.TabStop = false;
            this.OptionsGroupBox.Text = "Options";
            // 
            // LoggingGroupBox
            // 
            this.LoggingGroupBox.Controls.Add(this.ActivityLogListBox);
            this.LoggingGroupBox.Location = new System.Drawing.Point(18, 246);
            this.LoggingGroupBox.Name = "LoggingGroupBox";
            this.LoggingGroupBox.Size = new System.Drawing.Size(863, 287);
            this.LoggingGroupBox.TabIndex = 14;
            this.LoggingGroupBox.TabStop = false;
            this.LoggingGroupBox.Text = "Activity Log";
            // 
            // ActionsGroupBox
            // 
            this.ActionsGroupBox.Controls.Add(this.StartButton);
            this.ActionsGroupBox.Controls.Add(this.ExitButton);
            this.ActionsGroupBox.Controls.Add(this.StopButton);
            this.ActionsGroupBox.Controls.Add(this.StopExitButton);
            this.ActionsGroupBox.Location = new System.Drawing.Point(782, 17);
            this.ActionsGroupBox.Name = "ActionsGroupBox";
            this.ActionsGroupBox.Size = new System.Drawing.Size(99, 214);
            this.ActionsGroupBox.TabIndex = 15;
            this.ActionsGroupBox.TabStop = false;
            this.ActionsGroupBox.Text = "Actions";
            // 
            // OverloadServerToolNotifyIcon
            // 
            this.OverloadServerToolNotifyIcon.Text = "Overload Server Tool";
            this.OverloadServerToolNotifyIcon.Visible = true;
            this.OverloadServerToolNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.OverloadServerToolNotifyIcon_MouseDoubleClick);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(909, 543);
            this.Controls.Add(this.ActionsGroupBox);
            this.Controls.Add(this.LoggingGroupBox);
            this.Controls.Add(this.OptionsGroupBox);
            this.Controls.Add(this.OlproxyGroupBox);
            this.Controls.Add(this.OverloadGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Text = "Overload Server Tool";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Load += new System.EventHandler(this.Main_Load);
            this.MouseEnter += new System.EventHandler(this.Main_MouseEnter);
            this.Resize += new System.EventHandler(this.Main_Resize);
            this.OverloadGroupBox.ResumeLayout(false);
            this.OverloadGroupBox.PerformLayout();
            this.OlproxyGroupBox.ResumeLayout(false);
            this.OlproxyGroupBox.PerformLayout();
            this.OptionsGroupBox.ResumeLayout(false);
            this.OptionsGroupBox.PerformLayout();
            this.LoggingGroupBox.ResumeLayout(false);
            this.ActionsGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox OverloadExecutable;
        private System.Windows.Forms.TextBox OverloadArgs;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox OlproxyExecutable;
        private System.Windows.Forms.TextBox OlproxyArgs;
        private System.Windows.Forms.CheckBox AutoStart;
        private System.Windows.Forms.CheckBox UseTrayIcon;
        private System.Windows.Forms.OpenFileDialog SelectExecutable;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button StopExitButton;
        private System.Windows.Forms.CheckBox SelectDark;
        private System.Windows.Forms.ListBox ActivityLogListBox;
        private System.Windows.Forms.CheckBox UseEmbeddedOlproxy;
        private System.Windows.Forms.GroupBox OverloadGroupBox;
        private System.Windows.Forms.GroupBox OlproxyGroupBox;
        private System.Windows.Forms.GroupBox OptionsGroupBox;
        private System.Windows.Forms.GroupBox LoggingGroupBox;
        private System.Windows.Forms.GroupBox ActionsGroupBox;
        private System.Windows.Forms.TextBox TrackerBaseUrl;
        private System.Windows.Forms.CheckBox SignOff;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox IsServer;
        private System.Windows.Forms.TextBox ServerName;
        private System.Windows.Forms.TextBox ServerNotes;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NotifyIcon OverloadServerToolNotifyIcon;
    }
}

