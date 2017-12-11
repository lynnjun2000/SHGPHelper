using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wind.Comm;
using Wind.Comm.Expo4;
using System.Threading;

namespace ImgServer.Net
{
    public  delegate void ProcessBroadcastMessageHandler(Message msg);
    public  delegate void ProcessCondBroadcastMessageHandler(Message msg);
    

    public class UserPolicyData
    {
        public virtual String UserID { get; set; }
        /// <summary>
        /// 报价时间点，整数部分为分钟，小数部分为秒
        /// </summary>
        public virtual double OfferTime { get; set; }
        /// <summary>
        /// 加价，如800，900
        /// </summary>
        public virtual string PriceMarkup { get; set; }
        /// <summary>
        /// 提前400提交时间点，整数部分为秒，小数部分为毫秒
        /// </summary>
        public virtual double Submit400 { get; set; }
        /// <summary>
        /// 提前500提交时间点，整数部分为秒，小数部分为毫秒
        /// </summary>
        public virtual double Submit500 { get; set; }
        /// <summary>
        /// 最晚提交时间点，整数部分为秒，小数部分为毫秒
        /// </summary>
        public virtual double SubmitForce { get; set; }

        public virtual string ServerIP { get; set; }
        public virtual bool RemoveSelf { get; set; }

        public UserPolicyData()
        {
            RemoveSelf = false;
        }

        public UserPolicyData(Message msg)
        {
            MessageV2ReaderHelper reader = new MessageV2ReaderHelper(msg);
            UserID = reader.ReadStr();
            OfferTime = reader.ReadDouble();
            PriceMarkup = reader.ReadStr();
            Submit400 = reader.ReadDouble();
            Submit500 = reader.ReadDouble();
            SubmitForce = reader.ReadDouble();
            ServerIP = reader.ReadStr();
            RemoveSelf = reader.ReadBool();
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("UserID:").Append(UserID).Append("  报价时间点:").Append(OfferTime)
                .Append("   加价:").Append(PriceMarkup)
                .Append("   提前400出价时间:").Append(Submit400)
                .Append("   提前500出价时间:").Append(Submit500)
                .Append("   强制出价时间:").Append(SubmitForce)
                .Append("   服务器IP:").Append(ServerIP);
            return sb.ToString();
        }

    }

    public static class UserPolicyDataEx
    {
        public static void WriteUserPolicyData2Msg(this MessageV2BuilderHelper helper,UserPolicyData policyData)
        {
            helper.WriteString(policyData.UserID);
            helper.WriteDouble(policyData.OfferTime);
            helper.WriteString(policyData.PriceMarkup);
            helper.WriteDouble(policyData.Submit400);
            helper.WriteDouble(policyData.Submit500);
            helper.WriteDouble(policyData.SubmitForce);
            helper.WriteString(policyData.ServerIP);
            helper.WriteBool(policyData.RemoveSelf);
        }

    }
    public class UserState
    {
        public virtual string UserID { get; set; }
        public virtual bool Exists { get; set; }
    }

    interface IInfomationControl
    {
        /// <summary>
        /// 线程安全方式同步界面上的数据到UserPolicyData对象
        /// </summary>
        /// <param name="policyData"></param>
        void SyncGetSelfPolicyData(ref UserPolicyData policyData);
        
        /// <summary>
        /// 刷新主界面的各用户策略数据信息，如果不是删除的情况下，不存在则添加，否则则更新
        /// </summary>
        /// <param name="policyData"></param>
        /// <param name="isRemove"></param>
        void SyncUpdatePolicyList(ref UserPolicyData policyData);

        bool SyncCheckUserExists(string userID);
    }


    class UserAdmin
    {
        private IAppServer _server;
        private IInfomationControl _UIControl;
        private string _userID;
        private bool _logined = false;

        private Thread _PolicyBroadcastThread = null;

        public   const uint   LOGMSGID = 4001;
        private bool _policyThreadStop;


        public UserAdmin( )
        {
            _policyThreadStop = false;
        }

        public void bind( IInfomationControl  uiControl, IAppServer server)
        {
            _UIControl = uiControl;
            _server = server;
        }

        public bool loginToServer(string userID)
        {
            if (_logined) return true;
            if (_UIControl.SyncCheckUserExists(userID)) return false;
            if (_PolicyBroadcastThread != null) return false;
            
            _PolicyBroadcastThread = new Thread( (obj)=>{
                int loop = 0;
                while (true)
                {
                    if (_policyThreadStop) return;
                    if (loop++ % 2 != 0)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    UserPolicyData policyData = new UserPolicyData();
                    policyData.UserID = obj as string;
                    _UIControl.SyncGetSelfPolicyData(ref policyData);
                    _UIControl.SyncUpdatePolicyList(ref policyData);
                    //Console.WriteLine(policyData.ToString());
                    Message msg = new Message();
                    msg.SetCommand(3100, LOGMSGID);
                    MessageV2BuilderHelper helper = new MessageV2BuilderHelper();
                    helper.WriteUserPolicyData2Msg(policyData);
                    msg.setMsgBody(helper.GetMsgBody());
                    _server.conBroadcast(msg, PacketHeader.ExpoDealType.InterGroup);
                    Thread.Sleep(500);
                }
                
            });
            _PolicyBroadcastThread.Start(userID);

            _logined = true;

            return _logined;
        }

        public void logoutFromServer()
        {
            if (!_logined) return;
            _policyThreadStop = true;
            _PolicyBroadcastThread.Join();
            _PolicyBroadcastThread = null;
            _policyThreadStop = false;
            _logined = false;
        }

        /// <summary>
        /// 不同用户之间的聊天，在线状态，报价策略信息在本类中使用全网广播的方式通知其他用户
        /// 所以在本函数中处理这些消息，由于在不同的线程中，所有界面的刷新和信息提取都通过接口IInfomationControl进行操作
        /// IInfomationControl中使用SynchronizationContext这个对象进行安全操作UI
        /// 所有消息的操作都依赖于MessageV2ReaderHelper和MessageV2WriterHelper实现，无需define.xml文件的定义
        /// </summary>
        /// <param name="msg"></param>
        public void OnBroadcastMessage(Message msg)
        {
        }

        public void OnCondBroadcastMessage(Message msg)
        {
            switch (msg.Header.CommandValue)
            {
                case LOGMSGID:
                    {
                        UserPolicyData policyData = new UserPolicyData(msg);
                        _UIControl.SyncUpdatePolicyList(ref policyData);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

    }
}
