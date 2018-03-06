using System;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using Microarea.Common.CoreTypes;
using Microarea.Common.Applications;
using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using System.Text;

namespace Microarea.RSWeb.WoormEngine
{
	/// <summary>
	/// Descrizione di riepilogo per RdeWriter.
	/// </summary>
	//============================================================================
	public class RdeWriter : IDisposable
	{
		public enum Command
		{
			// DisplayTable
			NextLine,
			TitleLine, CustomTitleLine,
            Interline,

			// RectField
			LowerInput,
			UpperInput,

			// general commands
			Message,
			TotalPages,
			Alias
		};

		protected XmlDocument		output = null;
		protected XmlElement		currentElement = null;
		protected Report			report;
		protected string			filename;
		private	  int				pageNo = 1;
		private	  string			pageFilename = string.Empty;

		//---------------------------------------------------------------------------
		virtual public	string	Release { get { return "1"; }}
		//---------------------------------------------------------------------------
		public int PageNumber { get { return pageNo; } set { pageNo = value; } }

		//---------------------------------------------------------------------------
		public RdeWriter(Report report) 
		{
			this.filename	= report.ReportName;
			this.report		= report;
		}

		//------------------------------------------------------------------------------
		virtual public void Close(string file)
		{
            string dirPath = Path.GetDirectoryName(file);
            PathFinder.PathFinderInstance.CreateFolder(dirPath, false); 

            if (output != null) 
                PathFinder.PathFinderInstance.SaveTextFileFromXml(file, output);

		   Dispose();
		}

       
        //------------------------------------------------------------------------------
        public void Dispose()
		{
            output = null;
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

		//---------------------------------------------------------------------------
		private string Filename 
		{
			get	{ 
                //return PathFunctions.WoormTempFilename(report.SessionID, report.UniqueID, filename, pageNo);
                if (pageFilename == string.Empty)
                {
                    pageFilename = PathFunctions.WoormTempFilePath(report.SessionID, report.UniqueID) +
                                    NameSolverStrings.Directoryseparetor +
                                    Path.GetFileNameWithoutExtension(filename);
                }
                return pageFilename + pageNo.ToString() + ".xml";
            }
		}

		//---------------------------------------------------------------------------
		private string InfoFilename 
		{
			get	{ 
                //return PathFunctions.WoormTempFilename(report.SessionID, report.UniqueID, filename); 
                if (pageFilename == string.Empty)
                {
                    pageFilename = PathFunctions.WoormTempFilePath(report.SessionID, report.UniqueID) +
                                    NameSolverStrings.Directoryseparetor +
                                    Path.GetFileNameWithoutExtension(filename);
                }
                return pageFilename + ".xml";
            }
		}

		//---------------------------------------------------------------------------
		private string TotPageFilename
		{
			get
			{
				//return PathFunctions.WoormTempFilename(report.SessionID, report.UniqueID, filename); 
				if (pageFilename == string.Empty)
				{
					pageFilename = PathFunctions.WoormTempFilePath(report.SessionID,report.UniqueID) +
                                    NameSolverStrings.Directoryseparetor +
                                    Path.GetFileNameWithoutExtension(filename);
				}
				return pageFilename + "Pages.xml";
			}
		}

		//---------------------------------------------------------------------------
		virtual protected void XmlHeader() 
		{
			output.AppendChild(output.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\""));

			XmlElement reportNode = output.CreateElement(RdeWriterTokens.Element.Report);
			reportNode.SetAttribute(RdeWriterTokens.Attribute.Release, Release);
			output.AppendChild(reportNode);
		}

		//------------------------------------------------------------------------------
		protected bool Open()
		{
			// permette di chiamare sempre la open anche se già aperto
			if (output != null) return true;

			bool ok = true;
			try
			{
				output = new XmlDocument();
				XmlHeader();
			}
			catch (IOException)
			{
				output = null;
				return false;
			}
			return ok;
		}

