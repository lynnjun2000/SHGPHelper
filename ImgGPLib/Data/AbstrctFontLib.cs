using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using System.Collections;

namespace ImgGPLib.Data
{
    [Serializable]
    public class CharXPos
    {
        public char Value;
        public int XPos;
    }

    class CharXPosCompare : IComparer<CharXPos>
    {

        #region IComparer<CharXPos> Members

        int IComparer<CharXPos>.Compare(CharXPos x, CharXPos y)
        {
            if (x.XPos == y.XPos) return 0;
            return x.XPos - y.XPos;
        }

        #endregion
    }

    [Serializable]
    public class MyChar
    {
        /// <summary>
        /// 表示的字符，如0,1,2
        /// </summary>
        public char Value;
        /// <summary>
        /// 该字符自身的实际高度，如果图片的高度小于本字体的高度，则不需要进行识别
        /// </summary>
        public int RealHeight;
        /// <summary>
        /// 该字符自身的实际宽度
        /// </summary>
        public int RealWight;
    }
    /// <summary>
    /// 字库文件，字库文件中包含多种字体
    /// </summary>
    [Serializable]
    public abstract class AbstrctFontLib
    {
        /// <summary>
        /// <字符， <字体，字体信息（包含宽度，高度，识别标示字符串）> >
        /// </summary>
        public Dictionary<char, Dictionary<Font, AbstractFont>> FontMap = new Dictionary<char, Dictionary<Font, AbstractFont>>();

        /// <summary>
        /// <字符识别标示字符串，对应的字符>
        /// </summary>
        //protected Dictionary<StringBuilder, MyChar> _identifyStrMap = new Dictionary<StringBuilder, MyChar>();
        protected Dictionary<string, MyChar> _identifyStrMap = new Dictionary<string, MyChar>();

        #region 存盘或者载入

        public void SaveToFile(string fileName)
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

        public static AbstrctFontLib LoadFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream(fileName, FileMode.Open);
                try
                {
                    AbstrctFontLib lib =(AbstrctFontLib)(bf.Deserialize(fs));
                    lib.RaiseFontLibChangedEvent();
                    return lib;
                }
                finally
                {
                    fs.Close();
                }
            }
            else
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// 根据当前字库中的内容，构建基于字符识别特征码的映射表对象
        /// </summary>
        private void BuildIdentifyStrMap()
        {
            Dictionary<string, MyChar> identifyMap = new Dictionary<string, MyChar>();

            foreach (KeyValuePair<char, Dictionary<Font, AbstractFont>> kvp in FontMap)
            {
                foreach (KeyValuePair<Font, AbstractFont> kvpDetail in kvp.Value)
                {
                    MyChar mychar;
                    if (!identifyMap.TryGetValue(kvpDetail.Value.RealData, out mychar))
                    {
                        mychar = new MyChar() { Value = kvpDetail.Value.FontValue, RealHeight = kvpDetail.Value.RealHSize , RealWight=kvpDetail.Value.RealWSize };
                        identifyMap[kvpDetail.Value.RealData] = mychar;
                    }
                }
            }
            _identifyStrMap = identifyMap;
        }

        protected Dictionary<string, MyChar> BuildAllSystemFontIdentifyStrMap(ref Dictionary<char, Dictionary<Font, AbstractFont>> fontmap)
        {
            Dictionary<string, MyChar> identifyMap = new Dictionary<string, MyChar>();
            foreach (KeyValuePair<char, Dictionary<Font, AbstractFont>> kvp in fontmap)
            {
                foreach (KeyValuePair<Font, AbstractFont> kvpDetail in kvp.Value)
                {
                    MyChar mychar;
                    if (!identifyMap.TryGetValue(kvpDetail.Value.RealData, out mychar))
                    {
                        mychar = new MyChar() { Value = kvpDetail.Value.FontValue, RealHeight = kvpDetail.Value.RealHSize, RealWight = kvpDetail.Value.RealWSize };
                        identifyMap[kvpDetail.Value.RealData] = mychar;
                    }
                }
            }
            //Console.WriteLine("构建系统字体识别特征库完成，累计识别码：" + identifyMap.Count+" 个");
            return identifyMap;
        }

        /// <summary>
        /// 当字库集合中的内容发生变化的时候，需要调用本事件处理函数
        /// </summary>
        protected void RaiseFontLibChangedEvent()
        {
            BuildIdentifyStrMap();
        }
        
    }
}
