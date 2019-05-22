using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;
using Newtonsoft.Json;
using OverloadServerApplication;

namespace OverloadServerTool
{
    public partial class OSTMain : Form
    {
        private void AutoUpdateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            AutoUpdateOST = AutoUpdateCheckBox.Checked;
        }

        private void ForceUpdateButton_Click(object sender, EventArgs e)
        {
            SaveSettings();
            UpdateCheck(debugFileName, true);
        }

        public static OSTRelease GetLastestRelease
        {
            get
            {
                string jsonOverloadServerUrl = @"https://api.github.com/repos/maestrodk/ost/releases/latest";

                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = (Binder, certificate, chain, errors) => { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    string json = "";

                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add("User-Agent", "OST - user " + WindowsIdentity.GetCurrent().Name);
                        json = wc.DownloadString(jsonOverloadServerUrl);
                    }

                    dynamic releaseInfo = JsonConvert.DeserializeObject(json);

                    string zipUrl = releaseInfo.assets[0].browser_download_url;
                    long size = Convert.ToInt64(releaseInfo.assets[0].size);
                    DateTime created = Convert.ToDateTime(releaseInfo.assets[0].created_at, CultureInfo.InvariantCulture);
                    string version = releaseInfo.tag_name;

                    OSTRelease release = new OSTRelease();
                    release.DownloadUrl = zipUrl;
                    release.Size = size;
                    release.Created = created;
                    release.Version = version;

                    return release;
                }
                catch (Exception ex)
                {
                }

                return null;
            }
        }

        public void UpdateCheck(string debugFileName, bool forceUpdate = false)
        {
            try
            {
                OverloadServerToolApplication.LogDebugMessage("Checking for new OST release.", debugFileName);

                OSTRelease release = GetLastestRelease;
                if (release != null)
                {
                    OverloadServerToolApplication.LogDebugMessage("Got release info - checking version.", debugFileName);

                    // Fix version numbers so they are both in xx.yy.zz format (3 components and numbers only).
                    string newVersion = OverloadServerToolApplication.VersionStringFix(release.Version);
                    string[] newVersionSplit = newVersion.Split(".".ToCharArray());
                    if (newVersionSplit.Length > 3) newVersion = newVersionSplit[0] + "." + newVersionSplit[1] + "." + newVersionSplit[2];

                    string currentVersion = null;
                    using (var process = Process.GetCurrentProcess()) currentVersion = OverloadServerToolApplication.GetFileVersion(process.MainModule.FileName);
                    string[] currentVersionSplit = currentVersion.Split(".".ToCharArray());
                    if (currentVersionSplit.Length > 3) currentVersion = currentVersionSplit[0] + "." + currentVersionSplit[1] + "." + currentVersionSplit[2];

                    // Check if update is available.
                    if (forceUpdate || (!String.IsNullOrEmpty(currentVersion) && (currentVersion != newVersion)))
                    {
                        PerformUpdate(release, currentVersion, newVersion, Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory));
                    }
                    else
                    {
                        OverloadServerToolApplication.LogDebugMessage(String.Format($"No update available - continuing OST startup.", debugFileName));
                    }
                }
                else
                {
                    OverloadServerToolApplication.LogDebugMessage("Unable to get OST release info.", debugFileName);
                }
            }
            catch (Exception ex)
            {
                OverloadServerToolApplication.LogDebugMessage(String.Format($"Unable to check/perform OST update: {ex.Message}"), debugFileName);
            }
        }

        public void PerformUpdate(OSTRelease release, string currentVersion, string newVersion, string installFolder)
        {
            OSTUpdateForm updateForm = new OSTUpdateForm(release, currentVersion, newVersion, installFolder);

            ApplyThemeToControl(updateForm, theme);
            updateForm.StartPosition = FormStartPosition.CenterParent;

            if (updateForm.ShowDialog() == DialogResult.Cancel) return;

            string localTempZip = Path.GetTempFileName() + "_OST_Update.zip";
            string localTempFolder = Path.GetTempFileName() + "_OST_Update";
            Directory.CreateDirectory(localTempFolder);

            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("User-Agent", "Overload Server Tool - user " + WindowsIdentity.GetCurrent().Name);
                    wc.DownloadFile(release.DownloadUrl, localTempZip);
                }

                ZipFile.ExtractToDirectory(localTempZip, localTempFolder);
                try { System.IO.File.Delete(localTempZip); } catch { }

                string localSourceFolder = localTempFolder;

                // If the ZIP contains a folder then the files to copy will be inside this.
                DirectoryInfo subDirList = new DirectoryInfo(localTempFolder);
                DirectoryInfo[] subDirs = subDirList.GetDirectories();
                if (subDirs.Length > 0) localSourceFolder = subDirs[0].FullName;

                Process appStart = new Process();
                appStart.StartInfo = new ProcessStartInfo(Path.Combine(localSourceFolder, "Updater.exe"));
                appStart.StartInfo.Arguments = String.Format($"-installfolder \"{installFolder}\"");
                appStart.StartInfo.WorkingDirectory = localSourceFolder;

                // Do graceful shutdown before launching the updater.
                Main_FormClosing(null, null);

                appStart.Start();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format($"Unable to update OST: {ex.Message}", "Error"));
            }
        }
    }
}
