using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImgGPLib.Data
{
    /// <summary>
    /// 字体，一个字体中有多种
    /// </summary>
    [Serializable]
    public abstract class AbstractFont
    {
        #region 属性定义
        /// <summary>
        /// 字体自身表示的字符，在这里只表示0~9
        /// </summary>
        public char FontValue;

        /// <summary>
        /// 符合字体自身高度的字体图片
        /// </summary>
        public Bitmap FontOriBmp = null;

        /// <summary>
        /// 字体名称
        /// </summary>
        public string FontName;
        /// <summary>
        /// 字体大小
        /// </summary>
        public float FontSize;
        /// <summary>
        /// 是否粗体
        /// </summary>
        public bool isBlod;

        /// <summary>
        /// 字体自身的宽度
        /// </summary>
        public int MaxWSize = 0;
        /// <summary>
        /// 字体自身的高度
        /// </summary>
        public int MaxHSize =0 ;
        /// <summary>
        /// 字体提取后的实际宽度
        /// </summary>
        public int RealWSize=0;
        /// <summary>
        /// 字体提取后的实际高度
        /// </summary>
        public int RealHSize=0;

        /// <summary>
        /// 字体提取后的字符串特征，使用01表示
        /// </summary>
        public string RealData="";

        #endregion

        protected AbstractFont()
        {
        }

        public AbstractFont(char value)
        {
            FontValue = value;
        }

        public void PrintMeta()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FontValue).Append("--");

            sb.Append(this.FontName).Append(",").Append(this.FontSize).Append(",");
            if (this.isBlod)
            {
                sb.Append("粗体");
            }
            else
            {
                sb.Append("普通");
            }
            sb.Append(",").Append(RealWSize).Append("X").Append(RealHSize);
            sb.Append(":").Append(this.RealData);
            Console.WriteLine(sb.ToString());
        }

        public static string GetFontStr(Font font)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(font.Name).Append(",").Append(font.Size);
            if (font.Bold)
            {
                sb.Append(",").Append("粗体");
            }
            return sb.ToString();
        }

        public static Font GetFontByStr(string fontstr)
        {
            string[] tmp = fontstr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            string fontName = tmp[0];
            float fontSize =(float)Convert.ToDouble(tmp[1]);

            if (tmp.Length > 2)
            {
                return new Font(fontName, fontSize, FontStyle.Bold);
            }
            else
            {
                return new Font(fontName, fontSize);
            }
            
        }
    }
}
