using System;
using System.Collections;
using System.IO;
using System.Xml;

using Microarea.Common.NameSolver;


using Microarea.Common.CoreTypes;
using Microarea.Common.Generic;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.Common.Applications
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
			string path = PathFinder.PathFinderInstance.GetCustomReportPathFromNamespace(reportNamespace, company, user);
			path = Path.Combine(path, reportFileName);
			
			return ReadReports(path, company, user, reportNamespace, reportFileName);
		}

		//--------------------------------------------------------------------------------
		public RunnedReport[] GetRunnedReports(string company, string user)
		{
			ArrayList list = new ArrayList();
            //TODO RSWEB 
			foreach (ApplicationInfo ai in PathFinder.PathFinderInstance.ApplicationInfos)
			{
				if (ai.ApplicationType != ApplicationType.TaskBuilderApplication)
					continue;

				foreach (ModuleInfo mi in ai.Modules)
				{
					string reportPath = mi.GetCustomReportPath(company, user);

					if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(reportPath))
						continue;

					foreach (TBDirectoryInfo subPath in PathFinder.PathFinderInstance.FileSystemManager.GetSubFolders(reportPath))
					{
                        //TODO RSWEB subPath ritorna il path di una dir mentre invece dovresti prenere i wrm nella dir
                        string reportName = Path.GetFileName(subPath.CompleteDirectoryPath);
						NameSpace ns = new NameSpace
								(
									ai.Name + NameSpace.TokenSeparator + mi.Name + NameSpace.TokenSeparator + reportName, 
									NameSpaceObjectType.Report
								);
						list.AddRange(ReadReports(subPath.CompleteDirectoryPath, company, user, ns, reportName));
					}
				}
			}
		
			return list.ToArray(typeof(RunnedReport)) as RunnedReport[];
		}

		//--------------------------------------------------------------------------------
		private RunnedReport[] ReadReports(string path, string company, string user, INameSpace reportNamespace, string reportFileName)
		{	
			if (!PathFinder.PathFinderInstance.FileSystemManager.ExistPath(path))
				return new RunnedReport[0];

			ArrayList list = new ArrayList();
			foreach (TBDirectoryInfo subDir in PathFinder.PathFinderInstance.FileSystemManager.GetSubFolders(path))
			{
				string reportPath = Path.Combine(subDir.CompleteDirectoryPath, reportFileName) + NameSolverStrings.XmlExtension;
                if (!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(reportPath))
                    continue;

				RunnedReport r = new RunnedReport(reportNamespace, company, user);

				string file = Path.GetFileNameWithoutExtension(reportFileName);
				r.TimeStamp = DateTime.ParseExact(file, PathFunctions.ReportFolderNameFormatter, null);
				r.FilePath = reportPath;

                XmlDocument d = null;
                d = PathFinder.PathFinderInstance.FileSystemManager.LoadXmlDocument(d, r.FilePath);

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
		public string		FilePath			{ get { return filePath; }		set { if (PathFinder.PathFinderInstance.FileSystemManager.ExistFile(value)) filePath = value; else filePath = String.Empty; } }
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

            PathFinder.PathFinderInstance.FileSystemManager.RemoveFolder(Path.GetDirectoryName(filePath), true, true, true);
		}
	}
	
}
