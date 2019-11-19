using System;
using System.Collections.Generic;
using System.IO; 
using System.Management;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Configuration;
using System.Text;

namespace DownloadApp
{
    public class UDisk
    {  
        public UDisk() {

        }

        public void Start() {
            ManagementEventWatcher watcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            watcher.Query = query;
            watcher.Start();
            //watcher.WaitForNextEvent();
        }

        string findZipFile(string path) {
            string[] files = Directory.GetFiles(path, "*offline.zip");
            if (files.Length > 0)
            {
                return files[0];
            }
            else {
                return "";
            }
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try {
                string disk = "";

                foreach (var item in e.NewEvent.Properties) {
                    if (item.Name == "DriveName") {
                        disk = item.Value.ToString();
                    }
                }
                string videoPath = disk + "\\";
                if (Directory.Exists(videoPath)) {

                    string zipFile = findZipFile(videoPath);
                    if (zipFile == "") {
                        return;
                    }
                    videoPath = videoPath + Path.GetFileName(zipFile).Replace(".zip", "") + "\\";
                    
                    Tool.Log("发现U盘媒体文件，开始解压");

                    bool unzipDone = Tool.Unzip(zipFile, videoPath);
                    if (!unzipDone) {
                        return;
                    }
                    
                    Tool.Log("U盘媒体文件解压完毕，开始解密");

                    string[] encryptJson = Directory.GetFiles(videoPath, "*encrypt.json");
                    if (encryptJson.Length == 0) {
                        return;
                    }
                    string json = Tool.ReadFile(encryptJson[0]);
                    dynamic dyPayload = JsonConvert.DeserializeObject<dynamic>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    Dictionary<string, string> dicKeys = new Dictionary<string, string>();
                    foreach (dynamic media in dyPayload)
                    {
                        if (!dicKeys.ContainsKey((string)media.encrypt_file_name))
                        {
                            dicKeys.Add(Tool.GetMd5Hash((string)media.encrypt_file_name), (string)media.key);
                        }
                    }

                    // copy play-list.* 
                    string playlistFileName = "";

                    string[] playlistAlias = GlobalObj.playlistAlias.Split('|');
                    foreach (var name in playlistAlias ) {
                        if (File.Exists(videoPath + name))
                        {
                            playlistFileName = name;
                        }
                    } 

                    if (!Tool.DecryptFile(videoPath + playlistFileName, Path.Combine( GlobalObj.dataPath, playlistFileName.Replace(".phx", "") + ".tmp"), dicKeys[Tool.GetMd5Hash(playlistFileName)], ""))
                    {
                        // send failed message.
                        Tool.Log("U盘文件解密失败");
                    }

                    System.Threading.Thread.Sleep(2000);

                    // copy videos 
                    string[] encryptedFiles = Directory.GetFiles(videoPath);
                    foreach (var f in encryptedFiles) {
                        if (!f.ToString().Contains(playlistFileName) && !f.ToString().Contains("encrypt.json")) {
                            var key = Tool.GetMd5Hash(Path.GetFileName(f));
                            Tool.DecryptFile(f, Path.Combine(GlobalObj.videosPath, Path.GetFileName(f).Replace(".phx", "")), dicKeys[key], ""); 
                        }
                    }

                    Tool.Log(encryptedFiles.Length + "个媒体文件解密完毕");

                    System.Threading.Thread.Sleep(2000);

                    var file = Path.Combine(GlobalObj.dataPath, playlistFileName.Replace(".phx", ""));
                    if (File.Exists(file)) {
                        File.Delete(file);
                    }

                    // update play-list.*
                    var pfile = Path.Combine( GlobalObj.dataPath, playlistFileName.Replace(".phx", "") + ".tmp");
                    File.Move(pfile, pfile.Replace(".tmp", ""));
                    
                    Tool.Log("U盘离线播放资源已全部拷贝就位");
                    
                    string content = "{ \"command\": \"notify\", \"flag\": \"completed\", \"file_name\": \"" + playlistFileName.Replace(".phx", "") + "\" }";
                    GlobalObj._m2client.Publish(GlobalObj._topic + "/notify", Encoding.UTF8.GetBytes(content));
                     
                }
            }
            catch (Exception ex) {
                Tool.Log("U盘解密、拷贝文件错误: " + ex.Message );
            }
        }
    }
}
