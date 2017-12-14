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
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	#region CommandsListBox class

	public partial class CommandsListBox : System.Windows.Forms.ListBox
	{
		[Flags]
			internal enum CommandTypesToShow
		{
			Undefined	= 0x00000000,		
			Documents	= 0x00000001,				
			Reports		= 0x00000002,
			Batches		= 0x00000004,		
			Functions	= 0x00000008,						
			Executables	= 0x00000010,			
			Texts		= 0x00000020,
			OfficeItems	= 0x00000040,
			All			= Documents | Reports | Batches | Functions | Executables | Texts | OfficeItems
		}

		private MenuXmlParser		menuXmlParser = null;
		private IPathFinder			pathFinder = null;
		private MenuXmlNode			currentMenuNode = null;
		private CommandsImageInfos	commandsImagesInfo = null;
		private CommandTypesToShow	commandTypesToShow = CommandTypesToShow.Undefined;
		private bool				showDescriptions = false;
		private bool				showReportsDates = false;
		private bool				isRefreshSuspended = false;
		private int					currentClientWidth = 0;
		private bool				parentMenuIndependent = false;

		CommandListBoxItemCollection listItems = null;
			
		public static System.Drawing.Color DefaultCurrentUserCommandForeColor = Color.RoyalBlue;
		public static System.Drawing.Color DefaultAllUsersCommandForeColor = Color.Blue; 
		public static System.Drawing.Color DefaultDescriptionsForeColor = Color.Navy; 
		public static System.Drawing.Color DefaultReportDatesForeColor = Color.RoyalBlue; 

		private System.Drawing.Color currentUserCommandForeColor = DefaultCurrentUserCommandForeColor;
		private System.Drawing.Color allUsersCommandForeColor = DefaultAllUsersCommandForeColor;
		private System.Drawing.Color descriptionsForeColor = DefaultDescriptionsForeColor;
		private System.Drawing.Color reportDatesForeColor = DefaultReportDatesForeColor;

		private System.Windows.Forms.ImageList	typeImageList = null;
		private bool showStateImages = false;
		private System.Windows.Forms.ImageList	stateImageList = null;

		private const int defaultItemHeight = 28; 
		private const int maximumItemHeight = 255; // The maximum height of a ListBox item is 255 pixels 
		private const int itemXOffset = 2; 
		private const int itemYOffset = 2; 

		public event System.EventHandler SelectedCommandChanged;
		public event System.EventHandler ShowFlagsChanged;

		#region CommandsListBox constructor

		//---------------------------------------------------------------------------
		public CommandsListBox()
		{
			listItems = new CommandListBoxItemCollection(this);

			InitializeComponent();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
			this.UpdateStyles();
			
			AssignImages();
		}
		
		#endregion
		
		#region CommandsListBox protected overridden methods

		//--------------------------------------------------------------------
		protected override void OnLayout(LayoutEventArgs e)
		{
			// Invoke base class implementation
			base.OnLayout(e);

			if (currentClientWidth != this.ClientRectangle.Width)
			{
				currentClientWidth = this.ClientRectangle.Width;
				if (this.Items == null || this.ItemsCount == 0)
					return;

				// Devo necessariamente rinfrescare la lista perchè, altrimenti, i suoi
				// item verrebbero ridisegnati solo parzialmente con un brutto effetto
				// di "sporcizia" in background. Se chiamassi semplicemente il metodo di
				// Refresh per ridisegnare gli item della lista, essi non verrebbero 
				// "rimisurati" (cioè non verrebbe scatenato il OnMeasureItem) e le dimensioni
				// degli item non verrebbero aggiornate. Pertanto, devo "ricaricarli"
				RefreshCommandsList();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnMeasureItem(MeasureItemEventArgs e)
		{
			base.OnMeasureItem(e);

			if (e == null || e.Index == -1 || e.Index >= this.ItemsCount)
				return;

			CommandListBoxItem itemToMeasure = this.GetElementAt(e.Index);
			if (itemToMeasure == null)
				return;

			e.ItemWidth = currentClientWidth;

			int itemHeight = 0;
			if (showDescriptions && itemToMeasure.Description != null && itemToMeasure.Description.Length > 0)
			{
				int descrLinesNumber = 1 +
					(int)Math.Ceiling(e.Graphics.MeasureString(itemToMeasure.Description, this.Font).Width) / (this.ClientRectangle.Width - 2*itemXOffset - typeImageList.ImageSize.Width);
				itemHeight = (1+descrLinesNumber) * this.Font.Height;
			}
			else
				itemHeight = this.Font.Height;

			if (showReportsDates && itemToMeasure.IsRunReport && (itemToMeasure.CreationTime != DateTime.MinValue || itemToMeasure.LastWriteTime != DateTime.MinValue))
			{
				System.Drawing.Font datesFont = new Font(this.Font.FontFamily.Name, this.Font.SizeInPoints - 0.5f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
				
				if (itemToMeasure.CreationTime != DateTime.MinValue)
					itemHeight +=  datesFont.Height + 1;
				if (itemToMeasure.LastWriteTime != DateTime.MinValue)
					itemHeight +=  datesFont.Height + 1;
				
				datesFont.Dispose();
			}
			
			e.ItemHeight = Math.Min(maximumItemHeight, Math.Max(itemHeight, defaultItemHeight));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (e == null || e.Graphics == null || e.Index == -1 || e.Index >= this.ItemsCount)
				return;

			CommandListBoxItem itemToDraw = this.GetElementAt(e.Index);
			if (itemToDraw == null)
				return;

			bool isItemSelected = ((e.State & DrawItemState.Selected) == DrawItemState.Selected);
			bool isDescriptionAvailable = (showDescriptions && itemToDraw.Description != null && itemToDraw.Description.Length > 0);
			bool isCreationTimeAvailable = (showReportsDates && itemToDraw.IsRunReport && itemToDraw.CreationTime != DateTime.MinValue);
			bool isLastWriteTimeAvailable = (showReportsDates && itemToDraw.IsRunReport && itemToDraw.LastWriteTime != DateTime.MinValue);

			System.Drawing.SolidBrush backColorBrush = new System.Drawing.SolidBrush(this.BackColor);
			e.Graphics.FillRectangle(backColorBrush, e.Bounds);

			int leftOffset = e.Bounds.Left + itemXOffset;

			if (typeImageList != null && itemToDraw.ImageIndex != -1 && itemToDraw.ImageIndex < typeImageList.Images.Count)
			{			
				System.Drawing.Image itemImage = typeImageList.Images[itemToDraw.ImageIndex];

				if (itemImage != null)
				{
					if (isItemSelected)
						ControlPaint.DrawImageDisabled(e.Graphics, itemImage, leftOffset, e.Bounds.Top + itemYOffset, e.BackColor);
					else
						e.Graphics.DrawImageUnscaled(itemImage, leftOffset, e.Bounds.Top + itemYOffset);
					
					leftOffset += (itemImage.Width + itemXOffset);
				}
			}

			System.Drawing.Rectangle itemTextRect = new System.Drawing.Rectangle(leftOffset, e.Bounds.Top, e.Bounds.Width - leftOffset - itemXOffset, e.Bounds.Height);

			System.Drawing.SolidBrush textBackColorBrush = new System.Drawing.SolidBrush(isItemSelected ? SystemColors.Highlight : this.BackColor);
			e.Graphics.FillRectangle(textBackColorBrush, itemTextRect);

			if (this.Focused && isItemSelected)
				ControlPaint.DrawFocusRectangle(e.Graphics, itemTextRect);

			System.Drawing.Rectangle titleRect = new System.Drawing.Rectangle(leftOffset, e.Bounds.Top, e.Bounds.Width - leftOffset - itemXOffset, (isDescriptionAvailable || isCreationTimeAvailable || isLastWriteTimeAvailable)? e.Font.Height : e.Bounds.Height);
			
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Trimming = StringTrimming.EllipsisWord;
			format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;

			System.Drawing.SolidBrush titleColorBrush = new System.Drawing.SolidBrush(isItemSelected ? SystemColors.HighlightText : itemToDraw.TitleColor);

			e.Graphics.DrawString(itemToDraw.Title, e.Font, titleColorBrush, titleRect, format);

			int topOffset = e.Bounds.Top + e.Font.Height;

			if (isDescriptionAvailable)
			{
				int rectHeight = e.Bounds.Bottom - topOffset;
				if (isCreationTimeAvailable)
					rectHeight -= e.Font.Height;
				if (isLastWriteTimeAvailable)
					rectHeight -= e.Font.Height;
				
				System.Drawing.Rectangle descriptionRect = new System.Drawing.Rectangle(leftOffset, topOffset, e.Bounds.Width - leftOffset - itemXOffset, rectHeight);
				
				topOffset += descriptionRect.Height;

				System.Drawing.SolidBrush descriptionColorBrush = new System.Drawing.SolidBrush(isItemSelected ? SystemColors.HighlightText : descriptionsForeColor);
				
				StringFormat descrFormat = new StringFormat();
				descrFormat.LineAlignment = StringAlignment.Center;
				descrFormat.Trimming = StringTrimming.EllipsisWord;
				descrFormat.FormatFlags = StringFormatFlags.NoClip;

				//Sostituisco gli a capo con uno spazio perche il rettangolo e' calcolato
				//senza tener conto dell'andata a capo
				string desc = itemToDraw.Description.Replace("\r\n", " ");
				e.Graphics.DrawString(desc, e.Font, descriptionColorBrush, descriptionRect, descrFormat);
			}
			
			if (isCreationTimeAvailable || isLastWriteTimeAvailable) 
			{
				System.Drawing.Font datesFont = new Font(e.Font.FontFamily.Name, e.Font.SizeInPoints - 0.5f, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
				if ((e.Bounds.Bottom - topOffset) >= datesFont.Height)
				{
					System.Drawing.Rectangle dateRect = new System.Drawing.Rectangle(leftOffset, topOffset, e.Bounds.Width - leftOffset - itemXOffset, e.Font.Height);
				
					StringFormat datesFormat = new StringFormat();
					datesFormat.LineAlignment = StringAlignment.Center;
					datesFormat.Trimming = StringTrimming.EllipsisWord;
					datesFormat.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;

					System.Drawing.SolidBrush datesColorBrush = new System.Drawing.SolidBrush(isItemSelected ? SystemColors.HighlightText : reportDatesForeColor);
					if (isCreationTimeAvailable)
						e.Graphics.DrawString(String.Format(MenuManagerWindowsControlsStrings.ReportCreationTimeFormat, itemToDraw.CreationTime.ToString()), datesFont, datesColorBrush, dateRect, datesFormat);

					if ((e.Bounds.Bottom - dateRect.Bottom) >= datesFont.Height)
					{
						dateRect.Offset(0, datesFont.Height);

						if (isLastWriteTimeAvailable)
							e.Graphics.DrawString(String.Format(MenuManagerWindowsControlsStrings.ReportLastWriteTimeFormat, itemToDraw.LastWriteTime.ToString()), datesFont, datesColorBrush, dateRect, datesFormat);
					}
				}
				datesFont.Dispose();
			}	
		}

		#endregion
		
		#region CommandsListBox public properties
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlParser MenuXmlParser { get { return menuXmlParser; } set { menuXmlParser = value; } }
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public IPathFinder PathFinder { get { return pathFinder; } set { pathFinder = value; } }

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlNode CurrentMenuNode
		{
			get { return currentMenuNode; }
			set 
			{
				if (value != null && value.IsMenu)
				{
					currentMenuNode = value;
					parentMenuIndependent = false;
				}
				else
					currentMenuNode = null;

				LoadCurrentMenuCommands();
			}
		}

		//---------------------------------------------------------------------------
		[Browsable(false)]
		public CommandListBoxItem SelectedCommand
		{
			get { return (this.SelectedIndex >= 0 && this.SelectedIndex < this.ItemsCount) ? (CommandListBoxItem)this.SelectedItem : null; }
			set 
			{
				this.SelectedItem = value;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		[Localizable(false)]
		public new System.Windows.Forms.DrawMode DrawMode 
		{
			get { return DrawMode.OwnerDrawVariable; }
			set { base.DrawMode = DrawMode.OwnerDrawVariable; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		[Localizable(false)]
		public new System.Windows.Forms.SelectionMode SelectionMode 
		{
			get { return SelectionMode.One; }
			set { base.SelectionMode = SelectionMode.One; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		[Localizable(false)]
		public new int ItemHeight 
		{
			get { return defaultItemHeight; }
			set { base.ItemHeight = defaultItemHeight; }
		}
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public new bool Sorted { get { return false;} set { base.Sorted = false;} }

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		new public CommandListBoxItemCollection Items
		{
			get { return listItems; }
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public int ItemsCount
		{
			get { return base.Items.Count; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public IEnumerator Enumerator
		{
			get { return base.Items.GetEnumerator(); }
		}

		//---------------------------------------------------------------------------
		public bool ShowStateImages { get { return showStateImages; } set { showStateImages = value; } }

		//---------------------------------------------------------------------------
		public bool ParentMenuIndependent  { get { return parentMenuIndependent; } set { parentMenuIndependent = value; } }

		//---------------------------------------------------------------------------
		public System.Drawing.Color CurrentUserCommandForeColor	{ get { return currentUserCommandForeColor; } set { currentUserCommandForeColor = value; } }

		//---------------------------------------------------------------------------
		public System.Drawing.Color AllUsersCommandForeColor { get { return allUsersCommandForeColor; } set { allUsersCommandForeColor = value; } }

		//---------------------------------------------------------------------------
		public System.Drawing.Color DescriptionsForeColor { get { return descriptionsForeColor; } set { descriptionsForeColor = value; } }
		//---------------------------------------------------------------------------
		public System.Drawing.Color ReportDatesForeColor { get { return reportDatesForeColor; } set { reportDatesForeColor = value; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public bool IsRefreshSuspended { get { return isRefreshSuspended; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		[Browsable(false)]
		public int ShowFlags { get { return (int)commandTypesToShow; } }
		
		//-------------------------------------------------------------------------------------
		public bool ShowDocuments
		{
			get
			{
				return ((commandTypesToShow & CommandTypesToShow.Documents) == CommandTypesToShow.Documents);
			}
			set
			{
				SetShowFlag(CommandTypesToShow.Documents, value);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool ShowReports
		{
			get
			{
				return ((commandTypesToShow & CommandTypesToShow.Reports) == CommandTypesToShow.Reports);
			}
			set
			{
				SetShowFlag(CommandTypesToShow.Reports, value);
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowBatches
		{
			get
			{
				return ((commandTypesToShow & CommandTypesToShow.Batches) == CommandTypesToShow.Batches);
			}
			set
			{
				SetShowFlag(CommandTypesToShow.Batches, value);
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowFunctions
		{
			get
			{
				return ((commandTypesToShow & CommandTypesToShow.Functions) == CommandTypesToShow.Functions);
			}
			set
			{
				SetShowFlag(CommandTypesToShow.Functions, value);
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowExecutables
		{
			get
			{
				return ((commandTypesToShow & CommandTypesToShow.Executables) == CommandTypesToShow.Executables);
			}
			set
			{
				SetShowFlag(CommandTypesToShow.Executables, value);
			}
		}
		
		//-------------------------------------------------------------------------------------
		public bool ShowTexts
		{
			get
			{
				return ((commandTypesToShow & CommandTypesToShow.Texts) == CommandTypesToShow.Texts);
			}
			set
			{
				SetShowFlag(CommandTypesToShow.Texts, value);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool ShowOfficeItems
		{
			get
			{
				return ((commandTypesToShow & CommandTypesToShow.OfficeItems) == CommandTypesToShow.OfficeItems);
			}
			set
			{
				SetShowFlag(CommandTypesToShow.OfficeItems, value);
			}
		}

		//-------------------------------------------------------------------------------------
		public bool ShowDescriptions 
		{
			get { return showDescriptions; } 
			set 
			{
				showDescriptions = value; 
				RefreshCommandsList();
			} 
		}

		//-------------------------------------------------------------------------------------
		public bool ShowReportsDates
		{
			get { return showReportsDates; } 
			set 
			{
				showReportsDates = value; 
				RefreshCommandsList();
			} 
		}
		
		#endregion

		#region CommandsListBox private methods

		#region CommandsListBox private initializing methods

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AssignImages()
		{
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultDocumentImage.gif");			// 0
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultReportImage.gif");				// 1
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultBatchImage.gif");				// 2
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultFunctionImage.gif");			// 3
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultTextImage.gif");				// 4
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultExecutableImage.gif");			// 5
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultExternalItemImage.gif");		// 6
			AddBitmapFromCurrentAssemblyToImageList("EnhancedWizardBatchImage.gif");				// 7
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultDocumentFunctionImage.gif");	// 8
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultReportFunctionImage.gif");		// 9
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultBatchFunctionImage.gif");		// 10
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultTextFunctionImage.gif");		// 11
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultExecutableFunctionImage.gif");	// 12
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultExcelItemImage.gif");			// 13
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultExcelDocumentImage.gif");		// 14
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultExcelTemplateImage.gif");		// 15
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultWordItemImage.gif");			// 16
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultWordDocumentImage.gif");		// 17
			AddBitmapFromCurrentAssemblyToImageList("EnhancedDefaultWordTemplateImage.gif");		// 18
			AddBitmapFromCurrentAssemblyToImageList("EnhancedAllUsersReportImage.gif");				// 19
			AddBitmapFromCurrentAssemblyToImageList("EnhancedAllUsersExcelDocumentImage.gif");		// 20
			AddBitmapFromCurrentAssemblyToImageList("EnhancedAllUsersExcelTemplateImage.gif");		// 21
			AddBitmapFromCurrentAssemblyToImageList("EnhancedAllUsersWordDocumentImage.gif");		// 22
			AddBitmapFromCurrentAssemblyToImageList("EnhancedAllUsersWordTemplateImage.gif");		// 23
			AddBitmapFromCurrentAssemblyToImageList("EnhancedCurrentUserReportImage.gif");			// 26
			AddBitmapFromCurrentAssemblyToImageList("EnhancedCurrentUserExcelDocumentImage.gif");	// 27
			AddBitmapFromCurrentAssemblyToImageList("EnhancedCurrentUserExcelTemplateImage.gif");	// 28
			AddBitmapFromCurrentAssemblyToImageList("EnhancedCurrentUserWordDocumentImage.gif");	// 29
			AddBitmapFromCurrentAssemblyToImageList("EnhancedCurrentUserWordTemplateImage.gif");	// 30
				
			AddBitmapFromCurrentAssemblyToStateImageList("DummyState.bmp");						// 0
			AddBitmapFromCurrentAssemblyToStateImageList("Traced.bmp");							// 1
			AddBitmapFromCurrentAssemblyToStateImageList("Protected.bmp");						// 2
			AddBitmapFromCurrentAssemblyToStateImageList("ProtectedAndTraced.bmp");				// 3
			AddBitmapFromCurrentAssemblyToStateImageList("AllDescendantsProtected.bmp");		// 4
			AddBitmapFromCurrentAssemblyToStateImageList("AtLeastOneDescendantProtected.bmp");	// 5
		}
		
		//---------------------------------------------------------------------------
		private System.Drawing.Image GetResizedImage(Image imageToResize, Size newSize)
		{
			if (imageToResize.Width == newSize.Width && imageToResize.Height == newSize.Height)
				return imageToResize;

			Bitmap resizedBitmap = new Bitmap(newSize.Width, newSize.Height, PixelFormat.Format24bppRgb);
				
			//Create a graphics object attached to the new bitmap
			Graphics bitmapGraphics = Graphics.FromImage(resizedBitmap);	
			
			bitmapGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			bitmapGraphics.ScaleTransform(((float)newSize.Width/(float)imageToResize.Width), ((float)newSize.Height/(float)imageToResize.Height));
			
			bitmapGraphics.Clear(this.BackColor);

			bitmapGraphics.DrawImage(imageToResize, 0, 0, imageToResize.Width, imageToResize.Height);

			bitmapGraphics.Dispose();

			return resizedBitmap;
		}

		//---------------------------------------------------------------------------
		private int AddImageToImageList(System.Windows.Forms.ImageList aImageList, Image imageToAdd)
		{
			if (aImageList == null || imageToAdd == null)
				return -1;
						
			if (imageToAdd.Width != aImageList.ImageSize.Width || imageToAdd.Height != aImageList.ImageSize.Height)
				aImageList.Images.Add(GetResizedImage(imageToAdd, aImageList.ImageSize));
			else		
				aImageList.Images.Add(imageToAdd);
											
			return aImageList.Images.Count-1;
		}

		//---------------------------------------------------------------------------
		private int AddImageToImageList(System.Windows.Forms.ImageList imgList, string resourceName)
		{
			if (imgList == null || resourceName == null || resourceName.Length == 0)
				return -1;
						
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			if (imageStream == null)
			{
				Debug.Fail("CommandsListBox.AddImageToImageList Error: Image not found");
				return -1;
			}
			Image image = Image.FromStream(imageStream);
					
			return AddImageToImageList(imgList, Image.FromStream(imageStream));
		}

		//---------------------------------------------------------------------------
		private int AddBitmapFromCurrentAssemblyToImageList(string resourceName)
		{
			return 	AddImageToImageList("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps." + resourceName);
		}

		//---------------------------------------------------------------------------
		private int AddBitmapFromCurrentAssemblyToStateImageList(string resourceName)
		{
			return AddImageToStateImageList("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps." + resourceName);
		}
		
		//---------------------------------------------------------------------------
		private int FindCommandImageIndex(string imageFile)
		{
			if 
				(
				commandsImagesInfo == null ||
				imageFile == null ||
				imageFile.Length == 0
				)
				return -1;

			int commandImageIndex = commandsImagesInfo.FindImage(imageFile);
			if (commandImageIndex != -1 && commandsImagesInfo[commandImageIndex] != null && commandsImagesInfo[commandImageIndex].ListIndex != -1)
				return commandsImagesInfo[commandImageIndex].ListIndex;
			
			return -1;
		}

		//---------------------------------------------------------------------------
		private int AddCommandImage(string imageFile)
		{
			if 
				(
				imageFile == null ||
				imageFile.Length == 0
				)
				return -1;

			if (commandsImagesInfo == null)
				commandsImagesInfo = new CommandsImageInfos();

			Image commandImage = ImagesHelper.LoadImageWithoutLockFile(imageFile);
			if (commandImage == null)
				return -1;

			int imageListIndex = AddImageToImageList(commandImage);
			commandsImagesInfo.AddCommandImageInfo(imageFile, imageListIndex);

			return imageListIndex;
		}

		#endregion

		//---------------------------------------------------------------------------
		private void LoadCurrentMenuCommands()
		{
			MenuXmlNode currentlySelectedCommand = (this.SelectedCommand != null) ? this.SelectedCommand.Node : null;

			if (parentMenuIndependent)
			{
				if (this.Items.Count == 0)
					return;

				ArrayList listedCommands = new ArrayList();
				listedCommands.AddRange(this.Items);
				
				this.Items.Clear();
	
				this.BeginUpdate();

				foreach (CommandListBoxItem commandItem  in listedCommands)
					AddMenuCommand(commandItem.Node, commandItem.Tag);

				this.EndUpdate();
			}
			else
			{
				this.Items.Clear();

				if (currentMenuNode == null)
					return;
		
				ArrayList commandItems = currentMenuNode.CommandItems;

				if (commandItems == null || commandItems.Count == 0)
					return;

				this.BeginUpdate();

				foreach (MenuXmlNode cmdNode  in commandItems)
					AddMenuCommand(cmdNode);

				this.EndUpdate();
			}

			SelectedCommand = FindItem(currentlySelectedCommand);
		}

		//-------------------------------------------------------------------------------------
		private void SetShowFlag(CommandTypesToShow aType, bool valueToset)
		{
			CommandTypesToShow currentFlags = commandTypesToShow;

			if (valueToset)
				currentFlags |= aType;
			else
				currentFlags &= ~aType;

			if (currentFlags != commandTypesToShow)
			{
				commandTypesToShow = currentFlags;
				RefreshCommandsList();

				if (ShowFlagsChanged != null)
					ShowFlagsChanged(this, null);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private bool IsCommandToShow(MenuXmlNode aCommandNode)
		{
			return (
				aCommandNode != null &&
				aCommandNode.IsCommand &&
				(
				(aCommandNode.IsRunDocument		&& this.ShowDocuments) ||
				(aCommandNode.IsRunReport		&& this.ShowReports) ||
				(aCommandNode.IsRunBatch		&& this.ShowBatches) ||
				(aCommandNode.IsRunFunction		&& this.ShowFunctions) ||
				(aCommandNode.IsRunExecutable	&& this.ShowExecutables) ||
				(aCommandNode.IsRunText			&& this.ShowTexts) ||
				(aCommandNode.IsOfficeItem		&& this.ShowOfficeItems)
				)
				);
		}
		
		//---------------------------------------------------------------------------
		private void RefreshCommandsList()
		{
			if (isRefreshSuspended)
				return;
	
			LoadCurrentMenuCommands();
		}

		#endregion

		#region CommandsListBox public methods

		//---------------------------------------------------------------------------
		public void SuspendRefresh()
		{
			isRefreshSuspended = true;
		}

		//---------------------------------------------------------------------------
		public void ResumeRefresh()
		{
			isRefreshSuspended = false;
			RefreshCommandsList();
		}

		//---------------------------------------------------------------------------
		public int AddMenuCommand(MenuXmlNode aCommandNode, object aItemTag)
		{
			if 
				(
				aCommandNode == null || 
				!aCommandNode.IsCommand || 
				!IsCommandToShow(aCommandNode)
				)
				return -1;
			
			CommandListBoxItem commandItem = new CommandListBoxItem(aCommandNode, this);
			commandItem.Tag = aItemTag;
			return this.Items.Add(commandItem);
		}

		//---------------------------------------------------------------------------
		public int AddMenuCommand(MenuXmlNode aCommandNode)
		{
			return AddMenuCommand(aCommandNode, null);
		}
		
		//---------------------------------------------------------------------------
		public CommandListBoxItem FindItem(MenuXmlNode aNodeToFind)
		{
			if 
				(
				aNodeToFind == null || 
				!aNodeToFind.IsCommand ||
				Items == null ||
				ItemsCount == 0
				)
				return null;

			foreach(CommandListBoxItem listItem in Items)
			{
				if (listItem == null || listItem.Node == null || !listItem.Node.IsCommand)
					continue;

				if (aNodeToFind.IsSameCommandAs(listItem.Node))
					return listItem;
			}

			return null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetElementAt(int index, CommandListBoxItem item)
		{
			if (base.Items == null || index < 0 || index >= ItemsCount)
				return;

			if (item == null || !IsCommandToShow(item.Node))
				return;
			
			base.Items[index] = item;
		
			if (item != null)
				item.AdjustGraphics();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandListBoxItem GetElementAt(int index)
		{
			if (index < 0 || index >= ItemsCount)
			{
				Debug.Fail("Error in CommandsListBox.GetElementAt");
				return null;
			}

			return (CommandListBoxItem)base.Items[index];
		}

		//---------------------------------------------------------------------------
		public CommandListBoxItem GetItemAt(int x, int y)
		{
			if (base.Items == null || base.Items.Count == 0)
				return null;

			for (int i = 0; i < base.Items.Count; i++)
			{
				System.Drawing.Rectangle itemRect = GetItemRectangle(i);
				if (itemRect.Contains(x, y))
					return GetElementAt(i);
			}

			return null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertItemAt(int index, CommandListBoxItem item)
		{
			if (base.Items == null)
				return -1;

			if (item == null || !IsCommandToShow(item.Node))
				return -1;

			base.Items.Insert(index, item);
			if (item != null)
				item.AdjustGraphics();

			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddItem(CommandListBoxItem item)
		{
			if (base.Items == null)
				return -1;
	
			if (item == null || !IsCommandToShow(item.Node))
				return -1;

			int addedItemIndex = base.Items.Add(item);
				
			item.AdjustGraphics();
			
			return addedItemIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveItemAt(int index)
		{
			if (base.Items == null)
				return;

			base.Items.RemoveAt(index);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void ClearItems()
		{
			if (base.Items == null)
				return;

			base.Items.Clear();
		}
		
		//---------------------------------------------------------------------------
		public int AddImageToImageList(Image imageToAdd)
		{
			return AddImageToImageList(typeImageList, imageToAdd);
		}

		//---------------------------------------------------------------------------
		public int AddImageToImageList(string resourceName)
		{
			return 	AddImageToImageList(typeImageList, resourceName);
		}
		
		//---------------------------------------------------------------------------
		public int AddImageToStateImageList(Image imageToAdd)
		{
			return AddImageToImageList(stateImageList, imageToAdd);
		}

		//---------------------------------------------------------------------------
		public int AddImageToStateImageList(string resourceName)
		{
			return 	AddImageToImageList(stateImageList, resourceName);
		}
			
		//---------------------------------------------------------------------------
		public int GetCommandImageIdx(MenuXmlNode aNode)
		{
			if (aNode == null)
				return -1;

			if (aNode.IsExternalItem)
			{
				int index = aNode.ExternalItemImageIndex;
				if (index >= 0)
					return index;
			}

			if (!aNode.IsCommand)
				return -1;
		
			string currentLanguage = (menuXmlParser != null && menuXmlParser.LoginManager != null && menuXmlParser.LoginManager.LoginManagerState == LoginManagerState.Logged) ? 
				this.menuXmlParser.LoginManager.PreferredLanguage : 
				String.Empty;

			if (pathFinder != null)
				MenuInfo.SetMenuCommandOrigin(pathFinder, aNode, currentLanguage);
			
			string differentCommandImageSpecified = aNode.DifferentCommandImage;
			if (differentCommandImageSpecified != null && differentCommandImageSpecified.Length > 0)
				return GetCommandImageIdx(new MenuXmlNode.MenuXmlNodeType(differentCommandImageSpecified));

			if (menuXmlParser != null)
			{
				string imageFile = menuXmlParser.GetCommandImageFileName(aNode.GetApplicationName(),aNode.ItemObject);
				if (imageFile != null && imageFile.Length > 0)
				{
					int imageFileIndex = FindCommandImageIndex(imageFile);
					if (imageFileIndex == -1)
						imageFileIndex = AddCommandImage(imageFile);
					if (imageFileIndex != -1)
						return imageFileIndex;
				}
			}

			return GetCommandImageIdx(aNode.Type, aNode.CommandSubType, aNode.GetOfficeApplication(), aNode.CommandOrigin);
		}

		#region CommandsListBox static public methods

		//---------------------------------------------------------------------------
		public static int GetCommandImageIdx(MenuXmlNode.MenuXmlNodeType aNodeType)
		{
			return GetCommandImageIdx(aNodeType, null, CommandOrigin.Unknown);
		}
		
		//---------------------------------------------------------------------------
		public static int GetCommandImageIdx(MenuXmlNode.MenuXmlNodeType aNodeType, CommandOrigin commandOrigin)
		{
			return GetCommandImageIdx(aNodeType, null, commandOrigin);
		}
		
		//---------------------------------------------------------------------------
		public static int GetCommandImageIdx(MenuXmlNode.MenuXmlNodeType aNodeType, MenuXmlNode.MenuXmlNodeCommandSubType	commandSubType, CommandOrigin commandOrigin)
		{
			return GetCommandImageIdx(aNodeType, commandSubType, MenuXmlNode.OfficeItemApplication.Undefined, commandOrigin);
		}

		//---------------------------------------------------------------------------
		public static int GetCommandImageIdx(
			MenuXmlNode.MenuXmlNodeType				aNodeType, 
			MenuXmlNode.MenuXmlNodeCommandSubType	commandSubType,
			MenuXmlNode.OfficeItemApplication		application,
			CommandOrigin				commandOrigin
			)
		{
			if (!aNodeType.IsCommand)
				return -1;

			if (aNodeType.IsRunDocument)
				return GetRunDocumentDefaultImageIndex;
			
			if (aNodeType.IsRunReport)
			{
				if (commandOrigin == CommandOrigin.CustomAllUsers)
					return GetRunAllUsersReportDefaultImageIndex;

				if (commandOrigin == CommandOrigin.CustomCurrentUser)
					return GetRunCurrentUserReportDefaultImageIndex;

				return GetRunReportDefaultImageIndex;
			}
			if (aNodeType.IsRunBatch)
			{
				if (commandSubType != null && commandSubType.IsWizardBatch)
					return GetWizardBatchDefaultImageIndex;
				return GetRunBatchDefaultImageIndex;
			}

			if (aNodeType.IsRunFunction)
			{
				if (commandSubType != null)
				{
					if (commandSubType.IsRunDocumentFunction)
						return GetRunDocumentFunctionDefaultImageIndex;
					if (commandSubType.IsRunReportFunction)
						return GetRunReportFunctionDefaultImageIndex;
					if (commandSubType.IsRunBatchFunction)
						return GetRunBatchFunctionDefaultImageIndex;
					if (commandSubType.IsRunTextFunction)
						return GetRunTextFunctionDefaultImageIndex;
					if (commandSubType.IsRunExecutableFunction)
						return GetRunExecutableFunctionDefaultImageIndex;
				}
				return GetRunFunctionDefaultImageIndex;
			}

			if (aNodeType.IsRunText)
				return GetRunTextDefaultImageIndex;
			
			if (aNodeType.IsRunExecutable)
				return GetRunExeDefaultImageIndex;
			
			if (aNodeType.IsExternalItem)
				return GetDefaultExternalItemDefaultImageIndex;

			if (aNodeType.IsOfficeItem)
			{
				if (application == MenuXmlNode.OfficeItemApplication.Undefined)
					return -1;

				if (application == MenuXmlNode.OfficeItemApplication.Excel)
				{
					if (commandSubType != null)
					{
						if (commandSubType.IsOfficeDocument)
						{
							if (commandOrigin == CommandOrigin.CustomAllUsers)
								return GetRunAllUsersExcelDocumentDefaultImageIndex;

							if (commandOrigin == CommandOrigin.CustomCurrentUser)
								return GetRunCurrentUserExcelDocumentDefaultImageIndex;

							return GetRunExcelDocumentDefaultImageIndex;
						}
						if (commandSubType.IsOfficeTemplate)
						{
							if (commandOrigin == CommandOrigin.CustomAllUsers)
								return GetRunAllUsersExcelTemplateDefaultImageIndex;

							if (commandOrigin == CommandOrigin.CustomCurrentUser)
								return GetRunCurrentUserExcelTemplateDefaultImageIndex;

							return GetRunExcelTemplateDefaultImageIndex;
						}
					}
					return GetDefaultExcelItemDefaultImageIndex;
				}
				if (application == MenuXmlNode.OfficeItemApplication.Word)
				{
					if (commandSubType != null)
					{
						if (commandSubType.IsOfficeDocument)
						{
							if (commandOrigin == CommandOrigin.CustomAllUsers)
								return GetRunAllUsersWordDocumentDefaultImageIndex;

							if (commandOrigin == CommandOrigin.CustomCurrentUser)
								return GetRunCurrentUserWordDocumentDefaultImageIndex;

							return GetRunWordDocumentDefaultImageIndex;
						}
						if (commandSubType.IsOfficeTemplate)
						{
							if (commandOrigin == CommandOrigin.CustomAllUsers)
								return GetRunAllUsersWordTemplateDefaultImageIndex;

							if (commandOrigin == CommandOrigin.CustomCurrentUser)
								return GetRunCurrentUserWordTemplateDefaultImageIndex;

							return GetRunWordTemplateDefaultImageIndex;
						}
					}
					return GetDefaultWordItemDefaultImageIndex;
				}
			}

			return -1;
		}
		
		//---------------------------------------------------------------------------
		public static int GetStateImageIdx(MenuXmlNode aMenuXmlNode)
		{
			if (aMenuXmlNode.ProtectedState)
			{
				if (aMenuXmlNode.TracedState)
					return GetProtectedAndTracedStateImageIndex;
				return GetProtectedStateImageIndex;
			}
			if (aMenuXmlNode.TracedState)
				return GetTracedStateImageIndex;
			
			if (aMenuXmlNode.HasAllCommandDescendantsInProtectedState)
				return GetAllDescendantsProtectedStateImageIndex;
				
			if (aMenuXmlNode.HasAtLeastOneCommandDescendantInProtectedState)
				return GetAtLeastOneDescendantProtectedStateImageIndex;

			return GetDummyStateImageIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static int GetRunDocumentDefaultImageIndex					{ get { return 0; } }
		public static int GetRunReportDefaultImageIndex						{ get { return 1; } }
		public static int GetRunBatchDefaultImageIndex						{ get { return 2; } }
		public static int GetRunFunctionDefaultImageIndex					{ get { return 3; } }
		public static int GetRunTextDefaultImageIndex						{ get { return 4; } }
		public static int GetRunExeDefaultImageIndex						{ get { return 5; } }
		public static int GetDefaultExternalItemDefaultImageIndex			{ get { return 6; } }
		public static int GetWizardBatchDefaultImageIndex					{ get { return 7; } }
		public static int GetRunDocumentFunctionDefaultImageIndex			{ get { return 8; } }
		public static int GetRunReportFunctionDefaultImageIndex				{ get { return 9; } }
		public static int GetRunBatchFunctionDefaultImageIndex				{ get { return 10; } }
		public static int GetRunTextFunctionDefaultImageIndex				{ get { return 11; } }
		public static int GetRunExecutableFunctionDefaultImageIndex			{ get { return 12; } }
		public static int GetDefaultExcelItemDefaultImageIndex				{ get { return 13; } }
		public static int GetRunExcelDocumentDefaultImageIndex				{ get { return 14; } }
		public static int GetRunExcelTemplateDefaultImageIndex				{ get { return 15; } }
		public static int GetDefaultWordItemDefaultImageIndex				{ get { return 16; } }
		public static int GetRunWordDocumentDefaultImageIndex				{ get { return 17; } }
		public static int GetRunWordTemplateDefaultImageIndex				{ get { return 18; } }
		public static int GetRunAllUsersReportDefaultImageIndex				{ get { return 19; } }
		public static int GetRunAllUsersExcelDocumentDefaultImageIndex		{ get { return 20; } }
		public static int GetRunAllUsersExcelTemplateDefaultImageIndex		{ get { return 21; } }
		public static int GetRunAllUsersWordDocumentDefaultImageIndex		{ get { return 22; } }
		public static int GetRunAllUsersWordTemplateDefaultImageIndex		{ get { return 23; } }
		public static int GetRunCurrentUserReportDefaultImageIndex			{ get { return 24; } }
		public static int GetRunCurrentUserExcelDocumentDefaultImageIndex	{ get { return 25; } }
		public static int GetRunCurrentUserExcelTemplateDefaultImageIndex	{ get { return 26; } }
		public static int GetRunCurrentUserWordDocumentDefaultImageIndex	{ get { return 27; } }
		public static int GetRunCurrentUserWordTemplateDefaultImageIndex	{ get { return 28; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public static int GetDummyStateImageIndex							{ get { return 1; } }
		public static int GetTracedStateImageIndex							{ get { return 2; } }
		public static int GetProtectedStateImageIndex						{ get { return 3; } }
		public static int GetProtectedAndTracedStateImageIndex				{ get { return 4; } }
		public static int GetAllDescendantsProtectedStateImageIndex			{ get { return 5; } }
		public static int GetAtLeastOneDescendantProtectedStateImageIndex	{ get { return 6; } }

		#endregion

		#endregion

		#region CommandsListBox event handlers

		//-------------------------------------------------------------------------------------
		private void CommandsListView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (SelectedCommandChanged != null)
				SelectedCommandChanged(this, e);
		}

		#endregion
	}

	#endregion

	#region CommandListBoxItem class

	public class CommandListBoxItem
	{
		private MenuXmlNode				node;
		private CommandsListBox			ownerListBox = null;
		private System.Drawing.Color	titleColor = SystemColors.WindowText;
		private int						imageIndex = -1;
		private int						stateImageIndex = -1;
		private object					tag = null;
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandListBoxItem(MenuXmlNode aNode, CommandsListBox aOwnerListBox, int aImageIndex, int aStateImageIndex)
		{			
			ownerListBox = aOwnerListBox;

			Node = aNode;

			AdjustGraphics(false);

			if (aImageIndex != -1)
				imageIndex = aImageIndex;

			if (aStateImageIndex != -1)
				stateImageIndex = aStateImageIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandListBoxItem(MenuXmlNode aNode, CommandsListBox aOwnerListBox) : this(aNode, aOwnerListBox, -1, -1)
		{
			AdjustImageIndex();
			AdjustStateImageIndex();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandListBoxItem(MenuXmlNode aNode) : this(aNode, null)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandListBoxItem() : this(null)
		{
		}

		#region CommandListBoxItem public properties

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode Node
		{
			get { return node; }
			set 
			{
				if (value != null && (value.IsCommand || value.IsMenu))
				{
					node = value;

					AdjustDescriptions();

					AdjustGraphics();
				}
				else
				{
					Debug.Assert(value == null || !value.IsCommand, "Set CommandListBoxItem.Node property error: invalid node type");
					node = null;
				}
			}
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsRunDocument
		{
			get
			{
				return (node != null  && node.IsRunDocument);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsRunReport
		{
			get
			{
				return (node != null  && node.IsRunReport);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsRunBatch
		{
			get
			{
				return (node != null  && node.IsRunBatch);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsRunFunction
		{
			get
			{
				return (node != null  && node.IsRunFunction);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string Title
		{
			get
			{
				if (node == null)
					return String.Empty;

				return node.Title;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string Description
		{
			get
			{
				if (node == null)
					return String.Empty;

				return node.ExternalDescription;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public DateTime CreationTime
		{
			get
			{
				if (!IsRunReport)
					return DateTime.MinValue;

				return node.ReportFileCreationTime;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public DateTime LastWriteTime
		{
			get
			{
				if (!IsRunReport)
					return DateTime.MinValue;

				return node.ReportFileLastWriteTime;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandsListBox ListBox { get {return ownerListBox; } }
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public System.Drawing.Color TitleColor { get { return titleColor; } }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public int ImageIndex  { get { return imageIndex; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public int StateImageIndex  { get { return stateImageIndex; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public object Tag  { get { return tag; } set { tag = value; } }
	
		#endregion

		#region CommandListBoxItem overridden methods

		//--------------------------------------------------------------------------------------------------------------------------------
		public override string ToString()
		{
			return this.Title;
		}
		
		#endregion

		#region CommandListBoxItem public methods

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustDescriptions()
		{
			if 
				(
				node == null ||
				ownerListBox == null ||
				ownerListBox.PathFinder == null
				)
				return;
		
			// Se il nodo che viene caricato è riferito ad un lancio di un comando
			// occorre impostare la sua descrizione (se non è mai stata letta essa verrà
			// definitivamente impostata una prima volta e poi riutilizzata senza
			// doverla rileggere successivamente)
			MenuInfo.SetExternalDescription(ownerListBox.PathFinder, node);

			if (IsRunReport)
				MenuInfo.SetReportFileDateTimes(ownerListBox.PathFinder, node);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustGraphics(bool adjustImageIndexes)
		{
			if (ownerListBox == null)
				return;

			if (node != null && node.IsCommand)
			{
				string currentLanguage = (ownerListBox.MenuXmlParser != null && ownerListBox.MenuXmlParser.LoginManager != null && ownerListBox.MenuXmlParser.LoginManager.LoginManagerState == LoginManagerState.Logged) ? 
					ownerListBox.MenuXmlParser.LoginManager.PreferredLanguage : 
					String.Empty;

				// Occorre ricavare l'origine del comando (se non è mai stata letta essa
				// verrà definitivamente impostata una prima volta e poi riutilizzata senza
				// doverla rileggere successivamente
				IPathFinder menuPathFinder = ownerListBox.PathFinder;
				if (menuPathFinder != null)
					MenuInfo.SetMenuCommandOrigin(menuPathFinder, node, currentLanguage);
			
				switch(node.CommandOrigin)
				{
					case CommandOrigin.CustomCurrentUser:
						titleColor = ownerListBox.CurrentUserCommandForeColor;
						break;
					case CommandOrigin.CustomAllUsers:
						titleColor = ownerListBox.AllUsersCommandForeColor;
						break;
					default:
						titleColor = ownerListBox.ForeColor;
						break;
				}
			}
	
			if (adjustImageIndexes)
			{
				AdjustImageIndex();
				AdjustStateImageIndex();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustGraphics()
		{
			AdjustGraphics(true);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustImageIndex()
		{
			if (ownerListBox != null)
				imageIndex = ownerListBox.GetCommandImageIdx(this.Node);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustStateImageIndex()
		{
			stateImageIndex = (node != null) ? CommandsListBox.GetStateImageIdx(node) : -1;
		}

		#endregion
	}


	#endregion

	#region CommandListBoxItemCollection class

	//============================================================================
	public class CommandListBoxItemCollection : IList, ICollection, IEnumerable
	{
		private CommandsListBox owner = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandListBoxItemCollection(CommandsListBox aOwnerListBox)
		{
			owner = aOwnerListBox;
		}
        
		#region IList implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set { this[index] = (CommandListBoxItem)value; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			Add((CommandListBoxItem)item);

			return Count;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			Insert(index, (CommandListBoxItem)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion

		#region ICollection implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		void ICollection.CopyTo(Array array, int index) 
		{
			for (IEnumerator e = GetEnumerator(); e.MoveNext();)
				array.SetValue(e.Current, index++);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool ICollection.IsSynchronized 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		object ICollection.SyncRoot 
		{
			get { return this; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Count 
		{
			get 
			{
				return (owner != null) ? owner.ItemsCount : 0;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsReadOnly 
		{
			get { return false; }
		}
			
		#endregion

		//--------------------------------------------------------------------------------------------------------------------------------
		public CommandListBoxItem this[int index]
		{
			get 
			{
				return (owner != null) ? owner.GetElementAt(index) : null;
			}
			set
			{
				if (owner != null)
					owner.SetElementAt(index, value);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator GetEnumerator() 
		{
			return (owner != null) ? owner.Enumerator : null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Contains(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int IndexOf(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Remove(CommandListBoxItem item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Insert(int index, CommandListBoxItem item)
		{
			return (owner != null) ? owner.InsertItemAt(index, item) : -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Add(CommandListBoxItem item)
		{
			return (owner != null) ? owner.AddItem(item) : -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AddRange(CommandListBoxItem[] items)
		{
			for(IEnumerator e = items.GetEnumerator(); e.MoveNext();)
				Add((CommandListBoxItem)e.Current);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			if (owner != null && owner.Items != null)
				owner.ClearItems();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (owner != null)
				owner.RemoveItemAt(index);
		}
		
	}
	
	#endregion

}
