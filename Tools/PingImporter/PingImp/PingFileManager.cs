using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Microarea.Internals.PingImporter
{
	//=========================================================================
	public class PingFileManager : IPingFileManager
	{
		private IConfiguration configuration;

		//---------------------------------------------------------------------
		public PingFileManager(IConfiguration configuration)
		{
			if (configuration == null)
				throw new ArgumentNullException("configuration");

			this.configuration = configuration;
		}

		#region IPingFileManager Members

		//---------------------------------------------------------------------
		public string[] ListFiles(string repository)
		{
			if (repository == null)
				throw new ArgumentNullException("repository");

			FtpWebRequest ftp = CreateFtpWebRequest(repository);

			ftp.Method = WebRequestMethods.Ftp.ListDirectory;

			return GetResponse(ftp);
		}

		//---------------------------------------------------------------------
		public string DownloadFile(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			FtpWebRequest ftp = CreateFtpWebRequest(filePath);

			ftp.Method = WebRequestMethods.Ftp.DownloadFile;

			string[] files = GetResponse(ftp);
			if (files == null || files.Length != 1)
				throw new WebException(String.Format("Error downloading {0}", filePath));

			return files[0];
		}

		//---------------------------------------------------------------------
		public void DeleteFile(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath");

			FtpWebRequest ftp = CreateFtpWebRequest(filePath);

			ftp.Method = WebRequestMethods.Ftp.DeleteFile;

			GetResponse(ftp);
		}

		#endregion

		//---------------------------------------------------------------------
		private FtpWebRequest CreateFtpWebRequest(string resourcePath)
		{
			string ftpUri = String.Format(
						 "ftp://{0}:{1}@{2}/{3}",
						 configuration.UserName,
						 configuration.Password,
						 configuration.FtpUri.Host,
						 resourcePath
						 );
			FtpWebRequest ftp = WebRequest.Create(ftpUri) as FtpWebRequest;
			ftp.Proxy = null;
			ftp.Credentials = new NetworkCredential(configuration.UserName, configuration.Password);

			return ftp;
		}

		//---------------------------------------------------------------------
		private string[] GetResponse(FtpWebRequest ftp)
		{
			List<string> files = new List<string>();
			string buffer = null;

			FtpWebResponse response = ftp.GetResponse() as FtpWebResponse;

			using (Stream responseStream = response.GetResponseStream())
			using (StreamReader responseReader = new StreamReader(responseStream))
			{
				buffer = responseReader.ReadLine();
				if (buffer == null)
				{
					if (response.StatusCode != FtpStatusCode.FileActionOK)
					{
						response.Close();
						throw new WebException("Operation terminated with errors.");
					}
					response.Close();
					return new string[] { };
				}

				while (!String.IsNullOrWhiteSpace(buffer))
				{
					files.Add(buffer);
					buffer = responseReader.ReadLine();
				}
			}
			response.Close();
			return files.ToArray();
		}
	}
}
