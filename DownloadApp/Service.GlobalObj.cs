using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using uPLibrary.Networking.M2Mqtt;

namespace DownloadApp
{
    public static class GlobalObj
    {
        public static MqttClient _m2client;
        public static string _topic;

        public static string version = ConfigurationManager.AppSettings["version"].ToString();

        public static string playerPath = ConfigurationManager.AppSettings["player_path"].ToString();
        public static string dataPath   = ConfigurationManager.AppSettings["data_path"].ToString();
        public static string videosPath = Path.Combine(dataPath, "videos");
        public static string logPath    = Path.Combine(dataPath, "download_logs");

        public static string playlistAlias = ConfigurationManager.AppSettings["playlistalias"].ToString();

        public static bool isScreenLocked = false;

    }
}
