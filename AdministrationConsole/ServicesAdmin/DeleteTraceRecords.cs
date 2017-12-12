using System;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	/// <summary>
	/// DeleteTraceRecords
	/// </summary>
	/// =======================================================================
	public partial class DeleteTraceRecords : System.Windows.Forms.Form
	{
		public DateTime SelectedToDate { get { return selectedToDate.Value; } set { selectedToDate.Value = value; }}


		/// <summary>
		/// DeleteTraceRecords (Costruttore)
		/// </summary>
		//---------------------------------------------------------------------
		public DeleteTraceRecords()
		{
			InitializeComponent();
		}

        /// <summary>
		/// BtnOk_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		/// <summary>
		/// BtnCancel_Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
