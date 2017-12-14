using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    /// <summary>
    /// UIDateTimeColumn
    /// </summary>
    //=============================================================================================
    public class UIDateTimeColumn : GridViewDateTimeColumn, IUIGridColumn
    {
        Type controlType;

        //-------------------------------------------------------------------------------------
        public UIDateTimeColumn()
        {
            controlType = typeof(UIDateTimePicker);
        }

        //-------------------------------------------------------------------------------------
        public Type ControlType
        {
            get { return controlType; }
            set { controlType = value; }
        }

        //---------------------------------------------------------------------------
        public void InitializeControl(IUIControl control)
        {
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
                DateTimePicker oPicker = new DateTimePicker();
                oPicker.Font = oFont;

                // new Size(100, 100) is an empirical value,
                // kind of a space size bounding the result. Note 
                // that it could be less than needed.
                iRequiredHeight = oPicker.GetPreferredSize(new Size(100, 100)).Height + iVerticalBorderThickness;
            }
            return iRequiredHeight;
        }




        public void InitializeFormatter(IDataObj prototypeDataObj, ITBFormatterProvider oFormatter)
        {
            // set the proper formatter to the column.
            // The Telerik grid seems to work well with date formats 
            // even when Custom Format is not set, so this 
            // statement could be avoided.
            this.CustomFormat = oFormatter.CustomDateFormatForEdit;
        }
    }
}