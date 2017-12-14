
namespace Microarea.Console.Core.PlugIns
{
	/// <summary>
	/// PlugInsForm
	/// Form da cui ereditano le Form dei PlugIns 
	/// </summary>
	//=========================================================================
	public partial class PlugInsForm : System.Windows.Forms.Form
	{
		private StateEnums state = StateEnums.View;

		public StateEnums State { get { return state; } set { state = value; } }

		//---------------------------------------------------------------------
		public PlugInsForm()
		{
			InitializeComponent();

		}
	}

	//---------------------------------------------------------------------
	public enum StateEnums { None = 0, View = 1, Editing = 2, Processing = 4, Waiting = 8 };
}