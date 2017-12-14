using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Microarea.TBPicComponents
{
	//===============================================================================
	public partial class TBThumbnailEx : UserControl
	{
		//---------------------------------------------------------------------------
		public TBThumbnailEx()
		{
			TBPicBaseComponents.Register();
			InitializeComponent();
		}

		//--------------------------------------------------------------------------------
		public bool AllowDropFiles { get { return thumbnailEx.AllowDropFiles; } set { thumbnailEx.AllowDropFiles = value; } }

		//--------------------------------------------------------------------------------
		public bool AllowMoveItems { get { return thumbnailEx.AllowMoveItems; } set { thumbnailEx.AllowMoveItems = value; } }

		//--------------------------------------------------------------------------------
		public bool CheckBoxes { get { return thumbnailEx.CheckBoxes; } set { thumbnailEx.CheckBoxes = value; } }

		//--------------------------------------------------------------------------------
		public bool HotTracking { get { return thumbnailEx.HotTracking; } set { thumbnailEx.HotTracking = value; } }

		//--------------------------------------------------------------------------------
		public bool MultiSelect { get { return thumbnailEx.MultiSelect; } set { thumbnailEx.MultiSelect = value; } }

		//--------------------------------------------------------------------------------
		public bool LockGdViewerEvents { get { return thumbnailEx.LockGdViewerEvents; } set { thumbnailEx.LockGdViewerEvents = value; } }

		//--------------------------------------------------------------------------------
		public bool OwnDrop { get { return thumbnailEx.OwnDrop; } set { thumbnailEx.OwnDrop = value; } }

		//--------------------------------------------------------------------------------
		public bool PauseThumbsLoading { get { return thumbnailEx.PauseThumbsLoading; } set { thumbnailEx.PauseThumbsLoading = value; } }

		//--------------------------------------------------------------------------------
		public bool PreloadAllItems { get { return thumbnailEx.PreloadAllItems; } set { thumbnailEx.PreloadAllItems = value; } }

		//--------------------------------------------------------------------------------
		public bool RotateExif { get { return thumbnailEx.RotateExif; } set { thumbnailEx.RotateExif = value; } }

		//--------------------------------------------------------------------------------
		public bool DisplayAnnotations { get { return thumbnailEx.DisplayAnnotations; } set { thumbnailEx.DisplayAnnotations = value; } }

		//--------------------------------------------------------------------------------
		public bool ShowText { get { return thumbnailEx.ShowText; } set { thumbnailEx.ShowText = value; } }

		//--------------------------------------------------------------------------------
		public System.Drawing.Color SelectedThumbnailBackColor { get { return thumbnailEx.SelectedThumbnailBackColor; } set { thumbnailEx.SelectedThumbnailBackColor = value; } }

		//--------------------------------------------------------------------------------
		public System.Drawing.Color ThumbnailBackColor { get { return thumbnailEx.ThumbnailBackColor; } set { thumbnailEx.ThumbnailBackColor = value; } }

	

		//--------------------------------------------------------------------------------
		public int CheckBoxesMarginLeft { get { return thumbnailEx.CheckBoxesMarginLeft; } set { thumbnailEx.CheckBoxesMarginLeft = value; } }

		//--------------------------------------------------------------------------------
		public int CheckBoxesMarginTop { get { return thumbnailEx.CheckBoxesMarginTop; } set { thumbnailEx.CheckBoxesMarginTop = value; } }

		//--------------------------------------------------------------------------------
		public int TextMarginLeft { get { return thumbnailEx.TextMarginLeft; } set { thumbnailEx.TextMarginLeft = value; } }

		//--------------------------------------------------------------------------------
		public int TextMarginTop { get { return thumbnailEx.TextMarginTop; } set { thumbnailEx.TextMarginTop = value; } }

		//--------------------------------------------------------------------------------
		public bool ThumbnailBorder { get { return thumbnailEx.ThumbnailBorder; } set { thumbnailEx.ThumbnailBorder = value; } }

		//--------------------------------------------------------------------------------
		public Size ThumbnailSize { get { return thumbnailEx.ThumbnailSize; } set { thumbnailEx.ThumbnailSize = value; } }

		//--------------------------------------------------------------------------------
		public Size ThumbnailSpacing { get { return thumbnailEx.ThumbnailSpacing; } set { thumbnailEx.ThumbnailSpacing = value; } }



		//---------------------------------------------------------------------------
		public void ClearAllItems()
		{
			thumbnailEx.ClearAllItems();	
		}

		//---------------------------------------------------------------------------
		public void LoadFromGdViewer(TBPicViewer viewer)
		{
			
			thumbnailEx.LoadFromGdViewer(viewer.InternalGdViewer);
		}
		//---------------------------------------------------------------------------
		public void SelectItem(int  itemIndex)
		{
			thumbnailEx.SelectItem(itemIndex);
		}

		//---------------------------------------------------------------------------
		public new void Focus()
		{
			thumbnailEx.Focus();
		}
	}
}
