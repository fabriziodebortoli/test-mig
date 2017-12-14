using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	/// <summary>
	/// UICheckBoxColumn
	/// </summary>
	//=============================================================================================
	public class UICheckBoxColumn : GridViewTextBoxColumn, IUIGridColumn
	{
        Type controlType;
        //-------------------------------------------------------------------------------------
        public UICheckBoxColumn()
		{
            controlType = typeof(UICheckBox);
		}

        //-------------------------------------------------------------------------------------
        public Type ControlType
        {
            get { return controlType; }
            set { controlType = value; }
        }

        //---------------------------------------------------------------------------
        public float GetColumnWidth(IDataObj dataObj, ITBFormatterProvider formatterProvider, Font font)
        {
            string defaultString = formatterProvider.GetInputDefaultString(dataObj);
            string text  = defaultString.Length > HeaderText.Length ? defaultString : HeaderText;
            return SizeHelper.GetTextWidth(text, font);
        }

        //---------------------------------------------------------------------------
        public void InitializeControl(IUIControl control)
        {
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
                CheckBox oBox = new CheckBox();
                oBox.Font = oFont;
            
                // new Size(100, 100) is an empirical value,
                // kind of a space size bounding the result. Note 
                // that it could be less than needed.
                iRequiredHeight = oBox.GetPreferredSize(new Size(100, 100)).Height + iVerticalBorderThickness;
            }
            return iRequiredHeight;       
        }


        public void InitializeFormatter(IDataObj prototypeDataObj, ITBFormatterProvider oFormatter)
        {
           
        }
    }
}
