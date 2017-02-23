
using System;
using System.Drawing;
using System.Runtime.Serialization;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;

namespace Microarea.Common.Temp
{   //Temporary
    public class __Color
    {
        public int A { get; set; }

        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public __Color()
        {
            A = 255;
            R = 255;
            G = 255;
            B = 255;
        }

        public static __Color FromArgb(int a, int r, int g, int b)
        {
            __Color c = new __Color();
            c.A = a;
            c.B = b;
            c.R = r;
            c.G = g;
            return c;
        }
        public static __Color FromArgb(int cr)
        {
            __Color c = new __Color();
            c.R = cr & 0xFF;
            c.G = cr & 0xFF00;
            c.B = cr & 0xFF0000;
            c.A = 255;

          return c;
        }
    }

    // Summary:
    //     Specifies style information applied to text.
    [Flags]
    public enum __FontStyle
    {
        //
        // Summary:
        //     Normal text.
        Regular = 0,
        //
        // Summary:
        //     Bold text.
        Bold = 1,
        //
        // Summary:
        //     Italic text.
        Italic = 2,

        HS_BOLDITALIC = 3,

        //
        // Summary:
        //     Underlined text.
        Underline = 4,
        //
        // Summary:
        //     Text with a line through the middle.
        Strikeout = 8
    }
}