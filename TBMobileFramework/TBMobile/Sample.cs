using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TBMobile
{
	public class DTitles : TDocument<DBTTitles>
	{
		public override bool OnAttachData()
		{
			Title = "Titles document";
			DBTTitles titles = new DBTTitles(this);
			AttachMaster(titles);
			titles.AttachSlave(new DBTTitleLanguages(this));
			return true;


		}
	}

	public class DBTTitles : TDBTMaster<TTitles>
	{
		public DBTTitles(MDocument document)
			: base(new TTitles(), "titles", document)
		{

			DefiningQuery += DBTTitles_DefiningQuery;
			PreparingBrowser += DBTTitles_PreparingBrowser;
		}

		void DBTTitles_PreparingBrowser(object sender, DataManagerEventArgs e)
		{
			//MDataStr s = new MDataStr();
			//s.Value = "m";
			//e.Table.Where.AddCompareColumn(Record.TitleCode, s, ">");
		}

		void DBTTitles_DefiningQuery(object sender, DataManagerEventArgs e)
		{
			//MDataStr s = new MDataStr();
			//s.Value = "g";
			//e.Table.Where.AddCompareColumn(Record.GetField(Record.TitleCode), s, ">");
		}


	}

	public class DBTTitleLanguages : TDBTSlaveBuffered<TTitleLanguages, DBTTitles>
	{
		public DBTTitleLanguages(MDocument document)
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

	[Table(Name = "MA_TITLES")]
	public class TTitles : MSqlRecord
	{
		[Column(Name = "TitleCode", SegmentKey = true)]
		public MDataStr TitleCode { get; set; }
		[Column(Name = "Description", DescriptionField = true)]
		public MDataStr Description { get; set; }
	}

	[Table(Name = "MA_TITLESLANG")]
	public class TTitleLanguages : MSqlRecord
	{
		[Column(Name = "TitleCode", SegmentKey = true)]
		public MDataStr TitleCode { get; set; }
		[Column(Name = "Language", SegmentKey = true)]
		public MDataStr Language { get; set; }
		[Column(Name = "Description")]
		public MDataStr Description { get; set; }
	}
}
