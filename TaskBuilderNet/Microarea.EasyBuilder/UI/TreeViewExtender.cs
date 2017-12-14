using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.UI
{
	//=========================================================================
	internal class TreeViewExtender
	{
		private TreeView treeView;
		private TreeNode lastDragDestination;
		private DateTime lastDragDestinationTime;
		private const int scrollThreshold = 40;

		//-----------------------------------------------------------------------------
		public TreeViewExtender(TreeView treeView)
		{
			this.treeView = treeView;
			treeView.DragOver += new DragEventHandler(TreeView_DragOverForScroll);
			treeView.DragOver += new DragEventHandler(TreeView_DragOverForAutoExpand);
		}

		//-----------------------------------------------------------------------------
		void TreeView_DragOverForAutoExpand(object sender, DragEventArgs e)
		{
			Point p = treeView.PointToClient(new Point(e.X, e.Y));
			TreeNode currentItem = treeView.GetNodeAt(p.X, p.Y) as TreeNode;
			if (currentItem == null)
				return;

			AutoExpandNodes(currentItem);
		}

		//-----------------------------------------------------------------------------
		void TreeView_DragOverForScroll(object sender, DragEventArgs e)
		{
			ScrollTreeDataManagers(e);
		}

		//-----------------------------------------------------------------------------
		//Scroll in su o in giù a seconda che il drag over avvenga vicino
		//al bordo inferiore o superiore della finestra per permettere la visualizzazione
		//di nodi non presenti sullo schermo.
		private void ScrollTreeDataManagers(DragEventArgs e)
		{
			if (treeView == null || treeView.IsDisposed)
				return;

			Point mouseClientPosition = treeView.PointToClient(new Point(e.X, e.Y));

			// Se il mouse gravita vicino al bordo inferiore allora scroll dell'albero
			// in su per visualizzare i nodi nascosti sotto al bordo inferiore
			if (mouseClientPosition.Y + scrollThreshold > treeView.Height)
				ExternalAPI.PostMessage(treeView.Handle, ExternalAPI.WM_VSCROLL, (IntPtr)1, (IntPtr)0);

			// Altrimenti se il mouse gravita vicino al bordo superiore allora
			// scroll dell'albero in giù per visualizzare i nodi nascosti sopra
			// al bordo superiore
			else if (mouseClientPosition.Y < scrollThreshold)
				ExternalAPI.PostMessage(treeView.Handle, ExternalAPI.WM_VSCROLL, (IntPtr)0, (IntPtr)0);
		}

		//-----------------------------------------------------------------------------
		//Autoespande i nodi chiusi su cui si grafita con il mouse durante le operazioni di drag and drop
		private void AutoExpandNodes(TreeNode currentItem)
		{
			try
			{
				//Tengo da parte l'ultimo nodo selezionato, e il tempo in cui sono stato in hover sopra quel nodo, 
				if (currentItem != lastDragDestination)
				{
					lastDragDestination = currentItem;
					lastDragDestinationTime = DateTime.Now;
				}
				else
				{
					//Se supero il secondo di attesa in hover sullo stesso nodo, allora lo espando
					TimeSpan hoverTime = DateTime.Now.Subtract(lastDragDestinationTime);
					if (hoverTime.TotalSeconds > 1)
						currentItem.Expand();
				}
			}
			catch { }
		}
	}
}
