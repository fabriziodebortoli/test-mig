using System.Drawing;
using Microarea.TaskBuilderNet.Core.ComponentModel;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.Primitives;


namespace Microarea.TaskBuilderNet.Forms
{
    internal sealed class SizeHelper
    {
		//------------------------------------------------------------------
		private SizeHelper()
		{}

        //------------------------------------------------------------------
        public static float GetTextWidth(int maxLen, Font font)
        {
            return GetTextWidth(ComponentModelHelper.RandomString(maxLen), font);
        }

        //------------------------------------------------------------------
        public static float GetTextWidth(string text, Font font)
        {
            return GetTextSize(text, font).Width;
        }

        //------------------------------------------------------------------
        public static float GetTextHeight(string text, Font font)
        {
            return GetTextSize(text, font).Height;
        }

        //------------------------------------------------------------------
        public static SizeF GetTextSize(string text, Font font)
        {
            if (string.IsNullOrEmpty(text) || font == null)
                return SizeF.Empty;

            TextPrimitiveImpl textPrimitive = new TextPrimitiveImpl();
            TextParams textParams = new TextParams();
            textParams.text = text;
            textParams.font = font;
            textParams.useCompatibleTextRendering = true;
            return textPrimitive.GetTextSize(textParams);
        }


        /// <summary>
        /// Evaluates the height of the editor bound to the given grid column.
        /// </summary>
        /// <param name="uiColumn">The given column</param>
        /// <returns>The height of the editor for the given column, 0 if uiColumn is null.</returns>
        //------------------------------------------------------------------
        public static int GetEditorHeight(IUIGridColumn uiColumn, int iVerticalBorderThickness, Font oFont)
        {
            int iRequiredHeight = 0;
            if (uiColumn != null && oFont != null) 
            {
                iRequiredHeight = uiColumn.GetMinHeight(iVerticalBorderThickness, oFont);                 
            }
            return iRequiredHeight;
        }
    }
}
