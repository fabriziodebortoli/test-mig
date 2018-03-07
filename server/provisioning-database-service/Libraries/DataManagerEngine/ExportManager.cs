using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Microarea.Common.NameSolver;
using Microarea.ProvisioningDatabase.Libraries.DatabaseManager;
using TaskBuilderNetCore.Interfaces;

namespace Microarea.ProvisioningDatabase.Libraries.DataManagerEngine
{
	/// <summary>
	/// ExportManager gestore delle operazioni di export dei dati
	/// </summary>
	//=========================================================================
	public class ExportManager
	{
		private const string schemaNamespace = "http://www.w3.org/2001/XMLSchema";
		
		private ExportSelections	expSelections;
		private DatabaseDiagnostic	dbDiagnostic;
		public	string				DataMngPath;

		//---------------------------------------------------------------------
		public ExportManager(ExportSelections expSel, DatabaseDiagnostic diagnostic)
		{
			expSelections = expSel;	
			dbDiagnostic = diagnostic;

			dbDiagnostic.SetGenericText(string.Format(DataManagerEngineStrings.MsgExportCompanyData, expSelections.ContextInfo.CompanyName));
		}

		# region Funzioni per richiamare l'algoritmo di Esportazione dati (con thread separato)
		//---------------------------------------------------------------------
		public Thread Export()
		{
			Thread myThread = new Thread(new ThreadStart(InternalExport));
			myThread.Start();
			return myThread;
		}
		
		//---------------------------------------------------------------------
		private void InternalExport()
		{
			DataMngPath = Path.Combine
				(
				expSelections.ContextInfo.PathFinder.GetCustomCompanyDataManagerPath(expSelections.ContextInfo.CompanyName),
				expSelections.ContextInfo.CompanyName + DateTime.Now.ToString(DataManagerConsts.ExportDateTime)
				);

			string fileName = string.Empty;
			string tableName = string.Empty;

			try
			{
				//DataSet dataSet = null;
				if (!PathFinder.PathFinderInstance.ExistPath (DataMngPath))
                    PathFinder.PathFinderInstance.CreateFolder(DataMngPath, false);
						
				if (!expSelections.OneFileForTable)
				{
					//dataSet = new DataSet();
					// Modify to match the other dataset
					//dataSet.DataSetName = DataManagerConsts.DataTables;
				}

				foreach (CatalogTableEntry entry in expSelections.Catalog.TblDBList)
				{
					if (expSelections.AllTables || entry.Selected)
					{
						//@@TODOMICHI
						//ExportTable(entry, ref dataSet);
						tableName = entry.TableName;
					}

					// se l'utente vuole interrompere l'elaborazione allora non procedo.
					if (dbDiagnostic.AbortWizard)
						break;
				}
				
				if (!expSelections.OneFileForTable)
				{
					fileName = Path.Combine(DataMngPath, DataManagerConsts.ExportDataFileName);

					//@@TODOMICHI (devo scrivere l'xml)
					/*XmlTextWriter myXmlWriter = new XmlTextWriter(fileName, Encoding.UTF8);
					myXmlWriter.Formatting = Formatting.Indented;
					myXmlWriter.WriteStartDocument(true);
					// Write to the file with the WriteXml method.
					dataSet.WriteXml(myXmlWriter, (expSelections.SchemaInfo) ? XmlWriteMode.WriteSchema : XmlWriteMode.IgnoreSchema);   
					myXmlWriter.Close();*/

					if (expSelections.ContextInfo.DbType != DBMSType.SQLSERVER)
						ModifyNotSqlFile(fileName, false);
				}			
			}
			catch (Exception e) 
			{
				dbDiagnostic.SetMessageNoAppAndModuleName
								(
								false,
								fileName,
								tableName,
								e.Message,
								string.Empty
								);
				string error = "Error exporting table " + tableName + "\r\nFile name " + fileName + e.Message;
				Debug.Fail(error);
				Debug.WriteLine(error + e.ToString());
			}

			// al termine dell'elaborazione setto la progress bar e visualizzo il messaggio
			dbDiagnostic.SetFinish((dbDiagnostic.AbortWizard) ? DataManagerEngineStrings.MsgOperationInterrupted : DataManagerEngineStrings.MsgEndOperation);

			// salvo il file di log
			string filePath = this.expSelections.CreateLogFile(dbDiagnostic);

			// scrivo la riga con il riferimento al file di log salvato
			dbDiagnostic.SetMessageNoAppAndModuleName(true, Path.GetFileName(filePath), DatabaseManagerStrings.CreateLogFile, string.Empty, filePath);
		}
		#endregion

