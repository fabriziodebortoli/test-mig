
namespace Microarea.EasyAttachment.Components
{
	///<summary>
	/// Tipo generico utilizzato come tipo base per passare le informazioni in caso
	/// di Drag&Drop dei control da una form di Mago.
	///</summary>
	//================================================================================
	public class DragDropData
	{
		private ParsedControlWrapper parsedControlWrapper;

		//--------------------------------------------------------------------------------
		public ParsedControlWrapper Data
		{
			get { return parsedControlWrapper; }
			set { parsedControlWrapper = value; }
		}

		//--------------------------------------------------------------------------------
		public DragDropData(ParsedControlWrapper parsedControlWrapper)
		{
			this.parsedControlWrapper = parsedControlWrapper;
		}
	}
}
