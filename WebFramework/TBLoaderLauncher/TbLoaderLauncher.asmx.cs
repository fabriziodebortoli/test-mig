using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Microarea.WebServices.TBLoaderLauncher
{
	/// <summary>
	/// Summary description for TBLoaderLauncher
	/// </summary>
	//==================================================================================
	[WebService(Namespace = "http://microarea.it/TBLoaderLauncher/")]
	public class TBLoaderLauncher : System.Web.Services.WebService
	{

		[WebMethod]
		public int ExecuteUsingPsExec(string psexecPath, string arguments, string remoteComputerName, string user, string password)
		{
			Process proc = new Process();
			proc.StartInfo.FileName = psexecPath;
			string psexecArguments = string.Format("\\\\{0} -d -accepteula -u {1} -p {2} {3}",
														remoteComputerName,
														user,
														password,
														arguments);
			proc.StartInfo.Arguments = psexecArguments;
			StringBuilder outputBuilder = new StringBuilder();
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;
			proc.Start();
			outputBuilder.Append(proc.StandardOutput.ReadToEnd());
			outputBuilder.Append(proc.StandardError.ReadToEnd());

			// use the output
			string output = outputBuilder.ToString();

			Match m = Regex.Match(output, "process\\sID\\s(?<procid>\\d+)");
			Group procidGroup = m.Groups["procid"];
			if (!procidGroup.Success)
				throw new ApplicationException(output);

			return int.Parse(procidGroup.Value);
		}
	}
}