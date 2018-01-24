using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microarea.Common.DiagnosticManager;
using Microarea.AdminServer.Libraries.DatabaseManager;
using TaskBuilderNetCore.Interfaces;
using Microarea.Common.NameSolver;

namespace Microarea.AdminServer.Libraries.DataManagerEngine
{
	# region GRAMMATICA FILE DI ESEMPIO
	/* PER IL PARSING DEL FILE CON QUESTA GRAMMATICA:
	<?xml version="1.0" encoding="UTF-8"?>
	<DefaultExportSelections>
		<Parameters>
			<Configuration value="" />
			<CountryCode value="" />
			<DBProvider value="SQLSERVER" />
			<ObjectsSelection allTables="true" allColumns="false" whereClause="false" />
			<MandatoryColumns colTBCreated="true" colTBModified="false" />
			<Script execute="false"></Script>
		</Parameters>
		<ExportedObjects>
			<Application name="">
				<Module name="">
					<Table name="" whereClause="" >
						<Column name="" />
					</Table>
				</Module>
			</Application>
		</ExportedObjects>
	</ExportSelections>

	se allTables="true" allColumns="true" 
	<ExportedObjects> non contiene nodi

	se allTables="false" allColumns="true" 
	non esistono nodi di tipo <Column>

	se allTables="false" allColumns="false" 
	il tag <ExportedObjects> sarà completo
	*/
	# endregion

	# region Classi di supporto per memorizzare le informazioni (TableNode, ModuleNode, ApplicationNode)
	/// <summary>
	/// Classe per identificare il nodo di tipo <Table>
	/// </summary>
	//=========================================================================
	public class TableNode
	{
		private string name = string.Empty;
		private string whereClause	= string.Empty;

		public List<string> Columns = null;

		public string Name { get { return name; } }
		public string WhereClause { get { return whereClause; } }

		//-----------------------------------------------------------------------
		public TableNode(string name, string whereClause)
		{
			this.name = name;
			this.whereClause = whereClause;
		}

		//-----------------------------------------------------------------------
		public bool ExistColumn(string name)
		{
			return Columns.Contains(name);
		}
	}

	/// <summary>
	/// Classe per identificare il nodo di tipo <Module>
	/// </summary>
	//=========================================================================
	public class ModuleNode
	{
		private string name = string.Empty;
		public List<TableNode> Tables = null;

		public string Name { get { return name; } }

		//-----------------------------------------------------------------------
		public ModuleNode(string name)
		{
			this.name = name;
		}
	}

	/// <summary>
	/// Classe per identificare il nodo di tipo <Application>
	/// </summary>
	//=========================================================================
	public class ApplicationNode
	{
		private string name = string.Empty;
		public List<ModuleNode> Modules = null;

		public string Name { get { return name; } }

		//-----------------------------------------------------------------------
		public ApplicationNode(string name)
		{
			this.name = name;
		}
	}
	# endregion

	/// <summary>
	/// ConfigurationInfo
	/// Classe utilizzata nel wizard di creazione dati di default per:
	/// 1. leggere le informazioni da un file xml in modo da pre-caricare le selezioni nel wizard
	/// 2. scrivere in un file xml le selezioni effettuate nel wizard
	/// </summary>
	//=========================================================================
	public class ConfigurationInfo
	{
		public string	FilePath		= string.Empty;
		
		public string	Configuration	= string.Empty;
		public string	CountryCode		= string.Empty;
		public string	DBProvider		= string.Empty;
		
		// attributi del nodo <ObjectsSelection>
		public bool		AllTables		= true;
		public bool		AllColumns		= false;
		public bool		WhereClause		= false;

		// attributi del nodo <MandatoryColumns>
		public bool		ColTBCreated	= true;
		public bool		ColTBModified	= false;

		public bool		ExecuteScript	= false;
		public string	Script			= string.Empty;

		// array per memorizzare tutto il contenuto del nodo <ExportedObjects>
		public List<ApplicationNode> ExportedObjectsArray = null;

		private DefaultSelections selections = null;

