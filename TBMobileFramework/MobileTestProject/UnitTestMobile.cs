using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TBMobile;
using MobileTestProject.Properties;
using System.Text.RegularExpressions;

namespace MobileTestProject
{
	[TestClass]
	public class UnitTestMobile
	{
		[TestMethod]
		public void TestDocument1JSON()
		{
			DTest titles = new DTest();
			titles.OnAttachData();
			string a = titles.ToJSON();
			
			string b = Resources.document1;
			if (!Trim(a).Equals(Trim(b)))
				throw new Exception("Invalid JSON description dor document DTest 1");

		}

		[TestMethod]
		public void TestDocument2JSON()
		{
			DTest titles = new DTest();
			titles.OnAttachData();
			titles.Master.PreparingBrowser += (sender, e) =>
			{
					MDataStr s = new MDataStr();
					s.Value = "g";
					e.Table.Where.AddCompareColumn(titles.Master.Record.TitleCode, s, ">");
			};
			string a = titles.ToJSON();

			string b = Resources.document2;
			if (!Trim(a).Equals(Trim(b)))
				throw new Exception("Invalid JSON description dor document DTest 2");

		}

		[TestMethod]
		public void TestDocument3JSON()
		{
			DTest titles = new DTest();
			titles.OnAttachData();
			DBTestBody body = titles.Master.GetSlave<DBTestBody>();
			body.DefiningQuery += (sender, e) =>
			{
				MDataStr s = new MDataStr();
				s.Value = "IT";
				e.Table.Where.AddCompareColumn(body.Record.Language, s);
			};
			string a = titles.ToJSON();

			string b = Resources.document3;
			if (!Trim(a).Equals(Trim(b)))
				throw new Exception("Invalid JSON description dor document DTest 3");

		}

		private object Trim(string a)
		{
			return Regex.Replace(a, "\\s|\\t|\\r|\\n", "");
		}
	}
}
