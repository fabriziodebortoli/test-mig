using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;

using Microarea.TaskBuilderNet.Core.NameSolver;

using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Woorm.WoormEngine
{
	/// <summary>
	/// Summary description for TableNode.
	/// </summary>
	/// ================================================================================
	public class TableNode
	{
		public ushort		Id;
		public XmlElement	Table;
		public XmlElement	Row;
	}

	/// <summary>
	/// Summary description for TableNodeList.
	/// </summary>
	/// ================================================================================
	public class TableNodeList : ArrayList
	{
		XmlDocument	output;
		XmlElement	reportDataNode;
		string		namespaceURI;
		XmlWriter   xmlWriter;

		//---------------------------------------------------------------------------
		public TableNodeList (XmlDocument output, XmlElement reportDataNode, string namespaceURI, XmlWriter xmlW)
			: base()
		{
			this.output = output;
			this.reportDataNode = reportDataNode;
			this.namespaceURI = namespaceURI;
			
			xmlWriter = xmlW;
		}

		//---------------------------------------------------------------------------
		public string GetTableIdByName(string TableName)
		{
			foreach (TableNode node in this)
				if (node.Table.Name == TableName)
					return node.Id.ToString();

			return string.Empty;
		}

		//---------------------------------------------------------------------------
		public TableNode GetTableNode(ushort id)
		{
			foreach (TableNode node in this)
				if (node.Id == id)
					return node;

			TableNode tableNode = new TableNode();
			tableNode.Id = id;

			string tableName = xmlWriter.GetTableName(id);
            tableNode.Table = output.CreateElement(XmlWriterTokens.Prefix, tableName, namespaceURI);
			reportDataNode.AppendChild(tableNode.Table);
            
            string sRow = xmlWriter.GetRowName(id);
			tableNode.Row = output.CreateElement(XmlWriterTokens.Prefix, sRow, namespaceURI);

			tableNode.Table.AppendChild(tableNode.Row);

			this.Add(tableNode);
			return tableNode;
		}

		//---------------------------------------------------------------------------
        public XmlElement GetTableRowNode(ushort id)
		{
			return GetTableNode(id).Row;
		}

		//---------------------------------------------------------------------------
		public void AddNewRow(ushort tableId)
		{
			// crea una nuova riga
			foreach (TableNode node in this)
                if (node.Id == tableId)
				{
                    string sRow = xmlWriter.GetRowName(tableId);

					node.Row = output.CreateElement(XmlWriterTokens.Prefix, sRow, namespaceURI);

					node.Table.AppendChild(node.Row);
					return;
				}

			// crea il nota tabella e la prima riga
            GetTableNode(tableId);
		}
	}

	/// <summary>
	/// Summary description for XmlWriter.
	/// </summary>
	/// ================================================================================
	public class XmlWriter : RdeWriter, IDisposable
	{
		string root;
		string tbNamespace;
		string namespaceURI;

		// TB C++ compatible approximation
		const double EPSILON = 0.0000001F;
		const int EPSILON_DECIMAL = 7;

		XmlElement reportDataNode;
		XmlElement parametersNode;
		TableNodeList tableNodes;

		//---------------------------------------------------------------------------
		ILocalizer Localizer { get { return report.ReportSession.Localizer; } }


		override public	string	Release { get { return "1"; }}

		//---------------------------------------------------------------------------
		public XmlWriter(Report report) : base(report)
		{				
			tbNamespace = report.ReportSession.ReportNamespace;
			NameSpace ns = new NameSpace(tbNamespace);
			root = ns.Report;
			if (root.Length == 0) 
				root = "UnknowReport";

			string user = BasePathFinder.GetUserUri(this.report.ReportSession.UserInfo.ImpersonatedUser);

			namespaceURI = string.Format(XmlWriterTokens.SchemaPrefix, ns.Application, ns.Module, user, ns.Report);
		}

		//------------------------------------------------------------------------------
		~XmlWriter()
		{
			Dispose(false);
		}

        private string Translate(string s)
        {
            return Localizer != null ? Localizer.Translate(s) : s;
        }

		//------------------------------------------------------------------------------
		override public void Close(string file)
		{
		}

		//------------------------------------------------------------------------------
		public void RemoveLastEmptyRow()
		{
			XmlNodeList reports = output.DocumentElement.GetElementsByTagName(XmlWriterTokens.Element.ReportData, namespaceURI);
			foreach (XmlNode reportData in reports)
				foreach (XmlElement item in reportData)
				{
					// solo le tabelle hanno dei nodi figli che sono le righe
					if (!item.HasChildNodes) 
                        continue;

          //TODO FATTURAZIONE ELETTRONICA   Ricontrollare questo metodo
					string sId = tableNodes.GetTableIdByName(item.Name);
                    if (sId == null || sId == string.Empty)
                        continue;

                    ushort tableId;
                    if (!ushort.TryParse(sId, out tableId))
                        continue;
                    
                    string sRow = GetRowName(tableId);

                    XmlNodeList rows = item.GetElementsByTagName(sRow, namespaceURI);
					if (rows.Count > 0)
					{
						XmlNode lastRow = rows[rows.Count - 1];
						if (!lastRow.HasChildNodes)
							item.RemoveChild(lastRow);
					}
				}
		}

		//------------------------------------------------------------------------------
		public void Close()
		{
			RemoveLastEmptyRow();
			report.XmlResultReports.Add(XmlDocumentToString(output));
			output = null;
		}

		// permette di aggiungere eventuali altre informazioni all' xml corrente
		//------------------------------------------------------------------------------
		override public void SavePage()
		{
			if (output != null)
			{
				AddParametersInfo(output, parametersNode);
				Close();
			}
		}

		// non scrive file di informazioni
		//------------------------------------------------------------------------------
		override public void SaveTotPageFile()
		{
		}

		//---------------------------------------------------------------------------
		private XmlElement TableRowNode(ushort tableId)
		{
            return tableNodes.GetTableRowNode(tableId);
		}
		
		// Salva in string un DOM preservando intestazione XML, whitespace etc...
		//------------------------------------------------------------------------------
		public static string XmlDocumentToString(XmlDocument dom)
		{
			StringWriter s = new StringWriter();
			dom.Save(s);
			
			return s.ToString();
		}

		//  non scrive il file di informazioni
		//------------------------------------------------------------------------------
		override public void SaveInfo()
		{
		}

		// permette di aggiungere eventuali altre informazioni al file xml corrente
		//------------------------------------------------------------------------------
		private void AddParametersInfo(XmlDocument dom, XmlElement whereInsert)
		{
			foreach (AskDialog ask in report.AskingRules)
			{

				XmlElement askNode = dom.CreateElement(XmlWriterTokens.Prefix, ask.FormName, namespaceURI);
                askNode.SetAttribute(XmlWriterTokens.Attribute.AskDialogTitle, Translate(ask.FormTitle));
				whereInsert.AppendChild(askNode);

				int i = 0;
				foreach (AskGroup group in ask.Groups)
				{
					i++;
					XmlElement groupNode = dom.CreateElement(XmlWriterTokens.Prefix, XmlWriterTokens.Element.Group + i.ToString(), namespaceURI);
					if (group.IsVisible) groupNode.SetAttribute(XmlWriterTokens.Attribute.GroupTitle, Translate(group.Caption));
					askNode.AppendChild(groupNode);

					foreach (AskEntry entry in group.Entries)
					{
						XmlElement element = dom.CreateElement(XmlWriterTokens.Prefix, entry.Field.Name, namespaceURI);

                        string sType = entry.Field.DataType;
                        //if (sType == "DateTime" && string.Compare(entry.Field.WoormType, "Date", true) == 0)
                        //    sType = "Date";
                        element.SetAttribute(XmlWriterTokens.Attribute.EntryType, sType);

						element.SetAttribute(XmlWriterTokens.Attribute.EntryLength, entry.Field.Len.ToString());
						element.SetAttribute(XmlWriterTokens.Attribute.EntryCaption, Translate(entry.Caption));
						element.SetAttribute(XmlWriterTokens.Attribute.ControlType, entry.ControlStyleAttributeValue);

						bool limit = false;
						switch (entry.Field.InputLimit)
						{
							case Token.LOWER_LIMIT : 
								element.SetAttribute(XmlWriterTokens.Attribute.InputLimit, XmlWriterTokens.AttributeValue.Lower);
								if (ObjectHelper.IsLowerValue(entry.Field.AskData))
								{
									element.InnerText = ""; // fa Encoding
									limit = true;
								}
								break;

							case Token.UPPER_LIMIT : 
								element.SetAttribute(XmlWriterTokens.Attribute.InputLimit, XmlWriterTokens.AttributeValue.Upper);
								if (ObjectHelper.IsUpperValue(entry.Field.AskData, report.MaxString))
								{
									element.InnerText = ""; // fa Encoding
									limit = true;
								}
								break;
						}

						if (!limit)
                            element.InnerXml = FormatValue(entry.Field.AskData, entry.Field.WoormType); 

						groupNode.AppendChild(element);
					}
				}

			}
		}

		//---------------------------------------------------------------------------
		override protected void XmlHeader() 
		{
			XmlElement reportNode = output.CreateElement(XmlWriterTokens.Prefix, root, namespaceURI);
			reportNode.SetAttribute(XmlWriterTokens.Attribute.TbNamespace, tbNamespace);
			reportNode.SetAttribute(XmlWriterTokens.XmlNs + XmlWriterTokens.Prefix, namespaceURI);
			output.AppendChild(reportNode);	

			reportDataNode = output.CreateElement(XmlWriterTokens.Prefix, XmlWriterTokens.Element.ReportData, namespaceURI);
			reportNode.AppendChild(reportDataNode);

			parametersNode = output.CreateElement(XmlWriterTokens.Prefix, XmlWriterTokens.Element.Parameters, namespaceURI);
			reportNode.AppendChild(parametersNode);

			tableNodes = new TableNodeList(output, reportDataNode, namespaceURI, this);
		}

		//---------------------------------------------------------------------------
		private string FormatMessage(StringCollection Errors)
		{
			string buffer = "";
			foreach (string s in Errors) buffer += s + "\r\n";
		
			return buffer;
		}

		//	aggiunge gli errori se ce ne sono stati
		//---------------------------------------------------------------------------
		override public bool XmlGetErrors(StringCollection errors, StringCollection warnings)
		{
			bool ok = true;
			try
			{
				XmlWriterHelper helper = new XmlWriterHelper(root, tbNamespace, namespaceURI, XmlWriterTokens.Prefix);
				if (errors.Count > 0) helper.AddError(1, "Easylook", FormatMessage(errors));
				if (warnings.Count > 0) helper.AddWarning(1, "Easylook", FormatMessage(warnings));

				report.XmlResultReports.Clear();
				report.XmlResultReports.Add(XmlDocumentToString(helper.Dom));
			}
			catch (IOException)
			{
				return false;
			}
			return ok;
		}


		// restituisce solo i parametri per permettere al chiamante di costruire la
		// dialog di richiesta
		//---------------------------------------------------------------------------
		override public bool XmlGetParameters()
		{
			bool ok = true;
			try
			{
				XmlDocument dom = new XmlDocument();

				XmlElement reportNode = dom.CreateElement(XmlWriterTokens.Prefix, root, namespaceURI);
				reportNode.SetAttribute(XmlWriterTokens.Attribute.TbNamespace, tbNamespace);
				reportNode.SetAttribute(XmlWriterTokens.XmlNs + XmlWriterTokens.Prefix, namespaceURI);
				dom.AppendChild(reportNode);	
	
				XmlElement insertNode = dom.CreateElement(XmlWriterTokens.Prefix, XmlWriterTokens.Element.Parameters, namespaceURI);
				reportNode.AppendChild(insertNode);

				AddParametersInfo(dom, insertNode);	
			
				// pulisco tutto perchè devo ritornare solo il dom dei parametri
				report.XmlResultReports.Clear();
				report.XmlResultReports.Add(XmlDocumentToString(dom));
			}
			catch (IOException)
			{
				return false;
			}
			return ok;
		}

		//---------------------------------------------------------------------------
		override public bool WriteMessageBox(Command cmd, string message)
		{
			return true;
		}		

		// In formato Xml non passo i subtotali automatici di colonna perchè non riesco a gestirli
		// in exel perchè sono dinamici ed arrivano quando voliono loro. Magari esplorare le tabelle Pivot.
		// Il nome della variabile di SubTotal è SubTot<nome della colonna> quindi occhio nella definizione
		// dello schema perchè non viene dichiarata. Occorre Strippare "SubTot" dal nome per poterlo passare
		// come dato di colonna.
		//---------------------------------------------------------------------------
		override public bool WriteSubTotal(ushort tableId, string name, int id, object o, bool last)		
		{ 
			return true; 
		}
		
		// In formato Xml non passo i totali automatici di colonna perchè si suppone che vengano 
		// calcolati da fuori attraverso altri tools. SI suppone che la tabella estratta sia una row
		// table. Occhio al nome: vedi SubTotal
		// Il nome della variabile di ColTotal è Tot<nome della colonna>
		//---------------------------------------------------------------------------
		override public bool WriteColTotal(ushort tableId, string name, int id, object o, bool last)		
		{ 
			return true;
		}

		// if enabled approximate only real value using the same approximation used in
		// TBc++ to manage precision on real numbers
		//---------------------------------------------------------------------------
		private object Approximate(object o)		
		{ 
			if (!this.report.ReportSession.UserInfo.UseApproximation)
				return o;

			switch (o.GetType().Name)
			{
				case "Decimal"	: return (decimal)	Math.Round((decimal)o,	EPSILON_DECIMAL);
				case "Single"	: return (float)	Math.Round((double)o,	EPSILON_DECIMAL);
				case "Double"	: return (double)	Math.Round((double)o,	EPSILON_DECIMAL);
				default : return o;
			}
		}

        //---------------------------------------------------------------------------
        protected string FormatValue(object o, string WoormType)
        {
            if (WoormType == "Date" && o.GetType().Name == "DateTime")
            {
                return SoapTypes.ToSoapDate((DateTime)o);
            }

            return SoapTypes.To(Approximate(o));
        }

		//---------------------------------------------------------------------------
        override public bool WriteCell(ushort tableId, string name, int id, object o, bool isCellTail, string WoormType, bool isValid)		
		{ 
			if (Open())
			{
				XmlElement element = output.CreateElement(XmlWriterTokens.Prefix, name, namespaceURI);
                if (this.report.ReportSession.WriteNotValidField && !isValid)
                {
                    element.SetAttribute(XmlWriterTokens.Attribute.IsValid, Boolean.FalseString);
                }
                element.InnerXml = FormatValue(o, WoormType);
                TableRowNode(tableId).AppendChild(element);

				return true; 
			}
			return false;
		}
				
		//---------------------------------------------------------------------------
        override public bool WriteField(string name, int id, object o, string WoormType, bool isValid)		
		{ 
			if (Open())
			{
				XmlElement element = output.CreateElement(XmlWriterTokens.Prefix, name, namespaceURI);
                if (this.report.ReportSession.WriteNotValidField && !isValid)
                {
                    element.SetAttribute(XmlWriterTokens.Attribute.IsValid, Boolean.FalseString);
                }
                element.InnerXml = FormatValue(o, WoormType);

				reportDataNode.AppendChild(element);

				return true;
			}
			return false;
		}

        //---------------------------------------------------------------------------
        public string GetTableName(ushort tableId)
        {
            //TODO FATTURAZIONE ELETTRONICA
            if (this.report.ReportSession.EInvoice && this.report.SymTable != null && this.report.SymTable.DisplayTables != null)
            {
                DisplayTable dt = this.report.SymTable.DisplayTables.Find(tableId);
                if (dt != null)
                {
                   return dt.PublicName;
                }
            }
            return XmlWriterTokens.Element.Table + tableId.ToString();
        }

        //---------------------------------------------------------------------------
        public string GetRowName(ushort tableId)
        {
        //TODO FATTURAZIONE ELETTRONICA
            if (this.report.ReportSession.EInvoice)
			{
				return GetTableName(tableId) + '_' + XmlWriterTokens.Element.Row;
			}
            return XmlWriterTokens.Element.Row;
        }

		//---------------------------------------------------------------------------
		override public bool WriteIDCommand(ushort tableId, string name, int id , object o, Command cmd, string WoormType)	
		{ 
			if (Open())
			{
				switch (cmd)
				{
					case Command.NextLine:
					case Command.TitleLine:
					case Command.Interline:
					{
                        tableNodes.AddNewRow(tableId);
						break;
					}

					// è un Field (non passo primo/ultimo, ma il valore effettivo)
					case Command.LowerInput:
					case Command.UpperInput:
                    return WriteField(name, id, o, WoormType, true);
				}
				return true; 
			}
			return false;
		}
	}
}
