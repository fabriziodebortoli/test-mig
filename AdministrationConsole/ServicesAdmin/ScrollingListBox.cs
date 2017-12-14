using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin 
{
	/// <summary>
	/// Summary description for ScrollingListBox.
	/// </summary>
	public partial class ScrollingListBox : ListView
	{
		private System.Windows.Forms.TextBox		editBox = new System.Windows.Forms.TextBox();
		private System.Windows.Forms.CheckBox		chkBox	= new System.Windows.Forms.CheckBox();

//		public event ScrollEventHandler Scrolled = null;

		private const int WM_HSCROLL = 0x114;
		private const int WM_VSCROLL = 0x115;


		public System.Windows.Forms.TextBox		EditBox { get { return editBox;}}
		public System.Windows.Forms.CheckBox	ChkBox	{ get { return chkBox;}}

		/// <summary>
		/// Trappo l'evento di OnMouseWheel (ovvero quando scorro con la rotella del Mouse)
		/// in modo da far scomparire i controlli di editing se no mi rimangono disallineati con 
		/// gli elementi della lista
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			if (!SystemInformation.MouseWheelPresent)
				return;

			editBox.Visible = false;
			chkBox.Visible  = false;

			base.OnMouseWheel(e);
			
		}

		/// <summary>
		/// Trappo l'evento di WndProc 
		/// in modo da far scomparire i controlli di editing se no mi rimangono disallineati con 
		/// gli elementi della lista, nel caso in cui io stia scollando la lista
		/// </summary>
		/// <param name="msg"></param>
		protected override void WndProc(ref System.Windows.Forms.Message msg)
		{
			if( msg.Msg == WM_HSCROLL ||  msg.Msg == WM_VSCROLL)
			{
				editBox.Visible = false;
				chkBox.Visible  = false;
			}
			base.WndProc(ref msg);
		}

		public ScrollingListBox()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.MultiSelect = false;
			this.View = View.Details;

			this.Columns.Add(Strings.Parameter,	-2,HorizontalAlignment.Left );
			this.Columns.Add(Strings.Value,		-2,HorizontalAlignment.Left );

		}
	}
}
