using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	// BugFix 21140 - problema campi indici di tipo data
	//================================================================================
	internal class SingleERPDocumentInstance
	{
		public string PrimaryKeyValue = string.Empty;
		public string DocNamespace = string.Empty;
        public List<SearchFieldIndex> GlobalSearchFieldsIndex = null;

		public List<SearchFieldIndex> SearchIndexField = new List<SearchFieldIndex>();

		//--------------------------------------------------------------------------    
		public SingleERPDocumentInstance(string primaryKeyValue, string docNamespace)
		{
			PrimaryKeyValue = primaryKeyValue;
			DocNamespace = docNamespace;
		}

		//--------------------------------------------------------------------------    
		public void AddSearchField(SearchFieldIndex searchField)
		{
			SearchIndexField.Add(searchField);
		}

		// Select DocumentDate, TaxAccrualDate, PostingDate from MA_JournalEntriesTax where JournalEntryId = 269499
		//--------------------------------------------------------------------------    
		private string GetSelectText()
		{
			string commandText = string.Empty;
			string fields = string.Empty;
			string tableName = string.Empty;
			List<string> enumsKeyField = new List<string>();

			string fieldValue = string.Empty;
			char[] delimier = { ';' };
			
			List<string> values = null;
			
			//lista di coppie: fieldName:fieldValue
            SearchFieldIndex existFieldIndex = null;

            try
            {
                foreach (SearchFieldIndex field in SearchIndexField)
                {
                    if (!string.IsNullOrEmpty(fields))
                        fields += ", ";
                    else
                        tableName = field.ErpTableName;

                    fields += field.ErpFieldName;
                }
                commandText = "SELECT " + fields + " FROM " + tableName + " WHERE ";

                fields = string.Empty;

                //WHERE
                values = PrimaryKeyValue.Split(delimier, StringSplitOptions.RemoveEmptyEntries).ToList();
                //lista di coppie: fieldName:fieldValue

                foreach (string value in values)
                {
                    if (!string.IsNullOrEmpty(fields))
                        fields += " AND ";

                    char[] delimier1 = { ':' };
                    string[] segmentValue = value.Split(delimier1, StringSplitOptions.RemoveEmptyEntries);

                    fieldValue = segmentValue[1];

                    existFieldIndex = SearchIndexField.Find(a => string.Compare(a.FieldName, segmentValue[0]) == 0);
                    //se non l'ho trovato nei campi da modificare per la singola istanza dell'ERPDocument allora vado a controllare se è nella lista globale
                    // questo accadde ad esempio in questo caso:
                    //CollectionID	SearchIndexID	ErpDocumentID	PrimaryKeyValue	                            FieldName	Docnamespace	                                            FieldValue	FormattedValue	PhysicalName	                    ValueType
                    //        5	        1063	        13	        ApplicationType:4E280001;ContractID:507;	ContractID	Document.PBNet.PBContractsRental.Documents.ContractsRental	    507	         507	    PB_CON_Contracts.ContractID	        long
                    //        5	        5901	        13	        ApplicationType:4E280001;ContractID:507;	ApplicationType	Document.PBNet.PBContractsRental.Documents.ContractsRental	1311244289	1311244289	PB_CON_Contracts.ApplicationType	10:20008
                    //        5	        5901	        8014	    ApplicationType:4E280001;ContractID:1505;	ApplicationType	Document.PBNet.PBContractsRental.Documents.ContractsRental	1311244289	1311244289	PB_CON_Contracts.ApplicationType	10:20008

                    //Dove il campo 5901 non viene considerato per ErpDocumentID = 8014 visto che viene considerato per il documento precedente con ID 13
                    if (existFieldIndex == null)
                        existFieldIndex = GlobalSearchFieldsIndex.Find(a => string.Compare(a.FieldName, segmentValue[0]) == 0);

                    if (existFieldIndex != null)
                    {
                        //se è di tipo enums
                        if (existFieldIndex.SearchField.IsEnumType)
                        {
                            Int64 enumNumb = Convert.ToInt64(fieldValue, 16);
                            fieldValue = enumNumb.ToString();
                        }


                        //se è di tipo BOOL bisogna fare la conversione tra il valore stringa (TRUE/FALSE) ed il valore del campo ERP char(1): 1/0
                        if (existFieldIndex.SearchField.ValueType == DataType.DataTypeStrings.Bool)
                            fieldValue = (string.Compare(fieldValue, "TRUE", true) == 0) ? "'1'" : "'0'";
                    }
                    else//se non è tra la lista dei campi da modificare allora vuol dire che è una stringa               
                        fieldValue = "'" + fieldValue + "'";

                    fields += segmentValue[0] + '=' + fieldValue;
                }
            }
            catch (Exception e)
            {
                throw (e);
            }


			return commandText += fields;
		}

		//--------------------------------------------------------------------------    
		public void GetNewSosVariableValues(bool isOracleValue)
		{
			// DateTime oldDateTime;
			// vedi caso Registri Iva, LIbri giornale, Cespiti, Comunicazione intento (per il SOSConnector), gli allegati sono di una procedura batch e non di un dataentry
			// per cui non riesco a recuperare i valori originali
			// cerco di convertire la data (inserita sempre in formato italiano)
			string oldValue;
			// onde evitare incovenienti dovuti ai formattatori, la data sarà sempre memorizzata come stringa con formato fisso: YYYYMMDDHHMMSS
			char[] delimier = { '/' };

			try
			{
				for (int i = 0; i < SearchIndexField.Count; i++)
				{
					SearchFieldIndex field = SearchIndexField[i];
					if (string.Compare(field.ValueType, "date", true) == 0)
					{
                        oldValue = field.OldFieldValue.Replace(" ", string.Empty);
						List<string> values = oldValue.Split(delimier, StringSplitOptions.RemoveEmptyEntries).ToList();
						string year = (values[2].Length == 3) ? '2' + values[2] : (values[2].Length == 2) ? "20" + values[2] : values[2];
						DateTime dateTime = new DateTime(int.Parse(year), int.Parse(values[1]), int.Parse(values[0]));
						field.NewFieldValue = GetEAStringValue(dateTime, "date", isOracleValue);
					}
				}
			}
			catch (Exception e)
			{
				throw (e);
			}          
		}

		//--------------------------------------------------------------------------    
		public void GetNewKeyBindingKeyValues(TBConnection erpConnection)
		{
			TBCommand erpSelectCommand = null;
			IDataReader sdr = null;
			
			try
			{
				erpSelectCommand = new TBCommand(erpConnection);
				erpSelectCommand.CommandText = GetSelectText();

				// recupero i nuovi valori
				sdr = erpSelectCommand.ExecuteReader();
				
				if (sdr.Read())
				{
					//onde evitare incovenienti dovuti ai formattatori, la data sarà sempre memorizzata come stringa con formato fisso: YYYYMMDDHHMMSS
                    for (int i = 0; i < SearchIndexField.Count; i++)
                    {
                        SearchFieldIndex field = SearchIndexField[i];

                        if (field.SearchField.IsEnumType)
                        {
                            field.SearchField.SetEnumType(erpConnection.IsOracleConnection() ? (int)(decimal)sdr[i] : (int)sdr[i]);
                            field.NewFieldValue = GetEAStringValue(sdr[i], field.SearchField.NewEnumType, erpConnection.IsOracleConnection());
                        }

                        else
                        {
                            if (field.SearchField.ValueType == DataType.DataTypeStrings.Bool)
                            {
                                Boolean boolField;
                                boolField = (sdr[i].ToString() == "1");
                                field.NewFieldValue = GetEAStringValue(boolField, DataType.DataTypeStrings.Bool, erpConnection.IsOracleConnection());
                            }
                            else
                                field.NewFieldValue = GetEAStringValue(sdr[i], field.ValueType, erpConnection.IsOracleConnection());
                        }
                    }
				}
			}
			catch (TBException e)
			{
				throw (e);
			}
			catch (Exception e)
			{
				throw (e);
			}
			finally
			{
				if (sdr != null)
				{
					sdr.Close();
					sdr.Dispose();
				}
				erpSelectCommand.Dispose();
			}
		}

		//---------------------------------------------------------------------
		private string GetEAStringValue(object erpValue, string valueType, bool isOracleValue)
		{
			string stringValue = string.Empty;
            try
            {
                if (string.Compare(valueType, DataType.DataTypeStrings.String, true) == 0) return SoapTypes.ToSoapString((string)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Integer, true) == 0) return SoapTypes.ToSoapShort((short)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Long, true) == 0) return SoapTypes.ToSoapInt(isOracleValue ? (int)(decimal)erpValue : (int)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Double, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Money, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Quantity, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Percent, true) == 0) return SoapTypes.ToSoapDouble((double)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Bool, true) == 0) return SoapTypes.ToSoapBoolean((bool)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Uuid, true) == 0) return SoapTypes.ToSoapGuid((Guid)erpValue);
                else if (string.Compare(valueType, DataType.DataTypeStrings.Date, true) == 0)
                {
                    DateTime erpDate = (DateTime)erpValue;
                    return (erpDate.Year == 1799 && erpDate.Month == 12 && erpDate.Day == 31) ? string.Empty : SoapTypes.ToSoapDate(erpDate);
                }
                else
                    if (
                        (string.Compare(valueType, DataType.DataTypeStrings.Time, true) == 0) ||
                        (string.Compare(valueType, DataType.DataTypeStrings.DateTime, true) == 0)
                        )
                    {
                        DateTime erpDateTime = (DateTime)erpValue;
                        return (erpDateTime.Year == 1799 && erpDateTime.Month == 12 && erpDateTime.Day == 31) ? string.Empty : SoapTypes.ToSoapDateTime(erpDateTime);
                    }
                    else if (string.Compare(valueType, DataType.DataTypeStrings.ElapsedTime, true) == 0) return SoapTypes.ToSoapInt((int)erpValue);
                    else if (string.Compare(valueType, DataType.DataTypeStrings.Enum, true) == 0) return SoapTypes.ToSoapDataEnum(new DataEnum(Convert.ToUInt32(erpValue)));
                    else if (string.Compare(valueType, DataType.DataTypeStrings.Text, true) == 0) return SoapTypes.ToSoapString((string)erpValue);
                    else if (valueType.StartsWith(DataType.DataTypeStrings.EnumType)) return SoapTypes.ToSoapDataEnum(new DataEnum(Convert.ToUInt32(erpValue)));
            }
            catch (Exception e)
            {
                throw (e);
            }
			return stringValue;
		}
	}

	//================================================================================
	internal class SearchField
	{
		private string valueType = string.Empty;
		public string FieldName = string.Empty;
		public string NewEnumType = string.Empty;
		public bool IsEnumType = false;

		private Enums enumsTable = null;

		//--------------------------------------------------------------------------    
		public SearchField(string fieldName, Enums enums)
		{
			FieldName = fieldName;
			enumsTable = enums;
		}

		//--------------------------------------------------------------------------    
		public string ValueType
		{
			get { return valueType; }

			set
			{
				valueType = value;
				IsEnumType = !
					(
						(string.Compare(value, DataType.DataTypeStrings.String, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Integer, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Long, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Double, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Money, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Quantity, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Percent, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Bool, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Uuid, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Date, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Time, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.DateTime, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.ElapsedTime, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Enum, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Text, true) == 0) ||
						(string.Compare(value, DataType.DataTypeStrings.Blob, true) == 0)
				  );
			}
		}

		//--------------------------------------------------------------------------    
		public void SetEnumType(int enumValue)
		{
			if (string.IsNullOrEmpty(NewEnumType))
			{
				EnumTag enumTag = enumsTable.GetTagByStoredValue(enumValue);
				if (enumTag != null)
					NewEnumType = "10:" + enumTag.Value.ToString();
			}
		}
	}

	//================================================================================
	internal class SearchFieldList
	{
		public List<SearchField> SearchFields = null;

		Enums enums = null;

		//--------------------------------------------------------------------------    
		public SearchFieldList(Enums tableEnums)
		{
			SearchFields = new List<SearchField>();
			enums = tableEnums;
		}

		//--------------------------------------------------------------------------    
		public SearchField GetSearchField(string fieldName, string valueType)
		{
			SearchField searchField = SearchFields.Find(a => string.Compare(a.FieldName, fieldName) == 0);

			if (searchField == null)
			{
				searchField = new SearchField(fieldName, enums);
				searchField.ValueType = valueType;
				SearchFields.Add(searchField);
			}
			return searchField;
		}

		//--------------------------------------------------------------------------    
		public void UpdateEnumValueType(TBCommand dmsUpdateCommand)
		{
			try
			{
				dmsUpdateCommand.CommandText = "UPDATE [DMS_Field] SET [ValueType] = @valueType WHERE [FieldName] = @fieldName";
				dmsUpdateCommand.Parameters.Add("@valueType", SqlDbType.VarChar);
				dmsUpdateCommand.Parameters.Add("@fieldName", SqlDbType.VarChar);

				foreach (SearchField searchField in SearchFields)
				{
					if (searchField.IsEnumType)
					{
						dmsUpdateCommand.Parameters.GetParameterAt(0).Value = searchField.NewEnumType;
						dmsUpdateCommand.Parameters.GetParameterAt(1).Value = searchField.FieldName;
						dmsUpdateCommand.ExecuteNonQuery();
					}
				}
			}
			catch (TBException e)
			{
				throw (e);
			}
			catch (Exception e)
			{
				throw (e);
			}
			finally
			{
				dmsUpdateCommand.Dispose();
			}
		}
	}

	//================================================================================
	internal class SearchFieldIndex
	{
		public SearchField SearchField = null;

		public string ErpFieldName = string.Empty;
		public string ErpTableName = string.Empty;

        public string OldFieldValue = string.Empty;
		public string NewFieldValue = string.Empty;

		public int SearchIndexID;

		//--------------------------------------------------------------------------------
		public string FieldName { get { return SearchField.FieldName; } }
		//--------------------------------------------------------------------------------
		public string ValueType { get { return SearchField.ValueType; } }
		//--------------------------------------------------------------------------    
		public string PhysicalName
		{
			set
			{
				if (string.IsNullOrEmpty(value))
					return;
				int nPos = value.IndexOf('.');
				if (nPos == -1)
					ErpFieldName = value;
				else
				{
					ErpTableName = value.Substring(0, nPos);
					ErpFieldName = value.Substring(nPos + 1, (value.Length - (nPos + 1)));
				}
			}
		}

		//--------------------------------------------------------------------------    
		public SearchFieldIndex(int searchIndexID)
		{
			SearchIndexID = searchIndexID;
		}
	}

	//================================================================================
	internal class CollectionFields
	{
		public int CollectionID = -1;
		public string curPrimaryKeyValue = string.Empty;

		public SingleERPDocumentInstance currErpDocumentInstance = null;
		public List<SingleERPDocumentInstance> ErpDocumentInstanceList = new List<SingleERPDocumentInstance>();

		//--------------------------------------------------------------------------    
		public CollectionFields(int collectionID)
		{
			CollectionID = collectionID;
		}

		//--------------------------------------------------------------------------    
        public SingleERPDocumentInstance GetERPDocumentInstance(string primaryKeyValue, string docNamespace, List<SearchFieldIndex> searchFieldsList)
        {
			if (currErpDocumentInstance != null && string.Compare(currErpDocumentInstance.PrimaryKeyValue, primaryKeyValue) != 0)
				currErpDocumentInstance = ErpDocumentInstanceList.Find(a => string.Compare(a.PrimaryKeyValue, primaryKeyValue) == 0);

			if (currErpDocumentInstance == null)
			{
				currErpDocumentInstance = new SingleERPDocumentInstance(primaryKeyValue, docNamespace);
                currErpDocumentInstance.GlobalSearchFieldsIndex = searchFieldsList;
                ErpDocumentInstanceList.Add(currErpDocumentInstance);
			}
			return currErpDocumentInstance;
		}
	}

	//classe che si occupa di effettuare l'update degli indici
	//================================================================================
	public class DmsUpdater
	{
		private string companyConnectionString = string.Empty;
		private DBMSType companyDBMSType = DBMSType.SQLSERVER;
		private string dmsConnectionString = string.Empty;

		private SearchFieldList searchFieldsList;
		private Enums enums = new Enums();

		//---------------------------------------------------------------
		public DmsUpdater(string companyConnString, DBMSType companyDbType, string dmsConnString)
		{
			companyConnectionString = companyConnString;
			companyDBMSType = companyDbType;
			dmsConnectionString = dmsConnString;
		}

		///<summary>
		/// Entry-point per l'esecuzione dell'aggiornamento dei valori dei fields
		///</summary>
		//---------------------------------------------------------------
		public void UpdateFieldsValue()
		{
			TBConnection dmsConnection = null;
			
			TBCommand dmsUpdateTypeCommand = null;
			TBCommand dmsUpdateCommand = null;
			TBCommand dmsSelectFieldCommand = null;
			TBCommand dmsUpdateAttachSearchIndex = null;
			IDataReader sdr = null;

			try
			{
				dmsConnection = new TBConnection(dmsConnectionString, DBMSType.SQLSERVER);
				dmsConnection.Open();

				TBDatabaseSchema tbSchema = new TBDatabaseSchema(dmsConnection);

				if (!tbSchema.ExistTable(DatabaseLayerConsts.DMS_SearchFieldIndexes))
					return;
				
				DataTable colDataTable = tbSchema.GetTableSchema(DatabaseLayerConsts.DMS_SearchFieldIndexes, false);
				//the physicalname is the key ot the datatable
                DataRow[] rows = colDataTable.Select("ColumnName = 'FormattedValue'");
				if (rows == null || rows.Length == 0)
					return;

			    // Carico gli enumerativi installati
				// servono per ottenere a partire dal valore numerico di un enumerativo il proprio tag di appartenenza
				// questo per poter scrivere il corretto tipo di enumerativo: {10:tagValue}
				enums.LoadXml();
				searchFieldsList = new SearchFieldList(enums);

				// E' necessario discriminare i campi di tipo Key/Binding (ovvero quelli che hanno una corrispondenza con un campo di ERP) da i campi di tipo SOS/Variable per cui non esiste 
				// un campo corrispondente in ERP per i campi di tipo External non possiamo farci niente perchè non si hanno le informazioni necessari per accedervi (solo via runtime con Hotlink). 
				// ma questi campi non danno problema poichè sono di tipo stringa per i campi di tipo Category non è necessario fare niente

				// prima lavoro sui campi di tipo Key/Binding (ovwero FieldGroup = 0/1)
				List<SearchFieldIndex> bindingSearchFieldIndexList = GetSearchIndexesToUpdate(dmsConnection, true, ref searchFieldsList);
				// poi sui campi di tipo Sos/Variable (ovwero FieldGroup = 4/5)
				List<SearchFieldIndex> variableSearchFieldIndexList = GetSearchIndexesToUpdate(dmsConnection, false, ref searchFieldsList);

				if (
					(bindingSearchFieldIndexList == null || bindingSearchFieldIndexList.Count == 0) &&
					(variableSearchFieldIndexList == null || variableSearchFieldIndexList.Count == 0)
					)
					return;

				dmsUpdateTypeCommand = new TBCommand(dmsConnection);
				dmsUpdateCommand = new TBCommand(dmsConnection);
				dmsSelectFieldCommand = new TBCommand(dmsConnection);
				dmsUpdateAttachSearchIndex = new TBCommand(dmsConnection);

				//per prima cosa aggiorno il campo ValueType della tabella DMS_Fields per i soli campi di tipo enumerativo
				searchFieldsList.UpdateEnumValueType(dmsUpdateTypeCommand);

				//query per verificare l'esistenza del nuovo valore da assegnare all'indice
				dmsSelectFieldCommand.CommandText = "SELECT [SearchIndexID] FROM [DMS_SearchFieldIndexes] WHERE [FieldName] = @FieldName AND [FieldValue] = @fieldValue";
				dmsSelectFieldCommand.Parameters.Add("@FieldName", SqlDbType.VarChar);
				dmsSelectFieldCommand.Parameters.Add("@fieldValue", SqlDbType.VarChar);

				//query per modificare il valore dell'indice               
				dmsUpdateCommand.CommandText = "UPDATE [DMS_SearchFieldIndexes] SET [FieldValue] = @fieldValue WHERE [SearchIndexID] = @searchField";
				dmsUpdateCommand.Parameters.Add("@fieldValue", SqlDbType.VarChar);
				dmsUpdateCommand.Parameters.Add("@searchField", SqlDbType.Int);

				//query per modificare la tabella DMS_AttachmentsIndexes              
				dmsUpdateAttachSearchIndex.CommandText = "UPDATE [DMS_AttachmentSearchIndexes] SET [SearchIndexID] = @newIndexID WHERE [SearchIndexID] = @oldIndexID";
				dmsUpdateAttachSearchIndex.Parameters.Add("@newIndexID", SqlDbType.Int);
				dmsUpdateAttachSearchIndex.Parameters.Add("@oldIndexID", SqlDbType.Int);

				UpdateSearchFieldValues(dmsUpdateCommand, dmsSelectFieldCommand, dmsUpdateAttachSearchIndex, bindingSearchFieldIndexList);
				UpdateSearchFieldValues(dmsUpdateCommand, dmsSelectFieldCommand, dmsUpdateAttachSearchIndex, variableSearchFieldIndexList);
			}
			catch (TBException e)
			{
				throw (e);
			}
			catch (Exception e)
			{
				throw (e);
			}
			finally
			{
				if (sdr != null && !sdr.IsClosed)
				{
					sdr.Close();
					sdr.Dispose();
				}
				if (dmsSelectFieldCommand != null) dmsSelectFieldCommand.Dispose();
				if (dmsUpdateTypeCommand != null) dmsUpdateTypeCommand.Dispose();
				if (dmsUpdateCommand != null) dmsUpdateCommand.Dispose();
				if (dmsUpdateAttachSearchIndex != null) dmsUpdateAttachSearchIndex.Dispose();
				if (dmsConnection != null && dmsConnection.State == ConnectionState.Open)
				{
					dmsConnection.Close();
					dmsConnection.Dispose();
				}
			}
		}

		//---------------------------------------------------------------
		private void ValorizeSosVariableIndexes(List<CollectionFields> collectionFieldsList)
		{
			//mi faccio dare da ERP il valore corretto delle date
			foreach (CollectionFields collectionFields in collectionFieldsList)
				foreach (SingleERPDocumentInstance erpInstance in collectionFields.ErpDocumentInstanceList)
					erpInstance.GetNewSosVariableValues(this.companyDBMSType == DBMSType.ORACLE);
		}

		//---------------------------------------------------------------
		private void ValorizeBindingKeyIndexes(List<CollectionFields> collectionFieldsList)
		{
			TBConnection erpConnection = null;
			try
			{
				erpConnection = new TBConnection(companyConnectionString, companyDBMSType);
				erpConnection.Open();

				//mi faccio dare da ERP il valore corretto delle date
				foreach (CollectionFields collectionFields in collectionFieldsList)
					foreach (SingleERPDocumentInstance erpInstance in collectionFields.ErpDocumentInstanceList)
						erpInstance.GetNewKeyBindingKeyValues(erpConnection);
			}
			catch (TBException e)
			{
				throw (e);
			}
			catch (Exception e)
			{
				throw (e);
			}
			finally
			{
				if (erpConnection != null && erpConnection.State == ConnectionState.Open)
				{
					erpConnection.Close();
					erpConnection.Dispose();
				}
			}
		}

		//---------------------------------------------------------------
		private List<SearchFieldIndex> GetSearchIndexesToUpdate(TBConnection dmsConnection, bool bindingKey, ref SearchFieldList searchFieldsList)
		{
			TBCommand tbCommand = null;
			IDataReader sdr = null;

			List<CollectionFields> collectionFieldsList = new List<CollectionFields>();
			SearchFieldIndex searchFieldIndex = new SearchFieldIndex(-1);
			CollectionFields currCollectionFields = null;
			SingleERPDocumentInstance erpDocumentInstance = null;
			List<SearchFieldIndex> searchFieldIndexList = new List<SearchFieldIndex>();

			string primaryKeyValue = string.Empty;
			int collectionID = -1;
			int currFieldIndexID = -1;
			int fieldIndexID = -1;

			try
			{
				//leggo tutti gli indici di tipo data per cui è necessario riscrivere il valore
				tbCommand = new TBCommand(dmsConnection);
				tbCommand.CommandText = string.Format
					(
					@"SELECT c.CollectionID, i.SearchIndexID, e.ErpDocumentID, e.PrimaryKeyValue, i.FieldName, e.Docnamespace, i.FieldValue, c.PhysicalName, f.ValueType 
					FROM DMS_ErpDocument e, DMS_Attachment a, DMS_AttachmentSearchIndexes s, DMS_SearchFieldIndexes i, DMS_Field f, DMS_CollectionsFields c
                    WHERE f.ValueType <>'String' AND ({0}) AND f.FieldName = c.FieldName AND c.CollectionID = a.CollectionID AND c.FieldName = f.FieldName AND 
					i.FieldName = f.FieldName AND i.SearchIndexID = s.SearchIndexID AND s.AttachmentID = a.AttachmentID AND a.ErpDocumentID = e.ErpDocumentID 
                    ORDER BY c.CollectionID, i.SearchIndexID, ErpDocumentID",
					bindingKey ? "c.FieldGroup = 0 OR c.FieldGroup = 1" : "c.FieldGroup = 4 OR c.FieldGroup = 5"
					);

				sdr = tbCommand.ExecuteReader();

				while (sdr.Read())
				{
					collectionID = (int)sdr["CollectionID"];
					fieldIndexID = (int)sdr["SearchIndexID"];

					if (currCollectionFields == null || currCollectionFields.CollectionID != collectionID)
					{
						currCollectionFields = new CollectionFields(collectionID);
						collectionFieldsList.Add(currCollectionFields);
					}

					if (currFieldIndexID != fieldIndexID)
					{
						currFieldIndexID = fieldIndexID;
						searchFieldIndex = new SearchFieldIndex(fieldIndexID);
						searchFieldIndex.SearchField = searchFieldsList.GetSearchField(sdr["FieldName"].ToString(), sdr["ValueType"].ToString());

						searchFieldIndex.PhysicalName = sdr["PhysicalName"].ToString();
                        searchFieldIndex.NewFieldValue = searchFieldIndex.OldFieldValue = sdr["FieldValue"].ToString();


						searchFieldIndexList.Add(searchFieldIndex);
                        erpDocumentInstance = currCollectionFields.GetERPDocumentInstance(sdr["PrimaryKeyValue"].ToString(), sdr["DocNamespace"].ToString(), searchFieldIndexList);
                        erpDocumentInstance.AddSearchField(searchFieldIndex);
					}
				}
				sdr.Close();

				if (bindingKey)
					ValorizeBindingKeyIndexes(collectionFieldsList);
				else
					ValorizeSosVariableIndexes(collectionFieldsList);
			}
			catch (TBException e)
			{
				Debug.Assert(false);
				Debug.Write(e.Message);
				throw (e);
			}
			catch (Exception e)
			{
				Debug.Assert(false);
				Debug.Write(e.Message);
				throw (e);
			}
			finally
			{
				if (sdr != null && !sdr.IsClosed)
				{
					sdr.Close();
					sdr.Dispose();
				}
				tbCommand.Dispose();
			}

			return searchFieldIndexList;
		}

		//---------------------------------------------------------------
		private void UpdateSearchFieldValues(TBCommand dmsUpdateCommand, TBCommand dmsSelectFieldCommand, TBCommand dmsUpdateAttachSearchIndex, List<SearchFieldIndex> searchFieldIndexList)
		{
			try
			{
				foreach (SearchFieldIndex searchFieldIndex in searchFieldIndexList)
				{
					if (string.IsNullOrEmpty(searchFieldIndex.NewFieldValue))
						continue;

					//prima verifico se esiste o meno l'indice con la data modificata, se esiste uso quell'indice e vado ad aggiornare la tabella DMS_AttachmentSearchIndexes
					dmsSelectFieldCommand.Parameters.GetParameterAt(0).Value = searchFieldIndex.FieldName;
					dmsSelectFieldCommand.Parameters.GetParameterAt(1).Value = searchFieldIndex.NewFieldValue;

					object obj = dmsSelectFieldCommand.ExecuteScalar();
					int existingID = (obj != null) ? (int)obj : -1;
					//esiste già lo stesso valore allora considero questo SearchIndexID e lo associo agli AttachementID che erano associati all'indice da modificare
					if (existingID > -1)
					{
						dmsUpdateAttachSearchIndex.Parameters.GetParameterAt(0).Value = existingID;
						dmsUpdateAttachSearchIndex.Parameters.GetParameterAt(1).Value = searchFieldIndex.SearchIndexID;
						dmsUpdateAttachSearchIndex.ExecuteNonQuery();
					}
					else //aggiorno il valore dell'indice
					{
						dmsUpdateCommand.Parameters.GetParameterAt(0).Value = searchFieldIndex.NewFieldValue;
						dmsUpdateCommand.Parameters.GetParameterAt(1).Value = searchFieldIndex.SearchIndexID;
						dmsUpdateCommand.ExecuteNonQuery();
					}
				}
			}
			catch (TBException e)
			{
				throw (e);
			}
			catch (Exception e)
			{
				throw (e);
			}
		}
	}
}