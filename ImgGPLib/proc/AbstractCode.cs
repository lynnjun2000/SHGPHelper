using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using ImgGPLib.intf;

namespace ImgGPLib.proc
{
    public abstract class AbstractCode : IntfCodeHelper
    {
        //public Bitmap  JPEG2BMP(
        public Bitmap BmpScale(Bitmap sourceBmp,int rate)
        {
            int newW = sourceBmp.Width * rate;
            int newH = sourceBmp.Height * rate;
            Bitmap destBmp = new Bitmap(newW, newH);
            Graphics g = Graphics.FromImage(destBmp);
            //设置高质量插值法  
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度  
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //消除锯齿
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawImage(sourceBmp, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, sourceBmp.Width, sourceBmp.Height), GraphicsUnit.Pixel);
            g.Dispose();
            return destBmp;
        }

        public int GetBmpGridSize()
        {
            return 12;
        }
        private void MakeGrid(Bitmap sourceBmp)
        {
            int gridSize = GetBmpGridSize();
            Graphics g = Graphics.FromImage(sourceBmp);
            int iw = sourceBmp.Width;
            int ih = sourceBmp.Height;
            int incPixel = ih / gridSize;
            Pen pen = new Pen(Color.Red);
            for (int posy = 0; posy < ih; posy++)
            {
                if (posy % gridSize == 0)
                {
                    g.DrawLine(pen, new Point(0, posy), new Point(iw, posy));
                }
            }
            for (int posx = 0; posx < iw; posx++)
            {
                if (posx % gridSize == 0)
                {
                    g.DrawLine(pen,new Point(posx,0),new Point(posx,ih));
                }
            }
        }

        public Bitmap PreProcessBmp(Bitmap sourceBmp, bool removeNoise,bool drawGrid){
            Bitmap bmp = sourceBmp;
            if (removeNoise)
            {
                bmp = RemoveNoise(bmp);
            }
            else
            {
                bmp = RemoveNoise(bmp, 100);
            }
            bmp = MakeBMPGray(bmp);
            bmp = MakeBMPSingle(bmp,150);
            if (drawGrid) MakeGrid(bmp);
            return bmp;
        
        }
        /// <summary>
        /// 对验证码进行预处理，灰度化，单色化
        /// </summary>
        /// <param name="sourceBmp">原始验证码</param>
        /// <param name="removeNoise">是否消除噪点</param>
        /// <returns></returns>
        public Bitmap PreProcessBmp(Bitmap sourceBmp, bool removeNoise)
        {
            return PreProcessBmp(sourceBmp, removeNoise, true);
        }


        private Bitmap RemoveNoise(Bitmap source,int colorValue)
        {
            Bitmap sourcebmp = source.Clone() as Bitmap;
            int iw = sourcebmp.Width;    //图片宽度  
            int ih = sourcebmp.Height;    //图片高度  
            for (int i = 0; i < iw; i++)
            {
                for (int j = 0; j < ih; j++)
                {
                    Color c = sourcebmp.GetPixel(i, j);
                    if (c.R < colorValue)
                    {
                        sourcebmp.SetPixel(i, j, Color.FromArgb(c.A, 255, 255, 255));
                    }
                }
            }
            return sourcebmp;

        }
        public  Bitmap RemoveNoise(Bitmap source)
        {
            return RemoveNoise(source, 155);
        }

        public Bitmap MakeBMPGray(Bitmap source)
        {
            Bitmap sourcebmp = source.Clone() as Bitmap;
            int iw = sourcebmp.Width;    //图片宽度  
            int ih = sourcebmp.Height;    //图片高度  
            //下面双循环是图片灰度处理  
            for (int i = 0; i < iw; i++)
            {//从左到右
                for (int j = 0; j < ih; j++)
                {//从上到下
                    Color c = sourcebmp.GetPixel(i, j);    //获取该点的颜色
                    int luma = (int)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);    //将颜色转换为亮度数值体现  
                    sourcebmp.SetPixel(i, j, Color.FromArgb(luma, luma, luma));    //将这一点进行灰度处理,非白色的部分变黑
                }
            }
            return sourcebmp;
        }

        public Bitmap MakeBMPSingle(Bitmap source, int flagvalue)
        {
            if (flagvalue<100 || flagvalue > 255) flagvalue = 150;
            
            Bitmap sourcebmp = source.Clone() as Bitmap;
            int iw = sourcebmp.Width;    //图片宽度  
            int ih = sourcebmp.Height;    //图片高度  
            int dgGrayValue = flagvalue;// 110;    //灰度值
            for (int i = 0; i < iw; i++)
            {//从左到右
                for (int j = 0; j < ih; j++)
                {//从上到下
                    Color c = sourcebmp.GetPixel(i, j);    //获取该点的颜色
                    if (c.R < dgGrayValue)
                    {
                        sourcebmp.SetPixel(i, j, Color.Black);
                    }
                    else
                    {
                        sourcebmp.SetPixel(i, j, Color.White);
                    }

                }
            }
            return sourcebmp;

        }

    }
}
