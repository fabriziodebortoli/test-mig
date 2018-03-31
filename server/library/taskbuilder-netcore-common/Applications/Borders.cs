using System;
using System.Drawing;
using Microarea.Common.Generic;
//using Microarea.RSWeb.Temp;

namespace Microarea.Common.Applications
{
    /// <summary>
    /// Summary description for BorderPen.
    /// </summary>
    ///================================================================================ 
    [Serializable]
    public class BorderPen
	{
        public Color Color = Color.Black;
		public int Width = 1;

		//------------------------------------------------------------------------------
		public BorderPen()
		{
		}

		//------------------------------------------------------------------------------
		public BorderPen(Color aColor, int w = 1)
		{
			Color = aColor;
			Width = w;
		}

        //------------------------------------------------------------------------------
        public string ToJson(string name = "pen", bool bracket = false)
        {
            string s = name.ToJson() + ":{" +
                                        Width.ToJson("width") + ',' +
                                        Color.ToJson("color") +
                                     '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //------------------------------------------------------------------------------
        public bool IsDefault
		{
			get
			{
				return
					(Width == 1) &&
					(Color == Color.FromArgb(255, 0, 0, 0));
			}
		}

		//------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return (this == (BorderPen)obj);
		}

		//------------------------------------------------------------------------------
		public static bool operator !=(BorderPen e1, BorderPen e2)
		{
			return !(e1 == e2);
		}

		//------------------------------------------------------------------------------
		public static bool operator ==(BorderPen e1, BorderPen e2)
		{
			if (Object.ReferenceEquals(e1, e2))
				return true;

			if (Object.ReferenceEquals(e1, null) || Object.ReferenceEquals(e2, null))
				return false;

			return
				e1.Width == e2.Width &&
				e1.Color == e2.Color;
		}

		//------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}

    /// <summary>
    /// Summary description for Borders.
    /// </summary>
    //================================================================================ 
    [Serializable]
    public class Borders
	{
		public bool Left = true;
		public bool Right = true;
		public bool Top = true;
		public bool Bottom = true;

		//------------------------------------------------------------------------------
		public Borders(bool enabled) { Init(enabled); }
		public Borders() { Init(true); }

        //------------------------------------------------------------------------------
        public string ToJson(string name = "borders", bool bracket = false)
        {
            string s = name.ToJson() + ":{" +
                                Left.ToJson("left") + ',' +
                                Right.ToJson("right") + ',' +
                                Top.ToJson("top") + ',' +
                                Bottom.ToJson("bottom") + 
                                '}';

            if (bracket)
                s = '{' + s + '}';

            return s;
        }

        //------------------------------------------------------------------------------
        private void Init(bool enabled)
		{
			Top = enabled;
			Left = enabled;
			Bottom = enabled;
			Right = enabled;
		}

		//------------------------------------------------------------------------------
		public Borders(bool aTop, bool aLeft, bool aBottom, bool aRight)
		{
			Top = aTop;
			Left = aLeft;
			Bottom = aBottom;
			Right = aRight;
		}

		//------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return (this == (Borders)obj);
		}

		//------------------------------------------------------------------------------
		public static bool operator !=(Borders e1, Borders e2)
		{
			return !(e1 == e2);
		}

		//------------------------------------------------------------------------------
		public static bool operator ==(Borders e1, Borders e2)
		{
			if (Object.ReferenceEquals(e1, e2))
				return true;

			if (Object.ReferenceEquals(e1, null) || Object.ReferenceEquals(e2, null))
				return false;

			return
				e1.Bottom == e2.Bottom &&
				e1.Top == e2.Top &&
				e1.Left == e2.Left &&
				e1.Right == e2.Right;
		}

		//------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//------------------------------------------------------------------------------
		public bool IsDefault()
		{
			return Left && Right && Top && Bottom;
		}
	}
}
