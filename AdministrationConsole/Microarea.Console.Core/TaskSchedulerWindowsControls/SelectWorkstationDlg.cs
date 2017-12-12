using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for SelectWorkstationDlg.
	/// </summary>
	public partial class SelectWorkstationDlg : System.Windows.Forms.Form
	{
		private string[] alreadySelectedWorkstations = null;

		public SelectWorkstationDlg(string aSelectedWorkstationsList)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			if (aSelectedWorkstationsList != null && aSelectedWorkstationsList != String.Empty)
				alreadySelectedWorkstations = aSelectedWorkstationsList.Split(';');
		}

		#region SelectWorkstationDlg event handlers

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			base.OnLoad(e);

			CenterToParent();

			ArrayList workstations;
			if (WTENetSendMessage.EnumWorkstations(out workstations) <= 0 || workstations == null)
			{
				this.Close();
				MessageBox.Show(TaskSchedulerWindowsControlsStrings.NoWorkstationFoundMsg, TaskSchedulerWindowsControlsStrings.NoWorkstationFoundCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				return;
			}

			foreach(string workstationName in workstations)
			{
				CheckState workstationCheckState = CheckState.Unchecked;
				if (alreadySelectedWorkstations != null && alreadySelectedWorkstations.Length > 0)
				{
					foreach(string alreadySelectedWorkstationName in alreadySelectedWorkstations)
					{
						if (String.Compare(alreadySelectedWorkstationName, workstationName, true, CultureInfo.InvariantCulture) == 0)
						{
							workstationCheckState = CheckState.Checked;
							break;
						}
					}
				}
				WorkStationsListBox.Items.Add(workstationName, workstationCheckState);
			}

		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string SelectedWorkstations
		{
			get
			{
				if (WorkStationsListBox.CheckedItems == null || WorkStationsListBox.CheckedItems.Count == 0)
					return String.Empty;

				string selWorkstations = String.Empty;
				foreach(string workstationName in WorkStationsListBox.CheckedItems)
				{
					if (selWorkstations.Length > 0)
						selWorkstations += ';';
					selWorkstations += workstationName;
				}
				return selWorkstations;
			}
		}
		
		#endregion
	}

}
