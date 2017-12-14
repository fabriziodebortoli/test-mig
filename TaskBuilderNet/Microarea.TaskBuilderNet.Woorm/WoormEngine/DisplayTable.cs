using System.Collections;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Woorm.WoormViewer;

namespace Microarea.TaskBuilderNet.Woorm.WoormEngine
{
	/// <summary>
	/// DisplayTable
	/// </summary>
	//============================================================================
	public class DisplayTable
	{
		public static string	LayoutDefaultName = Layout.DefaultName;  
		
		private ArrayList		columns = new ArrayList();
		private int				rowsNumber;
		private int				currentRow;
		private bool			tableFull;
		private bool			dataDisplayed;
		private int				multiLineFieldsNum;
		private int				multiLineFieldsCurrLine;
		private EventActions	tableActions;			// actions before a overflow table event and/or
		private bool			executingTableActions;	// actions after a full table event
		private string			publicName;
		private ushort			internalId;
		private string			layoutTable = LayoutDefaultName;
	
		//---------------------------------------------------------------------------
		public ushort		InternalId				{ get { return internalId; }}
		public string		PublicName				{ get { return publicName; }}
		public string		LayoutTable				{ get { return layoutTable; } }
		public ArrayList	Columns					{ get { return columns; }}

		public bool			TableFull				{ set { tableFull	= value; }}
		public bool			DataDisplayed			{ set { dataDisplayed	= value; }}
		public EventActions	TableActions			{ get { return tableActions; } set { tableActions = value; }}
		public int			MultiLineFieldsNum		{ get { return multiLineFieldsNum;} set { multiLineFieldsNum = value; }}
		public int			MultiLineFieldsCurrLine	{ get { return multiLineFieldsCurrLine;} set { multiLineFieldsCurrLine = value; }}

        public int			RowsNumber				{ get { return rowsNumber; } }
        public int			CurrentRow				{ get { return currentRow + 1; } }

        //---------------------------------------------------------------------------
		public DisplayTable(string strName)
		{
			publicName				= strName;
			internalId				= 0;
			rowsNumber				= 0;
			currentRow				= 0;
			tableFull				= false;
			dataDisplayed			= false;
			tableActions			= null;
			multiLineFieldsNum		= 0;
			multiLineFieldsCurrLine	= 0;
			executingTableActions	= false;
		}

		//---------------------------------------------------------------------------
		public void	AddColumn(Field pCol)
		{ 
			columns.Add(pCol);
		}

		//---------------------------------------------------------------------------
		public void ResetRowsCounter ()
		{
			currentRow		= 0;
			dataDisplayed	= false;
			tableFull		= false;
			executingTableActions = false;
		}

		//---------------------------------------------------------------------------
		public bool ExecOverflowActions()
		{
			if (tableFull)
			{
				if (tableActions != null)
				{
					if (executingTableActions)
					{
						executingTableActions = false;
						tableActions.Engine.SetError(string.Format(WoormEngineStrings.TableFull, PublicName));
						return false;
					}

					// semaforizzo l'esecuzione delle azioni Before per evitare
					// ricorsivita` indotta da comandi di visualizzazione
					// su una tabella in stato di full
					executingTableActions = true;
					if (!tableActions.BeforeActions.Exec())
					{
						executingTableActions = false;
						return false;
					}
				}

				ResetRowsCounter();
			}

			return true;
		}

		//---------------------------------------------------------------------------
		public bool WriteLine (ReportEngine reportEngine, RdeWriter.Command command)
		{
			if (multiLineFieldsNum > 0)
			{
				//
				// Se sono state "displayate" delle colonne multilinea si innesca una
				// scrittura ricorsiva delle sotto-stringhe rimanenti
				//

				// Il comando INTER_LINE deve essere pre o post-posto ai comandi
				// SPACE_LINE/NEXT_LINE
				if (command == RdeWriter.Command.Interline)
				{
					reportEngine.SetError(string.Format(WoormEngineStrings.BadInterline, PublicName));
					return false;
				}
				// Viene forzato un comando di NEXT_LINE
				if (!WriteLineSeparator(reportEngine, RdeWriter.Command.NextLine))
				{
					multiLineFieldsCurrLine = 0;
					return false;
				}

				// la "prima linea" e` gia` stata scritta dalle Display
				// chiamate direttamente dal ReportEngine, quindi si incrementa
				// PRIMA di chiamare le Write
				//
				multiLineFieldsCurrLine++;

				foreach (Field pColField in columns)
				{
					if	(
							!pColField.Hidden && !pColField.IsColTotal &&
							!pColField.Write(reportEngine)
						)
					{
						multiLineFieldsCurrLine = 0;
						return false;
					}
				}
				return WriteLine (reportEngine, command);
			}

			multiLineFieldsCurrLine = 0;
			return WriteLineSeparator(reportEngine, command);
		}

