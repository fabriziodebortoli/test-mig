using System.Data;

using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;
namespace Microarea.Console.Core.DataManager
{
	/// <summary>
	/// Summary description for ShowDataSetForm.
	/// </summary>
	//=========================================================================
	public partial class ShowDataSetForm : System.Windows.Forms.Form
	{
		// richiamato dalle ExportSelections
		//---------------------------------------------------------------------------
		public ShowDataSetForm(DataTable dTable)
		{
			InitializeComponent();
			TableValuesDataGrid.CaptionText = dTable.TableName;
		}

		// richiamato dalle ImportSelections
		//---------------------------------------------------------------------------
		public ShowDataSetForm(DataSet dataSet, string table)
		{
			InitializeComponent();
			TableValuesDataGrid.CaptionText = table;
		}

		//---------------------------------------------------------------------------
		public bool SetDataGridBinding(object obj, string table)
		{
			try
			{
				TableValuesDataGrid.SetDataBinding(obj, table);
			}
			catch (System.ArgumentException)
			{
				DiagnosticViewer.ShowErrorTrace(DataManagerEngineStrings.UnknownDBObject, string.Empty, string.Empty);
				return false;
			}

			return true;
		}
	}
}
