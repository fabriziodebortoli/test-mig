using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.Console.Core.FileConverter
{
	//================================================================================
	public abstract class GenericTranslator
	{
		
		protected const string newNameAttribute	= "newName";
		protected const string oldNameAttribute	= "oldName";
		protected const string newAppNameAttribute	= "newAppName";
		protected const string oldAppNameAttribute	= "oldAppName";
		
		//--------------------------------------------------------------------------------
		public abstract void AddFromFile(string file);
	}

	//================================================================================
	/// <summary>
	/// This class, although implementing ITableTranslator, does not translate tables and columns:
	/// it can be used to generate the lookup file (to complete with new table/column names)
	/// used by the TableTranslator class
	/// </summary>
	public class LookUpTableGenerator : ITableTranslator
	{
		private const string tables		= "Tables";
		private const string table		= "Table";
		private const string column		= "Column";

		string outputFile;
		XmlDocument lookUpDocument = new XmlDocument();

		//--------------------------------------------------------------------------------
		public LookUpTableGenerator(string outputFile, bool append)
		{
			this.outputFile = outputFile;	
			if (append && File.Exists(outputFile))
				lookUpDocument.Load(outputFile);
		}

		#region ITableTranslator Members	

		//--------------------------------------------------------------------------------
		public bool Splitted(string tableOldName, out string allSplittedTables)
		{
			allSplittedTables = null;
			return false;
		}

		//--------------------------------------------------------------------------------
		public bool ExistColumn(string tableOldName, string columnOldName)
		{
			return false;
		}

		//--------------------------------------------------------------------------------
		public string ReverseTranslateTable(string tableName)		
		{
			XmlElement root = lookUpDocument.SelectSingleNode(tables) as XmlElement;
			if (root  == null)
				return string.Empty;

			tableName = tableName.ToUpper();
			string xPath = string.Format("{0}[@newName='{1}']", table, tableName);
			XmlElement el = root.SelectSingleNode(xPath) as XmlElement;
			if (el != null)
			{
				return el.GetAttribute("oldName");
			}
			return string.Empty;;	
		}

		//--------------------------------------------------------------------------------
		public string TranslateTable(string tableOldName)
		{
			XmlElement el = GetTableNode(tableOldName);
			return tableOldName;
		}

		//--------------------------------------------------------------------------------
		public string OwnerColumn(string tableOldName, string columnOldName)
		{
			return null;
		}

		//--------------------------------------------------------------------------------
		public string TranslateColumn(string tableOldName, string columnOldName)
		{
			XmlElement el = GetColumnNode(tableOldName, columnOldName);
			return columnOldName;
		}

		#endregion

		//--------------------------------------------------------------------------------
		private XmlElement GetTablesNode()
		{
			XmlElement el = lookUpDocument.SelectSingleNode(tables) as XmlElement;
			if (el == null)
			{
				el = lookUpDocument.CreateElement(tables);
				lookUpDocument.AppendChild(el);
			}
			return el;
		}

		//--------------------------------------------------------------------------------
		private XmlElement GetTableNode(string tableName)
		{
			tableName = tableName.ToUpper();

			XmlElement root = GetTablesNode();
			string xPath = string.Format("{0}[@oldName='{1}']", table, tableName);
			XmlElement el = root.SelectSingleNode(xPath) as XmlElement;
			if (el == null)
			{
				el = lookUpDocument.CreateElement(table);
				el.SetAttribute("oldName", tableName);
				el.SetAttribute("newName", "");
				root.AppendChild(el);
				Save();
			}
			return el;	
		}

		//--------------------------------------------------------------------------------
		private XmlElement GetColumnNode(string tableName, string columnName)
		{
			columnName = columnName.ToUpper();
			
			XmlElement root = GetTableNode(tableName);
			string xPath = string.Format("{0}[@oldName='{1}']", column, columnName);
			XmlElement el = root.SelectSingleNode(xPath) as XmlElement;
			if (el == null)
			{
				el = lookUpDocument.CreateElement(column);
				el.SetAttribute("oldName", columnName);
				el.SetAttribute("newName", "");
				root.AppendChild(el);
				Save();
			}
			return el;	
		}

		//--------------------------------------------------------------------------------
		public void Save()
		{
			lookUpDocument.Save(outputFile);
		}
	}

	/// <summary>
	/// Summary description for Translator.
	/// </summary>
	//================================================================================
	public class TableTranslator : GenericTranslator, ITableTranslator
	{
		/*	sintassi del file:
		 * <Tables>
		 *	<Table oldName="" newName="">
		 *		<Column oldName="" newName=""/>
		 *	<Table>
		 * </Tables>
		 * */
		
		private const string tablesNode			= "Tables";
		private const string tableNode			= "Table";
		private const string columnNode			= "Column";

        Hashtable tables = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable oldTableNames = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
		//================================================================================
		private class Table
		{
			public string newName = string.Empty;
            public Hashtable columns = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
		}

		//---------------------------------------------------------------------------------------------------
		public TableTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public ArrayList TableNames()
		{
			ArrayList list = new ArrayList();
			foreach (string t in tables.Keys)
				list.Add(t);

			return list;
		}

		//--------------------------------------------------------------------------------
		public ArrayList ColumnNames(string tableName)
		{
			ArrayList list = new ArrayList();
			Table t = tables[tableName] as Table;
			if (t != null)
				foreach (string c in t.columns.Keys)
					list.Add(c);
			
			return list;
		}
	
		//---------------------------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);

			XmlNodeList nodes = lookUpDocument.SelectNodes("//Tables/Table");
			foreach (XmlElement el in nodes)
			{
				Table t = new Table();
				string oldName = el.GetAttribute(oldNameAttribute);
				string newName = el.GetAttribute(newNameAttribute);
				t.newName = newName;
				XmlNodeList colNodes = el.SelectNodes("Column");
				foreach (XmlElement colEl in colNodes)
					t.columns[colEl.GetAttribute(oldNameAttribute)] = colEl.GetAttribute(newNameAttribute);
				tables[oldName] = t;
				oldTableNames[newName] = oldName;
			}
		}

		//---------------------------------------------------------------------------------------------------
		public string TranslateTable(string tableName)
		{
			if (tableName == null) return tableName;

			Table t = tables[tableName] as Table;
			if (t == null) return tableName;
	
			return t.newName;
			
		}

		/// <summary>
		/// effettua la traduzione al contrario
		/// </summary>
		/// <param name="tableName"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------------------------------------
		public string ReverseTranslateTable(string tableName)
		{
			if (tableName == null) return tableName;

			foreach(DictionaryEntry entry in tables)
			{
				if (string.Compare(((Table)entry.Value).newName, tableName, true) == 0)
					return entry.Key as string;
			}

			return tableName;
		}

		//---------------------------------------------------------------------------------------------------
		public string TranslateColumn(string tableName, string columnName)
		{
			return TranslateColumn(tableName, columnName, true);
		}

		//---------------------------------------------------------------------------------------------------
		private string TranslateColumn(string tableName, string columnName, bool bIterate)
		{
			if (columnName == null ) return columnName;

			if (tableName == null || tableName.Length == 0)
			{
				string firstColumnNewName = null;
				StringBuilder sb = new StringBuilder();
				bool conflict = false;
				foreach (DictionaryEntry d in tables)
				{
					Table table = d.Value as Table;
					string tName = d.Key as string;
					string columnNewName = table.columns[columnName] as string;
					if (columnNewName != null) 
					{
						sb.AppendFormat(" '{0}'", tName);
						if (firstColumnNewName == null || string.Compare(firstColumnNewName, columnNewName, true) == 0)
							firstColumnNewName = columnNewName; 
						else
							conflict = true;
					}
				}

				if (conflict)
					throw new ApplicationException
						(
						string.Format
						(
						FileConverterStrings.TranslateColumnConflict,
						columnName,
						sb.ToString()
						)
						);
				return firstColumnNewName == null ? columnName : firstColumnNewName;
			}

			Table t = tables[tableName] as Table;
			if (t == null) 
				return bIterate 
					? TranslateColumn(ReverseTranslateTable(tableName), columnName, false) 
					: columnName;
				
			string newName = t.columns[columnName] as string;
			if (newName == null) 
				return bIterate 
					? TranslateColumn(ReverseTranslateTable(tableName), columnName, false) 
					: columnName;
			
			return newName;
		}

		//---------------------------------------------------------------------------------------------------
		public bool ExistColumn(string tableName, string columnName)
		{
			if (tableName == null || columnName==null ) return false;

			Table t = tables[tableName] as Table;
			if (t == null) 
				return false;
				
			string newName = t.columns[columnName] as string;
			if (newName == null) 
				return false;
			
			return newName != string.Empty;
		}
		//---------------------------------------------------------------------------------------------------
		public string OwnerColumn(string tableName, string columnName)
		{
			if (tableName == null || columnName == null ) return string.Empty;

			Table t = tables[tableName] as Table;
			if (t == null) 
				return string.Empty;
				
			string newName = t.columns[columnName] as string;
			if (newName == null || newName == string.Empty) 
				return string.Empty;
			
			return t.newName;
		}

		//--------------------------------------------------------------------------------
		public bool Splitted(string tableOldName, out string allSplittedTables)
		{
			allSplittedTables = string.Empty;
			return false;
		}
	}
	//================================================================================
	public class FontFormatterTranslator : GenericTranslator, IFormatterTranslator, IFontTranslator
	{
		/*	sintassi del file:
		* <...>
		*	<Formatters>
		*		<Formatter oldName="" newName="">
		*	</Formatters>
		*	<Fonts>
		*		<Font oldName="" newName="">
		*	</Fonts>
		* </...>
		* */

        Hashtable fonts = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable formatters = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

		//---------------------------------------------------------------------------------------------------
		public FontFormatterTranslator(string file)
		{
			AddFromFile(file);			
		}

		//---------------------------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//Formatters/Formatter");
			foreach (XmlElement el in nodes)
				formatters[el.GetAttribute(oldNameAttribute)] = el.GetAttribute(newNameAttribute);

			nodes = lookUpDocument.SelectNodes("//Fonts/Font");
			foreach (XmlElement el in nodes)
				fonts[el.GetAttribute(oldNameAttribute)] = el.GetAttribute(newNameAttribute);
		}

		//--------------------------------------------------------------------------------
		public string TranslateFormatter(string formatterName)
		{
			if (formatterName == null) return formatterName;

			string newName = formatters[formatterName] as string;
			return newName == null ? formatterName : newName;
		}

		//--------------------------------------------------------------------------------
		public bool IsPublicFormatter(string formatterName)
		{
			if (formatterName == null) return false;

			return formatters[formatterName] != null;
		}

		//--------------------------------------------------------------------------------
		public string TranslateFont(string fontName)
		{
			if (fontName == null) return fontName;

			string newName = fonts[fontName] as string;
			return newName == null ? fontName : newName;
		}

		//--------------------------------------------------------------------------------
		public bool IsPublicFont(string fontName)
		{
			if (fontName == null) return false;

			return fonts[fontName] != null;
		}
	}

	//================================================================================
	public class HotLinkTranslator : GenericTranslator, IHotLinkTranslator
	{
		/*	sintassi del file:
		*	<HotLinks>
		*		<HotLink oldName="" newName="">
		*	</HotLinks>
		* */

		protected const string descriptionAttribute = "description";
		protected const string withContextAttribute = "context";

        Hashtable hotLinks = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable descriptions = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable withContext = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

		//---------------------------------------------------------------------------------------------------
		public HotLinkTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//HotLinks/HotLink");
			foreach (XmlElement el in nodes)
			{
				string name = el.GetAttribute(oldNameAttribute);
				hotLinks[name] = el.GetAttribute(newNameAttribute);
				descriptions[name] = el.GetAttribute(descriptionAttribute);
				withContext[name] = el.GetAttribute(withContextAttribute);
			}

		}

		//--------------------------------------------------------------------------------
		public string TranslateHotLink(string hotLinkName)
		{
			if (hotLinkName == null) return hotLinkName;

			string newName = hotLinks[hotLinkName] as string;
			if (newName == null) return hotLinkName;
	
			return newName;
		}

		//--------------------------------------------------------------------------------
		public string GetDescription(string hotLinkName)
		{
			if (hotLinkName == null) return string.Empty;

			string desc = descriptions[hotLinkName] as string;
			if (desc == null) return string.Empty;
	
			return desc;
		}

		//--------------------------------------------------------------------------------
		public bool WithContext(string hotLinkName)
		{
			if (hotLinkName == null) return false;

			string desc = withContext[hotLinkName] as string;
			if (desc == null) return false;
	
			return string.Compare(desc, "True", true) == 0;
		}

	}

	//================================================================================
	public class FunctionTranslator : GenericTranslator, IFunctionTranslator 
	{
		/*	sintassi del file:
		*	<Functions>
		*		<Function oldName="" unsupported="true|false" newName="|..." document="true|false" context="true|false" description="|..." >
		*		<Function oldName="" woorminfo="true" newName="" description="...">
		*			<ReturnValue var="" type="" />
		*			<Parameter var="" type="" />
		*			<Parameter var="" type="" />
		*		</Function>
		*		<Function oldName="" woorminfo="true|false" newName="" description="...">
		*			<Module name="" newName="" />
		*			<Module name="" newName="" />
		*		</Function>
		*	</Functions>
		* */

		protected const string unsupportedAttribute	= "unsupported";
		protected const string documentAttribute = "document";
		protected const string descriptionAttribute = "description";
		protected const string contextAttribute = "context";
		
		protected const string ParameterTag	= "Parameter";
		protected const string ReturnValueTag	= "ReturnValue";
		protected const string ModuleTag	= "Module";
		protected const string woormInfoAttribute	= "woorminfo";
		protected const string typeAttribute	= "type";
		protected const string varAttribute	= "var";
		protected const string moduleNameAttribute	= "name";

        Hashtable functions = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
		
		//================================================================================
		private class FunctionParameter
		{
			//--------------------------------------------------------------------------------
			public FunctionParameter(string name, string type)
			{
				this.type = type;
				this.name = name;
			}
			public string type;
			public string name;
		}

		//================================================================================
		private class ModuleNamespace
		{
			//--------------------------------------------------------------------------------
			public ModuleNamespace(string module, string ns)
			{
				this.module = module;
				this.ns = ns;
			}
			public string module;
			public string ns;
		 
		}

		//================================================================================
		private class Function
		{
			public bool unsupported = false;
			public bool document = false;
			public bool context = false;
			public bool woormInfo = false;
			public bool needOtherParameters = false;

			public string newName;
			public string description;

			public ArrayList parameters = new ArrayList();
			public ArrayList module2Namespace = new ArrayList();
			public FunctionParameter returnValue = null;

			//--------------------------------------------------------------------------------
			public void AddReturnValue(string name, string type) 
			{
				returnValue = new FunctionParameter(name, type);
			}
			//--------------------------------------------------------------------------------
			public void AddParameter(string name, string type) 
			{
				parameters.Add(new FunctionParameter(name, type));
			}
			//--------------------------------------------------------------------------------
			public void AddNewName(string module, string ns) 
			{
				module2Namespace.Add(new ModuleNamespace(module, ns));
			}
		}

		//---------------------------------------------------------------------------------------------------
		public FunctionTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;
			try
			{

				XmlDocument lookUpDocument = null;
				lookUpDocument = new XmlDocument();
				lookUpDocument.Load(file);
				
				XmlNodeList nodes = lookUpDocument.SelectNodes("//Functions/Function");
				foreach (XmlElement el in nodes)
				{
					Function f = new Function();
					f.newName = el.GetAttribute(newNameAttribute);
					f.description = el.GetAttribute(descriptionAttribute);
					f.document = ToBool(el.GetAttribute(documentAttribute));
					f.context = ToBool(el.GetAttribute(contextAttribute));
					f.woormInfo = ToBool(el.GetAttribute(woormInfoAttribute));
					f.unsupported = ToBool(el.GetAttribute(unsupportedAttribute));
					f.needOtherParameters = ToBool(el.GetAttribute("needOtherParameters"));

					XmlNode returnValNode = el.SelectSingleNode("./" + ReturnValueTag);
					if (returnValNode != null)
					{
						XmlElement returnVal = returnValNode as XmlElement;

						if (returnVal != null)
							f.AddReturnValue(returnVal.GetAttribute(varAttribute), returnVal.GetAttribute(typeAttribute).Replace("'","\""));
					}

					XmlNodeList childs = el.SelectNodes("./" + ParameterTag);
					foreach (XmlElement elParameter in childs)
					{
						f.AddParameter(elParameter.GetAttribute(varAttribute), elParameter.GetAttribute(typeAttribute).Replace("'","\""));
					}

					childs = el.SelectNodes("./" + ModuleTag);
					foreach (XmlElement elParameter in childs)
					{
						f.AddNewName(elParameter.GetAttribute(moduleNameAttribute), elParameter.GetAttribute(newNameAttribute));
					}

					functions[el.GetAttribute(oldNameAttribute)] = f;
				}
			}
			catch(XmlException e)
			{
				System.Diagnostics.Debug.Fail(string.Format(FileConverterStrings.AddFromFileParsingError, file, e.Message));
				return;
			}
		}
		
		//--------------------------------------------------------------------------------
		private bool ToBool (string s)
		{
			if (s == string.Empty) return false;
			try
			{
				return bool.Parse(s);
			}
			catch(FormatException)
			{
				return false;
			}
		}
		//--------------------------------------------------------------------------------
		public string GetReturnValueName(string functionName)
		{
			if (functionName == null) return functionName;
			Function f = functions[functionName] as Function;
			if (f == null) return string.Empty;
	
			if (f.woormInfo && f.returnValue != null)
			{
				string name = translateVar == null ? f.returnValue.name : translateVar(f.returnValue.name);
				return name;
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------
		public string TranslateFunction(string functionName, string targetModule)
		{
			if (functionName == null) return functionName;
			Function f = functions[functionName] as Function;
			if (f == null) return functionName;
	
//			if (f.woormInfo && f.returnValue != null)
//			{
//				string name = translateVar == null ? f.returnValue.name : translateVar(f.returnValue.name);
//				return name;
//			}

			if (f.woormInfo && f.parameters.Count > 0)
				return functionName;

			for (int i=0; i < f.module2Namespace.Count; i++)
			{
				ModuleNamespace m2ns = f.module2Namespace[i] as ModuleNamespace;
				if (string.Compare(m2ns.module, targetModule, true) == 0)
					return m2ns.ns;
			}
			return f.newName;
		}

		//--------------------------------------------------------------------------------
		public delegate string DTranslateString(string s);
		public DTranslateString translateVar = null;

		public string GetDescription(string functionName)
		{
			if (functionName == null) return string.Empty;
			Function f = functions[functionName] as Function;
			if (f == null) return string.Empty;
	
			string des = f.description;

			for (int i=0; i < f.module2Namespace.Count; i++)
			{
				ModuleNamespace m2ns = f.module2Namespace[i] as ModuleNamespace;
				des += string.Format(FileConverterStrings.FunctionAlreadyExists, m2ns.module, m2ns.ns);
			}
			for (int i=0; i < f.parameters.Count; i++)
			{
				FunctionParameter fp = f.parameters[i] as FunctionParameter;
				string name = translateVar == null ? fp.name : translateVar(fp.name);

				des += string.Format(FileConverterStrings.FunctionReplacedByAutomaticField, fp.type, name);
			}
			if (f.returnValue != null)
			{
				string name = translateVar == null ? f.returnValue.name : translateVar(f.returnValue.name);
				des += string.Format(FileConverterStrings.ReplacedFunctionReturnValue, f.returnValue.type, name);
			}
			return des;
		}

		//--------------------------------------------------------------------------------
		public bool NeedOtherParameters(string functionName)
		{
			if (functionName == null) return true;

			Function f = functions[functionName] as Function;
			if (f == null) return false;
	
			return f.needOtherParameters;
		}

		//--------------------------------------------------------------------------------
		public bool IsUnsupportedFunction(string functionName)
		{
			if (functionName == null) return true;

			Function f = functions[functionName] as Function;
			if (f == null) return false;
	
			return f.unsupported || f.woormInfo;
		}

		//--------------------------------------------------------------------------------
		public bool IsDocumentFunction(string functionName)
		{
			if (functionName == null) return false;

			Function f = functions[functionName] as Function;
			if (f == null) return false;
	
			return f.document;
		}

		//--------------------------------------------------------------------------------
		public bool IsFunctionWithContext(string functionName)
		{
			if (functionName == null) return false;

			Function f = functions[functionName] as Function;
			if (f == null) return false;
	
			return f.context;
		}

		//--------------------------------------------------------------------------------
		public bool IsWoorminfoFunction(string functionName)
		{
			if (functionName == null) return false;

			Function f = functions[functionName] as Function;
			if (f == null) return false;
	
			return f.woormInfo;
		}

		//--------------------------------------------------------------------------------
		public bool IsWoorminfoReturnValueFunction(string functionName)
		{
			if (functionName == null) return false;

			Function f = functions[functionName] as Function;
			if (f == null) return false;
	
			return f.woormInfo && f.returnValue != null && f.parameters.Count == 0;
		}

		//--------------------------------------------------------------------------------
		public bool IsOverloadedDocumentFunction(string functionName)
		{
			if (functionName == null) return false;

			Function f = functions[functionName] as Function;
			if (f == null) return false;
	
			return f.document && f.module2Namespace.Count > 0;
		}

		//---------------------------------------------------------------------------------------------------
		public bool RetrieveSubstituteVariables(string functionName, out string [] variables, out string [] variableTypes)
		{
			variables = null; variableTypes = null;
			if (functionName == null) return false;
			Function f = functions[functionName] as Function;
			if (f == null) return false;
			if (!f.woormInfo) return false;
			if (f.parameters.Count == 0) return false;

			variables = new string [f.parameters.Count];
			variableTypes = new string [f.parameters.Count];
			for (int i=0; i < f.parameters.Count; i++)
			{
				FunctionParameter fp = f.parameters[i] as FunctionParameter;
				variables[i] = fp.name;
				variableTypes[i] = fp.type;
			}
			return true;
		}

		//---------------------------------------------------------------------------------------------------
		public bool RetrieveSubstituteReturnValue(string functionName, out string variable, out string type)
		{
			variable = null; type = null;
			if (functionName == null) return false;
			Function f = functions[functionName] as Function;
			if (f == null) return false;
			if (!f.woormInfo) return false;
			if (f.returnValue == null) return false;

			variable = f.returnValue.name;
			type = f.returnValue.type;
			return true;
		}
	}

	//================================================================================
	public class LinkTranslator : GenericTranslator, ILinkTranslator
	{
		/*	sintassi del file:
		*	<...>
		*		<LinkForms>
		*			<LinkForm oldName="" newName="" oldGuid="" oldTable="" >
		*		</LinkForms>
		*		<LinkReports>
		*			<LinkReport oldName="" newName="" oldGuid="" >
		*		</LinkReports>
		*	</...>
		* */

        Hashtable links = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable linkOldTables = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable linkOldGuids = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
		
		protected const string oldTableAttribute = "oldTable";
		protected const string oldGuidAttribute = "oldGuid";
		
		//---------------------------------------------------------------------------------------------------
		public LinkTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//LinkForms/LinkForm");
			foreach (XmlElement el in nodes)
			{
				string oldName = el.GetAttribute(oldNameAttribute);
				links[oldName] = el.GetAttribute(newNameAttribute);
				linkOldTables[oldName] = el.GetAttribute(oldTableAttribute);
				linkOldGuids[oldName] = el.GetAttribute(oldGuidAttribute);
			}
			nodes = lookUpDocument.SelectNodes("//LinkReports/LinkReport");
			foreach (XmlElement el in nodes)
			{
				string oldName = el.GetAttribute(oldNameAttribute);
				links[oldName] = el.GetAttribute(newNameAttribute);
				linkOldTables[oldName] = el.GetAttribute(oldTableAttribute);
				linkOldGuids[oldName] = el.GetAttribute(oldGuidAttribute);
			}
		}

		//--------------------------------------------------------------------------------
		public string TranslateLink(string linkName)
		{
			if (linkName == null) return linkName;

			string newName = links[linkName] as string;
			if (newName == null) return linkName;
	
			return newName;
		}

		//--------------------------------------------------------------------------------
		public string GetFormMasterTableName(string linkFormName)
		{
			if (linkFormName == null) return null;

			return linkOldTables[linkFormName] as string;
		}

		//--------------------------------------------------------------------------------
		public string GetGuid(string linkName)
		{
			if (linkName == null) return null;
			return linkOldGuids[linkName] as string;
		}
	}

	//================================================================================
	
	public class EnumTranslator : GenericTranslator, IEnumTranslator 
	{
		//--------------------------------------------------------------------------------
		//================================================================================
		private class EnumItem 
		{
			public string OldName = string.Empty;
			public string NewName = string.Empty;
			public bool Removed = false;
			public string val = string.Empty;

			//--------------------------------------------------------------------------------
			public EnumItem (string oldName) { OldName = oldName; Removed = true; }
			//--------------------------------------------------------------------------------
			public EnumItem (string oldName, string newName) { OldName = oldName; NewName = newName;}
		}

		//--------------------------------------------------------------------------------
		//================================================================================
		private class EnumTag 
		{
			public string OldName = string.Empty;
			public string NewName = string.Empty;
			public bool Removed = false;
			public string val = string.Empty;

			public ArrayList items = new ArrayList();
	
			//--------------------------------------------------------------------------------
			public EnumTag (string oldName, bool removed) { OldName = oldName; Removed = removed; NewName = oldName;}
			//--------------------------------------------------------------------------------
			public EnumTag (string oldName, string newName) { OldName = oldName; NewName = newName;}

			//--------------------------------------------------------------------------------
			public EnumItem FindEnumValue(string enumValue)
			{
				for(int i=0; i < items.Count; i++)
				{
					EnumItem ev = items[i] as EnumItem;
					if (string.Compare(enumValue, ev.OldName,true) == 0)
						return ev;
				}
				return null;
			}

		}

		//--------------------------------------------------------------------------------
		private ArrayList enums = new ArrayList();
        Hashtable enumsMap = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

		//--------------------------------------------------------------------------------
		private EnumTag FindEnumType(string enumName)
		{
			for(int i=0; i < enums.Count; i++)
			{
				EnumTag et = enums[i] as EnumTag;
				if (string.Compare(enumName, et.OldName,true) == 0)
					return et;
			}
			return null;
		}

		//--------------------------------------------------------------------------------
		public EnumTranslator()
		{
			EnumTag et; EnumItem ev; 

			et = new EnumTag("Tipo componente distinta", false);
			ev = new EnumItem("Materiale"); et.items.Add(ev);
			ev = new EnumItem("Costo produzione"); et.items.Add(ev);
			ev = new EnumItem("Lavorazione"); et.items.Add(ev);
			enums.Add(et);

			et = new EnumTag("Stato OdL", true);
			enums.Add(et);

			et = new EnumTag("Stato avanzamento OdL", true);
			enums.Add(et);

			et = new EnumTag("Tipo movimento prima nota", false);
			ev = new EnumItem("Pagamento IVA in sospensione"); et.items.Add(ev);
			enums.Add(et);

			et = new EnumTag("Tipo documento", false);
			ev = new EnumItem("Bolla", "DDT"); et.items.Add(ev);

			ev = new EnumItem("Autofattura"); et.items.Add(ev);
			ev = new EnumItem("Libero 1"); et.items.Add(ev);
			ev = new EnumItem("Libero 2"); et.items.Add(ev);
			enums.Add(et);

			et = new EnumTag("Tipo documento da parametrizzare", false);
			ev = new EnumItem("Bolla/DDT Vendita", "DDT vendita"); et.items.Add(ev);
			ev = new EnumItem("Ordine di Lavorazione", "Ordine di Produzione"); et.items.Add(ev);

			ev = new EnumItem("Reso a Fornitore"); et.items.Add(ev);
			ev = new EnumItem("Movimento di Magazzino"); et.items.Add(ev);
			enums.Add(et);

			et = new EnumTag("Tipo documento di riferimento", false);
			ev = new EnumItem("Bolla"); et.items.Add(ev);
			enums.Add(et);

			et = new EnumTag("Tipo provvigione MAGO III", true);
			enums.Add(et);

			et = new EnumTag("Tipo documento per email", true);
			enums.Add(et);

			et = new EnumTag("Calcolo provvigioni", true);
			enums.Add(et);

			et = new EnumTag("Tipo barcode", false);
			ev = new EnumItem("<predefinito>", "predefinito"); et.items.Add(ev);
			enums.Add(et);

			et = new EnumTag("Costo valorizzazione magazzino", "Tipo valorizzazione giacenze");
//			ev = new EnumItem("LIFO", "LIFO (Annuale)" ); et.items.Add(ev);
//			ev = new EnumItem("FIFO", "FIFO (Annuale)" ); et.items.Add(ev);
			enums.Add(et);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//Enums/Tag");
			foreach (XmlElement el in nodes)
			{
				string oldName = el.GetAttribute(oldNameAttribute);
				string newName = el.GetAttribute(newNameAttribute);
				string val = el.GetAttribute("value");

				EnumTag et = enumsMap[oldName] as EnumTag;
				if (et == null)
				{
					et = new EnumTag(oldName, newName);
					et.val = val;
					enumsMap[oldName] = et;
				}

				foreach (XmlElement eli in el.SelectNodes("Item"))
				{
					oldName = eli.GetAttribute(oldNameAttribute);
					newName = eli.GetAttribute(newNameAttribute);
					val = eli.GetAttribute("value");
					
					EnumItem ei = new EnumItem(oldName, newName);
					ei.val = val;
					et.items.Add(ei);
				}
			}
		}

		//--------------------------------------------------------------------------------
		public string TranslateEnumType(string enumName)
		{
			EnumTag et = FindEnumType(enumName);
			if (et == null) return enumName;
			if (et.Removed) return string.Empty;
			return et.NewName;
		}

		//--------------------------------------------------------------------------------
		public string TranslateEnumValue(string enumName, string enumValue)
		{
			EnumTag et = FindEnumType(enumName);
			if (et == null) return enumValue;
			if (et.Removed) return string.Empty;

			EnumItem ev = et.FindEnumValue(enumValue);
			if (ev == null) return enumValue;
			if (ev.Removed) return string.Empty;

			return ev.NewName;
		}

		//--------------------------------------------------------------------------------
		public string GetTagValue(string tagName)
		{
			EnumTag et = enumsMap[tagName] as EnumTag;
			if (et == null) return string.Empty;
			if (et.Removed) return string.Empty;

			return et.val;
		}

		//--------------------------------------------------------------------------------
		public string GetItemValue(string tagName, string itemName)
		{
			EnumTag et = enumsMap[tagName] as EnumTag;
			if (et == null) return string.Empty;

			EnumItem ev = et.FindEnumValue(itemName);
			if (ev == null) return string.Empty;
			if (ev.Removed) return string.Empty;

			return ev.val;
		}
	}

	//================================================================================
	public class FileObjectTranslator : IFileObjectTranslator 
	{
		//--------------------------------------------------------------------------------
		public string TranslateFile(string fileName, bool isText, string reportNameSource, ArrayList reportExternalPaths, DestinationReportsData reportsData)
		{
			string destFilePath = string.Empty;
			string destNamespace = string.Empty;
			string sourceFileName = string.Empty;
			if (!File.Exists(fileName) && reportExternalPaths.Count > 0)
			{
				sourceFileName = ResolveExternalFilePath (fileName,  isText, reportExternalPaths);
				if (sourceFileName == string.Empty)
				{
					sourceFileName = ResolveExternalFilePath(Path.GetFileName(fileName), isText, reportExternalPaths);
				}
			}
			else if (File.Exists(fileName))
				sourceFileName = fileName;

			if (sourceFileName != string.Empty) 
			{
				destNamespace = string.Format
					(
					"{0}.{1}.{2}.{3}.{4}",
					isText ? "Text" : "Image",
					reportsData.Application, reportsData.Module,
					isText ? "Texts" : "Images",
					Path.GetFileName(sourceFileName)
					);

				//TODO usare il pathfinder
				if (reportsData.Standard)
				{
					int pos = reportsData.Path.LastIndexOfOccurrence("\\", 2, reportsData.Path.Length - 1);
					destFilePath = string.Format
						(
						"{0}\\Files\\{1}\\{2}",
						reportsData.Path.Substring(0, pos),
						isText ? "Texts" : "Images",
						Path.GetFileName(sourceFileName)
						);
				}
				else if (reportsData.User == "AllUsers")
				{
					int pos = reportsData.Path.LastIndexOfOccurrence("\\", 3, reportsData.Path.Length - 1);
					destFilePath = string.Format
						(
						"{0}\\Files\\{1}\\AllUsers\\{2}",
						reportsData.Path.Substring(0, pos),
						isText ? "Texts" : "Images",
						Path.GetFileName(sourceFileName)
						);
				}
				else
				{
					int pos = reportsData.Path.LastIndexOfOccurrence("\\", 4, reportsData.Path.Length - 1);
					destFilePath = string.Format
						(
						"{0}\\Files\\{1}\\Users\\{2}\\{3}",
						reportsData.Path.Substring(0, pos),
						isText ? "Texts" : "Images",
						reportsData.User,
						Path.GetFileName(sourceFileName)
						);
				}

				try
				{
					if (!Directory.Exists(Path.GetDirectoryName(destFilePath)))
						Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));

					File.Copy(sourceFileName, destFilePath, true);
				}
				catch (Exception)
				{
					//TODO ...
				}
			}

			return destNamespace;
		}

		//--------------------------------------------------------------------------------
		private string ResolveExternalFilePath(string fileName, bool isText, ArrayList reportExternalPaths)
		{
			string sourceFileName = string.Empty;
			int i = 0;
			for (i = 0; i < reportExternalPaths.Count; i++)
			{
				string path = reportExternalPaths[i] as string;
				sourceFileName = Path.Combine(path, fileName);
				if (File.Exists(sourceFileName))
				{			
					return sourceFileName;
				}
			}
			return string.Empty;
		}
	}
	//================================================================================
	public class DocumentsTranslator : GenericTranslator, IDocumentsTranslator
	{
		/*	sintassi del file:
		* <...>
		*	<Documents>
		*		<Document oldName="" newName="">
		*	</Documents>
		* </...>
		* */

        Hashtable documents = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

		//---------------------------------------------------------------------------------------------------
		public DocumentsTranslator(string file)
		{
			AddFromFile(file);			
		}

		//---------------------------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//Documents/Document");
			foreach (XmlElement el in nodes)
				documents[el.GetAttribute(oldNameAttribute)] = el.GetAttribute(newNameAttribute);
		}

		//--------------------------------------------------------------------------------
		public string TranslateDocument(string name)
		{
			if (name == null) return name;
			if (name.ToLower().IndexOf("document.") == 0)
				name = name.Substring(9);

			string newName = documents[name] as string;
			return newName == null ? name : newName;
		}

		//--------------------------------------------------------------------------------
		public bool ExistDocument(string name)
		{
			if (name == null) return false;
			if (name.ToLower().IndexOf("document.") == 0)
				name = name.Substring(9);

			return documents[name] != null;
		}

	}
	//================================================================================
	public class ReportsTranslator : GenericTranslator, IReportsTranslator
	{
		/*	sintassi del file:
		*		<Documents>
		*			<Document oldName="" newName=""  >
		*		</Documents>
		* */

        Hashtable reports = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
		
		//---------------------------------------------------------------------------------------------------
		public ReportsTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//Reports/Report");
			foreach (XmlElement el in nodes)
			{
				string oldName = el.GetAttribute(oldNameAttribute);
				reports[oldName] = el.GetAttribute(newNameAttribute);
			}
		}

		//--------------------------------------------------------------------------------
		public string TranslateReport(string name)
		{
			if (name == null) return name;
			if (name.ToLower().IndexOf("report.") == 0)
				name = name.Substring(7);

			string newName = reports[name] as string;
			if (newName == null) return name;
	
			return newName;
		}
		
		//--------------------------------------------------------------------------------
		public bool ExistReport(string name)
		{
			if (name == null) return false;
			if (name.ToLower().IndexOf("report.") == 0)
				name = name.Substring(7);

			return  reports[name] != null;
		}

	}
	//================================================================================
	public class LibrariesTranslator : GenericTranslator, ILibrariesTranslator
	{
		/*	sintassi del file:
		*		<Libraries>
		*			<Librarie oldName="" newName=""  >
		*		</Libraries>
		* */

        Hashtable libs = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable modules = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable applications = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

		//---------------------------------------------------------------------------------------------------
		public LibrariesTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//Libraries/Library");
			foreach (XmlElement el in nodes)
			{
				string oldName = el.GetAttribute(oldNameAttribute);
				string newName = el.GetAttribute(newNameAttribute);;
				libs[oldName] = newName;
				int pos = oldName.LastIndexOf('.');
				string m = oldName.Substring(0, pos);
				if (modules[m] == null)
				{
					pos = newName.LastIndexOf('.');
					modules[m] = newName.Substring(0, pos);
					string a = m.Substring(0, m.IndexOf('.'));
					if (applications[a] == null)
						applications[a] = newName.Substring(0, newName.IndexOf('.'));
				}
			}
		}

		//--------------------------------------------------------------------------------
		public string TranslateLibrary(string name)
		{
			if (name == null) return name;

			string newName = libs[name] as string;
			if (newName == null) return name;
	
			return newName;
		}

		//--------------------------------------------------------------------------------
		public string TranslateModule(string name)
		{
			if (name == null) return name;

			string newName = modules[name] as string;
			if (newName == null) return name;
	
			return newName;
		}

		//--------------------------------------------------------------------------------
		public string TranslateApplication(string name)
		{
			//TODO
			return name;
		}
		

	}
	//================================================================================
	public class ActivationsTranslator : GenericTranslator, IActivationsTranslator
	{
		/*	sintassi del file:
		*		<Activations>
		*			<Activation oldName="" newName=""  >
		*		</Activations>
		* */

        Hashtable appsName = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        Hashtable appsModules = new Hashtable(StringComparer.CurrentCultureIgnoreCase);

		//---------------------------------------------------------------------------------------------------
		public ActivationsTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);

			XmlNode root = lookUpDocument.SelectSingleNode("//Activations");
			string oldName = root.Attributes[oldAppNameAttribute].Value as string;
			string newName = root.Attributes[newAppNameAttribute].Value as string;

            Hashtable modules = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
			appsName[oldName] = newName;
			appsModules[oldName] = modules;

			XmlNodeList nodes = lookUpDocument.SelectNodes("//Activations/Activation");
			foreach (XmlElement el in nodes)
			{
				oldName = el.GetAttribute(oldNameAttribute);
				newName = el.GetAttribute(newNameAttribute);;
				modules[oldName] = newName;
			}
		}

		//--------------------------------------------------------------------------------
		public string TranslateActivation(string app)
		{
			if (app == null) return app;

			string newName = appsName[app] as string;
			if (newName == null) return app;
	
			return newName;
		}

		//--------------------------------------------------------------------------------
		public string TranslateActivation(string app, string mod)
		{
			if (app == null) return app;

			Hashtable modules = appsModules[app] as Hashtable;
			if (modules == null) return mod;

			string newName = modules[mod] as string;
			if (newName == null) return mod;
	
			return newName;
		}
	}

	//================================================================================
	public class WoormInfoVariablesTranslator : GenericTranslator, IWoormInfoVariablesTranslator
	{
		/*	sintassi del file:
		*		<WrmVars>
		*			<WrmVar oldName="" newName=""  />
		*		</WrmVars>
		* */

        Hashtable vars = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
		
		//---------------------------------------------------------------------------------------------------
		public WoormInfoVariablesTranslator(string file)
		{
			AddFromFile(file);
		}

		//--------------------------------------------------------------------------------
		public override void AddFromFile(string file)
		{
			if (!File.Exists(file)) return;

			XmlDocument lookUpDocument = null;
			lookUpDocument = new XmlDocument();
			lookUpDocument.Load(file);
			
			XmlNodeList nodes = lookUpDocument.SelectNodes("//WrmVars/WrmVar");
			foreach (XmlElement el in nodes)
			{
				string oldName = el.GetAttribute(oldNameAttribute);
				vars[oldName] = el.GetAttribute(newNameAttribute);
			}
		}

		//--------------------------------------------------------------------------------
		public string TranslateWoormInfoVariable(string name)
		{
			if (name == null) return name;

			string newName = vars[name] as string;
			if (newName == null) return name;
	
			return newName;
		}
		
		//--------------------------------------------------------------------------------
		public bool ExistWoormInfoVariable(string name)
		{
			if (name == null) return false;
			return  vars[name] != null;
		}
	}

}