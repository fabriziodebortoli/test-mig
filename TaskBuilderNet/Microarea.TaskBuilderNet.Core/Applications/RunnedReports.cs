using System;
using System.Collections;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.Applications
{
	//================================================================================
	public class RunnedReportMng
	{
		//--------------------------------------------------------------------------------
		public RunnedReportMng()
		{
		}

		//--------------------------------------------------------------------------------
		public RunnedReport[] GetRunnedReports(INameSpace reportNamespace, string company)
		{
			return GetRunnedReports(reportNamespace, company, NameSolverStrings.AllUsers);
		}

		//--------------------------------------------------------------------------------
		public RunnedReport[] GetRunnedReports(INameSpace reportNamespace, string company, string user)
		{
			if (reportNamespace == null)
				return GetRunnedReports(company, user);

			string reportFileName = Path.GetFileNameWithoutExtension(reportNamespace.Report);
			string path = BasePathFinder.BasePathFinderInstance.GetCustomReportPathFromNamespace(reportNamespace, company, user);
			path = Path.Combine(path, reportFileName);
			
			return ReadReports(path, company, user, reportNamespace, reportFileName);
		}

		//--------------------------------------------------------------------------------
		public RunnedReport[] GetRunnedReports(string company, string user)
		{
			ArrayList list = new ArrayList();

			foreach (BaseApplicationInfo ai in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
			{
				if (ai.ApplicationType != ApplicationType.TaskBuilderApplication)
					continue;

				foreach (BaseModuleInfo mi in ai.Modules)
				{
					string reportPath = mi.GetCustomReportPath(company, user);

					if (!Directory.Exists(reportPath))
						continue;

					foreach (string subPath in Directory.GetDirectories(reportPath))
					{
						string reportName = Path.GetFileName(subPath);
						NameSpace ns = new NameSpace
								(
									ai.Name + NameSpace.TokenSeparator + mi.Name + NameSpace.TokenSeparator + reportName, 
									NameSpaceObjectType.Report
								);
						list.AddRange(ReadReports(subPath, company, user, ns, reportName));
					}
				}
			}
		
			return list.ToArray(typeof(RunnedReport)) as RunnedReport[];
		}

		//--------------------------------------------------------------------------------
		private RunnedReport[] ReadReports(string path, string company, string user, INameSpace reportNamespace, string reportFileName)
		{	
			if (!Directory.Exists(path))
				return new RunnedReport[0];

			ArrayList list = new ArrayList();
			foreach (string report in Directory.GetDirectories(path))
			{
				string reportPath = Path.Combine(report, reportFileName) + NameSolverStrings.XmlExtension;

				if (!File.Exists(reportPath))
					continue;

				RunnedReport r = new RunnedReport(reportNamespace, company, user);

				string file = Path.GetFileNameWithoutExtension(report);
				r.TimeStamp = DateTime.ParseExact(file, PathFunctions.ReportFolderNameFormatter, null);
				r.FilePath = reportPath;
				
				XmlDocument d = new XmlDocument();
				d.Load(r.FilePath);
				XmlNode descriptionNode = d.SelectSingleNode("//Description");
				r.Description = descriptionNode == null ? string.Empty : descriptionNode.InnerText;

				list.Add(r);
			}

			return list.ToArray(typeof(RunnedReport)) as RunnedReport[];
		}
		
		//--------------------------------------------------------------------------------
		public void DeleteOldRunnedReports(INameSpace reportNamespace, string company, string user, DateTime expirationDate)
		{
			RunnedReport[] reports = GetRunnedReports(reportNamespace, company, user);
			
			foreach (RunnedReport report in reports)
			{
				if (report.TimeStamp < expirationDate)
					report.Delete();
			}
		}
		
		//--------------------------------------------------------------------------------
		public void DeleteOldRunnedReports(INameSpace reportNamespace, string company, string user)
		{
			DeleteOldRunnedReports(reportNamespace, company, user, DateTime.MaxValue);
		}

		//--------------------------------------------------------------------------------
		public void DeleteOldRunnedReports(string company, string user, DateTime expirationDate)
		{
			DeleteOldRunnedReports(null, company, user, expirationDate);
		}

		//--------------------------------------------------------------------------------
		public void DeleteOldRunnedReports(string company, string user)
		{
			DeleteOldRunnedReports(null, company, user, DateTime.MaxValue);
		}

		//--------------------------------------------------------------------------------
		public void DeleteOldRunnedReports(INameSpace reportNamespace, string company, DateTime expirationDate)
		{
			DeleteOldRunnedReports(reportNamespace, company, NameSolverStrings.AllUsers, expirationDate);
		}
		
		//--------------------------------------------------------------------------------
		public void DeleteOldRunnedReports(string company, DateTime expirationDate)
		{
			DeleteOldRunnedReports(null, company, NameSolverStrings.AllUsers, expirationDate);
		}
		
		//--------------------------------------------------------------------------------
		public void DeleteOldRunnedReports(string company)
		{
			DeleteOldRunnedReports(null, company, NameSolverStrings.AllUsers, DateTime.MaxValue);
		}
	}

	//================================================================================
	public class RunnedReport
	{
		private INameSpace ownerReportNamespace = null;
		private string		filePath = String.Empty;
		private string		description = String.Empty;
		private DateTime	timeStamp = DateTime.Now;
		private string		company = String.Empty;
		private string		user = String.Empty;

		//--------------------------------------------------------------------------------
		public INameSpace OwnerReportNamespace { get { return ownerReportNamespace; } }
		public string		FilePath			{ get { return filePath; }		set { if (File.Exists(value)) filePath = value; else filePath = String.Empty; } }
		public string		Description			{ get { return description; }	set { description = value; } }
		public DateTime		TimeStamp			{ get { return timeStamp; }		set { timeStamp = value; } }
		public string		Company				{ get { return company; }		set { company = value; } }
		public string		User				{ get { return user; }			set { user = value; } }

		//--------------------------------------------------------------------------------
		public RunnedReport(INameSpace aNameSpace, string aCompany, string aUser)
		{
			ownerReportNamespace = aNameSpace;
			company = aCompany;
			user = aUser;
		}
		
		//--------------------------------------------------------------------------------
		public void Delete()
		{
			if (filePath == null || filePath == String.Empty)
				return;

			Directory.Delete(Path.GetDirectoryName(filePath), true);
		}
	}
	
}
