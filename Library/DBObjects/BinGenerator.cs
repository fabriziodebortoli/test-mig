using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microarea.Library.TBWizardProjects;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.Generic; 


namespace Microarea.Library.DBObjects
{
	///<summary>
	/// Classe che si occupa della creazione dell'xml partendo dalla lettura dei file dbxml
	///</summary>
	//=================================================================================
	public class BinGenerator
	{
		private uint version = 1;
		private IBasePathFinder pathFinder;
		private XmlDocument currentDbXml;

		private XmlDocument dbObjectsDoc = new XmlDocument(); // struttura in memoria che conterrà il bin
		private XmlElement tblsElem;
		private XmlElement viewsElem;
		private XmlElement procsElem;
		private XmlElement extraAddColElem;

		//---------------------------------------------------------------------
		public BinGenerator(IBasePathFinder pathFinder)
		{
			this.pathFinder = pathFinder;
			
			// creo i nodi base del file
			XmlElement rootElem = dbObjectsDoc.CreateElement(DBObjectXML.Element.RootElement);
			if (rootElem != null)
				dbObjectsDoc.AppendChild(rootElem);

			tblsElem = dbObjectsDoc.CreateElement(DBObjectXML.Element.Tables);
			rootElem.AppendChild(tblsElem);

			viewsElem = dbObjectsDoc.CreateElement(DBObjectXML.Element.Views);
			rootElem.AppendChild(viewsElem);

			procsElem = dbObjectsDoc.CreateElement(DBObjectXML.Element.Procedures);
			rootElem.AppendChild(procsElem);

			extraAddColElem = dbObjectsDoc.CreateElement(DBObjectXML.Element.ExtraAddedColumns);
			rootElem.AppendChild(extraAddColElem);
			//
		}

		//---------------------------------------------------------------------
		public bool CreateBIN(out string error)
		{
			error = string.Empty;
			if (!LoadFiles())
			{
				error = "Error loading dbxml files";
				return false;
			}

			// linea di codice che serve per creare un file xml con le stesse informazioni del BIN
			//dbObjectsDoc.Save(Path.Combine(pathFinder.GetCustomPath(), "TestDatabaseObjectsBIN.xml"));

			try
			{
				// il file si chiama DatabaseObjects.BIN e viene salvato nella Custom
				using (BinaryFileParser parser = new BinaryFileParser(pathFinder.GetDatabaseObjectsBinPath()))
				{
					parser.UnparseUInt(version);

					uint nLength = (uint)dbObjectsDoc.InnerXml.Length;
					parser.UnparseUInt(nLength);
					parser.UnparseString(dbObjectsDoc.InnerXml);

					parser.Dispose();
				}
			}
			catch (Exception exc) 
			{
				error = exc.Message;
				return false;
			}
			return true;
		}

		//---------------------------------------------------------------------
		private bool LoadFiles()
		{
			if (pathFinder == null)
				return false;

			foreach (IBaseApplicationInfo ai in pathFinder.ApplicationInfos)
			{
				foreach (IBaseModuleInfo mi in ai.Modules)
				{
					// se il modulo non ha signature o un numero di release valido lo skippo
					if (string.IsNullOrEmpty(mi.ModuleConfigInfo.Signature) ||
						mi.ModuleConfigInfo.Release <= 0)
						continue;

					// x ogni app+modulo carico i file dbxml e li leggo per estrapolare le info che devo mettere nel bin
					string dbxmlPath = mi.GetDatabaseScriptPath();
					if (string.IsNullOrEmpty(dbxmlPath) || !Directory.Exists(dbxmlPath))
						continue;

					foreach (string fileName in Directory.GetFiles(dbxmlPath, "*.dbxml"))
						Parse(fileName);
				}
			}

			return true;
		}

