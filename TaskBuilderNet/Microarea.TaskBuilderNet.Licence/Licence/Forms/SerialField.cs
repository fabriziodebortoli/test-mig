
namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{
	/// <summary>
	/// Controllo derivato da SerialTextBoxes, per inserirlo in una ColumnStyle
	/// </summary>
	//=========================================================================
	public class SerialField : SerialTextBoxes
	{
		public bool isInEditOrNavigateMode = true;
		
 
		#region INITIALIZECOMPONENT
		//---------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SerialField));
			this.SuspendLayout();
			// 
			// SerialField
			// 
			resources.ApplyResources(this, "$this");
			this.Name = "SerialField";
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion
	}
}
