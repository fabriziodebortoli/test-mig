using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Woorm.WoormController;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSjson;

namespace Microarea.TaskBuilderNet.Test
{
	[Serializable]
	[KnownType(typeof(Rectangle))]
	class Dynamic : WoormSerializable
	{
		[WoormSerializable]
		public int MyIntProperty;
		[WoormSerializable]
		public string MyStringProperty;
		[WoormSerializable]
		public Rectangle MyRectProperty;
		[WoormSerializable(Name = "obj", Conditional = true)]
		public Dynamic MyDynamicProperty;

		public Dynamic(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public Dynamic()
		{
			// TODO: Complete member initialization
		}
		public override bool ShouldSerialize(string name)
		{
			if (name == "MyDynamicProperty")
				return MyDynamicProperty != null;

			return base.ShouldSerialize(name);
		}
	}
	[Serializable]
	[KnownType(typeof(Rectangle))]
	class Static : ISerializable
	{
		public int MyIntProperty;
		public string MyStringProperty;
		public Rectangle MyRectProperty;
		public Static MyDynamicProperty;

		public Static(SerializationInfo info, StreamingContext context)
		{
			MyIntProperty = info.GetInt32("MyIntProperty");
			MyStringProperty = info.GetString("MyStringProperty");
			MyRectProperty = info.GetValue<Rectangle>("MyRectProperty");
			try
			{
				MyDynamicProperty = info.GetValue<Static>("MyDynamicProperty");
			}
			catch
			{

			}
		}

		public Static()
		{
			// TODO: Complete member initialization
		}
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("MyIntProperty", MyIntProperty);
			info.AddValue("MyStringProperty", MyStringProperty);
			info.AddValue("MyRectProperty", MyRectProperty);
			if (MyDynamicProperty != null)
				info.AddValue("obj", MyDynamicProperty);
		}
	}

	[TestClass]
	public class WoormSerializationUT
	{

		[TestMethod]
		public void TestPerformance()
		{
			DateTime start = DateTime.Now;
			int count = 1;
			for (int i = 0; i < count; i++)
			{
				Static sta = new Static();
				sta.MyIntProperty = 3;
				sta.MyRectProperty = new Rectangle(0, 0, 200, 200);
				sta.MyStringProperty = "prova";
				sta.MyDynamicProperty = new Static();
				sta.MyDynamicProperty.MyIntProperty = 4;
				sta.MyDynamicProperty.MyRectProperty = new Rectangle(0, 0, 200, 400);
				sta.MyStringProperty = "inner";
				String s2 = Serialize(sta);
			}
			
			TimeSpan elapsed1 = DateTime.Now - start;
			start = DateTime.Now;
			for (int i = 0; i < count; i++)
			{
				Dynamic dyn = new Dynamic();
				dyn.MyIntProperty = 3;
				dyn.MyRectProperty = new Rectangle(0, 0, 200, 200);
				dyn.MyStringProperty = "prova";
				dyn.MyDynamicProperty = new Dynamic();
				dyn.MyDynamicProperty.MyIntProperty = 4;
				dyn.MyDynamicProperty.MyRectProperty = new Rectangle(0, 0, 200, 400);
				dyn.MyStringProperty = "inner";
				String s1 = Serialize(dyn);
			}
			TimeSpan elapsed2 = DateTime.Now - start;
			

		}


		[TestMethod]
		public void TestSerializeSqrRect()
		{
			Serialize(new SqrRect());
		}
		[TestMethod]
		public void TestSerializeTextRect()
		{
			TextRect tr = new TextRect();
			tr.Label.Text = "Prova";
			tr.Label.FontData = new FontData();
			tr.Label.FontData.Italic = true;
			tr.Label.FontData.Bold = true;
			tr.Label.FontData.Strikeout = true;
			tr.Label.FontData.Underline = true;
			tr.Label.FontData.Family = "Verdana";
			tr.Label.FontData.Size = 11;
			tr.LocalizedText = "Localized";
			Serialize(tr);
		}
		[TestMethod]
		public void TestSerializeFileRect()
		{
			Serialize(new FileRect());
		}
		[TestMethod]
		public void TestSerializeGraphRect()
		{
			Serialize(new GraphRect());
		}
		[TestMethod]
		public void TestSerializeFieldRect()
		{
			Serialize(new FieldRect());
		}
		[TestMethod]
		public void TestSerializeTable()
		{
			Table t = new Table(null, 1, 1);
			Serialize(t);
		}
		[TestMethod]
		public void TestSerializeColumn()
		{
			Serialize(new Column());
		}
		[TestMethod]
		public void TestSerializeLayout()
		{
			Serialize(new Layout());
		}
		[TestMethod]
		public void TestSerializeReportData()
		{
			ReportData data = new ReportData();
			data.paperWidth = 300;
			data.paperLength = 500;
			data.message = "a";
			data.ready = true;
			data.error = true;
			Layout layout = new Layout();
			data.reportObjects = layout;
			layout.Add(new FieldRect());
			layout.Add(new Table());
			Serialize(data);
		}

		[TestMethod]
		public void TestSerializeDBObjects()
		{
			DBObjects dbo = new DBObjects();
			dbo.catalog = new CatalogInfo();
			Serialize(dbo);
		}
		
		[TestMethod]
		public void TestSerializeDBTableColumns()
		{
			DBTableColumns dbtc = new DBTableColumns();
			dbtc.columns = new System.Collections.ArrayList();
			Serialize(dbtc);
		}

		private string Serialize(object o)
		{
			DataContractJsonSerializer json = new DataContractJsonSerializer(o.GetType());
			using (MemoryStream ms = new MemoryStream())
			{
				json.WriteObject(ms, o);
				string s = Encoding.UTF8.GetString(ms.ToArray());
				ms.Seek(0, SeekOrigin.Begin);
				o = json.ReadObject(ms);
				return s;
			}
		}
	}
}
