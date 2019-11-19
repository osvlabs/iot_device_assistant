using System.ComponentModel; 
using System.Text; 
using System.IO; 
using System.Configuration; 
using Newtonsoft.Json; 
using System.Net;
using System.Collections.Generic;
using System;
using uPLibrary.Networking.M2Mqtt; 
using uPLibrary.Networking.M2Mqtt.Messages; 
using System.Security.Cryptography.X509Certificates; 
using System.Timers;
using System.Linq;

namespace DownloadApp
{
    public class Downloader
    {   
        static string _mac; 
        static string _playlist_filename;

        static int _totalFiles = 0;
        static int _repeatFiles = 0;
        static int _downloadedFiles = 0;
        static int _downloadedFailedFiles = 0;

        static string _mqttServer;
        static int    _mqttPort;
        static string _mqttSsl;
        static string _username;
        static string _password;
        static string _clientId;

        static X509Certificate clientCert;
        static X509Certificate caCert;

        public static List<MediaFile> lsDownloadFiles;

        public Downloader() {
            Init();
        }

        void Init() {
            try
            {  
                _mqttServer = ConfigurationManager.AppSettings["mqttServer"].ToString();
                _mqttPort   = int.Parse(ConfigurationManager.AppSettings["mqttPort"].ToString());
                _mqttSsl    = ConfigurationManager.AppSettings["mqttSsl"].ToString();
                _username   = ConfigurationManager.AppSettings["username"].ToString();
                _password   = ConfigurationManager.AppSettings["password"].ToString();
                _mac        = ConfigurationManager.AppSettings["mac"].ToString();
                 
                var configFile = Path.Combine(GlobalObj.dataPath, "config\\config.json");
                if (File.Exists( configFile ))
                {
                    string strConfig    = Tool.ReadFile(configFile);
                    dynamic dyConfig    = JsonConvert.DeserializeObject<dynamic>(strConfig);
                    _mac                = (string)dyConfig.mac_address;
                }

                _clientId   = ConfigurationManager.AppSettings["clientId"].ToString().Replace("[mac]", _mac);
                GlobalObj._topic      = ConfigurationManager.AppSettings["topic"].ToString().Replace("[mac]", _mac);

                lsDownloadFiles = new List<MediaFile>();
                
                clientCert  = new X509Certificate2( Path.Combine( GlobalObj.playerPath, "local\\certs_prod\\client.pfx"));
                caCert      = X509Certificate.CreateFromCertFile( Path.Combine( GlobalObj.playerPath, "local\\certs_prod\\ca-cert.crt"));

                X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(new X509Certificate2(caCert));
                store.Close();

                Timer timer = new Timer(10000);
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
            }
            catch (Exception ex) {
                Tool.Log("Init Exception: " + ex.Message + "; inner exception:" + ex.InnerException.Message);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            publish("clients/downloads/status", "{ \"downloader_id\": \""+_mac+ "\", \"status\": \"1\", \"time\": \""+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +"\"  }");

            if (!GlobalObj._m2client.IsConnected) {
                //Tool.Log("mqtt offline, reconnecting...");
                this.Listen();
            }
        }

        void Reset() {
            lsDownloadFiles.Clear();
            _totalFiles = 0;
            _repeatFiles = 0;
            _downloadedFiles = 0;
            _downloadedFailedFiles = 0;
            listWC.ForEach(wc =>
            {
                wc.CancelAsync();
            });
            listWC.Clear();
        }

        public void Listen() {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                if (_mqttSsl == "1") {
                    GlobalObj._m2client = new MqttClient(_mqttServer, _mqttPort, true, caCert, clientCert, MqttSslProtocols.TLSv1_2);
                }
                else {
                    GlobalObj._m2client = new MqttClient( _mqttServer );
                }
                GlobalObj._m2client.Connect(_clientId, _username, _password, true, 60);
                GlobalObj._m2client.Subscribe(new string[] { GlobalObj._topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
                GlobalObj._m2client.ConnectionClosed += _m2client_ConnectionClosed;
                GlobalObj._m2client.MqttMsgPublishReceived += _m2client_MqttMsgPublishReceived;
                Tool.Log("MQTT 连接 " + (GlobalObj._m2client.IsConnected ? "成功":"失败") );
                Tool.Log("MQTT client ID " + _mac);
            }
            catch (Exception ex)
            {
                Tool.Log("main method error: " + ex.Message + "; inner exception:" + ex.InnerException.Message);
            }
        }
         
        private void _m2client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string payload = System.Text.Encoding.UTF8.GetString(e.Message);
            
            try
            {
                if (payload != "0" && payload != "")
                {
                    Tool.Log("收到新的播放列表"); 
                    
                    Reset();

                    dynamic dyPayload = JsonConvert.DeserializeObject<dynamic>(payload); 

                    var playlist_url = (string)dyPayload.playlist_file.url;
                    var key = (string)dyPayload.playlist_file.key;
                    var hash = (string)dyPayload.playlist_file.hash;

                    string saveFile = getSaveFile(playlist_url);

                    using (WebClient wc = new WebClient())
                    {
                        wc.DownloadFileCompleted += (oo, ee) => {
                            try
                            {
                                System.Threading.Thread.Sleep(2000);

                                if (Tool.DecryptFile(saveFile, saveFile.Replace(".phx", "") + ".tmp", key, hash))
                                {
                                    System.Threading.Thread.Sleep(2000);
                                    _playlist_filename = Path.GetFileName(new Uri(playlist_url).LocalPath).Replace(".phx", "");
                                    //download media files      
                                    downloadMedia(payload);
                                }
                            }
                            catch (Exception ex)
                            {
                                Tool.Log("Download play-list error: " + ex.Message);
                            }
                        };
                        wc.DownloadFileAsync(
                            new System.Uri(playlist_url),
                            saveFile
                        );
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Tool.Log("data process error: " + ex.Message.ToString());
                Tool.Log("data process: " + payload);
            } 
        }

        private void _m2client_ConnectionClosed(object sender, EventArgs e)
        {
            Tool.Log("连接被关闭，尝试重新连接...");
            this.Listen();
        }

        void publish(string topic, string message) {
            try {
                if (message == "")
                {
                    GlobalObj._m2client.Publish(topic, new byte[0]);
                }
                else {
                    GlobalObj._m2client.Publish(topic, Encoding.UTF8.GetBytes(message));
                }
            }
            catch (Exception ex) {
                Tool.Log("MQTT消息发送失败"+ex.Message);
            }
        }

        string getSaveFile(string uri)
        {
            string path = GlobalObj.videosPath;
            if (uri.Contains(".json") || uri.Contains(".lay"))
            {
                path = GlobalObj.dataPath;
            }
            return Path.Combine( path, Path.GetFileName(new Uri(uri).LocalPath) );
        }
         

        bool isExist(string filename, string hash) {
            var path = Path.Combine(GlobalObj.videosPath, filename);
            if ( File.Exists(path) )
            {
                if (Tool.GetMD5HashFromFile(path) == hash)
                {
                    _repeatFiles++;
                    Tool.Log(filename + " 文件已存在，跳过下载");
                    return true;
                }
            } 
            return false; 
        }

        static List<WebClient> listWC = new List<WebClient>();
        static Dictionary<string, string> listMedias = new Dictionary<string, string>();

        void downloadMedia(string payload)
        {
            try
            {
                _totalFiles = 0;
                _repeatFiles = 0;
                _downloadedFiles = 0;
                _downloadedFailedFiles = 0;

                dynamic dyPayload = JsonConvert.DeserializeObject<dynamic>(payload); 

                listMedias = new Dictionary<string, string>();
                
                foreach (dynamic media in dyPayload.media_file)
                {
                    _totalFiles++;

                    var filename = Path.GetFileName(new Uri((string)media.url).LocalPath).Replace(".phx", "");
                    
                    if (!listMedias.ContainsValue((string)media.url) && (string)media.url != ""  )
                    {
                        bool fileExist = isExist(filename, (string)media.hash);
                        if (!fileExist) {
                            listMedias.Add((string)media.hash, (string)media.url);
                        }
                        lsDownloadFiles.Add(
                            new MediaFile() { 
                                Url = (string)media.url, 
                                Key = (string)media.key, 
                                Progress = fileExist?"已存在":"-", 
                                Decrypt = "-" }
                            );
                    }
                    
                }

                if (listMedias.Count == 0) {
                    notifyDownloadComplete();
                    return;
                }

                Tool.Log("播放列表包含 "+_totalFiles+" 个文件, 开始下载... ");
                
                publish(GlobalObj._topic + "/message", "{ \"downloader_id\": \"" + _mac + "\", \"file_name\": \"播放机收到下载任务，共包含" + _totalFiles+"个文件:\",\"progress\": \"0\", \"status\": \"1\", \"command\": \"message\" }");

                download(0);
                dicInProcess = new List<string>();
            }
            catch (Exception ex)
            {
                Tool.Log("download media error: " + ex.Message);
            }
        }

        

        void download(int index)
        {
            if (listMedias.Count <= index) {
                return;
            }

            var item = listMedias.ElementAt(index);
            var url = item.Value;
            var hash = item.Key;

            try
            {   
                using (WebClient wc = new NewWebClient(30 * 60 * 1000))
                {   
                    wc.DownloadFileCompleted += (o, e) => Wc_DownloadFileCompleted(o, e, url, hash, index);
                    wc.DownloadProgressChanged += (o, e) => Wc_DownloadProgressChanged(o, e, url);
                    wc.DownloadFileAsync(
                       new System.Uri(url),
                       getSaveFile(url)
                    );
                    listWC.Add(wc);
                }
            }
            catch (Exception ex)
            {
                string originFileName = Path.GetFileName(new Uri(url).LocalPath).Replace(".phx", "");
                
                publish(GlobalObj._topic + "/message", "{ \"downloader_id\": \"" + _mac + "\", \"file_name\": \"" + originFileName + "\",\"progress\": \"0\", \"status\": \"-1\", \"command\": \"message\" }");

                Tool.Log("download error: " + ex.Message);
            }
        }

        static List<string> dicInProcess = new List<string>();
        void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e, string url, string hash, int index)
        {
            if (!dicInProcess.Contains(url))
            {
                dicInProcess.Add(url);
            }
            else {
                return;
            }

            index++;

            download(index);

            try
            {
                if (lsDownloadFiles.Where(m=>m.Url == url).Count() == 0) {
                    return;
                }

                string originFileName = Path.GetFileName(new Uri(url).LocalPath).Replace(".phx", "");
                
                string encriptedFilePath = getSaveFile(url);

                var key = lsDownloadFiles.Where(m => m.Url == url).First().Key;

                bool decryptSuccess = Tool.DecryptFile(encriptedFilePath, encriptedFilePath.Replace(".phx", ""), key, hash);
                if (!decryptSuccess){
                    Tool.Log("解密失败，第二次解密：" + originFileName);
                    decryptSuccess = Tool.DecryptFile(encriptedFilePath, encriptedFilePath.Replace(".phx", ""), key, hash);
                }
                if (!decryptSuccess){
                    Tool.Log("解密失败，第三次解密：" + originFileName);
                    decryptSuccess = Tool.DecryptFile(encriptedFilePath, encriptedFilePath.Replace(".phx", ""), key, hash);
                } 

                if (decryptSuccess)
                {
                    _downloadedFiles++;
                    Tool.Log(_downloadedFiles + " / " + _totalFiles + " " + originFileName + " 下载完毕");
                    publish(GlobalObj._topic + "/notify", "{ \"command\": \"notify\", \"flag\": \"" + originFileName + "\" }");
                    publish(GlobalObj._topic + "/message", "{ \"downloader_id\": \"" + _mac + "\", \"file_name\": \"" + originFileName + "\",\"progress\": \"100\", \"status\": \"1\", \"command\": \"message\" }");

                    // if download all media files, set play-list.json.tmp to play-list.json  
                    if (_repeatFiles + _downloadedFiles + _downloadedFailedFiles >= _totalFiles)
                    {
                        notifyDownloadComplete();
                    }
                }
                else {
                    _downloadedFailedFiles++;
                    Tool.Log(originFileName + " 下载完，无法解密。");
                    publish(GlobalObj._topic + "/message", "{ \"downloader_id\": \"" + _mac + "\", \"file_name\": \"" + originFileName + "\",\"progress\": \"0\", \"status\": \"-1\", \"command\": \"message\" }");
                }
                
                for (int i = 0; i < lsDownloadFiles.Count(); i++)
                {
                    var file = lsDownloadFiles[i];
                    if (file.Url == url)
                    {
                        lsDownloadFiles[i] = new MediaFile() { Url = url, Key = key, Progress = "100%", Decrypt = (decryptSuccess ? "成功" : "失败") };
                    }
                }
                System.Threading.Thread.Sleep(5000);
            }
            catch (Exception ex)
            { 
                Tool.Log("媒体文件下载完成后解密时出错: " + ex.Message);
            }
        }

        void notifyDownloadComplete() {
            try {
                var playlistFileName = Path.Combine( GlobalObj.dataPath, _playlist_filename);
                if (File.Exists(playlistFileName))
                {
                    File.Delete(playlistFileName);
                }
                File.Move(playlistFileName + ".tmp", playlistFileName);

                string downloadlog = "共计" + _totalFiles + "个文件，"+_repeatFiles+"个已存在，下载成功" + _downloadedFiles + "个，下载失败" + _downloadedFailedFiles + "个";

                publish(GlobalObj._topic + "/notify", "{ \"command\": \"notify\", \"flag\": \"completed\", \"file_name\": \"" + _playlist_filename + "\" }");
                publish(GlobalObj._topic + "/message", "{ \"downloader_id\": \"" + _mac + "\", \"file_name\": \"" + downloadlog+"\",\"progress\": \"0\", \"status\": \"1\", \"command\": \"message\" }");
                publish(GlobalObj._topic, "");

                if (File.ReadAllLines(playlistFileName).Length == 0) {
                    publish(GlobalObj._topic + "/message", "{ \"downloader_id\": \"" + _mac + "\", \"file_name\": \"播放列表文件异常，请重新下发\",\"progress\": \"0\", \"status\": \"-1\", \"command\": \"message\" }");
                }

                Tool.Log(downloadlog + ", 列表：" + _playlist_filename);

                Reset();

                deleteOtherVideos();
            }
            catch { }
        }

        void deleteOtherVideos() {
            var jsonContent = ""; 
            var files = Directory.GetFiles(GlobalObj.dataPath, "*.*").Where(s => s.EndsWith(".json") || s.EndsWith(".lay"));
            foreach (var file in files) {
                jsonContent += File.ReadAllText(file);
            }
            var count = 0;
            foreach (var video in Directory.GetFiles(GlobalObj.videosPath)) {
                var filename = Path.GetFileName(video);
                if (!jsonContent.Contains(filename)) {
                    try {
                        count++;
                        Tool.Log("清理文件 " + filename);
                        File.Delete(video);
                    } catch { }
                }
            }
            Tool.Log(count + "个文件清理完毕");
        }

         void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, string url)
        {
            string originFileName = Path.GetFileName(new Uri(url).LocalPath).Replace(".phx", "");
            var currentSec = DateTime.Now.Second.ToString();
            if ( currentSec.Contains("0") )
            { 
                publish(GlobalObj._topic + "/message", "{ \"downloader_id\": \"" + _mac + "\", \"file_name\": \"" + originFileName + "\",\"progress\": \""+e.ProgressPercentage+ "\", \"status\": \"0\", \"command\": \"message\", \"totals\": \""+_totalFiles+"\" }");
                
                float percent = (float)e.BytesReceived / (float)e.TotalBytesToReceive ;
                string progress = Math.Round(percent * 100, 2) + "%";
                Tool.Log( progress + "(" + e.BytesReceived / 1024 / 1024 + "/"+ e.TotalBytesToReceive / 1024 / 1024 + "mb)-"+ originFileName);

                for (int i=0; i< lsDownloadFiles.Count();i++) {
                    var file = lsDownloadFiles[i];
                    if (file.Url == url) {
                        lsDownloadFiles[i] = new MediaFile() { Url=url, Key=file.Key, Progress=progress, Decrypt="-" };
                    }
                } 
            }
        }
         
    }
}
