
using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microarea.TaskBuilderNet.Woorm.WebControls
{
	public class Helper
	{
		public static readonly Type DefaultReferringType = typeof(Helper);
		public const string DocumentParametersControlName = "__DocumentInitParameters";

		private static string linkDocumentFunctionScript = null;
		//--------------------------------------------------------------------------
		private static string LinkDocumentFunctionScript
		{
			get
			{
				if (linkDocumentFunctionScript == null)
				{
					string templateString = string.Format("<script type='text/javascript'>{0}</script>", WebControlsStrings.OpenDocumentJScript);
					string s = templateString.Replace("##InputName##", Helper.DocumentParametersControlName);
					s = s.Replace("##LoadingMessage##", WebControlsStrings.Loading);
					linkDocumentFunctionScript = s.Replace("##PopupDeniedMessage##", WebControlsStrings.PopupDenied);
				}

				return linkDocumentFunctionScript;
			}
		}

		//--------------------------------------------------------------------------
		public static void RegisterLinkDocumentFunction(Page page)
		{
			if (!page.ClientScript.IsClientScriptBlockRegistered("LinkDocumentFunction"))
				page.ClientScript.RegisterClientScriptBlock(page.GetType(), "LinkDocumentFunction", Helper.LinkDocumentFunctionScript);
		}

		//---------------------------------------------------------------------
		public static void AddAdjustTargetEvent(HyperLink link)
		{
			link.Attributes["onclick"] = "var now = new Date(); this.target = 'doc' + now.getHours() + now.getMinutes() + now.getSeconds() + now.getMilliseconds()";
		}
		//--------------------------------------------------------------------------
		public static string GetRunDocumentUrl(string docNamespace, string message, string parameters)
		{
			return string.Format("LinkDocument.axd?Action={0}&Parameters={1}&Alert={2}",
										HttpUtility.UrlEncode(string.Format("TBWindowForm.aspx?DocumentNamespace={0}", HttpUtility.UrlEncode(docNamespace))),
										parameters,
										HttpUtility.UrlEncode(message)
										);
		}

		//--------------------------------------------------------------------------
		public static string GetRunReportUrl(string reportNamespace, string message, string parameters)
		{
			string queryString = "WoormWebForm.aspx?namespace=" + HttpUtility.UrlEncode(reportNamespace);

			return string.Format("LinkDocument.axd?Action={0}&Parameters={1}&Alert={2}",
						HttpUtility.UrlEncode(queryString),
						parameters,
						message
						);
			
		}

		//--------------------------------------------------------------------------
		public static object GetRunReportUrlFromFile(string filename, string message, string parameters)
		{
			string queryString = "WoormWebForm.aspx?filename=" + HttpUtility.UrlEncode(filename);

			return string.Format("LinkDocument.axd?Action={0}&Parameters={1}&Alert={2}",
						HttpUtility.UrlEncode(queryString),
						parameters,
						message
						);
		}

		//--------------------------------------------------------------------------
		public static string GetRunCurrentReportUrl(string message)
		{
			return string.Format("LinkDocument.axd?Action={0}&Parameters={1}&Alert={2}",
										HttpUtility.UrlEncode("WoormWebForm.aspx?CurrentReport=true"),
										"",
										HttpUtility.UrlEncode(message)
										);
		}

		//------------------------------------------------------------------------------
		public static string InsertBR (string text)
		{
			string changed = "";
			if (text != null)
			{
				changed = text.Replace("\r\n", "<br/>");
				changed = changed.Replace("\n\r", "<br/>");
				changed = changed.Replace("\r", "<br/>");
				changed = changed.Replace("\n", "<br/>");
			}
			return changed;
		}

		///<summary>
		///Metodo che rimuove i singoli ampersand( usati lato c++ per marcare la lettera acceleratore)
		///e sostituisce i doppi ampersand consecuntivi, con il singolo (Se son omessi doppi significa
		///che l'intento era visualizzarne uno)
		///Es.
		/// S&tringa -> Stringa
		/// Str&&inga -> Str&inga
		/// </summary>

		//------------------------------------------------------------------------------
		public static string AdjustAmpersand(string text)
		{
			string changed = "";
			if (text != null)
			{
				//rimuovo i singoli ampersand (sono quelli che segnano il carattere come acceleratore) 
				string patternSingleAmpersand =  "([^&]*)&{1}([^&])";
				changed = Regex.Replace(text, patternSingleAmpersand, @"$1$2");
				//sostituisco i doppi ampersand con il singolo ampersand
				string patternDoubleAmpersand = "&&";
				changed = Regex.Replace(changed, patternDoubleAmpersand, "&");
			}
			return changed;
		}
	}
}
