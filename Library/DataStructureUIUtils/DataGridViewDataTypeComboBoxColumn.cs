using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Drawing;

using Microarea.Library.TBWizardProjects;

namespace Microarea.Library.DataStructureUtils.UI
{
    public class DataGridViewDataTypeComboBoxColumn : System.Windows.Forms.DataGridViewColumn
    {
        //--------------------------------------------------------------------------------------------------------------------------------
        public DataGridViewDataTypeComboBoxColumn(DataGridViewTextBoxColumn aDataGridViewUintTextBoxColumn)
            :
            base(new DataGridViewDataTypeCell(aDataGridViewUintTextBoxColumn))
        {
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public DataGridViewDataTypeComboBoxColumn()
            :
            this(null)
        {
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        public override DataGridViewCell CellTemplate 
        {
            get { return base.CellTemplate; }
            set
            {
                if (value != null && !typeof(DataGridViewDataTypeCell).IsAssignableFrom(value.GetType()))
                    throw new InvalidCastException("CellTemplate must be a DataGridViewDataTypeCell object.");

                base.CellTemplate = value;
            }
        }
       
        //===============================================================================================================================
        public class DataGridViewDataTypeCell : System.Windows.Forms.DataGridViewComboBoxCell
        {
            private DataGridViewTextBoxColumn relatedDataGridViewLengthColumn = null;

            //--------------------------------------------------------------------------------------------------------------------------------
            public DataGridViewDataTypeCell(DataGridViewTextBoxColumn aDataGridViewUintTextBoxColumn)
            {
                relatedDataGridViewLengthColumn = aDataGridViewUintTextBoxColumn;

                this.FlatStyle = FlatStyle.Standard;
                this.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                this.DisplayStyleForCurrentCellOnly = true;
                
                FillDataTypeDescriptions();
            }

            //--------------------------------------------------------------------------------------------------------------------------------
            public DataGridViewDataTypeCell()
                :
                this(null)
            {
            }

            private DataTable dataTypesTable = null;
            private const string DataTypeDescriptionColumnName = "TypeDescription";
            private const string DataTypeEnumValueColumnName = "TypeEnumValue";
            //--------------------------------------------------------------------------------------------------------------------------------
            private void FillDataTypeDescriptions()
            {
                dataTypesTable = new DataTable();
                dataTypesTable.Columns.Add(DataTypeDescriptionColumnName, typeof(String));
                dataTypesTable.Columns.Add(DataTypeEnumValueColumnName, typeof(WizardTableColumnDataType.DataType));

                Array dataTypeValues = Enum.GetValues(typeof(WizardTableColumnDataType.DataType));
                if (dataTypeValues != null && dataTypeValues.Length > 0)
                {
                    //int idx=0;
                    //string[] dataTypeValueDescriptions = new string[dataTypeValues.Length];
                    //foreach (WizardTableColumnDataType.DataType aDataType in dataTypeValues)
                    //    dataTypeValueDescriptions[idx++] = WizardTableColumnDataType.GetDataTypeDescription(aDataType);

                    //this.DataSource = dataTypeValueDescriptions;
                    foreach (WizardTableColumnDataType.DataType aDataType in dataTypeValues)
                    {
                        DataRow dataTypeRow = dataTypesTable.NewRow();
                        dataTypeRow[DataTypeDescriptionColumnName] = WizardTableColumnDataType.GetDataTypeDescription(aDataType);
                        dataTypeRow[DataTypeEnumValueColumnName] = aDataType;
                        dataTypesTable.Rows.Add(dataTypeRow);
                    }
                }

                this.DataSource = dataTypesTable;
                this.DisplayMember = DataTypeDescriptionColumnName;
                this.ValueMember = DataTypeEnumValueColumnName;
            }
            
            //--------------------------------------------------------------------------------------------------------------------------------
            public override object Clone()
            {
                DataGridViewDataTypeCell cell = base.Clone() as DataGridViewDataTypeCell;
                cell.relatedDataGridViewLengthColumn = relatedDataGridViewLengthColumn;
                return cell;
            }

            //--------------------------------------------------------------------------------------------------------------------------------
            public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, System.ComponentModel.TypeConverter formattedValueTypeConverter, System.ComponentModel.TypeConverter valueTypeConverter)
            {
                return WizardTableColumnDataType.GetDataTypeFromDescription(formattedValue as String);
            }

            //--------------------------------------------------------------------------------------------------------------------------------
            protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, System.ComponentModel.TypeConverter valueTypeConverter, System.ComponentModel.TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
            {
                if (value == null)
                    return String.Empty;

                return WizardTableColumnDataType.GetDataTypeDescription((WizardTableColumnDataType.DataType)value);
            }

            //--------------------------------------------------------------------------------------------------------------------------------
            public override Type FormattedValueType
            {
                get { return typeof(string); }
            }

            //--------------------------------------------------------------------------------------------------------------------------------
            public override Type ValueType
            {
                get { return typeof(WizardTableColumnDataType.DataType); }
            }

            //---------------------------------------------------------------------------------------------------------------------------
            public override object DefaultNewRowValue
            {
                get { return WizardTableColumnDataType.DataType.String; }
            }

            //---------------------------------------------------------------------------------------------------------------------------
            protected override bool ClickUnsharesRow(DataGridViewCellEventArgs e)
            {
                return true;
            }
            //--------------------------------------------------------------------------------------------------------------------------------
            // The InitializeEditingControl method is called by the grid control when the editing control 
            // is about to be shown. 
            // This gives the cell a chance to initialize its editing control based on its own properties 
            // and the formatted value provided. 
            //--------------------------------------------------------------------------------------------------------------------------------
            public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
            {
                base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

                if (this.DataGridView != null)
                {
                    DataGridViewComboBoxEditingControl comboBox = this.DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
                    if (comboBox != null)
                    {
                        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                        comboBox.AutoCompleteMode = AutoCompleteMode.None;
                        comboBox.TabStop = false;
                        // The DataGridView control hosts one editing control at a time,  
                        // and reuses the editing control whenever the cell type does not 
                        // change between edits. 
                        // When attaching event-handlers to the editing control, I must
                        // therefore take precautions to avoid attaching the same handler 
                        // multiple times. To avoid this problem, I remove the handler 
                        // from the event before you attach the handler to the event. 
                        // This will prevent duplication if the handler is already attached 
                        // to the event, but will have no effect otherwise. 
                        comboBox.SelectionChangeCommitted -= new EventHandler(EditingComboBox_SelectionChangeCommitted);
                        //comboBox.DropDownClosed -= new EventHandler(EditingComboBox_DropDownClosed);
                        
                        // Add Event
                        comboBox.SelectionChangeCommitted += new EventHandler(EditingComboBox_SelectionChangeCommitted);
                        //comboBox.DropDownClosed += new EventHandler(EditingComboBox_DropDownClosed);
                    }
                }

            }

            private object lastNotNullValue = DBNull.Value;
            //---------------------------------------------------------------------------------------------------------------------------
            private void UpdateRelatedDataGridViewLengthColumn()
            {
                if
                    (
                    this.DataGridView == null ||
                    relatedDataGridViewLengthColumn == null ||
                    relatedDataGridViewLengthColumn.DataGridView == null ||
                    relatedDataGridViewLengthColumn.DataGridView.IsDisposed
                    )
                    return;

                DataGridViewComboBoxEditingControl comboBox = this.DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
                if (comboBox == null || String.IsNullOrEmpty(comboBox.Text.Trim()) || comboBox.FindStringExact(comboBox.Text) == -1)
                    return;

                WizardTableColumnDataType.DataType selectedDataType = WizardTableColumnDataType.GetDataTypeFromDescription(comboBox.Text.Trim());
                if (WizardTableColumnDataType.IsTextualDataType(selectedDataType))
                {
                    if
                        (
                        relatedDataGridViewLengthColumn.DataGridView.Rows[this.RowIndex].Cells[relatedDataGridViewLengthColumn.Name].Value == DBNull.Value &&
                        lastNotNullValue != null &&
                        lastNotNullValue != DBNull.Value
                        )// Ripristino l'ultimo valore immesso non nullo
                        relatedDataGridViewLengthColumn.DataGridView.Rows[this.RowIndex].Cells[relatedDataGridViewLengthColumn.Name].Value = lastNotNullValue;
                    relatedDataGridViewLengthColumn.DataGridView.Rows[this.RowIndex].Cells[relatedDataGridViewLengthColumn.Name].ReadOnly = false;
                }
                else
                {
                    // Salvo l'ultimo valore immesso non nullo
                    if (relatedDataGridViewLengthColumn.DataGridView.Rows[this.RowIndex].Cells[relatedDataGridViewLengthColumn.Name].Value != DBNull.Value)
                    {
                        lastNotNullValue = relatedDataGridViewLengthColumn.DataGridView.Rows[this.RowIndex].Cells[relatedDataGridViewLengthColumn.Name].Value;
                        relatedDataGridViewLengthColumn.DataGridView.Rows[this.RowIndex].Cells[relatedDataGridViewLengthColumn.Name].Value = DBNull.Value;
                    }
                    relatedDataGridViewLengthColumn.DataGridView.Rows[this.RowIndex].Cells[relatedDataGridViewLengthColumn.Name].ReadOnly = true;
                }
            }

            //---------------------------------------------------------------------------------------------------------------------------
            private void EditingComboBox_SelectionChangeCommitted(object sender, EventArgs e)
            {
                UpdateRelatedDataGridViewLengthColumn();

                this.DataGridView.NotifyCurrentCellDirty(true);
            }
            
            ////---------------------------------------------------------------------------------------------------------------------------
            //private void EditingComboBox_DropDownClosed(object sender, EventArgs e)
            //{
            //    DataGridViewComboBoxEditingControl comboBox = this.DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
            //    if (comboBox != null)
            //    {
            //        if (comboBox.Equals(sender))
            //        {
 
            //        }
            //    }
            //}
        }
    }
}
