using System;
using System.Collections;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.Library.SqlScriptUtility;
using Microarea.Library.TBWizardProjects;
using System.Xml;

namespace Microarea.Library.DBObjects
{
	//=================================================================================
	public class ScriptGenerator
	{
        ///<summary>
        /// Generazione script SQL dal file XML
        ///</summary>
		//---------------------------------------------------------------------
		public static void GoSql(string pathxml, string pathsql, DBMSType dbmsType)
		{
            DBObjectsProjectParser projParser = new DBObjectsProjectParser(null);
			WizardCodeGenerator wcg = new WizardCodeGenerator(null);
			XmlDocument xmlDBObjectsDoc = new XmlDocument();
			xmlDBObjectsDoc.Load(pathxml);
			IList<WizardTableInfo> ti = projParser.ParseTablesInfoNode(xmlDBObjectsDoc, true);
            IList<WizardExtraAddedColumnsInfo> cc = projParser.ParseExtraAddedColumnsInfo(pathxml);
            IList<TableUpdate> tu = projParser.ParseTableUpdate(pathxml);
			
			if (ti != null)
			{
                // Generazione script di CREATE TABLE
				for (int i = 0; i < ti.Count; i++)
				{
					WizardTableInfo tab = ti[i] as WizardTableInfo;
                    wcg.GenerateTableCreationScript(tab, pathsql, dbmsType);
				}
			}

			if (cc != null)
			{
                // Generazione script di ALTER TABLE + UPDATE
                for (int i = 0; i < cc.Count; i++)
				{
                    WizardExtraAddedColumnsInfo extraAddedColumns = cc[i] as DBObjectsExtraAddedColumnsInfo;
                    ArrayList myAddedColumnsList = new ArrayList();

					if (extraAddedColumns != null && extraAddedColumns.ColumnsCount > 1)
                    {
                        foreach (WizardTableColumnInfo col in extraAddedColumns.ColumnsInfo)
                        {
							WizardExtraAddedColumnsInfo extraAddedColumnInfo = new DBObjectsExtraAddedColumnsInfo
								(
								extraAddedColumns.TableNameSpace, 
								extraAddedColumns.TableName
								);
                            extraAddedColumnInfo.AddColumnInfo(col);
                            myAddedColumnsList.Add(extraAddedColumnInfo);
                        }
                    }
                    else
                        myAddedColumnsList.Add(extraAddedColumns);

                    foreach (WizardExtraAddedColumnsInfo weaci in myAddedColumnsList)
                    {
                        TableUpdate currentColUpdate = GetUpdate(tu, weaci.TableName, weaci.ColumnsInfo[0].Name);
                        if(currentColUpdate != null)
                            tu.Remove(currentColUpdate);
                        wcg.GenerateAlterTableScripts(weaci, currentColUpdate, pathsql, dbmsType);
                    }
				}

				if (tu != null && tu.Count > 0)
                {
                    foreach (TableUpdate tableUpdate in tu)
                        wcg.GenerateAlterTableScripts(null, tableUpdate, pathsql, dbmsType);
                }
			}

			// Generazione script di CREATE VIEW
			SqlViewList svl = projParser.ParseViewsInfoNode(pathxml);
			if (svl != null && svl.Count > 0)
			{
				foreach (SqlView sv in svl)
					wcg.GenerateViewCreationScript(sv, pathsql, dbmsType);
			}

			// Generazione script di CREATE PROCEDURE
			SqlProcedureList spl = projParser.ParseProceduresInfoNode(pathxml);
			if (spl != null && spl.Count > 0)
			{
				foreach (SqlProcedure sp in spl)
					wcg.GenerateProcedureCreationScript(sp, pathsql, dbmsType);
			}
		}

        //----------------------------------------------------------------------------
        private static TableUpdate GetUpdate(IList<TableUpdate> tablesUpdate, string tableName, string addedColumnName)
        {
            TableUpdate update = null;
            if (tablesUpdate != null)
            {
                foreach (TableUpdate tu in tablesUpdate)
                    if (string.Compare(tu.TableName, tableName, StringComparison.InvariantCultureIgnoreCase) == 0 &&
                        string.Compare(tu.SetColumnName, addedColumnName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        update = tu;
                        break;
                    }
            }

            return update;
        }

      
	}
}