		//---------------------------------------------------------------------------
		private bool WriteTotalPages()		
		{ 
			Tag(Command.TotalPages.ToString());
			Attribute(RdeWriterTokens.Attribute.Number, pageNo);

			return true; 
		}

		// scrive nella reportSession i tipi dei dati linkati al viewer per permettere di formattarli
		// e di definire i corretti stili dei font e degli allineamenti.
		//---------------------------------------------------------------------------
		private bool WriteFieldsType()		
		{
			//Nel caso di report eseguito da magic link, la symbol table da usare e' quella "Fields" perche' non vengono eseguite le 
			//AskDialog come Easylook, e quindi la symtable "AskDialogState" risulta vuota
            //TODO RSWEB verificare check su enginetype
			FieldSymbolTable symTable = (report.EngineType == EngineType.PDFSharp_OfficePDF) ? 
                                            report.SymTable.Fields : 
                                            report.SymTable.AskDialogState;
			
			foreach (Field field in symTable)
			{
				if (field.InternalId <= 0 || field.IsSubTotal) continue;

				Tag(Command.Alias.ToString());
				Attribute(RdeWriterTokens.Attribute.Number, field.InternalId);
				Attribute(RdeWriterTokens.Attribute.Type, field.DataType);
				
				if (field.IsArray)
					Attribute(RdeWriterTokens.Attribute.BaseType, field.ArrayBaseType);
				
				Attribute(RdeWriterTokens.Attribute.WoormType, field.WoormType);

				// scrivo anche informazioni della symbol table per la valorizzazione
				// dinamica degli oggetti nel WoormWiewer che dipendono da espressioni
				Attribute(RdeWriterTokens.Attribute.Name, field.Name);

				if (field.IsArray)
					Attribute(RdeWriterTokens.Attribute.Value,((DataArray)field.Data).ConvertToJson());
				else
				    Attribute(RdeWriterTokens.Attribute.Value, SoapTypes.To(field.Data));

                /* TODO RSWEB CollateCulture (1)
				if (field.DataType == "String")
					Attribute(RdeWriterTokens.Attribute.Culture, field.CollateCulture.LCID);
                */

				if (field.IsColumn2)
					Attribute(RdeWriterTokens.Attribute.IsColumn, "true");
			}
			return true; 
		}

		// Salva un file con il numero di pagine, quando ha finito l'estrazione di tutte
		// le pagine
		//------------------------------------------------------------------------------
		virtual public void SaveTotPageFile()
		{
			if (Open())
			{
				WriteTotalPages();

				Close(TotPageFilename);
			}
		}
		
		// memorizza in un file xml che si chiama come il report valori cross tra il
		// motore di report ed il visualizzatore. Da rivedere quando gireranno in contemporanea
		// per ora si ammette che siano sequenziali.
		//------------------------------------------------------------------------------
		virtual public void SaveInfo()
		{
			if (Open())
			{
				WriteFieldsType();
				Close(InfoFilename);
			}
		}

		//	aggiunge gli errori se ce ne sono stati
		//---------------------------------------------------------------------------
		virtual public bool XmlGetErrors(StringCollection errors, StringCollection warnings)	{ return true; }
		virtual public bool XmlGetParameters()													{ return true; }

		//---------------------------------------------------------------------------
		virtual public void SavePage()
		{
			Close(Filename);
		}

		//---------------------------------------------------------------------------
		private void Tag(string tag) 
		{ 
			currentElement = output.DocumentElement.AppendChild(output.CreateElement(tag)) as XmlElement;
		}
		
        //---------------------------------------------------------------------------
        private void Attribute(string name, object o)
        {
            currentElement.SetAttribute(name, SoapTypes.To(o));
        }
		
		//---------------------------------------------------------------------------
		public void NextPage() 
		{ 
			SavePage();
			pageNo++;
		}

