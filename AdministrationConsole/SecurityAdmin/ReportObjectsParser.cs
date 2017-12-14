using System;
using System.Collections;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin
{

	//=========================================================================
	public class ReportObjectsParser
	{
		/// <summary>
		/// Path e node del file
		/// </summary>
		private string		filePath;
		/// <summary>
		/// Indica se ci sono stati degli errori durante il Parsing del file
		/// </summary>
		private	bool		valid;
		/// <summary>
		/// NameSpace del modulo padre
		/// </summary>
		private NameSpace	parentNameSpace;
		/// <summary>
		/// Array che conterrà l'elenco degli oggetti di tipo Report ricavati
		/// dal parsing del file
		/// </summary>
		private ArrayList	reportArray;
		/// <summary>
		/// PathFinder
		/// </summary>
		private PathFinder	pathFinder;
		/// <summary>
		/// Diagnostic degli eventuali errori di Parsing
		/// </summary>
		private Diagnostic	diagnostic	= null;

		/// <summary>
		/// Get del filePath
		/// </summary>
		public	string		FilePath		{ get { return filePath; } }
		/// <summary>
		/// Get del valid
		/// </summary>
		public	bool		Valid			{ get { return valid; } }
		/// <summary>
		/// Get del parentNameSpace
		/// </summary>
		public  NameSpace	ParentNameSpace	{ get { return parentNameSpace; } }
		/// <summary>
		/// Get del pathFinder
		/// </summary>
		public  PathFinder	PathFinder		{ get { return pathFinder; } }
		/// <summary>
		/// Get del diagnostic
		/// </summary>
		public	Diagnostic	Diagnostic		{ get { return diagnostic; }}
		/// <summary>
		/// Get dell'Array dei Report
		/// </summary>
		public ArrayList	Reports			{ get { return reportArray; }  }
		
		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="aFilePath">path del file DatabaseObjects del modulo</param>
		public ReportObjectsParser(string aFilePath, NameSpace aParentNameSpace, PathFinder aPathFinder)
		{
			diagnostic	= new Diagnostic("DiagnosticReportObjectParser");
			if (aFilePath == null || aFilePath.Length == 0)
			{
				SetError(Strings.FileNotFound);
			}

			filePath			= aFilePath;
			valid				= true;
			parentNameSpace		= aParentNameSpace;
			pathFinder			= aPathFinder;
			Parse();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Legge il file e crea gli array dei report
		/// </summary>
		/// <returns>true se la lettura ha avuto successo</returns>
		
		public bool Parse()
		{
			string nameSpace = "";
			string localize  = "";

			if (reportArray == null)
				reportArray = new ArrayList();
			if	(
				!File.Exists(filePath)		|| 
				parentNameSpace == null	
				)
				return false;

			LocalizableXmlDocument reportObjectsDocument = 
				new LocalizableXmlDocument
				(
				parentNameSpace.Application,
				parentNameSpace.Module,
				pathFinder
				);

			try
			{
				//leggo il file
				if (!File.Exists(filePath))
					return false;

				
				reportObjectsDocument.Load(filePath);
				// cerca con XPath solo le funzioni con un dato nome per poi selezionare quella con i parametri giusti
				XmlNode root = reportObjectsDocument.DocumentElement;

				if (root != null )
				{
					string xpath = string.Format
						(
						"/{0}/{1}",
						ReportObjectsXML.Element.ReportObjects,
						ReportObjectsXML.Element.Reports
						);

					XmlNodeList reports = root.SelectNodes(xpath);
					if (reports == null) return false;

					string defaultReport	= ((XmlElement)reports[0]).GetAttributeNode(ReportObjectsXML.Attribute.DefaultReport).Value;
					NameSpace aDefaultReport = new NameSpace(defaultReport, NameSpaceObjectType.Report);
					
					xpath = string.Format
							(
							"/{0}/{1}/{2}",
							ReportObjectsXML.Element.ReportObjects,
							ReportObjectsXML.Element.Reports,
							ReportObjectsXML.Element.Report
							);
					reports = root.SelectNodes(xpath);
					if (reports == null) 
					{
						SetWarning(Strings.NoReports);
						return false;
					}

					foreach (XmlElement report in reports)
					{
						nameSpace	= report.GetAttributeNode( ReportObjectsXML.Attribute.NameSpace).Value;
						localize	= report.GetAttributeNode( ReferenceObjectsXML.Attribute.Localize).Value;
						
						NameSpace aNameSpace	= new NameSpace(nameSpace, NameSpaceObjectType.Report);
						Report aFunction		= new Report(aNameSpace,localize);
						reportArray.Add (aFunction);
					}
				}
			}
			catch(XmlException err)
			{
				SetError(err.Message, err.LineNumber);
				valid = false;
				return false;
			}
			catch(Exception e)
			{
				SetError(e.Message);
				valid = false;
				return false;
			}

			return true;
		}
		
		//---------------------------------------------------------------------
		/// <summary>
		/// Inserisce l'errore nel Diagnostic
		/// </summary>
		/// <param name="explain"></param>
		public void SetError(string explain)
		{   
			diagnostic.Set(DiagnosticType.Error, explain) ;
		}	

		//---------------------------------------------------------------------
		/// <summary>
		/// Inserisce l'errore nel Diagnostic indicando anche la riga del Log
		/// </summary>
		/// <param name="explain"></param>
		/// <param name="line"></param>
		public void SetError(string explain, long line)
		{   
			ExtendedInfo info = new ExtendedInfo();

			info.Add("Line", line);
			info.Add("FilePath", filePath);

			diagnostic.Set(DiagnosticType.Error, this.PathFinder.GetModuleInfo(parentNameSpace).Name + "   " + explain, info);
		}	

		//---------------------------------------------------------------------
		/// <summary>
		/// Cancella dal Diagnostic tutti i messaggi con tipo = Error
		/// </summary>
		public void ClearError ()
		{
			diagnostic.Clear(DiagnosticType.Error);
		}
		//---------------------------------------------------------------------
		/// <summary>
		/// Cancella dal Diagnostic tutti i messaggi con tipo = Warning
		/// </summary>
		/// <param name="explain"></param>
		public void SetWarning(string explain)
		{   
			diagnostic.Set(DiagnosticType.Warning, explain);
		}	
	}
	//=========================================================================
	/// <summary>
	/// Classe contenente le informazioni relative agli oggetti Report presenti
	/// nel file .xml
	/// </summary>
	public class Report
	{
		/// <summary>
		/// Valore dell'attributo nameSpace
		/// </summary>
		protected NameSpace nameSpace;
		/// <summary>
		/// Valore dell'attributo localize
		/// </summary>
		protected string	localize;

		/// <summary>
		/// Get del valore dell'attributo NameSpace
		/// </summary>
		public NameSpace	NameSpace	{ get { return nameSpace; } }
		/// <summary>
		/// Get del valore dell'attributo localize
		/// </summary>
		public string		Localize	{ get { return localize ; } }

		//---------------------------------------------------------------------
		/// <summary>
		/// Costruttore valorizza i Data Menber
		/// </summary>
		/// <param name="aNameSpace"></param>
		/// <param name="alocalize"></param>
		public Report(NameSpace aNameSpace, string alocalize)
		{
			nameSpace	= aNameSpace;
			localize	= alocalize;
		}
	}
	//=========================================================================
}
