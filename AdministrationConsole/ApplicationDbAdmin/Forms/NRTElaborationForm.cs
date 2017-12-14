using System.Threading;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;

namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
	/// <summary>
	/// Summary description for NRTElaborationForm.
	/// </summary>
	//==================================================================================================
	public partial class NRTElaborationForm : System.Windows.Forms.Form
	{
		private RegressionTestEngine caller;
	
		public enum Step {Unzip, RestoreDb, CheckDb, UpdateDb, ExtraUpdateDb, BackupDb, ExportDb, Zip};
		public bool byStep = false;

		//---------------------------------------------------------------------
		public NRTElaborationForm(RegressionTestEngine rte)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			caller = rte;
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		//---------------------------------------------------------------------
		private void CMDExit_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		//---------------------------------------------------------------------
		public void SetUnit(string unitName)
		{
            AddTextToLog(string.Format("Unit: {0}\r\n", unitName));
		}

		//---------------------------------------------------------------------
		public void SetDataSet(string dataSetName)
		{
			AddTextToLog(string.Format("    DataSet: {0}\r\n", dataSetName));
		}

		//---------------------------------------------------------------------
		public void SetStartOperation(Step step)
		{
			switch (step)
			{
				case Step.Unzip:
					AddTextToLog("        1. Unzip del file");
					break;
				case Step.RestoreDb:
					AddTextToLog("        2. Restore del database");
					break;
				case Step.CheckDb:
                    AddTextToLog("        3. Check del database");
					break;
				case Step.UpdateDb:
					AddTextToLog("        4. Update del database");
					break;
				case Step.ExtraUpdateDb:
					AddTextToLog("        5. Esecuzione dello script di Extra Update");
					break;
				case Step.BackupDb:
					AddTextToLog("        6. Backup del database");
					break;
				case Step.ExportDb:
					AddTextToLog("        7. Esportazione in xml");
					break;
				case Step.Zip:
					AddTextToLog("        8. Zip del file");
					break;
			}

			if (byStep)
				MessageBox.Show("Waiting for user confirmation.");
		}

		//---------------------------------------------------------------------
		public void SetEndOperation(Step step, bool result)
		{
			if (result)
				AddTextToLog(" OK\r\n");
			else
				AddTextToLog(" NO\r\n");
		}

		//---------------------------------------------------------------------
		public void StopExecution()
		{
			AddTextToLog("Elaborazione terminata!\r\n");
            SetEnabled();
            RunUpdate();
		}

		//---------------------------------------------------------------------
		private void ENTLog_TextChanged(object sender, System.EventArgs e)
		{
            SelectTextToLog();
            RunUpdate();
		}

		//---------------------------------------------------------------------
		private void Run()
		{
			Thread t = new Thread(new ThreadStart(caller.ThreadExecution));
			t.Start();
		}

		//---------------------------------------------------------------------
		private void NRTElaborationForm_Load(object sender, System.EventArgs e)
		{
			Run();
		}

		//---------------------------------------------------------------------
		private void ENTLog_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			e.Handled = true;
		}

		//---------------------------------------------------------------------
		private void CHKByStep_CheckedChanged(object sender, System.EventArgs e)
		{
			byStep = CHKByStep.Checked;
		}

		//---------------------------------------------------------------------
		private void AddTextToLog(string text)
        {
            BeginInvoke((ThreadStart)delegate
            {
                ENTLog.Text += text;
            });
        }

		//---------------------------------------------------------------------
		private void SelectTextToLog()
        {
            BeginInvoke((ThreadStart)delegate
            {
                ENTLog.Select(ENTLog.Text.Length, 0);
                ENTLog.ScrollToCaret();
            });
        }

		//---------------------------------------------------------------------
		private void SetEnabled()
        {
            BeginInvoke((ThreadStart)delegate
            {
                CMDExit.Enabled = true;
            });
        }

		//---------------------------------------------------------------------
		private void RunUpdate()
        {
            BeginInvoke((ThreadStart)delegate 
			{ 
				Update(); 
			});
        }

		//---------------------------------------------------------------------
		public void ShowDiagnostic(Diagnostic myDiagnostic)
		{
			CallShowDiagnostic(myDiagnostic);
		}

		//---------------------------------------------------------------------
		private void CallShowDiagnostic(Diagnostic myDiagnostic)
		{
			BeginInvoke((ThreadStart)delegate
			{
				DiagnosticViewer.ShowDiagnostic(myDiagnostic);
			});
		}
	}
}
