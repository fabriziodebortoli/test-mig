using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Microarea.Library.DataStructureUtils 
{
    /// <summary>
    /// DataGridViewNumericTextBoxColumn is a type of column specialized for
    /// holding DataGridViewNumericTextBoxCells.
    /// </summary>
    [DataGridViewColumnDesignTimeVisible(true)]
    public class DataGridViewUintTextBoxColumn : DataGridViewTextBoxColumn 
    {
        public DataGridViewUintTextBoxColumn()
        {
            this.ValueType = typeof(uint);

            this.CellTemplate = new DataGridViewUintTextBoxCell();
        }


        //--------------------------------------------------------------------------------------------------------------------------------
        public override DataGridViewCell CellTemplate
        {
            get { return base.CellTemplate; }
            set
            {
                // Ensure that the cell used for the template is a DataGridViewUintTextBoxCell.
                if (value != null && !value.GetType().IsAssignableFrom(typeof(DataGridViewUintTextBoxCell)))
                    throw new InvalidCastException("Must be a DataGridViewUintTextBoxCell");
                base.CellTemplate = value;
            }
        }
    }

    /// <summary>
    /// DataGridViewUintTextBoxCell represents the individual
    /// cells within a DataGridViewNumericTextBoxColumn. The cell is what appears 
    /// when the cell is in a non-editable (ie a display) state.
    /// </summary>
    public class DataGridViewUintTextBoxCell : DataGridViewTextBoxCell
    {
        public DataGridViewUintTextBoxCell()
        {
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Specify the type of object used for editing. This is how the WinForms
        /// framework figures out what type of edit control to make.
        /// </summary>
        public override Type EditType
        {
            get { return typeof(DataGridViewUintTextBoxEditingControl); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public override Type ValueType
        {
            get { return typeof(uint); }
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Specify the default cell contents upon creation of a new cell.
        /// </summary>
        public override object DefaultNewRowValue
        {
            get { return 0; }
        }

    }

    /// <summary>
    /// This is the control that is created when one of the numeric column's cells
    /// is edited by the user.
    /// </summary>
    public class DataGridViewUintTextBoxEditingControl : DataGridViewTextBoxEditingControl
    {
        public DataGridViewUintTextBoxEditingControl()
        {
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool IsValidForNumberInput(char c)
        {
            return (Char.IsDigit(c) || c == '\b');
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!IsValidForNumberInput(e.KeyChar))
                e.Handled = true;
        }
    }
}