		#region Funzioni per esportazione singola tabella e gestione tag Optional
		///<summary>
		/// ExportSqlTable
		/// utilizzo l'ExecuteXmlReader per esportare i dati per SQL Server
		/// </summary>
		//---------------------------------------------------------------------
		/*private bool ExportSqlTable(string query, ref DataSet dataSet) //@@TODOMICHI
		{
			TBCommand command = new TBCommand(query, expSelections.ContextInfo.Connection);
			command.CommandTimeout = 0;

			if (dataSet == null)
				return false;

			try
			{
				using (XmlReader xmlReader = command.ExecuteXmlReader())
					dataSet.ReadXml(xmlReader, XmlReadMode.Fragment);
			}
			catch (Exception e)
			{
				Debug.WriteLine("Error in ExportSqlTable: " + e.Message);
				return false;
			}

			return true;
		}*/

		///<summary>
		/// ExportOracleTable
		/// Esportazione manuale perchè il provider Oracle non espone il metodo ExecuteXmlReader
		/// eseguo i seguenti passi:
		/// 1) fill del dataset mediante adapter
		/// 2) creazione del file xml attraverso il dataset
		/// 3) modifica del file (metodo ModifyOracleAndMySqlExportFile) passando dalla sintassi 
		/// <TableName>
		///		<ColName1> value1 <\ColName1>
		///		<ColName2> value2 <\ColName2>
		/// <\TableName>
		/// alla sintassi con attributi utilizzata per SqlServer e dal processo di importazione
		/// <TableName ColName1 = "value1", ColName2 = "value2">
		/// </summary>
		//---------------------------------------------------------------------
		/*private void ExportOracleTable(string tableName, string query, ref DataSet dataSet)
		{
			if (dataSet == null)
				return;

			try
			{
				TBDataAdapter adapter = new TBDataAdapter(query, expSelections.ContextInfo.Connection);
				adapter.Fill(dataSet);
				adapter.Dispose();
			}
			catch (TBException tbExc)
			{
				if (expSelections.OneFileForTable)
					dataSet = null;

				string error = "Error Fill data for table " + tableName;
				Debug.WriteLine(error + "\r\n" + tbExc.ToString());
				throw;
			}
		}*/

		//---------------------------------------------------------------------
		/*private void ExportPostgreTable(string tableName, string query, ref DataSet dataSet) //@@TODOMICHI
        {
            if (dataSet == null)
                return;

            try
            {
                TBDataAdapter adapter = new TBDataAdapter(query, expSelections.ContextInfo.Connection);
                adapter.Fill(dataSet);
                adapter.Dispose();
            }
            catch (TBException tbExc)
            {
                if (expSelections.OneFileForTable)
                    dataSet = null;

                string error = "Error Fill data for table " + tableName;
                Debug.WriteLine(error + "\r\n" + tbExc.ToString());
                throw;
            }
        }*/

		//---------------------------------------------------------------------
		//public bool ExportTable(CatalogTableEntry entry, ref DataSet dataSet)
		//{	
		//	// se il diagnostic mi dice che il wizard deve essere abortito non procedo...
		//	if (dbDiagnostic.AbortWizard)
		//		return false;

		//	string query = expSelections.MakeExportQuery(entry);

		//	if (string.IsNullOrWhiteSpace(query))
		//		return false; //ERRORE

		//	// mi serve per capire se devo forzare il parse alla Oracle anche per un tabella di tipo SQL
		//	// nel caso di errori in presenza di caratteri strani (ad esempio HTML)
		//	bool forceModifySQLFile = false;

		//	try
		//	{
		//		if (dataSet == null)
		//			dataSet = new DataSet();

		//		switch (expSelections.ContextInfo.DbType)
		//		{
		//			case DBMSType.SQLSERVER:
		//				if (!ExportSqlTable(query, ref dataSet))
		//				{
		//					// se fallisce la query riprovo a farla alla Oracle, ma SOLO se sto esportando le tabelle in singoli file
		//					// sul file ExportData.xml mi troverei ad avere un xml misto
		//					if (expSelections.OneFileForTable)
		//					{
		//						query = query.Replace(" FOR XML AUTO, XMLDATA", string.Empty);
		//						dataSet = new DataSet();
		//						forceModifySQLFile = true;
		//						goto case DBMSType.ORACLE;
		//					}
		//				}
		//				break;

