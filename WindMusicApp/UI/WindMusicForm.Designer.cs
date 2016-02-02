namespace WindMusicApp
{
    partial class WindMusicForm
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WindMusicForm));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.axWindowsMediaPlayer1 = new AxWMPLib.AxWindowsMediaPlayer();
            this.timerMusic = new System.Windows.Forms.Timer(this.components);
            this.listViewFolder = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonAdd = new System.Windows.Forms.Button();
            this.buttonDelete = new System.Windows.Forms.Button();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.notifyIconMin = new System.Windows.Forms.NotifyIcon(this.components);
            this.menuNotify = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuNotifyItemSetting = new System.Windows.Forms.ToolStripMenuItem();
            this.listViewMusic = new System.Windows.Forms.ListView();
            this.headerStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.headerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.headerUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonLocal = new System.Windows.Forms.Button();
            this.labelDuration = new System.Windows.Forms.Label();
            this.buttonDeleteSong = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).BeginInit();
            this.menuNotify.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.textBox1, "textBox1");
            this.textBox1.Name = "textBox1";
            // 
            // axWindowsMediaPlayer1
            // 
            resources.ApplyResources(this.axWindowsMediaPlayer1, "axWindowsMediaPlayer1");
            this.axWindowsMediaPlayer1.Name = "axWindowsMediaPlayer1";
            this.axWindowsMediaPlayer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axWindowsMediaPlayer1.OcxState")));
            this.axWindowsMediaPlayer1.Enter += new System.EventHandler(this.axWindowsMediaPlayer1_Enter);
            // 
            // timerMusic
            // 
            this.timerMusic.Enabled = true;
            this.timerMusic.Tick += new System.EventHandler(this.TimerMusic_Tick);
            // 
            // listViewFolder
            // 
            this.listViewFolder.AllowDrop = true;
            this.listViewFolder.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewFolder.HideSelection = false;
            resources.ApplyResources(this.listViewFolder, "listViewFolder");
            this.listViewFolder.Name = "listViewFolder";
            this.listViewFolder.UseCompatibleStateImageBehavior = false;
            this.listViewFolder.View = System.Windows.Forms.View.Details;
            this.listViewFolder.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewFolder_ItemDrag);
            this.listViewFolder.SelectedIndexChanged += new System.EventHandler(this.listViewFolder_SelectedIndexChanged);
            this.listViewFolder.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewFolder_DragDrop);
            this.listViewFolder.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewFolder_DragEnter);
            this.listViewFolder.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewFolder_DragOver);
            this.listViewFolder.DragLeave += new System.EventHandler(this.listViewFolder_DragLeave);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // buttonAdd
            // 
            resources.ApplyResources(this.buttonAdd, "buttonAdd");
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // buttonDelete
            // 
            resources.ApplyResources(this.buttonDelete, "buttonDelete");
            this.buttonDelete.Name = "buttonDelete";
            this.buttonDelete.UseVisualStyleBackColor = true;
            this.buttonDelete.Click += new System.EventHandler(this.buttonDelete_Click);
            // 
            // buttonSearch
            // 
            resources.ApplyResources(this.buttonSearch, "buttonSearch");
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // notifyIconMin
            // 
            this.notifyIconMin.ContextMenuStrip = this.menuNotify;
            resources.ApplyResources(this.notifyIconMin, "notifyIconMin");
            this.notifyIconMin.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIconMin_MouseClick);
            // 
            // menuNotify
            // 
            this.menuNotify.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNotifyItemSetting});
            this.menuNotify.Name = "menuNotify";
            resources.ApplyResources(this.menuNotify, "menuNotify");
            // 
            // menuNotifyItemSetting
            // 
            this.menuNotifyItemSetting.Name = "menuNotifyItemSetting";
            resources.ApplyResources(this.menuNotifyItemSetting, "menuNotifyItemSetting");
            this.menuNotifyItemSetting.Click += new System.EventHandler(this.menuNotifyItemSetting_Click);
            // 
            // listViewMusic
            // 
            this.listViewMusic.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.headerStatus,
            this.headerName,
            this.headerUser});
            this.listViewMusic.HideSelection = false;
            resources.ApplyResources(this.listViewMusic, "listViewMusic");
            this.listViewMusic.Name = "listViewMusic";
            this.listViewMusic.UseCompatibleStateImageBehavior = false;
            this.listViewMusic.View = System.Windows.Forms.View.Details;
            this.listViewMusic.SelectedIndexChanged += new System.EventHandler(this.listViewMusic_SelectedIndexChanged);
            // 
            // headerStatus
            // 
            resources.ApplyResources(this.headerStatus, "headerStatus");
            // 
            // headerName
            // 
            resources.ApplyResources(this.headerName, "headerName");
            // 
            // headerUser
            // 
            resources.ApplyResources(this.headerUser, "headerUser");
            // 
            // buttonLocal
            // 
            resources.ApplyResources(this.buttonLocal, "buttonLocal");
            this.buttonLocal.Name = "buttonLocal";
            this.buttonLocal.UseVisualStyleBackColor = true;
            this.buttonLocal.Click += new System.EventHandler(this.buttonLocal_Click);
            // 
            // labelDuration
            // 
            resources.ApplyResources(this.labelDuration, "labelDuration");
            this.labelDuration.Name = "labelDuration";
            // 
            // buttonDeleteSong
            // 
            resources.ApplyResources(this.buttonDeleteSong, "buttonDeleteSong");
            this.buttonDeleteSong.Name = "buttonDeleteSong";
            this.buttonDeleteSong.UseVisualStyleBackColor = true;
            this.buttonDeleteSong.Click += new System.EventHandler(this.buttonDeleteSong_Click);
            // 
            // WindMusicForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonDeleteSong);
            this.Controls.Add(this.labelDuration);
            this.Controls.Add(this.buttonLocal);
            this.Controls.Add(this.listViewMusic);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.buttonDelete);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.listViewFolder);
            this.Controls.Add(this.axWindowsMediaPlayer1);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.Name = "WindMusicForm";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.WindMusicForm_Load);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WindMusicForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.WindMusicForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WindMusicForm_MouseUp);
            ((System.ComponentModel.ISupportInitialize)(this.axWindowsMediaPlayer1)).EndInit();
            this.menuNotify.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private AxWMPLib.AxWindowsMediaPlayer axWindowsMediaPlayer1;
        private System.Windows.Forms.Timer timerMusic;
        private System.Windows.Forms.ListView listViewFolder;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.Button buttonDelete;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.NotifyIcon notifyIconMin;
        private System.Windows.Forms.ContextMenuStrip menuNotify;
        private System.Windows.Forms.ToolStripMenuItem menuNotifyItemSetting;
        private System.Windows.Forms.ListView listViewMusic;
        private System.Windows.Forms.ColumnHeader headerStatus;
        private System.Windows.Forms.ColumnHeader headerName;
        private System.Windows.Forms.ColumnHeader headerUser;
        private System.Windows.Forms.Button buttonLocal;
        private System.Windows.Forms.Label labelDuration;
        private System.Windows.Forms.Button buttonDeleteSong;
    }
}

