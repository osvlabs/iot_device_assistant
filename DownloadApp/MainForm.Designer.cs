namespace DownloadApp
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.lblversion = new System.Windows.Forms.Label();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.progress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.decrypt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblID = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTips = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.imgLock = new System.Windows.Forms.PictureBox();
            this.imgMqtt = new System.Windows.Forms.PictureBox();
            this.imgLockStatus = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgLock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgMqtt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLockStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem1,
            this.ToolStripMenuItem2});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(124, 48);
            // 
            // ToolStripMenuItem1
            // 
            this.ToolStripMenuItem1.Name = "ToolStripMenuItem1";
            this.ToolStripMenuItem1.Size = new System.Drawing.Size(123, 22);
            this.ToolStripMenuItem1.Text = "退出";
            this.ToolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItem1_Click);
            // 
            // ToolStripMenuItem2
            // 
            this.ToolStripMenuItem2.Name = "ToolStripMenuItem2";
            this.ToolStripMenuItem2.Size = new System.Drawing.Size(123, 22);
            this.ToolStripMenuItem2.Text = "查看日志";
            this.ToolStripMenuItem2.Click += new System.EventHandler(this.ToolStripMenuItem2_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "Device Assistant";
            this.notifyIcon1.Visible = true;
            // 
            // lblversion
            // 
            this.lblversion.AutoSize = true;
            this.lblversion.Location = new System.Drawing.Point(594, 11);
            this.lblversion.Name = "lblversion";
            this.lblversion.Size = new System.Drawing.Size(0, 17);
            this.lblversion.TabIndex = 3;
            // 
            // listViewFiles
            // 
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.name,
            this.progress,
            this.decrypt});
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point(18, 43);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.Size = new System.Drawing.Size(576, 288);
            this.listViewFiles.TabIndex = 7;
            this.listViewFiles.UseCompatibleStateImageBehavior = false;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            // 
            // name
            // 
            this.name.Text = "文件名";
            this.name.Width = 400;
            // 
            // progress
            // 
            this.progress.Text = "下载进度";
            // 
            // decrypt
            // 
            this.decrypt.Text = "解密状态";
            // 
            // lblID
            // 
            this.lblID.AutoSize = true;
            this.lblID.Location = new System.Drawing.Point(455, 336);
            this.lblID.Name = "lblID";
            this.lblID.Size = new System.Drawing.Size(43, 17);
            this.lblID.TabIndex = 10;
            this.lblID.Text = "label2";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(499, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(56, 17);
            this.label2.TabIndex = 11;
            this.label2.Text = "联网状态";
            // 
            // lblTips
            // 
            this.lblTips.AutoSize = true;
            this.lblTips.Location = new System.Drawing.Point(102, 23);
            this.lblTips.Name = "lblTips";
            this.lblTips.Size = new System.Drawing.Size(20, 17);
            this.lblTips.TabIndex = 13;
            this.lblTips.Text = "无";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 17);
            this.label4.TabIndex = 12;
            this.label4.Text = "当前下载任务：";
            // 
            // imgLock
            // 
            this.imgLock.Image = ((System.Drawing.Image)(resources.GetObject("imgLock.Image")));
            this.imgLock.Location = new System.Drawing.Point(16, 334);
            this.imgLock.Name = "imgLock";
            this.imgLock.Size = new System.Drawing.Size(50, 22);
            this.imgLock.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgLock.TabIndex = 14;
            this.imgLock.TabStop = false;
            // 
            // imgMqtt
            // 
            this.imgMqtt.Location = new System.Drawing.Point(561, 21);
            this.imgMqtt.Name = "imgMqtt";
            this.imgMqtt.Size = new System.Drawing.Size(20, 20);
            this.imgMqtt.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgMqtt.TabIndex = 9;
            this.imgMqtt.TabStop = false;
            // 
            // imgLockStatus
            // 
            this.imgLockStatus.Image = global::DownloadApp.Properties.Resources.ok;
            this.imgLockStatus.Location = new System.Drawing.Point(67, 333);
            this.imgLockStatus.Name = "imgLockStatus";
            this.imgLockStatus.Size = new System.Drawing.Size(20, 20);
            this.imgLockStatus.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgLockStatus.TabIndex = 15;
            this.imgLockStatus.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(611, 361);
            this.Controls.Add(this.imgLockStatus);
            this.Controls.Add(this.imgLock);
            this.Controls.Add(this.lblTips);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblID);
            this.Controls.Add(this.imgMqtt);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.lblversion);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Device Assistant";
            this.Load += new System.EventHandler(this.Main_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imgLock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgMqtt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imgLockStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem2;
        private System.Windows.Forms.Label lblversion;
        private System.Windows.Forms.ListView listViewFiles;
        private System.Windows.Forms.ColumnHeader name;
        private System.Windows.Forms.ColumnHeader progress;
        private System.Windows.Forms.ColumnHeader decrypt;
        private System.Windows.Forms.PictureBox imgMqtt;
        private System.Windows.Forms.Label lblID;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTips;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox imgLock;
        private System.Windows.Forms.PictureBox imgLockStatus;
    }
}

