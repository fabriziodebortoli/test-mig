using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{

	interface ITreeItem
	{
		TBTreeView TreeView { get; }
	}

	class TBTreeView : TBScrollPanel, ITreeItem
	{
		ImageBuffer bitmap;
		Bitmap treeBitmap;
		int iconCount = -1;
		int iconHeight = -1;

		//--------------------------------------------------------------------------------------
		public Bitmap TreeBitmap
		{
			get
			{
				if (treeBitmap == null)
				{
					treeBitmap = bitmap.CreateBitmap();
				}
				return treeBitmap;
			}
		}

		//--------------------------------------------------------------------------------------
		public int IconCount 
		{
			get	{ return iconCount != -1 ? iconCount : ((WndTreeCtrlDescription)ControlDescription).Icons; }
		}

		//--------------------------------------------------------------------------------------
		public int IconHeight
		{
			get { return iconHeight != -1 ? iconHeight : ((WndTreeCtrlDescription)ControlDescription).IconHeight; }
		}

		//--------------------------------------------------------------------------------------
		public int IconWidth
		{
			get { return TreeBitmap.Width; }
		}


		//--------------------------------------------------------------------------------------
		public TBTreeView()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			bitmap = ((WndTreeCtrlDescription)ControlDescription).Image;
			base.OnInit(e);
		}
		
		//--------------------------------------------------------------------------------------
		public override void UpdateFromControlDescription(WndObjDescription description)
		{
			WndTreeCtrlDescription treeCtrlDescription = (WndTreeCtrlDescription)description;
			bitmap = treeCtrlDescription.Image;
			iconCount = treeCtrlDescription.Icons;
			iconHeight = treeCtrlDescription.IconHeight; 
			base.UpdateFromControlDescription(description);
		}

		//--------------------------------------------------------------------------------------
		public override void Dispose()
		{
			if (treeBitmap != null)
				treeBitmap.Dispose();
			base.Dispose();
		}


		#region ITreeItem Members

		//--------------------------------------------------------------------------------------
		public TBTreeView TreeView
		{
			get { return this; }
		}

		#endregion
	}

	//=========================================================================
	class TBTreeNode : TBPanel, ITreeItem
	{
		protected ImageButton	toggle;
		protected ImageButton	icon;
		protected ImageButton	stateImg; //usata nelle opzioni di documento per segnare report di defualt con un flag
		protected Label			label;
		
		bool Expanded { get { return ((WndTreeNodeDescription)ControlDescription).Expanded;}}
		bool Selected { get { return ((WndTreeNodeDescription)ControlDescription).Selected;}}
		bool HasChild { get { return ((WndTreeNodeDescription)ControlDescription).HasChild;}}
		int  IdxIcon { get { return  ((WndTreeNodeDescription)ControlDescription).IdxIcon; }}
		int  IdxSelectedIcon { get { return ((WndTreeNodeDescription)ControlDescription).IdxSelectedIcon; } }
		bool HasStateImg { get { return ((WndTreeNodeDescription)ControlDescription).HasStateImg; } }

		public TBTreeView TreeView { get { return ((ITreeItem)parentTBWebControl).TreeView; } }
	
		//--------------------------------------------------------------------------------------
		public TBTreeNode()
		{
		}

		//--------------------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			label = new Label();
			toggle = new ImageButton();
			icon = new ImageButton();
			stateImg = new ImageButton();

			base.OnInit(e);

			toggle.OnClientClick = string.Format("tbActionOnNode(this,'{0}','ToggleNode')", this.ClientID);
			toggle.CssClass = "TBTreeNodeToggle";

			//in presenza di state il nodo non ha figli, lo posizione come l'icona per aprire i nodi figli
			stateImg.CssClass = "TBTreeNodeToggle";


			if (!string.IsNullOrWhiteSpace(icon.ImageUrl))
			{
				icon.OnClientClick = string.Format("tbActionOnNode(this,'{0}','SelectNode')",this.ClientID);
				icon.CssClass = "TBTreeNodeIcon";
				icon.ID = string.Format("icon{0}",ID);
				panel.Controls.Add(icon);
			}

			label.Height = InnerControl.Height;
			label.Attributes.Add("onclick", string.Format("tbActionOnNode(this,'{0}','SelectNode')", ClientID));
			label.Attributes.Add("onContextMenu", string.Format("onContextMenu(event, '{0}', '{1}', '0');return false;", InnerControl.ClientID, TreeView.WindowId));
			panel.Controls.Add(label);
			
			panel.Controls.Add(toggle);
			panel.Controls.Add(stateImg);
		}

		//--------------------------------------------------------------------------------------
		string GetImageName(int idx)
		{
			return string.Format("{0}_{1}_{2}.png", formControl.ProxyObjectId, WindowId, idx);
		}

		//--------------------------------------------------------------------------------------
		string GetImageUrl (int idx)
		{
			return ImagesHelper.GetImageUrl(GetImageName(idx));
		}

		//--------------------------------------------------------------------------------------
		void CreateIconImage(int idx)
		{
			try
			{
				if (TreeView.TreeBitmap == null || idx < 0 || idx > TreeView.IconCount - 1)
					return;

				string imageName = GetImageName(idx);
				if (ImagesHelper.HasImageInCache(imageName))
					return;

				int width = TreeView.TreeBitmap.Width;
				int height = TreeView.IconHeight;
				using (Bitmap bmp = new Bitmap(TreeView.TreeBitmap, width, height))
				{
					using (Graphics g = Graphics.FromImage(bmp))
						g.DrawImage(TreeView.TreeBitmap, new Rectangle(0, 0, width, height), new Rectangle(0, height * idx, width, height), GraphicsUnit.Pixel);

					Color transparentColor = Color.Magenta;
					bmp.MakeTransparent(transparentColor);
					CreateDocumentImage(bmp, imageName, null);
				}
			}
			catch
			{
			}
		}
		
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			InnerControl.Width = Unit.Pixel(((WndTreeCtrlDescription)TreeView.ControlDescription).Width - X);
			toggle.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X).ToString();
			//spazio per il pulsante di apertura chiusura del nodo 
			int xIncrement = 20;
			toggle.ID = string.Format("toggle{0}", ID);
			if (IdxIcon != -1 || IdxSelectedIcon != -1)
			{	
				CreateIconImage(IdxIcon);
				CreateIconImage(IdxSelectedIcon);
				icon.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + xIncrement).ToString();
				//incremento della larghezza dell'immagine'
				xIncrement += TreeView.TreeBitmap != null ? (TreeView.TreeBitmap.Width + 5) : 0;
				icon.ImageUrl = GetImageUrl(Selected ? IdxSelectedIcon : IdxIcon);
			}
			label.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + xIncrement).ToString();
			label.ToolTip = label.Text = ControlDescription.Text;
            label.CssClass = Selected ? "TBTreeNodeLabel Selected" : "TBTreeNodeLabel";
			toggle.ImageUrl = Expanded
				? ImagesHelper.CreateImageAndGetUrl("CollapsedNodeIcon.png", TBWebFormControl.DefaultReferringType)
				: ImagesHelper.CreateImageAndGetUrl("ExpandedNodeIcon.png", TBWebFormControl.DefaultReferringType);
			toggle.Visible = HasChild;

			stateImg.ID = string.Format("stateimg{0}", ID);
			if (HasStateImg && !HasChild)
			{
				stateImg.Visible = true;
				stateImg.ImageUrl = ImagesHelper.CreateImageAndGetUrl("CheckMark.png", TBWebFormControl.DefaultReferringType);
			}
			else
			{
				stateImg.Visible = false;
			}
		}
	}
}
