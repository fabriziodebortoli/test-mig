using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	///<summary>
	/// An Easy Collapsible panel
	/// To group two or more controls like this, insert them in a FlowLayoutPanel.
	/// NOTA BENE: di default i panel EasyCollapsiblePanel nascono di un'altezza di 150 px
	/// Per fare in modo di avere un panel di altezza piu' ampia e' necessario impostare la property Collapse = false
	/// e poi metterla a true nel costruttore dopo l'InitializeComponent
	/// Per evitare il flickering impostare anche la proprieta' UseAnimation=false (ovvero aumentare l'altezza nel panel di basso livello)
	///</summary>
	///<remarks>
	/// If the panels have different height and you want have all panels collapsed, the size will be modified during
	/// runtime and will become the same (150 pixel)
	/// To avoid this behaviour leave the panels expanded and set Collapse property programmatically after InitializeComponent
	///</remarks>
	//================================================================================
	[DefaultProperty("HeaderText")]
	public partial class EasyCollapsiblePanel : Panel
	{
		private bool collapse = false;
		private int originalHeight = 0;
		private bool useAnimation;
		private bool showHeaderSeparator = true;
		private bool roundedCorners;
		
		private int headerCornersRadius = 10;
		private bool headerTextAutoEllipsis;
		private string headerText;
		private Color headerTextColor;
		private Color headerStartBackColor;
		private Color headerEndBackColor;
		private Image headerImage;
		private Font headerFont;
		private bool headerMouseClickEnable = true;

		private RectangleF toolTipRectangle = new RectangleF();
		private bool useToolTip = false;

		//--------------------------------------------------------------------------------
		[Description("Occurs when the control is clicked to collapse/expand the panel")]
		public event EventHandler CollapseClick;

		#region "Public Properties"
		//--------------------------------------------------------------------------------
		[Browsable(false)]
		public new Color BackColor { get { return Color.Transparent; } set { base.BackColor = Color.Transparent; } }

		//--------------------------------------------------------------------------------
		[DefaultValue(false)]
		[Description("Collapses the control when set to true")]
		[Category("CollapsiblePanel")]
		public bool Collapse
		{
			get { return collapse; }
			set
			{
				// If using animation make sure to ignore requests for collapse or expand while a previous
				// operation is in progress.
				if (useAnimation)
				{
					// An operation is already in progress.
					if (timerAnimation.Enabled)
						return;
				}
				collapse = value;
				CollapseOrExpand();
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[DefaultValue(50)]
		[Category("CollapsiblePanel")]
		[Description("Specifies the speed (in ms) of Expand/Collapse operation when using animation. UseAnimation property must be set to true.")]
		public int AnimationInterval
		{
			get { return timerAnimation.Interval; }
			set
			{
				// Update animation interval only during idle times.
				if (!timerAnimation.Enabled)
					timerAnimation.Interval = value;
			}
		}

		//--------------------------------------------------------------------------------
		[DefaultValue(false)]
		[Category("CollapsiblePanel")]
		[Description("Indicate if the panel uses amination during Expand/Collapse operation")]
		public bool UseAnimation { get { return useAnimation; } set { useAnimation = value; } }

		//--------------------------------------------------------------------------------
		[DefaultValue(true)]
		[Category("CollapsiblePanel")]
		[Description("When set to true draws panel borders, and shows a line separating the panel's header from the rest of the control")]
		public bool ShowHeaderSeparator
		{
			get { return showHeaderSeparator; }
			set
			{
				showHeaderSeparator = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[DefaultValue(false)]
		[Category("CollapsiblePanel")]
		[Description("When set to true, draws a panel with rounded top corners, the radius can bet set through HeaderCornersRadius property")]
		public bool RoundedCorners
		{
			get { return roundedCorners; }
			set
			{
				roundedCorners = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[DefaultValue(10)]
		[Category("CollapsiblePanel")]
		[Description("Top corners radius, it should be in [1, 15] range")]
		public int HeaderCornersRadius
		{
			get { return headerCornersRadius; }
			set
			{
				if (value < 1 || value > 15)
					throw new ArgumentOutOfRangeException("HeaderCornersRadius", value, "Value should be in range [1, 90]");
				else
				{
					headerCornersRadius = value;
					Refresh();
				}
			}
		}

		//--------------------------------------------------------------------------------
		[DefaultValue(false)]
		[Category("CollapsiblePanel")]
		[Description("Enables the automatic handling of text that extends beyond the width of the label control.")]
		public bool HeaderTextAutoEllipsis
		{
			get { return headerTextAutoEllipsis; }
			set
			{
				headerTextAutoEllipsis = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[Category("CollapsiblePanel")]
		[Description("Text to show in panel's header")]
		[Localizable(true)]
		public string HeaderText
		{
			get { return headerText; }
			set
			{
				headerText = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[Category("CollapsiblePanel")]
		[Description("Color of text header, and panel's borders when ShowHeaderSeparator is set to true")]
		public Color HeaderTextColor
		{
			get { return headerTextColor; }
			set
			{
				headerTextColor = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[Category("CollapsiblePanel")]
		[Description("Starting color gradient for back color of header")]
		public Color HeaderStartBackColor
		{
			get { return headerStartBackColor; }
			set
			{
				headerStartBackColor = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[Category("CollapsiblePanel")]
		[Description("Ending color gradient for back color of header")]
		public Color HeaderEndBackColor
		{
			get { return headerEndBackColor; }
			set
			{
				headerEndBackColor = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[Category("CollapsiblePanel")]
		[Description("Image that will be displayed in the top left corner of the panel")]
		public Image HeaderImage
		{
			get { return headerImage; }
			set
			{
				headerImage = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[Category("CollapsiblePanel")]
		[Description("The font used to display text in the panel's header.")]
		public Font HeaderFont
		{
			get { return headerFont; }
			set
			{
				headerFont = value;
				Refresh();
			}
		}

		//--------------------------------------------------------------------------------
		[DefaultValue(true)]
		[Category("CollapsiblePanel")]
		[Description("Enable MouseClick event of the panel's header for collapse/expand operation.")]
		public bool HeaderMouseClickEnable { get { return headerMouseClickEnable; }	set { headerMouseClickEnable = value; } }
		#endregion

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public EasyCollapsiblePanel()
		{
			InitializeComponent();

			// add images in ImageList
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Panels.Images.collapse.gif");
			if (imageStream != null)
				PanelImageList.Images.Add(Image.FromStream(imageStream));
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Panels.Images.collapse_hightlight.gif");
			if (imageStream != null)
				PanelImageList.Images.Add(Image.FromStream(imageStream));
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Panels.Images.expand.gif");
			if (imageStream != null)
				PanelImageList.Images.Add(Image.FromStream(imageStream));
			imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.WinControls.Panels.Images.expand_highlight.gif");
			if (imageStream != null)
				PanelImageList.Images.Add(Image.FromStream(imageStream));
			// add default image to picture box
			pictureBoxExpandCollapse.Image = PanelImageList.Images[0];

			this.pnlHeader.Width = this.Width - 1;

			headerFont = new Font(Font, FontStyle.Bold);
			headerTextColor = Color.Black;
			headerStartBackColor = Color.RoyalBlue;
			headerEndBackColor = Color.Snow;

			// if property is true I add the event
			if (headerMouseClickEnable)
				pnlHeader.MouseClick += new MouseEventHandler(pnlHeader_MouseClick);
		}

		//--------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			DrawHeaderPanel(e);
		}

		//--------------------------------------------------------------------------------
		public void DrawHeaderCorners(Graphics g, Brush brush, float x, float y, float width, float height, float radius)
		{
			GraphicsPath gp = new GraphicsPath();

			gp.AddLine(x + radius, y, x + width - (radius * 2), y); // Line
			gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90); // Corner
			gp.AddLine(x + width, y + radius, x + width, y + height); // Line
			gp.AddLine(x + width, y + height, x, y + height); // Line
			gp.AddLine(x, y + height, x, y + radius); // Line
			gp.AddArc(x, y, radius * 2, radius * 2, 180, 90); // Corner
			gp.CloseFigure();
			g.FillPath(brush, gp);
			if (showHeaderSeparator)
				g.DrawPath(new Pen(headerTextColor), gp);
			gp.Dispose();
		}

		//--------------------------------------------------------------------------------
		private void DrawHeaderPanel(PaintEventArgs e)
		{
			Rectangle headerRect = pnlHeader.ClientRectangle;
			if (headerRect.Height == 0 || headerRect.Width == 0)
				return;
			LinearGradientBrush headerBrush = 
				new LinearGradientBrush(headerRect, headerEndBackColor, headerStartBackColor, LinearGradientMode.Horizontal);

			if (!roundedCorners)
			{
				e.Graphics.FillRectangle(headerBrush, headerRect);
				if (showHeaderSeparator)
					e.Graphics.DrawRectangle(new Pen(headerTextColor), headerRect);
			}
			else
				DrawHeaderCorners
					(
					e.Graphics,
					headerBrush,
					headerRect.X,
					headerRect.Y,
					headerRect.Width,
					headerRect.Height,
					headerCornersRadius
					);

			// Draw header separator
			if (showHeaderSeparator)
			{
				Point start = new Point(pnlHeader.Location.X, pnlHeader.Location.Y + pnlHeader.Height);
				Point end = new Point(pnlHeader.Location.X + pnlHeader.Width, pnlHeader.Location.Y + pnlHeader.Height);
				e.Graphics.DrawLine(new Pen(headerTextColor, 2), start, end);
				// Draw rectangle lines for the rest of the control.
				Rectangle bodyRect = this.ClientRectangle;
				bodyRect.Y += this.pnlHeader.Height;
				bodyRect.Height -= (this.pnlHeader.Height + 1);
				bodyRect.Width -= 1;
				e.Graphics.DrawRectangle(new Pen(headerTextColor), bodyRect);
			}

			int headerRectHeight = pnlHeader.Height;
			// Draw header image.
			pictureBoxImage.Image = (headerImage != null) ? headerImage : null;
			pictureBoxImage.Visible = (headerImage != null) ? true : false;

			// Calculate header string position.
			if (!String.IsNullOrEmpty(headerText))
			{
				useToolTip = false;
				int delta = pictureBoxExpandCollapse.Width + 5;
				int offset = 0;
				if (headerImage != null)
					offset = headerRectHeight;

				PointF headerTextPosition = new PointF();
				Size headerTextSize = TextRenderer.MeasureText(headerText, headerFont);
				if (headerTextAutoEllipsis)
				{
					if (headerTextSize.Width >= headerRect.Width - (delta + offset))
					{
						RectangleF rectLayout =
							new RectangleF((float)headerRect.X + offset,
							(float)(headerRect.Height - headerTextSize.Height) / 2,
							(float)headerRect.Width - delta,
							(float)headerTextSize.Height);

						StringFormat format = new StringFormat();
						format.Trimming = StringTrimming.EllipsisWord;
						e.Graphics.DrawString(headerText, headerFont, new SolidBrush(headerTextColor), rectLayout, format);

						toolTipRectangle = rectLayout;
						useToolTip = true;
					}
					else
					{
						headerTextPosition.X = (offset + headerRect.Width - headerTextSize.Width) / 2;
						headerTextPosition.Y = (headerRect.Height - headerTextSize.Height) / 2;
						e.Graphics.DrawString(headerText, headerFont, new SolidBrush(headerTextColor), headerTextPosition);
					}
				}
				else
				{
					headerTextPosition.X = (offset + headerRect.Width - headerTextSize.Width) / 2;
					headerTextPosition.Y = (headerRect.Height - headerTextSize.Height) / 2;
					e.Graphics.DrawString(headerText, headerFont, new SolidBrush(headerTextColor), headerTextPosition);
				}
			}
		}

		///<summary>
		/// Event raised when the user click on header panel to collapse/expand panel
		///</summary>
		//--------------------------------------------------------------------------------
		protected void OnCollapseClick(EventArgs e)
		{
			if (CollapseClick != null)
				CollapseClick(this, e);
		}

		//--------------------------------------------------------------------------------
		private void CollapseOrExpand()
		{
			if (!useAnimation)
			{
				if (collapse)
				{
					originalHeight = this.Height;
					this.Height = pnlHeader.Height + 3;
					pictureBoxExpandCollapse.Image = PanelImageList.Images[2];
				}
				else
				{
					this.Height = originalHeight;
					pictureBoxExpandCollapse.Image = PanelImageList.Images[0];
				}
			}
			else
			{
				// Keep original height only in case of a collapse operation.
				if (collapse)
					originalHeight = this.Height;

				timerAnimation.Enabled = true;
				timerAnimation.Start();
			}
		}

		///<summary>
		/// Click on pictureBox
		///</summary>
		//--------------------------------------------------------------------------------
		private void pictureBoxExpandCollapse_Click(object sender, EventArgs e)
		{
			CollapseOnClickEvent(e);
		}

		//--------------------------------------------------------------------------------
		private void pictureBoxExpandCollapse_MouseMove(object sender, MouseEventArgs e)
		{
			if (!timerAnimation.Enabled)
				pictureBoxExpandCollapse.Image = collapse ? PanelImageList.Images[3] : PanelImageList.Images[1];
		}

		//--------------------------------------------------------------------------------
		private void pictureBoxExpandCollapse_MouseLeave(object sender, EventArgs e)
		{
			if (!timerAnimation.Enabled)
				pictureBoxExpandCollapse.Image = collapse ? PanelImageList.Images[2] : PanelImageList.Images[0];
		}

		//--------------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			this.pnlHeader.Width = this.Width - 1;
			Refresh();
		}

		//--------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			this.pnlHeader.Width = this.Width - 1;
			Refresh();
		}

		//--------------------------------------------------------------------------------
		private void pnlHeader_MouseHover(object sender, EventArgs e)
		{
			if (useToolTip)
			{
				Point p = this.PointToClient(Control.MousePosition);
				if (toolTipRectangle.Contains(p))
					toolTip.Show(headerText, pnlHeader, p);
			}
		}

		//--------------------------------------------------------------------------------
		private void pnlHeader_MouseLeave(object sender, EventArgs e)
		{
			if (useToolTip)
			{
				Point p = this.PointToClient(Control.MousePosition);
				if (!toolTipRectangle.Contains(p))
					toolTip.Hide(pnlHeader);
			}
		}

		///<summary>
		/// MouseClick on header panel
		///</summary>
		//--------------------------------------------------------------------------------
		private void pnlHeader_MouseClick(object sender, MouseEventArgs e)
		{
			CollapseOnClickEvent(e);
		}

		///<summary>
		/// Executes collapse/expand operation, then raise the CollapseClick event outside
		///</summary>
		//--------------------------------------------------------------------------------
		private void CollapseOnClickEvent(EventArgs e)
		{
			Collapse = !Collapse;
			this.OnCollapseClick(e);
		}

		//--------------------------------------------------------------------------------
		private void timerAnimation_Tick(object sender, EventArgs e)
		{
			if (collapse)
			{
				if (this.Height <= pnlHeader.Height + 3)
				{
					timerAnimation.Stop();
					timerAnimation.Enabled = false;
					pictureBoxExpandCollapse.Image = PanelImageList.Images[2];
				}
				else
				{
					int newHeight = this.Height - 20;
					if (newHeight <= pnlHeader.Height + 3)
						newHeight = pnlHeader.Height + 3;
					this.Height = newHeight;
				}
			}
			else
			{
				if (this.Height >= originalHeight)
				{
					timerAnimation.Stop();
					timerAnimation.Enabled = false;
					pictureBoxExpandCollapse.Image = PanelImageList.Images[0];
				}
				else
				{
					int newHeight = this.Height + 20;
					if (newHeight >= originalHeight)
						newHeight = originalHeight;
					this.Height = newHeight;
				}
			}
		}
	}
}