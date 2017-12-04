using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Text;

namespace ImgGPLib.Data
{
    /// <summary>
    /// 标准字库
    /// </summary>
    [Serializable]
    public class StdFontLib : AbstrctFontLib
    {
        private Dictionary<char, Dictionary<Font, AbstractFont>> AllFontMap = null;


        private void buildAllSystemFontMap()
        {
            if (AllFontMap == null)
            {
                //构建所有字体数字的图形数据集合，本案例中allFontMap中包含0~9共10个key，每个key对应的value为一个集合
                //每个集合的key为一个字体对象，value为该字体对应某个数字的识别对象信息（主要包含高度、宽度、特征字符串等信息）
                AllFontMap = new Dictionary<char, Dictionary<Font, AbstractFont>>();
                string benchmarkStr = "0123456789";
                InstalledFontCollection ifc = new InstalledFontCollection();
                FontFamily[] ffs = ifc.Families;
                Font f;
                foreach (FontFamily ff in ffs)
                {
                    float fontSize = 10;
                    while (fontSize <= 20)
                    {

                        // 设置待写入文字的字体
                        if (ff.IsStyleAvailable(System.Drawing.FontStyle.Regular))
                        {
                            f = new Font(ff.GetName(1), fontSize, System.Drawing.FontStyle.Regular);
                            AppendFontToLib(benchmarkStr, f, ref AllFontMap);
                        }

                        if (ff.IsStyleAvailable(System.Drawing.FontStyle.Bold))
                        {
                            f = new Font(ff.GetName(1), fontSize, System.Drawing.FontStyle.Bold);
                            AppendFontToLib(benchmarkStr, f, ref AllFontMap);
                        }
                        fontSize += 0.5f;
                    }
                }
                Console.WriteLine("所有系统字库构建完成");

            }
        }

        public void AppendFontToLib(string benchmarkStr, Font font, ref Dictionary<char, Dictionary<Font, AbstractFont>> fontmap)
        {
            int len = benchmarkStr.Length;
            for (int i = 0; i < len; i++)
            {
                char v = benchmarkStr[i];
                StdFont stdfont = new StdFont(v);
                bool succ = stdfont.BuildData(font);
                if (!succ) return;
                //Console.WriteLine(stdfont.FontName+"   "+stdfont.RealData);
                Dictionary<Font, AbstractFont> fontInstanceMap;
                if (fontmap.TryGetValue(v, out fontInstanceMap))
                {
                    //字符存在
                    fontInstanceMap[font] = stdfont;
                }
                else
                {
                    //新字符
                    fontInstanceMap = new Dictionary<Font, AbstractFont>();
                    fontInstanceMap[font] = stdfont;
                    fontmap[v] = fontInstanceMap;
                }
            }
        }

        public void AppendFontToLib(string benchmarkStr, Font font)
        {
            int len = benchmarkStr.Length;
            for (int i = 0; i < len; i++)
            {
                char v = benchmarkStr[i];
                StdFont stdfont = new StdFont(v);
                stdfont.BuildData(font);
                Dictionary<Font,AbstractFont> fontInstanceMap;
                if (FontMap.TryGetValue(v, out fontInstanceMap))
                {
                    //字符存在
                    fontInstanceMap[font] = stdfont;
                }
                else
                {
                    //新字符
                    fontInstanceMap = new Dictionary<Font, AbstractFont>();
                    fontInstanceMap[font] = stdfont;
                    FontMap[v] = fontInstanceMap;
                }
            }
            RaiseFontLibChangedEvent();
        }

        public void RemoveFontType(Font font)
        {
            foreach (KeyValuePair<char, Dictionary<Font, AbstractFont>> kvp in FontMap)
            {
                List<Font> needRemovedList = new List<Font>();
                foreach (KeyValuePair<Font, AbstractFont> f in kvp.Value)
                {
                    if (font.Equals(f.Key))
                    {
                        needRemovedList.Add(f.Key);
                    }
                }
                foreach (Font needRemoveFont in needRemovedList)
                {
                    kvp.Value.Remove(needRemoveFont);
                }
            }
            RaiseFontLibChangedEvent();
        }


