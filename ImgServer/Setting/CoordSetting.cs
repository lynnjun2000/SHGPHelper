using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ImgServer.Setting
{
    [Serializable]
    enum CoordType
    {
        CTPriceTextbox,
        CTBidButton,
        CTCodeTextbox,
        CTCodeShow,
        CTCommitButton,
        CTAddPriceButton
    }

    [Serializable]
    class RectDefine
    {
        public Point LeftTop ;
        public Point RightBottom;

        public RectDefine()
        {
            LeftTop.X = 0;
            LeftTop.Y = 0;
            RightBottom.X = 0;
            RightBottom.Y = 0;
        }

        public void setValue(int left, int top, int right, int bottom, Point controlPoint)
        {
            LeftTop.X = left - controlPoint.X;
            LeftTop.Y = top - controlPoint.Y;
            RightBottom.X = right - controlPoint.X;
            RightBottom.Y = bottom - controlPoint.Y;
        }

        public Point GetMidLocation()
        {
            Point t = new Point();
            t.X = (LeftTop.X + RightBottom.X) / 2;
            t.Y = (LeftTop.Y + RightBottom.Y) / 2;
            return t;
        }
    }

    delegate void OnCoordSettingChanged(CoordType ctype,RectDefine define);

    [Serializable]
    class CoordSetting
    {
        /// <summary>
        /// 自定义加价按钮
        /// </summary>
        public RectDefine AddPriceButton = new RectDefine();
        /// <summary>
        /// 价格输入框
        /// </summary>
        public RectDefine PriceTextBox = new RectDefine();
        /// <summary>
        /// 出价按钮
        /// </summary>
        public RectDefine BidButton = new RectDefine();

        /// <summary>
        /// 验证码输入框
        /// </summary>
        public RectDefine CodeTextBox = new RectDefine();

        /// <summary>
        /// 验证码显示区域
        /// </summary>
        public RectDefine CodeShow = new RectDefine();

        /// <summary>
        /// 提交按钮
        /// </summary>
        public RectDefine CommitButton = new RectDefine();

        private static string _fileName = "CoordSetting.dat";

        [NonSerialized]
        public OnCoordSettingChanged OnCoordSettingChangedEvent = null;

        [NonSerialized]
        public SysSetting sysSetting = null;


        #region 存盘或者载入

        private void SaveToFile(string fileName)
        {
            BinaryFormatter bf = new BinaryFormatter();
            if (File.Exists(fileName)) File.Delete(fileName);
            FileStream fs = new FileStream(fileName, FileMode.CreateNew);
            try
            {
                bf.Serialize(fs, this);
            }
            finally
            {
                fs.Close();
            }
        }

        public void Save()
        {
            SaveToFile(_fileName);
        }

        private static CoordSetting LoadFromFile(string fileName,OnCoordSettingChanged settingChangeEvent)
        {
            if (File.Exists(fileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream(fileName, FileMode.Open);
                try
                {
                    CoordSetting setting = (CoordSetting)(bf.Deserialize(fs));
                    setting.OnCoordSettingChangedEvent += new OnCoordSettingChanged(settingChangeEvent);
                    setting.RaiseCoordChangedEvent();
                    return setting;
                }
                finally
                {
                    fs.Close();
                }
            }
            else
            {
                CoordSetting setting = new CoordSetting();
                setting.OnCoordSettingChangedEvent += new OnCoordSettingChanged(settingChangeEvent);
                setting.RaiseCoordChangedEvent();
                return setting;
            }
        }

        public static CoordSetting Load(OnCoordSettingChanged settingChangeEvent)
        {
            return LoadFromFile(_fileName, settingChangeEvent);
        }
        #endregion

        private void RaiseCoordChangedEvent()
        {
            if (OnCoordSettingChangedEvent != null)
            {
                OnCoordSettingChangedEvent(CoordType.CTAddPriceButton, this.AddPriceButton);
                OnCoordSettingChangedEvent(CoordType.CTPriceTextbox, this.PriceTextBox);
                OnCoordSettingChangedEvent(CoordType.CTBidButton, this.BidButton);
                OnCoordSettingChangedEvent(CoordType.CTCodeTextbox, this.CodeTextBox);
                OnCoordSettingChangedEvent(CoordType.CTCodeShow, this.CodeShow);
                OnCoordSettingChangedEvent(CoordType.CTCommitButton, this.CommitButton);

            }
        }

        public void SetCoordValue(CoordType ctType, int left, int top, int right, int bottom,  Point controlPoint)
        {
            switch (ctType)
            {
                case CoordType.CTAddPriceButton:
                    this.AddPriceButton.setValue(left, top, right, bottom,controlPoint);
                    break;
                case CoordType.CTPriceTextbox:
                    this.PriceTextBox.setValue(left, top, right, bottom,controlPoint);
                    break;
                case CoordType.CTBidButton:
                    this.BidButton.setValue(left, top, right, bottom,controlPoint);
                    break;
                case CoordType.CTCodeTextbox:
                    this.CodeTextBox.setValue(left, top, right, bottom,controlPoint);
                    break;
                case CoordType.CTCodeShow:
                    this.CodeShow.setValue(left, top, right, bottom,controlPoint);
                    break;
                case CoordType.CTCommitButton:
                    this.CommitButton.setValue(left, top, right, bottom,controlPoint);
                    break;
            }
            RaiseCoordChangedEvent();

        }

    }
}
