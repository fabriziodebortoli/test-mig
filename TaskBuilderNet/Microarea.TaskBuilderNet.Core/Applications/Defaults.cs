using System.Drawing;

namespace Microarea.TaskBuilderNet.Core.Applications
{
	/// <summary>
	/// Summary description for BorderPen.
	/// </summary>
	//================================================================================ 
	public class Defaults
	{
		public static Color DefaultTextColor = Color.Black;
		public static Color DefaultBackColor = Color.White;
		public static int DefaultBaseAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP;
		public static int DefaultPenWidth = 1;
		public static Color DefaultTotalForeground = Color.Black;
		public static Color DefaultTotalBackground = Color.White;
		public static Color DefaultSubTotalForeground = Color.Black;
		public static Color DefaultSubTotalBackground = Color.White;
		public static int DefaultAlign = BaseObjConsts.DT_CENTER | BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultTextAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_WORDBREAK | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultLabeleAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultFieldAlign = DefaultAlign | BaseObjConsts.DT_EX_VCENTER_LABEL;

		public static Color DefaultCellForeground = Color.Black;
		public static Color DefaultCellBackground = Color.White;

		public static int DefaultCellStringAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultCellNumAlign = BaseObjConsts.DT_RIGHT | BaseObjConsts.DT_VCENTER | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_SINGLELINE | BaseObjConsts.DT_EXPANDTABS;

		public static int DefaultTotalStringAlign = BaseObjConsts.DT_LEFT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_EXPANDTABS;
		public static int DefaultTotalNumAlign = BaseObjConsts.DT_RIGHT | BaseObjConsts.DT_TOP | BaseObjConsts.DT_NOPREFIX | BaseObjConsts.DT_EXPANDTABS;

		public static Color DefaultColumnTitleForeground = Color.Black;
		public static Color DefaultColumnTitleBackground = Color.FromArgb(192, 192, 192);

		public static Color DefaultColumnBorderColor = Color.Black;
		public static Color DefaultColumnTitleBorderColor = Color.Black;

		public static Color DefaultTableTitleForeground = Color.Black;
		public static Color DefaultTableTitleBackground = Color.White;

		public static int DefaultColumnTitleAlign = DefaultAlign;

		//public static Color AlternateColor = Color.FromArgb(188, 210, 155);
		public static Color AlternateColor = Color.FromArgb(192, 220, 192);
		

		public static Color DefaultEasyviewColor = Color.AliceBlue;
		public static Color DefaultShadowColor = Color.AliceBlue;
	}
}