		//			case DBMSType.ORACLE:
		//				{
		//					ExportOracleTable(entry.TableName, query, ref dataSet);
		//					if (dataSet.Tables.Count > 0)
		//						dataSet.Tables[dataSet.Tables.Count - 1].TableName = entry.TableName;
		//					break;
		//				}

  //                  case DBMSType.POSTGRE:
  //                      {
  //                          ExportPostgreTable(entry.TableName, query, ref dataSet);
  //                          if (dataSet.Tables.Count > 0)
  //                              dataSet.Tables[dataSet.Tables.Count - 1].TableName = entry.TableName;
  //                          break;
  //                      }

		//			default:
		//				return false; //ERRORE
		//		}

		//		// Modify to match the other dataset
		//		// NON METTERLO PRIMA, XCHÈ LA READXML SOVRASCRIVE IL DATASETNAME
		//		dataSet.DataSetName = DataManagerConsts.DataTables;

		//		// controllo che la tabella analizzata contenga delle righe di dati da esportare.
		//		// se è vuota non procedo nella scrittura e salvataggio del file xml 
		//		int rows = 0;
		//		//@@TODOMICHI
		//		/*foreach (DataTable myTable in dataSet.Tables)
		//		{
		//			if (string.Compare(myTable.TableName, entry.TableName, StringComparison.OrdinalIgnoreCase) == 0)
		//				rows += myTable.Rows.Count;
		//		}*/

		//		if (rows == 0)
		//		{
		//			dbDiagnostic.SetMessage
		//				(
		//				true,
		//				entry.Application,
		//				entry.Module,
		//				string.Empty,
		//				entry.TableName,
		//				DataManagerEngineStrings.ErrTableEmpty,
		//				DataMngPath
		//				);
		//			return true;
		//		}

		//		string fileName, detail;
		//		if (expSelections.OneFileForTable)
		//		{
		//			fileName = DataMngPath + Path.DirectorySeparatorChar;

		//			if (entry.Append)
		//				fileName = fileName + entry.TableName + DataManagerConsts.Append + NameSolverStrings.XmlExtension;
		//			else
		//				fileName = fileName + entry.TableName + NameSolverStrings.XmlExtension;

		//			XmlWriter myXmlWriter = XmlWriter.Create(File.Open(fileName, FileMode.OpenOrCreate), new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 });
		//			myXmlWriter.WriteStartDocument(true);

		//			// se la tabella che sto considerando ha una colonna di tipo text e sto forzando la gestione alla Oracle (perche' si 
		//			// e' verificato un errore nell'esecuzione della FOR XML AUTO di SQL)
		//			// devo andare a mano a manipolare il DataSet e pulire i caratteri che la WriteXml non riesce ad escapare
		//			// (e genera poi un errore in lettura)
		//			//@@TODOMICHI
		//			/*if (entry.HasDataTextColumn && forceModifySQLFile)
		//			{
		//				foreach (DataTable dt in dataSet.Tables)
		//				{
		//					for (int i = 0; i < dt.Rows.Count; i++)
		//					{
		//						DataRow dr = dt.Rows[i];

		//						for (int y = 0; y < dt.Columns.Count; y++)
		//						{
		//							DataColumn col = dt.Columns[y];
		//							if (col.DataType == Type.GetType("System.String"))
		//							{
		//								dr.BeginEdit();
		//								//Debug.WriteLine(string.Format("Analyzing special chars in column {0} of table {1} ", col.ColumnName, entry.TableName));
		//								dr[y] = RemoveTroublesomeCharacters(dr[y].ToString());
		//								dr.AcceptChanges();
		//								dr.EndEdit();
		//							}
		//						}
		//					}
		//				}
		//			}

		//			// Write to the file with the WriteXml method.
		//			dataSet.WriteXml(myXmlWriter, (expSelections.SchemaInfo) ? XmlWriteMode.WriteSchema : XmlWriteMode.IgnoreSchema);
		//			myXmlWriter.Close();*/

		//			if (expSelections.ContextInfo.DbType != DBMSType.SQLSERVER || forceModifySQLFile)
		//				ModifyNotSqlFile(fileName, entry.Optional); // il controllo dell'optional lo faccio già mentre sto modificando il file
		//			else
		//				if (entry.Optional)
		//					SetFileXmlOptional(fileName);

