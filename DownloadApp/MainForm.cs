using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace DownloadApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += Main_FormClosing;
            this.notifyIcon1.MouseDoubleClick += NotifyIcon1_MouseDoubleClick;
        }

        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        { 
            this.Show();
            WindowState = FormWindowState.Normal;
            this.Focus();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing) {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                notifyIcon1.Visible = true;
                this.Hide();
                return;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {
            try {

                this.displayLog();
                
                Downloader _downloader = new Downloader(); 
                _downloader.Listen();
                Tool.Log("下载进程已启动");

                Daemon daemon = new Daemon();
                daemon.Watch();
                Tool.Log("播放器守护进程已启动");

                UDisk udisk = new UDisk();
                udisk.Start();
                Tool.Log("U盘离线文件拷贝进程启动");

                MouseKeyboard.Hook_Start();
                Tool.Log("鼠标键盘锁进程启动");

                this.lblID.Text = GlobalObj._topic.Replace("clients", "").Replace("downloads", "");
                this.Text += " " + GlobalObj.version;
            }
            catch (Exception ex) { 
                Tool.Log(ex.Message);
            }
        }

        

        [DllImport("user32.dll")]
        static extern bool BlockInput(bool fBlockIt);

        private void BtnLock_Click(object sender, EventArgs e)
        {
            BlockInput(true);
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Close();
            this.Dispose();
            Application.Exit();
            Environment.Exit(0);
        }
         

        private void displayLog() {
            try {
                System.Windows.Forms.Timer checktimer = new System.Windows.Forms.Timer();
                checktimer.Interval = 2000;
                checktimer.Tick += (o, e) =>
                {
                    try
                    {
                        var logName = Path.Combine(GlobalObj.logPath, "Log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");
                        if (File.Exists(logName)) {
                            //this.txtLogs.Text = File.ReadAllText( logName );
                            //this.txtLogs.SelectionStart = this.txtLogs.TextLength;
                            //this.txtLogs.ScrollToCaret();

                            if (Tool.GetLastInputTime() >= 5 * 60)
                            {
                                if (!GlobalObj.isScreenLocked)
                                {
                                    BlockInput(true);
                                    GlobalObj.isScreenLocked = true;
                                    Tool.Log("电脑超过5分钟未操作，自动锁定屏幕。");
                                }
                            }
                        }
                    }
                    catch {}

                    try {
                        if (GlobalObj._m2client.IsConnected)
                        {
                            this.imgMqtt.ImageLocation = AppDomain.CurrentDomain.BaseDirectory + @"/image/green.png";
                        }
                        else {
                            this.imgMqtt.ImageLocation = AppDomain.CurrentDomain.BaseDirectory + @"/image/gray.png";
                        }
                        if (GlobalObj.isScreenLocked)
                        {
                            this.imgLockStatus.ImageLocation = AppDomain.CurrentDomain.BaseDirectory + @"/image/forbidden.png";
                        }
                        else {
                            this.imgLockStatus.ImageLocation = AppDomain.CurrentDomain.BaseDirectory + @"/image/ok.png";
                        }
                        
                    } catch { }

                    try {

                        if (Downloader.lsDownloadFiles.Count > 0)
                        {
                            this.lblTips.Text = Downloader.lsDownloadFiles.Count + "个文件";
                            foreach (var item in Downloader.lsDownloadFiles)
                            {
                                var name = Path.GetFileName(new Uri(item.Url).LocalPath).Replace(".phx", "");
                                ListViewItem findItem = this.listViewFiles.FindItemWithText(name);
                                if (findItem != null) {
                                    findItem.SubItems.Clear();
                                    findItem.Text = name;
                                    findItem.SubItems.Add(item.Progress);
                                    findItem.SubItems.Add(item.Decrypt);
                                }
                                else
                                {
                                    ListViewItem lvi = new ListViewItem();
                                    lvi.Text = name;
                                    lvi.SubItems.Add(item.Progress);
                                    lvi.SubItems.Add(item.Decrypt);
                                    this.listViewFiles.Items.Add(lvi);
                                }
                            }
                        }
                    } catch { }
                };
                checktimer.Enabled = true;
                checktimer.Start();
            }
            catch {}
        } 

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("notepad.exe", Path.Combine(GlobalObj.logPath, "Log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt"));
        }
         
    }
}
