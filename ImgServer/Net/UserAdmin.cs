using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wind.Comm;
using Wind.Comm.Expo4;

namespace ImgServer.Net
{
    public  delegate void ProcessBroadcastMessageHandler(Message msg);

    interface IInfomationControl
    {
    }

    class UserAdmin
    {
        private IAppServer _server;
        private IInfomationControl _UIControl;
        private string _userID;

        public UserAdmin( )
        {
        }

        public void bind( IInfomationControl  uiControl, IAppServer server)
        {
            _UIControl = uiControl;
            _server = server;
        }

        public void loginToServer(string userID)
        {
        }

        public void logoutFromServer()
        {
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

    }
}