		private Diagnostic configurationInfoDiagnostic = new Diagnostic("ConfigurationInfo");
		public	Diagnostic ConfigurationInfoDiagnostic	{ get { return configurationInfoDiagnostic; } }

		/// <summary>
		/// Constructor
		/// </summary>
		//-----------------------------------------------------------------------
		public ConfigurationInfo()
		{
		}

		# region P A R S E
		/// <summary>
		/// Legge il file contenente le selezioni memorizzate
		/// </summary>
		/// <param name="file">path del file</param>
		/// <returns>successo della funzione</returns>
		//-----------------------------------------------------------------------
		public bool Parse(string file)
		{
			if (file.Length == 0)
			{
				configurationInfoDiagnostic.SetError(DataManagerEngineStrings.FilePathIsEmpty);
				return false;
			}

			FilePath = file;
			
			XmlDocument document = new XmlDocument();

			try
			{
                document = PathFinder.PathFinderInstance.FileSystemManager.LoadXmlDocument(document, FilePath);

				
				XmlElement root = document.DocumentElement;
				if (root == null)
				{
					configurationInfoDiagnostic.SetError(DataManagerEngineStrings.ErrorParsingFile);
					return false;
				}

				// controllo che sia un file con il tag <DefaultExportSelections> altrimenti non lo parso
				if (root.Name != ConfigurationInfoXml.Element.DefaultExportSelections)
				{
					configurationInfoDiagnostic.SetError(DataManagerEngineStrings.SelectedFileIsWrong);
					return false;
				}

				// Nodo <Parameters>
				if (!ParseParameters(root))
					return false;

				// Nodo <ExportedObjects>
				if (!ParseExportedObjects(root))
					return false;
			}
			catch(XmlException xmlExc)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.ParseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, xmlExc.Message)
					);
				return false;
			}
			catch(Exception exc)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.ParseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, exc.Message)
					);
				return false;
			}
			
			return true;
		}
		# endregion

		# region ParseParameters (nodo Parameters e figli)
		/// <summary>
		/// Parse del nodo <Parameters> e relativi sotto nodi 
		/// (Configuration, CountryCode, DBProvider, ObjectsSelection, MandatoryColumns, Script)
		/// </summary>
		/// <param name="root">root element</param>
		/// <returns>successo della funzione</returns>
		//----------------------------------------------------------------------------
		private bool ParseParameters(XmlElement root)
		{
			if (root == null)
			{
				configurationInfoDiagnostic.SetError(string.Format(DataManagerEngineStrings.ErrorParsingTag, ConfigurationInfoXml.Element.Parameters));
				return false;
			}

			try
			{
				// Nodo <Configuration>
				XmlNodeList configurationElem = root.GetElementsByTagName(ConfigurationInfoXml.Element.Configuration);
				if (configurationElem != null && configurationElem.Count == 1)
				{
					XmlElement elem = (XmlElement)configurationElem[0];
					Configuration = elem.GetAttribute(ConfigurationInfoXml.Attribute.Value);
				}

				// Nodo <CountryCode>
				XmlNodeList countryCodeElem = root.GetElementsByTagName(ConfigurationInfoXml.Element.CountryCode);
				if (countryCodeElem != null && countryCodeElem.Count == 1)
				{
					XmlElement elem = (XmlElement)countryCodeElem[0];
					CountryCode = elem.GetAttribute(ConfigurationInfoXml.Attribute.Value);
				}

				// Nodo <DBProvider>
				XmlNodeList dbProviderElem = root.GetElementsByTagName(ConfigurationInfoXml.Element.DBProvider);
				if (dbProviderElem != null && dbProviderElem.Count == 1)
				{
					XmlElement elem = (XmlElement)dbProviderElem[0];
					DBProvider = elem.GetAttribute(ConfigurationInfoXml.Attribute.Value);
				}

				// Nodo <ObjectsSelection>
				XmlNodeList objectsSelectionElem = root.GetElementsByTagName(ConfigurationInfoXml.Element.ObjectsSelection);
				if (objectsSelectionElem != null || objectsSelectionElem.Count > 0)
				{				
					XmlElement elem = (XmlElement)objectsSelectionElem[0];
					AllTables	= Convert.ToBoolean(elem.GetAttribute(ConfigurationInfoXml.Attribute.AllTables));
					AllColumns	= Convert.ToBoolean(elem.GetAttribute(ConfigurationInfoXml.Attribute.AllColumns));
					WhereClause	= Convert.ToBoolean(elem.GetAttribute(ConfigurationInfoXml.Attribute.WhereClause));
				}

				// Nodo <MandatoryColumns>
				XmlNodeList mandatoryColsElem = root.GetElementsByTagName(ConfigurationInfoXml.Element.MandatoryColumns);
				if (mandatoryColsElem != null || mandatoryColsElem.Count > 0)
				{
					XmlElement elem = (XmlElement)mandatoryColsElem[0];
					ColTBCreated	= Convert.ToBoolean(elem.GetAttribute(ConfigurationInfoXml.Attribute.ColTBCreated));
					ColTBModified	= Convert.ToBoolean(elem.GetAttribute(ConfigurationInfoXml.Attribute.ColTBModified));
				}

				// Nodo <Script>
				XmlNodeList scriptElem = root.GetElementsByTagName(ConfigurationInfoXml.Element.Script);
				if (scriptElem != null && scriptElem.Count == 1)
				{
					XmlNode node = scriptElem[0];
					Script = (node != null) ? node.InnerText : string.Empty;
					ExecuteScript = Convert.ToBoolean(((XmlElement)node).GetAttribute(ConfigurationInfoXml.Attribute.Execute));
				}
			}
			catch(XmlException xe)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.ParseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, xe.Message)
					);
				return false;
			}

			return true;
		}
		# endregion

		# region ParseExportedObjects (nodo ExportedObjects e figli)
		/// <summary>
		/// Parse del nodo <ExportedObjects> e relativi sotto nodi (Application, Module, Table, Column)
		/// </summary>
		/// <param name="root">root element</param>
		/// <returns>successo della funzione</returns>
		//----------------------------------------------------------------------------
		private bool ParseExportedObjects(XmlElement root)
		{
			if (root == null)
			{
				configurationInfoDiagnostic.SetError(string.Format(DataManagerEngineStrings.ErrorParsingTag, ConfigurationInfoXml.Element.ExportedObjects));
				return false;
			}

			if (ExportedObjectsArray == null)
				ExportedObjectsArray = new List<ApplicationNode>();

			try
			{
				//cerco il tag <Application>
				XmlNodeList applicationElem = root.GetElementsByTagName(ConfigurationInfoXml.Element.Application);
				if (applicationElem != null && applicationElem.Count > 0)
				{
					// per ogni nodo 
					foreach (XmlElement appElem in applicationElem)
					{
						ApplicationNode appNode = 
							new ApplicationNode(appElem.GetAttribute(ConfigurationInfoXml.Attribute.Name));
				
						//cerco il tag <Module>
						XmlNodeList moduleElements = appElem.GetElementsByTagName(ConfigurationInfoXml.Element.Module);
						// richiamo il parse dei nodi <Module>
						ParseSingleModule(moduleElements, appNode);

						ExportedObjectsArray.Add(appNode);
					}
				}
			}
			catch (XmlException xe)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.ParseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, xe.Message)
					);
				return false;
			}

			return true;
		}
		# endregion

		# region ParseSingleModule, ParseSingleTable, ParseSingleColumn
		/// <summary>
		/// Parsa il nodo di tipo <Module>
		/// </summary>
		/// <param name="moduleElements">array di elementi di tipo Modulo</param>
		/// <param name="appNode">nodo padre di tipo Application</param>
		//----------------------------------------------------------------------------
		private void ParseSingleModule(XmlNodeList moduleElements, ApplicationNode appNode)
		{
			if (moduleElements == null)
				return;

			if (appNode.Modules == null)
				appNode.Modules = new List<ModuleNode>();

			try
			{
				// per ogni modulo
				foreach (XmlElement modElem in moduleElements)
				{
					ModuleNode modNode = new ModuleNode(modElem.GetAttribute(ConfigurationInfoXml.Attribute.Name));

					//cerco il tag <Table>
					XmlNodeList tableElements = modElem.GetElementsByTagName(ConfigurationInfoXml.Element.Table);
					// richiamo il parse dei nodi <Table>
					ParseSingleTable(tableElements, modNode);

					appNode.Modules.Add(modNode);
				}
			}
			catch (XmlException xe)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.ParseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, xe.Message)
					);
			}
		}

		/// <summary>
		/// Parsa il nodo di tipo <Table>
		/// </summary>
		/// <param name="tableElements">array di elementi di tipo Table</param>
		/// <param name="modNode">nodo padre di tipo Module</param>
		//----------------------------------------------------------------------------
		private void ParseSingleTable(XmlNodeList tableElements, ModuleNode modNode)
		{
			if (tableElements == null)
				return;

			if (modNode.Tables == null)
				modNode.Tables = new List<TableNode>();

			try
			{
				// per ogni tabella
				foreach (XmlElement tableElem in tableElements)
				{
					TableNode tableNode = new TableNode
						(
						tableElem.GetAttribute(ConfigurationInfoXml.Attribute.Name),
						tableElem.GetAttribute(ConfigurationInfoXml.Attribute.WhereClause)
						);

					//cerco il tag <Column>
					XmlNodeList columnElements = tableElem.GetElementsByTagName(ConfigurationInfoXml.Element.Column);
					// richiamo il parse dei nodi <Column>
					ParseSingleColumn(columnElements, tableNode);

					modNode.Tables.Add(tableNode);
				}
			}
			catch (XmlException xe)
			{
				configurationInfoDiagnostic.Set
							(
							DiagnosticType.Error,
							string.Format(DataManagerEngineStrings.ParseError, FilePath),
							new ExtendedInfo(DataManagerEngineStrings.LblError, xe.Message)
							);
			}
		}

		/// <summary>
		/// Parsa il nodo di tipo <Column>
		/// </summary>
		/// <param name="columnElements">array di elementi di tipo Column</param>
		/// <param name="tableNode">nodo padre di tipo Table</param>
		//----------------------------------------------------------------------------
		private void ParseSingleColumn(XmlNodeList columnElements, TableNode tableNode)
		{
			if (columnElements == null)
				return;

			if (tableNode.Columns == null)
				tableNode.Columns = new List<string>();

			// per ogni colonna
			foreach (XmlElement columnElem in columnElements)
			{
				tableNode.Columns.Add(columnElem.GetAttribute(ConfigurationInfoXml.Attribute.Name));

				if (ColTBCreated)
				{
					tableNode.Columns.Add(DatabaseLayerConsts.TBCreatedColNameForSql);
					tableNode.Columns.Add(DatabaseLayerConsts.TBCreatedIDColNameForSql);
				}

				if (ColTBModified)
				{
					tableNode.Columns.Add(DatabaseLayerConsts.TBModifiedColNameForSql);
					tableNode.Columns.Add(DatabaseLayerConsts.TBModifiedIDColNameForSql);
				}
			}
		}
		# endregion

		# region U N P A R S E
		/// <summary>
		/// Scrive il file di configurazione contenente le selezioni effettuate nel wizard
		/// </summary>
		/// <param name="file">path del file in cui salvare le info</param>
		/// <param name="selections">puntatore alle selezioni (DefaultSelections)</param>
		/// <returns>successo della funzione</returns>
		//----------------------------------------------------------------------------
		public bool Unparse(string file, DefaultSelections selections)
		{
			if (file.Length == 0)
			{
				configurationInfoDiagnostic.SetError(DataManagerEngineStrings.FilePathIsEmpty);
				return false;
			}

			FilePath = file;
			this.selections = selections;
			XmlDocument document;
			
			try
			{
				document = new XmlDocument();
				XmlDeclaration xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
				document.AppendChild(xmlDeclaration);
			
				//root del documento <DefaultExportSelections>
				XmlElement root = document.CreateElement(ConfigurationInfoXml.Element.DefaultExportSelections);
				document.AppendChild(root);

				// Scrivo il Nodo <Parameters>
				if (!UnparseParameters(root, document))
					return false;

				// Scrivo il Nodo <ExportedObjects>
				if (!UnparseExportedObjects(root, document))
					return false;
			}
			catch(XmlException xe)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.UnparseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, xe.Message)
					);
				return false;
			}

			try
			{
				FileInfo fi = new FileInfo(FilePath);

				if (fi.Exists && ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly))
					fi.Attributes -= FileAttributes.ReadOnly;

				fi.Directory.Create();

				var fileWriter = new StreamWriter(File.Create(FilePath));
				document.Save(fileWriter);
			}
			catch(Exception e)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.UnparseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, e.Message)
					);
				return false;
			}

			return true;
		}
		# endregion

		# region UnparseParameters (scrittura nodo <Parameters>)
		/// <summary>
		/// Scrive il nodo <Parameters> ed i sotto nodi relativi
		/// </summary>
		/// <param name="root">elemento root</param>
		/// <param name="document">puntatore al dom</param>
		//----------------------------------------------------------------------------
		private bool UnparseParameters(XmlElement root, XmlDocument document)
		{
			try
			{
				// creo il nodo <Parameters>
				XmlElement parametersNode = document.CreateElement(ConfigurationInfoXml.Element.Parameters);
				root.AppendChild(parametersNode);

				// creo il nodo <Configuration>
				XmlElement configurationNode = document.CreateElement(ConfigurationInfoXml.Element.Configuration);
				XmlAttribute attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Value);
				attribute.Value = selections.SelectedConfiguration;
				configurationNode.Attributes.Append(attribute);
				parametersNode.AppendChild(configurationNode);

				// creo il nodo <CountryCode>
				XmlElement countryNode = document.CreateElement(ConfigurationInfoXml.Element.CountryCode);
				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Value);
				attribute.Value = selections.SelectedIsoState;
				countryNode.Attributes.Append(attribute);
				parametersNode.AppendChild(countryNode);

				// creo il nodo <CountryCode>
				XmlElement dbProviderNode = document.CreateElement(ConfigurationInfoXml.Element.DBProvider);
				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Value);
				attribute.Value = selections.ContextInfo.DbType.ToString();
				dbProviderNode.Attributes.Append(attribute);
				parametersNode.AppendChild(dbProviderNode);

				// creo il nodo <ObjectsSelection>
				XmlElement objectsNode = document.CreateElement(ConfigurationInfoXml.Element.ObjectsSelection);
				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.AllTables);
				attribute.Value = selections.ExportSel.AllTables.ToString();
				objectsNode.Attributes.Append(attribute);

				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.AllColumns);
				attribute.Value = Convert.ToString(!selections.ExportSel.SelectColumns);
				objectsNode.Attributes.Append(attribute);

				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.WhereClause);
				attribute.Value = selections.ExportSel.WriteQuery.ToString();
				objectsNode.Attributes.Append(attribute);

				parametersNode.AppendChild(objectsNode);

				// creo il nodo <MandatoryColumns>
				XmlElement mandatoryNode = document.CreateElement(ConfigurationInfoXml.Element.MandatoryColumns);
				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.ColTBCreated);
				attribute.Value = selections.ExportSel.ExportTBCreated.ToString();
				mandatoryNode.Attributes.Append(attribute);

				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.ColTBModified);
				attribute.Value = selections.ExportSel.ExportTBModified.ToString();
				mandatoryNode.Attributes.Append(attribute);
				parametersNode.AppendChild(mandatoryNode);

				// creo il nodo <Script>
				XmlElement scriptNode = document.CreateElement(ConfigurationInfoXml.Element.Script);
				attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Execute);
				attribute.Value = selections.ExportSel.ExecuteScriptTextBeforeExport.ToString();
				scriptNode.Attributes.Append(attribute);
				scriptNode.InnerText = selections.ExportSel.ScriptTextBeforeExport;
				parametersNode.AppendChild(scriptNode);
			}
			catch (XmlException xe)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.UnparseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, xe.Message)
					);
				return false;
			}

			return true;
		}
		# endregion

		# region UnparseExportedObjects (scrittura nodo <ExportedObjects>)
		/// <summary>
		/// Scrive il nodo <ExportedObjects> ed i sotto nodi relativi
		/// </summary>
		/// <param name="root">elemento root</param>
		/// <param name="document">puntatore al dom</param>
		//----------------------------------------------------------------------------
		private bool UnparseExportedObjects(XmlElement root, XmlDocument document)
		{
			// creo il nodo <ExportedObjects>
			XmlElement expObjectsNode = document.CreateElement(ConfigurationInfoXml.Element.ExportedObjects);
			root.AppendChild(expObjectsNode);

			// se porto tutte le tabelle, non scrivo where clause e non esporto le singole colonne non aggiungo altri nodi
			if (selections.ExportSel.AllTables && !selections.ExportSel.WriteQuery && !selections.ExportSel.SelectColumns)
				return true;

			XmlAttribute attribute;

			try
			{
				foreach (CatalogTableEntry entry in selections.ExportSel.Catalog.TblDBList)
				{
					// per ogni tabella selezionata
					if (entry.Selected)
					{
						XmlNode nodeApp = document.SelectSingleNode(string.Format("//Application[@name='{0}']", entry.Application));
						// se il nodo dell'applicazione corrente non esiste lo creo
						if (nodeApp == null)
						{
							nodeApp = document.CreateElement(ConfigurationInfoXml.Element.Application);
							attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Name);
							attribute.Value = entry.Application;
							nodeApp.Attributes.Append(attribute);
							expObjectsNode.AppendChild(nodeApp);
						}

						XmlNode nodeModule = document.SelectSingleNode(string.Format("//Module[@name='{0}']", entry.Module));
						// se il nodo del modulo corrente non esiste lo creo
						if (nodeModule == null)
						{
							nodeModule = document.CreateElement(ConfigurationInfoXml.Element.Module);
							attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Name);
							attribute.Value = entry.Module;
							nodeModule.Attributes.Append(attribute);
							nodeApp.AppendChild(nodeModule);
						}

						XmlNode nodeTable = document.SelectSingleNode(string.Format("//Table[@name='{0}']", entry.TableName));
						// se il nodo della tabella corrente non esiste lo creo
						if (nodeTable == null)
						{
							nodeTable = document.CreateElement(ConfigurationInfoXml.Element.Table);
							attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Name);
							attribute.Value = entry.TableName;
							nodeTable.Attributes.Append(attribute);
							attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.WhereClause);
							attribute.Value = entry.WhereClause;
							nodeTable.Attributes.Append(attribute);
							nodeModule.AppendChild(nodeTable);
						}

						// se ho scelto di selezionare anche le colonne allora creo un nodo per ogni colonna selezionata
						// (per le colonne TBCreated e TBModified dipende dalle selezioni effettuate)
						if (selections.ExportSel.SelectColumns)
						{
							foreach (string column in entry.SelectedColumnsList)
							{
								// le colonne TBCreated e TBModified le skippo xchè gestite con il tag <MandatoryColumns>
								if (string.Compare(column, DatabaseLayerConsts.TBCreatedColNameForSql, StringComparison.OrdinalIgnoreCase) == 0 ||
									string.Compare(column, DatabaseLayerConsts.TBModifiedColNameForSql, StringComparison.OrdinalIgnoreCase) == 0 ||
									string.Compare(column, DatabaseLayerConsts.TBCreatedIDColNameForSql, StringComparison.OrdinalIgnoreCase) == 0 ||
									string.Compare(column, DatabaseLayerConsts.TBModifiedIDColNameForSql, StringComparison.OrdinalIgnoreCase) == 0)
									continue;

								XmlNode nodeColumn = document.CreateElement(ConfigurationInfoXml.Element.Column);
								attribute = document.CreateAttribute(ConfigurationInfoXml.Attribute.Name);
								attribute.Value = column;
								nodeColumn.Attributes.Append(attribute);
								nodeTable.AppendChild(nodeColumn);
							}
						}
					}
				}
			}
			catch (XmlException xe)
			{
				configurationInfoDiagnostic.Set
					(
					DiagnosticType.Error,
					string.Format(DataManagerEngineStrings.UnparseError, FilePath),
					new ExtendedInfo(DataManagerEngineStrings.LblError, xe.Message)
					);
				return false;
			}

			return true;
		}
		# endregion
		
		# region Funzioni di utilità per rintracciare le info nei nodi
		//----------------------------------------------------------------------------
		public bool ExistApplicationInConfigInfo(string application)
		{
			if (ExportedObjectsArray == null || ExportedObjectsArray.Count == 0)
				return false;

			foreach (ApplicationNode appNode in ExportedObjectsArray)
			{
				if (string.Compare(appNode.Name, application, StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}

			return false;
		}

		//----------------------------------------------------------------------------
		public ApplicationNode GetApplicationInConfigInfo(string application)
		{
			if (ExportedObjectsArray == null || ExportedObjectsArray.Count == 0)
				return null;

			foreach (ApplicationNode appNode in ExportedObjectsArray)
			{
				if (string.Compare(appNode.Name, application, StringComparison.OrdinalIgnoreCase) == 0)
					return appNode;
			}

			return null;
		}

		//----------------------------------------------------------------------------
		public bool ExistModuleInConfigInfo(string application, string module)
		{
			if (ExportedObjectsArray == null || ExportedObjectsArray.Count == 0)
				return false;

			foreach (ApplicationNode appNode in ExportedObjectsArray)
			{
				if (string.Compare(appNode.Name, application, StringComparison.OrdinalIgnoreCase) != 0)
					continue;

				foreach (ModuleNode modNode in appNode.Modules)
				{
					if (string.Compare(modNode.Name, module, StringComparison.OrdinalIgnoreCase) == 0)
						return true;
				}
			}

			return false;
		}

		//----------------------------------------------------------------------------
		public ModuleNode GetModuleInConfigInfo(ApplicationNode appNode, string module)
		{
			if (appNode == null)
				return null;

			foreach (ModuleNode modNode in appNode.Modules)
			{
				if (string.Compare(modNode.Name, module, StringComparison.OrdinalIgnoreCase) == 0)
					return modNode;
			}

			return null;
		}

		//----------------------------------------------------------------------------
		public bool ExistTableInConfigInfo(string application, string module, string table)
		{
			if (ExportedObjectsArray == null || ExportedObjectsArray.Count == 0)
				return false;

			foreach (ApplicationNode appNode in ExportedObjectsArray)
			{
				if (string.Compare(appNode.Name, application, StringComparison.OrdinalIgnoreCase) != 0)
					continue;

				foreach (ModuleNode modNode in appNode.Modules)
				{
					if (string.Compare(modNode.Name, module, StringComparison.OrdinalIgnoreCase) != 0)
						continue;

					foreach (TableNode tblNode in modNode.Tables)
					{
						if (string.Compare(tblNode.Name, table, StringComparison.OrdinalIgnoreCase) == 0)
							return true;
					}
				}
			}

			return false;
		}

		//----------------------------------------------------------------------------
		public TableNode GetTableInConfigInfo(ModuleNode modNode, string table)
		{
			if (modNode == null)
				return null;

			foreach (TableNode tblNode in modNode.Tables)
			{
				if (string.Compare(tblNode.Name, table, StringComparison.OrdinalIgnoreCase) == 0)
					return tblNode;
			}

			return null;
		}

		//----------------------------------------------------------------------------
		public TableNode GetTableInConfigInfo(string application, string module, string table)
		{
			if (ExportedObjectsArray == null || ExportedObjectsArray.Count == 0)
				return null;

			foreach (ApplicationNode appNode in ExportedObjectsArray)
			{
				if (string.Compare(appNode.Name, application, StringComparison.OrdinalIgnoreCase) != 0)
					continue;

				foreach (ModuleNode modNode in appNode.Modules)
				{
					if (string.Compare(modNode.Name, module, StringComparison.OrdinalIgnoreCase) != 0)
						continue;

					foreach (TableNode tblNode in modNode.Tables)
					{
						if (string.Compare(tblNode.Name, table, StringComparison.OrdinalIgnoreCase) == 0)
							return tblNode;
					}
				}
			}

			return null;
		}
		# endregion
	}
}