        ///// <summary>
        ///// 获取字库中所有字符识别字窜和字符的映射表
        ///// </summary>
        ///// <returns></returns>
        //private Dictionary<string, char> GetAllIdentifyStrs()
        //{
        //    Dictionary<string, char> strsMap = new Dictionary<string, char>();
        //}

        

        public void PrintFontInfo()
        {
            foreach (KeyValuePair<char,Dictionary<Font,AbstractFont>> kvp in FontMap){
                foreach (KeyValuePair<Font, AbstractFont> f in kvp.Value)
                {
                    f.Value.PrintMeta();
                }
            }
        }

        public HashSet<Font> GetFontList()
        {
            HashSet<Font> fontList = new HashSet<Font>();
            foreach (KeyValuePair<char, Dictionary<Font, AbstractFont>> kvp in FontMap)
            {

                foreach (Font font in kvp.Value.Keys)
                {
                    fontList.Add(font);
                }
            }
            return fontList;
        }

        //public bool AutoMathFont(Bitmap priceBmp, int precision, int basePrice, out Font matchedFont)
        public string AutoMathFont(Bitmap priceBmp, int precision, int basePrice, List<Font> matchedFontList)
        {            
            int iw = priceBmp.Width;
            int ih = priceBmp.Height;

            char[,] charArray = new char[ih, iw];    //定义个chai型的二维数组记录每个像素上0/1的值,形成一个矩形

            for (int posy = 0; posy < ih; posy++)
            {
                //按行读取图片数据
                for (int posx = 0; posx < iw; posx++)
                {
                    Color c = priceBmp.GetPixel(posx, posy);
                    if (c.R == 255)
                    {
                        //有笔画
                        charArray[posy, posx] = '0';
                    }
                    else
                    {
                        //空白，无笔画
                        charArray[posy, posx] = '1';
                    }
                }
            }
            string benchmarkStr = "0123456789";
            InstalledFontCollection ifc = new InstalledFontCollection();
            FontFamily[] ffs = ifc.Families;
            Font f1 = null;

            HashSet<Font> fonts = new HashSet<Font>();

            foreach (FontFamily ff in ffs)
            {
                float fontSize = 10;
                while (fontSize <= 20)
                {                    
                    Dictionary<char, Dictionary<Font, AbstractFont>> oneFontMap = new Dictionary<char, Dictionary<Font, AbstractFont>>();
                    // 设置待写入文字的字体
                    if (ff.IsStyleAvailable(System.Drawing.FontStyle.Regular))
                    {
                        StringBuilder sb1 = new StringBuilder();
                        //f1 = new Font(ff.GetName(1), fontSize, System.Drawing.FontStyle.Regular);
                        f1 = new Font(ff.Name, fontSize, System.Drawing.FontStyle.Regular);
                        AppendFontToLib(benchmarkStr, f1, ref oneFontMap);

                        Dictionary<string, MyChar> identifyStrMap = BuildAllSystemFontIdentifyStrMap(ref oneFontMap);
                        List<CharXPos> charSerial = new List<CharXPos>();
                        foreach (KeyValuePair<string, MyChar> kvp in identifyStrMap)
                        {
                            //忽略字体高度大于图片高度的字体识别特征码
                            if (kvp.Value.RealHeight > ih) continue;
                            List<CharXPos> posList;
                            findWord(ref charArray, kvp.Key, ih, iw, kvp.Value.RealWight, kvp.Value.RealHeight, kvp.Value.Value, precision, out posList);
                            if (posList.Count > 0) charSerial.AddRange(posList);
                        }

                        charSerial.Sort(new CharXPosCompare());
                        foreach (CharXPos charx in charSerial)
                        {
                            sb1.Append(charx.Value);
                        }
                        if (sb1.Length > 0)
                        {
                            Console.WriteLine(String.Format("{0} {1}pt {2} 识别内容：{3}", f1.Name,f1.Size ,f1.Style,sb1.ToString()));
                            try
                            {
                                int price = Convert.ToInt32(sb1.ToString());
                                if (price == basePrice)
                                {
                                    fonts.Add(f1);
                                    matchedFontList.Add(f1);
                                }
                            }
                            catch
                            {
                            }
                        }
                    }

                    if (ff.IsStyleAvailable(System.Drawing.FontStyle.Bold))
                    {
                        StringBuilder sb1 = new StringBuilder();
                        //f1 = new Font(ff.GetName(1), fontSize, System.Drawing.FontStyle.Bold);
                        f1 = new Font(ff.Name, fontSize, System.Drawing.FontStyle.Bold);
                        AppendFontToLib(benchmarkStr, f1, ref oneFontMap);
                        Dictionary<string, MyChar> identifyStrMap = BuildAllSystemFontIdentifyStrMap(ref oneFontMap);
                        List<CharXPos> charSerial = new List<CharXPos>();
                        foreach (KeyValuePair<string, MyChar> kvp in identifyStrMap)
                        {
                            //忽略字体高度大于图片高度的字体识别特征码
                            if (kvp.Value.RealHeight > ih) continue;
                            List<CharXPos> posList;
                            findWord(ref charArray, kvp.Key, ih, iw, kvp.Value.RealWight, kvp.Value.RealHeight, kvp.Value.Value, precision, out posList);
                            if (posList.Count > 0) charSerial.AddRange(posList);
                        }

                        charSerial.Sort(new CharXPosCompare());
                        foreach (CharXPos charx in charSerial)
                        {
                            sb1.Append(charx.Value);
                        }
                        if (sb1.Length > 0)
                        {
                            Console.WriteLine(String.Format("{0} {1}pt {2} 识别内容：{3}", f1.Name, f1.Size, f1.Style, sb1.ToString()));
                            try
                            {
                                int price = Convert.ToInt32(sb1.ToString());
                                if (price == basePrice)
                                {
                                    fonts.Add(f1);
                                    matchedFontList.Add(f1);
                                }
                            }
                            catch
                            {
                            }
                        }

                    }
                    fontSize += 0.25f;
                }

            }
            Console.WriteLine("自动匹配系统字体结束,匹配符合结果的字体类型：");
            foreach (Font f in fonts)
            {
                Console.WriteLine(String.Format("{0} {1}pt {2} GdiCharSet={3}", f.Name,f.Size ,f.Style,f.GdiCharSet));
            }
            return "";

        }

