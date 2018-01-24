using System.IO;
using System.Xml;
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.NameSolver;

namespace Microarea.RSWeb.WoormViewer
{
	/// <summary>
	/// Gestisce la persistenza di una singola esecuzione di report
	/// </summary>
	//================================================================================
	public class RDEPersister
	{
		private WoormDocument	woorm;
		private TbReportSession reportSession;
		private RunnedReportMng runnedReports;

		//--------------------------------------------------------------------------
		//se sto visualizzando un report da rde (woorm is RDEWoormDocument), non posso storicizzarlo nuovamente
		public bool CanSaveForUser			
		{
			get
			{
				return 
					!(woorm is RDEWoormDocument) &&
					!MaxNumberReached(reportSession.UserInfo.User); 
			}
		}

		//--------------------------------------------------------------------------
		//se sto visualizzando un report da rde (woorm is RDEWoormDocument), non posso storicizzarlo nuovamente
		public bool CanSaveForAllUsers		
		{
			get
			{
				return
					reportSession.UserInfo.Admin &&
					!(woorm is RDEWoormDocument)&&
					!MaxNumberReached(NameSolverStrings.AllUsers); 
			}
		}
		
		//--------------------------------------------------------------------------------
		public RDEPersister(WoormDocument woorm, TbReportSession session)
		{
			this.woorm = woorm;
			this.reportSession = session;
			this.runnedReports = new RunnedReportMng();
		}

		//--------------------------------------------------------------------------------
		private int MaxReportNumber(string company, string user)
		{		
			return 20;
		}

		//--------------------------------------------------------------------------
		public bool MaxNumberReached(string user)
		{
			return (reportSession != null && reportSession.UserInfo != null) ? 
				(runnedReports.GetRunnedReports(woorm.Namespace, reportSession.UserInfo.Company, user).Length 
				>=
				MaxReportNumber(reportSession.UserInfo.Company, user)) : false;

		}

		//--------------------------------------------------------------------------
		public bool SaveForUser(string user, string description)
		{ 
			if (woorm == null || woorm.GraphicSection == string.Empty) return false;

			if (MaxNumberReached(user)) return false;

			AddGraphicInfos(woorm.InfoFilename, woorm.GraphicSection, woorm.Filename, description);

			string customPath = reportSession.PathFinder.GetCustomReportPathFromWoormFile(woorm.Filename, reportSession.UserInfo.Company, user);
							
			string originPath = PathFunctions.WoormTempFilePath(woorm.SessionID, woorm.UniqueID);
			string destinationPath = PathFunctions.WoormRunnedReportPath
				(
				customPath, 
				Path.GetFileNameWithoutExtension(woorm.Filename),  
				true
				);
			
            //TODO LARA
			//foreach (string file in PathFinder.PathFinderInstance.FileSystemManager.GetFiles(originPath, "*.*"))
			//{
			//	string destFileName = Path.Combine(destinationPath, Path.GetFileName(file));
   //             PathFinder.PathFinderInstance.FileSystemManager.CopyFile(file, destFileName, false );
			//}

			return true;
		}

		//---------------------------------------------------------------------------
		public void AddGraphicInfos(string infoFileName, string graphics, string source, string description)
		{
            XmlDocument d = new XmlDocument();
            Stream s = PathFinder.PathFinderInstance.FileSystemManager.GetStream(infoFileName , true);//TODO lara
            d.Load(s);

            XmlElement descriptionNode = d.CreateElement("Description");
			descriptionNode.InnerText = description;
			d.DocumentElement.AppendChild(descriptionNode);
			
			XmlElement woormNode = d.CreateElement("Graphics");
			woormNode.SetAttribute("Source", source);
			d.DocumentElement.AppendChild(woormNode);

			XmlCDataSection cdataNode =  d.CreateNode(XmlNodeType.CDATA, string.Empty, string.Empty) as XmlCDataSection;
			cdataNode.Data = graphics;
			woormNode.AppendChild(cdataNode);

			d.Save(s);
		}
	}
}
