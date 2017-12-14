using System;
using System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	/// <summary>
	/// UITextBoxColumn
	/// </summary>
	//=============================================================================================
	public class UITextBoxColumn : GridViewMaskBoxColumn, IUIGridColumn
	{
        Type controlType;
        //-------------------------------------------------------------------------------------
        public UITextBoxColumn()
		{
            controlType = typeof(UITextBox);
            WrapText = true;            
		}        

        //-------------------------------------------------------------------------------------
        public Type ControlType
        {
            get { return controlType; }
            set { if (value == typeof(UITextBox) || value.IsSubclassOf(typeof(UITextBox))) controlType = value; }
        }

        //---------------------------------------------------------------------------
        public void InitializeControl(IUIControl control)
        {
            UITextBox uiTextBox = control as UITextBox;
            uiTextBox.Multiline = true;
            uiTextBox.AcceptsReturn = true;
        }


        //---------------------------------------------------------------------------
        public float GetColumnWidth(IDataObj dataObj, ITBFormatterProvider formatterProvider, Font font)
        {
            string defaultString = formatterProvider.GetInputDefaultString(dataObj);
            string text = defaultString.Length > HeaderText.Length ? defaultString : HeaderText;
            return SizeHelper.GetTextWidth(text, font);
        }

        /// <summary>
        /// Evaluates the minimum height needed to entirely show the editor 
        /// for the column.
        /// </summary>
        /// <param name="iVerticalBorderThickness">Extra space to be added, 
        /// i.e. paddings and border vertical space.</param>
        /// <param name="oFont">Font used to evaluate string height.</param>
        /// <returns>The minimum height needed to entirely show the editor 
        /// for the column, taking into account the given extra vertical space. 
        /// Result is an integer as members of Size objects.</returns>
        //---------------------------------------------------------------------------
        public int GetMinHeight(int iVerticalBorderThickness, Font oFont)
        {
            int iRequiredHeight = 0;
            if (iVerticalBorderThickness < 0)
                // negative space does not make sense in this context.
                iVerticalBorderThickness = 0;
            if (oFont != null) 
            {
                TextBox oBox = new TextBox();
                oBox.Font = oFont;
                // new Size(100, 100) is an empirical value,
                // kind of a space size bounding the result. Note 
                // that it could be less than needed.
                iRequiredHeight = oBox.GetPreferredSize(new Size(100, 100)).Height + iVerticalBorderThickness;
            }
            return iRequiredHeight;            
        }

       
        
        
        /// <summary>
        /// Initializes the column format info according to the column data type.
        /// </summary>
        /// <param name="prototypeDataObj">A prototype data obj related to the column.</param>
        /// <param name="oFormatter">The formatter to be used for the column data obj.</param>
        /// //---------------------------------------------------------------------------
        public void InitializeFormatter(IDataObj prototypeDataObj, ITBFormatterProvider oFormatter)
        {
            if (prototypeDataObj != null && oFormatter != null)
            {
                if (
                    // if column data is a numeric one.
                            prototypeDataObj.DataType.Type == Core.CoreTypes.DataType.Double.Type ||
                            prototypeDataObj.DataType.Type == Core.CoreTypes.DataType.Money.Type ||
                            prototypeDataObj.DataType.Type == Core.CoreTypes.DataType.Quantity.Type ||
                            prototypeDataObj.DataType.Type == Core.CoreTypes.DataType.Percent.Type ||
                            prototypeDataObj.DataType.Type == Core.CoreTypes.DataType.Long.Type ||
                            prototypeDataObj.DataType.Type == Core.CoreTypes.DataType.Integer.Type
                           )
                {
                    // build proper numeric format infos.
                    CultureInfo ci = CultureInfo.CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);                    
                    ci.NumberFormat = oFormatter.NumberFormatInfo;
                    // set numeric format infos to the column.
                    this.FormatInfo = ci;
                }
            }
        }
    }
}