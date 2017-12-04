using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ImgGPLib.Data
{
    [Serializable]
    public class StdFont : AbstractFont
    {
        public StdFont(char value):base(value)
        {
            
        }

        public bool BuildData(Font font)
        {
            try
            {
                this.FontName = font.Name;
                this.FontSize = font.Size;
                this.isBlod = font.Bold;
                Bitmap bmp = new Bitmap(100, 100);
                Graphics g = Graphics.FromImage(bmp);
                string v = Convert.ToString(this.FontValue);
                Size sif = TextRenderer.MeasureText(g, v, font, new Size(0, 0), TextFormatFlags.NoPadding);
                this.MaxWSize = sif.Width;
                this.MaxHSize = sif.Height;

                FontOriBmp = new Bitmap(MaxWSize, MaxHSize);
                g = Graphics.FromImage(FontOriBmp);
                TextRenderer.DrawText(g, v, font, new Point() { X = 0, Y = 0 }, Color.Black, TextFormatFlags.NoPadding);

                //FontOriBmp.Save("d:\\font" + this.FontValue + ".bmp");

                //根据字体原始图片提取特征字符串
                PickFontPropertyStr(FixFontBmp(FontOriBmp));
                return true;
            }catch{
                return false;
            }
        }

        /// <summary>
        /// 根据原始图修复空白处，字体上下左右只保留一个空白像数
        /// </summary>
        /// <param name="sourcebmp"></param>
        protected Bitmap FixFontBmp(Bitmap sourcebmp)
        {
            int iw = sourcebmp.Width;    //图片宽度  
            int ih = sourcebmp.Height;    //图片高度  

            int left = 0;
            int top = 0;
            int right = iw;
            int bottom = ih;

            bool font_h_touched = false;
            bool font_w_touched = false;

            int dgGrayValue = 110;    //灰度值
            //目的找出左上角坐标和右下角坐标

            for (int y = 0; y < ih; y++)
            {
                bool h_isWhite = true;
                for (int x = 0; x < iw; x++)
                {
                    Color c = sourcebmp.GetPixel(x, y);
                    
                    if (c.A == 255)
                    {
                        //非空白处
                        h_isWhite = false;
                    }
                }
                if (h_isWhite)
                {
                    if (!font_h_touched)
                    {
                        top = y;
                    }
                    else
                    {
                        if (y< bottom) bottom = y;
                    }
                }
                else
                {
                    font_h_touched = true;
                }
            }


            Rectangle rect = new Rectangle(left,top,  right, bottom-top);
            return sourcebmp.Clone(rect, sourcebmp.PixelFormat);
            //return sourcebmp;
        }

        /// <summary>
        /// 根据字体原始图片提取特征字符串，字体上下左右只保留一个空白像数
        /// </summary>
        protected void PickFontPropertyStr(Bitmap sourcebmp)
        {
            //sourcebmp.Save("d:\\font" + this.FontValue + "_1.bmp");
            int iw = sourcebmp.Width;    //图片宽度  
            int ih = sourcebmp.Height;    //图片高度  

            this.RealWSize = iw;
            this.RealHSize = ih;
            //int dgGrayValue = 110;    //灰度值
            StringBuilder sb_data = new StringBuilder();

            for (int y = 0; y < ih; y++)
            {
                for (int x = 0; x < iw; x++)
                {
                    Color c = sourcebmp.GetPixel(x, y);
                    //StringBuilder sb = new StringBuilder();
                    //sb.Append(y).Append(":").Append(x).Append(" A:").Append(c.A)
                    //    .Append(" R:").Append(c.R)
                    //    .Append(" G:").Append(c.G)
                    //    .Append(" B:").Append(c.B);

                    //Console.WriteLine(sb.ToString());
                    if (c.A ==255)
                    {
                        //字体笔画位置
                        sb_data.Append("1");
                    }
                    else
                    {
                        //空白位置
                        sb_data.Append("0");
                    }
                }
            }
            this.RealData = sb_data.ToString();

        }
    }
}
