using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.Applications
{
	/*
		Sintassi di ReferenceObjects.xml

		<?xml version="1.0" encoding="utf-8" ?> 
		<HotKeyLink>
			<Function type="long" namespace="TestApplication.TADataEntry.Clienti" localize="Ricerca dei Clienti">
					<Param name="Magazzino" type="long" localize="Codice" />
			</Function>
			<DbField name="Clienti.Codice">
	 		
	      //**************Estensione per esecuzione radarReport da hotlink		
	 		
			<DbFieldDescription name="TBBC_Customers.Surname" /> 
			<DbTable name="TBBC_Customers" /> 
-			<RadarReport name="TbBaseCourse.Masters.CustomersRadar">
				<Param name="Hkl_Selection" type="integer" localize="0 (Upper) / 1 (Lower)" /> 
				<Param name="Hkl_CodeValue" type="string" localize="code" /> 
				<Param name="Hkl_DescriptionValue" type="string" localize="description" /> 
			</RadarReport>
	
	       //***************
			<ComboBox>
				<Column length="10" when="<expr>" source="Clienti.Codice" />
				<Column source="Clienti.RagSoc"/>
				<Column when="<expr>" localize="inserito il" source="Clienti.DataInserimento" formatter="Date" />
				<Column loacalize="in sede" />
			<ComboBox>
		</HotKeyLink>

		namespace/name localize	namespace /nome/nome pubblico in lingua
		HotKeyLink			dichiarazione del singolo elemento
		Function			il contenuto di questa sezione descrive in dettaglio le parametrizzazioni e i filtri resi 
							disponibili dall’oggetto (vd. AskDialog di Woorm). La grammatica utilizzata è quella relativa ai 
		FunctionsObjects,	(vedi  progetto 1700).
		ComboBox			indica cosa deve essere visualizzato quando l’oggetto è disegnato in forma di combobox.
		Column				descrive il contenuto di una colonna e le sue caratteristiche grafiche. Di default (SelectAll)
		when				espressione che indica se il campo è da visualizzare o meno su una condizione. Opzionale
		length				indica il numero di caratteri da usare nella tendina. Opzionale.
		formatter			indica il formattatore specifico (di default viene preso quello associato al datatype)
		source				contiene il nome del campo di tabella che deve essere usato oppure una stringa. Opzionale.
		localize 			consente di definire una stringa da usare come prefisso al campo source (se esistente).
							Se non è in combinazione con l’ attributo source identifica una stringa letterale in lingua.
	 */


	/// <summary>
	/// Summary description for Functions.
	/// </summary>
	///=============================================================================
	public class ReferenceObjectsPrototype : FunctionPrototype
	{
		string dbFieldName = "";
		string dbFieldTableName = "";
		string dbTableName = "";
		string dbFieldDescriptionName = "";
		string radarReportName = "";
		List<Parameter> hotLinkParameters = new List<Parameter>(); 
        public bool IsDatafile = false;

		//-----------------------------------------------------------------------------
		public string DbFieldName				{ get { return dbFieldName; }}
		public string DbFieldTableName			{ get { return dbFieldTableName; }}
		public string DbTableName				{ get { return dbTableName; } }
		public string DbFieldDescriptionName	{ get { return dbFieldDescriptionName; } }
		public string RadarReportName			{ get { return radarReportName; } }
		public List<Parameter> HotLinkParameters { get { return hotLinkParameters; } }

		public override string Title
		{
			get
			{
				return
					moduleInfo == null
					? title
					: LocalizableXmlDocument.LoadXMLString
					(
						title,
						NameSolverStrings.ReferenceObjects,
						moduleInfo.DictionaryFilePath
					); 
			}
		}
		
		//-----------------------------------------------------------------------------
		public ReferenceObjectsPrototype
			(
			string		name, 
			string		localizedName,
			string		dbFieldTableName,
			string		dbFieldName,
			string		dbFieldDescriptionName,
			string		dbTableName,
			string		radarReportName,
			string		returnType,
            ParametersList prs,
            ParametersList hotLinkParameters,
			string		server,
			int		    port,
			string		service,
			string		serviceNamespace,
			IBaseModuleInfo	moduleInfo
			)
			:
			base(name, localizedName, returnType, String.Empty)
		{
            this.Server = server;
            this.Port = port;
            this.Service = service;
            this.ServiceNamespace = serviceNamespace;

            this.ModuleInfo = moduleInfo;
            this.Parameters = prs;

			this.dbFieldTableName = dbFieldTableName;
			this.dbFieldName = dbFieldName;
			this.dbFieldDescriptionName = dbFieldDescriptionName; 
			this.dbTableName = dbTableName;
			this.radarReportName = radarReportName;
			this.hotLinkParameters = hotLinkParameters;
		}
	}

	///=============================================================================
	public class ReferenceObjects
	{
		private ArrayList prototypes;
		private TbReportSession reportSession;

		//-----------------------------------------------------------------------------
		public TbReportSession	ReportSession	{ get { return reportSession; }}

		//-----------------------------------------------------------------------------
		public ReferenceObjects(TbReportSession session)
		{
			// è necessario inizializzare prima una sessione di lavoro.
			this.reportSession = session;
			if (session == null)
				throw (new Exception(ApplicationsStrings.ReferenceObjectsSessionError));

			prototypes = new ArrayList();
		}

		// elimina il nome della tabella se esiste perche il grid vuole solo il nome della colonna.
		//-----------------------------------------------------------------------------
		private string ColumnName(string name)
		{
			int pos = name.IndexOf('.');
			return pos >= 0 ? name.Substring(pos + 1) : name;
		}	

		// serve per poter localizzare i nomi di colonna nel titolo del grid
		//-----------------------------------------------------------------------------
		private string TableName(string name)
		{
			int pos = name.IndexOf('.');
			return pos >= 0 ? name.Substring(0, pos) : "";
		}	

		//-----------------------------------------------------------------------------
		public ReferenceObjectsPrototype LoadPrototypeFromXml(string name, int paramNo)
		{
			NameSpace ns = new NameSpace(name, NameSpaceObjectType.HotKeyLink);
			ModuleInfo mi = (ModuleInfo) reportSession.PathFinder.GetModuleInfo(ns);

			if  (mi == null)
				return null;

			// se il file delle funzioni esterne non esiste allora la funzione è indefinita
			string path = mi.GetReferenceObjectFileName(ns);
			if (!File.Exists(path))
				return null;

			// restituisce il dom già tradotto per i Tag o gli Attribute che sono localizzati
			LocalizableXmlDocument dom = new LocalizableXmlDocument(ns.Application, ns.Module, reportSession.PathFinder);
			dom.Load(path);

			// cerca con XPath solo le funzioni con un dato nome per poi selezionare quella con i parametri giusti
			XmlNode root = dom.DocumentElement;
			string xpath = string.Format
				(
				"/{0}/{1}[@{2}]",
				ReferenceObjectsXML.Element.HotKeyLink,
				ReferenceObjectsXML.Element.Function,
				ReferenceObjectsXML.Attribute.Namespace
				);

			// se non esiste la sezione allora il ReferenceObject è Undefined
			XmlNodeList functions = root.SelectNodes(xpath);
			if (functions == null) return null;

			foreach (XmlElement function in functions)
			{
				// controllo che il namespace sia quello giusto in modalità CaseInsensitive
				string namespaceAttribute = function.GetAttribute(ReferenceObjectsXML.Attribute.Namespace);
				if ((namespaceAttribute == null) || (string.Compare(namespaceAttribute, name, true, CultureInfo.InvariantCulture) != 0))
					continue;

				// se il numero di parametri non corrisponde allora cerca un'altra funzione con lo stesso nome
				XmlNodeList paramNodeList = function.SelectNodes(ReferenceObjectsXML.Element.Param);
				if ((paramNodeList == null) || (paramNodeList.Count != paramNo))
					continue;

                ParametersList parameters = new ParametersList();
                parameters.Parse(paramNodeList);

				// deve esistere la dichiarazione altrimenti io considero il referenceObject inesistente
				XmlElement dbField = (XmlElement)function.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.DbField);
				if (dbField == null)
					return null;

				string qualifiedColumnName = dbField.GetAttribute(ReferenceObjectsXML.Attribute.Name);

				//DbFieldDescription, DbTable e DbRadarReport

				string dbFieldDescriptionName = "";
				XmlElement dbFieldDescription = (XmlElement)function.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.DbFieldDescription);
				if (dbFieldDescription != null)
					dbFieldDescriptionName = dbFieldDescription.GetAttribute(ReferenceObjectsXML.Attribute.Name);
				
				string dbTableName = "";
				XmlElement dbTable = (XmlElement)function.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.DbTable);
				if (dbTable != null)
					dbTableName = dbTable.GetAttribute(ReferenceObjectsXML.Attribute.Name);

                //----
                string  datafile = string.Empty;
                XmlElement combo = (XmlElement)function.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.ComboBox);
                if (combo != null)
                {
                    datafile = combo.GetAttribute(ReferenceObjectsXML.Attribute.Datafile);
                }
                bool isDatafile = (datafile.Length > 0);
                //----

                ParametersList parametersHotLink = new ParametersList();
   				string radarReportName = "";
				XmlElement radarReport = (XmlElement)function.ParentNode.SelectSingleNode(ReferenceObjectsXML.Element.RadarReport);
				if (radarReport != null)
				{
					radarReportName = radarReport.GetAttribute(ReferenceObjectsXML.Attribute.Name);
					XmlNodeList paramNodeListHotLink = radarReport.SelectNodes(ReferenceObjectsXML.Element.Param);
					if ((paramNodeListHotLink == null) || (paramNodeListHotLink.Count != 3))
						continue;

					parametersHotLink.Parse(paramNodeListHotLink);
				}

                int port;
                if (!int.TryParse(function.GetAttribute(ReferenceObjectsXML.Attribute.Port), out port))
                    port = 80;
				
				// crea il nuovo prototipo e lo aggiunge all'elenco indicando anche in che dll si trova
				ReferenceObjectsPrototype fp = new ReferenceObjectsPrototype
					(
						ns,
						function.GetAttribute(ReferenceObjectsXML.Attribute.Localize),
						TableName(qualifiedColumnName),
						ColumnName(qualifiedColumnName),
						dbFieldDescriptionName,
						dbTableName,
						radarReportName,
						ObjectHelper.FromTBType(function.GetAttribute(ReferenceObjectsXML.Attribute.Type)),
						parameters,
						parametersHotLink,
						function.GetAttribute(ReferenceObjectsXML.Attribute.Server),
						port,
						function.GetAttribute(ReferenceObjectsXML.Attribute.Service),
						function.GetAttribute(ReferenceObjectsXML.Attribute.ServiceNamespace),
						mi
					);
                fp.IsDatafile = isDatafile;

				prototypes.Add(fp);
				return fp;
			}

			// la funzione non è dichiarata
			return null;
		}

		//-----------------------------------------------------------------------------
		public ReferenceObjectsPrototype GetPrototype(string name, int paramNo)
		{
			foreach (ReferenceObjectsPrototype fp in prototypes)
                if (name.CompareNoCase(fp.FullName) && fp.NrParameters == paramNo)
					return fp;

			return LoadPrototypeFromXml(name, paramNo);
		}
	}
}
