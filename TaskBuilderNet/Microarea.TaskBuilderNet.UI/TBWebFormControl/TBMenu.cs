using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	//==========================================================================================
	class TBContextMenu : Panel
	{
		MenuDescription menuDescription;
		string ownerId = "";

		public MenuDescription MenuDescription
		{
			get { return menuDescription; }
		}
		//--------------------------------------------------------------------------------------
		public TBContextMenu(MenuDescription desc, string ownerId, bool isSubMenu, string progressiveID)
		{
			this.menuDescription = desc;
			this.ownerId = ownerId;
			this.ID = progressiveID;
			if (isSubMenu)
			{
				Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
				Style.Add(HtmlTextWriterStyle.Display, "none");
			}
			Table menuTable = new Table();
			menuTable.CellPadding = 0;
			menuTable.CellSpacing = 0;
			menuTable.CssClass = "TBMenuTable";
			for (int i = 0; i < menuDescription.Childs.Count; i++)
			{
				MenuItemDescription menuItemDesc = (MenuItemDescription)menuDescription.Childs[i];
				TableRow rowItem = new TableRow();

				TableCell cellFlag = new TableCell();
				cellFlag.VerticalAlign = VerticalAlign.Middle;
				rowItem.Cells.Add(cellFlag);
				if (menuItemDesc.Checked)
				{
					ImageButton imageFlag = new ImageButton();
					imageFlag.ImageUrl = ImagesHelper.CreateImageAndGetUrl("CheckMark.png", TBWebFormControl.DefaultReferringType);
					cellFlag.Controls.Add(imageFlag);
					imageFlag.Enabled = menuItemDesc.Enabled;
				}

				TableCell cellText = new TableCell();
				cellText.VerticalAlign = VerticalAlign.Middle;
				Button btnText = new Button();
				btnText.Text = menuItemDesc.HtmlTextAttribute;
				btnText.CssClass = "TBMenuItem";
				btnText.Enabled = menuItemDesc.Enabled;

				cellText.Controls.Add(btnText);
				rowItem.Cells.Add(cellText);
				
				if (menuItemDesc.SubMenu != null)
				{
					TableCell cellArrowSubMenu = new TableCell();
					cellArrowSubMenu.VerticalAlign = VerticalAlign.Middle;
				
					ImageButton imageArrowSubMenu = new ImageButton();
					imageArrowSubMenu.ImageUrl = ImagesHelper.CreateImageAndGetUrl("ArrowRight.png", TBWebFormControl.DefaultReferringType);
					imageArrowSubMenu.Enabled = menuItemDesc.Enabled;
					cellArrowSubMenu.Controls.Add(imageArrowSubMenu);

					rowItem.Cells.Add(cellArrowSubMenu);

					TableCell cellSubMenu = new TableCell();
					cellSubMenu.VerticalAlign = VerticalAlign.Middle;

					TBContextMenu subMenu = new TBContextMenu(menuItemDesc.SubMenu, ownerId, true, ID + i.ToString());

					cellSubMenu.Controls.Add(subMenu);
					rowItem.Cells.Add(cellSubMenu);
					rowItem.Attributes.Add("onmouseover", String.Format("onShowSubMenu('{0}');", subMenu.ClientID));
				}
				else
				{
					rowItem.Style[HtmlTextWriterStyle.Cursor] = "hand";
					rowItem.Attributes["onclick"] = String.Format("onClickMenu('{0}','{1}','{2}');", ownerId, menuDescription.Hwnd, menuItemDesc.Cmd);
				}


				menuTable.Rows.Add(rowItem);
			}
			Controls.Add(menuTable);
		}
	}
}
