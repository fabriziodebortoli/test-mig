using System;
using System.Collections;
using System.IO;

namespace Microarea.Library.OracleScriptGenerator
{
	#region Classi che mappano la tabella da generare
	
	public class TableColumn
	{
		public string m_NomeColonna = string.Empty;
		public string m_TipoColonna = string.Empty;
		public int m_LenColonna = 0;
		public string m_Default = string.Empty;
		public string m_NomeColonnaEsteso = string.Empty;
		public bool isNullable = true;
		public string m_DefaultConstraintName = string.Empty;

		//-----------------------------------------------------------------------------
		public TableColumn(string nomeColonna, string tipoColonna, int lenColonna, bool nullable, string valoreDefault, string nomeConstraintDefault)
		{
			m_NomeColonnaEsteso = nomeColonna;
			if (nomeColonna.Length > 30)
				m_NomeColonna = nomeColonna.Substring(0, 30);
			else
				m_NomeColonna = nomeColonna;

			m_TipoColonna = tipoColonna;
			m_LenColonna = lenColonna;
			m_Default = valoreDefault;
			isNullable = nullable;
			m_DefaultConstraintName = nomeConstraintDefault;
		}

		public TableColumn() {}

		//-----------------------------------------------------------------------------
		public string GetColumnName()
		{
			return m_NomeColonnaEsteso;
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			if (m_NomeColonnaEsteso != m_NomeColonna)
				result.Add(string.Format("La colonna {0} è stata troncata. Il nuovo nome è {1}.", m_NomeColonnaEsteso, m_NomeColonna.ToUpper()));
		}
		//-----------------------------------------------------------------------------
		public void SetDefault()
		{
			if (m_Default != "NULL")
				return;

			switch (m_TipoColonna.ToLower())
			{
				case "smallint":
				{
					m_Default = "NULL DEFAULT (0)";
					break;
				}
				case "int":
				{
					m_Default = "NULL DEFAULT (###???###)";
					break;
				}
				case "float":
				{
					m_Default = "NULL DEFAULT (0)";
					break;
				}
				case "char":
				{
					if (m_LenColonna == 1)
						m_Default = "NULL DEFAULT (###???###)";
					else
						m_Default = "NULL DEFAULT ('')";
					break;
				}
				case "varchar":
				{
					m_Default = "NULL DEFAULT ('')";
					break;
				}
				case "uniqueidentifier":
				{
					m_Default = "NULL DEFAULT (0x00)";
					break;
				}
				case "text":
				{
					m_Default = "NULL DEFAULT ('')";
					break;
				}
				case "datetime":
				{
					m_Default = "NULL DEFAULT ('17991231')";
					break;
				}
			}
		}

		//-----------------------------------------------------------------------------
		public void SetDefaultConstraintName(string nomeTabella, CUIdList uid)
		{
			if (m_DefaultConstraintName != string.Empty)
				return;

			string nTabella = nomeTabella;
			string nColonna = m_NomeColonna;
			
			try
			{
				if (nTabella.Length > 13)
					nTabella = nTabella.Substring(3, 10);
				else
					nTabella = nTabella.Substring(3, nTabella.Length - 3);
			}
			catch
			{
			}

			if (nColonna.Length > 10)
				nColonna = nColonna.Substring(0, 10);

			string nConstraint = string.Format("DF_{0}_{1}", nTabella, nColonna);

			m_DefaultConstraintName = uid.Add(nConstraint);
		}

		public void Rename(string newName)
		{
			m_NomeColonnaEsteso = newName;
			if (newName.Length > 30)
				m_NomeColonna = newName.Substring(0, 30);
			else
				m_NomeColonna = newName;
		}
	}


	public class TableColumnList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public TableColumnList ()
		{
		}

		public void Add(string nomeColonna, string tipoColonna, int lenColonna, bool nullable, string valoreDefault, string nomeConstraintDefault)
		{
			if (!Exist(nomeColonna))
				base.Add(new TableColumn(nomeColonna, tipoColonna, lenColonna, nullable, valoreDefault, nomeConstraintDefault));
		}

