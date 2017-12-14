using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBMobile;

namespace MobileTestProject
{

	public class DTest : TDocument<DBTestHead>
	{
		public override bool OnAttachData()
		{
			Title = "Titles document";
			DBTestHead titles = new DBTestHead(this);
			AttachMaster(titles);
			titles.AttachSlave(new DBTestBody(this));
			return true;


		}
	}

	public class DBTestHead : TDBTMaster<TTitles>
	{
		public DBTestHead(MDocument document)
			: base(new TTitles(), "titles", document)
		{

		}


	}

	public class DBTestBody : TDBTSlaveBuffered<TTitleLanguages, DBTestHead>
	{
		public DBTestBody(MDocument document)
			: base(new TTitleLanguages(), "titlelanguages", document)
		{
			DefiningQuery += DBTTitleLanguages_DefiningQuery;

		}

		void DBTTitleLanguages_DefiningQuery(object sender, DataManagerEventArgs e)
		{
			e.Table.Where.AddForeignKeyColumn(Record.TitleCode, Master.Record.TitleCode);
			//MDataStr s = new MDataStr();
			//s.Value = "IT";
			//e.Table.Where.AddCompareColumn(Record.GetField(Record.Language), s);
		}


	}
}
