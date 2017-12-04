using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImgGPLib.Data
{
    public class CodeFont : AbstractFont
    {
        private List<Point> _fontProperty;

        public CodeFont(char value)
            : base(value)
        {
        }

        public List<Point> FontProperty
        {
            get
            {
                return _fontProperty;
            }
            set
            {
                _fontProperty = value;
            }
        }
    }
}
