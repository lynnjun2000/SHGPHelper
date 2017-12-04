using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;

namespace ImgServer
{
    public partial class SysFontWnd : Form
    {
        public SysFontWnd()
        {
            InitializeComponent();
        }

        public void ShowAllSysFonts()
        {
            InstalledFontCollection ifc = new InstalledFontCollection();
	            FontFamily[] ffs = ifc.Families;
	            Font f;
	            richTextBox1.Clear();
                foreach (FontFamily ff in ffs)
                {
                    // 设置待写入文字的字体
                    if (ff.IsStyleAvailable(System.Drawing.FontStyle.Regular))
                        f = new Font(ff.GetName(1), 12, System.Drawing.FontStyle.Regular);
                    else if (ff.IsStyleAvailable(System.Drawing.FontStyle.Bold))
                        f = new Font(ff.GetName(1), 12, System.Drawing.FontStyle.Bold);
                    else if (ff.IsStyleAvailable(System.Drawing.FontStyle.Italic))
                        f = new Font(ff.GetName(1), 12, System.Drawing.FontStyle.Italic);
                    else
                        f = new Font(ff.GetName(1), 12, System.Drawing.FontStyle.Underline);
                    // 注意：每次AppendText之前都设置字体
                    //richTextBox1.SelectionFont = f;
                    //richTextBox1.AppendText(ff.GetName(1) + "\r\n");
                    richTextBox1.SelectionFont = f;
                    richTextBox1.AppendText("0123456789"+"  ("+ff.GetName(1)+")\r\n");
                    richTextBox1.SelectionFont = f;
                }
        }
    }
}
