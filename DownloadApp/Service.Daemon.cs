using System; 
using System.Diagnostics;
using System.Linq; 
using System.Timers; 
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DownloadApp
{
    public class Daemon
    { 
        static string appPath = ConfigurationManager.AppSettings["player_exe"].ToString();
        static string _kill_process = ConfigurationManager.AppSettings["kill_process"].ToString();

        public Daemon()
        {
            
        }

        [DllImport("user32.dll")]
        static extern bool BlockInput(bool fBlockIt);

        public void Watch() { 

            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
             
            Tool.Log("watch started");
        } 

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                foreach (var p in Process.GetProcesses()) {
                    if (_kill_process.Split('|').Contains(p.ProcessName)) {
                        p.Kill();
                        Tool.Log("发现 " + p.ProcessName + " 进程，已清除");
                    }
                }

                if (Process.GetProcesses().Where( p => p.ProcessName.Contains("phoenix-player")).Count() == 0) {
                   // Tool.Log("重新启动 播放器 进程");
                   // System.Diagnostics.Process.Start(appPath);
                }
            }
            catch (Exception ex) {
                Tool.Log("进程查杀错误:" + ex.Message);
            }
        } 

        
    }
}
