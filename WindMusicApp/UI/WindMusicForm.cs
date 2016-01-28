using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Collections;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

using System.Diagnostics;

using Wayfarer.BroadCast.Common;

namespace WindMusicApp
{
    public partial class WindMusicForm : Form
    {
        private IBroadCast watch = null;
        private EventWrapper wrapper = null;
       
        private MusicControl m_musicControl = null;

        public WindMusicForm()
        {
            InitializeComponent();
        }

        private void WindMusicForm_Load(object sender, EventArgs e)
        {
            /*
            try
            {
                BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
                BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
                serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

                IDictionary props = new Hashtable();
                props["port"] = 0;
                HttpChannel channel = new HttpChannel(props, clientProvider, serverProvider);
                ChannelServices.RegisterChannel(channel, false);

                watch = (IBroadCast)Activator.GetObject(
                    typeof(IBroadCast), "http://localhost:8080/BroadCastMessage.soap");

                wrapper = new EventWrapper();
                wrapper.LocalBroadCastEvent += new BroadCastEventHandler(BroadCastingMessage);
                wrapper.LocalBroadCastDanmakuArgsEvent += new BroadCastDanmakuArgsEventHandler(BroadCastingDanmakuArgsMessage);
                watch.BroadCastEvent += new BroadCastEventHandler(wrapper.BroadCasting);
                watch.BroadCastDanmakuArgsEvent += new BroadCastDanmakuArgsEventHandler(wrapper.BroadCastingDanmakuArgs);
            }
            catch
            {

            }
            */

            //取消订阅
            //watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);

            //music player
            var player = new MusicHelper();
            player.SetMusicPlayer(this.axWindowsMediaPlayer1);
            player.SetTimer(this.timerMusic);

            //music control
            m_musicControl = new MusicControl(player, this);

            //music event handler
            m_musicControl.AddFolderEvent += new MusicEventAddFolderHandler(onAddFolderEvent);
            m_musicControl.RemoveFolderEvent += new MusicEventRemoveFolderHandler(onRemoveFolderEvent);
            m_musicControl.DemandInfoEvent += new MusicEventDemandInfoHandler(onDemandInfo);

            //button state
            buttonDelete.Enabled = false; 

        }


        private void onDemandInfo(DemandInfo info)
        {
            var queueId = info.QueueId.ToString();
            var status = info.Status;

            var items = listViewMusic.Items;
            ListViewItem item = items[queueId];

            if (item == null)
            {
                if (status != DemandSongStatus.Queue) //已经被删除
                {
                    return;
                }
                else //初始化
                {
                    item = new ListViewItem();
                    item.Name = queueId;
                    item.SubItems.Add(info.Keyword);
                    item.SubItems.Add(info.UserName);

                    listViewMusic.Items.Add(item);
                }
            }


            var isError = info.isError();
            

            if (isError)
            {
                string tipsStr = null;
                switch (info.Error)
                {
                    case DemandSongError.Full:
                        tipsStr = "曲单已满";
                        break;
                    case DemandSongError.Search:
                        tipsStr = "找不到歌曲";
                        break;
                    case DemandSongError.GetDetail:
                        tipsStr = "获取歌曲信息失败";
                        break;
                    case DemandSongError.Download:
                        tipsStr = "下载失败";
                        break;
                    case DemandSongError.Cancel:
                        tipsStr = "取消";
                        break;
                    default:
                        break;
                }

                if (tipsStr != null)
                {
                    item.Text = tipsStr;
                }
                Debug.WriteLine("SongInfo Error: " + info.Keyword + " status:" + info.Status + " error:" + info.Error);
            }
            else
            {
                var song = info.SongInfo;
                string tipsStr = null;
                switch (status)
                {
                    case DemandSongStatus.Queue:
                        tipsStr = "等待处理";
                        break;
                    case DemandSongStatus.Search:
                        tipsStr = "搜索中";
                        break;
                    case DemandSongStatus.GetDetail:
                        tipsStr = "获取信息中"; 
                        break;
                    case DemandSongStatus.Download:
                        tipsStr = "下载中";
                        item.SubItems[1].Text = song.Name;
                        break;
                    case DemandSongStatus.WaitPlay:
                        tipsStr = "等待播放";
                        break;
                    case DemandSongStatus.Playing:
                        tipsStr = "播放中";
                        break;
                    case DemandSongStatus.PlayEnd:
                        tipsStr = "播放完毕";
                        break;
                    default:
                        break;
                }
                if (tipsStr != null)
                {
                    item.Text = tipsStr;
                }
                Debug.WriteLine("SongInfo: " + info.Keyword + " status:" + status);
            }
        }


