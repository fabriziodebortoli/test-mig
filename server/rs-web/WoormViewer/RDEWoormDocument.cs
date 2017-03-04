
using Microarea.Common.Applications;
using Microarea.Common.CoreTypes;
using Microarea.Common.Lexan;

namespace Microarea.RSWeb.WoormViewer
{
	//================================================================================
	public class RDEWoormDocument : WoormDocument
	{
		private string rdeFileName;

		//--------------------------------------------------------------------------------
		public RDEWoormDocument(string rdeFilename, TbReportSession session, string sessionID, string uniqueID)
		{
			Init(session, sessionID, uniqueID);

			this.rdeFileName	= rdeFilename;
			lex = new WoormParser(Parser.SourceType.FromString);
			ReleaseChecker  = new ReleaseChecker(ref lex, this);

			RdeReader.LoadInfo();

			this.filename	= RdeReader.WoormFile;
			this.Namespace  = ReportSession.PathFinder.GetNamespaceFromPath(filename);
            //TODO RSWEB localizer = new WoormLocalizer(filename, ReportSession.PathFinder);
		}

		//---------------------------------------------------------------------------
		public override string InfoFilename { get { return rdeFileName; } }

		//---------------------------------------------------------------------------
		public override string TotPageFilename { get { return PathFunctions.TotPageFilename(rdeFileName); } }
	
		//---------------------------------------------------------------------------
		public override string CurrentRdeFilename(int pageNo)
		{
			return PathFunctions.RdeFilename(rdeFileName, pageNo);
		}

		//--------------------------------------------------------------------------------
		public override bool LoadDocument()
		{
			if (!Lex.Open(RdeReader.Graphics))
				return false;
			
			return ParseWoormDocument();
		}
	}
}
