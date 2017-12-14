using System;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;

namespace Microarea.TaskBuilderNet.Forms
{
	internal class TBWFCUITabber : TBCUI
	{
		//-------------------------------------------------------------------------
		internal TBWFCUITabber(IUIControl ctrl)
			:
			base(ctrl, NameSpaceObjectType.Tabber)
		{
		}

		/// <summary>
		/// </summary>
		//-------------------------------------------------------------------------
		public void AddTabPage(Type tabType, string tabTitle, IUIUserControl parent)
		{
			UITabDialog tbTabPage = Activator.CreateInstance(tabType) as UITabDialog;
			if (tbTabPage == null)
				return;

			UITabControl tabber = this.Component as UITabControl;

			tbTabPage.Dock = DockStyle.Fill;

			UITabPage page = new UITabPage();
			page.Controls.Add(tbTabPage);

			IUIDocumentPart uiPart = parent as IUIDocumentPart;
			if (uiPart != null)
			{
				tbTabPage.DocumentPart = uiPart.DocumentPart;
			}

			page.Text = tabTitle;
			tabber.UIPages.Add(page);

			tbTabPage.CreateComponents();
		}
	}
}
