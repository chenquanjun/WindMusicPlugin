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
        private MusicPlayer player = null;

        public WindMusicForm()
        {
            InitializeComponent();
        }

        private void WindMisicForm_Load(object sender, EventArgs e)
        {
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

            player = new MusicPlayer();
            player.SetMusicPlayer(this.axWindowsMediaPlayer1);
            player.SetTimer(this.timerMusic);

            //folder list
            listViewFolder.Columns.Add("文件路径:");
            player.AddFolderEvent += new MusicEventAddFolderHandler(onAddFolderEvent);
        }
        private void onAddFolderEvent(string folderName)
        {
            listViewFolder.Items.Add(folderName);
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


        private void button1_Click(object sender, EventArgs e)
        {
            
            try{
                watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
                MessageBox.Show("取消订阅广播成功!");
            }
            catch 
            {
                Debug.WriteLine("取消订阅广播失败，服务器已经关闭！");
            }
              
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog of = new OpenFileDialog();
            of.InitialDirectory = "c:\\";
            of.Filter = "mp3|*.mp3|wav|*.wav";
            of.RestoreDirectory = true;
            of.FilterIndex = 1;
            of.Multiselect = false;
            if (of.ShowDialog() == DialogResult.OK)
            {
                var fileName = of.FileName;
                player.SetMusicFileName(fileName);
                player.play();
            }
        }

        private void timerMusic_Tick(object sender, EventArgs e)
        {
            
        }

        private void listViewFolder_SelectedIndexChanged(object sender, EventArgs e)
        {

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
                player.AddMusicFolder(foldPath);
            }
        }

        private void listViewFolder_DragDrop(object sender, DragEventArgs e)
        {
            // 返回插入标记的索引值  
            int index = listViewFolder.InsertionMark.Index;
            // 如果插入标记不可见，则退出.  
            if (index == -1)
            {
                return;
            }
            // 如果插入标记在项目的右面，使目标索引值加一  
            if (listViewFolder.InsertionMark.AppearsAfterItem)
            {
                index++;
            }

            // 返回拖拽项  
            ListViewItem item = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
            //在目标索引位置插入一个拖拽项目的副本   
            listViewFolder.Items.Insert(index, (ListViewItem)item.Clone());
            // 移除拖拽项目的原文件  
            listViewFolder.Items.Remove(item);  
        }

        private void listViewFolder_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.AllowedEffect; 
        }

        private void listViewFolder_DragLeave(object sender, EventArgs e)
        {
            listViewFolder.InsertionMark.Index = -1;  
        }

        private void listViewFolder_DragOver(object sender, DragEventArgs e)
        {
            // 获得鼠标坐标  
            Point point = listViewFolder.PointToClient(new Point(e.X, e.Y));
            // 返回离鼠标最近的项目的索引  
            int index = listViewFolder.InsertionMark.NearestIndex(point);
            // 确定光标不在拖拽项目上  
            if (index > -1)
            {
                Rectangle itemBounds = listViewFolder.GetItemRect(index);
                if (point.X > itemBounds.Left + (itemBounds.Width / 2))
                {
                    listViewFolder.InsertionMark.AppearsAfterItem = true;
                }
                else
                {
                    listViewFolder.InsertionMark.AppearsAfterItem = false;
                }
            }
            listViewFolder.InsertionMark.Index = index;  
        }
    }
}