		//---------------------------------------------------------------------------
		virtual public bool WriteMessageBox(Command cmd, string message)		
		{ 
			if (Open())
			{
				Tag(cmd.ToString());
				Attribute(RdeWriterTokens.Attribute.Message, message);
				return true; 
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
        virtual public bool WriteIDCommand(ushort tableId, string name, int id, object o, Command cmd, string WoormType)		
		{ 
			if (Open())
			{
				Tag(cmd.ToString());
                Attribute(RdeWriterTokens.Attribute.ID, id);
				return true; 
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
		virtual public bool WriteSubTotal(ushort tableId, string name, int id, object o, bool isCellTail)		
		{ 
			if (Open())
			{
				Tag(RdeWriterTokens.Element.SubTotal);
				Attribute(RdeWriterTokens.Attribute.ID, id);
				Attribute(RdeWriterTokens.Attribute.Value, o);
				//indica se e' una cella il cui contenuto e' la coda di una stringa iniziata nella corrispondente cella
				//della una riga precedente
				if (isCellTail) 
					Attribute(RdeWriterTokens.Attribute.CellTail,isCellTail);
				return true; 
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
		virtual public bool WriteColTotal(ushort tableId, string name, int id, object o, bool isCellTail)		
		{ 
			if (Open())
			{
				Tag(RdeWriterTokens.Element.Total);
				Attribute(RdeWriterTokens.Attribute.ID, id);
				Attribute(RdeWriterTokens.Attribute.Value, o);
				//indica se e' una cella il cui contenuto e' la coda di una stringa iniziata nella corrispondente cella
				//della una riga precedente
				if (isCellTail) 
					Attribute(RdeWriterTokens.Attribute.CellTail, isCellTail);
				return true; 
			}
			return false;
		}
		
		//---------------------------------------------------------------------------
        virtual public bool WriteCell(ushort tableId, string name, int id, object o, bool isCellTail, string WoormType, bool isValid /*used only in xmlWriter*/)		
		{ 
			if (Open())
			{
				Tag(RdeWriterTokens.Element.Cell);
				Attribute(RdeWriterTokens.Attribute.ID, id);
                Attribute(RdeWriterTokens.Attribute.Value, o);
                //indica se e' una cella il cui contenuto e' la coda di una stringa iniziata nella corrispondente cella
				//della una riga precedente
				if (isCellTail)
					Attribute(RdeWriterTokens.Attribute.CellTail,isCellTail);
				return true; 
			}
			return false;
		}
				
		//---------------------------------------------------------------------------
        virtual public bool WriteField(string name, int id, object o, string WoormType, bool isValid /*used only in xmlWriter*/)		
		{ 
			if (Open())
			{
				Tag(RdeWriterTokens.Element.Cell);
				Attribute(RdeWriterTokens.Attribute.ID, id);
                Attribute(RdeWriterTokens.Attribute.Value, o);

				//layout lo scrivo anche come attributo della radice in modo che sia letto come prima cosa dal 
				//visualizzatore e possa caricare subito l'array degli objects corretto
				if (id == SpecialReportField.ID.LAYOUT)
				{
					LayoutAttribute(SpecialReportField.NAME.LAYOUT, o);
				}
				return true; 
			}
			return false;
		}

        virtual public bool WriteArray(string name, int id, object o, string WoormType, bool isValid /*used only in xmlWriter*/)
        {
            if (Open())
            {
                Tag(RdeWriterTokens.Element.Array);
                Attribute(RdeWriterTokens.Attribute.ID, id);

                DataArray ar = o as DataArray;
                if (ar == null)
                    return false;
                string s = ar.ToString();

                Attribute(RdeWriterTokens.Attribute.Value, s);

                return true;
            }
            return false;
        }

        ///<summary>
        ///scrive il nome del layout come attributo del nodo radice "Report"
        ///e.g.
        ///<Report Release="1" ReportLayout="Default">
        ///</summary>
        //---------------------------------------------------------------------------
        private void LayoutAttribute(string name,object o)
		{
			output.DocumentElement.SetAttribute(name,SoapTypes.To(o));
		}
	}
}