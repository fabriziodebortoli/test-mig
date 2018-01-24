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

        public static AlignType DefaultBaseAlign = AlignType.DT_LEFT | AlignType.DT_TOP;
		public static int DefaultPenWidth = 1;

		public static Color DefaultTotalForeground = Color.FromArgb(255, 0, 0, 0);
        public static Color DefaultTotalBackground = Color.FromArgb(255, 255, 255, 255);

        public static Color DefaultSubTotalForeground = Color.FromArgb(255, 0, 0, 0);
        public static Color DefaultSubTotalBackground = Color.FromArgb(255, 255, 255, 255);

        public static AlignType DefaultAlign = AlignType.DT_CENTER | AlignType.DT_VCENTER | AlignType.DT_NOPREFIX | AlignType.DT_SINGLELINE | AlignType.DT_EXPANDTABS;
		public static AlignType DefaultTextAlign = AlignType.DT_LEFT | AlignType.DT_TOP | AlignType.DT_NOPREFIX | AlignType.DT_WORDBREAK | AlignType.DT_EXPANDTABS;
		public static AlignType DefaultLabeleAlign = AlignType.DT_LEFT | AlignType.DT_TOP | AlignType.DT_NOPREFIX | AlignType.DT_SINGLELINE | AlignType.DT_EXPANDTABS;
		public static AlignType DefaultFieldAlign = DefaultAlign | AlignType.DT_EX_VCENTER_LABEL;

		public static Color DefaultCellForeground = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultCellBackground = Color.FromArgb(255, 0, 0, 0);

        public static AlignType DefaultCellStringAlign = AlignType.DT_LEFT | AlignType.DT_VCENTER | AlignType.DT_NOPREFIX | AlignType.DT_SINGLELINE | AlignType.DT_EXPANDTABS;
		public static AlignType DefaultCellNumAlign = AlignType.DT_RIGHT | AlignType.DT_VCENTER | AlignType.DT_NOPREFIX | AlignType.DT_SINGLELINE | AlignType.DT_EXPANDTABS;

		public static AlignType DefaultTotalStringAlign = AlignType.DT_LEFT | AlignType.DT_TOP | AlignType.DT_NOPREFIX | AlignType.DT_EXPANDTABS;
		public static AlignType DefaultTotalNumAlign = AlignType.DT_RIGHT | AlignType.DT_TOP | AlignType.DT_NOPREFIX | AlignType.DT_EXPANDTABS;

		public static Color DefaultColumnTitleForeground = Color.FromArgb(255, 0, 0, 0);
		public static Color DefaultColumnTitleBackground = Color.FromArgb(255,192, 192, 192);

		public static Color DefaultColumnBorderColor = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultColumnTitleBorderColor = Color.FromArgb(255, 255, 255, 255);

        public static Color DefaultTableTitleForeground = Color.FromArgb(255, 255, 255, 255);
        public static Color DefaultTableTitleBackground = Color.FromArgb(255, 0, 0, 0);

        public static AlignType DefaultColumnTitleAlign = DefaultAlign;

		public static Color AlternateColor = Color.FromArgb(255, 192, 220, 192);
		
		public static Color DefaultEasyviewColor = Color.FromArgb(255, 240, 248, 255);
        public static Color DefaultShadowColor = Color.FromArgb(255, 240, 248, 255);
    }
}
