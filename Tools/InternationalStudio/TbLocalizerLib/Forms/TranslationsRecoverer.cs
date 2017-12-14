using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	public partial class TranslationsRecoverer : Form
	{
		DictionaryTreeNode dictionaryNode;

		//---------------------------------------------------------------------
		public TranslationsRecoverer(DictionaryTreeNode node)
		{
			InitializeComponent();

			this.dictionaryNode = node;
			listViewNew.OnPaintList += new EventHandler(OnPaintList);
			listViewOld.OnPaintList += new EventHandler(OnPaintList);
		}

		//---------------------------------------------------------------------
		void OnPaintList(object sender, EventArgs e)
		{
			panelArrows.Refresh();
		}

		//---------------------------------------------------------------------
		private void TranslationsRecoverer_Load(object sender, EventArgs e)
		{
			AdjustPositions();
			AddDictionaryNode(dictionaryNode, false);
		}

		//---------------------------------------------------------------------
		private ListViewGroup AddDictionaryNode(DictionaryTreeNode aNode, bool onlyOld)
		{
			ListViewGroup lwgNew = null;
			if (!onlyOld)
			{
				lwgNew = new ListViewGroup(aNode.FullPath);
				lwgNew.Name = aNode.FullPath;
				listViewNew.Groups.Add(lwgNew);
			}

			ListViewGroup lwgOld = new ListViewGroup(aNode.FullPath);
			lwgOld.Name = aNode.FullPath;
			listViewOld.Groups.Add(lwgOld);

			XmlElement resNode = aNode.GetResourceNode();
			foreach (XmlElement stringNode in resNode.ChildNodes)
			{
				string targetString = stringNode.GetAttribute(AllStrings.target);
				bool valid = stringNode.GetAttribute(AllStrings.valid) != AllStrings.falseTag;

				if (!valid)
				{
					
					string baseString = stringNode.GetAttribute(AllStrings.baseTag);
					string id = stringNode.GetAttribute(AllStrings.id);
					ListViewItem lvi = new ListViewItem(baseString, lwgOld);
					lvi.ImageIndex = 0;
					lvi.SubItems.Add(id);
					lvi.ToolTipText = targetString;

					listViewOld.Items.Add(lvi);
				}
				else if (targetString.Length == 0 && lwgNew != null)
				{
					string baseString = stringNode.GetAttribute(AllStrings.baseTag);
					string id = stringNode.GetAttribute(AllStrings.id);
					ListViewItem lvi = new ListViewItem(baseString, lwgNew);
					lvi.ImageIndex = 0;
					lvi.SubItems.Add(id);
					listViewNew.Items.Add(lvi);
				}
			}

			return lwgOld;
		}
		
		//---------------------------------------------------------------------
		private void panelArrows_Paint(object sender, PaintEventArgs e)
		{

			foreach (ListViewItem item in listViewNew.Items)
			{
				if (item.Tag == null)
					continue;

				ListViewItem sourceItem = item.Tag as ListViewItem;
				Point sourcePosition = listViewNew.PointToScreen(listViewNew.GetItemRect(item.Index).Location);
				Point targetPosition = listViewOld.PointToScreen(listViewOld.GetItemRect(sourceItem.Index).Location);

				e.Graphics.DrawLine(Pens.Blue, 0, panelArrows.PointToClient(sourcePosition).Y + (item.Bounds.Height / 2), panelArrows.Width, panelArrows.PointToClient(targetPosition).Y + (sourceItem.Bounds.Height / 2));
			}
		}

		//---------------------------------------------------------------------
		private void btnAddSource_Click(object sender, EventArgs e)
		{
			OldStringsTree f = Functions.CurrentSolutionCache["OldStringsTree"].Object as OldStringsTree;
			if (f == null)
			{ 
				f = new OldStringsTree();
				Functions.CurrentSolutionCache["OldStringsTree"].Object = f;
			}
			if (DialogResult.OK == f.ShowDialog(this))
			{
				foreach (DictionaryTreeNode node in f.SelectedItems)
				{
					ListViewGroup lvgOld = AddDictionaryNode(node, true);
					foreach (ListViewItem item in lvgOld.Items)
					{
						if (item.Tag != null)
							continue;
						foreach (ListViewItem newItem in listViewNew.Items)
						{
							if (newItem.Tag == null && newItem.Text == item.Text)
							{
								Associate(newItem, item);
								break;
							}
						}
					}
				}
			}
		}
		

		//---------------------------------------------------------------------
		private void listViewOld_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != MouseButtons.Left)
				return;

			ListViewItem item = listViewOld.GetItemAt(e.X, e.Y);
			if (item == null)
				return;

			listViewOld.DoDragDrop(item, DragDropEffects.Link | DragDropEffects.Copy | DragDropEffects.Move);
		}

		//---------------------------------------------------------------------
		private void listViewNew_DragEnter(object sender, DragEventArgs e)
		{
			if ( e.Data.GetDataPresent(typeof(ListViewItem)) )
				e.Effect = DragDropEffects.Link;

		}

		//---------------------------------------------------------------------
		private void TranslationsRecoverer_DragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(ListViewItem)))
				return;

			e.Effect = DragDropEffects.Link;
		}

		//---------------------------------------------------------------------
		private void listViewNew_DragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(ListViewItem)))
				return;

			Point p = listViewNew.PointToClient(new Point(e.X, e.Y));
			ListViewItem item = listViewNew.GetItemAt(p.X, p.Y);
			if (item == null)
			{
				e.Effect = DragDropEffects.Link;
				return;
			}
			e.Effect = DragDropEffects.Copy;

		}
		
		//---------------------------------------------------------------------
		private void listViewNew_DragDrop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(ListViewItem)))
				return;
			Point p = listViewNew.PointToClient(new Point(e.X, e.Y));
			ListViewItem item = listViewNew.GetItemAt(p.X, p.Y);
			if (item == null)
				return;
			
			ListViewItem sourceItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));

			Associate(item, sourceItem);
		}

		//---------------------------------------------------------------------
		private void Associate(ListViewItem item, ListViewItem sourceItem)
		{
			item.ToolTipText = sourceItem.Text;

			ListViewItem existingItem = sourceItem.Tag as ListViewItem;
			if (existingItem != null)
				existingItem.Tag = null;

			sourceItem.Tag = item;
			item.Tag = sourceItem;
			
			panelArrows.Invalidate();
		}

		//---------------------------------------------------------------------
		private void listViewOld_SizeChanged(object sender, EventArgs e)
		{
			columnHeaderOldString.Width = listViewOld.Width;

		}

		//---------------------------------------------------------------------
		private void listViewNew_SizeChanged(object sender, EventArgs e)
		{
			columnHeaderNewString.Width = listViewNew.Width;

		}

		//---------------------------------------------------------------------
		private void listViewOld_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 3 //CTRL C
				&& listViewOld.SelectedItems.Count == 1)
			{
				Clipboard.SetDataObject(listViewOld.SelectedItems[0].Index);
			}
		}

		//---------------------------------------------------------------------
		private void listViewNew_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == 22 //CTRL V
				&& listViewNew.SelectedItems.Count == 1 
				&& Clipboard.GetDataObject().GetDataPresent(typeof(int)))
			{
				int index = (int)Clipboard.GetDataObject().GetData(typeof(int));
				if (index == -1)
					return;
				
				Associate(listViewNew.SelectedItems[0], listViewOld.Items[index]);
			}
		}

		//---------------------------------------------------------------------
		private void TranslationsRecoverer_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
				return;

			Save(e);
		}

		//---------------------------------------------------------------------
		private void Save(FormClosingEventArgs e)
		{
			List<string> oldIds = new List<string>();
			List<string> newIds = new List<string>();
			List<string> nodePaths = new List<string>();
			foreach (ListViewItem item in listViewNew.Items)
			{
				if (item.Tag == null)
					continue;

				ListViewItem sourceItem = (ListViewItem)item.Tag;
				oldIds.Add(item.SubItems[1].Text);
				
				string nodePath = sourceItem.Group.Name;
				nodePath = string.Format("{0}-{1}", nodePath, sourceItem.SubItems[1].Text); 
				nodePaths.Add(nodePath);
			}

			if (oldIds.Count > 0)
				LocalizerDocument.ReplaceIdInBaseLanguageFile(dictionaryNode, oldIds, nodePaths);
		}

		//---------------------------------------------------------------------
		private void TranslationsRecoverer_SizeChanged(object sender, EventArgs e)
		{
			AdjustPositions();
		}

		//---------------------------------------------------------------------
		private void AdjustPositions()
		{
			int space = 20;
			int width = (this.Width - panelArrows.Width - space * 2) / 2;
			listViewNew.Width = listViewOld.Width = width;
			listViewNew.Left = labelNewStrings.Left = space;
			panelArrows.Left = listViewNew.Right;
			listViewOld.Left = labelOldStrings.Left = panelArrows.Right;
		}

		//---------------------------------------------------------------------
		private void listViewOld_DragDrop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(ListViewItem)))
				return;
			Point p = listViewOld.PointToClient(new Point(e.X, e.Y));
			ListViewItem item = listViewOld.GetItemAt(p.X, p.Y);
			if (item == null)
				return;

			ListViewItem sourceItem = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
			if (item == sourceItem)
				return;
			ListViewGroup group = sourceItem.Group;
			sourceItem.Remove();
			group.Items.Add(sourceItem);
			listViewOld.Items.Insert(item.Index, sourceItem);
		}

		//---------------------------------------------------------------------
		private void listViewOld_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(typeof(ListViewItem)))
				e.Effect = DragDropEffects.Link;
		}

		//---------------------------------------------------------------------
		private void listViewOld_DragOver(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(typeof(ListViewItem)))
				return;

			Point p = listViewOld.PointToClient(new Point(e.X, e.Y));
			ListViewItem item = listViewNew.GetItemAt(p.X, p.Y);
			if (item == null)
			{
				e.Effect = DragDropEffects.Link;
				return;
			}
			e.Effect = DragDropEffects.Move;
		}

	}

	class MyListView : ListView
	{
		public event EventHandler OnPaintList;
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
			if (m.Msg == 0x000F)//WM_PAINT
			{
				if (OnPaintList != null)
					OnPaintList(this, EventArgs.Empty);
			}
		}
	}
}