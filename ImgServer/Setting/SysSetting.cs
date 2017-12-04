using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ImgServer.Setting
{
    [Serializable]
    class SysSetting
    {
        private static string _fileName = "SysDefine.dat";
        /// <summary>
        /// 输入框选择等待时间
        /// </summary>
        public int SelectSpan = 0;
        /// <summary>
        /// 每次键盘击键间隔时间
        /// </summary>
        public int KeyDownSpan =0;
        /// <summary>
        /// 鼠标点击按钮前等待时间
        /// </summary>
        public int ClickSpan =0 ;


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

        private static SysSetting LoadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream(fileName, FileMode.Open);
                try
                {
                    SysSetting setting = (SysSetting)(bf.Deserialize(fs));
                    return setting;
                }
                finally
                {
                    fs.Close();
                }
            }
            else
            {
                SysSetting setting = new SysSetting();
                return setting;
            }
        }

        public static SysSetting Load()
        {
            return LoadFromFile(_fileName);
        }
        #endregion



    }
}