		//---------------------------------------------------------------------
		private void Parse(string filePath)
		{
			if (!File.Exists(filePath))
				return;

			currentDbXml = new XmlDocument();

			try
			{
				currentDbXml.Load(filePath);

				// nodi di tipo <Tables>
				XmlElement tables = currentDbXml.DocumentElement.SelectSingleNode(DBObjectXML.Element.Tables) as XmlElement;
				if (tables != null)
				{
					XmlNodeList tableList = tables.SelectNodes(DBObjectXML.Element.Table);
					if (tableList != null && tableList.Count > 0)
						TranslateDbInfo(tableList, DBObjectXML.Element.Table);
				}

				// nodi di tipo <Views>
				XmlElement views = currentDbXml.DocumentElement.SelectSingleNode(DBObjectXML.Element.Views) as XmlElement;
				if (views != null)
				{
					XmlNodeList viewList = views.SelectNodes(DBObjectXML.Element.View);
					if (viewList != null && viewList.Count > 0)
						TranslateDbInfo(viewList, DBObjectXML.Element.View);
				}

				// nodi di tipo <Procedures>
				XmlElement procedures = currentDbXml.DocumentElement.SelectSingleNode(DBObjectXML.Element.Procedures) as XmlElement;
				if (procedures != null)
				{
					XmlNodeList procedureList = procedures.SelectNodes(DBObjectXML.Element.Procedure);
					if (procedureList != null && procedureList.Count > 0)
						TranslateDbInfo(procedureList, DBObjectXML.Element.Procedure);
				}

				// nodi di tipo <ExtraAddedColumns>
				XmlElement addedColumns = currentDbXml.DocumentElement.SelectSingleNode(DBObjectXML.Element.ExtraAddedColumns) as XmlElement;
				if (addedColumns != null)
				{
					XmlNodeList addedColList = addedColumns.SelectNodes(DBObjectXML.Element.ExtraAddedColumn);
					if (addedColList != null && addedColList.Count > 0)
						TranslateDbInfo(addedColList, DBObjectXML.Element.ExtraAddedColumn);
				}
			}
			catch (XmlException)
			{ }
		}

		//---------------------------------------------------------------------
		private void TranslateDbInfo(XmlNodeList dbObjects, string typeNode)
		{
			foreach (XmlElement objectNode in dbObjects)
			{
				XmlElement targetNode = dbObjectsDoc.CreateElement(typeNode);

				switch (typeNode)
				{
					case DBObjectXML.Element.Table:
						tblsElem.AppendChild(targetNode);
						break;

					case DBObjectXML.Element.View:
						viewsElem.AppendChild(targetNode);
						break;

					case DBObjectXML.Element.Procedure:
						procsElem.AppendChild(targetNode);
						break;

					case DBObjectXML.Element.ExtraAddedColumn:
						extraAddColElem.AppendChild(targetNode);

						string libNamespace = objectNode.GetAttribute(DBObjectXML.Attribute.LibraryNamespace);
						targetNode.SetAttribute(DBObjectXML.Attribute.LibraryNamespace, libNamespace);
						break;

					default:
						return;
				}

				// leggo il namespace completo dell'oggetto
				string objectNameSpace = objectNode.GetAttribute(DBObjectXML.Attribute.TbNamespace);
				targetNode.SetAttribute(DBObjectXML.Attribute.TbNamespace, objectNameSpace);

				string localize = objectNode.GetAttribute(DBObjectXML.Attribute.Localize);
				targetNode.SetAttribute(DBObjectXML.Attribute.Localize, localize);

				ParseColumns(objectNode, targetNode);
			}
		}

