using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ImgServer
{
    public partial class WinScreen : Form
    {
        private bool isClipping = false;
        private Rectangle rectSelected = Rectangle.Empty;
        private Bitmap resultBmp;
        private Bitmap screen;
        private Color coverColor;
        private Bitmap coverLayer = null;
        private Brush rectBrush = null;

        public WinScreen(Bitmap screen)
        {
            InitializeComponent();
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            coverLayer = new Bitmap(width, height);
            coverColor = Color.FromArgb(50, 200, 50, 100);
            rectBrush = new SolidBrush(coverColor);
            using (Graphics g = Graphics.FromImage(coverLayer))
            {
                g.Clear(coverColor);
            }
            this.Bounds = new Rectangle(0, 0, width, height);
            this.screen = screen;
            this.DoubleBuffered = true;
            //this.pictureBox1.Image = screen;

        }

        public Bitmap ResultBitmap
        {
            get { return resultBmp; }
        }

        public Rectangle SelectRect
        {
            get
            {
                return rectSelected;
            }
        }


        public void CapFullScreen()
        {
            //获得当前屏幕的大小 
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = Screen.PrimaryScreen.Bounds.Height;
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(0, 0, 0, 0, new Size(width, height));


            //double ceff = 1.5;//ceff为需要变暗的系数

            //for (int x = 0; x < bmp.Width; x++)
            //{
            //    for (int y = 0; y < bmp.Height; y++)
            //    {
            //        Color pixel = bmp.GetPixel(x, y);
            //        int valR = (int)(pixel.R / ceff);
            //        int valG = (int)(pixel.G / ceff);
            //        int valB = (int)(pixel.B / ceff);
            //        bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(valR, valG, valB));
            //    }
            //}
            //this.pictureBox1.Image = bmp;
            this.Refresh();
            this.DoubleBuffered = true;
        }


        private void WinScreen_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(screen, 0, 0);
            g.DrawImage(coverLayer, 0, 0);
            PaintRectangle();
        }

        private void PaintRectangle()
        {
            using (Graphics g = Graphics.FromImage(coverLayer))
            {
                g.Clear(coverColor);
                GraphicsPath path = new GraphicsPath();
                path.AddRectangle(this.Bounds);
                path.AddRectangle(rectSelected);
                g.FillPath(rectBrush, path);
                g.DrawRectangle(Pens.Blue, rectSelected);
            }
        }

        private void WinScreen_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isClipping = true;
                rectSelected.Location = e.Location;

            }
            else if (e.Button == MouseButtons.Right)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void WinScreen_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isClipping)
            {
                rectSelected.Width = e.X - rectSelected.X;
                rectSelected.Height = e.Y - rectSelected.Y;

                this.Invalidate();
            }

        }

        private void WinScreen_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isClipping)
            {
                rectSelected.Width = e.X - rectSelected.X;
                rectSelected.Height = e.Y - rectSelected.Y;
                if (rectSelected.Width <= 0 || rectSelected.Height <= 0)
                {
                    return;
                }
                this.Invalidate();
                resultBmp = new Bitmap(rectSelected.Width, rectSelected.Height);
                using (Graphics g = Graphics.FromImage(resultBmp))
                {
                    g.DrawImage(screen, new Rectangle(0, 0, rectSelected.Width, rectSelected.Height), rectSelected, GraphicsUnit.Pixel);
                }
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                isClipping = false;
            }
        }
    }
}
