using System;
using System.Drawing;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.NotificationManager
{
	public class DraggablePanel : Panel
	{
		/// <summary>
		/// riferimento alla form contenitrice da spostare
		/// </summary>
		private Form ParentForm;

		/// <summary>
		/// titolo della Form
		/// </summary>
		private Label TitleLabel;

		/// <summary>
		/// Immagine che viene mostrata quando si passa il mouse sul pannello per mostrare che è draggabile
		/// </summary>
		private PictureBox MovePicBox;

		/// <summary>
		/// immagine di chiusura della form
		/// </summary>
		private NoBorderedButton CloseBtn;

		/// <summary>
		/// immagine di minimizzazione della form
		/// </summary>
		private NoBorderedButton MinimizeBtn;

		/// <summary>
		/// properties per spostare il tutto
		/// </summary>
		internal bool dragging = false;

		internal Point dragCursorPoint;
		internal Point dragFormPoint;

		public DraggablePanel(Form parentForm)
		{
			//this.FlowDirection = FlowDirection.LeftToRight;
			this.ParentForm = parentForm;

			TitleLabel = new Label();
			TitleLabel.Text = parentForm.Text;
			TitleLabel.AutoSize = true;
			TitleLabel.AutoEllipsis = true;

			this.Dock = DockStyle.Top;
			this.Controls.Add(TitleLabel);

			MovePicBox = new PictureBox();
			MovePicBox.Image = Properties.Resources.Cursor_Move_white_16;
			MovePicBox.Size = MovePicBox.Image.Size;
			MovePicBox.Dock = DockStyle.Right;
			MovePicBox.Visible = false;
			this.Controls.Add(MovePicBox);

			MinimizeBtn = new NoBorderedButton();
			MinimizeBtn.FlatStyle = FlatStyle.Flat;
			MinimizeBtn.FlatAppearance.BorderSize = 0;
			MinimizeBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255); //Transparent
			MinimizeBtn.Dock = DockStyle.Right;
			MinimizeBtn.Visible = true;
			this.Controls.Add(MinimizeBtn);

			CloseBtn = new NoBorderedButton();
			CloseBtn.FlatStyle = FlatStyle.Flat;
			CloseBtn.FlatAppearance.BorderSize = 0;
			CloseBtn.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255); //Transparent
			CloseBtn.Dock = DockStyle.Right;
			CloseBtn.Visible = true;
			this.Controls.Add(CloseBtn);

			// setting the height of the panel
			this.Height = MovePicBox.Height > TitleLabel.Height ? MovePicBox.Height + MovePicBox.Padding.Vertical : TitleLabel.Height + TitleLabel.Padding.Vertical;

			SetEvents();

			SetDefaultThemeColors();
		}

		private void SetEvents()
		{
			this.MouseHover += DraggablePanel_MouseHover;
			this.MouseLeave += DraggablePanel_MouseLeave;

			this.MouseDown += DraggablePanel_MouseDown;
			this.MouseMove += DraggablePanel_MouseMove;
			this.MouseUp += DraggablePanel_MouseUp;
			this.MouseEnter += DraggablePanel_MouseEnter;

			this.MovePicBox.MouseDown += DraggablePanel_MouseEnter;
			this.MovePicBox.MouseDown += DraggablePanel_MouseDown;
			this.MovePicBox.MouseMove += DraggablePanel_MouseMove;
			this.MovePicBox.MouseUp += DraggablePanel_MouseUp;
			this.MovePicBox.MouseHover += DraggablePanel_MouseHover;
			this.MovePicBox.MouseLeave += DraggablePanel_MouseLeave;

			this.TitleLabel.MouseDown += DraggablePanel_MouseEnter;
			this.TitleLabel.MouseDown += DraggablePanel_MouseDown;
			this.TitleLabel.MouseMove += DraggablePanel_MouseMove;
			this.TitleLabel.MouseUp += DraggablePanel_MouseUp;
			this.TitleLabel.MouseHover += DraggablePanel_MouseHover;
			this.TitleLabel.MouseLeave += DraggablePanel_MouseLeave;

			this.CloseBtn.MouseDown += DraggablePanel_MouseEnter;
			this.CloseBtn.MouseDown += DraggablePanel_MouseDown;
			this.CloseBtn.MouseMove += DraggablePanel_MouseMove;
			this.CloseBtn.MouseUp += DraggablePanel_MouseUp;
			this.CloseBtn.MouseHover += DraggablePanel_MouseHover;
			this.CloseBtn.MouseLeave += DraggablePanel_MouseLeave;
			this.CloseBtn.Click += (sender, args) => { this.ParentForm.Close(); };

			this.MinimizeBtn.MouseDown += DraggablePanel_MouseEnter;
			this.MinimizeBtn.MouseDown += DraggablePanel_MouseDown;
			this.MinimizeBtn.MouseMove += DraggablePanel_MouseMove;
			this.MinimizeBtn.MouseUp += DraggablePanel_MouseUp;
			this.MinimizeBtn.MouseHover += DraggablePanel_MouseHover;
			this.MinimizeBtn.MouseLeave += DraggablePanel_MouseLeave;
			this.MinimizeBtn.Click += (sender, args) => { this.ParentForm.WindowState = FormWindowState.Minimized; };

			this.ClientSizeChanged += DraggablePanel_ClientSizeChanged;
		}

		/// <summary>
		/// aliinea lo stile del traylet a quello usato nel nuovo menu
		/// </summary>
		private void SetDefaultThemeColors()
		{
			MainColorsTheme colors = NotificationManagerUtility.GetMainColorsTheme();

			this.BackColor = colors.Primary;
			TitleLabel.ForeColor = colors.Text;

			CloseBtn.FlatAppearance.MouseDownBackColor = colors.Hover;
			CloseBtn.FlatAppearance.MouseOverBackColor = colors.Hover;

			MinimizeBtn.FlatAppearance.MouseDownBackColor = colors.Hover;
			MinimizeBtn.FlatAppearance.MouseOverBackColor = colors.Hover;

			if(colors.IsBackgroundDark())
			{
				CloseBtn.Image = Properties.Resources.CloseWindows_w;
				MinimizeBtn.Image = Properties.Resources.Minimize_w;
			}
			else
			{
				CloseBtn.Image = Properties.Resources.CloseWindows_blu;
				MinimizeBtn.Image = Properties.Resources.Minimize_blu;
			}

			CloseBtn.Size = CloseBtn.Image.Size;
			MinimizeBtn.Size = MinimizeBtn.Image.Size;
		}

		private void DraggablePanel_ClientSizeChanged(object sender, EventArgs e)
		{
			TitleLabel.MaximumSize = new Size((sender as Control).ClientSize.Width - TitleLabel.Left - MovePicBox.Width, 10000);
		}

		private void DraggablePanel_MouseLeave(object sender, EventArgs e)
		{
			MovePicBox.Visible = false;
		}

		private void DraggablePanel_MouseEnter(object sender, EventArgs e)
		{
			MovePicBox.Visible = true;
		}

		private void DraggablePanel_MouseHover(object sender, EventArgs e)
		{
			MovePicBox.Visible = true;
		}

		private void DraggablePanel_MouseDown(object sender, MouseEventArgs e)
		{
			this.dragging = true;
			this.dragCursorPoint = Cursor.Position;
			this.dragFormPoint = ParentForm.Location;
		}

		private void DraggablePanel_MouseMove(object sender, MouseEventArgs e)
		{
			if(this.dragging)
			{
				Point dif = Point.Subtract(Cursor.Position, new Size(this.dragCursorPoint));
				ParentForm.Location = Point.Add(this.dragFormPoint, new Size(dif));
			}
		}

		private void DraggablePanel_MouseUp(object sender, MouseEventArgs e)
		{
			this.dragging = false;
		}
	}

	public class NoBorderedButton : Button
	{
		public override void NotifyDefault(bool value)
		{
			base.NotifyDefault(false);
		}
	}
}