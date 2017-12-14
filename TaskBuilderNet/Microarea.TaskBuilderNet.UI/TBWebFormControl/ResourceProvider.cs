using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Microarea.TaskBuilderNet.Core.Applications;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{

	//================================================================================
	internal class TbWebFormResourceProvider : IHttpHandler, IReadOnlySessionState
	{
		/// <summary>
		/// Process the request for the image
		/// </summary>
		/// <param name="context">The current HTTP context</param>
		//--------------------------------------------------------------------------------
		void IHttpHandler.ProcessRequest (System.Web.HttpContext context)
		{
			string pdf = context.Request.QueryString["pdf"];
			if (!string.IsNullOrEmpty(pdf))
			{
				RenderPdf(pdf, context);
				return;
			}
			string folder = context.Request.QueryString["folder"];
			if (!string.IsNullOrEmpty(folder))
			{
				SendFileToClient(folder, context);
				return;
			}
			string css = context.Request.QueryString["css"];
			if (!string.IsNullOrEmpty(css))
				RenderCss(css, context);
			
			string img = context.Request.QueryString["img"];
			if (!string.IsNullOrEmpty(img))
				RenderImage(img, context);

			string script = context.Request.QueryString["script"];
			if (!string.IsNullOrEmpty(script))
				RenderScript(script, context);
		}

		//--------------------------------------------------------------------------------
		private static void RenderScript(string script, HttpContext context)
		{
			object scriptObject = context.Session[script];
			if (scriptObject != null)
			{
				context.Response.Write(scriptObject);
				return;
			}
			using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(script))
			{
				if (s != null)
					WriteStreamToResponse(context, s);
			}
		}

		//--------------------------------------------------------------------------------
		private void RenderImage(string img, HttpContext context)
		{
			using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("{0}.{1}", GetType().Namespace, img)))
			{
				if (s != null)
					WriteStreamToResponse(context, s);
			}
		}

		//--------------------------------------------------------------------------------
		private static void RenderPdf (string pdf, HttpContext context)
		{
			try
			{
				context.Response.ClearContent();

				UserInfo ui = UserInfo.FromSession();
				string filePath = Path.Combine(ui.PathFinder.GetWebProxyImagesPath(), pdf);
				using (FileSystemWatcher fsw = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath)))
				{
					fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
					int i = 0;
					while (true)
					{
						if (File.Exists(filePath))
							break;

						WaitForChangedResult result = fsw.WaitForChanged(WatcherChangeTypes.Created, 1000);

						if (!result.TimedOut)
							break;

						if (i++ > 60)
							throw new TimeoutException();
					}
				}

				bool locked = true;
				DateTime start = DateTime.Now;
				while (locked)
				{
					try
					{
						using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
						{
							locked = false;
						};
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine(string.Format("File '{0}' is locked", filePath));
						if ((DateTime.Now - start).Minutes >= 2)
							throw new TimeoutException();
					}
				}

				FileInfo file = new FileInfo(filePath); 
				context.Response.AddHeader("Content-Disposition", "attachment; filename=" + filePath);
				context.Response.AddHeader("Content-Length", file.Length.ToString());
				context.Response.ContentType = "application/pdf";
				context.Response.WriteFile(filePath, true);
				File.Delete(filePath);
			}
			catch (Exception ex)
			{
				context.Response.ContentType = "text/html";
				context.Response.Write(ex.ToString());
			}
		}

		///<summary>
		///Metodo che invia l'unico file presente nella folder al client, scrivendo sullo stream della response.
		///La folder che riceve come argomento ha il nome generato tramite GUID (in questa folder la TbFileDialog di TaskBuilder salva 
		///un unico file, che questo metodo va a leggere, senza guardare il nome)
		///Dopo averlo scritto nella response, cancella la folder.
		/// </summary>
		//--------------------------------------------------------------------------------
		private static void SendFileToClient (string folder, HttpContext context)
		{
			string filePath = "";
			try
			{
				context.Response.ClearContent();
				UserInfo ui = UserInfo.FromSession();
				string folderPath = Path.Combine(ui.PathFinder.GetWebProxyImagesPath(), folder);

				using (FileSystemWatcher fsw = new FileSystemWatcher(folderPath))
				{
					fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
					int i = 0;
					while (true)
					{
						WaitForChangedResult result = fsw.WaitForChanged(WatcherChangeTypes.Created, 1000);
						//cerco il file da downloadare nella Directory con il nome = GUID
						string[] files = Directory.GetFiles(folderPath);
						//cerco l'unico file detro la cartella con nome = GUID
						if (files.Length == 1 && File.Exists(files[0]))
						{
							filePath = files[0];
							break;
						}

						if (!result.TimedOut)
							break;

						if (i++ > 60)
							throw new TimeoutException();
					}
				}

				bool locked = true;
				DateTime start = DateTime.Now;
				long byteFileLenght = 0;
				//apro il file in lettura
				while (locked)
				{
					try
					{
						using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
						{
							byteFileLenght = fs.Length;
							locked = false;
						};
					}
					catch
					{
						System.Diagnostics.Debug.WriteLine(string.Format("File '{0}' is locked", filePath));
						if ((DateTime.Now - start).Minutes >= 2)
							throw new TimeoutException();
					}
				}
				//scrivo il file nella response
				context.Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
				context.Response.AddHeader("Content-Length", byteFileLenght.ToString());
				context.Response.ContentType = GetMimeType(filePath);
				context.Response.WriteFile(filePath, true);
				Directory.Delete(folderPath, true);
			}
			catch (Exception ex)
			{
				context.Response.ContentType = "text/html";
				context.Response.Write(ex.ToString());
			}
		}

		/// <summary>
		/// Metodo che restituisce il mimetype indicato per l'estensione del file passato come parametro
		/// (Per vedere l'elenco completo http://www.asciitable.it/mimetypes.asp)
		/// </summary>
		//--------------------------------------------------------------------------------
		private static string GetMimeType(string filePath)
		{
			string extension = Path.GetExtension(filePath);

			switch (extension)
			{
				case ".txt":
				case ".ban":
					return "text/plain";

				case ".xml":
				case ".xbrl":
					return "text/xml";
				// se non ho trovato mapping esplicito, lo tratto come bianrio
				default:
					return "application/octet-stream";
			}
		}

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
			return string.Format("~/TBWebFormResource.axd?css={0}", HttpUtility.UrlEncode(cssFile));
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