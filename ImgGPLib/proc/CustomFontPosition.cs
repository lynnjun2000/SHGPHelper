using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImgGPLib.proc
{
    public class CustomFontPosition
    {
        private List<Point> _fontPixelList = new List<Point>();

        private bool pixelExist(Point newPoint)
        {
            foreach (Point point in _fontPixelList)
            {
                if (point.X == newPoint.X && point.Y == newPoint.Y) return true;
            }
            return false;
        }

        public void AddNewPixel(Point newPoint)
        {
            if (!pixelExist(newPoint)) _fontPixelList.Add(newPoint);
        }

        private Point GetTopLeft()
        {
            Point topleft = new Point(int.MaxValue, int.MaxValue);
            foreach (Point point in _fontPixelList)
            {
                if (point.X < topleft.X) topleft.X = point.X;
                if (point.Y < topleft.Y) topleft.Y = point.Y;
            }
            if (topleft.X == int.MaxValue)
            {
                topleft.X = 0;
                topleft.Y = 0;
            }
            return topleft;
        }
        private int GetWidth(Point topLeft)
        {
            int width = 0;
            foreach (Point point in _fontPixelList)
            {
                if (point.X - topLeft.X > width) width = point.X - topLeft.X;
            }
            return width;
        }

        private int GetHeight(Point topLeft)
        {
            int height = 0;
            foreach (Point point in _fontPixelList)
            {
                int h = point.Y - topLeft.Y;
                if (h > height) height = h;
            }
            return height;
        }

        public List<Point> ExportSelfFontProperty(int r)
        {
            //int r = 12;
            List<Point> fixedPixelList = new List<Point>();
            Point topLeft = this.GetTopLeft();
            foreach (Point point in _fontPixelList)
            {
                int x = point.X - topLeft.X;
                int y = point.Y - topLeft.Y;
                int mx = x * r + r;
                int my = y * r + r;
                for (int posx = x * r; posx < mx; posx++)
                {
                    for (int posy = y * r; posy < my; posy++)
                    {
                        fixedPixelList.Add(new Point(posx, posy));
                    }
                }
            }
            return fixedPixelList;
        }

        public Bitmap BuildFontBmp(int r)
        {
            //int r =12;
            List<Point> fixedPixelList = ExportSelfFontProperty(r);
            Point topLeft = this.GetTopLeft();

            int iw = this.GetWidth(topLeft);
            int ih = this.GetHeight(topLeft);
            if (ih ==0 || iw == 0) return null;
            Bitmap bmp = new Bitmap((iw+1) * r, (ih+1)*r);

            foreach(Point point in fixedPixelList){
                bmp.SetPixel(point.X, point.Y, Color.Black);
            }
            //foreach (Point point in _fontPixelList)
            //{
            //    int x = point.X - topLeft.X;
            //    int y = point.Y - topLeft.Y;
            //    bmp.SetPixel(x, y, Color.Black);
            //}

            return bmp;
        }
    }
}
