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

        public UserPolicyData(MessageV2ReaderHelper reader)
        {            
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

    public interface IInfomationControl
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
        void ChangeServerIP();
        /// <summary>
        /// 异步更新主界面上的用户报价策略控件
        /// </summary>
        /// <param name="policyData"></param>
        void AsyncUpdatePolicy(string adminUserID, ref UserPolicyData policyData);
    }


    public class UserAdmin
    {
        private IAppServer _server;
        private IInfomationControl _UIControl;
        private string _userID;
        private bool _logined = false;

        private Thread _PolicyBroadcastThread = null;
        private Thread _userActiveCheckThread = null;

        public const uint   LOGMSGID = 4001;
        public const uint   CHATMSGID = 4002;
        public const uint   CHANGESERVERMSGID = 4003;
        public const uint   CHANGEPOLICYMSGID = 4004;

        /// <summary>
        /// <UID,LastActiveTime>
        /// </summary>
        private Dictionary<string, DateTime> _onlineUserMap = null;
        private Object _lockObj = null;
        private AutoResetEvent _activeCheckWaitEvent = null;
        private AutoResetEvent _policySyncWaitEvent = null;


        public UserAdmin( )
        {
            _lockObj = new object();
            _onlineUserMap = new Dictionary<string, DateTime>();
            _policySyncWaitEvent = new AutoResetEvent(false);
            _activeCheckWaitEvent = new AutoResetEvent(false);
            _userActiveCheckThread = new Thread(ClearUnActiveUserInfo);
            _userActiveCheckThread.Start();
        }

        public void bind( IInfomationControl  uiControl, IAppServer server)
        {
            _UIControl = uiControl;
            _server = server;
        }

        public IAppServer Server
        {
            get
            {
                return _server;
            }
        }

        private void UpdateUserOnlineInfo(string uid)
        {
            lock (_lockObj)
            {
                _onlineUserMap[uid] = DateTime.Now;
            }
        }

        private void ClearUnActiveUserInfo()
        {
            List<string> unActivedUserList = new List<string>();
            while (true)
            {
                unActivedUserList.Clear();
                lock (_lockObj)
                {
                    foreach (KeyValuePair<string, DateTime> kvp in _onlineUserMap)
                    {
                        if ((DateTime.Now-kvp.Value).TotalSeconds > 5){
                            unActivedUserList.Add(kvp.Key);
                        }
                    }
                    foreach (string uid in unActivedUserList)
                    {
                        _onlineUserMap.Remove(uid);
                    }
                }
                foreach (string uid in unActivedUserList)
                {
                    UserPolicyData policyData = new UserPolicyData();
                    policyData.UserID = uid;
                    policyData.RemoveSelf = true;
                    _UIControl.SyncUpdatePolicyList(ref policyData);
                }
                if (_activeCheckWaitEvent.WaitOne(1000 * 2))
                {
                    //收到信号退出当前线程
                    return;
                }
            }
        }

        public void StopCheckThread()
        {
            if (_userActiveCheckThread != null)
            {
                _activeCheckWaitEvent.Set();
                _userActiveCheckThread.Join();
                _userActiveCheckThread = null;
            }
        }


        public bool loginToServer(string userID)
        {
            if (_logined) return true;
            if (_UIControl.SyncCheckUserExists(userID)) return false;
            if (_PolicyBroadcastThread != null) return false;
            

            _PolicyBroadcastThread = new Thread( (obj)=>{
                while (true)
                {
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
                    //Thread.Sleep(500);
                    if (_policySyncWaitEvent.WaitOne(1000))
                    {
                        //收到信号退出当前线程
                        return;
                    }
                }
                
            });
            _PolicyBroadcastThread.Start(userID);
            _userID = userID;
            _logined = true;

            return _logined;
        }

        public void logoutFromServer()
        {
            if (!_logined) return;
            if (_PolicyBroadcastThread != null)
            {
                _policySyncWaitEvent.Set();
                _PolicyBroadcastThread.Join();
                _PolicyBroadcastThread = null;
            }

            _logined = false;
        }

        public void NotifyChangeServer(string userID)
        {
            Message msg = new Message();
            msg.SetCommand(3100, CHANGESERVERMSGID);
            MessageV2BuilderHelper helper = new MessageV2BuilderHelper();
            helper.WriteString(userID);
            msg.setMsgBody(helper.GetMsgBody());
            _server.conBroadcast(msg, PacketHeader.ExpoDealType.InterGroup);
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
                        MessageV2ReaderHelper reader = new MessageV2ReaderHelper(msg);
                        UserPolicyData policyData = new UserPolicyData(reader);
                        if (!policyData.RemoveSelf)
                        {
                            UpdateUserOnlineInfo(policyData.UserID);
                        }
                        else
                        {
                            lock (_lockObj)
                            {
                                DateTime lastActiveTime;
                                if (_onlineUserMap.TryGetValue(policyData.UserID, out lastActiveTime))
                                {
                                    _onlineUserMap.Remove(policyData.UserID);
                                }
                            }
                        }
                        _UIControl.SyncUpdatePolicyList(ref policyData);
                        break;
                    }
                case CHANGESERVERMSGID:
                    {
                        MessageV2ReaderHelper reader = new MessageV2ReaderHelper(msg);
                        string userID = reader.ReadStr();
                        if (userID.Equals(_userID, StringComparison.OrdinalIgnoreCase))
                        {
                            _UIControl.ChangeServerIP();
                        }
                        break;
                    }
                case CHANGEPOLICYMSGID:
                    {
                        MessageV2ReaderHelper reader = new MessageV2ReaderHelper(msg);
                        string adminUserID = reader.ReadStr();
                        UserPolicyData policyData = new UserPolicyData(reader);
                        if (policyData.UserID.Equals(_userID, StringComparison.OrdinalIgnoreCase))
                        {
                            _UIControl.AsyncUpdatePolicy(adminUserID, ref policyData);
                        }
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
