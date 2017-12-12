using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook
{
	/// <summary>
	/// Summary description for ChangeApplicationData.
	/// </summary>
	public partial class ChangeApplicationDate : System.Web.UI.Page
	{
		protected Microarea.Web.EasyLook.DateSelectionUserControl DateSelectionControl;
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			
			// Disabilita la cache
			this.Response.Cache.SetCacheability(HttpCacheability.NoCache);

			//Setto la culture
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
				return;
			
			ui.SetCulture();

			int[] docs = ui.EnumActiveDocuments();
			if (docs.Length > 0)
			{
				DateSelectionControl.Visible = false;
				Label l = new Label();
				l.Text = LabelStrings.CannotChangeOperationsDateMessageText;
				l.Width = Unit.Percentage(100);
				DateSelectionPanel.Controls.Add(l);
			}

			Page.Title = CommonFunctions.GetBrandedTitle();

			if (!Page.IsPostBack)
			{
				//Applico i setting inseriti dal PlugIn
				ApplyCustomSetting();
				SetLabelsText();

				CultureInfo culture			= CultureInfo.CreateSpecificCulture(Thread.CurrentThread.CurrentCulture.Name);
				CurrentDateValueLabel.Text	= ui.ApplicationDate.ToString(culture.DateTimeFormat.ShortDatePattern);
				DateSelectionControl.Date	= ui.ApplicationDate;
			}
		}


		//---------------------------------------------------------------------
		private void SetLabelsText()
		{
			TitleLabel.Text					= LabelStrings.ChangeDateTitle;
			CurrentDateCaptionLabel.Text	= LabelStrings.CurrentDateValueLabelText;
			NewDateCaptionLabel.Text		= LabelStrings.NewDateValueLabelText;
			OkButton.Text					= LabelStrings.OkButtonText;
			CancelButton.Text				= LabelStrings.CancelButtonText;
		}

		//---------------------------------------------------------------------
		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    

		}
		#endregion

		protected void OkButton_Click(object sender, System.EventArgs e)
		{
			UserInfo ui = UserInfo.FromSession();
			if (ui == null)
				return;

			int[] docs = ui.EnumActiveDocuments();
			if (docs.Length == 0)
				ui.ApplicationDate = DateSelectionControl.Date;
			
			RegisterCloseScript();
		}

		//---------------------------------------------------------------------
		private void RegisterCloseScript()
		{
			string scriptValue	=  @"this.window.close();";	
		
			ScriptManager.RegisterStartupScript (this, ClientScript.GetType(), "close", scriptValue, true);
		}

		//---------------------------------------------------------------------
		protected void CancelButton_Click(object sender, System.EventArgs e)
		{
			RegisterCloseScript();
		}

		//---------------------------------------------------------------------
		private void ApplyCustomSetting()
		{
			if (Session[EasyLookCustomSettings.SessionKey] == null)
				return;

			EasyLookCustomSettings	setting	= (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];

			if (setting == null)
				return;

			//Font della label e della combo
			TitleLabel.Font.Name				= setting.FontFamily;
			CurrentDateCaptionLabel.Font.Name	= setting.FontFamily;
			CurrentDateValueLabel.Font.Name		= setting.FontFamily;
			NewDateCaptionLabel.Font.Name		= setting.FontFamily;
			DateSelectionControl.FontName		= setting.FontFamily;
			OkButton.Font.Name					= setting.FontFamily;
			CancelButton.Font.Name				= setting.FontFamily;

		}
	}
}
