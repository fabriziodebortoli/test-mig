using System.Drawing;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.ServicesAdmin
{


	public partial class EditListView : ListView 
	{
		private CustomEditListViewItem li;
		private int	X=0;
		private int	Y=0;
		private string subItemText;
		private int	subItemSelected = 0 ; 		
		//---------------------------------------------------------------------
		public EditListView()
		{
			
			InitializeComponent();
			this.Font			= new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			
			chkBox.Size			= new System.Drawing.Size(0,0);
			chkBox.Location		= new System.Drawing.Point(0,0);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {this.chkBox});	
			chkBox.LostFocus	+= new System.EventHandler(this.FocusOverChk);
			chkBox.Font			= new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			chkBox.CheckAlign	= ContentAlignment.MiddleCenter;
			chkBox.Hide();
			chkBox.Text			= "";
			this.Controls.AddRange(new System.Windows.Forms.Control[] {this.editBox});			
			chkBox.MouseLeave	+= new System.EventHandler(chkBox_MouseLeave);
			
			editBox.Size		= new System.Drawing.Size(0,0);
			editBox.Location	= new System.Drawing.Point(0,0);
			editBox.KeyPress	+= new System.Windows.Forms.KeyPressEventHandler(this.EditOver);
			editBox.MouseLeave	+= new System.EventHandler(editBox_MouseLeave);			
			editBox.LostFocus	+= new System.EventHandler(this.FocusOver);
			editBox.Font		= new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			editBox.BackColor	= Color.LightYellow; 
			editBox.BorderStyle = BorderStyle.Fixed3D;
			editBox.Hide();
			editBox.Text = "";	
		}
		//----------------------------------------------------------------------	
		private void EditOver(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if ( e.KeyChar == 13 ) 
			{
				li.SubItems[subItemSelected].Text = editBox.Text;
				editBox.Hide();
			}

			if ( e.KeyChar == 27 ) 
				editBox.Hide();
		}
		//---------------------------------------------------------------------
		private void FocusOver(object sender, System.EventArgs e)
		{
			li.SubItems[subItemSelected].Text = editBox.Text;
			editBox.Hide();
		}
		//---------------------------------------------------------------------
		public  void EditListViewDoubleClick(object sender, System.EventArgs e)
		{	

			int widthCustomCell = 0;
			int nStart			= X ;
			int spos			= 0 ; 
			int epos			= this.Columns[0].Width ;
			
			for ( int i=0; i < this.Columns.Count ; i++)
			{
				if ( nStart > spos && nStart < epos ) 
				{
					subItemSelected = i ;
					break; 
				}
				spos	= epos ; 
				epos	+= this.Columns[i].Width;
			}

			if (subItemSelected ==0 ) return;
	
			subItemText		= li.SubItems[subItemSelected].Text ;
			widthCustomCell = this.Columns[subItemSelected].Width;

			if ( li.Type == ConstString.boolType ) 
			{
				Rectangle r		= new Rectangle(spos , li.Bounds.Y , widthCustomCell , li.Bounds.Bottom);
				chkBox.Size		= new System.Drawing.Size(widthCustomCell , li.Bounds.Bottom-li.Bounds.Top);
				chkBox.Location = new System.Drawing.Point(spos , li.Bounds.Y);
				chkBox.Show() ;

				if (subItemText == "1")
					chkBox.Checked = true;
				else
					chkBox.Checked = false;
				chkBox.Focus();
			}
			else
			{
				Rectangle r			= new Rectangle(spos , li.Bounds.Y , widthCustomCell , li.Bounds.Bottom);
				editBox.Size		= new System.Drawing.Size(widthCustomCell, li.Bounds.Bottom-li.Bounds.Top);
				editBox.Location	= new System.Drawing.Point(spos , li.Bounds.Y);
				editBox.Show() ;

				editBox.Enabled = true;
				editBox.Text	= subItemText;

				editBox.SelectAll();
				editBox.Focus();
			}
		}
		//---------------------------------------------------------------------
		public void EditListViewMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			li	= (CustomEditListViewItem) GetItemAt(e.X , e.Y);
			X	= e.X ;
			Y	= e.Y ;
		}
		//---------------------------------------------------------------------
		private void FocusOverChk(object sender, System.EventArgs e)
		{
			if (chkBox.Checked == true)
				li.SubItems[subItemSelected].Text = "1";
			else
				li.SubItems[subItemSelected].Text = "0";

			chkBox.Hide();
		}
		//---------------------------------------------------------------------
		private void chkBox_MouseLeave(object sender, System.EventArgs e)
		{
			chkBox.Visible = false;
		}
		//---------------------------------------------------------------------
		private void editBox_MouseLeave(object sender, System.EventArgs e)
		{
			editBox.Visible = false;
		}

		//---------------------------------------------------------------------
	}
	//=========================================================================
	public class CustomEditListViewItem : ListViewItem
	{
		private string	localize				= "";
		private string	name					= "";
		private string	type					= "";
		private bool	hidden					= false;
		private string	parameterValue			= "";
		private bool	allowAddNewParameter	= false;
		private	int		release					= 0;
		private bool	isPassword				= false;

		//Properties
		//---------------------------------------------------------------------
		public string Localize { get { return localize; } set { localize = value; } }
		//---------------------------------------------------------------------
		public string ParameterName { get { return name; } set { name = value; } }
		//---------------------------------------------------------------------
		public string Type { get { return type; } set { type = value; } }
		//---------------------------------------------------------------------
		public string ParameterValue { get { return parameterValue; } set { parameterValue = value; } }
		//---------------------------------------------------------------------
		public bool	Hidden	{ get { return hidden; } set { hidden = value; }}
		//---------------------------------------------------------------------
		public bool	AllowAddNewParameter	{ get { return allowAddNewParameter; } set { allowAddNewParameter = value; }}
		//---------------------------------------------------------------------
		public int	Release	{ get { return release; }	set { release = value; } }
		//---------------------------------------------------------------------
		public bool	IsPassword	{ get { return isPassword; }	set { isPassword = value; } }
		//---------------------------------------------------------------------
		public CustomEditListViewItem() {}
	}
	//=========================================================================
}
