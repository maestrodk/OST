using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Forms;

namespace OverloadServerTool
{
    internal class OverloadMap
    {
        public string Url;
        public DateTime DateTime;
        public int Size = 0;

        public OverloadMap(string url, DateTime dateTime, int size)
        {
            this.Url = url;
            this.DateTime = dateTime;
            this.Size = size;
        }
    }

    public class OverloadMapManager
    {
        private const string defaultOnlineMapList = @"https://www.overloadmaps.com/data/mp.json";

        public delegate void LogMessageDelegate(string message);
        private LogMessageDelegate logger = null;
        private LogMessageDelegate loggerError = null;

        public void SetLogger(LogMessageDelegate logger = null, LogMessageDelegate errorLogger = null)
        {
            this.logger = logger;
            if (this.loggerError == null)
            {
                if (errorLogger == null) this.loggerError = logger;
                else this.loggerError = errorLogger;
            }
        }

        void LogMessage(string s)
        {
            logger?.Invoke(s);
            Debug.WriteLine(s);
        }

        void LogErrorMessage(string s)
        {
            loggerError?.Invoke(s);
            Debug.WriteLine(s);
        }

        public OverloadMapManager()
        {
        }

        /// <summary>
        /// Get online master map list.
        /// </summary>
        public bool Update(string onlineMapList = null, string overloadDLCLocation = null)
        {
            if (String.IsNullOrEmpty(onlineMapList)) onlineMapList = defaultOnlineMapList;

            string overloadMapLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Revival\Overload";
            
            List<OverloadMap> master = new List<OverloadMap>();

            try
            {
                // Make sure Revival Overload map directory exists.
                Directory.CreateDirectory(overloadMapLocation);

                // Make sure Revival DLC directory exists.
                if (!String.IsNullOrEmpty(overloadDLCLocation)) Directory.CreateDirectory(overloadDLCLocation);

                // Get base so we can construct full url to the online ZIP files.
                Uri uri = new Uri(onlineMapList); 
                string baseUrl = String.Concat(uri.Scheme, Uri.SchemeDelimiter, uri.Host); 

                // Get map list and build internal map master list.
                string json = new WebClient().DownloadString(onlineMapList);
                dynamic mapList = JsonConvert.DeserializeObject(json);
                foreach (var map in mapList)
                {
                    string url = baseUrl + map.url;
                    string time = map.mtime;
                    string size = map.size;

                    master.Add(new OverloadMap(url, UnixTimeStampToDateTime(Convert.ToDouble(time)), Convert.ToInt32(size)));
                }
            }
            catch (Exception ex)
            {
                LogErrorMessage(String.Format($"Cannot read master map list: {ex.Message}"));
                return false;
            }

            // Iterate master list and download all new/updated maps.
            int ok = 0, bad = 0;
            for (int i = 0; i < master.Count; i++)
            {
                if (UpdateMap(overloadMapLocation, overloadDLCLocation, master[i])) ok++;
                else bad++;
            }
           return true;
        }

        private bool UpdateMap(string overloadMapLocation, string overloadDLCLocation, OverloadMap map)
        {
            Uri uri = new Uri(map.Url);
            string mapZipName = uri.Segments[uri.Segments.Length - 1];

            try
            {
                // Update map.
                if (!String.IsNullOrEmpty(mapZipName) && mapZipName.ToLower().EndsWith(".zip"))
                {
                    string localDLCFileName = null;

                    string localZipFileName = Path.Combine(overloadMapLocation, mapZipName);
                    localZipFileName = WebUtility.UrlDecode(localZipFileName);

                    bool updateDLC = false;
                    bool update = false;

                    // First the DLC location (if enabled).
                    if (!String.IsNullOrEmpty(overloadDLCLocation))
                    {
                        localDLCFileName = Path.Combine(overloadDLCLocation, mapZipName);
                        localDLCFileName = WebUtility.UrlDecode(localDLCFileName);
                        updateDLC = UpdateMapFile(localDLCFileName, map);
                    }

                    // Now check the ProgramData Revival folder.
                    update = UpdateMapFile(localZipFileName, map);

                    // Check results.
                    if (updateDLC)
                    {
                        LogMessage(String.Format($"Updating map (using DLC folder): {WebUtility.UrlDecode(mapZipName)}"));
                        UpdateLocalMap(map.Url, localDLCFileName, map.DateTime);
                    }
                    else if (update)
                    {
                        LogMessage(String.Format($"Updating map: {WebUtility.UrlDecode(mapZipName)}"));
                        UpdateLocalMap(map.Url, localZipFileName, map.DateTime);
                    }
                }

                // No errors or map doesn't need updating.
                return true;
            }
            catch (Exception ex)
            {
                LogErrorMessage(String.Format($"Error updating map {mapZipName}: {ex.Message}"));
                return false;
            }
        }

        private bool UpdateMapFile(string localFileName, OverloadMap map)
        {
            string localDirectory = Path.GetDirectoryName(localFileName);
            if (!Directory.Exists(localDirectory)) return false;

            if (File.Exists(localFileName))
            {
                // Map ZIP file found, check date and size.
                FileInfo fi = new FileInfo(localFileName);

                if (fi.Length != map.Size) return true;
                else if (fi.LastWriteTime != map.DateTime) return true;

                return false;
            }
            else
            {
                // Directory exists but ZIP file does not.
                return true;
            }
        }

        /// <summary>
        /// Download map ZIP to local directory and set local ZIP file time to match online map creation time (mtime).
        /// </summary>
        /// <param name="url"></param>
        /// <param name="localName"></param>
        /// <param name="dateTime"></param>
        private void UpdateLocalMap(string url, string localName, DateTime dateTime)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile(url, localName);
                    File.SetCreationTime(localName, dateTime);
                    File.SetLastWriteTime(localName, dateTime);
                }
                catch (Exception ex)
                {
                    LogErrorMessage(String.Format($"Error downloading map ZIP {localName}: {ex.Message}"));
                }
            }
        }

        /// <summary>
        /// Convert from UNIX time to .NET DateTime.
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch.
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}