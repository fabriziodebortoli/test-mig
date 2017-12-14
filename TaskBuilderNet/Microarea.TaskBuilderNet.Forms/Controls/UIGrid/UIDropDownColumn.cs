using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Telerik.WinControls;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    /// <summary>
    /// UIDropDownColumn
    /// </summary>
    //=============================================================================================
    public class UIDropDownColumn : GridViewComboBoxColumn, IUIGridColumn
    {
        Type nonEditableControlType;
        Type editableControlType;

        bool multiSelection;

        //-------------------------------------------------------------------------------------
        public bool MultiSelection
        {
            get { return multiSelection; }
            set
            {
                multiSelection = value;
                nonEditableControlType = value ? typeof(UIMultiSelectionDropDownList) : typeof(UISingleSelectionDropDownList);

                if (multiSelection)
                {
                    this.DropDownStyle = RadDropDownStyle.DropDownList;
                }

                OnNotifyPropertyChanged("MultiSelection");
            }
        }



        //-------------------------------------------------------------------------------------
        public UIDropDownColumn()
        {
            nonEditableControlType = typeof(UISingleSelectionDropDownList);
            editableControlType = typeof(UIEditableDropDownList);
            this.PropertyChanged += new PropertyChangedEventHandler(UIDropDownColumn_PropertyChanged);
        }

        //-------------------------------------------------------------------------------------
        void UIDropDownColumn_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DropDownStyle" && this.DropDownStyle == RadDropDownStyle.DropDown)
            {
                MultiSelection = false;
            }
        }

        //-------------------------------------------------------------------------------------
        [Browsable(false)]
        public Type ControlType
        {
            get
            {
                return this.DropDownStyle == RadDropDownStyle.DropDown ? editableControlType : nonEditableControlType;
            }
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

            RadDropDownList control = Activator.CreateInstance(ControlType) as RadDropDownList;

            float buttonOffset = 0;
            foreach (RadElement element in control.RootElement.Children)
            {
                RadDropDownListElement radDropDownlistElem = element as RadDropDownListElement;
                if (radDropDownlistElem != null)
                {
                    buttonOffset = radDropDownlistElem.ArrowButton.DefaultSize.Width;
                    break;
                }
            }

            return SizeHelper.GetTextWidth(text, font) + (int)buttonOffset;
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
            
        }
    }
}