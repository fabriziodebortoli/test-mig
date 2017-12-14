using System;
using System.Diagnostics;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Interfaces;
//using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.UI.EasyLookCustomization;

namespace Microarea.Web.EasyLook
{

	//================================================================================
	public partial class ErrorPage : System.Web.UI.Page
	{
		protected override void OnInit (EventArgs e)
		{
			base.OnInit(e);

			EasyLookCustomSettings easyLookCustomSettings = (EasyLookCustomSettings)Session[EasyLookCustomSettings.SessionKey];
			if (easyLookCustomSettings != null)
			{
				Page.Form.Style[HtmlTextWriterStyle.FontFamily] = easyLookCustomSettings.FontFamily;
				Page.Form.Style[HtmlTextWriterStyle.FontSize] = FontUnit.XSmall.ToString();
			}

			Title = LabelStrings.EasyError;
			ErrorLabel.Text = LabelStrings.ErrorLabelText;
			ButtonDetails.Text = LabelStrings.ShowDetails;
			DetailLabel.Text = LabelStrings.ErrorDetails;
			StackTrace.Text = LabelStrings.Stack;
			CloseButton.Text = LabelStrings.Close;

			Exception ex = Session[Helper.LatestExceptionKey] as Exception;
			
			if (ex == null)
			{
				ButtonDetails.Visible = false;
				ErrorLabelDescription.Text = LabelStrings.UnknownError;
				return;
			}

			if (ex is HttpUnhandledException && ex.InnerException != null)
				ex = ex.InnerException;

			ErrorLabelDescription.Text = Microarea.TaskBuilderNet.UI.WebControls.Helper.InsertBR(ex.Message);
			string details = GetDetail(ex);
			
			DetailLabelDescription.Visible = true;
			DetailLabelDescription.Text = Microarea.TaskBuilderNet.UI.WebControls.Helper.InsertBR(details);
			DetailLabel.Visible = true;
			
			StackTraceDescription.Text = Microarea.TaskBuilderNet.UI.WebControls.Helper.InsertBR(ex.StackTrace);

			//Associo la funzione javascript che gestisce la visualizzazione dei dettagli viene gestita via javascript
			ButtonDetails.OnClientClick = string.Format("OnToggleDetails('{0}', '{1}');return false", LabelStrings.HideDetails, LabelStrings.ShowDetailsText);
			//i dettagli partono in stato nascosto
			DetailsPanel.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
		}

		//--------------------------------------------------------------------------------
		protected void Page_Load (object sender, EventArgs e)
		{
			

		}

		//--------------------------------------------------------------------------------
		private string GetDetail (Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			if (ex is IDetailedException)
			{
				string details = ((IDetailedException)ex).Details;
				if (details.Length > 0)
					sb.AppendLine(details);
			}

			if (ex.InnerException != null)
			{
				sb.AppendLine(ex.InnerException.Message);
				string details = GetDetail(ex.InnerException);
				if (details.Length > 0)
					sb.AppendLine(details);
			}
			return sb.ToString();
		}
	}
}
