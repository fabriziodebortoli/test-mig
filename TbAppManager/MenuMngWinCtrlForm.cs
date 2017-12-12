using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;
using Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer;
using WeifenLuo.WinFormsUI.Docking;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.MenuManager
{
	public partial class MenuMngWinCtrlForm : DockContent
	{
		MenuMngWinCtrl menuManagerWinCtrl;
		public event EventHandler<TbNavigateEventArgs> RunCommand;

		public MenuMngWinCtrlForm(IPathFinder pathFinder, LoginManager loginManager)
		{
			InitializeComponent();

			menuManagerWinCtrl = new MenuMngWinCtrl();
			menuManagerWinCtrl.Dock = DockStyle.Fill;
			this.Controls.Add(menuManagerWinCtrl);
			menuManagerWinCtrl.LoginManager = loginManager;

            if (loginManager.AuthenticationToken != string.Empty)
            {
                loginManager.GetLoginInformation(loginManager.AuthenticationToken);
                pathFinder = new PathFinder(loginManager.CompanyName, loginManager.UserName);
            }

            menuManagerWinCtrl.PathFinder = pathFinder;
			menuManagerWinCtrl.RunCommand += MenuManagerWinCtrl_RunCommand;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			//carica il menu con l'environment come applicazione standalone
			MenuLoader ml = new MenuLoader(menuManagerWinCtrl.PathFinder);

			ml.LoadAllMenus(false, true);
			menuManagerWinCtrl.MenuXmlParser = ml.AppsMenuXmlParser;
			menuManagerWinCtrl.RefreshAppMenu();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void MenuManagerWinCtrl_RunCommand(object sender, MenuMngCtrlEventArgs e)
		{
			TbNavigateEventArgs tb = new TbNavigateEventArgs();
			tb.Namespace = e.ItemObject;

			if (e.ItemType.IsRunReport)
				tb.Type = NameSpaceObjectType.Report;
			if (e.ItemType.IsRunDocument)
				tb.Type = NameSpaceObjectType.Document;
			if (e.ItemType.IsRunBatch)
				tb.Type = NameSpaceObjectType.Document;
			if (e.ItemType.IsApplication)
				tb.Type = NameSpaceObjectType.Application;
			if (e.ItemType.IsRunFunction)
				tb.Type = NameSpaceObjectType.Function;
			if (e.ItemType.IsRunText)
				tb.Type = NameSpaceObjectType.Text;

			if (RunCommand != null)
				RunCommand(this, tb);
		}

	}
}
