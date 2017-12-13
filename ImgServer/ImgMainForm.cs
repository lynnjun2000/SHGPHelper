using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ImgGPLib.Data;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Drawing.Imaging;
using ImgServer.Setting;
using System.Runtime.InteropServices;
using ImgGPLib.intf;
using ImgGPLib.proc;
using ImgServer.UU;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using Wind.Comm;
using Wind.Comm.Expo4;
using System.Diagnostics;
using Microsoft.Win32;
using TimeSync;
using ImgServer.Net;

namespace ImgServer
{

    public partial class ImgMainForm : Form, IInfomationControl
    {
        private class SyncStateData
        {
            public string NewPrice { get; set; }
            public bool isSelf { get; set; }
        }

        private string _stdFontLibFileName = "StdFontLib.dat";
        private StdFontLib _stdFontLib;
        private CodeFontLib _codeFontLib;

        private CoordSetting _coordSetting;
        private SysSetting _sysSetting;

        private Bitmap _priceBmp = null;
        private string _priceStr = "";
        private string _priceBmpMD5 = "";

        private Thread _priceCapThread;
        //private Thread _sysTimeUpdateThread;
        private bool _threadStop;

        private SynchronizationContext _SyncContext = null;

        private int _screenWidth;
        private int _screenHeight;

        private IntfCodeHelper _BmpHelperIntf = new GPCodeHelper();
        private CustomFontPosition _curCustomFont = null;

        public UUCodeClient _uuClient = null;
        private HotKeys _hotkey = new HotKeys();
        private log4net.ILog _log = log4net.LogManager.GetLogger("MainForm");

        private int _MyPlanBidPrice = 0;

        private int _fontMatchPrecision = 95;
        private int _bmpGray2SingleValue = 110;

        private SimplePassiveAppServer _server = null;
        private Point _webControlPoint;

        private Thread _serverIPRefreshThread = null;

        private UserAdmin _teamAdmin = null;
        private bool _disableUIFunc = false;

        FormPolicyEdit editForm = null;

        #region 键盘和鼠标操作函数，从系统库中导入

        private readonly int MOUSEEVENTF_LEFTDOWN = 0x0002;//模拟鼠标移动
        private readonly int MOUSEEVENTF_MOVE = 0x0001;//模拟鼠标左键按下
        private readonly int MOUSEEVENTF_LEFTUP = 0x0004;//模拟鼠标左键抬起
        private readonly int MOUSEEVENTF_ABSOLUTE = 0x8000;//鼠标绝对位置
        private readonly int MOUSEEVENTF_RIGHTDOWN = 0x0008; //模拟鼠标右键按下 
        private readonly int MOUSEEVENTF_RIGHTUP = 0x0010; //模拟鼠标右键抬起 
        private readonly int MOUSEEVENTF_MIDDLEDOWN = 0x0020; //模拟鼠标中键按下 
        private readonly int MOUSEEVENTF_MIDDLEUP = 0x0040;// 模拟鼠标中键抬起

        private readonly byte VK_HOME = 36;
        private readonly byte VK_DELETE = 46;