        public string IdentifyPrice(Bitmap priceBmp, int precision)
        {
            StringBuilder sb1 = new StringBuilder();

            int iw = priceBmp.Width;
            int ih = priceBmp.Height;

            char[,] charArray = new char[ih, iw];    //定义个chai型的二维数组记录每个像素上0/1的值,形成一个矩形

            for(int posy=0;posy<ih;posy++){
                //按行读取图片数据
                for (int posx=0;posx<iw;posx++){
                    Color c = priceBmp.GetPixel(posx, posy);

                    //StringBuilder sb = new StringBuilder();
                    //sb.Append(posy).Append(":").Append(posx).Append(" A:").Append(c.A)
                    //    .Append(" R:").Append(c.R)
                    //    .Append(" G:").Append(c.G)
                    //    .Append(" B:").Append(c.B);

                    //Console.WriteLine(sb.ToString());

                    if (c.R == 255)
                    {
                        //有笔画
                        charArray[posy, posx] = '0';
                    }
                    else
                    {
                        //空白，无笔画
                        charArray[posy, posx] = '1';
                    }
                }
            }

            //对每一种字符识别特征码进行识别
            //Dictionary<int,char> foundCharMap = new Dictionary<int,char>();
            List<CharXPos> charSerial = new List<CharXPos>();
            foreach (KeyValuePair<string, MyChar> kvp in _identifyStrMap)
            {
                //忽略字体高度大于图片高度的字体识别特征码
                if (kvp.Value.RealHeight > ih) continue;
                List<CharXPos> posList;
                findWord(ref charArray, kvp.Key, ih, iw, kvp.Value.RealWight, kvp.Value.RealHeight, kvp.Value.Value,precision,out posList);
                if (posList.Count > 0) charSerial.AddRange(posList);
            }

            charSerial.Sort(new CharXPosCompare());
            foreach (CharXPos charx in charSerial)
            {
                sb1.Append(charx.Value);
            }
            return sb1.ToString();
        }