		//			dataSet = null;

		//			dbDiagnostic.SetMessage
		//				(
		//				true,
		//				entry.Application,
		//				entry.Module,
		//				fileName = (entry.Append) ? (entry.TableName + DataManagerConsts.Append + NameSolverStrings.XmlExtension) : (entry.TableName + NameSolverStrings.XmlExtension),
		//				entry.TableName,
		//				detail = (entry.Append) ? DataManagerEngineStrings.OptionalFile : string.Empty,
		//				Path.Combine(DataMngPath, fileName)
		//				);
		//		}
		//		else
		//			dbDiagnostic.SetMessage
		//				(
		//				true,
		//				entry.Application,
		//				entry.Module,
		//				DataManagerConsts.ExportDataFileName,
		//				entry.TableName,
		//				string.Empty,
		//				Path.Combine(DataMngPath, DataManagerConsts.ExportDataFileName)
		//				);
		//	}
		//	catch (OutOfMemoryException ome)
		//	{
		//		Debug.Fail(ome.ToString());
		//		dbDiagnostic.SetMessage
		//			(
		//			false,
		//			entry.Application,
		//			entry.Module,
		//			string.Empty,
		//			entry.TableName,
		//			ome.ToString(),
		//			DataMngPath
		//			);
		//		return false;
		//	}
		//	catch (TBException tbExc)
		//	{
		//		Debug.Fail(tbExc.ToString());
		//		dbDiagnostic.SetMessage
		//			(
		//			false,
		//			entry.Application,
		//			entry.Module,
		//			string.Empty,
		//			entry.TableName,
		//			tbExc.ToString(),
		//			DataMngPath
		//			);
		//		return false;
		//	}
		//	catch (Exception e)
		//	{
		//		Debug.Fail(e.ToString());
		//		dbDiagnostic.SetMessage
		//			(
		//			false,
		//			entry.Application,
		//			entry.Module,
		//			string.Empty,
		//			entry.TableName,
		//			e.ToString(),
		//			DataMngPath
		//			);
		//		return false;
		//	}
			
		//	return true;
		//}

		///// <summary>
		///// permette di rendere opzionale un file di dati di default ovvero aggiunge
		///// al tag DataTables l'attributo optional = 'true'
		///// </summary>
		////---------------------------------------------------------------------
		//public void SetFileXmlOptional(string fileName)
		//{
		//	XmlDocument xDoc = new XmlDocument();
		//	xDoc.Load(File.OpenText(fileName));
						
		//	//root del documento (DataTables)
		//	XmlElement root = xDoc.DocumentElement;
		//	if (root != null)
		//		root.SetAttribute(DataManagerConsts.Optional, bool.TrueString);

		//	// creo l'xmlwriter per dallo streamwriter e relativi settings
		//	XmlWriter writer = XmlWriter.Create(File.Open(fileName, FileMode.OpenOrCreate), new XmlWriterSettings { Indent = true });
		//	xDoc.WriteContentTo(writer);
		//	writer.Flush();
		//}		
		# endregion

