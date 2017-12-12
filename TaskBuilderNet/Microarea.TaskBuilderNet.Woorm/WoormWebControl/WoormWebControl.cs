using System;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormWebControl;
using System.Web.UI.HtmlControls;


[assembly: WebResource(WoormWebControl.ScriptUrl,"text/javascript")]

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	/// <summary>
	/// Nomi delle immagini utilizzate dal control
	/// </summary>
	/// ================================================================================
	internal class ImageNames
	{
		public const string FirstPage			= "FirstPage.gif";
		public const string PrevPage			= "PrevPage.gif";		
		public const string NextPage			= "NextPage.gif";		
		public const string LastPage			= "LastPage.gif";		
		public const string Run					= "Run.gif";
		public const string RunExec				= "RunExec.png";
		public const string ToggleMode			= "ToggleMode.gif";		
		public const string SaveForUser			= "SaveForUser.gif";
		public const string SaveForAllUsers		= "SaveForAllUsers.gif";
		public const string Exit				= "Exit.gif";
		public const string PrintHtml			= "PrintHtml.gif";
		public const string PrintPdf			= "PrintPdf.gif";
        public const string PrintXls            = "PrintXls.gif";
        public const string HotLink             = "Hotlink.gif";
		public const string DisabledHotLink		= "DisabledHotlink.gif";
		public const string ErrorIcon			= "ErrorIcon.png";
	}

	/// <summary>
	/// Descrizione di riepilogo per ViewerControl.
	/// </summary>
	/// ================================================================================
	public  class SessionKey
	{
		public static string ReportPath			= "ReportNameSpace";
	}
	//==================================================================================
	public class WoormWebControl : Control, INamingContainer
	{
		internal const string ScriptUrl = "Microarea.TaskBuilderNet.Woorm.WoormWebControl.WoormClientControl.js";
		public static readonly Type DefaultReferringType = typeof(WoormWebControl);
		private ReportController controller;
		
		private		  string                    controlToFocusID = "";
		private		  ConditionalUpdatePanel	mainUpdatePnl;
		//--------------------------------------------------------------------------
		internal ConditionalUpdatePanel MainUpdatePnl
		{
			get { return mainUpdatePnl; }
		}

		//--------------------------------------------------------------------------------------
		public string OwnerApplication { get; set;} 
		//--------------------------------------------------------------------------------------
		public string OwnerModule { get; set;} 
		//--------------------------------------------------------------------------
		public RSEngine StateMachine { get { return controller.StateMachine; } }
		//--------------------------------------------------------------------------
		public static string AssemblyName = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
		
		//Memorizza l'ID del controllo che aveva il fuoco prima di ricostruire da zero 
		//le ask dialog in presenza di campi auto assegnati, per reimpostarlo sulla
		//nuova pagina visualizzata
		//--------------------------------------------------------------------------
		public string ControlToFocusID
		{
			get { return controlToFocusID; }
			set { controlToFocusID = value; }
		}
	
		//--------------------------------------------------------------------------
		public void RadarButtonSelectionClick(object sender, EventArgs e)
		{
			if (controller.ExecuteDataFromRadar(((LinkButton)sender).Text))
				RebuildControls();
		}

		//--------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);

			if (DesignMode)
				return;

			if (Page.Request.Params[Helper.PrintParam] != null)
			{
				controller = TBWebContext.Current.FromSession(Page.Request.Params[Helper.PrintParam]) as ReportController;
				if (controller != null)
					controller.Print();
				return;
			}	
				
			controller = ReportController.FromSession(Page.Request.Params);

			Microarea.TaskBuilderNet.Woorm.WebControls.Helper.RegisterLinkDocumentFunction(Page);

			controller.InitStateMachine(Page.IsPostBack);
			controller.StateMachine.Step();
			//Imposto il titolo localizzato alla pagina
			Page.Title = controller.LocalizedReportTitle;
			
			//option 1: fails -> too early sys is yet undefined (rendered before client framework in html page)
			//ClientScriptManager cs = this.Page.ClientScript;
			//cs.RegisterClientScriptResource(GetType(),ScriptUrl);
			//option 2:  fails -> too early sys is yet undefined ()(rendered before client framework in html page)
			//ScriptManager.RegisterClientScriptResource(this,GetType(),ScriptUrl);	
			ScriptManager sm = ScriptManager.GetCurrent(this.Page);
			if (sm != null)
			{
				string assemblyName = Assembly.GetExecutingAssembly().FullName;
				sm.Scripts.Add(new ScriptReference(ScriptUrl, assemblyName));
			}
		}


		//--------------------------------------------------------------------------
		public void RebuildControls()
		{
			// Clear any existing child controls.
			Controls.Clear();

			// Clear any previous view state for the existing child controls.
			ClearChildViewState();

			// Create child controls again.
			ChildControlsCreated = false;
			EnsureChildControls();
		}


		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			mainUpdatePnl = new ConditionalUpdatePanel(true);
			mainUpdatePnl.UpdateMode = UpdatePanelUpdateMode.Conditional;
			Controls.Add(mainUpdatePnl);

			
		 string reportIdentifierField = string.Format("<input type='hidden' value = '{0}' name = '{1}'/>",
					controller.StateMachineSessionTag,
					ReportController.StateMachineSessionTagIdentifier
					);

			//used to persist current report state acrosso round trips
			mainUpdatePnl.ContentTemplateContainer.Controls.Add(new LiteralControl(reportIdentifierField));

			switch (controller.RenderingStep())
			{
				case HtmlPageType.Error:
				{
					ErrorPage error = new ErrorPage(this);
					mainUpdatePnl.ContentTemplateContainer.Controls.Add(error);
					break;
				}

				case HtmlPageType.Viewer:
				{
					AspNetRender viewer = new AspNetRender(controller.Woorm, controller.StateMachineSessionTag, this);
					mainUpdatePnl.ContentTemplateContainer.Controls.Add(viewer);
					break;
				}

				case HtmlPageType.Print:
				{
					PrintViewerControl viewer = new PrintViewerControl(controller.Woorm, controller.StateMachineSessionTag, this);
					mainUpdatePnl.ContentTemplateContainer.Controls.Add(viewer);
					break;
				}

				case HtmlPageType.Form:
				{
					AspNetAskWebForm form = new AspNetAskWebForm(controller.Report.CurrentAskDialog, this);
					mainUpdatePnl.ContentTemplateContainer.Controls.Add(form);
					break;
				}
			
				case HtmlPageType.HotLink:
				{
					HotLinkForm form = new HotLinkForm(controller.Report.CurrentAskDialog, controller.HotLinkFormKey, this);
					mainUpdatePnl.ContentTemplateContainer.Controls.Add(form);

					break;
				}

				case HtmlPageType.Persister:
				{
					PersisterControl persister = new PersisterControl(controller.Woorm, this);
					mainUpdatePnl.ContentTemplateContainer.Controls.Add(persister);

					break;
				}
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			////COMMENTATO PER USCITA 3_8 (LA MODIFICA E" PER MAGO INFINITY)
			//AddCssLink("WoormClientControlInfinity.css");
			AddCssLink("WoormClientControl.css");
		}

		//--------------------------------------------------------------------------------------
		private string GetCssFile(string cssFileName)
		{
			string file = cssFileName;
			UserInfo ui = UserInfo.FromSession();
			if (ui != null)
			{
				string cssFile = Path.Combine(ui.PathFinder.GetCustomModuleTextPath(ui.Company, ui.User, OwnerApplication, OwnerModule), file);
				if (File.Exists(cssFile))
					file = cssFile;
			}
			return file;
		}

		//--------------------------------------------------------------------------------------
		private void AddCssLink(string cssFileName)
		{
			string file = GetCssFile(cssFileName);
			HtmlLink link = new HtmlLink();
			link.ID = cssFileName;
			link.Href = TbCssProvider.GetCssUrl(file);
			link.Attributes.Add("type", "text/css");
			link.Attributes.Add("rel", "stylesheet");
			Page.Header.Controls.AddAt(0, link);
		}
	}
}