        /// <summary>
        /// 和字库进行匹配
        /// </summary>
        /// <param name="charArray">记录图片中每个像素的二维数组</param>
        /// <param name="charNum">字库中0/1值一维数组形式的字符的字符串</param>
        /// <param name="imageHeight">图片的像素高度</param>
        /// <param name="imageWidth">图片的像素宽度</param>
        /// <param name="binaryWidth">字库中该字符的像素宽度</param>
        /// <param name="binaryHeight">字库中该字符的像素高度</param>
        /// <param name="stringChar">字库中该字符</param>
        public void findWord(ref char[,] charArray,  string charNum, int imageHeight, int imageWidth,
            int binaryWidth, int binaryHeight, char stringChar, int precision ,out List<CharXPos> posList)
        {
            if (precision > 100) precision = 99;
            posList = new List<CharXPos>();
            StringBuilder bmpSB = new StringBuilder();
            bool found = false;
            //目标图的高度比字库的高度大，只需要从最上部分到两个图的高度差之间进行循环，表示从图的顶部按像数下移，直到字库的底部和目标图的底部重合
            for (int posy = 0; posy < imageHeight - binaryHeight; posy++)  
            {
                if (found) return;
                //目标图的宽度必然比字库的宽度大，只需要从最左边部分到右边（保留一个字库的宽度，超过这个位置，剩余的目标图部分就比字库的小了）
                for (int posx = 0; posx < imageWidth - binaryWidth; posx++)
                {
                    //在当前的位置提取同字库大小相同的图片数据
                    //int posb_y = posy + binaryHeight;
                    //int posb_x = posx + binaryWidth;
                    bmpSB.Remove(0, bmpSB.Length);
                    for (int y = posy; y < posy+ binaryHeight; y++)
                    {
                        for (int x = posx; x < posx+ binaryWidth; x++)
                        {
                            bmpSB.Append(charArray[y, x]);
                        }
                    }
  
                    //bmpSB中包含的就是某个字库大小的位图数据
                    if (CheckIsSame(ref bmpSB, ref charNum, precision))  //99
                    {
                        //当前选择的区域和字库吻合                        
                        //Console.WriteLine("找到字符:" + stringChar +" pos:"+posx);
                        posx = posx + binaryWidth-1;
                        CharXPos t = new CharXPos() { Value = stringChar, XPos = posx };
                        posList.Add(t);
                        found = true;
                    }
                    else
                    {
                        //当前选择的区域和字库不吻合
                    }
                    
                }
            }
        }

        /// <summary>
        /// 检查和字库字符串之间的相似度
        /// </summary>
        /// <param name="bmpData">要进行比较的图片数据</param>
        /// <param name="fontData">字库的数据</param>
        /// <param name="relativityValue">相似度，100表示全部吻合</param>
        /// <returns></returns>
        private bool CheckIsSame(ref StringBuilder bmpData, ref string fontData, double relativityValue)
        {
            if (bmpData.Length ==0) return false;
            int matchedPixelCount = 0;
            for (int i = 0; i < bmpData.Length; i++)
            {
                if (bmpData[i] == fontData[i]) matchedPixelCount++;                
            }
            double matchedValue = (matchedPixelCount+0.0) / bmpData.Length * 100;

            //if (matchedValue > 95)
            //{
            //    Console.WriteLine(matchedValue);
            //}
            if (matchedValue >= relativityValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
