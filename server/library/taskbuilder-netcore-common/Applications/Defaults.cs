using Microarea.Common.Temp;
using System.Drawing;

namespace Microarea.Common.Applications
{
    /// <summary>
    /// Summary description for BorderPen.
    /// </summary>
    //================================================================================ 
    public class Defaults
	{
		public static Color DefaultTextColor = Color.FromArgb(255, 0, 0, 0); 
		public static Color DefaultBackColor = Color.FromArgb(255, 255, 255, 255);
        public static int DefaultBaseAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP;
		public static int DefaultPenWidth = 1;
		public static Color DefaultTotalForeground = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultTotalBackground = Color.FromArgb(255, 0, 0, 0);
        public static Color DefaultSubTotalForeground = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultSubTotalBackground = Color.FromArgb(255, 0, 0, 0);
        public static int DefaultAlign = BaseObjConsts.DT_CENTER | BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultTextAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_WORDBREAK | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultLabeleAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultFieldAlign = DefaultAlign | BaseObjConsts.DT_EX_VCENTER_LABEL;

		public static Color DefaultCellForeground = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultCellBackground = Color.FromArgb(255, 0, 0, 0);

        public static int DefaultCellStringAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultCellNumAlign = BaseObjConsts.DT_RIGHT | BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;

		public static int DefaultTotalStringAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultTotalNumAlign = BaseObjConsts.DT_RIGHT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_EXPANDTABS;

		public static Color DefaultColumnTitleForeground = Color.FromArgb(255, 255, 255, 255);
		public static Color DefaultColumnTitleBackground = Color.FromArgb(255,192, 192, 192);

		public static Color DefaultColumnBorderColor = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultColumnTitleBorderColor = Color.FromArgb(255, 255, 255, 255);

        public static Color DefaultTableTitleForeground = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultTableTitleBackground = Color.FromArgb(255, 0, 0, 0);

        public static int DefaultColumnTitleAlign = DefaultAlign;

		//public static Color AlternateColor = Color.FromArgb(25588, 210, 155);
		public static Color AlternateColor = Color.FromArgb(255, 192, 220, 192);
		

		public static Color DefaultEasyviewColor = Color.FromArgb(255, 240, 248, 255);
        public static Color DefaultShadowColor = Color.FromArgb(255, 240, 248, 255);
    }
}
