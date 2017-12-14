using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
	/// <summary>
	/// Summary description for WebColorPicker.
	/// </summary>
	//================================================================================
	public partial class WebColorPicker : System.Windows.Forms.UserControl
	{
		public event System.EventHandler SelectedColorChanged;

		//--------------------------------------------------------------------------------
		public WebColorPicker()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		//--------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);
		}

		//---------------------------------------------------------------------------------
		protected override void OnFontChanged(System.EventArgs e)
		{
			base.OnFontChanged(e);

			WebColorComboBox.Font = this.Font;
		}

		//---------------------------------------------------------------------------------
		protected override void OnForeColorChanged(System.EventArgs e)
		{
			base.OnForeColorChanged(e);

			WebColorComboBox.ForeColor = this.ForeColor;
		}

		//---------------------------------------------------------------------------------
		private void WebColorComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (SelectedColorChanged != null)
				SelectedColorChanged(this, System.EventArgs.Empty);
		}

		//--------------------------------------------------------------------------------
		private void SelectColorButton_Click(object sender, System.EventArgs e)
		{
			if (this.DesignMode)
			{
				this.Parent.Select();
				return;
			}

			ColorDialog selectColorDlg = new ColorDialog();

			selectColorDlg.Color = WebColorComboBox.SelectedColor;

			if (selectColorDlg.ShowDialog(this) == DialogResult.OK)
			{
				WebColorComboBox.CustomColor = selectColorDlg.Color;
				WebColorComboBox.SelectedIndex = 0;

				if (SelectedColorChanged != null)
					SelectedColorChanged(this, System.EventArgs.Empty);
			}
		}

		//---------------------------------------------------------------------------------
		public Color Color
		{
			get { return WebColorComboBox.SelectedColor; }
			set
			{
				WebColorComboBox.SelectedColor = value;

				if (SelectedColorChanged != null)
					SelectedColorChanged(this, System.EventArgs.Empty);
			}
		}

		//---------------------------------------------------------------------------------
		public Color ComboBoxBackColor
		{
			get { return WebColorComboBox.BackColor; }
			set { WebColorComboBox.BackColor = value; }
		}

		//===============================================================================
		public class WebColorSelectionButton : Control
		{
			private IContainer components = new Container();

			private Image image = null;
			private bool mouseOver = false;
			private bool mouseCapture = false;

			//---------------------------------------------------------------------------
			public WebColorSelectionButton()
			{
				Initialize();
			}

			//---------------------------------------------------------------------------
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (components != null)
						components.Dispose();
				}
				base.Dispose(disposing);
			}

			#region WebColorSelectionButton protected overridden methods

			//---------------------------------------------------------------------------
			protected override void OnEnabledChanged(EventArgs e)
			{
				base.OnEnabledChanged(e);
				if (Enabled == false)
				{
					mouseOver = false;
					mouseCapture = false;
				}
				Invalidate();
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);

				if (e.Button != MouseButtons.Left)
					return;

				if (mouseCapture == false || mouseOver == false)
				{
					mouseCapture = true;
					mouseOver = true;

					//Redraw to show button state
					Invalidate();
				}
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseUp(MouseEventArgs e)
			{
				base.OnMouseUp(e);

				if (e.Button != MouseButtons.Left)
					return;

				if (mouseOver == true || mouseCapture == true)
				{
					mouseOver = false;
					mouseCapture = false;

					// Redraw to show button state
					Invalidate();
				}

				base.OnMouseUp(e);
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseMove(MouseEventArgs e)
			{
				base.OnMouseMove(e);

				// Is mouse point inside our client rectangle
				bool over = this.ClientRectangle.Contains(new Point(e.X, e.Y));

				// If entering the button area or leaving the button area...
				if (over != mouseOver)
				{
					// Update state
					mouseOver = over;

					// Redraw to show button state
					Invalidate();
				}
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseEnter(EventArgs e)
			{
				// Update state to reflect mouse over the button area
				if (!mouseOver)
				{
					mouseOver = true;

					// Redraw to show button state
					Invalidate();
				}

				base.OnMouseEnter(e);
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseLeave(EventArgs e)
			{
				// Update state to reflect mouse not over the button area
				if (mouseOver)
				{
					mouseOver = false;

					// Redraw to show button state
					Invalidate();
				}

				base.OnMouseLeave(e);
			}

			//---------------------------------------------------------------------------
			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);

				DrawImage(e.Graphics);
				DrawBorder(e.Graphics);
			}

			#endregion

			#region WebColorSelectionButton private methods

			//---------------------------------------------------------------------------
			private void Initialize()
			{
				Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Combo.Images.WebColorPickerButton.bmp");
				if (bitmapStream != null)
				{
					System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
					if (bitmap != null)
						image = bitmap;
				}

				// Prevent drawing flicker by blitting from memory in WM_PAINT
				SetStyle
					(
					ControlStyles.ResizeRedraw |
					ControlStyles.UserPaint |
					ControlStyles.AllPaintingInWmPaint |
					ControlStyles.DoubleBuffer,
					true
					);

				// Prevent base class from trying to generate double click events and
				// so testing clicks against the double click time and rectangle. Getting
				// rid of this allows the user to press then release button very quickly.
				SetStyle(ControlStyles.StandardDoubleClick, false);

				// Should not be allowed to select this control
				SetStyle(ControlStyles.Selectable, false);
			}

			//---------------------------------------------------------------------------
			private void DrawImage(Graphics g)
			{
				if (image == null)
					return;

				Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

				if (Enabled)
				{
					// Three points provided are upper-left, upper-right and 
					// lower-left of the destination parallelogram. 
					Point[] pts = new Point[3];
					pts[0].X = (Enabled && mouseOver && mouseCapture) ? 1 : 0;
					pts[0].Y = (Enabled && mouseOver && mouseCapture) ? 1 : 0;
					pts[1].X = pts[0].X + image.Width;
					pts[1].Y = pts[0].Y;
					pts[2].X = pts[0].X;
					pts[2].Y = pts[1].Y + image.Height;

					g.DrawImage(image, pts, rect, GraphicsUnit.Pixel);
				}
				else
					ControlPaint.DrawImageDisabled(g, image, 0, 0, this.BackColor);
			}

			//---------------------------------------------------------------------------
			private void DrawBorder(Graphics g)
			{
				if (!Enabled || !mouseOver)
					return;

				Border3DStyle borderStyle = mouseCapture ? Border3DStyle.SunkenOuter : Border3DStyle.RaisedOuter;

				ControlPaint.DrawBorder3D(g, this.ClientRectangle, borderStyle);
			}
			#endregion
		}
	}
}