		///<summary>
		/// Metodo richiamato SOLO per i database Oracle (fix anomalia nr. 18146)
		/// Durante l'esportazione di tabelle contenenti grandi quantita' di dati, accadeva che il 
		/// caricamento in memoria del file "intermedio" (per aggiustare la sintassi dei file) tramite il DOM
		/// generava un'OutOfMemoryException.
		/// Per ovviare all'errore, viene istanziato un XmlReader sul file intermedio e un XmlWriter su un file di
		/// appoggio. Si procede poi sequenzialmente con il Reader: per ogni riga letta viene riempita una struttura
		/// dati in memoria e poi viene scritta una riga nel file di appoggio tramite il Writer.
		/// Alla fine il file intermedio viene eliminato e il file di appoggio viene rinominato con il nome corretto.
		///</summary>
		//--------------------------------------------------------------------------------
		private void ModifyNotSqlFile(string fileName, bool optional)
		{
			Stream myStream = null;
			XmlReader xReader = null;
			StreamWriter sWriter = null;
			XmlWriter xWriter = null;

			string newFileName = Path.Combine(Path.GetDirectoryName(fileName), string.Concat("NEW", Path.GetFileName(fileName)));

			try
			{
				// carico in uno stream il file intermedio con la sintassi ad albero generata da Oracle
				myStream = File.Open(fileName, FileMode.Open);

				// creo l'xmlreader dallo stream con i settings
				xReader = XmlReader.Create(myStream, new XmlReaderSettings() { IgnoreComments = true, IgnoreWhitespace = true, ConformanceLevel = ConformanceLevel.Document });
				
				// creo un file di appoggio nel quale andro' a scrivere i dati con la sintassi corretta
				StreamWriter swNewFile = File.CreateText(newFileName);

				// istanzio lo streamwriter sul file di appoggio
				sWriter = new StreamWriter(File.Open(newFileName, FileMode.OpenOrCreate), Encoding.UTF8);

				// creo l'xmlwriter dallo streamwriter e relativi settings
				xWriter = XmlWriter.Create(sWriter, new XmlWriterSettings() { ConformanceLevel = ConformanceLevel.Document, Encoding = Encoding.UTF8, Indent = true });

				// con il reader mi posiziono sul primo nodo "buono"
				xReader.MoveToContent();

				xWriter.WriteStartDocument(true); // imposto l'attributo standalone="yes"
				xWriter.WriteStartElement(xReader.Prefix, xReader.LocalName, xReader.NamespaceURI);

				// GESTIONE ATTRIBUTO OPTIONAL
				if (optional)
				{
					if (xReader.NodeType == XmlNodeType.Element &&
						string.Compare(xReader.Name, DataManagerConsts.DataTables, StringComparison.OrdinalIgnoreCase) == 0)
						xWriter.WriteAttributeString(DataManagerConsts.Optional, bool.TrueString);
				}

				WriteAttributes(xReader, xWriter);

				xReader.Read();

				do
				{
					List<SingleRow> rows = new List<SingleRow>();

					// leggo ed elaboro una riga nel file intermedio
					ReadRow(xReader, xReader.LocalName, rows);

					// scrivo la riga con la sintassi corretta nel file di appoggio
					WriteRow(xWriter, xReader.LocalName, rows);

					xReader.Read();
				}
				while (xReader.NodeType == XmlNodeType.Element); // vado avanti a leggere fintanto che trovo elementi
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
				throw (ex);
			}
			finally
			{
				xReader.Dispose();
				xReader = null;
			}

			try
			{
				// elimino il vecchio file (se esiste)
				if (expSelections.ContextInfo.PathFinder.ExistFile(fileName))
					File.Delete(fileName);
				// rinomino il nuovo (se esiste) con il file originale
				if (expSelections.ContextInfo.PathFinder.ExistFile(newFileName))
					File.Move(newFileName, fileName);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
				throw (ex); 
			}
		}

		///<summary>
		/// Legge una riga del file intermedio e riempie una lista di oggetti con il nome della tabella
		/// e relativi attributi e valori
		///</summary>
		//--------------------------------------------------------------------------------
		private void ReadRow(XmlReader reader, string tableName, List<SingleRow> rows)
		{
			try
			{
				SingleRow sRow = new SingleRow();
				sRow.Name = tableName;

				while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						AttributeValue am = new AttributeValue();
						am.Name = reader.Name;
						am.Value = reader.ReadElementContentAsString();
						sRow.Attributes.Add(am);
					}
				}

				rows.Add(sRow);
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}

