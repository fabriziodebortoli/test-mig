using System.Windows.Forms;

using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.DiagnosticUI
{
	/// <summary>
	/// Informazioni di dettaglio del messaggio in DiagnosticView.
	/// </summary>
	//=========================================================================
	public partial class MessageDetail : Form
	{
		#region Costructors
		//---------------------------------------------------------------------
		public MessageDetail()
		{
			InitializeComponent();
		}

		//---------------------------------------------------------------------
		public MessageDetail(ParseMessageEventArgs details) : this()
		{
			DetailMessage.Items.Add(new ParseMessageEventArgs(details.MessageType, details.Time, details.MessageText));	

			DetailRichTextBox.Clear();
			//DetailRichTextBox.SelectionFont = new Font(DetailRichTextBox.Font, DetailRichTextBox.Font.Style | FontStyle.Bold);
			DetailRichTextBox.AppendText(details.ExtendedInfo.Format(LineSeparator.CrLf));
			DetailRichTextBox.Select(0, 0);
		}
		#endregion

	}
}