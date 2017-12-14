using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	#region MenuApplicationPanel class
	/// <summary>
	/// An extended Panel that provides collapsible panels like those provided in Windows XP.
	/// </summary>
	//============================================================================
	public partial class MenuApplicationPanel : System.Windows.Forms.Panel
	{
		private MenuXmlNode		applicationNode;
		private MenuXmlParser	menuXmlParser;
		private MenuMngWinCtrl	menuMngWinCtrl;
		private ArrayList		groupLabels;
		private GroupLinkLabel	currentGroupLabel;

		public enum PanelState	{ Undefined = -1, Expanded = 0, Collapsed = 1 }
		
		public event PanelStateChangedEventHandler PanelStateChanged;

		private int	panelHeight;
		private int imageIndex = 0;
		private int minTitleHeight = 0;
		private const int iconBorder = 2;
		private const int expandBorder = 4;
		private const int labelsXOffset = 10;
		private const int labelsYSpacing = 4;
		private const int labelsXSpacingFromPicture = 2;
		
		private System.Drawing.Color					startColor = Color.White;
		private System.Drawing.Color					endColor = Color.LightSteelBlue;
		private System.Drawing.Image					titleImage;
		private System.Drawing.Color					imagesTransparentColor = Color.White;
		private System.Drawing.Imaging.ColorMatrix		grayMatrix;
		private System.Drawing.Imaging.ImageAttributes	grayAttributes;

		private PanelState	state = PanelState.Undefined;
		
		public event MenuMngCtrlEventHandler SelectedApplicationChanged;
		public event MenuMngCtrlEventHandler SelectedGroupChanging;
		public event MenuMngCtrlEventHandler SelectedGroupChanged;
		//---------------------------------------------------------------------------
		public MenuApplicationPanel(MenuMngWinCtrl aMenuMngWinCtrl, MenuXmlNode aAppNode)
		{
			applicationNode = null;

			Debug.Assert(aMenuMngWinCtrl != null);
			menuMngWinCtrl = aMenuMngWinCtrl;

			groupLabels = null;
			currentGroupLabel = null;

			Debug.Assert(aAppNode == null || aAppNode.IsApplication);
			applicationNode = (aAppNode != null && aAppNode.IsApplication) ? aAppNode : null;

			menuXmlParser = (menuMngWinCtrl != null) ? menuMngWinCtrl.MenuXmlParser : null;
			
			Font = new System.Drawing.Font(menuMngWinCtrl.Font, System.Drawing.FontStyle.Regular);

			InitializeComponent();

			TitleLabel.Font = new System.Drawing.Font(Font.Name, Font.SizeInPoints + 1.5F, Font.FontFamily.IsStyleAvailable(System.Drawing.FontStyle.Bold) ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			
			TitleLabel.TabIndex = 0;
			TitleLabel.TabStop = true;
			
			minTitleHeight = TitleLabel.Font.Height + (2 * MenuApplicationPanel.expandBorder);
			if (TitleLabel.Height < minTitleHeight)
				TitleLabel.Height = minTitleHeight;

			if (applicationNode != null)
			{

				Title = applicationNode.Title;
								
				if (menuXmlParser != null)
				{
					string appImageFile = menuXmlParser.GetApplicationImageFileName(applicationNode.GetNameAttribute());
										
					if (appImageFile != null && appImageFile.Length > 0 && File.Exists(appImageFile))
					{
						try
						{
							

							TitleImage = ImagesHelper.LoadBitmapWithoutLockFile(appImageFile);
						}
						catch(OutOfMemoryException exception)
						{	
							// The Image.FromFile method throws an OutOfMemoryException exception if the pixel format of the
							// image specified by imageFile is not supported.
							Debug.Fail("OutOfMemoryException raised in MenuApplicationPanel Constructor: " + exception.Message);
						}
					}
				}
				else
				{
					Stream defaultImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.AddOnAppSmall.bmp");
					if (defaultImageStream != null)
						TitleImage = Image.FromStream(defaultImageStream);
				}
				if (menuMngWinCtrl != null)
					TitleLabel.ContextMenu = menuMngWinCtrl.ItemsContextMenu;
			}

			this.BackColor = Color.Lavender;

			// Setup the ColorMatrix and ImageAttributes for grayscale images.
			grayMatrix = new ColorMatrix();
			grayMatrix.Matrix00 = 1/3f;
			grayMatrix.Matrix01 = 1/3f;
			grayMatrix.Matrix02 = 1/3f;
			grayMatrix.Matrix10 = 1/3f;
			grayMatrix.Matrix11 = 1/3f;
			grayMatrix.Matrix12 = 1/3f;
			grayMatrix.Matrix20 = 1/3f;
			grayMatrix.Matrix21 = 1/3f;
			grayMatrix.Matrix22 = 1/3f;
			grayAttributes = new ImageAttributes();
			grayAttributes.SetColorMatrix(grayMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
			grayAttributes.SetColorKey(ImagesTransparentColor, ImagesTransparentColor, ColorAdjustType.Default);

			Stream collapseImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.Collapse.bmp");
			if (collapseImageStream != null)
				PanelImageList.Images.Add(Image.FromStream(collapseImageStream));
			
			Stream expandImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.Expand.bmp");
			if (expandImageStream != null)
				PanelImageList.Images.Add(Image.FromStream(expandImageStream));

			Expand();
		}

		#region MenuApplicationPanel protected overridden methods

		//---------------------------------------------------------------------------
		protected override void InitLayout()
		{
			UpdateDisplayedState();
		}

		//---------------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{	
			// Invoke base class implementation
			base.OnResize(e);

			if (IsExpanded) 
				panelHeight = Height;

			foreach (Control addedControl in this.Controls)
			{
				if (addedControl is GroupLinkLabel)
					((GroupLinkLabel)addedControl).SetWidthToFitPanel(this);
			}
		}

		//---------------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			
			if (e != null && e.Control != null && e.Control is GroupLinkLabel)
				((GroupLinkLabel)e.Control).SetWidthToFitPanel(this);
		}
				
		#endregion

		#region MenuApplicationPanel public properties
		
		//---------------------------------------------------------------------------
		public override bool AutoScroll
		{
			get { return false; }
		}

		//---------------------------------------------------------------------------
		public int PanelHeight { get { return panelHeight; } }
		
		//---------------------------------------------------------------------------
		public int TitleHeight { get { return TitleLabel.Height; } }

		/// <summary>
		/// Gets/sets the PanelState.
		/// </summary>
		//---------------------------------------------------------------------------
		public PanelState State
		{
			get
			{
				return state;
			}
			set
			{
				PanelState oldState = state;
				state = value;
				if(oldState != state)
				{
					// State has changed to update the display
					UpdateDisplayedState();
				}
			}
		}

		//---------------------------------------------------------------------------
		public bool IsCollapsed
		{
			get
			{
				return State == PanelState.Collapsed;
			}
		}
		
		//---------------------------------------------------------------------------
		public bool IsExpanded
		{
			get
			{
				return State == PanelState.Expanded;
			}
		}

		/// <summary>
		/// Gets/sets the text displayed as the panel title.
		/// </summary>
		//---------------------------------------------------------------------------
		public string Title
		{
			get
			{
				return TitleLabel.Text;
			}
			set
			{
				TitleLabel.Text = value;
			}
		}

		/// <summary>
		/// Gets/sets the foreground color used for the title bar.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color TitleFontColor
		{
			get
			{
				return TitleLabel.ForeColor;
			}
			set
			{
				TitleLabel.ForeColor = value;
			}
		}

		/// <summary>
		/// Gets/sets the font used for the title bar text.
		/// </summary>
		//---------------------------------------------------------------------------
		public Font TitleFont
		{
			get
			{
				return TitleLabel.Font;
			}
			set
			{
				TitleLabel.Font = value;
				minTitleHeight =  Math.Max(TitleLabel.Font.Height + (2 * MenuApplicationPanel.expandBorder), TitleLabel.Height);
				if(TitleLabel.Height < minTitleHeight)
				{
					TitleLabel.Height = minTitleHeight;
				}
			}
		}

		/// <summary>
		/// Gets/sets the image list used for the expand/collapse image.
		/// </summary>
		//---------------------------------------------------------------------------
		public ImageList ImageList
		{
			get
			{
				return PanelImageList;
			}
			set
			{
				imageIndex = -1;
				
				PanelImageList = value;
				if(null != PanelImageList)
				{
					if(PanelImageList.Images.Count > 0)
					{
						imageIndex = 0;
					}
				}
			}
		}

		/// <summary>
		/// Gets/sets the starting color for the background gradient of the header.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color StartColor
		{
			get
			{
				return startColor;
			}
			set
			{
				startColor = value;
				TitleLabel.Invalidate();
			}
		}

		/// <summary>
		/// Gets/sets the ending color for the background gradient of the header.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color EndColor
		{
			get
			{
				return endColor;
			}
			set
			{
				endColor = value;
				TitleLabel.Invalidate();
			}
		}

		/// <summary>
		/// Gets/sets the image displayed in the header of the title bar.
		/// </summary>
		//---------------------------------------------------------------------------
		public Image TitleImage //The image that will be displayed on the left hand side of the title bar.
		{
			get
			{
				return titleImage;
			}
			set
			{
				titleImage = value;
				if (value != null)
				{
					// Update the height of the title label
					TitleLabel.Height = Math.Max(titleImage.Height + (2 * MenuApplicationPanel.iconBorder), TitleLabel.Height);
					if(TitleLabel.Height < minTitleHeight)
					{
						TitleLabel.Height = minTitleHeight;
					}
				}
				TitleLabel.Invalidate();
			}
		}

		/// <summary>
		/// Gets/sets the transparent color for the the image displayed in the header of the title bar.
		/// </summary>
		//---------------------------------------------------------------------------
		public Color ImagesTransparentColor
		{
			get
			{
				return imagesTransparentColor;
			}
			set
			{
				imagesTransparentColor = value;
				if (grayAttributes != null)
				{
					grayAttributes.ClearColorKey(ColorAdjustType.Default);
					grayAttributes.SetColorKey(value, value, ColorAdjustType.Default);
				}
			}
		}

		//---------------------------------------------------------------------------
		public MenuXmlNode ApplicationNode	{ get { return applicationNode; } }

		//---------------------------------------------------------------------------
		public string ApplicationName
		{
			get
			{
				if (applicationNode == null || !applicationNode.IsApplication)
					return String.Empty;

				return applicationNode.GetNameAttribute();
			}
		}

		//---------------------------------------------------------------------------
		public MenuXmlParser MenuXmlParser
		{
			get
			{
				return menuXmlParser;
			}
		}

		//---------------------------------------------------------------------------
		public ArrayList GroupLabels
		{
			get
			{
				return groupLabels;
			}
		}
		
		//---------------------------------------------------------------------------
		public GroupLinkLabel CurrentGroupLabel
		{
			get
			{
				return currentGroupLabel;
			}
			set
			{
				if (value == null || !value.Equals(currentGroupLabel))
				{
					if (SelectedGroupChanging != null)
						SelectedGroupChanging(this, new MenuMngCtrlEventArgs((value != null) ? value.Node : null));

					currentGroupLabel = value;

					if (menuMngWinCtrl != null)
						menuMngWinCtrl.SelectGroupLinkLabel(currentGroupLabel);
					
					if (SelectedGroupChanged != null)
						SelectedGroupChanged(this, new MenuMngCtrlEventArgs((currentGroupLabel != null) ? currentGroupLabel.Node : null));
				}
			}
		}
		
		#endregion

		#region MenuApplicationPanel public methods

		//---------------------------------------------------------------------------
		public void Collapse()
		{
			State = PanelState.Collapsed;
		}
		
		//---------------------------------------------------------------------------
		public void Expand()
		{
			State = PanelState.Expanded;
		}
		
		//---------------------------------------------------------------------------
		public void InitializeGroupLabels()
		{
			if (applicationNode == null || !applicationNode.IsApplication || groupLabels != null)
				return;

			ArrayList groupItems = applicationNode.GroupItems;
			if (groupItems == null || groupItems.Count == 0)
				return;

			int groupIndex = 0;

			Point myPoint = new Point(labelsXOffset, TitleLabel.Height + labelsYSpacing);

			groupLabels = new ArrayList();

			System.Drawing.Bitmap defaultImage = null;
			Stream defaultImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DefaultGroupImage.bmp");
			if (defaultImageStream != null)
			{
				defaultImage = new Bitmap(Image.FromStream(defaultImageStream));

				if (defaultImage != null)
					defaultImage.MakeTransparent(Color.Magenta);
			}

			System.Drawing.Bitmap defaultUserReportsImage = null;
			Stream defaultUserReportsImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DefaultUserReportsGroup.bmp");
			if (defaultUserReportsImageStream != null)
			{
				defaultUserReportsImage = new Bitmap(Image.FromStream(defaultUserReportsImageStream));
				if (defaultUserReportsImage != null)
					defaultUserReportsImage.MakeTransparent(Color.Magenta);
			}

			System.Drawing.Bitmap defaultUserOfficeFilesImage = null;
			Stream defaultUserOfficeFilesImageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DefaultUserOfficeFilesGroup.bmp");
			if (defaultUserOfficeFilesImageStream != null)
			{
				defaultUserOfficeFilesImage = new Bitmap(Image.FromStream(defaultUserOfficeFilesImageStream));
				if (defaultUserOfficeFilesImage != null)
					defaultUserOfficeFilesImage.MakeTransparent(Color.Magenta);
			}


			foreach ( MenuXmlNode aGroupNode in groupItems)
			{
				// se un gruppo non contiene alcun menù non devo nemmeno farlo vedere:
				if (!aGroupNode.HasMenuChildNodes() || !aGroupNode.HasCommandDescendantsNodes)
					continue;

				myPoint.X = labelsXOffset;
				
				PictureBox myPictureBox = new System.Windows.Forms.PictureBox();
				myPictureBox.Location = myPoint;
				myPictureBox.TabStop = false;
				myPictureBox.BackColor = Color.Transparent;

				string imageFile = (menuXmlParser != null) ? menuXmlParser.GetGroupImageFileName(applicationNode.GetNameAttribute(),aGroupNode.GetNameAttribute()) : null;
				if (imageFile != null && imageFile.Length > 0 && File.Exists(imageFile))
				{
					try
					{
						Bitmap groupBitmap = ImagesHelper.LoadBitmapWithoutLockFile(imageFile);
						groupBitmap.MakeTransparent(Color.Magenta);

						myPictureBox.Image = groupBitmap;
					}
					catch(OutOfMemoryException exception)
					{	
						// The Image.FromFile method throws an OutOfMemoryException exception if the pixel format of the
						// image specified by imageFile is not supported.
						Debug.Fail("OutOfMemoryException raised in MenuApplicationPanel.InitializeGroupLabels: " + exception.Message);
						myPictureBox.Image = defaultImage;
					}
				}
				else
				{
					if (defaultUserReportsImage != null && aGroupNode.UserReportsGroup)
						myPictureBox.Image = defaultUserReportsImage;
					else if (defaultUserOfficeFilesImage != null && aGroupNode.UserOfficeFilesGroup)
						myPictureBox.Image = defaultUserOfficeFilesImage;
					else
						myPictureBox.Image = defaultImage;
				}

				if (myPictureBox.Image != null)
				{
					myPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
					Controls.Add(myPictureBox);
					myPoint.X += myPictureBox.Width + labelsXSpacingFromPicture;
				}

				GroupLinkLabel myLabel = new GroupLinkLabel(aGroupNode, (myPictureBox.Image != null) ? myPictureBox : null, Font);

				if (groupIndex == 0)
				{
					myLabel.TabStop = true;
					myLabel.TabIndex = 1;
				}
				else
					myLabel.TabStop = false;

				Size labelSize = new Size(myLabel.PreferredWidth, (myPictureBox != null) ? Math.Max(myLabel.PreferredHeight, myPictureBox.Image.Size.Height) : myLabel.PreferredHeight);
				myLabel.Size = labelSize;
				myLabel.Location = myPoint;
				myPoint.Y += labelSize.Height + labelsYSpacing;
				if (menuMngWinCtrl != null)
					myLabel.ContextMenu = menuMngWinCtrl.ItemsContextMenu;
				myLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(MenuApplicationPanel_GroupLinkLabelClicked);
				
				myLabel.Visible = true;
				
				groupLabels.Add(myLabel);

				Controls.Add(myLabel);
				
				groupIndex++;
				
			}				
			panelHeight = myPoint.Y;
		}

		//---------------------------------------------------------------------------
		public GroupLinkLabel AddLinkLabel(string newLabelText, Image newlabelImage)
		{
			if (newLabelText == null || newLabelText.Length == 0)
				return null;

			Point myPoint = new Point(labelsXOffset, Math.Max(panelHeight, TitleLabel.Height) + labelsYSpacing);

			PictureBox newPictureBox = null;
			if (newlabelImage != null)
			{
				newPictureBox = new System.Windows.Forms.PictureBox();
				newPictureBox.Location = myPoint;
				newPictureBox.Visible = true;
				newPictureBox.TabStop = false;
				newPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
				newPictureBox.Image = newlabelImage;
				newPictureBox.BackColor = Color.Transparent;
				
				myPoint.X += newPictureBox.Width + labelsXSpacingFromPicture;
				
				Controls.Add(newPictureBox);
			}
			GroupLinkLabel newLabel = new GroupLinkLabel(null, newPictureBox, Font);

			newLabel.Text = newLabelText;

			newLabel.BackColor = Color.Transparent;
			newLabel.LinkColor = Color.MidnightBlue;
			newLabel.VisitedLinkColor = Color.Purple;
			newLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
					
			if (Controls.Count == 0)
			{
				newLabel.TabStop = true;
				newLabel.TabIndex = 1;
			}
			else
				newLabel.TabStop = false;
			
			Size labelSize = new Size(newLabel.PreferredWidth,(newPictureBox != null) ? Math.Max(newLabel.PreferredHeight, newPictureBox.Height) : newLabel.PreferredHeight);
			newLabel.Size = labelSize;
			newLabel.TextAlign= ContentAlignment.MiddleLeft;
			newLabel.Location = myPoint;
			newLabel.Visible = true;
			if (menuMngWinCtrl != null)
				newLabel.ContextMenu = menuMngWinCtrl.ItemsContextMenu;
			newLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(MenuApplicationPanel_GroupLinkLabelClicked);

			if (groupLabels != null)
				groupLabels.Add(newLabel);
			
			Controls.Add(newLabel);
										
			panelHeight = myPoint.Y + labelSize.Height + labelsYSpacing;
			UpdateDisplayedState();

			return newLabel;
		}

		//---------------------------------------------------------------------------
		public void RemoveLinkLabel(GroupLinkLabel groupLinkLabelToRemove)
		{
			if (groupLinkLabelToRemove == null)
				return;

			if (groupLinkLabelToRemove.PictureBox != null)
				Controls.Remove(groupLinkLabelToRemove.PictureBox);
			Controls.Remove(groupLinkLabelToRemove);
			
			if (groupLabels != null)
			{
				groupLabels.Remove(groupLinkLabelToRemove);

				Point myPoint = new Point(labelsXOffset, TitleLabel.Height + labelsYSpacing);
				foreach (GroupLinkLabel groupLabel in groupLabels)
				{
					myPoint.X = labelsXOffset;
					if (groupLabel.PictureBox != null)
					{
						groupLabel.PictureBox.Location = myPoint;
						myPoint.X += groupLabel.PictureBox.Image.Size.Width + labelsXSpacingFromPicture;
					}
					groupLabel.Location = myPoint;
					myPoint.Y += groupLabel.Size.Height + labelsYSpacing;
				}
				panelHeight = myPoint.Y;
				UpdateDisplayedState();
			}
			return;
		}

		//---------------------------------------------------------------------------
		public MenuShortcutsCompositeComboBox AddMenuShortcutsComboBox()
		{
			Point myPoint = new Point(labelsXOffset, Math.Max(panelHeight, TitleLabel.Height) + labelsYSpacing);

			MenuShortcutsCompositeComboBox newComboBox = new MenuShortcutsCompositeComboBox(menuMngWinCtrl);

			if (menuMngWinCtrl != null)
				newComboBox.ImageList = menuMngWinCtrl.CommandsImageList;
					
			if (Controls.Count == 0)
			{
				newComboBox.TabStop = true;
				newComboBox.TabIndex = 1;
			}
			else
				newComboBox.TabStop = false;
			
			newComboBox.Location = myPoint;
			newComboBox.Visible = true;

			Size comboSize = new Size(Width - (2 * labelsXOffset), newComboBox.PreferredHeight);
			newComboBox.Size = comboSize;

			newComboBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
 
			Controls.Add(newComboBox);
										
			panelHeight = myPoint.Y + comboSize.Height + labelsYSpacing;
			UpdateDisplayedState();

			return newComboBox;
		}

		#endregion
		
		#region MenuApplicationPanel private methods
		
		// Expand/Collapse functionality updated as per Windows XP. Whole of title bar is active
		/// <summary>
		/// Helper function to determine if the mouse is currently over the title bar.
		/// </summary>
		/// <param name="xPos">The x-coordinate of the mouse position.</param>
		/// <param name="yPos">The y-coordinate of the mouse position.</param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		private bool IsOverTitle(int xPos, int yPos)
		{
			// Get the dimensions of the title label
			Rectangle rectTitle = TitleLabel.Bounds;
			// Check if the supplied coordinates are over the title label
			if(rectTitle.Contains(xPos, yPos))
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Helper function to update the displayed state of the panel.
		/// </summary>
		//---------------------------------------------------------------------------
		private void UpdateDisplayedState()
		{
			switch(state)
			{
				case PanelState.Collapsed :
					// Entering collapsed state, so collapse the panel
					Height = TitleLabel.Height;
					// Update the image.
					imageIndex = 1;
					break;
				case PanelState.Expanded :
					// Entering expanded state, so expand the panel.
					Height = panelHeight;
					// Update the image.
					imageIndex = 0;
					break;
				default :
					// Ignore
					break;
			}
			TitleLabel.Invalidate();

			OnPanelStateChanged(new MenuApplicationPanelEventArgs(this));
		}

		#endregion

		#region MenuApplicationPanel event handlers
		
		/// <summary>
		/// Event handler for the PanelStateChanged event.
		/// </summary>
		/// <param name="e">A MenuApplicationPanelEventArgs that contains the event data.</param>
		//---------------------------------------------------------------------------
		protected virtual void OnPanelStateChanged(MenuApplicationPanelEventArgs e)
		{
			if(PanelStateChanged != null)
				PanelStateChanged(this, e);
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
			const int diameter = 14;
			int radius = diameter / 2;
			Rectangle bounds = TitleLabel.Bounds;
			int offsetY = 0;
			if(titleImage != null)
			{
				offsetY = TitleLabel.Height - minTitleHeight;
				if(offsetY < 0)
				{
					offsetY = 0;
				}
				bounds.Offset(0, offsetY);
				bounds.Height -= offsetY;
			}

			e.Graphics.Clear(Parent.BackColor);

			// Create a GraphicsPath with curved top corners
			GraphicsPath path = new GraphicsPath();
			path.AddLine(bounds.Left + radius, bounds.Top, bounds.Right - diameter - 1, bounds.Top);
			path.AddArc(bounds.Right - diameter - 1, bounds.Top, diameter, diameter, 270, 90);
			path.AddLine(bounds.Right, bounds.Top + radius, bounds.Right, bounds.Bottom);
			path.AddLine(bounds.Right, bounds.Bottom, bounds.Left - 1, bounds.Bottom);
			path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);

			// Create a color gradient
			// Draws the title gradient grayscale when disabled.
			e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
			if (Enabled)
			{
				LinearGradientBrush brush = new LinearGradientBrush(
					bounds, startColor, endColor, LinearGradientMode.Horizontal);

				// Paint the color gradient into the title label.
				e.Graphics.FillPath(brush, path);
			}
			else
			{
				PanelColor grayStart = new PanelColor();
				grayStart.CurrentColor = startColor;
				grayStart.Saturation = 0f;
				
				PanelColor grayEnd = new PanelColor();
				grayEnd.CurrentColor = endColor;
				grayEnd.Saturation = 0f;

				LinearGradientBrush brush = new LinearGradientBrush(
					bounds, grayStart.CurrentColor, grayEnd.CurrentColor,
					LinearGradientMode.Horizontal);

				// Paint the grayscale gradient into the title label.
				e.Graphics.FillPath(brush, path);
			}

			// Draw the header icon, if there is one
			System.Drawing.GraphicsUnit graphicsUnit = System.Drawing.GraphicsUnit.Display;
			int offsetX = MenuApplicationPanel.iconBorder;
			if(titleImage != null)
			{
				offsetX += titleImage.Width + MenuApplicationPanel.iconBorder;
				// Draws the title icon grayscale when the panel is disabled.
				RectangleF srcRect = titleImage.GetBounds(ref graphicsUnit);
				Rectangle destRect = new Rectangle(MenuApplicationPanel.iconBorder,
					MenuApplicationPanel.iconBorder, titleImage.Width, titleImage.Height);
				
				if(Enabled)
				{
					ImageAttributes imgAttrs = new ImageAttributes();
					// Set the default color key.
					imgAttrs.SetColorKey(ImagesTransparentColor, ImagesTransparentColor, ColorAdjustType.Default);
				
					e.Graphics.DrawImage
								(
									titleImage, 
									destRect, 
									(int)srcRect.Left, 
									(int)srcRect.Top,
									(int)srcRect.Width, 
									(int)srcRect.Height, 
									graphicsUnit, 
									imgAttrs
								);
				}
				else
				{
					e.Graphics.DrawImage
							(
								titleImage, 
								destRect, 
								(int)srcRect.Left, 
								(int)srcRect.Top,
								(int)srcRect.Width, 
								(int)srcRect.Height, 
								graphicsUnit, 
								grayAttributes
							);
				}
			}

			// Draw the title text.
			// Title text truncated with an ellipsis where necessary.
			float left = (float)offsetX;
			float top = (float)offsetY + (float)MenuApplicationPanel.iconBorder;
			float width = (float)TitleLabel.Width - left - PanelImageList.ImageSize.Width - MenuApplicationPanel.expandBorder;
			float height = (float)minTitleHeight - (2f * (float)MenuApplicationPanel.iconBorder);
			RectangleF textRectF = new RectangleF(left, top, width, height);
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.EllipsisWord;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			// Draw title text disabled where appropriate.
			if(Enabled)
			{
				SolidBrush textBrush = new SolidBrush(TitleFontColor);
				e.Graphics.DrawString(TitleLabel.Text, TitleLabel.Font, textBrush, textRectF, format);
				textBrush.Dispose();

				if (TitleLabel.Focused)
					ControlPaint.DrawFocusRectangle(e.Graphics, new Rectangle((int)Math.Floor(left), (int)Math.Floor(top), (int)Math.Ceiling(width), (int)Math.Ceiling(height)));
			}
			else
			{
				Color disabled = SystemColors.GrayText;
				ControlPaint.DrawStringDisabled(e.Graphics, TitleLabel.Text, TitleLabel.Font, disabled, textRectF, format);
			}

			// Draw a white line at the bottom:
			const int lineWidth = 1;
			SolidBrush lineBrush = new SolidBrush(Color.White);
			Pen linePen = new Pen(lineBrush, lineWidth);
			path.Reset();
			path.AddLine(bounds.Left, bounds.Bottom - lineWidth, bounds.Right, 
				bounds.Bottom - lineWidth);
			e.Graphics.DrawPath(linePen, path);

			linePen.Dispose();
			lineBrush.Dispose();

			path.Dispose();

			// Draw the expand/collapse image
			// Expand/Collapse image drawn grayscale when panel is disabled.
			if (state >= 0 && (int)state < PanelImageList.Images.Count)
			{
				int xPos = bounds.Right - PanelImageList.ImageSize.Width - MenuApplicationPanel.expandBorder;
				int yPos = bounds.Top + MenuApplicationPanel.expandBorder;
				RectangleF srcIconRectF = PanelImageList.Images[(int)state].GetBounds(ref graphicsUnit);
				Rectangle destIconRect = new Rectangle(xPos, yPos, 
					PanelImageList.ImageSize.Width, PanelImageList.ImageSize.Height);
				if(Enabled)
				{
					e.Graphics.DrawImage(PanelImageList.Images[(int)state], destIconRect,
						(int)srcIconRectF.Left, (int)srcIconRectF.Top, (int)srcIconRectF.Width,
						(int)srcIconRectF.Height, graphicsUnit);
				}
				else
				{
					e.Graphics.DrawImage(PanelImageList.Images[(int)state], destIconRect,
						(int)srcIconRectF.Left, (int)srcIconRectF.Top, (int)srcIconRectF.Width,
						(int)srcIconRectF.Height, graphicsUnit, grayAttributes);
				}
			}
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button == MouseButtons.Left) && (true == IsOverTitle(e.X, e.Y)))
			{
				if((PanelImageList != null) && (PanelImageList.Images.Count >= 2))
				{
					// Currently expanded, so store the current height.
					// Currently collapsed, so expand the panel.
					State = (imageIndex == 0) ? PanelState.Collapsed : PanelState.Expanded;
				}
			}
		}

		//---------------------------------------------------------------------------
		private void TitleLabel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if((e.Button == MouseButtons.None) && (true == IsOverTitle(e.X, e.Y)))
			{
				TitleLabel.Cursor = Cursors.Hand;
			}
			else
			{
				TitleLabel.Cursor = Cursors.Default;
			}
		}
		
		//---------------------------------------------------------------------------
		private void TitleLabel_KeyUp(object sender, KeyEventArgs e)
		{
			base.OnKeyUp (e);

			if (TitleLabel.Focused && e.KeyCode == Keys.Enter)
			{
				if (IsExpanded)
					Collapse();
				else if (IsCollapsed)
					Expand();
			}

		}
		
		//---------------------------------------------------------------------------
		public void MenuApplicationPanel_GroupLinkLabelClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if (sender != null && (sender is GroupLinkLabel))
			{
				if 
					(
						menuMngWinCtrl.CurrentApplicationNode == null ||
						!menuMngWinCtrl.CurrentApplicationNode.IsSameApplicationAs((MenuXmlNode)applicationNode)
					)
				{
					menuMngWinCtrl.SelectApplicationPanel(this);
					
					if (SelectedApplicationChanged != null)
						SelectedApplicationChanged(this, new MenuMngCtrlEventArgs((MenuXmlNode)applicationNode));
				}
//				if (menuMngWinCtrl != null)
//					menuMngWinCtrl.KeyboardInputEnabled = false;

				this.CurrentGroupLabel = (GroupLinkLabel)sender;
				((GroupLinkLabel)sender).Focus();
		
//				Application.DoEvents();
//
//				if (menuMngWinCtrl != null)
//					menuMngWinCtrl.KeyboardInputEnabled = true;
			}
		}
	
		#endregion
	}
	
	#endregion

	#region MenuApplicationPanelCollection class
	/// <summary>
	/// Summary description for MenuApplicationPanelCollection.
	/// </summary>
	//============================================================================
	public class MenuApplicationPanelCollection : System.Collections.CollectionBase
	{
		/// <summary>
		/// Adds a MenuApplicationPanel to the end of the collection.
		/// </summary>
		/// <param name="panel">The MenuApplicationPanel to add.</param>
		public void Add(MenuApplicationPanel panel)
		{
			List.Add(panel);
		}

		/// <summary>
		/// Removes the MenuApplicationPanel from the collection at the specified index.
		/// </summary>
		/// <param name="index">The index of the MenuApplicationPanel to remove.</param>
		//---------------------------------------------------------------------------
		public void Remove(int index)
		{
			// Ensure the supplied index is valid
			if((index >= Count) || (index < 0))
			{
				throw new IndexOutOfRangeException("The supplied index is out of range");
			}
			List.RemoveAt(index);
		}

		/// <summary>
		/// Gets a reference to the MenuApplicationPanel at the specified index.
		/// </summary>
		/// <param name="index">The index of the MenuApplicationPanel to retrieve.</param>
		/// <returns></returns>
		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuApplicationPanel this[int index]
		{
			get 
			{
				if((index >= Count) || (index < 0))
				{
					throw new IndexOutOfRangeException("The supplied index is out of range");
				}
				return (MenuApplicationPanel)List[index];
			}
		}

		/// <summary>
		/// Inserts a MenuApplicationPanel at the specified position.
		/// </summary>
		/// <param name="index">The zero-based index at which <i>panel</i> should be inserted.</param>
		/// <param name="panel">The MenuApplicationPanel to insert into the collection.</param>
		//---------------------------------------------------------------------------
		public void Insert(int index, MenuApplicationPanel panel)
		{
			List.Insert(index, panel);
		}

		/// <summary>
		/// Copies the elements of the collection to a System.Array starting at a particular index.
		/// </summary>
		/// <param name="array">The one-dimensional System.Array that is the destination of the elements. The array must have zero-based indexing.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		//---------------------------------------------------------------------------
		public void CopyTo(System.Array array, System.Int32 index)
		{
			List.CopyTo(array, index);
		}

		/// <summary>
		/// Searches for the specified MenuApplicationPanel and returns the zero-based index of the first occurrence.
		/// </summary>
		/// <param name="panel">The MenuApplicationPanel to search for.</param>
		/// <returns></returns>
		//---------------------------------------------------------------------------
		public int IndexOf(MenuApplicationPanel panel)
		{
			return List.IndexOf(panel);
		}
	}

	#endregion
	
	#region MenuApplicationPanelEventArgs class

	//============================================================================
	public class MenuApplicationPanelEventArgs : System.EventArgs
	{
		private MenuApplicationPanel panel;

		//---------------------------------------------------------------------------
		public MenuApplicationPanelEventArgs(MenuApplicationPanel sender)
		{
			panel = sender;
		}

		/// <summary>
		/// Gets the MenuApplicationPanel that triggered the event.
		/// </summary>
		//---------------------------------------------------------------------------
		public MenuApplicationPanel MenuApplicationPanel
		{
			get
			{
				return panel;
			}
		}

		/// <summary>
		/// Gets the PanelState of the MenuApplicationPanel that triggered the event.
		/// </summary>
		//---------------------------------------------------------------------------
		[Localizable(true)]
		public MenuApplicationPanel.PanelState State
		{
			get
			{
				return panel.State;
			}
		}
	}

	#endregion

	#region GroupLinkLabel class

	/// <summary>
	/// Summary description for GroupLinkLabel.
	/// </summary>
	//============================================================================
	public class GroupLinkLabel : System.Windows.Forms.LinkLabel
	{
		public		string		CurrentMenuPath = String.Empty;
		public		string		CurrentCommandPath = String.Empty;
		internal	bool		GroupVisited = false;
		private		PictureBox	pictureBox;
		private		bool		underline = false;
		
		private	const int xBorderOffset = 6;
		
		//---------------------------------------------------------------------------
		public GroupLinkLabel(MenuXmlNode aGroupNode, PictureBox aPictureBox, System.Drawing.Font aFont)
		{
			Debug.Assert(aGroupNode == null || aGroupNode.IsGroup);

			Font = new System.Drawing.Font(aFont, System.Drawing.FontStyle.Regular);
			FlatStyle = FlatStyle.System;
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
			BackColor = Color.Transparent;
            //RenderTransparent = false; //'System.Windows.Forms.Label.RenderTransparent' is obsolete. This property has been deprecated. Use BackColor instead

			LinkColor = Color.MidnightBlue;
			VisitedLinkColor = Color.Purple;
			Visible = false;
			TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			TabStop = true;
					
			Text = (aGroupNode != null && aGroupNode.IsGroup) ? aGroupNode.Title : String.Empty;

			Tag = (aGroupNode != null && aGroupNode.IsGroup) ? aGroupNode : null;
		
			CurrentMenuPath = String.Empty;
			CurrentCommandPath = String.Empty;
			GroupVisited = false;

			pictureBox = aPictureBox;
		}
		
		#region GroupLinkLabel public properties

		//---------------------------------------------------------------------------
		public MenuXmlNode Node
		{
			get
			{
				return ((Tag != null && (Tag is MenuXmlNode) && ((MenuXmlNode)Tag).IsGroup)) ? (MenuXmlNode)Tag : null;
			}
		}
		
		//---------------------------------------------------------------------------
		public string GroupName
		{
			get
			{
				MenuXmlNode groupNode = this.Node;
				return (groupNode != null) ? groupNode.GetNameAttribute() : String.Empty;
			}
		}

		//---------------------------------------------------------------------------
		public PictureBox PictureBox
		{
			get
			{
				return pictureBox;
			}
		}
		
		#endregion

		#region GroupLinkLabel public methods
		
		//---------------------------------------------------------------------------
		public void SetWidthToFitPanel(MenuApplicationPanel aPanel)
		{
			if (aPanel != null)
				this.Width = GetFittingWidth(aPanel);
		}
		
		#endregion

		#region GroupLinkLabel protected overridden methods
		
		//---------------------------------------------------------------------------
		protected override void InitLayout()
		{
			base.InitLayout();

			underline = (this.LinkBehavior == System.Windows.Forms.LinkBehavior.AlwaysUnderline);
			
			this.AutoSize = false;
		}
		
		//---------------------------------------------------------------------------
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			this.LinkArea = new System.Windows.Forms.LinkArea(0, this.Text.Length);
		}
		
		//---------------------------------------------------------------------------
		protected override void OnAutoSizeChanged(EventArgs e)
		{
			this.AutoSize = false;
			base.OnAutoSizeChanged(e);
		}
		
		//---------------------------------------------------------------------------
		protected override void OnMouseHover(EventArgs e)
		{
			base.OnMouseHover(e);

			underline = (this.LinkBehavior == System.Windows.Forms.LinkBehavior.AlwaysUnderline) ||
				(this.LinkBehavior == System.Windows.Forms.LinkBehavior.HoverUnderline);
		}
		
		//---------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);

			underline = (this.LinkBehavior == System.Windows.Forms.LinkBehavior.AlwaysUnderline);

		}
		
		//---------------------------------------------------------------------------
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			// Devo espandere il pannello dell'applicazione qualora esso risulti 
			// collassato, altrimenti non vedo dove sta correntemente il fuoco
			if 
				(
					this.Parent != null && 
					this.Parent is MenuApplicationPanel &&
					((MenuApplicationPanel)this.Parent).IsCollapsed
				)
				((MenuApplicationPanel)this.Parent).Expand();

		}
		
		//---------------------------------------------------------------------------
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
		}
		
		//---------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			PaintLabel(e.Graphics);		
		}

		#endregion
		
		#region GroupLinkLabel private methods

		//---------------------------------------------------------------------------
		private void PaintLabel(System.Drawing.Graphics graphics)
		{
			if (graphics == null)
				return;

			System.Drawing.SolidBrush transparentBrush = new SolidBrush(Color.Transparent);
			graphics.FillRectangle(transparentBrush, this.ClientRectangle);
			transparentBrush.Dispose();

			System.Drawing.RectangleF labelRectF = new System.Drawing.RectangleF(0.0f, 0.0f, (float)this.ClientRectangle.Width, (float)this.ClientRectangle.Height);

			if (this.ImageList != null && this.ImageIndex != -1)
			{
				System.Drawing.Image image = this.ImageList.Images[this.ImageIndex];
				if (image != null)
				{
					int destWidth = Math.Min(image.Width, this.Width);
					Rectangle destIconRect = new Rectangle(0, 0, destWidth,  Math.Min(image.Height, this.Height));

					graphics.DrawImage
						(
						image, 
						destIconRect
						);

					labelRectF = new System.Drawing.RectangleF((float)destWidth, 0.0f, (float)(this.ClientRectangle.Width - destWidth), (float)this.ClientRectangle.Height);
				}
			}
			
			System.Drawing.Color textColor = this.LinkColor;
			if (!this.Enabled) 
				textColor = this.DisabledLinkColor;
			else if (this.LinkVisited) 
				textColor = this.VisitedLinkColor;

			System.Drawing.Font linkLabelFont = new System.Drawing.Font(this.Font, underline ? System.Drawing.FontStyle.Underline : System.Drawing.FontStyle.Regular);

			StringFormat format = new StringFormat();
			format.Trimming = StringTrimming.EllipsisWord;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
			format.LineAlignment = StringAlignment.Center;
			
			System.Drawing.SolidBrush textBrush = new SolidBrush(textColor);
			graphics.DrawString(this.Text, linkLabelFont, textBrush, labelRectF, format);
			textBrush.Dispose();
	
			if (this.Focused)
			{
				System.Drawing.SizeF textSize = graphics.MeasureString(this.Text, linkLabelFont);
				// Devo centrare verticalmente il rettangolo nell'area client della
				// label visto cho ho specificato LineAlignment = StringAlignment.Center
				int focusRectHeight = Math.Min(this.ClientRectangle.Height - 2,(int)Math.Ceiling(textSize.Height));
				int focusRectWidth = Math.Min(this.ClientRectangle.Width - 2,(int)Math.Ceiling(textSize.Width));
				if (focusRectWidth > 0 && focusRectHeight > 0)
				{
					System.Drawing.Rectangle textRect = new Rectangle(1,(this.ClientRectangle.Height - focusRectHeight)/2, focusRectWidth, focusRectHeight); 
					ControlPaint.DrawFocusRectangle(graphics, textRect);
				}
			}
			linkLabelFont.Dispose();
		}

		//---------------------------------------------------------------------------
		private int GetFittingWidth(MenuApplicationPanel aPanel)
		{
			if (aPanel == null)
				return this.Width;

			System.Drawing.Rectangle labelRect = this.RectangleToScreen(this.ClientRectangle);
			System.Drawing.Point labelPoint = aPanel.PointToClient(new System.Drawing.Point(labelRect.Left, labelRect.Top));
			return aPanel.Width - labelPoint.X - xBorderOffset;
		}
		
		#endregion
	}

	#endregion

	#region PanelColor class
	/// <summary>
	/// Stores a color and provides conversion between the RGB and HLS color models
	/// </summary>
	//============================================================================
	public class PanelColor
	{
		// Constants
		public const int HUEMAX = 360;
		public const float SATMAX = 1.0f;
		public const float BRIGHTMAX = 1.0f;
		public const int RGBMAX	= 255;

		// Member variables
		private Color	currentColor = Color.Red;

		/// <summary>
		/// The current PanelColor (RGB model)
		/// </summary>
		//---------------------------------------------------------------------------
		public Color CurrentColor
		{
			get
			{
				return currentColor;
			}
			set
			{
				currentColor = value;
			}
		}


		/// <summary>
		/// The Red component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Red
		{
			get
			{
				return currentColor.R;
			}
			set
			{
				currentColor = Color.FromArgb(value, Green, Blue);
			}
		}

		/// <summary>
		/// The Green component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Green
		{
			get
			{
				return currentColor.G;
			}
			set
			{
				currentColor = Color.FromArgb(Red, value, Blue);
			}
		}

		/// <summary>
		/// The Blue component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public byte Blue
		{
			get
			{
				return currentColor.B;
			}
			set
			{
				currentColor = Color.FromArgb(Red, Green, value);
			}
		}

		/// <summary>
		/// The Hue component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public int Hue
		{
			get
			{
				return (int)currentColor.GetHue();
			}
			set
			{
				currentColor = HSBToRGB(value,
					currentColor.GetSaturation(),
					currentColor.GetBrightness());
			}
		}

		
		//---------------------------------------------------------------------------
		public float GetHue()
		{
			float top = ((float)(2 * Red - Green - Blue)) / (2 * 255);
			float bottom = (float)Math.Sqrt(((Red - Green) * (Red - Green) + (Red - Blue) * (Green - Blue)) / 255);
			return (float)Math.Acos(top / bottom);
		}

		/// <summary>
		/// The Saturation component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public float Saturation
		{
			get
			{
				if(0.0f == Brightness)
				{
					return 0.0f;
				}
				else
				{
					float fMax = (float)Math.Max(Red, Math.Max(Green, Blue));
					float fMin = (float)Math.Min(Red, Math.Min(Green, Blue));
					return (fMax - fMin) / fMax;
				}
			}
			set
			{
				currentColor = HSBToRGB((int)currentColor.GetHue(),
					value, currentColor.GetBrightness());
			}
		}

		//---------------------------------------------------------------------------
		public float GetSaturation()
		{
			return (255 -
				(((float)(Red + Green + Blue)) / 3) * Math.Min(Red, Math.Min(Green, Blue))) / 255;
		}

		/// <summary>
		/// The Brightness component of the current color
		/// </summary>
		//---------------------------------------------------------------------------
		public float Brightness
		{
			get
			{
				//return currentColor.GetBrightness();
				return (float)Math.Max(Red, Math.Max(Green, Blue)) / (255.0f);
			}
			set
			{
				currentColor = PanelColor.HSBToRGB((int)currentColor.GetHue(),
					currentColor.GetSaturation(),
					value);
			}
		}

		//---------------------------------------------------------------------------
		public float GetBrightness()
		{
			return ((float)(Red + Green + Blue)) / (255.0f * 3.0f);
		}
		
		/// <summary>
		/// Converts HSB color components to an RGB System.Drawing.Color
		/// </summary>
		/// <param name="Hue">Hue component</param>
		/// <param name="Saturation">Saturation component</param>
		/// <param name="Brightness">Brightness component</param>
		/// <returns>Returns the RGB value as a System.Drawing.Color</returns>
		//---------------------------------------------------------------------------
		public static Color HSBToRGB(int Hue, float Saturation, float Brightness)
		{
			// TODO: CheckHSBValues(Hue, Saturation, Brightness);
			int red = 0; int green = 0; int blue = 0;
			if(Saturation == 0.0f)
			{
				// Achromatic color (black and white centre line)
				// Hue should be 0 (undefined), but we'll ignore it.
				// Set shade of grey
				red = green = blue = (int)(Brightness * 255);
			}
			else
			{
				// Chromatic color
				// Map hue from [0-255] to [0-360] to hexagonal-space [0-6]
				// (360 / 256) * hue[0-255] / 60
				float fHexHue = (6.0f / 360.0f) * Hue;
				// Determine sector in hexagonal-space (RGB cube projection) {0,1,2,3,4,5}
				float fHexSector = (float)Math.Floor((double)fHexHue);
				// Determine exact position in particular sector [0-1]
				float fHexSectorPos = fHexHue - fHexSector;

				// Convert parameters to in-formula ranges
				float fBrightness = Brightness * 255.0f;
				float fSaturation = Saturation/*(float)Saturation * (1.0f / 360.0f)*/;

				// Magic formulas (from Foley & Van Dam). Adding 0.5 performs rounding instead of truncation
				byte bWashOut = (byte)(0.5f + fBrightness * (1.0f - fSaturation));
				byte bHueModifierOddSector = (byte)(0.5f + fBrightness * (1.0f - fSaturation * fHexSectorPos));
				byte bHueModifierEvenSector = (byte)(0.5f + fBrightness * (1.0f - fSaturation * (1.0f - fHexSectorPos)));

				// Assign values to RGB components (sector dependent)
				switch((int)fHexSector)
				{
					case 0 :
						// Hue is between red & yellow
						red = (int)(Brightness * 255); green = bHueModifierEvenSector; blue = bWashOut;
						break;
					case 1 :
						// Hue is between yellow & green
						red = bHueModifierOddSector; green = (int)(Brightness * 255); blue = bWashOut;
						break;
					case 2 :
						// Hue is between green & cyan
						red = bWashOut; green = (int)(Brightness * 255); blue = bHueModifierEvenSector;
						break;
					case 3 :
						// Hue is between cyan & blue
						red = bWashOut; green = bHueModifierOddSector; blue = (int)(Brightness * 255);
						break;
					case 4 :
						// Hue is between blue & magenta
						red = bHueModifierEvenSector; green = bWashOut; blue = (int)(Brightness * 255);
						break;
					case 5 :
						// Hue is between magenta & red
						red = (int)(Brightness * 255); green = bWashOut; blue = bHueModifierOddSector;
						break;
					default :
						red = 0; green = 0; blue = 0;
						break;
				}
			}

			return Color.FromArgb(red, green, blue);
		}
	}

	#endregion

}