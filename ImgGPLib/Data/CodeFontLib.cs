using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ImgGPLib.Data
{
    [Serializable]
    public  class CodeFontLib
    {
        private static string fileName = "CodeFontlib.dat";
        //public Dictionary<char, Dictionary<string, AbstractFont>> FontMap = new Dictionary<char, Dictionary<string, AbstractFont>>();
        public Dictionary<string, Dictionary<char, CodeFont>> FontMap = new Dictionary<string, Dictionary<char, CodeFont>>();

        #region 存盘或者载入

        public void SaveToFile()
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

        public static CodeFontLib LoadFromFile()
        {
            if (File.Exists(fileName))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream(fileName, FileMode.Open);
                try
                {
                    CodeFontLib lib = (CodeFontLib)(bf.Deserialize(fs));
                    //lib.RaiseFontLibChangedEvent();
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

        public HashSet<string> GetFontList()
        {
            HashSet<string> fontList = new HashSet<string>();
            foreach (KeyValuePair<string, Dictionary<char, CodeFont>> kvp in FontMap)
            {
                    fontList.Add(kvp.Key);
            }
            return fontList;
        }

        public void AddNewFontType(string fontName)
        {
            Dictionary<char, CodeFont> fontlist;
            if (!FontMap.TryGetValue(fontName, out fontlist))
            {
                FontMap.Add(fontName, null);
            }
        }

        public void AddNewChar(string fontName, char v, CodeFont fontProperty)
        {
            Dictionary<char, CodeFont> fontlist;
            if (!FontMap.TryGetValue(fontName, out fontlist))
            {
                fontlist = new Dictionary<char, CodeFont>();
                FontMap.Add(fontName, fontlist);
            }
            fontlist[v] = fontProperty;

        }
    }
}
