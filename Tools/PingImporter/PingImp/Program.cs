using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Net;
using System.IO;

using Microarea.Internals.PingImporter.Properties;

namespace Microarea.Internals.PingImporter
{
	//=========================================================================
	class Program
	{
		private const string pingFileExt = ".ping";

		//---------------------------------------------------------------------
		static void Main(string[] args)
		{
			try
			{
				DoWork(
						Settings.Default.ftpHostName,
						Settings.Default.userName,
						Settings.Default.password,
						Settings.Default.pingWebServerHostName,
						args/*,
						"/iproev68/microarea.eu/ActivationKey/Pings",
						"/iproev68/microarea.eu/Registration/Pings"*/
						);
			}
			catch (Exception exc)
			{
				Console.WriteLine("PANIC: {0}", exc.ToString());
			}
		}

		//---------------------------------------------------------------------
		private static void DoWork(
			string ftpServerHostName,
			string userName,
			string password,
			string pingWebServerHostName,
			params string[] repoPaths
			)
		{
			IConfiguration ftpConf = new Configuration(
						 new Uri(String.Format("{0}{1}{2}", Uri.UriSchemeFtp, Uri.SchemeDelimiter, ftpServerHostName)),
						 userName,
						 password
						 );
			IPingFileManager pfm = new PingFileManager(ftpConf);
			it.microarea.www.ResponseBag response = null;
			using (it.microarea.www.Registration proxy = new it.microarea.www.Registration())
			{
				proxy.Url = String.Format(
					"{0}{1}{2}/Registration/Registration.asmx",
					Uri.UriSchemeHttp,
					Uri.SchemeDelimiter,
					pingWebServerHostName
					);

				Console.WriteLine("Import started");
				foreach (string repoPath in repoPaths)
				{
					Console.WriteLine();
					string[] files = pfm.ListFiles(repoPath);
					Console.WriteLine("{0} files to import from {1}", GetPingFilesCount(files), repoPath);

					string actualFileFullPath = null;
					string pingFileContent = null;
					foreach (string fileName in files)
					{
						if (String.Compare(Path.GetExtension(fileName), pingFileExt, StringComparison.InvariantCultureIgnoreCase) != 0)
							continue;

						Console.WriteLine("{0}:", fileName);
						Console.Write("\tDownloading...");
						actualFileFullPath = String.Format("{0}/{1}", repoPath, fileName);
						try
						{
							pingFileContent = pfm.DownloadFile(actualFileFullPath);
							Console.WriteLine("OK");
						}
						catch (Exception exc)
						{
							Console.WriteLine("KO: {0}", exc.ToString());
							continue;
						}

						Console.Write("\tImporting...");
						try
						{
							response = proxy.ImportPing(Settings.Default.authenticationToken, GetPingTimeStamp(fileName), pingFileContent);
							if (response.ReturnCode == 0)
								Console.WriteLine("OK");
							else
							{
								Console.WriteLine("KO: {0}", response.ReturnCodeExpl);
								continue;
							}
						}
						catch (Exception exc)
						{
							Console.WriteLine("KO: {0}", exc.ToString());
							continue;
						}

						Console.Write("\tDeleting from ftp server...");
						try
						{
							pfm.DeleteFile(actualFileFullPath);
							Console.WriteLine("OK");
						}
						catch (Exception exc)
						{
							Console.WriteLine("KO: {0}", exc.ToString());
						}
						Console.WriteLine();
					}
					Console.WriteLine("Import from {0} terminated", repoPath);
				}
				Console.WriteLine("Import terminated");
			}
		}

		//---------------------------------------------------------------------
		private static int GetPingFilesCount(string[] files)
		{
			int pingFilesCount = 0;
			foreach (string fileName in files)
			{
				if (String.Compare(Path.GetExtension(fileName), pingFileExt, StringComparison.InvariantCultureIgnoreCase) == 0)
					pingFilesCount++;
			}
			return pingFilesCount;
		}

		//---------------------------------------------------------------------
		private static DateTime GetPingTimeStamp(string fileName)
		{
			string[] tokens = Path.GetFileName(fileName).Split(new char[] { '_', '.' });

			int year = 0;
			bool ok = Int32.TryParse(tokens[2], out year);

			int month = 0;
			ok = Int32.TryParse(tokens[3], out month);

			int day = 0;
			ok = Int32.TryParse(tokens[4], out day);

			int hours24 = 0;
			ok = Int32.TryParse(tokens[6], out hours24);

			int minutes = 0;
			ok = Int32.TryParse(tokens[7], out minutes);

			int secs = 0;
			ok = Int32.TryParse(tokens[8], out secs);

			return new DateTime(year, month, day, hours24, minutes, secs);
		}
	}
}
