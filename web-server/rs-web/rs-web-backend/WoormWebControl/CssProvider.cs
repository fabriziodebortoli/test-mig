using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Reflection;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	class TbCssProvider:  IHttpHandler, IReadOnlySessionState
	{
		/// <summary>
		/// Process the request for the image
		/// </summary>
		/// <param name="context">The current HTTP context</param>
		//--------------------------------------------------------------------------------
		void IHttpHandler.ProcessRequest (System.Web.HttpContext context)
		{
			string css = context.Request.QueryString["css"];
			if (!string.IsNullOrEmpty(css))
				RenderCss(css, context);
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// This handler can be reused, it doesn't need to be recycled
		/// </summary>
		//--------------------------------------------------------------------------------
		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	
		//--------------------------------------------------------------------------------
		public static string GetCssUrl (string cssFile)
		{
			return string.Format("~/TBCssResource.axd?css={0}", HttpUtility.UrlEncode(cssFile));
		}

		//--------------------------------------------------------------------------------
		private void RenderCss (string css, HttpContext context)
		{
			context.Response.ContentType = "text/css";
			string file = css;
			if (!File.Exists(file))
				file = HttpContext.Current.Server.MapPath(css);

			if (File.Exists(file))
			{
				using (StreamReader sr = new StreamReader(file))
				{
					string text = sr.ReadToEnd();
					byte[] buffer = Encoding.UTF8.GetBytes(text);
					context.Response.OutputStream.Write(buffer, 0, buffer.Length);
				}
			}
			else
			{
				using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.{1}", GetType().Namespace, css)))
				{
					if (s != null)
						WriteStreamToResponse(context, s);
				}
			}

		}

		//--------------------------------------------------------------------------------
		private static void WriteStreamToResponse(HttpContext context, Stream s)
		{
			s.Seek(0, SeekOrigin.Begin);
			byte[] buffer = new byte[s.Length];
			s.Read(buffer, 0, buffer.Length);
			context.Response.OutputStream.Write(buffer, 0, buffer.Length);
		}
	}
}