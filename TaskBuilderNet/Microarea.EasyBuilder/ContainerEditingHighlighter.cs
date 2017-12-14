using Microarea.EasyBuilder.MVC;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.EasyBuilder
{
	//=================================================================================
	internal class ContainerEditingHighlighter
    {
        private static int numberOfMouseClick = 2;
		private bool isSwitchTileGroup = false;
		//-----------------------------------------------------------------------------
		private List<WindowWrapperContainer> currentEditedObjects;
        private List<WindowWrapperContainer> currentRemovedObjects;

      //-----------------------------------------------------------------------------
        public bool ImEditingOnlyThisTile
		{
            get
            {
                return currentEditedObjects.Count > 0;
            }
        }

		//-----------------------------------------------------------------------------
		internal ContainerEditingHighlighter()
        {
            currentRemovedObjects = new List<WindowWrapperContainer>();
			currentEditedObjects = new List<WindowWrapperContainer>();
        }

        //-----------------------------------------------------------------------------
        internal bool IsToggleShortCut (MouseEventArgs e)
        {			
            return e?.Button == MouseButtons.Left && e?.Clicks == numberOfMouseClick;
        }

        //-----------------------------------------------------------------------------
        internal bool IsInEdit(IWindowWrapperContainer container)
        {
            return currentEditedObjects.Contains(container);
        }

		//-----------------------------------------------------------------------------
		internal void SelectionChanging(IWindowWrapper oldSelected)
		{
			WindowWrapperContainer container = oldSelected as WindowWrapperContainer;
			if (container != null)
				container.AfterSelectionChanged(false);
		}

		//-----------------------------------------------------------------------------
		internal void SelectionChanged(IWindowWrapper selected)
		{
			WindowWrapperContainer container = selected as WindowWrapperContainer;
			if (container != null)
				container.AfterSelectionChanged(true);
		}

		//-----------------------------------------------------------------------------
		internal void OnMouseDownOn(ICollection selections, MouseEventArgs e, bool UniqueTileORSaveExitEvent)
        {
			if (ImEditingOnlyThisTile && selections!=null)
				foreach (IComponent component in selections)
				{
					IWindowWrapper tgroupIWant = component as IWindowWrapper;
					if (tgroupIWant == null || !(tgroupIWant is MTileGroup)) continue;
					foreach (WindowWrapperContainer tileIWas in currentEditedObjects)
					{
						if (currentEditedObjects.Contains(tgroupIWant) ||  tileIWas.Parent == tgroupIWant    )            //non sono lo stesso elemento  non sono nello stesso tileGroup
							continue;

						if (tgroupIWant.Parent != null && (tgroupIWant.Parent == tileIWas.ParentComponent.ParentComponent) ) //se hanno lo stesso TileManager
						{
							isSwitchTileGroup = true;
							break;
						}
					}
				}

			if (IsToggleShortCut(e) || isSwitchTileGroup || UniqueTileORSaveExitEvent)
                ToggleHighlights(selections);
        }

		//-----------------------------------------------------------------------------
		internal void EnterInOwnEditing(IWindowWrapper wrapper)
		{
			WindowWrapperContainer container = wrapper as WindowWrapperContainer;
			// se si tratta di un tile panel, per primo mando in editing il tile panel
			if (container is MTilePanel)
			{
				EnterInOwnEditing(SelecFirstTileInPanel(container) as WindowWrapperContainer);
			}
			// se si tratta di una tile contenuta in una tile panel tab, per primo mando in editing il tile panel
			else if (wrapper.Parent is MTilePanelTab)
			{
				EnterInOwnEditing(wrapper.Parent.Parent as WindowWrapperContainer);

			}

			EnterInOwnEditing(container);

		}

		//-----------------------------------------------------------------------------
		internal void EnterInOwnEditing(WindowWrapperContainer container)
		{
			if (container != null && container.HasOwnEditing)
			{
				currentEditedObjects.Add(container);
				HightLightOn(currentEditedObjects);
			}
		}

		//-----------------------------------------------------------------------------
		internal void ExitFromOwnEditing()
		{
			HightLightOff();
			currentRemovedObjects.Clear();
			currentEditedObjects.Clear();
			isSwitchTileGroup = false;
		}



		//-----------------------------------------------------------------------------
		internal void ToggleHighlights(ICollection selections)
        {
			if (selections == null)
				return;
			bool isTheRightArea = false;
			if (ImEditingOnlyThisTile)
			{
				foreach (IComponent component in selections)
				{
					IWindowWrapper item = component as IWindowWrapper;
					if (item == null)
						continue;

					IWindowWrapper tileSelected = item;
					if (!(item is MTileDialog) && (item.Parent is MTileDialog)   )
					{	//voglio uscire dall'editing, ma la selection contiene un parsed control
						tileSelected = item.Parent; 
					}
					if (currentEditedObjects.Contains(tileSelected))
						isTheRightArea = true; //esco dall'editing solo se sono sulla stessa tile che stavo editando
				}

				if (isTheRightArea || isSwitchTileGroup)	
					ExitFromOwnEditing();
			}
			else
				foreach (IComponent component in selections)
				{
					IWindowWrapper item = component as IWindowWrapper;
					if (item == null)
						continue;

					EnterInOwnEditing(item);
				}
        }

        //-----------------------------------------------------------------------------
        private void HightLightOn(List<WindowWrapperContainer> containersToHightLight)
        {
            foreach (WindowWrapperContainer container in containersToHightLight)
                HightLightOn(container);
        }

        //-----------------------------------------------------------------------------
        private void HightLightOn(IWindowWrapperContainer containerToHightLight)
        {
            if (containerToHightLight == null)
                return;

            WindowWrapperContainer container = containerToHightLight.Parent as WindowWrapperContainer;
            foreach (IComponent component in container.Components)
            {
                WindowWrapperContainer containerToManage = component as WindowWrapperContainer;
                if (containerToManage == null)
                    continue;
                if (containerToManage == containerToHightLight)
                {
                    containerToManage.AfterOwnEditingSwitching(true, true);
                    continue;
                }
                if (containerToManage.IsWindowVisible())
                {
                    currentRemovedObjects.Add(containerToManage);
                    containerToManage.AfterOwnEditingSwitching(true, false);
                }
            }

            container.AfterOwnEditingSwitching(true, true);
        }

        //-----------------------------------------------------------------------------
        private void HightLightOff()
        {
            List<WindowWrapperContainer> parents = new List<WindowWrapperContainer>	();
            for (int i = currentEditedObjects.Count - 1; i >= 0; i--)
            {
                WindowWrapperContainer containerToSwitch= currentEditedObjects[i] as WindowWrapperContainer;
                containerToSwitch.AfterOwnEditingSwitching(false, false);
				WindowWrapperContainer parent = containerToSwitch.Parent as WindowWrapperContainer;
				if (parent != null && !parents.Contains(parent))
					parents.Add(parent);
			}


            for (int i = currentRemovedObjects.Count - 1; i >= 0; i--)
            {
                WindowWrapperContainer containerToShow = currentRemovedObjects[i] as WindowWrapperContainer;
                if (containerToShow != null)
                {
					WindowWrapperContainer parent = containerToShow.Parent as WindowWrapperContainer;
					if (parent != null && !parents.Contains(parent))
						parents.Add(parent);

					containerToShow.AfterOwnEditingSwitching(false, false);
                }
            }
      
            foreach (WindowWrapperContainer parent in parents)
                parent.AfterOwnEditingSwitching(false, false);//relayout

        }

		//-----------------------------------------------------------------------------
		internal bool IsTheUniqueTile(DocumentView view, object primarySelection)
		{
			foreach (IComponent item in view.Components)
			{
				MTileManager tileMang = item as MTileManager;
				if (tileMang != null)
					return IsTheUniqueTileInTileGroup(tileMang, primarySelection);
				MTileGroup tilegroup = item as MTileGroup;
				if (tilegroup != null && tilegroup.Components.Count == 1)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		private bool IsTheUniqueTileInTileGroup(MTileManager tileMang, object primarySelection)
		{
			MTileGroup confrontTG = null;
			if (primarySelection is MTileDialog)
			{
				if(((MTileDialog)primarySelection).Parent is MTileGroup)
					confrontTG = (MTileGroup)((MTileDialog)primarySelection).Parent;
				else if (((MTileDialog)primarySelection).Parent is MTilePanelTab)
				{
					MTileDialog tile = primarySelection as MTileDialog;
					confrontTG = tile.Parent.Parent.Parent as MTileGroup;
				}

			}

			
			foreach (IComponent item in tileMang.Components)
				{
					MTileGroup tilegroup = item as MTileGroup;
				if (tilegroup != null && tilegroup == confrontTG)
					return tilegroup.Components.Count == 1;								
				}
			
			return false;
		}

		//-----------------------------------------------------------------------------
		internal IWindowWrapper SelecFirstTileInPanel(WindowWrapperContainer wrapper)
		{
		//	MTileDialog uniqueTile = null;
			foreach (IComponent item in wrapper.Components)
			{
				MTilePanelTab tilePanelTab = item as MTilePanelTab;
				if (tilePanelTab != null && tilePanelTab.Components.Count>0)
					foreach (MTileDialog tile in tilePanelTab.Components)
						return tile as IWindowWrapper;
			}
		/*	foreach (MTilePanelTab item in ((MTilePanel)wrapper).Components)
			{
				if (item is MTilePanelTab)
					foreach (MTileDialog tile in item.Components)
						return tile as IWindowWrapper;
			}*/
			return null;
		}


		//-----------------------------------------------------------------------------
		internal void HighlightRect(Rectangle clientCurrRect, FormEditor editor)
		{
			int counter = 0;
			using (Graphics g = editor.CreateGraphics())
			using (Pen pen = new Pen(Color.BlueViolet, 3))
			{
				while (counter < 4)
				{
					ITheme theme = DefaultTheme.GetTheme();
					pen.Color = counter % 2 == 0 ?
						theme.GetThemeElementColor("HighlightingColor") 
						: Color.White;
					g.DrawRectangle(pen, clientCurrRect);
					Thread.Sleep(300);
					counter++;
				}
			}
		}


        //-----------------------------------------------------------------------------
        internal void RecursiveCalcComponentRect(TreeNode node, object linkedComponent, ref Point hlLocation, ref Size hlSize, ref MTileGroup tileGroup)
        {
            foreach (TreeNode item in node.Nodes)
            {
                MLayoutComponent mlContainer = item.Tag as MLayoutComponent;
                if (mlContainer != null)
                {

                    MLayoutContainer container = item.Tag as MLayoutContainer;
                    if (container != null && container.LinkedComponent == null)
                    {
                        RecursiveCalcComponentRect(item, linkedComponent, ref hlLocation, ref hlSize, ref tileGroup);
                        continue;
                    }
                    WindowWrapperContainer wndContainer = mlContainer.LinkedComponent as WindowWrapperContainer;
                    if (wndContainer == null)
                        continue;

                    hlLocation.X = Math.Min(wndContainer.Rectangle.X, hlLocation.X);
                    hlLocation.Y = Math.Min(wndContainer.Rectangle.Y, hlLocation.Y);
                    hlSize.Width = Math.Max(wndContainer.Rectangle.Width, hlSize.Width);
                    hlSize.Height += wndContainer.Rectangle.Height;
                    tileGroup = linkedComponent == null ?
                        wndContainer.Parent as MTileGroup : wndContainer.Parent.Parent.Parent as MTileGroup; //panelTab->Panel->TileGroup
                }
            }
        }

        //-----------------------------------------------------------------------------
        internal Rectangle GetRectFromContainer(TreeNode node)
		{
			EasyBuilderComponent ebComponent = node.Tag as EasyBuilderComponent;
            MLayoutContainer container = ebComponent as MLayoutContainer;
            if (container == null)
                return Rectangle.Empty;

            object linkedComponent = container.LinkedComponent;

			Size hlSize = new Size();
			Point hlLocation = new Point(20000, 20000);
			Rectangle rect = new Rectangle();
			MTileGroup tileGroup = null;

			if (linkedComponent is MTileGroup)
			{
				tileGroup = (linkedComponent as MTileGroup);
				rect = tileGroup.Rectangle;
			}

			else if (linkedComponent == null || linkedComponent is MTilePanelTab)
			{
                RecursiveCalcComponentRect(node, linkedComponent, ref hlLocation, ref hlSize, ref tileGroup);
				rect = new Rectangle(hlLocation.X, hlLocation.Y, hlSize.Width, hlSize.Height);
			}
			else if(linkedComponent is MView)
			{
				rect = (linkedComponent as MView).Rectangle;
			}

			tileGroup?.Activate();
			return rect;
		}

		//-----------------------------------------------------------------------------
		internal bool CanBeSelected(IWindowWrapper wrapper)
		{
			WindowWrapperContainer wrapperAsContainer = wrapper as WindowWrapperContainer;
			if (wrapperAsContainer != null && currentRemovedObjects.Contains(wrapperAsContainer))
				return false;

			IWindowWrapperContainer parent = wrapper.Parent as IWindowWrapperContainer;
			if (parent is MTilePanelTab && parent.Parent != null && !IsInEdit(parent.Parent))
					return false;
			if (!(wrapper is WindowWrapperContainer) && parent != null && !IsInEdit(parent))
				return false;

			return true;
		}

	}

}