		//---------------------------------------------------------------------
		private void ParseColumns(XmlElement sourceNode, XmlElement targetNode)
		{
			bool isProcedureNode = 
				string.Compare(sourceNode.Name, DBObjectXML.Element.Procedure, StringComparison.InvariantCultureIgnoreCase) == 0;

			XmlNodeList columnsList = 
				isProcedureNode
				? sourceNode.SelectNodes(DBObjectXML.Element.Parameters + "/" + DBObjectXML.Element.Parameter) // estraggo i nodi di tipo <Parameter>
				: sourceNode.SelectNodes(DBObjectXML.Element.Columns + "/" + DBObjectXML.Element.Column); // estraggo i nodi di tipo <Column>
			
			if (columnsList == null)
				return;

			XmlElement columns = dbObjectsDoc.CreateElement(isProcedureNode ? DBObjectXML.Element.Parameters : DBObjectXML.Element.Columns);
			targetNode.AppendChild(columns);

			foreach (XmlElement column in columnsList)
			{
				XmlElement colNode = dbObjectsDoc.CreateElement(isProcedureNode ? DBObjectXML.Element.Parameter : DBObjectXML.Element.Column);
				columns.AppendChild(colNode);

				// leggo i nodi di tipo <Column>
				string colName = column.GetAttribute(DBObjectXML.Attribute.Name);
				colNode.SetAttribute(DBObjectXML.Attribute.Name, colName);

				string localize = column.GetAttribute(DBObjectXML.Attribute.Localize);
				colNode.SetAttribute(DBObjectXML.Attribute.Localize, localize);
				
				string dataType = column.GetAttribute(DBObjectXML.Attribute.DataType);
				WizardTableColumnDataType type = new WizardTableColumnDataType(dataType);
				//traduciamo da tipo del wizard a tipo di TB
				colNode.SetAttribute(DBObjectXML.Attribute.DataType, WizardTableColumnDataType.GetDataTypeTBXmlValue(type.Type));

				string baseType = column.GetAttribute(DBObjectXML.Attribute.BaseType);
				if (!string.IsNullOrEmpty(baseType))
					colNode.SetAttribute(DBObjectXML.Attribute.BaseType, baseType);

				string dataLength = column.GetAttribute(DBObjectXML.Attribute.DataLength);
				if (!string.IsNullOrEmpty(dataLength))
					colNode.SetAttribute(DBObjectXML.Attribute.DataLength, dataLength);
			}
		}
	}

	///<summary>
	/// Classe che si occupa della generazione del bin serializzando il file xml del BinGenerator
	///</summary>
	//================================================================================
	public class BinaryFileParser : IDisposable
	{
		private Stream stream;
		private const int numberLen = 4; //sizeof(uint)

		//--------------------------------------------------------------------------------
		public BinaryFileParser(string fileName)
		{
			this.stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
		}

		//--------------------------------------------------------------------------------
		public void UnparseString(string s)
		{
			byte[] buff = Encoding.UTF8.GetBytes(s);
			UnparseUInt(Convert.ToUInt32(buff.Length));
			stream.Write(buff, 0, buff.Length);
		}

		//--------------------------------------------------------------------------------
		public void UnparseUInt(uint n)
		{
			stream.Write(BitConverter.GetBytes(n), 0, numberLen);
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			if (stream != null)
				stream.Close();
		}

		//TUTTA LA PARTE SOTTOSTANTE NON È UTILIZZATA
		/*public long Position { get { return stream.Position; } }
		  public bool EOF { get { return stream.Position == stream.Length; } }
		*/

		/*//--------------------------------------------------------------------------------
		public BinaryFileParser(Stream stream)
		{
			this.stream = stream;
		}*/

		/*//--------------------------------------------------------------------------------
		private void Seek(int offset)
		{
			stream.Seek(offset, SeekOrigin.Begin);
		}*/

		/*//--------------------------------------------------------------------------------
		public string ParseString()
		{
			int len = Convert.ToInt32(ParseUInt());
			if (len == 0)
				return string.Empty;

			byte[] buff = new byte[len];
			if (stream.Read(buff, 0, len) != len)
				throw new BinaryFileParserException("TODO");

			return Encoding.UTF8.GetString(buff, 0, len);

		}*/

		/*//--------------------------------------------------------------------------------
		public uint ParseUInt()
		{
			byte[] buff = new byte[numberLen];
			if (stream.Read(buff, 0, numberLen) != numberLen)
				throw new BinaryFileParserException("TODO");

			return BitConverter.ToUInt32(buff, 0);
		}*/

		/*//--------------------------------------------------------------------------------
		public byte ParseByte()
		{
			int b;
			if ((b = stream.ReadByte()) == -1)
				throw new BinaryFileParserException("TODO");

			return Convert.ToByte(b);
		}*/

		/*//--------------------------------------------------------------------------------
		public void UnparseByte(byte b)
		{
			stream.WriteByte(b);
		}*/

		/*//--------------------------------------------------------------------------------
		public void Clear()
		{
			stream.SetLength(0);
		}*/
	}

	/*//================================================================================
	public class BinaryFileParserException : ApplicationException
	{
		//---------------------------------------------------------------------
		public BinaryFileParserException(string message, params object[] args)
			: base(string.Format(message, args))
		{
		}
	}*/
}
