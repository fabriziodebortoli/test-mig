
namespace Microarea.TaskBuilderNet.Core.Generic
{
	/// <summary>
	/// Sarebbe comodo avere qualcosa di analogo a WebControls.ListItem.
	/// La classe ComboBoxItem risulta utile quando in una ComboBox si
	/// devono inserire oggetti di cui si visualizza una stringa diversa
	/// da quella che poi si deve elaborare.
	/// </summary>
	//=========================================================================
	public class ComboBoxItem
	{
		public string Text;
		public string Value;

		/// <summary>
		/// Crea una nuova istanza di ComboBoxItem.
		/// </summary>
		/// <param name="aText">string da visualizzare.</param>
		/// <param name="aValue">string valore.</param>
		//---------------------------------------------------------------------
		public ComboBoxItem(string aText, string aValue)
		{
			Text = aText;
			Value = aValue;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return Text;
		}
	}
}