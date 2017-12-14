using Microarea.EasyBuilder.Properties;
using Microarea.EasyBuilder.UI;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.EasyBuilder.CppData
{

	internal class CppClass
	{
		List<CppClass> derivedClasses = new List<CppClass>();

		private string name;

		private CppClass baseClass = null;

		private List<DataManagerInfo> dbts = new List<DataManagerInfo>();
		private List<DataManagerInfo> hotlinks = new List<DataManagerInfo>();
		private List<string> variables = new List<string>();

		private List<RecordField> recordFields = new List<RecordField>();
		public string RecordClass = "";//classe del sqlrecord usata quando questa classe si riferisce ad un dbt o ad un hotlink
		public string ObjectName = "";//il nome dell'hotlink è associato all'istanza, mentre quello del dbt alla classe

		public List<string> Variables
		{
			get { return variables; }
		}

		internal List<CppClass> DerivedClasses
		{
			get { return derivedClasses; }
		}
		internal CppClass BaseClass
		{
			get { return baseClass; }
		}
		internal List<DataManagerInfo> Dbts
		{
			get { return dbts; }
		}
		internal List<DataManagerInfo> HotLinks
		{
			get { return hotlinks; }
		}
		public List<RecordField> RecordFields
		{
			get { return recordFields; }
		}

		public string Name
		{
			get { return name; }
		}
		public override string ToString()
		{
			return (baseClass == null)
				? name
				: string.Concat(baseClass.ToString(), ".", name);
		}

		public CppClass(string name)
		{
			this.name = name;
		}

		internal void SetBase(CppClass b)
		{
			b.derivedClasses.Add(this);
			if (baseClass != null)
			{
				baseClass.derivedClasses.Remove(this);
			}
			baseClass = b;
		}
		internal CppClass FindClass(string name)
		{
			foreach (CppClass c in derivedClasses)
			{
				if (c.name.Equals(name))
					return c;
				CppClass c1 = c.FindClass(name);
				if (c1 != null)
					return c1;
			}

			return null;
		}
		internal bool IsKindOf(string className)
		{
			if (name.Equals(className))
				return true;
			return baseClass == null ? false : baseClass.IsKindOf(className);
		}

		internal void AddDBT(string dbtClassName)
		{
			Debug.Assert(!string.IsNullOrEmpty(dbtClassName));
			dbts.Add(new DataManagerInfo() { DataManagerClass = dbtClassName });
		}
		internal void AddHotLink(string hklClassName, string hklName)
		{
			Debug.Assert(!string.IsNullOrEmpty(hklClassName) && !string.IsNullOrEmpty(hklName));
			hotlinks.Add(new DataManagerInfo() { DataManagerClass = hklClassName, HotLinkName = hklName });
		}
		internal void AddVariable(string varName)
		{
			Debug.Assert(!string.IsNullOrEmpty(varName));
			variables.Add(varName);
		}
		//aggiunge alla lista la colonna di database; potrebbe già essere presente se prima è stata 
		//parsata la dichiarazione del SqlRecord; il campo che li lega è il nome della variabile
		internal void AddRecordColumn(string colName, string varName, string sourceFile, int pos, long line)
		{
			foreach (var f in recordFields)
			{
				//se la trovo, aggiorno il valore
				if (f.Variable.Equals(varName))
				{
					//se c'è già, è perché è stata aggiunta parsando la dichiarazione di variabile del dataobj
					if (string.IsNullOrEmpty(f.Name) || f.Name.Equals(colName))
					{
						f.Name = colName;
						f.BindColumnSourceFile = sourceFile;
						f.BindColumnPos = pos;
						f.BindColumnLine = line;
					}
					else
					{
						//Debug.Fail("Variable already found with another name");
					}
					return;
				}

			}
			//se non l atrovo, la aggiungo
			recordFields.Add(new RecordField() { Name = colName, Variable = varName, DataType = DataType.Null, BindColumnSourceFile = sourceFile, BindColumnPos = pos, BindColumnLine = line });
		}

		//aggiunge alla lista il campo DataObj; potrebbe già essere presente se prima è stata 
		//parsata la BindRecord; il campo che li lega è il nome della variabile
		internal void AddRecorDataObj(string dataObj, string varName, string sourceFile, int pos, long line)
		{
			DataType dataType = ToDataType(dataObj);
			foreach (var f in recordFields)
			{
				//se la trovo, aggiorno il valore
				if (f.Variable.Equals(varName))
				{
					//se c'è già, è perché è stata aggiunta parsando la BindRecords
					if (f.DataType == null || f.DataType.Equals(dataType))
					{

						f.DataType = dataType;
						f.DeclarationSourceFile = sourceFile;
						f.DeclarationLine = line;
						f.DeclarationPos = pos;
					}
					else
					{
						Debug.Fail("Variable already found with another data type");
					}
					return;
				}

			}
			//se non l atrovo, la aggiungo
			recordFields.Add(new RecordField() { DataType = dataType, Variable = varName, Name = "", DeclarationSourceFile = sourceFile, DeclarationLine = line, DeclarationPos = pos });
		}

		private DataType ToDataType(string dataObj)
		{
			switch (dataObj)
			{
				case "DataStr": return DataType.String;
				case "DataInt": return DataType.Integer;
				case "DataLng": return DataType.Long;
				case "DataDbl": return DataType.Double;
				case "DataMon": return DataType.Money;
				case "DataQty": return DataType.Quantity;
				case "DataPerc": return DataType.Percent;
				case "DataDate": return DataType.Date;
				case "DataBool": return DataType.Bool;
				case "DataEnum": return DataType.Enum;
				case "DataGuid": return DataType.Guid;
				case "DataText": return DataType.Text;
				case "DataBlob": return DataType.Blob;
				default: return DataType.Null;
			}
		}

		//---------------------------------------------------------------------------------------------------
		internal bool IsDerivedFrom(string className)
		{
			if (BaseClass == null)
				return false;
			if (BaseClass.Name == className)
				return true;
			return BaseClass.IsDerivedFrom(className);
		}
	}

	//---------------------------------------------------------------------------------------------------
	internal class RecordField
	{
		public DataType DataType { get; set; }
		public string Variable { get; set; }
		public string Name { get; set; }

		public string BindColumnSourceFile { get; set; }
		public int BindColumnPos { get; set; }
		public long BindColumnLine { get; set; }
		public string DeclarationSourceFile { get; set; }
		public int DeclarationPos { get; set; }
		public long DeclarationLine { get; set; }
		public bool IsIncomplete { get { return DataType == null || string.IsNullOrEmpty(Variable) || string.IsNullOrEmpty(Name); } }
		public override string ToString()
		{
			return string.Concat("Data type: ", DataType, " - Variable: ", Variable, " - Name: ", Name, " - Bind file: ", BindColumnSourceFile, " - Declaration file: ", DeclarationSourceFile);
		}
	}
	//---------------------------------------------------------------------------------------------------
	//struttura che associa ad ogni classe di documento le informazioni sui dbt o hotlink
	internal class DataManagerInfo
	{
		public string DataManagerClass { get; set; }
		public string HotLinkName { get; set; } //il nome dell'hotlink è associato all'istanza, mentre quello del dbt alla classe
		public override string ToString()
		{
			return string.Concat(DataManagerClass);
		}
	}
	//struttura che compatta le informazioni associando ad ogni dbt o hotlink i campi dei record
	//è quella usata per serializzare in json
	//---------------------------------------------------------------------------------------------------
	/// <summary>
	/// Internal use
	/// </summary>
	internal class DataManager
	{
		public string Name { get; set; }
		public bool IsSlaveBuffered { get; set; }
		public List<DataField> Fields = new List<DataField>();
		//---------------------------------------------------------------------------------------------------
		public override string ToString()
		{
			return Name;
		}

		//---------------------------------------------------------------------------------------------------
		public bool IsEmpty
		{
			get
			{
				return Fields.Count == 0;

			}
		}

		//---------------------------------------------------------------------------------------------------
		internal static int Compare(DataManager x, DataManager y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}
	//---------------------------------------------------------------------------------------------------
	/// <summary>
	/// Internal use
	/// </summary>
	internal class DataField
	{
		[JsonIgnore]
		public DataType DataType { get; set; }
		public int DataTypeInt
		{
			get { return DataType == null ? 0 : (int)DataType; }
			set { DataType = new DataType(value); }
		}
		public string Name { get; set; }

		internal static int Compare(DataField x, DataField y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}

	//---------------------------------------------------------------------------------------------------
	/// <summary>
	/// Internal use
	/// </summary>
	internal class Document
	{
		public string Name { get; set; }
		public List<DataManager> Dbts = new List<DataManager>();
		public List<DataManager> HotLinks = new List<DataManager>();
		public List<DataField> Variables = new List<DataField>();
		public override string ToString()
		{
			return Name;
		}

		[JsonIgnore]
		public bool IsEmpty
		{
			get
			{
				if (Variables.Count > 0)
					return false;
				if (Dbts.Count == 0 && HotLinks.Count == 0) return true;
				foreach (var dbt in Dbts)
					if (!dbt.IsEmpty)//ne basta uno non vuoto
						return false;
				foreach (var hkl in HotLinks)
					if (!hkl.IsEmpty)//ne basta uno non vuoto
						return false;

				return true;
			}
		}

		public static int Compare(Document x, Document y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}
	//---------------------------------------------------------------------------------------------------
	/// <summary>
	/// Internal use
	/// </summary>
	internal class CppInfo
	{
		static CppInfo info = new CppInfo();
		List<Document> documents = new List<Document>();

		bool ready = false;
		bool stopped = false;
		EBSplash ww = null;
		CppClass root = new CppClass("root");
		private Thread thread;
		private object lockOnThread = new object();

		public CppInfo()
		{
			Application.ApplicationExit += (sender, args) =>
			{
				stopped = true;
			};
		}
		//---------------------------------------------------------------------------------------------------
		internal List<Document> Documents
		{
			get { return documents; }
		}

		//---------------------------------------------------------------------------------------------------
		public static CppInfo GetCurrent()
		{
			return info.VerifyCurrent(true);

		}
		//---------------------------------------------------------------------------------------------------
		public static void StartParsing()
		{
			info.LoadCacheFile(); //carico il file 
			info.StartParsingThread();//riparso i sorgenti in un altro thread per aggiornare il file
		}
		//---------------------------------------------------------------------------------------------------
		public CppInfo VerifyCurrent(bool giveFeedback)
		{
			if (ready)
				return this;
			LoadCacheFile();
			//in ogni caso, avvio il parsing per aggiornare il file di intellisense
			if (thread == null)
			{
				StartParsingThread();
			}
			else if (!giveFeedback) //richiesta interattiva: alzo la priorità
			{
				thread.Priority = ThreadPriority.AboveNormal;
			}
			if (giveFeedback)
			{
				ww = EBSplash.StartSplash();
				ww.SetTitle(Resources.GeneratingIntellisense);
				ww.FormClosed += (sender, args) => { ww = null; };
			}

			while (!info.ready)
			{
				Application.DoEvents();
			}
			if (ww != null)
				ww.Dispose();

			return this;
		}

		//---------------------------------------------------------------------------------------------------
		private void LoadCacheFile()
		{
			string file = GetJsonFile();
			//se trovo il file, inizio a leggere quello e torno i dati di quello
			if (File.Exists(file))
			{
				try
				{
					ParseIntelliDbts(file);
					ready = true;
				}
				catch
				{
				}
			}
		}
		//---------------------------------------------------------------------------------------------------
		private bool GenerateIntelliDbts(string intelliFileDbts)
		{
			try
			{
				string tmp = Path.GetTempFileName();

				using (StreamWriter sw = new StreamWriter(tmp, false, new UTF8Encoding(false)))
				{
					sw.Write(JsonConvert.SerializeObject(documents, Formatting.Indented));
				}
				File.Copy(tmp, intelliFileDbts, true);
				File.Delete(tmp);
				return true;
			}
			catch
			{
				return false;
			}
		}
		//---------------------------------------------------------------------------------------------------
		private void ParseIntelliDbts(string intelliFileDbts)
		{
			using (StreamReader sr = new StreamReader(intelliFileDbts))
			{
				documents = JsonConvert.DeserializeObject<List<Document>>(sr.ReadToEnd());
			}
		}
		//---------------------------------------------------------------------------------------------------
		private void StartParsingThread()
		{
			lock(lockOnThread)
			{
				if (thread != null)
					return;

				thread = new Thread(new ThreadStart(info.GenerateProcedure));
				thread.Priority = ThreadPriority.Lowest;
				thread.Start();
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void GenerateProcedure()
		{
			lock (this)
			{
				try
				{
					root = new CppClass("root");
					//salverò i file nel path di installazione
					//cerco tutti i file con estensione *.cpp
					List<string> files = new List<string>();
					foreach (BaseApplicationInfo app in BasePathFinder.BasePathFinderInstance.ApplicationInfos)
					{
						if (app.ApplicationType == TaskBuilderNet.Interfaces.ApplicationType.TaskBuilder || app.ApplicationType == TaskBuilderNet.Interfaces.ApplicationType.TaskBuilderApplication)
						{
							files.AddRange(Directory.GetFiles(app.Path, "*.h", SearchOption.AllDirectories));
							files.AddRange(Directory.GetFiles(app.Path, "*.cpp", SearchOption.AllDirectories));
						}

					}
					for (int i = 0; i < files.Count; i++)
					{
						string f = "";
						try
						{
							if (stopped)
								break;
							f = files[i];
							CPPParser parser = new CPPParser(info);
							if (ww != null && !ww.IsDisposed)
								ww.SetMessage(string.Format(Resources.IntellisenseProgress, i, files.Count, Path.GetFileName(f)));
							parser.Parse(f);
						}
						catch (Exception e)
						{
							Console.WriteLine("\n\nintelliDBt.json--> Found a problem! File: " + f + "\nException: "+ e.Message + "\n" + e.StackTrace);
						}
					}
					if (!stopped)
					{
						PopulateDocuments();
						GenerateIntelliDbts(GetJsonFile());
					}
					ready = true;
				}
				catch (Exception e)
				{
					Console.WriteLine("\n\nintelliDBt.json--> Found generic problem!\nException: " + e.Message + "\n" + e.StackTrace);
				}
			}
		}

		//---------------------------------------------------------------------------------------------------
		private string GetJsonFile()
		{
			return Path.Combine(BasePathFinder.BasePathFinderInstance.GetCustomPath(), "intellidbt.json");
		}
		//---------------------------------------------------------------------------------------------------
		internal void AddClass(string theClass, string theBaseClass)
		{
			//prima cerco la base
			CppClass b = GetClass(theBaseClass);
			//poi cerco la classe
			CppClass c = root.FindClass(theClass);
			if (c == null)
			{
				//se non la trovo la creo; potrei averla già creata se mi era arrivata in precedenza come base class 
				c = new CppClass(theClass);
			}
			//infine ne imposto la base (se ne aveva già un'altra (root) le parentele vengono riallineate
			c.SetBase(b);
		}

		//---------------------------------------------------------------------------------------------------
		private CppClass GetClass(string name)
		{
			CppClass b = root.FindClass(name);
			if (b == null)
			{
				//se non la trovo, la creo e imposto la root come suo parent
				b = new CppClass(name);
				b.SetBase(root);
			}
			return b;
		}

		//---------------------------------------------------------------------------------------------------
		internal void AddDBTToDocumentClass(string docClassName, string dbtClassName)
		{
			CppClass c = GetClass(docClassName);
			c.AddDBT(dbtClassName);
		}
		//---------------------------------------------------------------------------------------------------
		internal void AddVariableToDocumentClass(string docClassName, string varName)
		{
			CppClass c = GetClass(docClassName);
			c.AddVariable(varName);
		}
		//---------------------------------------------------------------------------------------------------
		internal void SetDBTName(string dbtClass, string dbtName)
		{
			CppClass c = GetClass(dbtClass);
			c.ObjectName = dbtName;
		}

		//---------------------------------------------------------------------------------------------------
		internal void AddRecordColumn(string className, string colName, string varName, string sourceFile, int pos, long line)
		{
			CppClass c = GetClass(className);
			c.AddRecordColumn(colName, varName, sourceFile, pos, line);
		}
		//---------------------------------------------------------------------------------------------------
		internal void AddRecordDataObj(string className, string dataObj, string varName, string sourceFile, int pos, long line)
		{
			CppClass c = GetClass(className);
			c.AddRecorDataObj(dataObj, varName, sourceFile, pos, line);
		}

		//---------------------------------------------------------------------------------------------------
		private List<DataManager> GetDataManagers(CppClass docClass, Document doc, bool dbt)
		{
			List<DataManager> l = new List<DataManager>();
			foreach (DataManagerInfo info in dbt ? docClass.Dbts : docClass.HotLinks)
			{
				CppClass dbtClass = root.FindClass(info.DataManagerClass);
				if (dbtClass == null)
					continue;
				CppClass recordClass = root.FindClass(dbtClass.RecordClass);
				if (recordClass == null)
					continue;
				DataManager dataManager = new DataManager() { Name = dbt ? dbtClass.ObjectName : info.HotLinkName, IsSlaveBuffered = dbtClass.IsDerivedFrom("DBTSlaveBuffered") };//il nome dell'hotlink è associato all'istanza, mentre quello del dbt alla classe
																																												  //aggiungo i campi della classe e quelli ereditati
				while (recordClass != null)
				{
					foreach (var recordField in recordClass.RecordFields)
						if (!recordField.IsIncomplete)
							dataManager.Fields.Add(new DataField() { DataType = recordField.DataType, Name = recordField.Name });
					recordClass = recordClass.BaseClass;
				}
				dataManager.Fields.Sort(DataField.Compare);
				l.Add(dataManager);
			}
			//ogni classe ha i duoi dbt più quelli ereditati
			if (docClass.BaseClass != null)
				l.AddRange(GetDataManagers(docClass.BaseClass, doc, dbt));
			return l;
		}

		//---------------------------------------------------------------------------------------------------
		private void PopulateDocuments()
		{
			List<Document> docs = new List<Document>();
			List<CppClass> docClasses = new List<CppClass>();
			CppClass b = root.FindClass("CAbstractFormDoc");
			if (b != null)
			{
				AddDocuments(docs, b);
				docs.Sort(Document.Compare);
			}
			documents = docs;
		}

		//---------------------------------------------------------------------------------------------------
		private void AddDocuments(List<Document> docs, CppClass c)
		{
			foreach (var item in c.DerivedClasses)
			{
				if (!string.IsNullOrEmpty(item.ObjectName))
				{
					Document d = new Document() { Name = item.ObjectName };

					List<DataManager> dbts = GetDataManagers(item, d, true);
					dbts.Sort(DataManager.Compare);
					d.Dbts.AddRange(dbts);

					List<DataManager> hotlinks = GetDataManagers(item, d, false);
					hotlinks.Sort(DataManager.Compare);
					d.HotLinks.AddRange(hotlinks);

					foreach (var field in item.Variables)
						d.Variables.Add(new DataField { Name = field });
					docs.Add(d);
				}
				AddDocuments(docs, item);
			}
		}

		//---------------------------------------------------------------------------------------------------
		internal void AddRecordClassToDataManagerClass(string hotLinkClass, string recordClass)
		{
			GetClass(hotLinkClass).RecordClass = recordClass;
		}

		//---------------------------------------------------------------------------------------------------
		internal void AddHotlinkToDocumentClass(string docClassName, string hotLinkClassName, string hklName)
		{
			CppClass hklClass = GetClass(hotLinkClassName);
			GetClass(docClassName).AddHotLink(hklClass.Name, hklName);
		}

		//---------------------------------------------------------------------------------------------------
		internal void SetDocName(string docClass, string docName)
		{
			GetClass(docClass).ObjectName = docName;
		}
	}

	class CPPParser
	{
		string[] prevLexemes = Enumerable.Repeat("", 30).ToArray();
		Parser lex;
		CppInfo cppInfo;
		enum CPPToken
		{
			DATATYPE = Token.USR01,
			IMPLEMENT_DYNCREATE = Token.USR03,
			IMPLEMENT_DYNAMIC = Token.USR04,
			IMPLEMENT_SERIAL = Token.USR05,
			ON_ATTACH_DATA = Token.USR06,
			NEW = Token.USR07,
			RUNTIME_CLASS = Token.USR08,
			_NS_DBT = Token.USR09,
			_NS_FLD = Token.USR10,
			_NS_LFLD = Token.USR11,
			BIND_RECORD = Token.USR12,
			CLASS = Token.USR13,
			TB_EXPORT = Token.USR14,
			DECLARE_VAR = Token.USR15,
			ATTACH = Token.USR16,
			HOTKEYLINK = Token.USR17,
			BEGIN_DOCUMENT = Token.USR18,
			END_DOCUMENT = Token.USR19,
			REGISTER_MASTER_TEMPLATE = Token.USR20,
			_NS_DOC = Token.USR21,
			ATTACH_FAMILY_CLIENT_DOC = Token.USR22,
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE = Token.USR23,
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE = Token.USR24,
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_BATCH_MODE = Token.USR25,
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_FRAME = Token.USR26,
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_FRAME = Token.USR27,
			ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_WIZARD_BATCH_FRAME = Token.USR28,
			ATTACH_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE = Token.USR29,
			ATTACH_CLIENT_DOC = Token.USR30,
			_NS_CD = Token.USR31
		}



		//---------------------------------------------------------------------
		public CPPParser(CppInfo cppInfo)
		{
			this.cppInfo = cppInfo;
			lex = new Parser(Parser.SourceType.FromFile);
			lex.PreprocessorDisabled = true;
#if DEBUG
			lex.DoAudit = true;
#endif

			lex.UserKeywords.Add("DataType", CPPToken.DATATYPE);
			lex.UserKeywords.Add("IMPLEMENT_DYNCREATE", CPPToken.IMPLEMENT_DYNCREATE);
			lex.UserKeywords.Add("IMPLEMENT_DYNAMIC", CPPToken.IMPLEMENT_DYNAMIC);
			lex.UserKeywords.Add("IMPLEMENT_SERIAL", CPPToken.IMPLEMENT_SERIAL);
			lex.UserKeywords.Add("OnAttachData", CPPToken.ON_ATTACH_DATA);
			lex.UserKeywords.Add("new", CPPToken.NEW);
			lex.UserKeywords.Add("RUNTIME_CLASS", CPPToken.RUNTIME_CLASS);
			lex.UserKeywords.Add("_NS_DBT", CPPToken._NS_DBT);
			lex.UserKeywords.Add("_NS_FLD", CPPToken._NS_FLD);
			lex.UserKeywords.Add("_NS_LFLD", CPPToken._NS_LFLD);
			lex.UserKeywords.Add("BindRecord", CPPToken.BIND_RECORD);
			lex.UserKeywords.Add("class", CPPToken.CLASS);
			lex.UserKeywords.Add("TB_EXPORT", CPPToken.TB_EXPORT);
			lex.UserKeywords.Add("DECLARE_VAR", CPPToken.DECLARE_VAR);
			lex.UserKeywords.Add("ATTACH", CPPToken.ATTACH);
			lex.UserKeywords.Add("HotKeyLink", CPPToken.HOTKEYLINK);
			lex.UserKeywords.Add("BEGIN_DOCUMENT", CPPToken.BEGIN_DOCUMENT);
			lex.UserKeywords.Add("END_DOCUMENT", CPPToken.END_DOCUMENT);
			lex.UserKeywords.Add("REGISTER_MASTER_TEMPLATE", CPPToken.REGISTER_MASTER_TEMPLATE);
			lex.UserKeywords.Add("REGISTER_MASTER_JSON_TEMPLATE", CPPToken.REGISTER_MASTER_TEMPLATE);
			lex.UserKeywords.Add("_NS_DOC", CPPToken._NS_DOC);
			lex.UserKeywords.Add("ATTACH_FAMILY_CLIENT_DOC", CPPToken.ATTACH_FAMILY_CLIENT_DOC);
			lex.UserKeywords.Add("ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE", CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE);
			lex.UserKeywords.Add("ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE", CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE);
			lex.UserKeywords.Add("ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_BATCH_MODE", CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_BATCH_MODE);
			lex.UserKeywords.Add("ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_FRAME", CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_FRAME);
			lex.UserKeywords.Add("ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_FRAME", CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_FRAME);
			lex.UserKeywords.Add("ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_WIZARD_BATCH_FRAME", CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_WIZARD_BATCH_FRAME);
			lex.UserKeywords.Add("ATTACH_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE", CPPToken.ATTACH_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE);
			lex.UserKeywords.Add("ATTACH_CLIENT_DOC", CPPToken.ATTACH_CLIENT_DOC);
			lex.UserKeywords.Add("_NS_CD", CPPToken._NS_CD);
		}

		//---------------------------------------------------------------------
		public bool Parse(string currentFile)
		{
			try
			{

				lex.Open(currentFile);
				while (!lex.Eof)
				{
					for (int i = prevLexemes.Length - 1; i > 0; i--)
					{
						prevLexemes[i] = prevLexemes[i - 1];
					}
					prevLexemes[0] = lex.CurrentLexeme;

					switch (lex.LookAhead())
					{
						case (Token)CPPToken.IMPLEMENT_DYNAMIC:
						case (Token)CPPToken.IMPLEMENT_DYNCREATE:
						case (Token)CPPToken.IMPLEMENT_SERIAL:
							ParseImplementMacro();
							break;
						case (Token)CPPToken.ON_ATTACH_DATA:
							ParseOnAttachData();
							break;
						case (Token)CPPToken.HOTKEYLINK:
							ParseHotLinkConstructor();
							break;
						case (Token)CPPToken._NS_DBT:
							ParseDBTConstructor();
							break;
						case (Token)CPPToken.BIND_RECORD:
							ParseBindRecord();
							break;
						case (Token)CPPToken.CLASS:
							ParseClassDeclatarion();
							break;
						case (Token)CPPToken.BEGIN_DOCUMENT:
							ParseDocumentInterface();
							break;
						case (Token)CPPToken.ATTACH_CLIENT_DOC:
						case (Token)CPPToken.ATTACH_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE:
						case (Token)CPPToken.ATTACH_FAMILY_CLIENT_DOC:
						case (Token)CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_FRAME:
						case (Token)CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_BATCH_MODE:
						case (Token)CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_FRAME:
						case (Token)CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_BATCH_MODE:
						case (Token)CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_UNATTENDED_MODE:
						case (Token)CPPToken.ATTACH_FAMILY_CLIENT_DOC_EXCLUDE_WIZARD_BATCH_FRAME:
							ParseAttachClientDoc();
							break;
						default:
							{
								lex.SkipToken();
								break;
							}
					}// switch
				}
			}
			finally
			{
				lex.Close();
			}

			return !lex.Error;
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseHotLinkConstructor()
		{
			lex.SkipToken();//HotKeyLink
							//HotKeyLink(RUNTIME_CLASS(TPaymentTerms), _NS_DOC("PaymentTerms.Documents.PaymentTerms")),

			//sono nel costruttore dell'hotlink, ho già parsato i token con le informazioni che mi servono (la classe dell'hotlink)
			//e ho davanti a me il nome del dbt
			if (!lex.Parsed(Token.ROUNDOPEN) || !lex.Parsed((Token)CPPToken.RUNTIME_CLASS) || !lex.Parsed(Token.ROUNDOPEN))
				return;
			lex.SkipToken();
			string hotLinkClass = prevLexemes[3];
			string recordClass = lex.CurrentLexeme;
			cppInfo.AddRecordClassToDataManagerClass(hotLinkClass, recordClass);
		}
		//---------------------------------------------------------------------------------------------------
		private void ParseDocumentInterface()
		{
			lex.SkipToken();//BEGIN_DOCUMENT
			if (!lex.ParseOpen())
				return;
			if (lex.Parsed((Token)CPPToken._NS_DOC) && !lex.ParseOpen())
				return;
			string docName;
			if (!lex.ParseFlatString(out docName))
				return;
			while (!lex.Parsed((Token)CPPToken.REGISTER_MASTER_TEMPLATE))
			{
				if (lex.CurrentLexeme == "END_DOCUMENT")
					return;
				lex.SkipToken();
			}
			if (!lex.ParseOpen())
				return;
			string docClass = "";
			for (int i = 0; i < 2; i++)//mi posiziono dopo la seconda virgola
			{
				while (!lex.Parsed(Token.COMMA) && !lex.Eof)
				{
					docClass = lex.CurrentLexeme;
					lex.SkipToken();
				}
			}
			if (string.IsNullOrEmpty(docClass))
				return;

			cppInfo.SetDocName(docClass, docName);
		}
		//---------------------------------------------------------------------------------------------------
		private void ParseAttachClientDoc()
		{
			lex.SkipToken();//ATTACH macro
			if (!lex.ParseOpen())
				return;
			lex.SkipToken();
			string docClass = lex.CurrentLexeme;
			if (!lex.Parsed(Token.COMMA))
				return;

			if (lex.Parsed((Token)CPPToken._NS_CD) && !lex.ParseOpen())
				return;
			string docName;
			if (!lex.ParseFlatString(out docName))
				return;

			cppInfo.SetDocName(docClass, docName);
		}
		//---------------------------------------------------------------------------------------------------
		private void ParseClassDeclatarion()
		{
			lex.SkipToken();//class
			lex.Parsed((Token)CPPToken.TB_EXPORT);

			lex.SkipToken();//class name
			string className = lex.CurrentLexeme;
			if (lex.Parsed(Token.SEP))//dichiarazione di classe forward
				return;
			//mi porto all'apertura della dichiarazione di classe
			while (!lex.Eof && !lex.Parsed(Token.BRACEOPEN))
				lex.SkipToken();
			int openCount = 1;//ne ho aperta una
			string fieldName, dataObj;
			while (!lex.Eof && openCount > 0)
			{
				if (lex.Parsed(Token.BRACEOPEN))
					openCount++;
				else if (lex.Parsed(Token.BRACECLOSE))
				{
					openCount--;
				}
				else
				{
					//ho trovato una potenziale dichiarazione di variabile di tipo DataObj
					if (lex.CurrentLexeme.StartsWith("Data"))
					{
						dataObj = lex.CurrentLexeme;
						lex.SkipToken();//DataObj
						List<string> vars = new List<string>();
						while (!lex.Eof)
						{
							if (!lex.Parsed(Token.ID))
							{
								vars.Clear();
								break;
							}
							fieldName = lex.CurrentLexeme;
							//dopo devo avere o un'assegnazione, o una virgola di separazione, o la fine di istruzione
							//diversamente, non sono nel caso di dichiarazione di variabile
							if (lex.Parsed(Token.SEP))
							{
								//la metto da parte, non so ancora se è buona
								vars.Add(fieldName);
								break;//ho trovato fine istruzione; 
							}
							else if (lex.Parsed(Token.COMMA))//più variabili dichiarate in cascata
							{
								//la metto da parte, non so ancora se è buona
								vars.Add(fieldName);
							}
							else
							{
								//trovato token non valido: falso allarme!
								vars.Clear();
								break;
							}
						}
						foreach (var v in vars)
							cppInfo.AddRecordDataObj(className, dataObj, v, lex.Filename, lex.CurrentPos, lex.CurrentLine);
					}
					else
					{
						lex.SkipToken();
					}
				}

			}

		}

		//---------------------------------------------------------------------------------------------------
		private void ParseBindRecord()
		{
			lex.SkipToken();//BindRecord
			if (!lex.Parsed(Token.ROUNDOPEN) || !lex.Parsed(Token.ROUNDCLOSE) || !lex.Parsed(Token.BRACEOPEN))
				return;
			string className = prevLexemes[2];

			int openCount = 1;//ne ho aperta una
			string colName;
			while (!lex.Eof && openCount > 0)
			{

				if (lex.Parsed(Token.BRACEOPEN))
					openCount++;
				else if (lex.Parsed(Token.BRACECLOSE))
				{
					openCount--;
				}
				else if (lex.Parsed((Token)CPPToken._NS_FLD) || lex.Parsed((Token)CPPToken._NS_LFLD))
				{
					if (lex.Parsed(Token.ROUNDOPEN) && lex.ParseFlatString(out colName) && lex.Parsed(Token.ROUNDCLOSE) && lex.Parsed(Token.COMMA) && lex.Parsed(Token.ID))
					{
						cppInfo.AddRecordColumn(className, colName, lex.CurrentLexeme, lex.Filename, lex.CurrentPos, lex.CurrentLine);
					}
				}
				else
				{
					lex.SkipToken();
				}

			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDBTConstructor()
		{
			//sono nel costruttore del dbt, ho già parsato i token con le informazioni che mi servono (la classe del dbt)
			//e ho davanti a me il nome del dbt
			lex.SkipToken();//_NS_DBT
			string dbtName;
			if (!lex.ParseOpen() || !lex.ParseFlatString(out dbtName))
				return;
			//risalco i token parsati finché non trovo la parentesi di apertura del costruttore,
			//subito prima trovo il nome della classe
			//da tenere presente che il costruttore potrebbe trovarsi nel .h, quindi non posso apettarmi il costrutto <nome>::<nome>
			int upper = prevLexemes.Length - 2;
			for (int i = 0; i < upper; i++)
			{
				if (prevLexemes[i] == null) prevLexemes[i] = "";
				if (prevLexemes[i + 1] == null) prevLexemes[i+1] = "";
				if (prevLexemes[i].Equals("CRuntimeClass") && prevLexemes[i + 1].Equals("("))
				{
					string dbtClass = prevLexemes[i + 2];
					cppInfo.SetDBTName(dbtClass, dbtName);
					return;
				}
			}
		}

		class CreatedObject
		{
			public string VarName { get; set; }
			public string ClassName { get; set; }
		}
		//---------------------------------------------------------------------------------------------------
		private void ParseOnAttachData()
		{

			lex.SkipToken();//OnAttachData
			if (!lex.Parsed(Token.ROUNDOPEN) || !lex.Parsed(Token.ROUNDCLOSE) || !lex.Parsed(Token.BRACEOPEN))
				return;
			string className = prevLexemes[2];
			List<CreatedObject> createdObjects = new List<CreatedObject>();
			int openCount = 1;//ne ho aperta una
			string[] prevLocalLexemes = new string[2];
			while (!lex.Eof && openCount > 0)
			{
				prevLocalLexemes[1] = prevLocalLexemes[0];
				prevLocalLexemes[0] = lex.CurrentLexeme;
				if (lex.Parsed(Token.BRACEOPEN))
					openCount++;
				else if (lex.Parsed(Token.BRACECLOSE))
				{
					openCount--;
				}
				else if (lex.Parsed((Token)CPPToken.NEW))
				{
					ParseObjectCreation(className, createdObjects, prevLocalLexemes);
				}
				else if (lex.Parsed((Token)CPPToken.DECLARE_VAR))
				{
					ParseDocumentVariable(className);
				}
				else if (lex.Parsed((Token)CPPToken.ATTACH))
				{
					ParseAttachHotLink(className, createdObjects);
				}
				else
				{
					lex.SkipToken();
				}

			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseAttachHotLink(string docClassName, List<CreatedObject> createdObjects)
		{
			//Attach(m_pHKLPaymentTerms, _T("HKLPaymentTerms"));
			if (!lex.Parsed(Token.ROUNDOPEN))
				return;
			//salto m_pHKLPaymentTerms
			lex.SkipToken();
			string hotlinkVar = lex.CurrentLexeme;
			if (!lex.Parsed(Token.COMMA))
				return;

			//vado al token successivo
			lex.SkipToken();

			if (lex.CurrentLexeme == "_T")
			{
				if (!lex.Parsed(Token.ROUNDOPEN))
					return;
			}

			string hklName;
			if (!lex.ParseString(out hklName))
				return;
			foreach (var item in createdObjects)
			{
				if (item.VarName == hotlinkVar)
					cppInfo.AddHotlinkToDocumentClass(docClassName, item.ClassName, hklName);
			}

		}

		//---------------------------------------------------------------------------------------------------
		private void ParseDocumentVariable(string docClassName)
		{
			if (!lex.Parsed(Token.ROUNDOPEN))
				return;

			//vado al token successivo
			lex.SkipToken();

			if (lex.CurrentLexeme == "_T")
			{
				if (!lex.Parsed(Token.ROUNDOPEN))
					return;
			}

			string varName;
			if (!lex.ParseString(out varName))
				return;
			cppInfo.AddVariableToDocumentClass(docClassName, varName);

		}

		//---------------------------------------------------------------------------------------------------
		private void ParseObjectCreation(string docClassName, List<CreatedObject> createdObjects, string[] prevLocalLexemes)
		{
			//new DBTJournalEntries(RUNTIME_CLASS(TJournalEntries), this);
			lex.SkipToken();//objectClass
			string objClass = lex.CurrentLexeme;
			if (!lex.Parsed(Token.ROUNDOPEN))
				return;
			if ("=".Equals(prevLocalLexemes[0]))
				createdObjects.Add(new CreatedObject { VarName = prevLocalLexemes[1], ClassName = objClass });
			if (lex.Parsed((Token)CPPToken.RUNTIME_CLASS) && lex.Parsed(Token.ROUNDOPEN))
			{
				lex.SkipToken();//record class
				string sqlRecord = lex.CurrentLexeme;
				if (!lex.ParseClose())
					return;
				cppInfo.AddDBTToDocumentClass(docClassName, objClass);
				cppInfo.AddRecordClassToDataManagerClass(objClass, sqlRecord);
			}
		}

		//---------------------------------------------------------------------------------------------------
		private void ParseImplementMacro()
		{
			lex.SkipToken();
			if (!lex.ParseOpen())
				return;
			lex.SkipToken();
			string theClass = lex.CurrentLexeme;
			if (!lex.ParseComma())
				return;
			lex.SkipToken();
			string theBaseClass = lex.CurrentLexeme;
			if (!lex.ParseClose())
				return;
			if (theClass.Equals(theBaseClass))
			{
				lex.SetError("Class name equals base class: " + theBaseClass);
				return;
			}
			cppInfo.AddClass(theClass, theBaseClass);
		}

	}


}
