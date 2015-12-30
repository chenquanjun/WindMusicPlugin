using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Collections;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Serialization.Formatters;

using Wayfarer.BroadCast.Common;
using Wayfarer.BroadCast.RemoteObject;


namespace WindTestPluginApp
{
    public partial class ServerForm : Form
    {
        private static BroadCastObj Obj = null;
        public ServerForm()
        {
            InitializeComponent();

            initServer();
        }

        private void initServer()
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary props = new Hashtable();
            props["port"] = 8080;
            HttpChannel channel = new HttpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel, false);

            #region 客户端订阅服务端事件

            Obj = new BroadCastObj();
            ObjRef objRef = RemotingServices.Marshal(Obj, "BroadCastMessage.soap");

            #endregion
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Obj.BroadCastingInfo("haha");


            var danmuRawData = "{\"info\":[[0,1,25,16777215,1450888087,\"1450888075\",0,\"082b9a9b\",0],\"O_O\",[245166,\"往事如风\",0,\"0\"],[],[20,5685],[]],\"cmd\":\"DANMU_MSG\",\"roomid\":42728}";
            BilibiliDM_PluginFramework.DanmakuModel danmaku = new BilibiliDM_PluginFramework.DanmakuModel(danmuRawData, 2);
  
            var danmu = new BilibiliDM_PluginFramework.ReceivedDanmakuArgs();
            danmu.Danmaku = danmaku;

            //弹幕
            //"{\"info\":[[0,1,25,16777215,1450888087,\"1450888075\",0,\"082b9a9b\",0],\"O_O\",[245166,\"往事如风\",0,\"0\"],[],[20,5685],[]],\"cmd\":\"DANMU_MSG\",\"roomid\":42728}"
            
            //礼物
            //"{\"cmd\":\"SEND_GIFT\",\"data\":{\"giftName\":\"辣条\",\"num\":1,\"uname\":\"往事如风\",\"rcost\":48706,\"uid\":245166,\"top_list\":[],\"timestamp\":1450888185,\"giftId\":1,\"giftType\":0,\"action\":\"喂食\",\"super\":0,\"price\":100,\"rnd\":\"1450888075\",\"newMedal\":0,\"medal\":-1},\"roomid\":42728}"
            var danmaArg = new BroadcastEventArgs(danmu);
            Obj.BroadCastingDanmakuArgsInfo(danmaArg);
        }
    }
}
