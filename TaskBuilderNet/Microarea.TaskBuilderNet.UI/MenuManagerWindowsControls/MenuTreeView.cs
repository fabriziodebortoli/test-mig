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
	/// <summary>
	/// </summary>
	//============================================================================
	public partial class MenuTreeView : System.Windows.Forms.TreeView, IMenuTreeNodesOwner
	{
		private MenuTreeNodeCollection nodes = null;
        
		private bool showStateImages = false;
		private bool hiliteMenusWithMoreTitles = false;

		private MenuXmlParser		menuXmlParser = null;
		private IPathFinder			pathFinder = null;
		private CommandsImageInfos	commandsImagesInfo = null;

		private System.Windows.Forms.ImageList	typeImageList;
		private System.Windows.Forms.ImageList	stateImageList;
		private System.Windows.Forms.ToolTip	CommandToolTip;

		private System.Drawing.Color currentUserCommandForeColor = Color.RoyalBlue;
		private System.Drawing.Color allUsersCommandForeColor = Color.Blue;

			
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public MenuXmlParser MenuXmlParser { get { return menuXmlParser; } set { menuXmlParser = value; } }
		
		//---------------------------------------------------------------------------
		[Browsable(false)]
		public IPathFinder PathFinder { get { return pathFinder; } set { pathFinder = value; } }

		//---------------------------------------------------------------------------
		public System.Drawing.Color CurrentUserCommandForeColor	{ get { return currentUserCommandForeColor; } set { currentUserCommandForeColor = value; } }

		//---------------------------------------------------------------------------
		public System.Drawing.Color AllUsersCommandForeColor { get { return allUsersCommandForeColor; } set { allUsersCommandForeColor = value; } }

		//---------------------------------------------------------------------------
		public MenuTreeView()
		{
			InitializeComponent();

			nodes = new MenuTreeNodeCollection(this);
		
			PathSeparator = MenuXmlNode.ActionMenuPathSeparator;

			AddBitmapFromCurrentAssemblyToImageList("CollapsedMenu.bmp");				// 0
			AddBitmapFromCurrentAssemblyToImageList("ExpandedMenu.bmp");				// 1
			AddBitmapFromCurrentAssemblyToImageList("RunDocument.bmp");					// 2
			AddBitmapFromCurrentAssemblyToImageList("RunReport.bmp");					// 3
			AddBitmapFromCurrentAssemblyToImageList("RunBatch.bmp");					// 4
			AddBitmapFromCurrentAssemblyToImageList("RunFunction.bmp");					// 5
			AddBitmapFromCurrentAssemblyToImageList("RunText.bmp");						// 6
			AddBitmapFromCurrentAssemblyToImageList("RunExe.bmp");						// 7
			AddBitmapFromCurrentAssemblyToImageList("DefaultExternalItem.bmp");			// 8
			AddBitmapFromCurrentAssemblyToImageList("RunWizardBatch.bmp");				// 9
			AddBitmapFromCurrentAssemblyToImageList("RunDocumentFunction.bmp");			// 10
			AddBitmapFromCurrentAssemblyToImageList("RunReportFunction.bmp");			// 11
			AddBitmapFromCurrentAssemblyToImageList("RunBatchFunction.bmp");			// 12
			AddBitmapFromCurrentAssemblyToImageList("RunTextFunction.bmp");				// 13
			AddBitmapFromCurrentAssemblyToImageList("RunExeFunction.bmp");				// 14
			AddBitmapFromCurrentAssemblyToImageList("RunExcel.bmp");					// 15
			AddBitmapFromCurrentAssemblyToImageList("RunExcelDocument.bmp");			// 16
			AddBitmapFromCurrentAssemblyToImageList("RunExcelTemplate.bmp");			// 17
			AddBitmapFromCurrentAssemblyToImageList("RunWord.bmp");						// 18
			AddBitmapFromCurrentAssemblyToImageList("RunWordDocument.bmp");				// 19
			AddBitmapFromCurrentAssemblyToImageList("RunWordTemplate.bmp");				// 20
			AddBitmapFromCurrentAssemblyToImageList("RunAllUsersReport.bmp");			// 21
			AddBitmapFromCurrentAssemblyToImageList("RunAllUsersExcelDocument.bmp");	// 22
			AddBitmapFromCurrentAssemblyToImageList("RunAllUsersExcelTemplate.bmp");	// 23
			AddBitmapFromCurrentAssemblyToImageList("RunAllUsersWordDocument.bmp");		// 24
			AddBitmapFromCurrentAssemblyToImageList("RunAllUsersWordTemplate.bmp");		// 25
			AddBitmapFromCurrentAssemblyToImageList("RunCurrentUserReport.bmp");		// 26
			AddBitmapFromCurrentAssemblyToImageList("RunCurrentUserExcelDocument.bmp");	// 27
			AddBitmapFromCurrentAssemblyToImageList("RunCurrentUserExcelTemplate.bmp");	// 28
			AddBitmapFromCurrentAssemblyToImageList("RunCurrentUserWordDocument.bmp");	// 29
			AddBitmapFromCurrentAssemblyToImageList("RunCurrentUserWordTemplate.bmp");	// 30

			ImageList = typeImageList;											
					
			// Devo comunque inserire nella lista delle immagini relative allo
			// stato dell'elemento una prima immagine (quella, cioè, con indice
			// uguale a 0) che non verrà mai usata: infatti, l'indice individuato
			// dall'attributo stateImageIndex di un oggetto di classe MenuTreeNode
			// deve essere sempre > 0 perchè usato per settare un certo bit
			// di tvi.state.
			AddBitmapFromCurrentAssemblyToStateImageList("DummyState.bmp");						// 0
			AddBitmapFromCurrentAssemblyToStateImageList("DummyState.bmp");						// 1
			AddBitmapFromCurrentAssemblyToStateImageList("Traced.bmp");							// 2
			AddBitmapFromCurrentAssemblyToStateImageList("Protected.bmp");						// 3
			AddBitmapFromCurrentAssemblyToStateImageList("ProtectedAndTraced.bmp");				// 4
			AddBitmapFromCurrentAssemblyToStateImageList("AllDescendantsProtected.bmp");		// 5
			AddBitmapFromCurrentAssemblyToStateImageList("AtLeastOneDescendantProtected.bmp");	// 6
			AddBitmapFromCurrentAssemblyToStateImageList("DeniedObject.gif");					// 7
			AddBitmapFromCurrentAssemblyToStateImageList("AllowedObject.gif");					// 8
			AddBitmapFromCurrentAssemblyToStateImageList("PartiallyDeniedObject.gif");			// 9
            AddBitmapFromCurrentAssemblyToStateImageList("UnattendedAllowed.gif");			    // 10
        }
			
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnBeforeCollapse(System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			base.OnBeforeCollapse(e);

			if (e.Node != null && e.Node is MenuTreeNode && ((MenuTreeNode)e.Node).Node != null && ((MenuTreeNode)e.Node).Node.IsMenu)
			{
				e.Node.ImageIndex = GetCollapsedMenuDefaultImageIndex;
				e.Node.SelectedImageIndex = GetExpandedMenuDefaultImageIndex;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnBeforeExpand(System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			base.OnBeforeExpand(e);

			if (e.Node != null && e.Node is MenuTreeNode && ((MenuTreeNode)e.Node).Node != null && ((MenuTreeNode)e.Node).Node.IsMenu)
			{
				e.Node.ImageIndex = e.Node.SelectedImageIndex = GetExpandedMenuDefaultImageIndex;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnAfterSelect(System.Windows.Forms.TreeViewEventArgs e)
		{
			if (CommandToolTip != null)
			{
				MenuXmlNode selectednode = ((MenuTreeNode)e.Node).Node;
				if (selectednode.IsCommand)
					CommandToolTip.SetToolTip(this, selectednode.Description);
				else
					CommandToolTip.SetToolTip(this, String.Empty);
			}

			base.OnAfterSelect(e);
		}

		//---------------------------------------------------------------------------
		private void AdjustNodesGraphics(bool adjustImageIndexes)
		{
			if (this.NodesCount == 0)
				return;

			nodes.AdjustGraphics(adjustImageIndexes);
		}

		//---------------------------------------------------------------------------
		private System.Drawing.Image GetResizedImage(Image imageToResize, Size newSize)
		{
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
			{
				Image resized = GetResizedImage(imageToAdd, aImageList.ImageSize);
				aImageList.Images.Add(resized);
				imageToAdd.Dispose();
			}
			else
			{
				aImageList.Images.Add(imageToAdd);
			}
											
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
				Debug.Fail("MenuTreeView.AddImageToImageList Error: Image not found");
				return -1;
			}
			Image img = Image.FromStream(imageStream, true, true);
			return AddImageToImageList(imgList, img);
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
		private int AddBitmapFromCurrentAssemblyToImageList(string resourceName)
		{
			return 	AddImageToImageList("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps." + resourceName);
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
		private int AddBitmapFromCurrentAssemblyToStateImageList(string resourceName)
		{
			return 	AddImageToStateImageList("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps." + resourceName);
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

			if (!File.Exists(imageFile))
				return -1;

			Image commandImage = ImagesHelper.LoadImageWithoutLockFile(imageFile);
			if (commandImage == null)
				return -1;

			int imageListIndex = AddImageToImageList(commandImage);
			commandsImagesInfo.AddCommandImageInfo(imageFile, imageListIndex);

			return imageListIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetNodeAt(int index, MenuXmlNode node)
		{
			SetElementAt(index, new MenuTreeNode(node));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetElementAt(int index, MenuTreeNode item)
		{
			if (index < 0 || index >= NodesCount)
				return;

			base.Nodes[index] = item;
					
			item.AdjustGraphics();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode GetNodeAt(int index)
		{
			MenuTreeNode item = GetElementAt(index);
			if (item == null)
				return null;

			return item.Node;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode GetMenuTreeNodeAt(int x, int y)
		{
			return (MenuTreeNode)base.GetNodeAt(x, y);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode GetMenuTreeNodeAt(System.Drawing.Point aPoint)
		{
			return (MenuTreeNode)base.GetNodeAt(aPoint);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode GetElementAt(int index)
		{
			if (index < 0 || index >= NodesCount)
			{
				Debug.Fail("Error in MenuTreeView.GetElementAt");
				return null;
			}

			return (MenuTreeNode)base.Nodes[index];
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertNodeAt(int index, MenuXmlNode node)
		{
			if (index < 0)
				return -1;

			return InsertItemAt(index, new MenuTreeNode(node));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertItemAt(int index, MenuTreeNode item)
		{
			base.Nodes.Insert(index, item);
			
			item.AdjustGraphics();
			
			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddNode(MenuXmlNode node)
		{
			return AddItem( new MenuTreeNode(node) );
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddItem(MenuTreeNode item)
		{
			int index = base.Nodes.Add(item);
			
			item.AdjustGraphics();
			
			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveItemAt(int index)
		{
			MenuTreeNode nodeToRemove = GetElementAt(index);
			MenuTreeNode parentOfNodeToRemove = GetElementAt(index).Parent;

			base.Nodes.RemoveAt(index);

			if (parentOfNodeToRemove != null)
				parentOfNodeToRemove.AdjustStateImageIndex();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			if (this.Disposing || this.IsDisposed)
				return;

			base.Nodes.Clear();

			if (CommandToolTip != null)
				CommandToolTip.SetToolTip(this, String.Empty);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode FindMenuTreeNode(MenuXmlNode aMenuNodeToFind)
		{
			return FindMenuTreeNode(aMenuNodeToFind, true);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode FindMenuTreeNode(MenuXmlNode aMenuNodeToFind, bool searchDescendants)
		{
			if (Nodes == null || Nodes.Count == 0)
				return null;

			foreach (MenuTreeNode aTreeNode in Nodes)
			{
				MenuTreeNode nodeFound = aTreeNode.FindMenuTreeNode(aMenuNodeToFind, searchDescendants);
				if (nodeFound != null)
					return nodeFound;
			}
			return null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator Enumerator {	get { return base.Nodes.GetEnumerator(); } }

		//--------------------------------------------------------------------------------------------------------------------------------
		new public MenuTreeNodeCollection Nodes { get { return nodes; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public int NodesCount { get { return base.Nodes.Count; } }

		//---------------------------------------------------------------------------
		public bool ShowStateImages 
		{
			get { return showStateImages; }
			set
			{
                if (this.Disposing || this.IsDisposed || showStateImages == value)
					return;

				showStateImages = value;

                this.StateImageList = showStateImages ? stateImageList : null;

                if (showStateImages && this.Nodes != null && this.Nodes.Count > 0)
                    this.Nodes.AdjustStateImageIndexes();
            }
		}

		//---------------------------------------------------------------------------
		public bool HiliteMenusWithMoreTitles 
		{
			get { return hiliteMenusWithMoreTitles; } 
			set 
			{
				if (hiliteMenusWithMoreTitles == value)
					return;

				hiliteMenusWithMoreTitles = value;

				AdjustNodesGraphics(false);
			} 
		}
		
		//---------------------------------------------------------------------------
		public int GetMenuTreeImageIdx(MenuTreeNode aTreeNode)
		{
			if (aTreeNode == null || aTreeNode.Node == null)
				return -1;

			return GetMenuTreeImageIdx(aTreeNode.Node, aTreeNode.IsExpanded || aTreeNode.IsSelected);
		}

		//---------------------------------------------------------------------------
		public int GetMenuTreeImageIdx(MenuXmlNode aNode, bool isExpandedOrSelected)
		{
			if (aNode == null)
				return -1;

			if (aNode.IsExternalItem)
			{
				int index = aNode.ExternalItemImageIndex;
				if (index >= 0)
					return index;
			}

			if (aNode.IsCommand || aNode.IsShortcut)
			{
				string currentLanguage = (menuXmlParser != null && menuXmlParser.LoginManager != null && menuXmlParser.LoginManager.LoginManagerState == LoginManagerState.Logged) ? 
					this.menuXmlParser.LoginManager.PreferredLanguage : 
					String.Empty;

				if (pathFinder != null)
					MenuInfo.SetMenuCommandOrigin(pathFinder, aNode, currentLanguage);
			}
			
			string differentCommandImageSpecified = aNode.DifferentCommandImage;
			if (differentCommandImageSpecified != null && differentCommandImageSpecified.Length > 0)
				return GetMenuTreeImageIdx(new MenuXmlNode.MenuXmlNodeType(differentCommandImageSpecified), isExpandedOrSelected);

			if (aNode.IsShortcut)
			{
				string shortcutImageLink = aNode.ImageLink;
				if (shortcutImageLink != null && shortcutImageLink.Length > 0)
				{
					string runningPath = (pathFinder != null) 
						? pathFinder.GetInstallationPath()
						: String.Empty;
					if (!Path.IsPathRooted(shortcutImageLink))
					{
						if (runningPath != null && runningPath.Length > 0)
							shortcutImageLink = Path.Combine(runningPath, shortcutImageLink);
					}
					
					if (Path.IsPathRooted(shortcutImageLink))
					{
						int shortcutImageFileIndex = FindCommandImageIndex(shortcutImageLink);
						if (shortcutImageFileIndex == -1)
							shortcutImageFileIndex = AddCommandImage(shortcutImageLink);
						if (shortcutImageFileIndex != -1)
							return shortcutImageFileIndex;
					}
				}
				
				return GetMenuTreeImageIdx(new MenuXmlNode.MenuXmlNodeType(aNode.GetShortcutTypeXmlTag()), new MenuXmlNode.MenuXmlNodeCommandSubType(aNode.GetShortcutCommandSubType()), aNode.GetOfficeApplication(), aNode.CommandOrigin, isExpandedOrSelected);
			}

			if (aNode.IsCommand && menuXmlParser != null)
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

			return GetMenuTreeImageIdx(aNode.Type, aNode.CommandSubType, aNode.GetOfficeApplication(), aNode.CommandOrigin, isExpandedOrSelected);
		}

		//---------------------------------------------------------------------------
		public static int GetMenuTreeImageIdx(MenuXmlNode.MenuXmlNodeType aNodeType, bool isExpandedOrSelected)
		{
			return GetMenuTreeImageIdx(aNodeType, null, CommandOrigin.Unknown, isExpandedOrSelected);
		}
		
		//---------------------------------------------------------------------------
		public static int GetMenuTreeImageIdx(MenuXmlNode.MenuXmlNodeType aNodeType, CommandOrigin commandOrigin, bool isExpandedOrSelected)
		{
			return GetMenuTreeImageIdx(aNodeType, null, commandOrigin, isExpandedOrSelected);
		}
		
		//---------------------------------------------------------------------------
		public static int GetMenuTreeImageIdx(MenuXmlNode.MenuXmlNodeType aNodeType, MenuXmlNode.MenuXmlNodeCommandSubType	commandSubType, CommandOrigin commandOrigin, bool isExpandedOrSelected)
		{
			return GetMenuTreeImageIdx(aNodeType, commandSubType, MenuXmlNode.OfficeItemApplication.Undefined, commandOrigin, isExpandedOrSelected);
		}

		//---------------------------------------------------------------------------
		public static int GetMenuTreeImageIdx(
												MenuXmlNode.MenuXmlNodeType				aNodeType, 
												MenuXmlNode.MenuXmlNodeCommandSubType	commandSubType,
												MenuXmlNode.OfficeItemApplication		application,
												CommandOrigin				commandOrigin,
												bool									isExpandedOrSelected
											)
		{
			if (aNodeType.IsMenu)
				return isExpandedOrSelected ? GetExpandedMenuDefaultImageIndex : GetCollapsedMenuDefaultImageIndex;

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

            if (aMenuXmlNode.AccessDeniedState)
            {
                if (aMenuXmlNode.AccessInUnattendedModeAllowedState)
                    return GetUnattendedModeAllowedStateImageIndex;

                return GetAccessDeniedStateImageIndex;
            }

			if (aMenuXmlNode.AccessAllowedState)
				return GetAccessAllowedStateImageIndex;

			if (aMenuXmlNode.AccessPartiallyAllowedState)
				return GetAccessPartiallyAllowedStateImageIndex;
			
			return GetDummyStateImageIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public static int GetCollapsedMenuDefaultImageIndex				{ get { return 0; } }
		public static int GetExpandedMenuDefaultImageIndex				{ get { return 1; } }
		public static int GetRunDocumentDefaultImageIndex				{ get { return 2; } }
		public static int GetRunReportDefaultImageIndex					{ get { return 3; } }
		public static int GetRunBatchDefaultImageIndex					{ get { return 4; } }
		public static int GetRunFunctionDefaultImageIndex				{ get { return 5; } }
		public static int GetRunTextDefaultImageIndex					{ get { return 6; } }
		public static int GetRunExeDefaultImageIndex					{ get { return 7; } }
		public static int GetDefaultExternalItemDefaultImageIndex		{ get { return 8; } }

		public static int GetWizardBatchDefaultImageIndex				{ get { return 9; } }
		public static int GetRunDocumentFunctionDefaultImageIndex		{ get { return 10; } }
		public static int GetRunReportFunctionDefaultImageIndex			{ get { return 11; } }
		public static int GetRunBatchFunctionDefaultImageIndex			{ get { return 12; } }
		public static int GetRunTextFunctionDefaultImageIndex			{ get { return 13; } }
		public static int GetRunExecutableFunctionDefaultImageIndex		{ get { return 14; } }
		public static int GetDefaultExcelItemDefaultImageIndex			{ get { return 15; } }
		public static int GetRunExcelDocumentDefaultImageIndex			{ get { return 16; } }
		public static int GetRunExcelTemplateDefaultImageIndex			{ get { return 17; } }
		public static int GetDefaultWordItemDefaultImageIndex			{ get { return 18; } }
		public static int GetRunWordDocumentDefaultImageIndex			{ get { return 19; } }
		public static int GetRunWordTemplateDefaultImageIndex			{ get { return 20; } }
		public static int GetRunAllUsersReportDefaultImageIndex			{ get { return 21; } }
		public static int GetRunAllUsersExcelDocumentDefaultImageIndex	{ get { return 22; } }
		public static int GetRunAllUsersExcelTemplateDefaultImageIndex	{ get { return 23; } }
		public static int GetRunAllUsersWordDocumentDefaultImageIndex	{ get { return 24; } }
		public static int GetRunAllUsersWordTemplateDefaultImageIndex	{ get { return 25; } }
		public static int GetRunCurrentUserReportDefaultImageIndex		{ get { return 26; } }
		public static int GetRunCurrentUserExcelDocumentDefaultImageIndex	{ get { return 27; } }
		public static int GetRunCurrentUserExcelTemplateDefaultImageIndex	{ get { return 28; } }
		public static int GetRunCurrentUserWordDocumentDefaultImageIndex	{ get { return 29; } }
		public static int GetRunCurrentUserWordTemplateDefaultImageIndex	{ get { return 30; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public static int GetDummyStateImageIndex							{ get { return 1; } }
		public static int GetTracedStateImageIndex							{ get { return 2; } }
		public static int GetProtectedStateImageIndex						{ get { return 3; } }
		public static int GetProtectedAndTracedStateImageIndex				{ get { return 4; } }
		public static int GetAllDescendantsProtectedStateImageIndex			{ get { return 5; } }
		public static int GetAtLeastOneDescendantProtectedStateImageIndex	{ get { return 6; } }
		public static int GetAccessDeniedStateImageIndex					{ get { return 7; } }
		public static int GetAccessAllowedStateImageIndex					{ get { return 8; } }
		public static int GetAccessPartiallyAllowedStateImageIndex			{ get { return 9; } }
        public static int GetUnattendedModeAllowedStateImageIndex           { get { return 10; } }
	
	}
	
	//============================================================================
	public class MenuTreeNode : TreeNode, IMenuTreeNodesOwner
	{
		private MenuXmlNode node;
		private MenuTreeNodeCollection nodes = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode(MenuXmlNode aNode, int aImageIndex, int aStateImageIndex)
		{			
			Node = aNode;

			AdjustGraphics(false);

			if (aImageIndex != -1)
				ImageIndex = aImageIndex;

			if (aStateImageIndex != -1)
				StateImageIndex = aStateImageIndex;
			
			nodes = new MenuTreeNodeCollection(this);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode(MenuXmlNode aNode) : this(aNode, -1, -1)
		{
			AdjustImageIndex();
			AdjustStateImageIndex();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode() : this(null)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetNodeAt(int index, MenuXmlNode node)
		{
			SetElementAt(index, new MenuTreeNode(node));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetElementAt(int index, MenuTreeNode item)
		{
			if (index < 0 || index >= NodesCount)
				return;

			base.Nodes[index] = item;
				
			item.AdjustGraphics();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode GetNodeAt(int index)
		{
			MenuTreeNode item = GetElementAt(index);
			if (item == null)
				return null;

			return item.Node;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode GetElementAt(int index)
		{
			if (index < 0 || index >= NodesCount)
			{
				Debug.Fail("Error in MenuTreeView.MenuTreeNode");
				return null;
			}

			return (MenuTreeNode)base.Nodes[index];
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertNodeAt(int index, MenuXmlNode node)
		{
			if (index < 0)
				return -1;

			return InsertItemAt(index, new MenuTreeNode(node));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertItemAt(int index, MenuTreeNode item)
		{
			base.Nodes.Insert(index, item);
			
			item.AdjustGraphics();

			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddNode(MenuXmlNode node)
		{
			return AddItem( new MenuTreeNode(node) );
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddItem(MenuTreeNode item)
		{
			int index = base.Nodes.Add(item);

			item.AdjustGraphics();

			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveItemAt(int index)
		{
			base.Nodes.RemoveAt(index);

			AdjustStateImageIndex();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetNameAttribute()
		{
			return (node != null) ? node.GetNameAttribute() : String.Empty;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetCommandItemObject()
		{
			return (node != null && (node.IsCommand || node.IsExternalItem)) ? node.ItemObject : String.Empty;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string GetNamedFullPath()
		{		
			if 
				(
				node == null || 
				!(node.IsMenu || node.IsCommand|| node.IsExternalItem) ||
				TreeView == null || 
				!(TreeView is MenuTreeView)
				)
				return String.Empty;
			
			string nodeFullPath = String.Empty;
			
			MenuTreeNode tmpNode = this;
			do
			{
				if (nodeFullPath.Length > 0)
					nodeFullPath = TreeView.PathSeparator + nodeFullPath;
				
				string tmpNodeText = String.Empty; 
				
				if (tmpNode.node.IsMenu)
					tmpNodeText = tmpNode.GetNameAttribute();
				else if (tmpNode.node.IsCommand)
					tmpNodeText = tmpNode.GetCommandItemObject();
				
				if (tmpNodeText == null || tmpNodeText.Length == 0)
					tmpNodeText = tmpNode.Text;

				nodeFullPath = tmpNodeText + nodeFullPath;

				tmpNode = tmpNode.Parent;

			}while (tmpNode != null);

			return nodeFullPath;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustGraphics(bool adjustImageIndexes)
		{
			if (TreeView == null || !(TreeView is MenuTreeView))
				return;

			if (node != null)
			{
				if (node.IsMenu)
				{
					ForeColor = node.HasOtherTitles ? System.Drawing.Color.Red : ((MenuTreeView)TreeView).ForeColor;
				}
				else if (node.IsCommand)
				{
					string currentLanguage = (((MenuTreeView)TreeView).MenuXmlParser != null && ((MenuTreeView)TreeView).MenuXmlParser.LoginManager != null && ((MenuTreeView)TreeView).MenuXmlParser.LoginManager.LoginManagerState == LoginManagerState.Logged) ? 
						((MenuTreeView)TreeView).MenuXmlParser.LoginManager.PreferredLanguage : 
						String.Empty;

					IPathFinder menuPathFinder = ((MenuTreeView)TreeView).PathFinder;
					if (menuPathFinder != null)
						MenuInfo.SetMenuCommandOrigin(menuPathFinder, node, currentLanguage);
			
					switch(node.CommandOrigin)
					{
						case CommandOrigin.CustomCurrentUser:
							ForeColor = ((MenuTreeView)TreeView).CurrentUserCommandForeColor;
							//NodeFont = new Font(TreeView.Font.FontFamily.Name, TreeView.Font.Size, FontStyle.Bold);
							break;
						case CommandOrigin.CustomAllUsers:
							ForeColor = ((MenuTreeView)TreeView).AllUsersCommandForeColor;
							//NodeFont = new Font(TreeView.Font.FontFamily.Name, TreeView.Font.Size, FontStyle.Bold);
							break;
						default:
							break;
					}
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
			if (TreeView != null && (TreeView is MenuTreeView))
				ImageIndex = SelectedImageIndex = ((MenuTreeView)TreeView).GetMenuTreeImageIdx(this);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AdjustStateImageIndex()
		{
            this.StateImageIndex = (node != null && this.TreeView != null && (TreeView is MenuTreeView) && ((MenuTreeView)this.TreeView).ShowStateImages) ? MenuTreeView.GetStateImageIdx(node) : -1;

			// Occorre reimpostare anche le immagini di stato relative agli antenati del
			// nodo corrente: se, ad esempio, si è impostata la proprietà di ProtectedState
			// sul nodo di menù ed in precedenza il nodo padre non possedeva figli protetti
			// esso dovrà avere adesso l'immagine di stato relativa ai nodi che hanno
			// almeno un figlio protetto
			MenuTreeNode parentNode = this.Parent;
			if (parentNode != null)
				parentNode.AdjustStateImageIndex();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			base.Nodes.Clear();
		}

		//---------------------------------------------------------------------------
		public void SetBranchStateImages()
		{
			if (TreeView == null ||!(TreeView is MenuTreeView))
				return;

			if (Nodes != null && NodesCount > 0)
			{
				foreach(MenuTreeNode aChildNode in Nodes)
					aChildNode.SetBranchStateImages();
			}

            AdjustStateImageIndex();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuXmlNode Node
		{
			get { return node; }
			set 
			{
				if (value != null && (value.IsCommand || value.IsMenu))
				{
					node = value;

					Text = (node != null) ? node.Title : String.Empty;
					
					AdjustGraphics();
				}
				else
				{
					Debug.Assert(value == null, "Set MenuTreeNode.Node property error: invalid node type");
					node = null;
				}
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public new MenuTreeNode Parent
		{
			get { return (MenuTreeNode)base.Parent; }
		}
       
		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator Enumerator
		{
			get { return base.Nodes.GetEnumerator(); }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		new public MenuTreeNodeCollection Nodes
		{
			get { return nodes; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int NodesCount
		{
			get { return (base.Nodes != null) ? base.Nodes.Count : 0; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode FindMenuTreeNode(MenuXmlNode aMenuNodeToFind, bool searchDescendants)
		{
			if (aMenuNodeToFind == null || !(aMenuNodeToFind.IsMenu || aMenuNodeToFind.IsCommand))
				return null;

			if (Node != null && Node.IsMenu && Node.IsSameMenuAs(aMenuNodeToFind))
				return this;

			if (Node != null && Node.IsCommand && Node.IsSameCommandAs(aMenuNodeToFind) && (String.Compare(Node.GetMenuHierarchyTitlesString(), aMenuNodeToFind.GetMenuHierarchyTitlesString()) == 0))
				return this;

			if (searchDescendants)
			{
				foreach (MenuTreeNode aTreeNode in Nodes)
				{
					MenuTreeNode nodeFound = aTreeNode.FindMenuTreeNode(aMenuNodeToFind, searchDescendants);
					if (nodeFound != null)
						return nodeFound;
				}
			}
			
			return null;
		}
	}

	/// <summary>
	/// The tree's items collection class
	/// </summary>
	//============================================================================
	public class MenuTreeNodeCollection : IList, ICollection, IEnumerable
	{
		private IMenuTreeNodesOwner owner = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNodeCollection(object aOwner)
		{
			if (aOwner != null && (aOwner is IMenuTreeNodesOwner))
				owner = (IMenuTreeNodesOwner)aOwner;
		}
        
		#region IList implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set { this[index] = (MenuTreeNode)value; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((MenuTreeNode)item);
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
			Insert(index, (MenuTreeNode)item);
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
				return (owner != null) ? owner.NodesCount : 0;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsReadOnly 
		{
			get { return false; }
		}
			
		#endregion

		//--------------------------------------------------------------------------------------------------------------------------------
		public MenuTreeNode this[int index]
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
		public void Remove(MenuTreeNode item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Insert(int index, MenuTreeNode item)
		{
			if (owner != null)
				owner.InsertItemAt(index, item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Add(MenuTreeNode item)
		{
			return (owner != null) ? owner.InsertItemAt(Count, item) : -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AddRange(MenuTreeNode[] items)
		{
			for(IEnumerator e = items.GetEnumerator(); e.MoveNext();)
				Add((MenuTreeNode)e.Current);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			if (owner != null)
				owner.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (owner != null)
				owner.RemoveItemAt(index);
		}
		
		//---------------------------------------------------------------------------
        public void AdjustStateImageIndexes()
		{
			for(int i = 0; i < this.Count; i++)
			{
				MenuTreeNode aNode = this[i];
				if (aNode == null)
					continue;

				aNode.AdjustStateImageIndex();
				if (aNode.NodesCount > 0)
					aNode.Nodes.AdjustStateImageIndexes();
			}
		}
		
		//---------------------------------------------------------------------------
		public void AdjustGraphics(bool adjustImageIndexes)
		{
			for(int i = 0; i < this.Count; i++)
			{
				MenuTreeNode aNode = this[i];
				if (aNode == null)
					continue;

				aNode.AdjustGraphics(adjustImageIndexes);
				if (aNode.NodesCount > 0)
					aNode.Nodes.AdjustGraphics(adjustImageIndexes);
			}
		}
		
	}

    //============================================================================
	public interface IMenuTreeNodesOwner
	{
		MenuTreeNode GetElementAt(int index);
		void SetElementAt(int index, MenuTreeNode item);
		int InsertItemAt(int index, MenuTreeNode item);
		void RemoveItemAt(int index);
		void Clear();
		MenuTreeNode FindMenuTreeNode(MenuXmlNode aMenuNodeToFind, bool searchDescendants);

		int NodesCount { get; }
		IEnumerator Enumerator { get; }
	}
}