        private readonly int KEYEVENTF_KEYUP = 2;

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, IntPtr dwExtraInfo);

        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, //虚拟键值
            byte bScan,// 一般为0
            int dwFlags, //这里是整数类型 0 为按下，2为释放
            int dwExtraInfo //这里是整数类型 一般情况下设成为 0
        );

        [DllImport("user32.dll", EntryPoint = "MapVirtualKey")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll", EntryPoint = "VkKeyScan")]
        public static extern short VkKeyScan(char ch);

        #endregion

        public ImgMainForm()
        {
            InitializeComponent();
            regIEVer(true);
            regIEVer(false);

            tabPage2.Parent = null;

            _screenWidth = Screen.PrimaryScreen.Bounds.Width;
            _screenHeight = Screen.PrimaryScreen.Bounds.Height;

            _SyncContext = SynchronizationContext.Current;
            _coordSetting = CoordSetting.Load(OnCoordSettingChangedEvent);
            IniSysSetting();

            LoadStdFontLib();
            LoadCodeFontLib();

            _threadStop = false;
            //创建获取价格的线程并启动
            _priceCapThread = new Thread(CapPriceThreadProc);
            _priceCapThread.Start(this);
            //创建获取当前进程连接8300端口的TCP连接的服务器信息，计划每秒获取一次

            _serverIPRefreshThread = new Thread(GetServerIP);
            _serverIPRefreshThread.Start(1000);  //参数为1000毫秒

            this.dtAutoBid.Value = MyDateTime.GetDefaultBidTime();
            this.dtAutoCommit1.Value = MyDateTime.GetDefaultCommitTime1();
            this.dtAutoCommit400.Value = MyDateTime.GetDefaultCommitTime400();
            this.dtAutoCommit500.Value = MyDateTime.GetDefaultCommitTime500();
            this.dtAutoCommit2.Value = MyDateTime.GetDefaultCommitTime2();
            timer1.Enabled = true;
            setPriceMatchParam();

            //Console.WriteLine("本地侦听端口："+nodes.Default.LocalPort);
            //Console.WriteLine("相邻节点信息："+nodes.Default.OtherNode);
            _teamAdmin = new UserAdmin();
            startExpoServer();
            _teamAdmin.bind(this, _server);
            editForm = new FormPolicyEdit();
            editForm.TermAdmin = _teamAdmin;
            
        }

        public void SetDebugInfo(string info)
        {


        }

        private void startExpoServer()
        {
            _server = new SimplePassiveAppServer();
            _server.LogHandler += new OnLogInfo(OnLogInfo);
            OriMsgProcessor msgProcessor = new OriMsgProcessor(_server,this);
            msgProcessor.BroadcastMessageHandler += new ProcessBroadcastMessageHandler(_teamAdmin.OnBroadcastMessage);
            msgProcessor.CondBroadcastMessageHandler += new ProcessCondBroadcastMessageHandler(_teamAdmin.OnCondBroadcastMessage);
            msgProcessor.LogHandler += new OnLogInfo(OnLogInfo);
            //_server.BusMessageProcessHandler += new OnProcessBusMessage(msgProcessor.OnProcessBusMessage);
            _server.BusMessageProcessHandler += new OnProcessBusMessage(msgProcessor.OnProcessBusMessage);
            _server.Start();
            OnLogInfo(null, "在总线上注册的应用ID：" + _server.AppClass + "\n");
            OnLogInfo(null, "AppAddress:" + _server.AppAddress);
        }

        private class OriMsgProcessor
        {
            private IPassiveAppServer _intf;
            private ImgMainForm _mainForm;
            public OnLogInfo LogHandler { get; set; }
            public virtual ProcessBroadcastMessageHandler BroadcastMessageHandler { get; set; }
            public virtual ProcessCondBroadcastMessageHandler CondBroadcastMessageHandler { get; set; }
            public OriMsgProcessor(IPassiveAppServer intf, ImgMainForm mainForm)
            {
                this._intf = intf;
                this._mainForm = mainForm;
                BroadcastMessageHandler = null;
            }

            //Message msg, out bool continueProcess
            public void OnProcessBusMessage(Wind.Comm.Expo4.Message msg, out bool continueProcess)
            {
                //this.LogHandler(null, "msg src addr:" + msg.Header.SourceAddr);
                #region 处理通用错误应答
                if (msg.isErrMsg())
                {
                    string errStr;
                    uint errCode = msg.GetErrInfo(out errStr);
                    Console.WriteLine(String.Format("common response,{0},{1}", errCode, errStr));
                    continueProcess = false;
                    return;
                }
                #endregion

                #region 屏蔽所有全网广播,全网广播委托给UserAdmin进行处理，主要处理聊天和各用户的策略信息
                if (msg.Header.DeliverType == PacketHeader.ExpoDeliverType.Broadcast)
                {
                    if (BroadcastMessageHandler != null)
                    {
                        BroadcastMessageHandler(msg);
                    }
                    continueProcess = false;
                    return;
                }
                #endregion

                #region  处理条件广播
                if (msg.Header.DeliverType == PacketHeader.ExpoDeliverType.CondBroadcast)
                {
                    //Console.WriteLine("rev cond msg");
                    if (msg.Header.CommandValue == 1000)
                    {
                        Wind.Comm.Expo4.MessageV2ReaderHelper readHelper = new MessageV2ReaderHelper(msg);
                        string revPrice = readHelper.ReadStr();
                        if (revPrice.Trim().Length > 0)
                        {
                            Console.WriteLine("Rev Sync price:" + revPrice);
                            _mainForm._SyncContext.Post(_mainForm.SetPriceTextSafePost, new SyncStateData() { NewPrice = revPrice, isSelf = false });
                            //以下的判断机制在SetPriceTextSafePost这个函数中实现
                            /*
                            try
                            {
                                int price = Convert.ToInt32(revPrice);
                                string formPriceStr = _mainForm._priceStr;
                                if (formPriceStr.Trim().Length == 0)
                                {
                                    //当前窗体上的价格为空白，使用收到的同步价格更新当前窗体的价格
                                    _mainForm._SyncContext.Post(_mainForm.SetPriceTextSafePost, revPrice);
                                }
                                else
                                {
                                    int formPrice = Convert.ToInt32(formPriceStr);
                                    if (price > formPrice)
                                    {
                                        //当前收到的同步价格大于窗体上的价格，使用同步价格更新窗体上的价格
                                        _mainForm._SyncContext.Post(_mainForm.SetPriceTextSafePost, revPrice);
                                    }
                                }
                                 *
                            }
                            catch (Exception ee)
                            {
                            }
                             */
                        }
                        //Console.WriteLine(revPrice);
                    }
                    else
                    {
                        if (CondBroadcastMessageHandler != null)
                        {
                            CondBroadcastMessageHandler(msg);
                            continueProcess = false;
                            return;
                        }
                    }
                    
                }
                #endregion

                continueProcess = false;
            }
        }

        private void syncCurPrice(string curPriceStr)
        {
            if (_server != null)
            {
                Wind.Comm.Expo4.Message priceMsg = new Wind.Comm.Expo4.Message();
                priceMsg.SetCommand((ushort)3100,1000);
                Wind.Comm.Expo4.MessageV2BuilderHelper writeHelper = new MessageV2BuilderHelper();
                writeHelper.WriteString(curPriceStr.Trim());
                priceMsg.setMsgBody(writeHelper.GetMsgBody());
                _server.conBroadcast(priceMsg, PacketHeader.ExpoDealType.InterGroup);
            }
        }

        private void OnLogInfo(object sender, string message)
        {
            Console.WriteLine(message);
        }

        private void setPriceMatchParam()
        {
            _fontMatchPrecision = Convert.ToInt16(tbFontPrecision.Text.Trim());
            _bmpGray2SingleValue = Convert.ToInt16(tbGrayValue.Text.Trim());
        }

        //private void UpdateSysTimeStrThreadProc(object stateObj)
        //{
        //    ImgMainForm mainForm = (ImgMainForm)stateObj;
        //    while (!mainForm._threadStop)
        //    {
        //        Thread.Sleep(100);
        //    }
        //}


        private Bitmap PriceBmp
        {
            get
            {
                return _priceBmp;
            }
            set
            {
                _priceBmp = value;
                if (!this.ckNoMD5.Checked)
                {
                    _priceBmpMD5 = MD5Calc(_priceBmp);
                }
            }
        }


        private void IniSysSetting()
        {
            _sysSetting = SysSetting.Load();
            this.tbSpanSelect.Text = _sysSetting.SelectSpan.ToString();
            this.tbSpanKeydown.Text = _sysSetting.KeyDownSpan.ToString();
            this.tbSpanMouseClick.Text = _sysSetting.ClickSpan.ToString();

            _coordSetting.sysSetting = _sysSetting;
        }


        private void btStdFont_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StdFont stdFont = new StdFont('3');
            stdFont.BuildData(new Font("宋体", 9));
        }

        private void LoadStdFontLib()
        {
            AbstrctFontLib obj = AbstrctFontLib.LoadFromFile(_stdFontLibFileName);
            if (obj != null)
            {
                _stdFontLib = obj as StdFontLib;
            }
            else
            {
                _stdFontLib = new StdFontLib();
            }
            cbStdFontName.Items.Clear();
            HashSet<Font> fontList = _stdFontLib.GetFontList();
            foreach (Font font in fontList)
            {
                cbStdFontName.Items.Add(font);
            }
            //_stdFontLib.PrintFontInfo();
        }

        private void LoadCodeFontLib()
        {
            _codeFontLib = CodeFontLib.LoadFromFile();
            if (_codeFontLib == null)
            {
                _codeFontLib = new CodeFontLib();
            }
            cbCodeFontName.Items.Clear();
            HashSet<string> fontList = _codeFontLib.GetFontList();
            foreach (string font in fontList)
            {
                cbCodeFontName.Items.Add(font);
            }


        }

        private void cbStdFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            //object obj = cbStdFontName.SelectedItem;
            //Font font = obj as Font;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (fontDlg.ShowDialog() == DialogResult.OK)
            {
                _stdFontLib.AppendFontToLib(tbBenchMark.Text.Trim(), fontDlg.Font);
                _stdFontLib.SaveToFile(_stdFontLibFileName);

                cbStdFontName.Items.Clear();
                HashSet<Font> fontList = _stdFontLib.GetFontList();
                foreach (Font font in fontList)
                {
                    cbStdFontName.Items.Add(font);
                }

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            object obj = cbStdFontName.SelectedItem;
            Font font = obj as Font;
            if (font == null)
            {
                MessageBox.Show("必须选择一个字体进行删除");
            }
            else
            {
                _stdFontLib.RemoveFontType(font);
                _stdFontLib.SaveToFile(_stdFontLibFileName);
                cbStdFontName.Items.Remove(font);
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            object obj = cbStdFontName.SelectedItem;
            Font font = obj as Font;
            if (font == null)
            {
                MessageBox.Show("必须选择一个字体");
            }
            else
            {

                Graphics g = this.CreateGraphics();
                Size sif = TextRenderer.MeasureText(g, tbBenchMark.Text.Trim(), font, new Size(0, 0), TextFormatFlags.NoPadding);

                Bitmap bmp = new Bitmap(sif.Width, sif.Height);
                g = Graphics.FromImage(bmp);
                TextRenderer.DrawText(g, tbBenchMark.Text.Trim(), font, new Point() { X = 0, Y = 0 }, Color.Black, TextFormatFlags.NoPadding);
                this.pboxStd.Image = bmp;
                pboxScreen.Left = pboxStd.Left;
                pboxScreen.Top = pboxStd.Top + pboxStd.Height;

                pboxScreen.Image = (Bitmap)(PriceBmp.Clone());
                lbScreenBmpInfo.Text = "识别图片信息  " + pboxScreen.Image.Width + " x " + pboxScreen.Image.Height;
            }

        }


        private void btCoordSetting_click(object sender, EventArgs e)
        {
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            //var screenPoint = PointToScreen(webBrowser1.Location);
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));

            WinScreen myScreen = new WinScreen(bmp);
            myScreen.ShowIcon = false;
            if (myScreen.ShowDialog() == DialogResult.OK)
            {                
                //StringBuilder sb = new StringBuilder();
                //sb.Append(myScreen.SelectRect.Left - screenPoint.X).Append(",").Append(myScreen.SelectRect.Top - screenPoint.Y)
                //    .Append(",")
                //    .Append(myScreen.SelectRect.Left + myScreen.SelectRect.Width - screenPoint.X).Append(",")
                //    .Append(myScreen.SelectRect.Top + myScreen.SelectRect.Height - screenPoint.Y);
                if (sender is Button)
                {
                    Button bt = sender as Button;
                    switch (Convert.ToInt32(bt.Tag))
                    {
                        case 1:
                            _coordSetting.SetCoordValue(CoordType.CTPriceTextbox,
                                myScreen.SelectRect.Left,
                                myScreen.SelectRect.Top,
                                myScreen.SelectRect.Left + myScreen.SelectRect.Width,
                                myScreen.SelectRect.Top + myScreen.SelectRect.Height,
                                _webControlPoint);
                            //textBox = this.tbPriceInputPos;
                            break;
                        case 2:
                            _coordSetting.SetCoordValue(CoordType.CTBidButton,
                                myScreen.SelectRect.Left,
                                myScreen.SelectRect.Top,
                                myScreen.SelectRect.Left + myScreen.SelectRect.Width,
                                myScreen.SelectRect.Top + myScreen.SelectRect.Height,
                                _webControlPoint);

                            break;
                        case 3:
                            _coordSetting.SetCoordValue(CoordType.CTCodeTextbox,
                                myScreen.SelectRect.Left,
                                myScreen.SelectRect.Top,
                                myScreen.SelectRect.Left + myScreen.SelectRect.Width,
                                myScreen.SelectRect.Top + myScreen.SelectRect.Height,
                                _webControlPoint);
                            break;
                        case 4:
                            _coordSetting.SetCoordValue(CoordType.CTCodeShow,
                                myScreen.SelectRect.Left,
                                myScreen.SelectRect.Top,
                                myScreen.SelectRect.Left + myScreen.SelectRect.Width,
                                myScreen.SelectRect.Top + myScreen.SelectRect.Height,
                                _webControlPoint);
                            break;
                        case 5:
                            _coordSetting.SetCoordValue(CoordType.CTCommitButton,
                                myScreen.SelectRect.Left,
                                myScreen.SelectRect.Top,
                                myScreen.SelectRect.Left + myScreen.SelectRect.Width,
                                myScreen.SelectRect.Top + myScreen.SelectRect.Height,
                                _webControlPoint);

                            break;
                        case 6:
                            _coordSetting.SetCoordValue(CoordType.CTAddPriceButton,
                                myScreen.SelectRect.Left,
                                myScreen.SelectRect.Top,
                                myScreen.SelectRect.Left + myScreen.SelectRect.Width,
                                myScreen.SelectRect.Top + myScreen.SelectRect.Height,
                                _webControlPoint);

                            break;
                    }
                }
            }
        }

        private void OnCoordSettingChangedEvent(CoordType ctype, RectDefine define)
        {
            //var screenPoint = PointToScreen(webBrowser1.Location);
            StringBuilder sb = new StringBuilder();
            sb.Append(define.LeftTop.X ).Append(",").Append(define.LeftTop.Y )
                .Append(",")
                .Append(define.RightBottom.X ).Append(",")
                .Append(define.RightBottom.Y );

            switch (ctype)
            {
                case CoordType.CTAddPriceButton:
                    tbAddPriceButtonPos.Text = sb.ToString();
                    break;
                case CoordType.CTPriceTextbox:
                    tbPriceInputPos.Text = sb.ToString();
                    break;
                case CoordType.CTBidButton:
                    tbPriceButtonPos.Text = sb.ToString();
                    break;
                case CoordType.CTCodeTextbox:
                    tbCodeInputPos.Text = sb.ToString();
                    break;
                case CoordType.CTCodeShow:
                    tbCodeShowPos.Text = sb.ToString();
                    break;
                case CoordType.CTCommitButton:
                    tbCommitPos.Text = sb.ToString();
                    break;
            }
        }

        private void btPriceSetCoord_Click(object sender, EventArgs e)
        {
            //var screenPoint = PointToScreen(webBrowser1.Location);
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));

            WinScreen myScreen = new WinScreen(bmp);
            myScreen.ShowIcon = false;
            //myScreen.Show();
            //myScreen.CapFullScreen();
            if (myScreen.ShowDialog() == DialogResult.OK)
            {
                //pboxPrice.Image = myScreen.ResultBitmap;
                //Bitmap bmp1 = MakeBMPGray(myScreen.ResultBitmap);
                //bmp1 = MakeBMPSingle(bmp1);

                //PriceBmp = bmp1;
                //string priceStr = _stdFontLib.IdentifyPrice(bmp);
                //tbPrice.Text = priceStr;
                
                StringBuilder sb = new StringBuilder();
                sb.Append(myScreen.SelectRect.Left - _webControlPoint.X).Append(",").Append(myScreen.SelectRect.Top - _webControlPoint.Y);
                tbPriceCoord1.Text = sb.ToString();
                sb.Remove(0, sb.Length);



                sb.Append(myScreen.SelectRect.Left + myScreen.SelectRect.Width - _webControlPoint.X).Append(",")
                    .Append(myScreen.SelectRect.Top + myScreen.SelectRect.Height - _webControlPoint.Y);
                tbPriceCoord2.Text = sb.ToString();
            }
        }

        private string MD5Calc(Bitmap sourceBmp)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    sourceBmp.Save(ms, ImageFormat.Bmp);
                    ms.Position = 0;
                    byte[] data = md5Hash.ComputeHash(ms);
                    StringBuilder sBuilder = new StringBuilder();
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }
                    return sBuilder.ToString();
                }
            }
        }

        private void onPriceChanged(string newPrice,bool isMyCaped)
        {
            if (isMyCaped)
            {
                syncCurPrice(newPrice);
            }
            _log.Info("价格发生变化" + newPrice);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(String.Format("{0}:当前价格：{1}", DateTime.Now.ToString("HH:mm:ss.fff"), newPrice));
            Console.ForegroundColor = ConsoleColor.White;

        }


        private void SetPriceTextSafePost(object priceInfoObj)
        {
            SyncStateData priceInfo = (SyncStateData)priceInfoObj;
            string needUpdatePrice = priceInfo.NewPrice .Trim();
            if (needUpdatePrice.Length==0) return;
            try{
                int newPrice = Convert.ToInt32(needUpdatePrice);
                if (_priceStr.Trim().Length==0){
                    //当前屏幕上的价格为空白，必须使用新价格进行更新
                    this.tbPrice.Text = needUpdatePrice;
                    this._priceStr = needUpdatePrice;
                    onPriceChanged(needUpdatePrice,priceInfo.isSelf);
                }else{
                    //当前屏幕上的价格不为空白，只有新价格高于屏幕上的价格才进行更新
                    int formPrice = Convert.ToInt32(_priceStr.Trim());
                    if (newPrice >formPrice){
                        this.tbPrice.Text = needUpdatePrice;
                        this._priceStr = needUpdatePrice;
                        onPriceChanged(needUpdatePrice, priceInfo.isSelf);
                    }
                }
            }catch(Exception e){
            }
        }

        private void SetPriceBmpSafePost(object nullObj)
        {
            pboxPrice.Image = this.PriceBmp;
        }

        private UInt32 _capedTimes = 0;

        public void CapPriceAndAnalize()
        {
            if (_threadStop) return;
            if (tbPriceCoord1.Text.Trim().Length == 0 || tbPriceCoord1.Text.Trim().Length == 0) return;
            if (tbPriceCoord1.Text.Trim().Equals("0,0,0,0") || tbPriceCoord1.Text.Trim().Equals("0,0,0,0")) return;

            string[] tmp1 = tbPriceCoord1.Text.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (tmp1.Length != 2) return;
            string[] tmp2 = tbPriceCoord2.Text.Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (tmp2.Length != 2) return;
            try
            {

                int left = Convert.ToInt32(tmp1[0]);
                int top = Convert.ToInt32(tmp1[1]);
                int right = Convert.ToInt32(tmp2[0]);
                int bottom = Convert.ToInt32(tmp2[1]);
                int width = right - left;
                int height = bottom - top;
                Bitmap bmp = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bmp);
                g.CopyFromScreen(left + _webControlPoint.X, top + _webControlPoint.Y, 0, 0, new Size(width, height));
                //先转换为灰度图，在转换为单色图
                bmp = _BmpHelperIntf.MakeBMPGray(bmp);
                bmp = _BmpHelperIntf.MakeBMPSingle(bmp, _bmpGray2SingleValue);
                _capedTimes++;
                if (ckNoMD5.Checked)
                {
                    this.PriceBmp = (Bitmap)(bmp.Clone());
                    _SyncContext.Post(SetPriceBmpSafePost, null);
                    using (Bitmap bmp1 = (Bitmap)(bmp.Clone()))
                    {
                        string priceStr = _stdFontLib.IdentifyPrice(bmp1, _fontMatchPrecision);
                        if (priceStr.Trim().Length == 0)
                        {
                            if (_capedTimes % 5 == 0)
                            {
                                Console.WriteLine("当前识别的的价格为空白");
                            }
                            return;
                        }
                        if (!priceStr.Equals(_priceStr))
                        {
                            //只判断了捕获到的价格和当前屏幕的价格不一致，但是考虑存在价格从其他程序上同步的情况，有可能其他窗体收到的价格延时比较低
                            //可能同步的价格更加实时，要考虑忽略捕获到的价格，但是在测试过程中，捕获的价格可能被复位，即要捕获的价格可能被调低的情况
                            //设置情况下需要通过某个机制复位内存的数据
                            //_priceStr = priceStr;
                            //syncCurPrice(priceStr);
                            //只刷新价格框的内容
                            _SyncContext.Post(SetPriceTextSafePost, new SyncStateData() { NewPrice = priceStr, isSelf = true });
                            //Console.WriteLine("价格发生变化" + _priceStr);
                            /*
                            _log.Info("价格发生变化" + _priceStr);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(String.Format("{0}:当前价格：{1}", DateTime.Now.ToString("HH:mm:ss.fff"), _priceStr));
                            Console.ForegroundColor = ConsoleColor.White;
                             */
                        }
                    }

                }
                else
                {
                    string md5str = MD5Calc(bmp);
                    //Console.WriteLine(md5str);
                    if (!_priceBmpMD5.Equals(md5str))
                    {
                        this.PriceBmp = (Bitmap)(bmp.Clone());
                        _SyncContext.Post(SetPriceBmpSafePost, null);
                        using (Bitmap bmp1 = (Bitmap)(bmp.Clone()))
                        {
                            string priceStr = _stdFontLib.IdentifyPrice(bmp1, _fontMatchPrecision);
                            if (priceStr.Trim().Length == 0)
                            {
                                Console.WriteLine("当前识别的的价格为空白");
                                return;
                            }
                            //_priceStr = priceStr;
                            syncCurPrice(priceStr);
                            //_SyncContext.Post(SetPriceTextSafePost, priceStr);
                            _SyncContext.Post(SetPriceTextSafePost, new SyncStateData() { NewPrice = priceStr, isSelf = true });
                            //Console.WriteLine("价格发生变化" + _priceStr);
                            _log.Info("价格发生变化" + _priceStr);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(String.Format("{0}:当前价格：{1}", DateTime.Now.ToString("HH:mm:ss"), _priceStr));
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                }

            }
            catch(Exception e)
            {
                //StringBuilder sb = new StringBuilder();
                //sb.Append(e.Message);
                //if (e.InnerException != null) sb.Append(e.InnerException.Message);
                //_log.Error(sb.ToString());
                //Console.WriteLine(sb.ToString());
            }
        }

        private void UpdateSysTimeStr(object timeStr)
        {
            this.lbNowTime.Text = (string)timeStr;

        }

        private void UpdateTCPServerInfo(object tcpInfo)
        {
            tbServerIPInfo.Text = (string)tcpInfo;
        }

        private void GetServerIP(Object stateObj)
        {
            string ipInfo = ""; 
            while (true)
            {
                if (_threadStop) return;
                string serverInfo = TCPConnHelper.getTCPServerInfo(8300);
                if (!serverInfo.Equals(ipInfo, StringComparison.OrdinalIgnoreCase))
                {
                    ipInfo = serverInfo;
                    if (serverInfo.Length == 0)
                    {
                        this._SyncContext.Post(UpdateTCPServerInfo, "无");
                    }
                    else
                    {
                        this._SyncContext.Post(UpdateTCPServerInfo, ipInfo);
                    }
                }                
                Thread.Sleep(1000);
            }
        }

        public static void CapPriceThreadProc(Object stateObj)
        {
            ImgMainForm mainForm = (ImgMainForm)stateObj;
            int loopTime = 0;
            while (true)
            {
                if (mainForm._threadStop) return;
                loopTime += 10;
                if (loopTime % 20 == 0)
                {
                    string timeStr = DateTime.Now.ToString("HH:mm:ss");
                    mainForm._SyncContext.Post(mainForm.UpdateSysTimeStr, timeStr);
                    mainForm.CapPriceAndAnalize();
                }
                
                Thread.Sleep(10);
                if (mainForm._threadStop) return;
            }
        }

        private void ImgMainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this._threadStop = true;
            _priceCapThread.Join();
            if (this._server != null)
            {
                this._server.Stop();
            }
            _serverIPRefreshThread.Join();
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (pboxScreen.Image == null)
            {
                MessageBox.Show("请先显示图片");
            }
            else
            {
                string v = _stdFontLib.IdentifyPrice((Bitmap)(pboxScreen.Image),this._fontMatchPrecision);
                lbIdentifyTxt.Text = "识别内容：" + v;
                Console.WriteLine(v);
            }
        }

        private void pboxScreen_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox imgBox = sender as PictureBox;
            if (imgBox != null && imgBox.Image != null)
            {
                Bitmap Sourcebm = (Bitmap)imgBox.Image;
                Color c = Sourcebm.GetPixel(e.X, e.Y);
                StringBuilder sb = new StringBuilder();
                sb.Append("X:").Append(e.X).Append(" Y:").Append(e.Y).Append(" ARGB:");
                sb.Append(c.A).Append(",").Append(c.R).Append(",").Append(c.G).Append(",").Append(c.B);
                lbScreenBmpPos.Text = sb.ToString();
            }

        }


        private void btSaveCoord_Click(object sender, EventArgs e)
        {
            _coordSetting.Save();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                //_sysSetting.SelectSpan = Convert.ToInt32(this.tbSpanSelect.Text.Trim());
                //_sysSetting.KeyDownSpan = Convert.ToInt32(this.tbSpanKeydown.Text.Trim());
                //_sysSetting.ClickSpan = Convert.ToInt32(this.tbSpanMouseClick.Text.Trim());
                _sysSetting.Save();
            }
            catch (Exception e1)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(e1.Message);
                if (e1.InnerException != null) sb.Append("\n").Append(e1.InnerException.Message);
                MessageBox.Show(sb.ToString());
            }
        }

        private void tbSpanSelect_TextChanged(object sender, EventArgs e)
        {
            _sysSetting.SelectSpan = Convert.ToInt32(this.tbSpanSelect.Text.Trim());
        }

        private void tbSpanKeydown_TextChanged(object sender, EventArgs e)
        {
            _sysSetting.KeyDownSpan = Convert.ToInt32(this.tbSpanKeydown.Text.Trim());
        }

        private void tbSpanMouseClick_TextChanged(object sender, EventArgs e)
        {
            _sysSetting.ClickSpan = Convert.ToInt32(this.tbSpanMouseClick.Text.Trim());
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Point location = this._coordSetting.PriceTextBox.GetMidLocation();
            MyMouseClick(location);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Point location = this._coordSetting.BidButton.GetMidLocation();
            MyMouseClick(location);
        }

        #region  鼠标键盘操作函数

        private void fixWithBrowser(ref Point location)
        {
            //var screenPoint = PointToScreen(webBrowser1.Location);
            location.X = location.X + _webControlPoint.X;
            location.Y = location.Y + _webControlPoint.Y;            
        }
        /// <summary>
        /// 用户鼠标点击，首先鼠标移到指定坐标位置，在点击鼠标左键后再放开
        /// </summary>
        /// <param name="location">屏幕相对坐标</param>
        public void MyMouseClick(Point location)
        {
            fixWithBrowser(ref location);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, location.X * 65535 / _screenWidth, location.Y * 65535 / _screenHeight, 0, IntPtr.Zero);
            Thread.Sleep(_sysSetting.SelectSpan);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTDOWN, 0, 0, 0, IntPtr.Zero);
            Thread.Sleep(2);
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_LEFTUP, 0, 0, 0, IntPtr.Zero);
        }

        /// <summary>
        /// 鼠标选择某个编辑框，在清除改编辑框中的内容
        /// </summary>
        /// <param name="location">屏幕绝对坐标</param>
        public void MyMouseClearEditBox(Point location)
        {
            //fixWithBrowser(ref location);
            MyMouseClick(location);
            Thread.Sleep(_sysSetting.SelectSpan);
            keybd_event(VK_HOME, 0, 0, 0);
            keybd_event(VK_HOME, 0, KEYEVENTF_KEYUP, 0);
            byte vk_del = (byte)MapVirtualKey(VK_DELETE, 0);
            for (int i = 0; i < 30; i++)
            {
                keybd_event(VK_DELETE, vk_del, 0, 0);
                keybd_event(VK_DELETE, vk_del, KEYEVENTF_KEYUP, 0);
                Thread.Sleep(_sysSetting.KeyDownSpan);
            }
        }

        /// <summary>
        /// 选择某个编辑框，清除改编辑框中内容，并使用新内容填充
        /// </summary>
        /// <param name="location">屏幕绝对坐标位置</param>
        /// <param name="value">要填充的内容</param>
        public void MyMouseResetEditBoxValue(Point location, string value)
        {
            MyMouseClearEditBox(location);
            foreach (char v in value)
            {
                byte vk_code = (byte)VkKeyScan(v);
                keybd_event(vk_code, 0, 0, 0);
                keybd_event(vk_code, 0, KEYEVENTF_KEYUP, 0);
                Thread.Sleep(_sysSetting.KeyDownSpan);
            }
        }

        #endregion

        private void button7_Click(object sender, EventArgs e)
        {
            Point location = this._coordSetting.CodeTextBox.GetMidLocation();
            MyMouseClick(location);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Point location = this._coordSetting.CodeShow.GetMidLocation();
            MyMouseClick(location);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            Point location = this._coordSetting.CommitButton.GetMidLocation();
            MyMouseClick(location);

        }

        private int AddPrice(int incPrice)
        {
            Point location = this._coordSetting.PriceTextBox.GetMidLocation();
            int curPrice = 0;
            if (_priceStr.Trim().Length != 0)
            {
                curPrice = Convert.ToInt32(_priceStr.Trim());
            }
            curPrice += incPrice;
            MyMouseResetEditBoxValue(location, curPrice.ToString());
            return curPrice;
        }

        private void btAdd_400_Click(object sender, EventArgs e)
        {
            AddPrice(400);
        }

        private void btAdd_500_Click(object sender, EventArgs e)
        {
            AddPrice(500);
        }

        private void btAdd_600_Click(object sender, EventArgs e)
        {
            AddPrice(600);
        }

        private void btAdd_700_Click(object sender, EventArgs e)
        {
            AddPrice(700);
        }

        private void btAdd_800_Click(object sender, EventArgs e)
        {
            AddPrice(800);
        }

        private void AutoBid()
        {
            try
            {
                if (ckUseSysAddPriceButton.Checked)
                {
                    Point SysAddPrice_location = this._coordSetting.AddPriceButton.GetMidLocation();
                    MyMouseClick(SysAddPrice_location);
                    Point Bid_location = this._coordSetting.BidButton.GetMidLocation();
                    MyMouseClick(Bid_location);
                    if (_priceStr.Trim().Length != 0)
                    {
                        _MyPlanBidPrice = Convert.ToInt32(_priceStr.Trim()) + Convert.ToInt32(tbIncPrice.Text.Trim());
                        string logInfo = "自动报价信息(使用系统加价按钮)：当前价格：" + _priceStr + ",计划报的价格：" + _MyPlanBidPrice.ToString();
                        Console.WriteLine(logInfo);
                        _log.Info(logInfo);
                    }
                }
                else
                {
                    int bidPrice = AddPrice(Convert.ToInt32(tbIncPrice.Text.Trim()));
                    string logInfo = "自动报价信息(使用助手加价)，当前价格：" + _priceStr + " 报价:" + bidPrice;
                    Console.WriteLine(logInfo);
                    _log.Info(logInfo);
                    Point Bid_location = this._coordSetting.BidButton.GetMidLocation();
                    MyMouseClick(Bid_location);
                    _MyPlanBidPrice = bidPrice;
                }
            }
            catch (Exception e)
            {
                StringBuilder errInfo = new StringBuilder();
                errInfo.Append(e.Message);
                if (e.InnerException != null) errInfo.Append("\n").Append(e.InnerException.Message);
                _log.Error(errInfo.ToString());
            }
        }

        private void btRun1_Click(object sender, EventArgs e)
        {
            AutoBid();
        }

        private DateTime _autoBidTime = DateTime.Now;
        private DateTime _autoCommitTime = DateTime.Now;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (cbAutoBid.Checked)
            {
                DateTime autoBidTime = dtAutoBid.Value;
                autoBidTime = autoBidTime.AddMilliseconds(300);

                if (DateTime.Now >= autoBidTime)
                {
                    cbAutoBid.Checked = false;
                    AutoBid();
                }
                else
                {
                    if (_autoBidTime.Second != DateTime.Now.Second)
                    {
                        String logInfo = String.Format("当前时间{0}，还未到自动出价时间：{1}", DateTime.Now.ToString("HH:mm:ss.fff"), dtAutoBid.Value.ToString("HH:mm:ss"));
                        Console.WriteLine(logInfo);
                        _log.Info(logInfo);
                        _autoBidTime = DateTime.Now;
                    }
                }
            }

            if (cbAutoCommit.Checked)
            {
                MyDateTime myTime1 = new MyDateTime(dtAutoCommit1.Value);
                myTime1.MSec = Convert.ToInt32(tbAutoCommitMS1.Text.Trim());
                MyDateTime myTime2 = new MyDateTime(dtAutoCommit2.Value);                
                myTime2.MSec = Convert.ToInt32(tbAutoCommitMS2.Text.Trim());

                MyDateTime myTime400 = new MyDateTime(dtAutoCommit400.Value);
                myTime400.MSec = Convert.ToInt32(tbAutoCommitMS400.Text.Trim());
                MyDateTime myTime500 = new MyDateTime(dtAutoCommit500.Value);
                myTime500.MSec = Convert.ToInt32(tbAutoCommitMS500.Text.Trim());

                //强制出价时间前1秒，提前400也可以出价了
                //MyDateTime myTime3 = new MyDateTime(myTime2.ToDateTime());
                //myTime3.Second = myTime3.Second - 1;
                //myTime3.MSec = 400;

                #region 判断当前时间是否大于等于强制提交时间，如果过了强制提交时间，必须强制提交
                if (DateTime.Now >= myTime2.ToDateTime())
                {
                    cbAutoCommit.Checked = false;
                    Point location = this._coordSetting.CommitButton.GetMidLocation();
                    MyMouseClick(location);
                    Console.ForegroundColor = ConsoleColor.Red;
                    string logInfo = String.Format("当前时间{0}达到强制提交时间，已经强制提交", DateTime.Now.ToString("HH:mm:ss.fff"));
                    Console.WriteLine(logInfo);
                    _log.Info(logInfo);
                    Console.ForegroundColor = ConsoleColor.White;
                    return;
                }
                #endregion

                #region 判断当前时间是否大于等于第二档时间，即在强制出价时间前1秒，价格差是否达到400，达到就出价，等待排队过程中，价格应该会跳上100
                if (DateTime.Now >= myTime400.ToDateTime())
                {
                    try
                    {
                        int curPrice = 0;  //当前界面上的最低可成交价格
                        if (_priceStr.Trim().Length != 0)
                        {
                            //说明从界面上成功捕获了最低可成交价格
                            curPrice = Convert.ToInt32(_priceStr.Trim());
                            //curPrice = curPrice + Convert.ToInt32(tbIncPrice.Text.Trim());

                            //计划报的价格不大于当前最低可成交价格，就可以提交报价了，_MyPlanBidPrice为计划报的价格,该价格由函数AutoBid确定
                            if (_MyPlanBidPrice - curPrice <= 400)
                            {
                                cbAutoCommit.Checked = false;
                                Point location = this._coordSetting.CommitButton.GetMidLocation();
                                MyMouseClick(location);
                                Console.ForegroundColor = ConsoleColor.Blue;
                                string logInfo = String.Format("{0},计划报价{1}已经在可提前出价范围{2}~{3},提前400，执行自动提交价格任务",
                                    DateTime.Now.ToString("HH:mm:ss.fff"), _MyPlanBidPrice, curPrice - 300, curPrice + 400);
                                Console.WriteLine(logInfo);
                                _log.Info(logInfo);
                                Console.ForegroundColor = ConsoleColor.White;
                                _autoCommitTime = DateTime.Now;
                                return;
                            }
                        }

                    }
                    catch (Exception err1)
                    {
                        StringBuilder errInfo = new StringBuilder();
                        errInfo.Append(err1.Message);
                        if (err1.InnerException != null) errInfo.Append("\n").Append(err1.InnerException.Message);
                        _log.Error(errInfo.ToString());
                        Console.WriteLine(err1.Message);
                    }
                }
                #endregion

                #region 判断当前时间是否大于等于第三档时间，价格差是否达到500，达到就出价，等待排队过程中，价格应该会跳上100，且获取的价格应该是早些时候
                if (DateTime.Now >= myTime500.ToDateTime())
                {
                    try
                    {
                        int curPrice = 0;  //当前界面上的最低可成交价格
                        if (_priceStr.Trim().Length != 0)
                        {
                            //说明从界面上成功捕获了最低可成交价格
                            curPrice = Convert.ToInt32(_priceStr.Trim());
                            //curPrice = curPrice + Convert.ToInt32(tbIncPrice.Text.Trim());

                            //计划报的价格不大于当前最低可成交价格，就可以提交报价了，_MyPlanBidPrice为计划报的价格,该价格由函数AutoBid确定
                            if (_MyPlanBidPrice - curPrice <= 500)
                            {
                                cbAutoCommit.Checked = false;
                                Point location = this._coordSetting.CommitButton.GetMidLocation();
                                MyMouseClick(location);
                                Console.ForegroundColor = ConsoleColor.Blue;
                                string logInfo = String.Format("{0},计划报价{1}已经在可提前出价范围{2}~{3},提前500，执行自动提交价格任务",
                                    DateTime.Now.ToString("HH:mm:ss.fff"), _MyPlanBidPrice, curPrice - 300, curPrice + 500);
                                Console.WriteLine(logInfo);
                                _log.Info(logInfo);
                                Console.ForegroundColor = ConsoleColor.White;
                                _autoCommitTime = DateTime.Now;
                                return;
                            }
                        }

                    }
                    catch (Exception err1)
                    {
                        StringBuilder errInfo = new StringBuilder();
                        errInfo.Append(err1.Message);
                        if (err1.InnerException != null) errInfo.Append("\n").Append(err1.InnerException.Message);
                        _log.Error(errInfo.ToString());
                        Console.WriteLine(err1.Message);
                    }
                }
                #endregion

                try
                {
                    //判断当前时间过了最早计划提交时间，也就是说，如果价格合适就可以提交报价
                    if (DateTime.Now >= myTime1.ToDateTime())
                    {
                        int curPrice = 0;  //当前界面上的最低可成交价格
                        if (_priceStr.Trim().Length != 0)
                        {
                            //说明从界面上成功捕获了最低可成交价格
                            curPrice = Convert.ToInt32(_priceStr.Trim());
                            //curPrice = curPrice + Convert.ToInt32(tbIncPrice.Text.Trim());

                            //计划报的价格不大于当前最低可成交价格，就可以提交报价了，_MyPlanBidPrice为计划报的价格,该价格由函数AutoBid确定
                            if (_MyPlanBidPrice - curPrice <= 300)
                            {
                                cbAutoCommit.Checked = false;
                                Point location = this._coordSetting.CommitButton.GetMidLocation();
                                MyMouseClick(location);
                                Console.ForegroundColor = ConsoleColor.Blue;
                                string logInfo = String.Format("{0},计划报价{1}已经在可出价范围{2}~{3},执行自动提交价格任务",
                                    DateTime.Now.ToString("HH:mm:ss.fff"), _MyPlanBidPrice, curPrice - 300, curPrice + 300);
                                Console.WriteLine(logInfo);
                                _log.Info(logInfo);
                                Console.ForegroundColor = ConsoleColor.White;
                                _autoCommitTime = DateTime.Now;
                            }
                            else
                            {
                                if (_autoCommitTime.Second != DateTime.Now.Second)
                                {
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    string logInfo = String.Format("当前时间{0}，计划价格{1}相对最高可接受价格{2}过高，暂缓出价",
                                       DateTime.Now.ToString("HH:mm:ss.fff"), _MyPlanBidPrice, curPrice + 300);
                                    Console.WriteLine(logInfo);
                                    _log.Info(logInfo);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    _autoCommitTime = DateTime.Now;
                                }
                            }

                        }
                        else
                        {
                            //如果无法从界面上成功获取最低可成交价格，在第一时间进行报价，听天由命了。
                            cbAutoCommit.Checked = false;
                            Point location = this._coordSetting.CommitButton.GetMidLocation();
                            MyMouseClick(location);
                            string logInfo = String.Format("{0},无法从界面上成功获取最低可成交价格，在第一时间进行报价，听天由命了", DateTime.Now.ToString("HH:mm:ss.fff"));
                            Console.ForegroundColor = ConsoleColor.Green;                            
                            Console.WriteLine(logInfo);
                            Console.ForegroundColor = ConsoleColor.White;
                            _log.Info(logInfo);
                        }
                    }
                }
                catch (Exception err)
                {
                    StringBuilder errInfo = new StringBuilder();
                    errInfo.Append(err.Message);
                    if (err.InnerException != null) errInfo.Append("\n").Append(err.InnerException.Message);
                    _log.Error(errInfo.ToString());
                    Console.WriteLine(err.Message);
                }

            }
        }

        private void btShowSysFont_Click(object sender, EventArgs e)
        {
            SysFontWnd fontsWnd = new SysFontWnd();
            fontsWnd.Show();
            fontsWnd.ShowAllSysFonts();
        }



        private void button12_Click(object sender, EventArgs e)
        {
            string response = Microsoft.VisualBasic.Interaction.InputBox("输入要新建字库的名称", "新建", "字体类型1", 0, 0);
            if (response.Length > 0)
            {
                _codeFontLib.AddNewFontType(response.ToUpper().Trim());
                _codeFontLib.SaveToFile();
                LoadCodeFontLib();
            }
        }

        private void cbCodeFontName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (plCodeBase.HasChildren)
            {

            }
            //int count = plCodeBase.Container.Components.Count;
            //for (int i = count - 1; i >= 0; i--)
            //{
            //    plCodeBase.Container.Remove(plCodeBase.Container.Components[i]);
            //}
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (cbCodeFontName.SelectedIndex == -1)
            {
                MessageBox.Show("先选择一个字库名称");
                return;
            }
            if (DlgOpen.ShowDialog() == DialogResult.OK)
            {
                Bitmap m_Bitmap = (Bitmap)Bitmap.FromFile(DlgOpen.FileName, false);
                //m_Bitmap = _BmpHelperIntf.BmpScale(m_Bitmap, picRate);
                m_Bitmap = _BmpHelperIntf.BmpScale(m_Bitmap, 4);
                picGridImg.Top = 0;
                picGridImg.Left = 0;
                picGridImg.SizeMode = PictureBoxSizeMode.AutoSize;
                picGridImg.Image = _BmpHelperIntf.PreProcessBmp(m_Bitmap, false);
                //m_Bitmap = _BmpHelperIntf.PreProcessBmp(m_Bitmap, false);
                //picGridImg.Image = _BmpHelperIntf.BmpScale(m_Bitmap, picRate);
                plOriCode.Height = m_Bitmap.Height;

            }
        }

        private void picGridImg_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox imgBox = sender as PictureBox;
            if (imgBox != null && imgBox.Image != null)
            {
                int gridSize = _BmpHelperIntf.GetBmpGridSize();
                Bitmap Sourcebm = (Bitmap)imgBox.Image;
                //Color c = Sourcebm.GetPixel(e.X, e.Y);
                StringBuilder sb = new StringBuilder();
                sb.Append("坐标信息：").Append(e.X / gridSize).Append(",").Append(e.Y / gridSize);
                lbCodePosInfo.Text = sb.ToString();
            }

        }

        private void picGridImg_MouseDown(object sender, MouseEventArgs e)
        {
            PictureBox imgBox = sender as PictureBox;
            if (imgBox != null && imgBox.Image != null)
            {
                int gridSize = _BmpHelperIntf.GetBmpGridSize();
                Bitmap Sourcebm = (Bitmap)imgBox.Image;
                if (_curCustomFont == null) _curCustomFont = new CustomFontPosition();
                _curCustomFont.AddNewPixel(new Point(e.X / gridSize, e.Y / gridSize));
                Bitmap fontbmp = _curCustomFont.BuildFontBmp(4);
                if (fontbmp != null)
                {
                    picFontDemo.Image = fontbmp;
                    //picFont.Image = _BmpHelperIntf.BmpScale(fontbmp, 2);
                }
            }
        }



        private void button16_Click(object sender, EventArgs e)
        {
            _curCustomFont = new CustomFontPosition();
            picFontDemo.Image = null;
        }

        private void button17_Click(object sender, EventArgs e)
        {
            List<Point> fontPointList = _curCustomFont.ExportSelfFontProperty(_BmpHelperIntf.GetBmpGridSize());
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Point location = this._coordSetting.AddPriceButton.GetMidLocation();
            MyMouseClick(location);

        }


        public void AutoCommitCallBack()
        {
            //MessageBox.Show("快捷键被调用！");
            cbAutoCommit.Checked = false;
            Point location = this._coordSetting.CommitButton.GetMidLocation();
            MyMouseClick(location);
        }

        public void SetAllowAutoCommitCheckBox()
        {
            cbAutoCommit.Checked = true;
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            //窗口消息处理函数
            _hotkey.ProcessHotKey(m);
            base.WndProc(ref m);
        }

        private void ckSetAutoCommit_CheckStateChanged(object sender, EventArgs e)
        {
            if (ckSetAutoCommit.Checked)
            {
                try
                {
                    //_hotkey.Regist(this.Handle, (int)HotKeys.HotkeyModifiers.Control + (int)HotKeys.HotkeyModifiers.Alt, Keys.E, CallBack);
                    _hotkey.Regist(this.Handle, 0, Keys.Enter, SetAllowAutoCommitCheckBox);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message);
                }
            }
            else
            {
                try
                {
                    _hotkey.UnRegist(this.Handle, SetAllowAutoCommitCheckBox);
                }
                catch (Exception e1)
                {
                    MessageBox.Show(e1.Message);
                }
            }

        }

        private void btSetIdenty_Click(object sender, EventArgs e)
        {
            setPriceMatchParam();
        }

        private void btPriceReset_Click(object sender, EventArgs e)
        {
            _priceStr = "";
            this.tbPrice.Text = "";
        }

        private void regIEVer(bool isX64)
        {
            Process processes = Process.GetCurrentProcess();
            string name = processes.ProcessName;
            RegistryKey rk = Registry.LocalMachine;
            RegistryKey rks;
            if (isX64)
            {
                rks = rk.CreateSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION");
            }
            else
            {
                rks = rk.CreateSubKey("SOFTWARE\\Microsoft\\Internet Explorer\\MAIN\\FeatureControl\\FEATURE_BROWSER_EMULATION");
            }

            rks.SetValue(name + ".EXE", "8888", RegistryValueKind.DWord);
            rk.Close();
        }

        private void hideWebScrollbar()
        {
            HtmlElementCollection bodys = this.webBrowser1.Document.GetElementsByTagName("body");
            HtmlElement body = null;
            if (bodys.Count > 0)
                body = bodys[0];
            if (body != null)
            {
                if (body.Style != null)
                    body.Style += " overflow: hidden; ";
                else
                    body.Style = " overflow: hidden; ";
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.webBrowser1.ReadyState == WebBrowserReadyState.Complete)
            {
                hideWebScrollbar();
            }
        }

        private void btGo_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(tbUrl.Text.Trim(), false);            
        }

        private void lbURL_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(lbURL.Text.Trim(), false);
        }

        private void webBrowser1_ProgressChanged(object sender, WebBrowserProgressChangedEventArgs e)
        {
            webLoadProgressBar1.Maximum = (int)(e.MaximumProgress);
            webLoadProgressBar1.Value = (int)(e.CurrentProgress);
            if (e.CurrentProgress == e.MaximumProgress)
            {
                hideWebScrollbar();
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            tbWebStatus.Text = e.TargetFrameName + ":" + e.Url;
        }

        private void tbRefrash_Click(object sender, EventArgs e)
        {
            webBrowser1.Refresh();
            hideWebScrollbar();
        }

        private void btSetCommitButton_Click(object sender, EventArgs e)
        {

        }

        private void btFontMatch_Click(object sender, EventArgs e)
        {
            if (pboxScreen.Image == null)
            {
                MessageBox.Show("请先显示图片");
                return;
            }
            try
            {
                int basePrice = Convert.ToInt32(etBasePrice.Text.Trim());
                List<Font> matchedFontList = new List<Font>();
                string v = _stdFontLib.AutoMathFont((Bitmap)(pboxScreen.Image), this._fontMatchPrecision, basePrice, matchedFontList);
                if (ckAutoAddFont.Checked)
                {
                    foreach (Font f in matchedFontList)
                    {
                        _stdFontLib.AppendFontToLib(tbBenchMark.Text.Trim(), Font);
                        _stdFontLib.SaveToFile(_stdFontLibFileName);
                    }
                    if (matchedFontList.Count > 0)
                    {


                        cbStdFontName.Items.Clear();
                        HashSet<Font> fontList = _stdFontLib.GetFontList();
                        foreach (Font font in fontList)
                        {
                            cbStdFontName.Items.Add(font);
                        }
                    }

                }

            }
            catch (Exception e1)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("自动识别字体发生错误：").Append(e1.Message);
                if (e1.InnerException != null)
                {
                    sb.Append(e1.InnerException.StackTrace);
                }
                MessageBox.Show(sb.ToString());
                return;
            }
        }

        private void tbUrl_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void WebControlLocationtimer_Tick(object sender, EventArgs e)
        {
            _webControlPoint = PointToScreen(webBrowser1.Location);
        }

        private void ImgMainForm_Shown(object sender, EventArgs e)
        {
            _webControlPoint = PointToScreen(webBrowser1.Location);
            WebControlLocationtimer.Enabled = true;
        }

        private static readonly string TimeServer = "cn.pool.ntp.org";
        public static void SyncSystemTimeThreadProc(Object stateObj)
        {
            try
            {
                NTPClient timeClient = new NTPClient(TimeServer);
                timeClient.Connect(true);
                Console.Write(timeClient.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Time sync ERROR: {0}", e.Message);
                return;
            }

        }

        private void btSyncTime_Click(object sender, EventArgs e)
        {
        }

        private void btChangeServerIP_ButtonClick(object sender, EventArgs e)
        {
            ChangeServerIP();            
        }

        private void btSyncTimer_ButtonClick(object sender, EventArgs e)
        {
            Thread timerThread = new Thread(SyncSystemTimeThreadProc);
            timerThread.Start(this);

        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            string userID = tbUser.Text.Trim();
            if (userID.Length == 0)
            {
                MessageBox.Show("用户不能为空，可以为中文，只是用于标识身份","警告");
                return;
            }
            if (!_teamAdmin.loginToServer(userID))
            {
                MessageBox.Show("用户:" + userID + "不能登录，一般来说，该用户名称已经被占用", "警告");
                return;
            }
            else
            {
                btLogin.Enabled = false;
                tbUser.Enabled = false;
                editForm.AdminUserID = userID;
            }
        }

        
        private void BuildUserPolicyData(object userData)
        {
            if (_disableUIFunc) return;
            UserPolicyData policyData = userData as UserPolicyData;
            if (policyData != null)
            {
                policyData.CanRemoteControl = cbCanRemoteControl.Checked;
                policyData.OfferTime = dtAutoBid.Value.Minute;
                policyData.OfferTime += dtAutoBid.Value.Second / 100.0;

                policyData.PriceMarkup = tbIncPrice.Text.Trim();

                policyData.Submit400 = dtAutoCommit400.Value.Second;
                policyData.Submit400 += Convert.ToInt32(tbAutoCommitMS400.Text) / 1000.0;

                policyData.Submit500 = dtAutoCommit500.Value.Second;
                policyData.Submit500 += Convert.ToInt32(tbAutoCommitMS500.Text) / 1000.0;

                policyData.SubmitForce = dtAutoCommit2.Value.Second;
                policyData.SubmitForce += Convert.ToInt32(tbAutoCommitMS2.Text) / 1000.0;

                policyData.ServerIP = tbServerIPInfo.Text.Trim();

            }
        }

        private void changePolicyFromNet(object obj)
        {
            UserPolicyData policyData = obj as UserPolicyData;
            if (policyData != null)
            {
                tbIncPrice.Text = policyData.PriceMarkup;
                FormPolicyEdit.resetTimePickerValue_1(dtAutoBid, policyData.OfferTime);
                FormPolicyEdit.resetTimePickerValue_2(dtAutoCommit400, tbAutoCommitMS400, policyData.Submit400);
                FormPolicyEdit.resetTimePickerValue_2(dtAutoCommit500, tbAutoCommitMS500, policyData.Submit500);
                FormPolicyEdit.resetTimePickerValue_2(dtAutoCommit2, tbAutoCommitMS2, policyData.SubmitForce);

            }
        }


        private void UserIsInList(object userState)
        {
            UserState userInfo = userState as UserState;
            if (userInfo != null)
            {
                foreach (ListViewItem item in lvUser.Items)
                {
                    if (item.Text.Equals(userInfo.UserID, StringComparison.OrdinalIgnoreCase))
                    {
                        userInfo.Exists = true;
                        return;
                    }
                }
                userInfo.Exists = false;
            }
        }

        private void setItemColor(ListViewItem item)
        {
            if (item.Tag == null) return;
            try
            {
                int tagValue = Convert.ToInt32(item.Tag);
                switch (tagValue)
                {
                    case 0:
                        item.ForeColor = Color.Black;
                        item.ToolTipText = "就是你自己";
                        break;
                    case 1:
                        item.ForeColor = Color.Crimson;
                        item.ToolTipText = "对方不接受远程控制";
                        break;
                    case 2:
                        item.ForeColor = Color.Blue;
                        item.ToolTipText = "可以远程调整策略";
                        break;
                    default:
                        break;
                }
            }
            catch { }
        }

        private void UpdatePolicyList(object userData)
        {
            UserPolicyData policyData = userData as UserPolicyData;
            if (policyData != null)
            {
                if (policyData.RemoveSelf)
                {                    
                    foreach (ListViewItem item in lvUser.Items)
                    {
                        if (item.Text.Equals(policyData.UserID, StringComparison.OrdinalIgnoreCase))
                        {
                            lvUser.Items.Remove(item);
                            return;
                        }
                    }
                }
                else
                {
                    //add or update
                    foreach (ListViewItem item in lvUser.Items)
                    {
                        if (item.Text.Equals(policyData.UserID, StringComparison.OrdinalIgnoreCase))
                        {
                            //update
                            item.SubItems[1].Text = policyData.OfferTime.ToString();
                            item.SubItems[2].Text = policyData.PriceMarkup.ToString();
                            item.SubItems[3].Text = policyData.Submit400.ToString();
                            item.SubItems[4].Text = policyData.Submit500.ToString();
                            item.SubItems[5].Text = policyData.SubmitForce.ToString();
                            item.SubItems[6].Text = policyData.ServerIP.ToString();
                            if (item.Tag != null)
                            {                                
                                try
                                {
                                    int tagValue = Convert.ToInt32(item.Tag);
                                    if (tagValue != 0)
                                    {
                                        item.Tag = !policyData.CanRemoteControl ? 1 : 2;
                                    }
                                }
                                catch { }
                            }
                            setItemColor(item);
                            return;
                        }
                    }
                    //add
                    ListViewItem item1 = new ListViewItem();
                    item1.Text = policyData.UserID;

                    item1.SubItems.Add(new ListViewItem.ListViewSubItem(item1, policyData.OfferTime.ToString()));
                    item1.SubItems.Add(new ListViewItem.ListViewSubItem(item1, policyData.PriceMarkup.ToString()));
                    item1.SubItems.Add(new ListViewItem.ListViewSubItem(item1, policyData.Submit400.ToString()));
                    item1.SubItems.Add(new ListViewItem.ListViewSubItem(item1, policyData.Submit500.ToString()));
                    item1.SubItems.Add(new ListViewItem.ListViewSubItem(item1, policyData.SubmitForce.ToString()));
                    item1.SubItems.Add(new ListViewItem.ListViewSubItem(item1, policyData.ServerIP.ToString()));
                    if (policyData.UserID.Equals(tbUser.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        item1.Tag = 0;  //表示不能进行远程控制，本处表示该记录就是当前终端，所以没有必要进行修改
                    }else{
                        //item1.Tag 0,1都不能修改，只是0表示该条记录是自身，1表示该记录为其他终端，但是这些终端不接受远程控制，
                        // 2表示这些远程终端接受远程控制
                        item1.Tag = !policyData.CanRemoteControl ? 1 : 2;
                    }
                    setItemColor(item1);                    
                    lvUser.Items.Add(item1);

                }
            }
        }

        private bool CheckIsOtherUser(out string uid)
        {
            uid = "";
            if (lvUser.Items.Count == 0) return false;
            if (lvUser.SelectedItems.Count == 0) return false;
            if (lvUser.SelectedItems[0].Text.Equals(tbUser.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            else
            {
                uid = lvUser.SelectedItems[0].Text;
            }
            if (lvUser.SelectedItems[0].Tag == null) return false;
            try
            {
                int tagValue = Convert.ToInt32(lvUser.SelectedItems[0].Tag);
                if (tagValue >= 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }                
            }
            catch
            {
                return false;
            }
            
        }

        #region IInfomationControl Members

        public void SyncGetSelfPolicyData(ref UserPolicyData policyData)
        {
            _SyncContext.Send(BuildUserPolicyData, policyData);
        }

        public void SyncUpdatePolicyList(ref UserPolicyData policyData)
        {
            _SyncContext.Post(UpdatePolicyList, policyData);
        }

        public bool SyncCheckUserExists(string userID)
        {
            UserState userState = new UserState();
            userState.UserID = userID;
            userState.Exists = false;
            _SyncContext.Send(UserIsInList, userState);
            return userState.Exists;
        }

        public void ChangeServerIP()
        {
            Console.WriteLine("收到断开当前服务器连接进行切换的命令");
            TCPConnHelper.disconnect8300();
        }

        public void AsyncUpdatePolicy(string adminUserID, ref UserPolicyData policyData)
        {
            Console.WriteLine("收到来自" + adminUserID + "的策略调整命令");
            _SyncContext.Post(changePolicyFromNet, policyData);
        }

        #endregion


        private void cmItemChangeServer_Click(object sender, EventArgs e)
        {
            string uid;
            if (CheckIsOtherUser(out uid))
            {
                _teamAdmin.NotifyChangeServer(uid);
                //MessageBox.Show(uid);
            }
        }

        private void ImgMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _disableUIFunc = true;
            _teamAdmin.StopCheckThread();
            _teamAdmin.logoutFromServer();

        }

        private bool buildSelectUserPolicyData(out UserPolicyData policyData)
        {
            policyData = new UserPolicyData();
            ListViewItem item = lvUser.SelectedItems[0];
            try
            {
                policyData.OfferTime = Convert.ToDouble(item.SubItems[1].Text);
                policyData.PriceMarkup = item.SubItems[2].Text;
                policyData.Submit400 = Convert.ToDouble(item.SubItems[3].Text);
                policyData.Submit500 = Convert.ToDouble(item.SubItems[4].Text);
                policyData.SubmitForce = Convert.ToDouble(item.SubItems[5].Text);
                return true;
            }catch(Exception e) {
                StringBuilder sb = new StringBuilder();
                sb.Append("从用户列表中解析策略数据失败，").Append(e.Message);
                Console.WriteLine(sb.ToString());
                return false;
            }

        }

        private void cmItemModifyPolicy_Click(object sender, EventArgs e)
        {
            string uid;
            if (CheckIsOtherUser(out uid))
            {
                editForm.Text = "【" + uid + "】策略修改";
                editForm.UserID = uid;
                //editForm.Location = lvUser.PointToClient(Cursor.Position);            
                Point showPoint = Cursor.Position;
                showPoint.X = userContextMenu.Left;
                showPoint.Y = userContextMenu.Top;

                editForm.Location = showPoint;

                UserPolicyData policyData;
                buildSelectUserPolicyData(out policyData);
                policyData.UserID = uid;
                editForm.initParam(policyData);
                editForm.Show();
            }
            
        }

        private void lvUser_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {

        }



    }
}