		public void Delete(string nomeColonna)
		{
			if (Exist(nomeColonna))
				base.Remove(Get(nomeColonna));
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string nomeColonna)
		{
			foreach (TableColumn tCol in this)
			{
				if (tCol.GetColumnName() == nomeColonna)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public TableColumn Get(string nomeColonna)
		{
			foreach (TableColumn tCol in this)
			{
				if (tCol.GetColumnName() == nomeColonna)
					return tCol;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			foreach(TableColumn tCol in this)
				tCol.CheckNames(result);
		}
	}


	public class TableConstraint
	{
		public string m_Nome = string.Empty;
		public string m_NomeEsteso = string.Empty;

		public bool m_Primary = true;
		public bool m_OracleScriptApproved = false;

		public string m_ReferenceTableName = string.Empty;
		public string m_ReferenceTableNameEstesa = string.Empty;

		public TableColumnList m_Colonne = new TableColumnList();
		
		public ArrayList m_ReferenceColumns = new ArrayList();
		public ArrayList m_ReferenceColumnsEstese = new ArrayList();

		//-----------------------------------------------------------------------------
		public TableConstraint(string nomeConstraint, bool primario)
		{
			m_NomeEsteso = nomeConstraint;
			if (nomeConstraint.Length > 30)
				m_Nome = nomeConstraint.Substring(0, 30);
			else
				m_Nome = nomeConstraint;
			m_Primary = primario;
		}

		//-----------------------------------------------------------------------------
		public string GetConstraintName()
		{
			return m_NomeEsteso;
		}

		//-----------------------------------------------------------------------------
		public void AddColumn(TableColumn colonna)
		{
			m_Colonne.Add(colonna);
		}

		public void DeleteColumn(string nomeColonna)
		{
			m_Colonne.Delete(nomeColonna);
		}

		//-----------------------------------------------------------------------------
		public void AddTableReference(string nomeTabella)
		{
			m_ReferenceTableNameEstesa = nomeTabella;
			if (nomeTabella.Length > 30)
				m_ReferenceTableName = nomeTabella.Substring(0, 30);
			else
				m_ReferenceTableName = nomeTabella;
		}

		//-----------------------------------------------------------------------------
		public void AddColumnReference(string nomeColonna)
		{
			m_ReferenceColumnsEstese.Add(nomeColonna);
			if (nomeColonna.Length > 30)
				m_ReferenceColumns.Add(nomeColonna.Substring(0, 30));
			else
				m_ReferenceColumns.Add(nomeColonna);
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			if (m_NomeEsteso != m_Nome &&
				!m_OracleScriptApproved)
				result.Add(string.Format("Il constraint {0} è stato troncato. Il nuovo nome è {1}.", m_NomeEsteso, m_Nome.ToUpper()));
		}
	}


	public class TableConstraintList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public TableConstraintList ()
		{
		}

		public void Add(string nomeConstraint, bool primario)
		{
			if (!Exist(nomeConstraint))
				base.Add(new TableConstraint(nomeConstraint, primario));
		}

		//-----------------------------------------------------------------------------
		public TableConstraint Get(string nomeConstraint)
		{
			foreach (TableConstraint tCon in this)
			{
				if (tCon.GetConstraintName() == nomeConstraint)
					return tCon;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public TableConstraint GetPrimary()
		{
			foreach (TableConstraint tCon in this)
			{
				if (tCon.m_Primary)
					return tCon;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string nomeConstraint)
		{
			foreach (TableConstraint tCon in this)
			{
				if (tCon.GetConstraintName() == nomeConstraint)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			foreach(TableConstraint tCon in this)
				tCon.CheckNames(result);
		}

		public void DeleteColumn(string nomeColonna)
		{
			foreach(TableConstraint tCon in this)
				tCon.DeleteColumn(nomeColonna);
		}
	}


	public class TableIndex
	{
		public string m_Nome = string.Empty;
		public string m_Table = string.Empty;
		
		public TableColumnList m_Colonne = new TableColumnList();

		public string m_NomeEsteso = string.Empty;
		public string m_TableEstesa = string.Empty;

		//-----------------------------------------------------------------------------
		public TableIndex(string nomeIndex, string nomeTabella)
		{
			m_NomeEsteso = nomeIndex;
			if (nomeIndex.Length > 30)
				m_Nome = nomeIndex.Substring(0, 30);
			else
				m_Nome = nomeIndex;
			m_TableEstesa = nomeTabella;
			if (nomeTabella.Length > 30)
				m_Table = nomeTabella.Substring(0, 30);
			else
				m_Table = nomeTabella;
		}

		//-----------------------------------------------------------------------------
		public string GetIndexName()
		{
			return m_NomeEsteso;
		}

		//-----------------------------------------------------------------------------
		public void AddColumn(TableColumn colonna)
		{
			m_Colonne.Add(colonna);
		}

		public void DeleteColumn(string nomeColonna)
		{
			m_Colonne.Delete(nomeColonna);
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			if (m_NomeEsteso != m_Nome)
				result.Add(string.Format("L'indice {0} è stato troncato. Il nuovo nome è {1}.", m_NomeEsteso, m_Nome.ToUpper()));
		}
	}


	public class TableIndexList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public TableIndexList ()
		{
		}

		public void Add(string nomeIndex, string nomeTabella)
		{
			if (!Exist(nomeIndex))
				base.Add(new TableIndex(nomeIndex, nomeTabella));
		}

		//-----------------------------------------------------------------------------
		public TableIndex Get(string nomeIndex)
		{
			foreach (TableIndex tInd in this)
			{
				if (tInd.GetIndexName() == nomeIndex)
					return tInd;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string nomeIndex)
		{
			foreach (TableIndex tInd in this)
			{
				if (tInd.GetIndexName() == nomeIndex)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			foreach(TableIndex tInd in this)
				tInd.CheckNames(result);
		}

		public void DeleteColumn(string nomeColonna)
		{
			foreach(TableIndex tInd in this)
				tInd.DeleteColumn(nomeColonna);
		}
	}


	public class SqlTable
	{
		private string m_NomeTabella = string.Empty;
		public TableColumnList m_Colonne = new TableColumnList();
		public TableConstraintList m_Constraints = new TableConstraintList();
		public TableIndexList m_Indexes = new TableIndexList();

		private string m_NomeTabellaEsteso = string.Empty;

		//-----------------------------------------------------------------------------
		public SqlTable(string nomeTabella)
		{
			m_NomeTabellaEsteso = nomeTabella;
			if (nomeTabella.Length > 30)
				m_NomeTabella = nomeTabella.Substring(0, 30);
			else
				m_NomeTabella = nomeTabella;

		}

		//-----------------------------------------------------------------------------
		public string GetTableName()
		{
			return m_NomeTabellaEsteso;
		}

		//-----------------------------------------------------------------------------
		public string GetOracleTableName()
		{
			return m_NomeTabella.ToUpper();
		}

		//-----------------------------------------------------------------------------
		public void AddColumn(string nomeColonna)
		{
			AddColumn(nomeColonna, "", 0, true, "", "");
		}

		//-----------------------------------------------------------------------------
		public void AddColumn(string nomeColonna, string tipoColonna, int lenColonna, bool nullable, string valoreDefault, string nomeConstraintDefault)
		{
			m_Colonne.Add(nomeColonna, tipoColonna, lenColonna, nullable, valoreDefault, nomeConstraintDefault);
		}

		//-----------------------------------------------------------------------------
		public void DeleteColumn(string nomeColonna)
		{
			m_Colonne.Delete(nomeColonna);
			m_Constraints.DeleteColumn(nomeColonna);
			m_Indexes.DeleteColumn(nomeColonna);
		}

		//-----------------------------------------------------------------------------
		public TableColumn GetColumn(string nomeColonna)
		{
			return m_Colonne.Get(nomeColonna);
		}

		//-----------------------------------------------------------------------------
		public void AddConstraint(string nomeConstraint, bool primario)
		{
			m_Constraints.Add(nomeConstraint, primario);
		}

		//-----------------------------------------------------------------------------
		public void AddConstraintColumn(string nomeConstraint, string nomeColonna)
		{
			TableConstraint tCon = m_Constraints.Get(nomeConstraint);
			if (tCon != null)
			{
				tCon.AddColumn(GetColumn(nomeColonna));
			}
		}

		//-----------------------------------------------------------------------------
		public void AddConstraintTableReference(string nomeConstraint, string nomeTabella)
		{
			TableConstraint tCon = m_Constraints.Get(nomeConstraint);
			if (tCon != null)
			{
				tCon.AddTableReference(nomeTabella);
			}
		}

		//-----------------------------------------------------------------------------
		public void AddConstraintColumnReference(string nomeConstraint, string nomeColonna)
		{
			TableConstraint tCon = m_Constraints.Get(nomeConstraint);
			if (tCon != null)
			{
				tCon.AddColumnReference(nomeColonna);
			}
		}

		//-----------------------------------------------------------------------------
		public TableConstraint GetPrimaryConstraint()
		{
			return m_Constraints.GetPrimary();
		}

		//-----------------------------------------------------------------------------
		public void AddIndex(string nomeIndex, string nomeTabella)
		{
			m_Indexes.Add(nomeIndex, nomeTabella);
		}

		//-----------------------------------------------------------------------------
		public void AddIndexColumn(string nomeIndex, string nomeColonna)
		{
			TableIndex tInd = m_Indexes.Get(nomeIndex);
			if (tInd != null)
			{
				tInd.AddColumn(GetColumn(nomeColonna));
			}
		}

		//-----------------------------------------------------------------------------
		public TableIndex GetTableIndex(string nomeIndex)
		{
			return m_Indexes.Get(nomeIndex);
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			if (m_NomeTabellaEsteso != m_NomeTabella)
				result.Add(string.Format("La tabella {0} è stata troncata. Il nuovo nome è {1}.", m_NomeTabellaEsteso, m_NomeTabella.ToUpper()));

			int errorCount = result.Count;

			m_Colonne.CheckNames(result);
			m_Constraints.CheckNames(result);
			m_Indexes.CheckNames(result);

			if (result.Count > errorCount)
			{

				result.Insert(errorCount, string.Format("Check della tabella {0}:", m_NomeTabellaEsteso));
			}
		}
		//-----------------------------------------------------------------------------
		public void SetDefault()
		{
			foreach(TableColumn c in m_Colonne)
			{
				c.SetDefault();
			}
		}

		//-----------------------------------------------------------------------------
		public void SetDefaultConstraintName(CUIdList uid)
		{
			foreach(TableColumn c in m_Colonne)
			{
				c.SetDefaultConstraintName(m_NomeTabella, uid);
			}
		}
	}



	public class SqlTableList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public SqlTableList ()
		{
		}

		public void Add(string nomeTable)
		{
			if (!Exist(nomeTable))
				base.Add(new SqlTable(nomeTable));
		}

		//-----------------------------------------------------------------------------
		public SqlTable Get(string nomeTable)
		{
			foreach (SqlTable tTab in this)
			{
				if (tTab.GetTableName() == nomeTable)
					return tTab;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public SqlTable GetByOracleName(string nomeTable)
		{
			foreach (SqlTable tTab in this)
			{
				if (tTab.GetOracleTableName() == nomeTable)
					return tTab;
			}
			return null;
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string nomeTable)
		{
			foreach (SqlTable tTab in this)
			{
				if (tTab.GetTableName() == nomeTable)
					return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------
		public void CheckNames(ArrayList result)
		{
			foreach(SqlTable tSql in this)
				tSql.CheckNames(result);
		}
	}


	#endregion

	#region Clessi che gestiscono il parsing del file .sql

	public class CUId
	{
		public string m_Nome = string.Empty;
		public int m_Counter = 0;

		//-----------------------------------------------------------------------------
		public CUId(string nome)
		{
			m_Nome = nome;
			m_Counter = 0;
		}

		//-----------------------------------------------------------------------------
		public int Increment()
		{
			return ++ m_Counter;
		}
	}


	public class CUIdList : ArrayList
	{
		//-----------------------------------------------------------------------------
		public CUIdList ()
		{
		}

		public string Add(string nome)
		{
			if (!Exist(nome))
			{
				base.Add(new CUId(nome));
				return string.Format("{0}_000", nome);
			}
			else
				foreach (CUId tUid in this)
				{
					if (tUid.m_Nome.ToUpper() == nome.ToUpper())
					{
						int i = tUid.Increment();
						string cnt = string.Empty;
						if (i < 10)
							cnt = string.Format("00{0}", i.ToString());
						else if (i < 100)
							cnt = string.Format("0{0}", i.ToString());
						else
							cnt = i.ToString();
						return string.Format("{0}_{1}", nome, cnt);
					}
				}
			return string.Empty;
		}

		//-----------------------------------------------------------------------------
		public bool Exist(string nome)
		{
			foreach (CUId tUid in this)
			{
				if (tUid.m_Nome.ToUpper() == nome.ToUpper())
					return true;
			}
			return false;
		}
	}


	public class SqlParser
	{
		private CUIdList uid = new CUIdList();
		private SqlTableList m_Tabelle = new SqlTableList();
		private ArrayList errorList = new ArrayList();

		public SqlParser()
		{
		}

		public SqlTable GetTable(string tableName)
		{
			return m_Tabelle.Get(tableName);
		}

		public void AddTable(string tableName)
		{
			m_Tabelle.Add(tableName);
		}

		public bool EUnaView(string fileText)
		{
			string tmp = fileText.Replace(" ", string.Empty).ToUpper();;
			if (tmp.IndexOf("CREATEVIEW") > 0)
				return true;
			return false;
		}

		public void Init()
		{
			m_Tabelle.Clear();
			errorList.Clear();
		}

		public SqlTableList GetTableList()
		{
			return m_Tabelle;
		}

		public bool Parse(string fileName)
		{
			string sqlFileText = SqlFileRead(fileName);
			if (sqlFileText == string.Empty)
				return false;

			int stato = 0;

			string nomeTabella = string.Empty;
			string nomeColonna = string.Empty;
			string tipoColonna = string.Empty;
			bool colonnaNullable = true;
			int lenColonna = 0;
			string valoreDefault = string.Empty;
			string nomeConstraintDefault = string.Empty;
			string nomeConstraint = string.Empty;
			string nomeColonnaConstraint = string.Empty;
			bool isPrimary = true;
			string nomeTabellaRif = string.Empty;
			string nomeColonnaTabellaRif = string.Empty;
			string nomeIndice = string.Empty;
			string nomeTabellaIndice = string.Empty;
			string nomeColonnaIndice = string.Empty;
			bool cercoCommento = false;
			string riga = string.Empty;

			if (EUnaView(sqlFileText))
				return false;

			StringReader sr = new StringReader(sqlFileText);

			while (true) 
			{
				riga = sr.ReadLine();
				if (riga == null) 
				{
					break;
				}

				riga = riga.Trim();
				
				while(riga != string.Empty)
				{
					if (riga.Length >= 2)
					{
						if (riga.Substring(0,2) == "--")
						{
							riga = string.Empty;
							continue;
						}

						if (riga.Substring(0,2) == "/*")
							cercoCommento = true;

						if (cercoCommento)
						{
							if (riga.IndexOf("*/") >= 0)
							{
								riga = riga.Remove(0, riga.IndexOf("*/") + 2);
								cercoCommento = false;
							}
							else
								riga = string.Empty;
						}
					}

					if (riga == string.Empty)
						continue;

					switch(stato)
					{
						case 0: //Devo ancora trovare il BEGIN
						{
							if (string.Compare(riga.Split(' ')[0], "BEGIN", true) == 0)
							{
								riga = string.Empty;
								stato = 1;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 1: //Cerco TABLE
						{
							if (string.Compare(riga.Split(' ')[0], "TABLE", true) == 0)
							{
								stato = 2;
							}

							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 2: //Cerco il nome della tabella
						{
							nomeTabella = riga.Split(' ')[0];
							riga = riga.Substring(nomeTabella.Length);
							riga = riga.Trim();
							m_Tabelle.Add(nomeTabella);
							stato = 3;
							break;
						}
						case 3: //Cerco (
						{
							if (string.Compare(riga.Split(' ')[0], "(", true) == 0)
							{
								stato = 4;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 4: //Cerco il nome della colonna o il CONSTRAINT o )
						{
							nomeColonna = riga.Split(' ')[0];
							
							riga = riga.Substring(nomeColonna.Length);
							riga = riga.Trim();

							if (string.Compare(nomeColonna, "CONSTRAINT", true) == 0)
							{
								nomeColonna = string.Empty;
								stato = 9;
							}
							else
							{
								if (string.Compare(nomeColonna, ")", true) == 0)
								{
									nomeColonna = string.Empty;
									stato = 18;
								}
								else
									stato = 5;
							}
							
							break;
						}
						case 5: //Cerco il tipo della colonna
						{
							tipoColonna = riga.Split(' ')[0];
							riga = riga.Substring(tipoColonna.Length);
							riga = riga.Trim();
							if ((string.Compare(tipoColonna, "varchar", true) == 0) || (string.Compare(tipoColonna, "char", true) == 0))
								stato = 6;
							else
								stato = 7;
							break;
						}
						case 6: //Cerco la lunghezza della colonna
						{
							string tmp = riga.Split(' ')[0];
							tmp = tmp.Replace("(", string.Empty);
							tmp = tmp.Replace(")", string.Empty);
							tmp = tmp.Trim();

							lenColonna = int.Parse(tmp);
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							stato = 7;
							break;
						}
						case 7: //Cerco NULL o NOT NULL
						{
							string tmp = riga.Split(' ')[0];

							if (string.Compare(tmp, ",", true) == 0)
							{
								valoreDefault = string.Empty;
								nomeConstraintDefault = string.Empty;
								m_Tabelle.Get(nomeTabella).AddColumn(nomeColonna, tipoColonna, lenColonna, colonnaNullable, valoreDefault, nomeConstraintDefault);
								stato = 4;
							}
							
							if (string.Compare(tmp, "NULL", true) == 0)
							{
								colonnaNullable = true;
							}

							if (string.Compare(tmp, "NOT", true) == 0)
							{
								colonnaNullable = false;
								riga = riga.Substring(tmp.Length + riga.Split(' ')[1].Length + 1);
								riga = riga.Trim();
								break;
							}

							if (string.Compare(tmp, "CONSTRAINT", true) == 0)
							{
								riga = riga.Replace("(", "");
								riga = riga.Replace(")", "");
								stato = 26;
							}

							riga = riga.Substring(tmp.Length);
							riga = riga.Trim();
							
							break;
						}
						case 8: //Cerco il valore di default
						{
							if (string.Compare(riga.Split(' ')[0], ",", true) != 0)
							{
								valoreDefault = string.Format("{0} {1}", valoreDefault, riga.Split(' ')[0]).Trim();

								if (riga.Substring(riga.Split(' ')[0].Length).Trim() == string.Empty) // non è stata messa la ,
								{
									m_Tabelle.Get(nomeTabella).AddColumn(nomeColonna, tipoColonna, lenColonna, colonnaNullable, valoreDefault, nomeConstraintDefault);
									nomeConstraintDefault = string.Empty;
									valoreDefault = string.Empty;
									stato = 4;
								}
							}
							else
							{
								m_Tabelle.Get(nomeTabella).AddColumn(nomeColonna, tipoColonna, lenColonna, colonnaNullable, valoreDefault, nomeConstraintDefault);
								nomeConstraintDefault = string.Empty;
								valoreDefault = string.Empty;
								stato = 4;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();

							break;
						}
						case 9: //Cerco il nome del CONSTRAINT
						{
							nomeConstraint = riga.Split(' ')[0];
							riga = riga.Substring(nomeConstraint.Length);
							riga = riga.Trim();
							stato = 10;
							break;
						}
						case 10: //Cerco il tipo del CONSTRAINT
						{
							if (string.Compare(riga.Split(' ')[0], "PRIMARY", true) == 0)
								isPrimary = true;
							else
								isPrimary = false;
							m_Tabelle.Get(nomeTabella).AddConstraint(nomeConstraint, isPrimary);
							
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							stato = 11;
							break;
						}
						case 11: //Cerco (
						{
							if (string.Compare(riga.Split(' ')[0], "(", true) == 0)
							{
								stato = 12;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 12: //Cerco il nome della colonna
						{
							nomeColonnaConstraint = riga.Split(' ')[0];

							switch (nomeColonnaConstraint)
							{
								case "," :
								{
									break;
								}
								case ")" :
								{
									if (isPrimary)
										stato = 17;
									else
										stato = 13;

									break;
								}
								default :
								{
									m_Tabelle.Get(nomeTabella).AddConstraintColumn(nomeConstraint, nomeColonnaConstraint);
									break;
								}
							}
							
							riga = riga.Substring(nomeColonnaConstraint.Length);
							riga = riga.Trim();

							break;
						}
						case 13: //Cerco REFERENCES
						{
							if (string.Compare(riga.Split(' ')[0], "REFERENCES", true) == 0)
							{
								stato = 14;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 14: //Cerco il nome della tabella di riferimento
						{
							nomeTabellaRif = riga.Split(' ')[0];
							riga = riga.Substring(nomeTabellaRif.Length);
							riga = riga.Trim();
							m_Tabelle.Get(nomeTabella).AddConstraintTableReference(nomeConstraint, nomeTabellaRif);
							stato = 15;
							break;
						}
						case 15: //Cerco (
						{
							if (string.Compare(riga.Split(' ')[0], "(", true) == 0)
							{
								stato = 16;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 16: //Cerco il nome della colonna
						{
							nomeColonnaTabellaRif = riga.Split(' ')[0];

							switch (nomeColonnaTabellaRif)
							{
								case "," :
								{
									break;
								}
								case ")" :
								{
									stato = 4;

									break;
								}
								default :
								{
									m_Tabelle.Get(nomeTabella).AddConstraintColumnReference(nomeConstraint, nomeColonnaTabellaRif);
									break;
								}
							}
							
							riga = riga.Substring(nomeColonnaTabellaRif.Length);
							riga = riga.Trim();

							break;
						}
						case 17: //Cerco , o )
						{
							switch (riga.Split(' ')[0])
							{
								case "," :
								{
									stato = 4;
									break;
								}
								case ")" :
								{
									stato = 18;
									break;
								}
							}
							
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();

							break;
						}
						case 18: //Cerco CREATE o END
						{
							switch (riga.Split(' ')[0])
							{
								case "CREATE" :
								{
									stato = 19;
									break;
								}
								case "END" :
								{
									stato = 0;
									break;
								}
							}
							
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();

							break;
						}
						case 19: //Cerco INDEX
						{
							if (string.Compare(riga.Split(' ')[0], "INDEX", true) == 0)
							{
								stato = 20;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 20: //Cerco il nome dell'Indice
						{
							nomeIndice = riga.Split(' ')[0];

							riga = riga.Substring(nomeIndice.Length);
							riga = riga.Trim();
							stato = 21;
							break;
						}
						case 21: //Cerco ON
						{
							if (string.Compare(riga.Split(' ')[0], "ON", true) == 0)
							{
								stato = 22;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 22: //Cerco il nome della tabella
						{
							nomeTabellaIndice = riga.Split(' ')[0];
							m_Tabelle.Get(nomeTabella).AddIndex(nomeIndice, nomeTabellaIndice);

							riga = riga.Substring(nomeTabellaIndice.Length);
							riga = riga.Trim();
							stato = 23;
							break;
						}
						case 23: //Cerco (
						{
							if (string.Compare(riga.Split(' ')[0], "(", true) == 0)
							{
								stato = 24;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 24: //Cerco il nome della colonna
						{
							nomeColonnaIndice = riga.Split(' ')[0];

							m_Tabelle.Get(nomeTabella).AddIndexColumn(nomeIndice, nomeColonnaIndice);

							riga = riga.Substring(nomeColonnaIndice.Length);
							riga = riga.Trim();

							stato = 25;

							break;
						}
						case 25: //Cerco , o )
						{
							switch (riga.Split(' ')[0])
							{
								case "," :
								{
									stato = 24;
									break;
								}
								case ")" :
								{
									stato = 18;
									break;
								}
							}
							
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();

							break;
						}
						case 26: //Cerco il nome del constraint
						{
							nomeConstraintDefault = riga.Split(' ')[0].Trim();
							
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();

							stato = 27;

							break;
						}
						case 27: //Cerco la parola chiave DEFAULT
						{
							if (string.Compare(riga.Split(' ')[0], "DEFAULT", true) == 0)
							{
								riga = riga.Replace("(", "");
								riga = riga.Replace(")", "");
								stato = 8;
							}

							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();

							break;
						}
					}
				}
			}
			return true;
		}


		public ArrayList GetErrors()
		{
			return errorList;
		}

		public bool HasErrors()
		{
			if (errorList.Count > 0)
				return true;
			return false;
		}

		private string SqlFileRead(string sqlFile)
		{
			try
			{
				FileStream fr = new FileStream(sqlFile, FileMode.Open, FileAccess.Read);
				string sqlFileText = string.Empty;
				for (int i = 1; i<=(int)fr.Length; i++)
					sqlFileText += (char)fr.ReadByte();
				fr.Close();

				sqlFileText = sqlFileText.Replace("[dbo].", " ");
				sqlFileText = sqlFileText.Replace("\t", " ");
				sqlFileText = sqlFileText.Replace("[", " ");
				sqlFileText = sqlFileText.Replace("]", " ");
				sqlFileText = sqlFileText.Replace("(", " (");
				sqlFileText = sqlFileText.Replace(")", ") ");
				sqlFileText = sqlFileText.Replace(",", " , ");

				return sqlFileText;
			}
			catch (Exception ex)
			{
				errorList.Add(ex.Message);
				return string.Empty;
			}
		}

		private string OracleFileRead(string sqlFile)
		{
			try
			{
				FileStream fr = new FileStream(sqlFile, FileMode.Open, FileAccess.Read);
				string sqlFileText = string.Empty;
				for (int i = 1; i<=(int)fr.Length; i++)
					sqlFileText += (char)fr.ReadByte();
				fr.Close();

				sqlFileText = sqlFileText.Replace("\t", " ");
				sqlFileText = sqlFileText.Replace("\"", " ");
				sqlFileText = sqlFileText.Replace("(", " (");
				sqlFileText = sqlFileText.Replace(")", ") ");
				sqlFileText = sqlFileText.Replace(",", " , ");

				return sqlFileText;
			}
			catch (Exception ex)
			{
				errorList.Add(ex.Message);
				return string.Empty;
			}
		}

		public ArrayList CheckConstraint()
		{
			ArrayList constraints = new ArrayList();
			string oldconstr = string.Empty;
			ArrayList err = new ArrayList();

			foreach (SqlTable t in m_Tabelle)
			{
				foreach (TableConstraint constr in t.m_Constraints)
				{
					constraints.Add(constr.m_Nome);
				}
			}

			constraints.Sort();

			foreach (string constr in constraints)
			{
				if (constr == oldconstr)
					err.Add(constr);

				oldconstr = constr;
			}
			return err;
		}

		public void SetDefault()
		{
			foreach (SqlTable tabella in m_Tabelle)
			{
				tabella.SetDefault();
			}
		}

		public void SetDefaultConstraintName()
		{
			foreach (SqlTable tabella in m_Tabelle)
			{
				tabella.SetDefaultConstraintName(uid);
			}
		}


		// METODI PER ORACLE
		public void UnParseOracle(string fileName)
		{
			string unparsedText = string.Empty;
			foreach (SqlTable tabella in m_Tabelle)
			{
				unparsedText += MakeCreateTableOracle(tabella.GetTableName());
				foreach (TableColumn colonna in tabella.m_Colonne)
				{
					unparsedText += MakeInsertColumnOracle(colonna.m_NomeColonnaEsteso, colonna.m_TipoColonna, colonna.m_LenColonna, colonna.isNullable, colonna.m_Default, colonna.m_DefaultConstraintName) + ",";
				}
				foreach (TableConstraint constraint in tabella.m_Constraints)
				{
					unparsedText += MakeConstraintOracle(constraint.m_Nome, constraint.m_Primary);
					unparsedText += string.Format("    (\r\n");
					foreach (TableColumn colonna in constraint.m_Colonne)
					{
						if (constraint.m_Colonne.IndexOf(colonna) == constraint.m_Colonne.Count - 1)
							unparsedText += string.Format("        \"{0}\"\r\n", colonna.m_NomeColonna.ToUpper());
						else
							unparsedText += string.Format("        \"{0}\",\r\n", colonna.m_NomeColonna.ToUpper());
					}
					if (constraint.m_Primary)
						if (tabella.m_Constraints.IndexOf(constraint) < tabella.m_Constraints.Count - 1)
							unparsedText += string.Format("    ),\r\n");
						else
							unparsedText += string.Format("    )\r\n");
					else
					{
						unparsedText += string.Format("    ) REFERENCES \"{0}\" (\r\n", constraint.m_ReferenceTableName.ToUpper());
						foreach (string rifColonna in constraint.m_ReferenceColumns)
						{
							if (constraint.m_ReferenceColumns.IndexOf(rifColonna) == constraint.m_ReferenceColumns.Count - 1)
								unparsedText += string.Format("        \"{0}\"\r\n", rifColonna.ToUpper());
							else
								unparsedText += string.Format("        \"{0}\",\r\n", rifColonna.ToUpper());
						}
						if (tabella.m_Constraints.IndexOf(constraint) < tabella.m_Constraints.Count - 1)
							unparsedText += string.Format("    ),\r\n");
						else
							unparsedText += string.Format("    )\r\n");
					}
				}
				unparsedText += string.Format(")\r\n");
				unparsedText += string.Format("GO\r\n\r\n");
				foreach (TableIndex indice in tabella.m_Indexes)
				{
					unparsedText += MakeIndexesOracle(indice);
				}
			}
			
			m_Tabelle.CheckNames(errorList);
			OracleFileWrite(fileName, unparsedText);
		}

		private string MakeCreateTableOracle(string tableName)
		{
			return string.Format("CREATE TABLE \"{0}\" ( \r\n", tableName.ToUpper());
		}

		protected string MakeInsertColumnOracle(string columnName, string colummType, int columnLen, bool nullable, string columnDef, string columnDefConstName)
		{
			string null_default = string.Empty;
			if (nullable)
			{
				if (columnDef != string.Empty)
					null_default = InterpretaDefault(colummType.ToLower(), columnDef, columnDefConstName.ToUpper());
			}
			else
				null_default = "NOT NULL";

			switch (colummType.ToLower())
			{
				case "smallint":
				{
					return string.Format("    \"{0}\" NUMBER(6) {1}\r\n", columnName.ToUpper(), null_default);
				}
				case "int":
				{
					return string.Format("    \"{0}\" NUMBER(10) {1}\r\n", columnName.ToUpper(), null_default);
				}
				case "float":
				{
					return string.Format("    \"{0}\" FLOAT(126) {1}\r\n", columnName.ToUpper(), null_default);
				}
				case "char":
				{
					return string.Format("    \"{0}\" CHAR({1}) {2}\r\n", columnName.ToUpper(), columnLen.ToString(), null_default);
				}
				case "varchar":
				{
					return string.Format("    \"{0}\" VARCHAR2({1}) {2}\r\n", columnName.ToUpper(), columnLen.ToString(), null_default);
				}
				case "uniqueidentifier":
				{
					return string.Format("    \"{0}\" CHAR(38) {1}\r\n", columnName.ToUpper(), null_default);
				}
				case "text":
				{
					return string.Format("    \"{0}\" CLOB {1}\r\n", columnName.ToUpper(), null_default);
				}
				case "datetime":
				{
					return string.Format("    \"{0}\" DATE {1}\r\n", columnName.ToUpper(), null_default);
				}
			}
			return string.Empty;
		}

		private string InterpretaDefault(string tipo, string def, string conName)
		{
			switch (tipo)
			{
				case "uniqueidentifier":
				{
					return "DEFAULT('{00000000-0000-0000-0000-000000000000}')";
				}
				case "datetime":
				{
					try
					{
						string gg = def.Substring(7, 2);
						string mm = def.Substring(5, 2);
						string aaaa = def.Substring(1, 4);
						return string.Format("DEFAULT (TO_DATE('{0}-{1}-{2}','DD-MM-YYYY'))", gg, mm, aaaa);
					}
					catch (Exception e)
					{
						errorList.Add(e.Message);
						return string.Empty;
					}
				}
				default:
				{
					return string.Format("DEFAULT({0})", def);
				}
			}
		}

		private string MakeConstraintOracle(string costraintName, bool isPrimary)
		{
			if (isPrimary)
			{
				return string.Format("   CONSTRAINT \"{0}\" PRIMARY KEY\r\n", costraintName.ToUpper());
			}
			else
			{
				return string.Format("   CONSTRAINT \"{0}\" FOREIGN KEY\r\n", costraintName.ToUpper());
			}
		}

		private string MakeIndexesOracle(TableIndex indice)
		{
			string returnString = string.Empty;
			
			returnString = string.Format("CREATE INDEX \"{0}\" ON \"{1}\" (", indice.m_Nome.ToUpper(), indice.m_Table.ToUpper());
			foreach (TableColumn indiceColonna in indice.m_Colonne)
			{
				if (indice.m_Colonne.IndexOf(indiceColonna) == indice.m_Colonne.Count - 1)
					returnString += string.Format("\"{0}\")\r\n", indiceColonna.m_NomeColonna.ToUpper());
				else
					returnString += string.Format("\"{0}\", ", indiceColonna.m_NomeColonna.ToUpper());
			}
			returnString += string.Format("GO\r\n\r\n");

			return returnString;
		}

		private void OracleFileWrite(string oracleFile, string oracleFileText)
		{
			char[] oracleFileTextArray = oracleFileText.ToCharArray();

			try
			{
				if (File.Exists(oracleFile))
				{
					File.Delete(oracleFile);
				}

				int lenPath = oracleFile.LastIndexOf("\\");
				string oracleDiractory = oracleFile.Substring(0, lenPath);
				if (!Directory.Exists(oracleDiractory))
				{
					Directory.CreateDirectory(oracleDiractory);
				}

				FileStream fw = new FileStream(oracleFile, FileMode.Append, FileAccess.Write);
				for (int i = 1; i<=oracleFileText.Length; i++)
					fw.WriteByte((byte) oracleFileTextArray[i-1]);
				fw.Close();
			}
			catch (Exception ex)
			{
				errorList.Add(ex.Message);
				return;
			}
		}

		public bool ParseOracle(string fileName)
		{
			string sqlFileText = OracleFileRead(fileName);
			if (sqlFileText == string.Empty)
				return false;

			int stato = 1;

			string nomeTabella = string.Empty;
			string nomeColonna = string.Empty;
			string tipoColonna = string.Empty;
			string valoreDefault = string.Empty;
			string nomeConstraintDefault = string.Empty;
			string nomeConstraint = string.Empty;
			string nomeColonnaConstraint = string.Empty;
			bool isPrimary = true;
			string nomeTabellaRif = string.Empty;
			string nomeColonnaTabellaRif = string.Empty;
			string nomeIndice = string.Empty;
			string nomeTabellaIndice = string.Empty;
			string nomeColonnaIndice = string.Empty;
			bool cercoCommento = false;
			string riga = string.Empty;

			SqlTable currentTable = null;
			TableConstraintList oracleConstraints = null;

			if (EUnaView(sqlFileText))
				return false;

			StringReader sr = new StringReader(sqlFileText);

			while (true) 
			{
				riga = sr.ReadLine();
				if (riga == null) 
				{
					break;
				}

				riga = riga.Trim();
				
				while(riga != string.Empty)
				{
					if (riga.Length >= 2)
					{
						if (riga.Substring(0,2) == "--")
						{
							riga = string.Empty;
							continue;
						}

						if (riga.Substring(0,2) == "/*")
							cercoCommento = true;

						if (cercoCommento)
						{
							if (riga.IndexOf("*/") >= 0)
							{
								riga = riga.Remove(0, riga.IndexOf("*/") + 2);
								cercoCommento = false;
							}
							else
								riga = string.Empty;
						}
					}

					if (riga == string.Empty)
						continue;

					switch(stato)
					{
						case 1: //Cerco TABLE
						{
							if (string.Compare(riga.Split(' ')[0], "TABLE", true) == 0)
							{
								stato = 2;
							}

							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 2: //Cerco il nome della tabella
						{
							nomeTabella = riga.Split(' ')[0];
							riga = riga.Substring(nomeTabella.Length);
							riga = riga.Trim();
							currentTable = m_Tabelle.GetByOracleName(nomeTabella);
							if (currentTable == null)
							{
								errorList.Add(string.Format("Non ho trovato la definizione della tabella {0} nello script sql!", nomeTabella));
								stato = 1;
							}
							else
								stato = 3;
							
							break;
						}
						case 3: //Cerco (
						{
							if (string.Compare(riga.Split(' ')[0], "(", true) == 0)
							{
								stato = 4;
							}
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							break;
						}
						case 4: //Cerco la parola chiave CONSTRAINT o GO
						{
							nomeColonna = riga.Split(' ')[0];
							
							riga = riga.Substring(nomeColonna.Length);
							riga = riga.Trim();

							if (string.Compare(nomeColonna, "CONSTRAINT", true) == 0)
							{
								nomeColonna = string.Empty;
								stato = 9;
							}
							else
							{
								if (string.Compare(nomeColonna, "GO", true) == 0)
								{
									nomeColonna = string.Empty;
									stato = 1;
								}
							}
							
							break;
						}
						case 9: //Cerco il nome del CONSTRAINT (di chiave)
						{
							nomeConstraint = riga.Split(' ')[0];
							riga = riga.Substring(nomeConstraint.Length);
							riga = riga.Trim();
							stato = 10;
							break;
						}
						case 10: //Cerco il tipo del CONSTRAINT
						{
							if (string.Compare(riga.Split(' ')[0], "PRIMARY", true) == 0)
							{
								oracleConstraints = new TableConstraintList();
								isPrimary = true;
							}
							else
								isPrimary = false;
							oracleConstraints.Add(nomeConstraint, isPrimary);
							
							riga = riga.Substring(riga.Split(' ')[0].Length);
							riga = riga.Trim();
							stato = 11;
							break;
						}
						case 11: //Testo i constraints e reinizializzo tutto
						{
							if (oracleConstraints.Count > currentTable.m_Constraints.Count)
							{
								errorList.Add("Non posso effettuare il controllo della coerenza dei CONSTRAINT in quanto \r\nil numero di CONSTRAINT presente sullo script Oracle supera quello dello script \r\ndi SqlServer!");
								currentTable = null;
								oracleConstraints.Clear();
								oracleConstraints = null;
							}
							else
							{
								for (int idx = 0; idx < oracleConstraints.Count; idx ++)
								{
									TableConstraint truncatedConstraint = (TableConstraint) currentTable.m_Constraints[idx];
									TableConstraint parsedConstraint = (TableConstraint) oracleConstraints[idx];
									if (truncatedConstraint.m_Nome.ToUpper() != parsedConstraint.m_NomeEsteso.ToUpper())
										truncatedConstraint.m_Nome = parsedConstraint.m_NomeEsteso;
									
									truncatedConstraint.m_OracleScriptApproved = true;
								}
							}
							stato = 1;
							break;
						}
					}
				}
			}
			return true;
		}


		// METODI PER SQL
		public void UnParseSql(string fileName)
		{
			string unparsedText = string.Empty;
			foreach (SqlTable tabella in m_Tabelle)
			{
				unparsedText += MakeCreateTableSql(tabella.GetTableName());
				foreach (TableColumn colonna in tabella.m_Colonne)
				{
					unparsedText += MakeInsertColumnSql(colonna.m_NomeColonnaEsteso, colonna.m_TipoColonna, colonna.m_LenColonna, colonna.isNullable, colonna.m_Default, colonna.m_DefaultConstraintName) + ",";
				}
				foreach (TableConstraint constraint in tabella.m_Constraints)
				{
					unparsedText += MakeConstraintSql(constraint.m_NomeEsteso, constraint.m_Primary);
					unparsedText += string.Format("    (\r\n");
					foreach (TableColumn colonna in constraint.m_Colonne)
					{
						if (constraint.m_Colonne.IndexOf(colonna) == constraint.m_Colonne.Count - 1)
							unparsedText += string.Format("        [{0}]\r\n", colonna.m_NomeColonnaEsteso);
						else
							unparsedText += string.Format("        [{0}],\r\n", colonna.m_NomeColonnaEsteso);
					}
					if (constraint.m_Primary)
						if (tabella.m_Constraints.IndexOf(constraint) < tabella.m_Constraints.Count - 1)
							unparsedText += string.Format("    ) ON [PRIMARY],\r\n");
						else
							unparsedText += string.Format("    ) ON [PRIMARY]\r\n");
					else
					{
						unparsedText += string.Format("    ) REFERENCES [dbo].[{0}] (\r\n", constraint.m_ReferenceTableName);
						foreach (string rifColonna in constraint.m_ReferenceColumns)
						{
							if (constraint.m_ReferenceColumns.IndexOf(rifColonna) == constraint.m_ReferenceColumns.Count - 1)
								unparsedText += string.Format("        [{0}]\r\n", rifColonna);
							else
								unparsedText += string.Format("        [{0}],\r\n", rifColonna);
						}
						if (tabella.m_Constraints.IndexOf(constraint) < tabella.m_Constraints.Count - 1)
							unparsedText += string.Format("    ),\r\n");
						else
							unparsedText += string.Format("    )\r\n");
					}
				}
				unparsedText += string.Format(") ON [PRIMARY]\r\n\r\n");
				foreach (TableIndex indice in tabella.m_Indexes)
				{
					unparsedText += MakeIndexesSql(indice);
				}

				
				unparsedText += string.Format("\r\nEND\r\n");
				unparsedText += string.Format("GO\r\n\r\n");
			}
			
			//m_Tabelle.CheckNames(errorList);
			SqlFileWrite(fileName, unparsedText);
		}

		private string MakeCreateTableSql(string tableName)
		{
			string res = string.Empty;
			res = string.Format("if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[{0}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\r\n",tableName);
			res+= string.Format(" BEGIN\r\n");
			res+= string.Format("CREATE TABLE [dbo].[{0}] (\r\n", tableName);
			return res;
		}

		protected string MakeInsertColumnSql(string columnName, string colummType, int columnLen, bool nullable, string columnDef, string columnDefConstName)
		{
			string null_default = string.Empty;
			if (nullable)
			{
				if (columnDef != string.Empty)
					null_default = string.Format("NULL CONSTRAINT {0} DEFAULT({1})", columnDefConstName, columnDef);
			}
			else
				null_default = "NOT NULL";

			switch (colummType.ToLower())
			{
				case "smallint":
				{
					return string.Format("    [{0}] [smallint] {1}\r\n", columnName, null_default);
				}
				case "int":
				{
					return string.Format("    [{0}] [int] {1}\r\n", columnName, null_default);
				}
				case "float":
				{
					return string.Format("    [{0}] [float] {1}\r\n", columnName, null_default);
				}
				case "char":
				{
					return string.Format("    [{0}] [char] ({1}) {2}\r\n", columnName, columnLen.ToString(), null_default);
				}
				case "varchar":
				{
					return string.Format("    [{0}] [varchar] ({1}) {2}\r\n", columnName, columnLen, null_default);
				}
				case "uniqueidentifier":
				{
					return string.Format("    [{0}] [uniqueidentifier] {1}\r\n", columnName, null_default);
				}
				case "text":
				{
					return string.Format("    [{0}] [text] {1}\r\n", columnName, null_default);
				}
				case "datetime":
				{
					return string.Format("    [{0}] [datetime] {1}\r\n", columnName, null_default);
				}
			}
			return string.Empty;
		}

		private string MakeConstraintSql(string costraintName, bool isPrimary)
		{
			if (isPrimary)
			{
				return string.Format("   CONSTRAINT [{0}] PRIMARY KEY NONCLUSTERED \r\n", costraintName);
			}
			else
			{
				return string.Format("   CONSTRAINT [{0}] FOREIGN KEY\r\n", costraintName);
			}
		}

		private string MakeIndexesSql(TableIndex indice)
		{
			string returnString = string.Empty;
			
			returnString = string.Format("CREATE INDEX [{0}] ON [dbo].[{1}] (", indice.m_Nome, indice.m_Table);
			foreach (TableColumn indiceColonna in indice.m_Colonne)
			{
				if (indice.m_Colonne.IndexOf(indiceColonna) == indice.m_Colonne.Count - 1)
					returnString += string.Format("[{0}]) ON [PRIMARY]\r\n", indiceColonna.m_NomeColonnaEsteso);
				else
					returnString += string.Format("[{0}], ", indiceColonna.m_NomeColonnaEsteso);
			}

			return returnString;
		}

		private void SqlFileWrite(string sqlFile, string sqlFileText)
		{
			char[] sqlFileTextArray = sqlFileText.ToCharArray();

			try
			{
				if (File.Exists(sqlFile))
				{
					File.Delete(sqlFile);
				}

				int lenPath = sqlFile.LastIndexOf("\\");
				string sqlDiractory = sqlFile.Substring(0, lenPath);
				if (!Directory.Exists(sqlDiractory))
				{
					Directory.CreateDirectory(sqlDiractory);
				}

				FileStream fw = new FileStream(sqlFile, FileMode.Append, FileAccess.Write);
				for (int i = 1; i<=sqlFileText.Length; i++)
					fw.WriteByte((byte) sqlFileTextArray[i-1]);
				fw.Close();
			}
			catch (Exception ex)
			{
				errorList.Add(ex.Message);
				return;
			}
		}


		public void UnParseAll(string sqlFileName, string oracleFileName)
		{
			UnParseSql(sqlFileName);
			UnParseOracle(oracleFileName);
		}

	}

	#endregion
}
