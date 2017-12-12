using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;

namespace Microarea.EasyAttachment.UI.Controls
{
	//================================================================================
	public partial class SlideShowPanel : UserControl
	{
		public static int standardWidth = 63;
		public static int standardHeight = 89;

		public int Index = -1;
		public bool Selected = false;

		private AttachmentInfo attachment;

		Size originalSize = new Size(65, 91);
		Color originalColor = Color.Lavender;
		int offset = 5;
		Control child = null;
		int id = -1;

		//--------------------------------------------------------------------------------
		public AttachmentInfo AttachInfo
		{
			get { return attachment; }
			set
			{
				attachment = value;
				id = attachment == null ? -1 : attachment.AttachmentId;
				/*LblDescri.Text = attachment.OriginalName; */
			}
		}

		//--------------------------------------------------------------------------------
		public int Id { get { return id; } }

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public SlideShowPanel()
		{
			InitializeComponent();
			Size = originalSize;
			BackgroundImageLayout = ImageLayout.Center;
			BorderStyle = BorderStyle.None;
			originalColor = BackColor;
		}

		//--------------------------------------------------------------------------------
		public void AddControl(Control c)
		{
			Controls.Clear();
			c.Size = new System.Drawing.Size(standardWidth, standardHeight);
			c.Dock = DockStyle.Fill;
			c.Location = new Point(1, 5);
			Controls.Add(c);
			child = c;

			c.DoubleClick += new EventHandler(control_DoubleClick);
			c.Click += new EventHandler(c_Click);
			c.MouseHover += new EventHandler(control_MouseHover);
		}

		//--------------------------------------------------------------------------------
		void c_Click(object sender, EventArgs e)
		{
			Control c = sender as Control;
			if (c != null)
				c.Focus();

			OnClick(e);
		}

		//--------------------------------------------------------------------------------
		void control_MouseHover(object sender, EventArgs e)
		{
			OnMouseHover(e);
		}

		//--------------------------------------------------------------------------------
		void control_DoubleClick(object sender, EventArgs e)
		{
			OnDoubleClick(e);
		}

		//--------------------------------------------------------------------------------
		void control_MouseClick(object sender, MouseEventArgs e)
		{
			OnMouseClick(e);
		}

		//--------------------------------------------------------------------------------
		public void ToggleSelection(bool select)
		{
			if (select && !Selected)
			{
				BackColor = Color.Lavender;
				if (child != null) child.BackColor = Color.Lavender;
				Size = new System.Drawing.Size(Size.Width + 2 * offset, Size.Height + 2 * offset);
				Location = new Point(Location.X - offset, Location.Y - offset);
				BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
				Selected = true;
			}
			else
				if (!select && Selected)
				{
					Size = originalSize;
					Location = new Point(Location.X + offset, Location.Y + offset);
					if (child != null) child.BackColor = originalColor;
					BackColor = originalColor;
					BorderStyle = System.Windows.Forms.BorderStyle.None;
					Selected = false;
				}
		}
	}
}