		//---------------------------------------------------------------------------
		public bool WriteLineSeparator (ReportEngine reportEngine, RdeWriter.Command command)
		{
			if (!dataDisplayed) 
				return true;

			if ((command != RdeWriter.Command.Interline) && !ExecOverflowActions()) 
				goto writeError;

			if (!reportEngine.OutChannel.WriteIDCommand(internalId, PublicName, internalId, null, command, string.Empty))
				goto writeError;

			if	(
					(command != RdeWriter.Command.Interline) &&
					!tableFull                    &&
					(rowsNumber > 0)              &&
					(++currentRow == rowsNumber)
				)
			{
				currentRow = 0;
				dataDisplayed = false;

				// table full: Exec AFTER actions. Nel caso di Office considero la tabella infinita.
				if (reportEngine.Report.EngineType != EngineType.OfficeXML)
				{
					tableFull = true;
					if (tableActions == null || !tableActions.AfterActions.Exec())
						goto writeError;
				}
			}

			return true;

		writeError:
			reportEngine.SetError(WoormEngineStrings.WriteErrorCommand);
			return false;
		}

		//----------------------------------------------------------------------------
		public bool Parse(Parser lex, FieldSymbolTable symbolTable)
		{                                      
			ushort anID = 0;
 			
			if	(
					lex.ParseSubscr(out rowsNumber) &&
					lex.ParseAlias(out anID)
				)
				{
                
                    if (lex.Parsed(Token.PAGE_LAYOUT))
                    {
                        lex.ParseString(out layoutTable);
                    }
                    /* TODO
                     * 		IBaseObj* pObj = m_pDocument->FindByID(nID, dsLayout.GetString());
		
                            if (
                                    !pObj || !pObj->IsKindOf(RUNTIME_CLASS(IBaseObj)) || 
                                    (
                                        strcmp(pObj->GetRuntimeClass()->m_lpszClassName, "Table") &&
                                        strcmp(pObj->GetRuntimeClass()->m_lpszClassName, "Repeater")
                                    )
                                )
                                return lex.SetError(_TB("Report-Tables section - Wrong Table/Repeater Internal identifier"), strNameDispTable);

                            if (nRows != pObj->RowsNumber())
                                return lex.SetError(_TB("Report-Tables Section - Wrong Table/Repeater rows number"), strNameDispTable);

                     * */
                    if (symbolTable.Contains(anID))
					{
						lex.SetError(string.Format(WoormEngineStrings.AliasExist, anID.ToString()));
						return false;
					}

					internalId = anID;
                
					return lex.ParseSep();
				}

			return false;
		}

		//----------------------------------------------------------------------------
		internal bool Unparse(Unparser unparser)
		{
			unparser.WriteID(publicName, false);  
			unparser.WriteSquareOpen(false);
			unparser.Write(this.rowsNumber, false);
			unparser.WriteSquareClose(false);

			unparser.WriteAlias(internalId, false);

			if (!layoutTable.CompareNoCase(SpecialReportField.REPORT_DEFAULT_LAYOUT_NAME))  
			{
				unparser.WriteTag(Token.PAGE_LAYOUT, false);
				unparser.Write(layoutTable);
			}
			unparser.WriteSep(true);
			return true;
		}

		//----------------------------------------------------------------------------
		internal bool UnparseTableEventAction(Unparser unparser)
		{
			if (this.tableActions == null || this.TableActions.IsEmpty)
				return true;

			if (tableActions.ContainsOnlyNonUnparsableActions())
				return true;

			unparser.WriteTag(Token.TABLE, false);
			unparser.WriteID(this.PublicName, false);
			unparser.WriteTag(Token.COLON, false);
			unparser.WriteTag(Token.DO, true);

			this.TableActions.Unparse(unparser);

			unparser.WriteLine();

			return true;
		}
	}
}
