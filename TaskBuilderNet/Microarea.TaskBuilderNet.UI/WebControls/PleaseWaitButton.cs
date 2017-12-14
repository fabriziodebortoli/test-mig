using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microarea.TaskBuilderNet.UI.WebControls
{
	#region Enums
	public enum PleaseWaitTypeEnum
	{
		TextThenImage,
		ImageThenText,
		TextOnly,
		ImageOnly
	}
	#endregion

	[DefaultProperty("Text"),
	ToolboxData("<{0}:PleaseWaitButton runat=server></{0}:PleaseWaitButton>")]
	public class PleaseWaitButton : Button
	{

		#region data member privati
		private string				pleaseWaitText	= String.Empty;
		private string				pleaseWaitImage = String.Empty;
		private PleaseWaitTypeEnum	pleaseWaitType	= PleaseWaitTypeEnum.TextThenImage;
		public string				labelString		= string.Empty;

		#endregion

		#region proprietà

		[Bindable(true),
		Category("Appearance")]
		public string PleaseWaitText
		{
			get
			{
				return pleaseWaitText;
			}

			set
			{
				pleaseWaitText = value;
			}
		}


		//---------------------------------------------------------------------
		[Bindable(true),
		Category("Appearance"),
		DefaultValue("")]
		public string PleaseWaitImage
		{
			get
			{
				return pleaseWaitImage;
			}

			set
			{
				pleaseWaitImage = value;
			}
		}

		//---------------------------------------------------------------------
		[Bindable(true),
		Category("Appearance"),
		DefaultValue(PleaseWaitTypeEnum.TextThenImage)]
		public PleaseWaitTypeEnum PleaseWaitType
		{
			get
			{
				return pleaseWaitType;
			}

			set
			{
				pleaseWaitType = value;
			}
		}

		#endregion

		#region RegisterJavascript Functions
		/// <summary>
		/// Funzione che registra gli script per nascondere e mostrare la gif
		/// animata della progress bar
		/// </summary>
		private void RegisterJavascriptFromResource()
		{
			this.Page.ClientScript.RegisterClientScriptBlock(Page.ClientScript.GetType(), "PleaseWaitButtonScript", PleaseWaitButtonConstStrings.sScript);
		}


		//---------------------------------------------------------------------
		/// <summary>
		/// Funzione che registra il  Javascript che inserisce  l'oggetto image
		/// </summary>
		/// <param name="sImage"></param>
		private void RegisterJavascriptPreloadImage(string sImage)
		{
			Regex rex = new Regex("[^a-zA-Z0-9]");
			string sImgName = "img_" + rex.Replace(sImage, "_"); 

			StringBuilder sb = new StringBuilder();
			sb.Append("<script language='JavaScript'>");
			sb.Append("if (document.images) { ");
			sb.AppendFormat("{0} = new Image();", sImgName);
			sb.AppendFormat("{0}.src = \"{1}\";", sImgName, sImage);
			sb.Append(" } ");
			sb.Append("</script>");

            Page.ClientScript.RegisterClientScriptBlock(Page.ClientScript.GetType(), sImgName + "_PreloadScript", sb.ToString());
		}

		#endregion

		#region OnInit
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			RegisterJavascriptFromResource();
		}
		#endregion

		#region GetControlHtml
		private string GetControlHtml(Control c)
		{
			StringWriter sw = new StringWriter();
			HtmlTextWriter writer = new HtmlTextWriter(sw);
			c.RenderControl(writer);
			string sHtml = sw.ToString();
			writer.Close();
			sw.Close();

			return sHtml;
		}
		#endregion

		#region OnPreRender
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender (e);

			if (pleaseWaitImage != String.Empty && pleaseWaitType != PleaseWaitTypeEnum.TextOnly)
				RegisterJavascriptPreloadImage(pleaseWaitImage);
		}
		#endregion

		#region Render
		//---------------------------------------------------------------------
		/// <summary>
		/// Render this control to the output parameter specified.
		/// </summary>
		/// <param name="output"> The HTML writer to write out to </param>
		protected override void Render(HtmlTextWriter output)
		{
			StringWriter	sw = new StringWriter();
			HtmlTextWriter	wr = new HtmlTextWriter(sw);
			base.Render(wr);
			string sButtonHtml = sw.ToString();

			wr.Close();
			sw.Close();
			
			sButtonHtml = ModifyJavaScriptOnClick(sButtonHtml);
			
			output.Write(string.Format("<div id='pleaseWaitButtonDiv2_{0}'>", this.ClientID));
			output.Write("</div>");

			output.Write(string.Format("<div id='pleaseWaitButtonDiv_{0}'>", this.ClientID));
			output.Write(sButtonHtml);
			output.Write("</div>");
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Create PleaseWaitJavascript function call to insert in html code by 
		/// Render function
		/// The code is different according to PleaseWaitTypeEnum
		/// </summary>
		/// <returns></returns>
		private string GeneratePleaseWaitJavascript()
		{
			string sMessage = "";
			string sText	= pleaseWaitText;
			string sImage	= (pleaseWaitImage != String.Empty 
				? string.Format(
				"<img src=\"{0}\" align=\"absmiddle\" alt=\"{1}\"/>"
				, pleaseWaitImage, pleaseWaitText )
				: String.Empty);

			switch (pleaseWaitType)
			{
				case PleaseWaitTypeEnum.TextThenImage:
					sMessage = sText + sImage;
					break;
				case PleaseWaitTypeEnum.ImageThenText:
					sMessage = sImage + sText;
					break;
				case PleaseWaitTypeEnum.TextOnly:
					sMessage = sText;
					break;
				case PleaseWaitTypeEnum.ImageOnly:
					sMessage = sImage;
					break;
			}

			string sCode = string.Format(
				"PleaseWait('pleaseWaitButtonDiv_{0}', 'pleaseWaitButtonDiv2_{1}', '{2}', '" + labelString + "');"
				, this.ClientID, this.ClientID, sMessage);
			sCode = sCode.Replace("\"", "&quot;");

			return sCode;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Insert JavaScript "PleaseWaint"  on onclick event
		/// </summary>
		/// <param name="sHtml"></param>
		/// <returns></returns>
		private string ModifyJavaScriptOnClick(string sHtml)
		{
			
			string sReturn = "";
			string sPleaseWaitCode = GeneratePleaseWaitJavascript();

			Regex rOnclick = new Regex("onclick=\"(?<onclick>[^\"]*)");
			Match mOnclick = rOnclick.Match(sHtml);
			if (mOnclick.Success)
			{
				string sExisting = mOnclick.Groups["onclick"].Value;
				string sReplace = sExisting 
					+ (sExisting.Trim().EndsWith(";") ? "" : "; ");
				
				if (IsValidatorIncludeScript() && this.CausesValidation)
				{
					string sCode = "if (Page_IsValid) " + sPleaseWaitCode + " return Page_IsValid;";
					sReplace = sReplace + sCode;
				}
				else
					sReplace = sReplace + sPleaseWaitCode;

				sReplace = "onclick=\"" + sReplace;
				sReturn = rOnclick.Replace(sHtml, sReplace);
			}
			else
			{
				int i = sHtml.Trim().Length - 2;
				string sInsert = " onclick=\"" + sPleaseWaitCode + "\" ";
				sReturn = sHtml.Insert(i, sInsert);				
			}
			
			return sReturn;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// IsStartupScriptRegistered
		/// </summary>
		/// <returns></returns>
		private bool IsValidatorIncludeScript()
		{
			return this.Page.ClientScript.IsStartupScriptRegistered("ValidatorIncludeScript");
		}
		#endregion

	}

	//=========================================================================
	/// <summary>
	/// Class of constant strings of the Custom Control
	/// </summary>
	public class PleaseWaitButtonConstStrings
	{
		public const string sScript = @"<script language='JavaScript'>
									function GetDiv(sDiv)
									{
										var div;
										if (document.getElementById)
											div = document.getElementById(sDiv);
										else if (document.all)
											div = eval('window.' + sDiv);
										else if (document.layers)
											div = document.layers[sDiv];
										else
											div = null;

										return div;
									}

									function HideDiv(sDiv)
									{
										d = GetDiv(sDiv);
										if (d)
										{
											if (document.layers) d.visibility = 'hide';
											else d.style.visibility = 'hidden';
										}
									}

									function PleaseWait(sDivButton, sDivMessage, sInnerHtml, label)
									{
										opener.parent.PostBack();
										HideDiv(sDivButton);
										var d = GetDiv(sDivMessage);
										if (d) d.innerHTML = sInnerHtml;
										var lab = document.getElementById(label);
										if (lab)
											lab.style.visibility = 'hidden';
									
									}
									</script>";
	}
}