        private void onAddFolderEvent(string folderName)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = folderName;
            lvi.ImageKey = folderName;
            listViewFolder.Items.Add(lvi);  
        }

        private void onRemoveFolderEvent(string folderName)
        {
            var items = listViewFolder.Items;
            var removeIdx = -1;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var name = item.Text;
                if (folderName.CompareTo(name) == 0) {
                    removeIdx = i;
                    break;
                }
            }

            if (removeIdx != -1)
            {
                listViewFolder.Items.RemoveAt(removeIdx);
            }
        }

        private void BroadCastingMessage(string message)
        {
            //MessageBox.Show(message);
            Debug.WriteLine(message);
        }

        private delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            // InvokeRequired需要比较调用线程ID和创建线程ID
            // 如果它们不相同则返回true
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text += text;
            }
        }

        private void BroadCastingDanmakuArgsMessage(BroadcastEventArgs e)
        {
            //var damaku = e.DanmakuArgs.Danmaku;
            //foreach (System.Reflection.PropertyInfo p in damaku.GetType().GetProperties())
            //{
            //    Debug.WriteLine("Name:{0} Value:{1}", p.Name, p.GetValue(damaku, null));
            //}

            //MessageBox.Show(damaku.CommentText);
            var damaku = e.DanmakuArgs;
            var danmu = damaku.Danmaku;

            SetText("CommentText:" + danmu.CommentText);
            SetText(System.Environment.NewLine);

            //textBox1.Text += "CommentText:" + danmu.CommentText;
            //textBox1.Text += System.Environment.NewLine;
            //textBox1.Text += "CommentUser:" + danmu.CommentUser;
            //textBox1.Text += System.Environment.NewLine;
            //textBox1.Text += "isAdmin:" + danmu.isAdmin;
            //textBox1.Text += System.Environment.NewLine;
            //textBox1.Text += "GiftName:" + danmu.GiftName;
            //textBox1.Text += System.Environment.NewLine;
            //textBox1.Text += "GiftNum:" + danmu.GiftNum;
            //textBox1.Text += System.Environment.NewLine;


        }


        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private void TimerMusic_Tick(object sender, EventArgs e)
        {
            m_musicControl.TimerMusic_Tick(sender, e);
        }

        private void listViewFolder_SelectedIndexChanged(object sender, EventArgs e)
        {

            var focusedItem = listViewFolder.FocusedItem;
            buttonDelete.Enabled = focusedItem.Selected;
        }

        private void listViewFolder_ItemDrag(object sender, ItemDragEventArgs e)
        {
            listViewFolder.DoDragDrop(e.Item, DragDropEffects.Move);  
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择音乐文件夹路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string foldPath = dialog.SelectedPath;
                m_musicControl.AddMusicFolder(foldPath); 
            }
        }

        private void listViewFolder_DragDrop(object sender, DragEventArgs e)
        {
            string fullPath = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();

            if (System.IO.File.Exists(fullPath)) //如果是文件的话，则截取
            {
                string path = fullPath.Substring(0, fullPath.LastIndexOf("\\"));
                m_musicControl.AddMusicFolder(path);
            }
            else
            {
                m_musicControl.AddMusicFolder(fullPath);
            }

        }

        private void listViewFolder_DragEnter(object sender, DragEventArgs e)
        {
            //e.Effect = e.AllowedEffect; 
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else e.Effect = DragDropEffects.None; 
        }

        private void listViewFolder_DragLeave(object sender, EventArgs e)
        {

        }

        private void listViewFolder_DragOver(object sender, DragEventArgs e)
        {

        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            var selectItems = listViewFolder.SelectedItems;
            var count = selectItems.Count;
            if (count > 0)
            {
                var selectItem = selectItems[0];
                var text = selectItem.Text;
                m_musicControl.removeMusicFolder(text);
            }
            buttonDelete.Enabled = false;

        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("buttonSearch_Click" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            var text = textBox1.Text;
            //searchHelper.downloadSongList(text);
            m_musicControl.OnRcvDamaku("test", text);
        }

        Point m_mouseOff;//鼠标移动位置变量
        bool m_leftFlag;//标记是否为左键

        private void WindMusicForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                m_leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void WindMusicForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(m_mouseOff.X, m_mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void WindMusicForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_leftFlag)
            {
                m_leftFlag = false;//释放鼠标后标注为false;
            }
        }

        private void notifyIconMin_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                HideMainForm();
            }
            else if (this.WindowState == FormWindowState.Minimized)
            {
                ShowMainForm();
            }
        }

        private void HideMainForm()
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide();
        }

        private void ShowMainForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void menuNotifyItemSetting_Click(object sender, EventArgs e)
        {

        }

    }
}