		///<summary>
		/// Scrive una riga nel file di appoggio componendo la nuova struttura basandosi
		/// sulla lista di informazioni
		///</summary>
		//--------------------------------------------------------------------------------
		private void WriteRow(XmlWriter writer, string tableName, List<SingleRow> rows)
		{
			try
			{
				foreach (SingleRow am in rows)
				{
					writer.WriteStartElement(am.Name);

					foreach (AttributeValue av in am.Attributes)
						writer.WriteAttributeString(av.Name, av.Value);

					writer.WriteEndElement();
				}
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}

		//--------------------------------------------------------------------------------
		private void WriteAttributes(XmlReader reader, XmlWriter writer)
		{
			try
			{
				if (reader.MoveToFirstAttribute())
				{
					do
					{
						writer.WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
					}
					while (reader.MoveToNextAttribute());

					reader.MoveToElement();
				}
			}
			catch (Exception ex)
			{
				throw (ex);
			}
		}

		///<summary>
		/// Metodo che si occupa di eliminare tutti i caratteri "strani" da una stringa
		/// (per ovviare al problema di esportazione dati da colonne di tipo text)
		///</summary>
		//--------------------------------------------------------------------------------
		private string RemoveTroublesomeCharacters(string inString)
		{
			if (inString == null)
				return string.Empty;

			StringBuilder newString = new StringBuilder();
			char ch;

			for (int i = 0; i < inString.Length; i++)
			{
				ch = inString[i];
				// remove any characters outside the valid UTF-8 range as well as all control characters
				// except tabs and new lines and euro
				if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r' || ch == '€')
					newString.Append(ch);
				else
					Debug.WriteLine(string.Format("Remove char {0}", ch));
			}
			return newString.ToString();
		}

		# region Vecchia gestione per l'aggiustamento della sintassi (SOLO per Oracle)
		/// <summary>
		/// modifica del file (metodo ModifyOracleExportFile) passando dalla sintassi 
		/// <TableName>
		///		<ColName1> value1 </ColName1>
		///		<ColName2> value2 </ColName2>
		/// </TableName>
		/// alla sintassi con attributi utilizzata per SqlServer e dal processo di importazione
		/// <TableName ColName1 = "value1", ColName2 = "value2">
		/// </summary>
		//---------------------------------------------------------------------
		private void ModifyOracleExportFile(string fileName, bool setOptional)
		{
			XmlDocument xDoc = new XmlDocument();

			try
			{
				xDoc.Load(File.OpenText(fileName));

				//root del documento (DataTables)
				XmlElement root = xDoc.DocumentElement;

				// ottimizzazione: modifico già l'attributo di opzionale (se voluto)
				if (setOptional && root != null)
					root.SetAttribute(DataManagerConsts.Optional, bool.TrueString);

				foreach (XmlElement node in root.ChildNodes)
				{
					if (string.Compare(node.LocalName, "schema", StringComparison.OrdinalIgnoreCase) == 0 &&
						string.Compare(node.NamespaceURI, schemaNamespace, StringComparison.OrdinalIgnoreCase) == 0)
						ModifySchemaXmlElement(node);
					else
						ModifyTableXmlElement(node);
				}

				XmlWriter writer = XmlWriter.Create(File.Open(fileName, FileMode.OpenOrCreate), new XmlWriterSettings { Indent = true });
				xDoc.WriteContentTo(writer);
				writer.Flush();
			}
			catch (OutOfMemoryException ome)
			{
				string error = "Error in file name " + fileName + "\r\n" + ome.Message;
				Debug.WriteLine(error + ome.ToString());
				throw (ome);
			}
			catch (Exception e)
			{
				string error = "Error in file name " + fileName + "\r\n" + e.Message;
				Debug.WriteLine(error + e.ToString());
				throw (e);
			}
		}

		//---------------------------------------------------------------------
		private void ModifySchemaXmlElement(XmlElement node)
		{
			try
			{
				XmlNamespaceManager nsm = new XmlNamespaceManager(new NameTable());
				nsm.AddNamespace("xs", schemaNamespace);

				XmlNodeList nodeList =
					node.SelectNodes("xs:element[@name = 'DataTables']/xs:complexType/xs:choice/xs:element/xs:complexType", nsm);

				foreach (XmlElement complexNode in nodeList)
				{
					XmlElement sequenceNode = (XmlElement)complexNode.RemoveChild(complexNode.FirstChild);

					foreach (XmlElement attrNode in sequenceNode.ChildNodes)
					{
						XmlElement newNode = node.OwnerDocument.CreateElement("xs", "attribute", node.NamespaceURI);
						newNode.SetAttribute("name", attrNode.GetAttribute("name"));
						newNode.SetAttribute("type", attrNode.GetAttribute("type"));
						complexNode.AppendChild(newNode);
					}
				}
			}
			catch (OutOfMemoryException ome)
			{
				Debug.WriteLine(ome.ToString());
				throw (ome);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.ToString());
				throw (e);
			}
		}

		//---------------------------------------------------------------------
		private void ModifyTableXmlElement(XmlElement node)
		{
			try
			{
				foreach (XmlElement colNode in node.ChildNodes)
					node.SetAttribute(colNode.Name, colNode.InnerText);
				while (node.ChildNodes.Count > 0)
					node.RemoveChild(node.LastChild);
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.ToString());
				throw (e);
			}
		}
		# endregion
	}

	# region Classi di appoggio per memorizzare la struttura dati letta dal file intermedio (solo per Oracle)
	//================================================================================
	public class SingleRow
	{
		public string Name { get; set; }
		public List<AttributeValue> Attributes = new List<AttributeValue>();
	}

	//================================================================================
	public class AttributeValue
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}
	# endregion
}