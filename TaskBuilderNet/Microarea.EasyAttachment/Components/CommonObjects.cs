using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

using Microarea.EasyAttachment.BusinessLogic;
using Microarea.EasyAttachment.Core;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;

namespace Microarea.EasyAttachment.Components
{
    //================================================================================
    public class FieldData
    {
        private MDataObj dataObj;
        public object DataValue { set { dataObj.Value = value; } get { return dataObj.Value; } }
        public string FormattedValue { get { return dataObj.FormatData(); } }
        public string StringValue { set { dataObj.StringValue = value; } get { return GetEAStringValue(); } }
        public DataType ValueType { get { return dataObj.DataType; } }
        public MDataObj DataObj { get { return dataObj; } }
        
        //---------------------------------------------------------------------------------
        public FieldData(string valueType)
        {
            DataType dataType = DataType.StringToDataType(valueType);
            dataObj = MDataObj.Create(dataType);
        }

        //---------------------------------------------------------------------------------
        public FieldData(DataType dataType)
        {
            dataObj = MDataObj.Create(dataType);
        }

        //---------------------------------------------------------------------------------
        public FieldData(MDataObj mDataObj)
        {
            dataObj = MDataObj.Create(mDataObj.DataType);
            dataObj.Value = mDataObj.Value;
        }

        //---------------------------------------------------------------------------------
        public FieldData(FieldData fieldData)
        {
            dataObj = MDataObj.Create(fieldData.ValueType);
            dataObj.Value = fieldData.DataValue;
        }

         //---------------------------------------------------------------------------------
        public void AssignValue(MDataObj mDataObj)
        {
            DataValue = mDataObj.Value;
        }

        //---------------------------------------------------------------------------------
        private string GetEAStringValue()
        {
            //se è di tipo data voglio la versione YYYY-MM-DD e non il formato SOAP YYYY-MM-DDTHH:MM:SS 
            //se il tipo invece è double o derivati da double allora mi voglio fermare ai 7 decimali e non 15 come prevede il formato SOAP
            return (ValueType == DataType.Date || ValueType == DataType.Double || ValueType == DataType.Money || ValueType == DataType.Percent || ValueType == DataType.Quantity)
                  ? dataObj.StringNoSoapValue
                  : dataObj.StringValue;
        }     

    }


    //================================================================================
    public class BookmarksDataTable : DataTable
    {
        public enum DisableFilter
        {
            Both,
            OnlyDisable,
            OnlyEnable
        }

		//---------------------------------------------------------------------------------------------------
		public BookmarksDataTable()
		{
			Columns.Add(new DataColumn(CommonStrings.Selected, typeof(System.Boolean)));
			Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.FieldDescription, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.FieldData, typeof(FieldData))); //BugFix #
            Columns.Add(new DataColumn(CommonStrings.FormattedValue, typeof(string)));
            Columns.Add(new DataColumn(CommonStrings.ValueSet, typeof(System.Object)));
			Columns.Add(new DataColumn(CommonStrings.ControlName, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.PhysicalName, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.OCRPosition, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.ValueType, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.FieldGroup, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.ShowAsDescription, typeof(System.Boolean)));
			Columns.Add(new DataColumn(CommonStrings.HotKeyLink, typeof(System.Object)));
			Columns.Add(new DataColumn(CommonStrings.SosMandatory, typeof(System.Boolean)));
			Columns.Add(new DataColumn(CommonStrings.SosPosition, typeof(int)));
            Columns.Add(new DataColumn(CommonStrings.SosKeyCode, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.Disable, typeof(System.Boolean)));

            //the physicalname is the key ot the datatable
            DataColumn[] keys = new DataColumn[1];
            keys[0] = Columns[CommonStrings.Name];
            PrimaryKey = keys;
            Columns[CommonStrings.Selected].DefaultValue = false;
            Columns[CommonStrings.ShowAsDescription].DefaultValue = false;
            Columns[CommonStrings.FieldGroup].DefaultValue = FieldGroup.Category;
			Columns[CommonStrings.SosPosition].DefaultValue = 0;
			Columns[CommonStrings.SosMandatory].DefaultValue = false;			
			Columns[CommonStrings.HotKeyLink].DefaultValue = null;
            Columns[CommonStrings.SosKeyCode].DefaultValue = string.Empty;
			Columns[CommonStrings.Disable].DefaultValue = false;
        }

       	//---------------------------------------------------------------------
		public void Fill(IQueryable<DMS_CollectionsField> fields, DMSDocOrchestrator dmsOrch = null)
		{
            if (Rows.Count > 0)
                Clear();

			foreach (DMS_CollectionsField collField in fields)
			{
				if (string.IsNullOrWhiteSpace(collField.DMS_Field.FieldName))
					continue;

				// se la categoria e' disabilitata non metto la riga
				if ((FieldGroup)collField.FieldGroup == FieldGroup.Category &&
					collField.DMS_Field.DMS_FieldProperty != null &&
					collField.DMS_Field.DMS_FieldProperty.Disabled == true)
					continue;

				DataRow row = NewRow();
				row[CommonStrings.ValueType] = collField.DMS_Field.ValueType;

				row[CommonStrings.FieldDescription] = ((FieldGroup)collField.FieldGroup != FieldGroup.Category)
					? collField.DMS_Field.FieldDescription
					: collField.DMS_Field.FieldName;

				row[CommonStrings.Name] = collField.DMS_Field.FieldName;
				row[CommonStrings.PhysicalName] = collField.PhysicalName;
				row[CommonStrings.OCRPosition] = collField.OCRPosition;
				row[CommonStrings.ControlName] = collField.ControlName;
				row[CommonStrings.FieldGroup] = (FieldGroup)collField.FieldGroup;
				row[CommonStrings.ShowAsDescription] = collField.ShowAsDescription;
                row[CommonStrings.ValueSet] = new List<FieldData>();
                row[CommonStrings.FieldData] = new FieldData(collField.DMS_Field.ValueType);


				//improvement #5062: SOSConnector
                if (dmsOrch != null)
                {
                    row[CommonStrings.HotKeyLink] = (!string.IsNullOrEmpty(collField.HKLName)) ? dmsOrch.Document.GetHotLink(collField.HKLName) : null;
                    row[CommonStrings.SosMandatory] = (collField.SosMandatory == null) ? false : collField.SosMandatory;
                    row[CommonStrings.SosPosition] = collField.SosPosition;
                    row[CommonStrings.SosKeyCode] = collField.SosKeyCode; //impr. #5305
                }
				Rows.Add(row);
			}
		}


		//---------------------------------------------------------------------
		public void AddXMLSearchBookmark(MXMLSearchBookmark xmlSearchBookmark, DMSDocOrchestrator dmsOrchestrator)
		{
			FieldGroup group = FieldGroup.Binding;
			MSqlRecordItem recItem = null;
			DataRow dataRow = null;
			string tableName = dmsOrchestrator.MasterRecord.Name; ;
			DataRow rowFound = Rows.Find(xmlSearchBookmark.FieldName);
			if (!string.IsNullOrEmpty(xmlSearchBookmark.HKLName))
			{		
				if  (rowFound != null)
					return;

				MHotLink hklField = dmsOrchestrator.Document.GetHotLink(xmlSearchBookmark.HKLName);
				if (hklField != null && hklField.Record != null)
				{
						dataRow = NewRow();
						recItem = (MSqlRecordItem)hklField.Record.GetField(xmlSearchBookmark.FieldName);
						group = FieldGroup.External;
						tableName = hklField.TableName;
						dataRow[CommonStrings.HotKeyLink] = hklField;
				}				
			}
			else
			{
                if (rowFound != null)
                {
                    if (xmlSearchBookmark.ShowAsDescription && !(bool)rowFound[CommonStrings.ShowAsDescription])
					{
						rowFound[CommonStrings.ShowAsDescription] = true;
						if (rowFound.RowState == DataRowState.Unchanged)
						    rowFound.SetModified();
					}
                    if (!string.IsNullOrEmpty(xmlSearchBookmark.KeyCode))
                    {
                        rowFound[CommonStrings.SosKeyCode] = xmlSearchBookmark.KeyCode;
                        if (rowFound.RowState == DataRowState.Unchanged)
                            rowFound.SetModified();
                    }
                    return;
                }
				
				recItem = (MSqlRecordItem)dmsOrchestrator.MasterRecord.GetField(xmlSearchBookmark.FieldName);
                if (recItem != null)
                {
                    dataRow = NewRow();
                    group = (recItem.IsSegmentKey || xmlSearchBookmark.ShowAsDescription) ? FieldGroup.Key : FieldGroup.Binding;
                }
			}

			if (recItem == null)
				return;

			dataRow[CommonStrings.Name] = recItem.Name;
			dataRow[CommonStrings.FieldGroup] = group;
			dataRow[CommonStrings.FieldDescription] = recItem.LocalizableName;
			dataRow[CommonStrings.PhysicalName] = string.Format("{0}.{1}", tableName, recItem.Name);
            DataType dataType = recItem.DataObjType;
            dataRow[CommonStrings.ValueType] = dataType.DataTypeToString();

            dataRow[CommonStrings.FieldData] = new FieldData(dataType);
            ((FieldData)dataRow[CommonStrings.FieldData]).DataValue = ((MDataObj)recItem.DataObj).Value;

            List<FieldData> values = new List<FieldData>();
            values.Add((FieldData)dataRow[CommonStrings.FieldData]);
            dataRow[CommonStrings.ValueSet] = values;

			dataRow[CommonStrings.ValueType] = recItem.DataObjType.ToString();
			dataRow[CommonStrings.ShowAsDescription] = xmlSearchBookmark.ShowAsDescription;
            dataRow[CommonStrings.SosKeyCode] = (string.IsNullOrEmpty(xmlSearchBookmark.KeyCode)) ? recItem.Name  : xmlSearchBookmark.KeyCode;
			Rows.Add(dataRow);
		}

        
		//---------------------------------------------------------------------
		public DataRow AddBindingField(MSqlRecordItem recItem, bool showAsDescription)
		{
			DataRow dataRow = NewRow();
            if (recItem == null)
                return dataRow;

			dataRow[CommonStrings.FieldGroup] = (recItem.IsSegmentKey || showAsDescription) ? FieldGroup.Key : FieldGroup.Binding;
			dataRow[CommonStrings.Name] = recItem.Name;
			dataRow[CommonStrings.FieldDescription] = recItem.LocalizableName;

            DataType dataType = recItem.DataObjType;
            dataRow[CommonStrings.ValueType] = dataType.DataTypeToString();

            dataRow[CommonStrings.FieldData] = new FieldData(dataType);
            ((FieldData)dataRow[CommonStrings.FieldData]).DataValue = ((MDataObj)recItem.DataObj).Value;

            List<FieldData> values = new List<FieldData>();
            values.Add((FieldData)dataRow[CommonStrings.FieldData]);
            dataRow[CommonStrings.ValueSet] = values;
			
            dataRow[CommonStrings.PhysicalName] = string.Format("{0}.{1}", recItem.Record.Name, recItem.Name);
			dataRow[CommonStrings.ShowAsDescription] = showAsDescription;
            dataRow[CommonStrings.SosKeyCode] = recItem.Name;

			Rows.Add(dataRow);
			
			return dataRow;
		}

        //---------------------------------------------------------------------
        public DataRow AddBindingField(string fieldName, DMSOrchestrator dmsOrchestrator)
        {
            MSqlRecordItem recItem = (dmsOrchestrator.MasterRecord != null) ? (MSqlRecordItem)dmsOrchestrator.MasterRecord.GetField(fieldName) : null;
            return AddBindingField(recItem, false);             
        }


        //---------------------------------------------------------------------
        public DataRow AddBindingField(MXMLVariable xmlVar)
        {
			DataRow dataRow = NewRow();
            dataRow[CommonStrings.FieldGroup] = FieldGroup.Variable;
            dataRow[CommonStrings.Name] = xmlVar.Name;
            dataRow[CommonStrings.FieldDescription] = xmlVar.Name;
            DataType dataType = xmlVar.DataObj.DataType;
            dataRow[CommonStrings.ValueType] = dataType.DataTypeToString();

            dataRow[CommonStrings.FieldData] = new FieldData(dataType);
            ((FieldData)dataRow[CommonStrings.FieldData]).DataValue = ((MDataObj)xmlVar.DataObj).Value;

            List<FieldData> values = new List<FieldData>();
            values.Add((FieldData)dataRow[CommonStrings.FieldData]);
            dataRow[CommonStrings.ValueSet] = values;
            
            dataRow[CommonStrings.PhysicalName] = xmlVar.Name;
            dataRow[CommonStrings.ShowAsDescription] = true;
            dataRow[CommonStrings.SosKeyCode] = xmlVar.Name;
            Rows.Add(dataRow);
            return dataRow;
        }

        //---------------------------------------------------------------------
        public DataRow AddCategory(string name, string value)
        {
			DataRow dataRow = NewRow();
            dataRow[CommonStrings.FieldGroup] = FieldGroup.Category;
            dataRow[CommonStrings.Name] = name;
            dataRow[CommonStrings.FieldDescription] = name;
            dataRow[CommonStrings.FieldData] = new FieldData(DataType.String);
            ((FieldData)dataRow[CommonStrings.FieldData]).StringValue = value;
            dataRow[CommonStrings.FormattedValue] = value;
            Rows.Add(dataRow);
            return dataRow;
        }

		//---------------------------------------------------------------------
		public DataRow AddSOSSpecialField(string fieldName, DMSDocOrchestrator dmsOrchestrator)
		{
            DataRow row = Rows.Find(fieldName);
            if (row!= null)
                return row;     
		
			DataRow dataRow = NewRow();
			dataRow[CommonStrings.FieldGroup] = FieldGroup.SosSpecial;
			dataRow[CommonStrings.Name] = fieldName;
			dataRow[CommonStrings.PhysicalName] = fieldName;
			dataRow[CommonStrings.ShowAsDescription] = false;
			dataRow[CommonStrings.SosPosition] = 0;
			dataRow[CommonStrings.SosMandatory] = true;
            dataRow[CommonStrings.SosKeyCode] = fieldName;

			//se campo =  FiscalYear per il valore devo chiamare la funzione virtuale del documento specifica del campo
			if (string.Compare(fieldName, CommonStrings.FiscalYear, true) == 0)
			{
				dataRow[CommonStrings.FieldDescription] = "Anno fiscale";

				//il FiscalYear è un DataInt
				int year = dmsOrchestrator.Document.GetFiscalYear();
				Debug.Assert(year != DateTimeFunctions.MinYear);
                dataRow[CommonStrings.ValueType] = DataType.Integer.DataTypeToString();
                dataRow[CommonStrings.FieldData] = new FieldData(DataType.Integer);
                ((FieldData)dataRow[CommonStrings.FieldData]).StringValue = year.ToString();
			}
			else
			{
				if (string.Compare(fieldName, CommonStrings.Suffix, true) == 0)
				{
					dataRow[CommonStrings.FieldDescription] = "Suffisso";
					//il Suffix è un DataStr
					MDataStr suffix = new MDataStr();
                    suffix.Value = dmsOrchestrator.Document.GetSosSuffix(); 
                    dataRow[CommonStrings.ValueType] = DataType.String.DataTypeToString();
                    dataRow[CommonStrings.FieldData] = new FieldData(DataType.String);
                    ((FieldData)dataRow[CommonStrings.FieldData]).DataValue = suffix;
				}
				else
				{
                    if (string.Compare(fieldName, CommonStrings.SOSDocumentType, true) == 0)
					{
						dataRow[CommonStrings.FieldDescription] = "Tipo Documento";
						MDataStr docType = new MDataStr();
                        docType.Value = dmsOrchestrator.GetSOSDocumentType(); 
                        dataRow[CommonStrings.ValueType] = DataType.String.DataTypeToString();
                        dataRow[CommonStrings.FieldData] = new FieldData(DataType.String);
                        ((FieldData)dataRow[CommonStrings.FieldData]).DataValue = docType;
					}            
				}
			}

            List<FieldData> values = new List<FieldData>();
            values.Add((FieldData)dataRow[CommonStrings.FieldData]);
            dataRow[CommonStrings.ValueSet] = values;

			Rows.Add(dataRow);

			return dataRow;
		}


        //---------------------------------------------------------------------
		public DataRow FindBySosKeyCode(string keyCode)
        {
            foreach (DataRow row in Rows)            
                if (string.Compare(row[CommonStrings.SosKeyCode].ToString(), keyCode, true) == 0)
                    return row;
            
            return null;
        }


        //---------------------------------------------------------------------
        public DataRow FindByName(string fieldName)
        {
            foreach (DataRow row in Rows)
                if (string.Compare(row[CommonStrings.Name].ToString(), fieldName, true) == 0)
                    return row;

            return null;
        }
	}

    //================================================================================
    public class SearchResultDataTable : DataTable
    {
        //--------------------------------------------------------------------------------
        public SearchResultDataTable()
        {
            Columns.Add(new DataColumn(CommonStrings.ArchivedDocID, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.AttachmentID, typeof(System.Int32)));
			Columns.Add(new DataColumn(CommonStrings.Collector, typeof(System.Int32)));
			Columns.Add(new DataColumn(CommonStrings.Collection, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.DocNamespace, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.TBPrimaryKey, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.DocKeyDescription, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.AttachmentDate, typeof(System.DateTime)));
            Columns.Add(new DataColumn(CommonStrings.ExtensionType, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.DocStatus, typeof(System.Int32)));
			Columns.Add(new DataColumn(CommonStrings.AttachmentInfo, typeof(System.Object)));
        }		
    }

	//================================================================================
	public class AttachmentsForErpDocument : DataTable
	{
		//--------------------------------------------------------------------------------
		public AttachmentsForErpDocument()
		{
			Columns.Add(new DataColumn(CommonStrings.ArchivedDocID, typeof(System.Int32)));
			Columns.Add(new DataColumn(CommonStrings.AttachmentID, typeof(System.Int32)));
			Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.AttachmentDate, typeof(System.DateTime)));
			Columns.Add(new DataColumn(CommonStrings.ExtensionType, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.AttachmentInfo, typeof(System.Object)));
		}	
	}

    //================================================================================
    public class ArchivedDocDataTable : DataTable
    {
        //--------------------------------------------------------------------------------
        public ArchivedDocDataTable()
        {
            Columns.Add(new DataColumn(CommonStrings.ArchivedDocID, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.Description, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.TBModified, typeof(System.DateTime)));
            Columns.Add(new DataColumn(CommonStrings.TBCreated, typeof(System.DateTime)));
			Columns.Add(new DataColumn(CommonStrings.WorkerName, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.Attached, typeof(bool)));
            Columns.Add(new DataColumn(CommonStrings.ModifierID, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.StorageType, typeof(System.Int32)));
			Columns.Add(new DataColumn(CommonStrings.IsWoormReport, typeof(bool)));
            Columns.Add(new DataColumn(CommonStrings.AttachmentInfo, typeof(System.Object)));
        }
    }

	//================================================================================
	public class CollectorsResultDataTable : DataTable
	{
		//--------------------------------------------------------------------------------
		public CollectorsResultDataTable()
		{
			Columns.Add(new DataColumn(CommonStrings.Collector, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.CollectorID, typeof(System.Int32)));
			Columns.Add(new DataColumn(CommonStrings.Collections, typeof(DMS_Collection[])));
		}
	}


    //================================================================================
    public class CollectionResultDataTable : DataTable
    {
        //--------------------------------------------------------------------------------
        public CollectionResultDataTable()
        {
            Columns.Add(new DataColumn(CommonStrings.CollectorID, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.CollectionID, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.IsStandard, typeof(System.Boolean)));
            Columns.Add(new DataColumn(CommonStrings.DocNamespace, typeof(System.String)));

        }
    }

    //================================================================================
    public class OCRTemplateDataTable : DataTable
	{
		//--------------------------------------------------------------------------------
		public OCRTemplateDataTable()
		{
			Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.Collection, typeof(DMS_Collection)));
			Columns.Add(new DataColumn(CommonStrings.IsNew, typeof(System.Boolean)));
		}
	}

    //================================================================================
    public class SearchResultDocumentDataTable : DataTable
    {
        //--------------------------------------------------------------------------------
        public SearchResultDocumentDataTable()
        {
            Columns.Add(new DataColumn(CommonStrings.ArchivedDocID, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.DocContent, typeof(System.Byte[])));
            Columns.Add(new DataColumn(CommonStrings.ExtensionType, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.Path, typeof(System.String)));
        }
    }

    //================================================================================
    public class SearchFieldsConditionsDataTable : DataTable
    {
        //---------------------------------------------------------------------------------------------------
        public SearchFieldsConditionsDataTable()
        {
            Columns.Add(new DataColumn(CommonStrings.SearchIndexID, typeof(List<Int32>)));
            Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));

            DataColumn[] keys = new DataColumn[1];
            keys[0] = Columns[CommonStrings.Name];
            PrimaryKey = keys;

        }

        //---------------------------------------------------------------------------------------------------
        public bool ValidRow(DataRow row)
        {
            return (row != null 
                   && !string.IsNullOrEmpty(row[CommonStrings.Name].ToString())
                   && row[CommonStrings.SearchIndexID] != null 
                   && ((List<Int32>)row[CommonStrings.SearchIndexID]).Count > 0);
        }

        //---------------------------------------------------------------------------------------------------
        public void AddSearchFilterCondition(string fieldName, int searchIndexID)
        {
            DataRow row = Rows.Find(fieldName);
            if (row == null)
            { 
                row = this.NewRow();
                row[CommonStrings.SearchIndexID] = new List<int>();
                row[CommonStrings.Name] = fieldName;
                Rows.Add(row);
            }
            ((List<int>)row[CommonStrings.SearchIndexID]).Add(searchIndexID);
        }
    }
    //the search can be done using one or more data field. 
    //Each data can have one or more value 
    //================================================================================
    public class SearchFieldsDataTable : DataTable
	{
		//---------------------------------------------------------------------------------------------------
		public SearchFieldsDataTable()
		{
			Columns.Add(new DataColumn(CommonStrings.FieldDescription, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.FieldData, typeof(FieldData)));
            Columns.Add(new DataColumn(CommonStrings.FormattedValue, typeof(string)));
            Columns.Add(new DataColumn(CommonStrings.Count, typeof(System.Int32)));
            Columns.Add(new DataColumn(CommonStrings.ValueSet, typeof(List<FieldData>)));

            Columns[CommonStrings.ValueSet].DefaultValue = null;
		}

		//---------------------------------------------------------------------------------------------------
		public void Fill(BookmarksDataTable fields)
		{
			DataRow fieldRow = null;
			foreach (DataRow row in fields.Rows)
			{
				fieldRow = NewRow();
				fieldRow[CommonStrings.Name] = row[CommonStrings.Name];
				fieldRow[CommonStrings.FieldDescription] = row[CommonStrings.FieldDescription];
                fieldRow[CommonStrings.FieldData] = row[CommonStrings.FieldData];				
				Rows.Add(fieldRow);
			}
		}

		//---------------------------------------------------------------------------------------------------
		public void AddSearchField(string fieldName, string fieldDescription)
		{
			if (!this.AsEnumerable().Any(f => f[CommonStrings.Name].ToString() == fieldName))
			{
				DataRow row = this.NewRow();
				row[CommonStrings.Name] = fieldName;
				row[CommonStrings.FieldDescription] = fieldDescription;
				Rows.Add(row);
			}
		}

        //---------------------------------------------------------------------------------------------------
        public void AddSearchFieldValue(string fieldName, string fieldValue, string fieldType, bool enableDuplicate)
        {
            if (!this.AsEnumerable().Any(f => f[CommonStrings.Name].ToString() == fieldName) || enableDuplicate)
            {
                DataRow row = this.NewRow();
                row[CommonStrings.Name] = fieldName;
                row[CommonStrings.FieldDescription] = fieldName;
                FieldData fieldData = new FieldData(fieldType);
                fieldData.StringValue = fieldValue;
                row[CommonStrings.FieldData] = fieldData;
                Rows.Add(row);
            }
        }        

        //---------------------------------------------------------------------
        public void AddSearchFieldValue(string fieldName, MDataObj dataObj, bool enableDuplicate)
        {
            if (dataObj == null)
                return;

            if (!this.AsEnumerable().Any(f => f[CommonStrings.Name].ToString() == fieldName) || enableDuplicate)
            {
                DataRow dataRow = NewRow();            
                dataRow[CommonStrings.Name] = fieldName;
                dataRow[CommonStrings.FieldDescription] = fieldName;
                FieldData fieldData = new FieldData(dataObj.DataType);
                fieldData.DataValue = dataObj.Value;
                dataRow[CommonStrings.FieldData] = fieldData;
                Rows.Add(dataRow);
            }            
        }
    }

    // DataTable per le categorie
    //================================================================================
    public class DTCategories : DataTable
    {
        //--------------------------------------------------------------------------------
        public DTCategories()
        {
            Columns.Add(new DataColumn(CommonStrings.Name, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.FieldDescription, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.Value, typeof(System.String)));
            Columns.Add(new DataColumn(CommonStrings.ValueSet, typeof(System.Object))); // di tipo DTCategoriesValues
            Columns.Add(new DataColumn(CommonStrings.Disable, typeof(System.Boolean)));
            Columns.Add(new DataColumn(CommonStrings.Color, typeof(System.String)));
        }
    }

	// DataTable per i valori delle categorie
    //================================================================================
    public class DTCategoriesValues : DataTable
    {
        //--------------------------------------------------------------------------------
        public DTCategoriesValues()
        {
			// definisco la colonna in questo modo per assegnare un valore di default, cosi' non devo testare se System.DBNull
			DataColumn isDefaultColumn = new DataColumn();
			isDefaultColumn.DefaultValue = false;
			isDefaultColumn.ColumnName = CommonStrings.IsDefault;
			isDefaultColumn.DataType = typeof(System.Boolean);

			DataColumn valueColumn = new DataColumn();
			valueColumn.DefaultValue = string.Empty;
			valueColumn.ColumnName = CommonStrings.Value;
			valueColumn.DataType = typeof(System.String);

            Columns.Add(new DataColumn(CommonStrings.InUse, typeof(System.Boolean)));
			Columns.Add(isDefaultColumn);
			Columns.Add(valueColumn);

            // il valore della categoria e' chiave primaria nel DataTable
            DataColumn[] keys = new DataColumn[1];
            keys[0] = Columns[CommonStrings.Value];
            PrimaryKey = keys;
        }
    }

	// DataTable per i valori dei barcode
	//================================================================================
	public class DTBarcodes : DataTable
	{
		//--------------------------------------------------------------------------------
		public DTBarcodes()
		{
			// definisco la colonna in questo modo per assegnare un valore di default, cosi' non devo testare se System.DBNull
			DataColumn isSelected = new DataColumn();
			isSelected.DefaultValue = false;
			isSelected.ColumnName = CommonStrings.Selected;
			isSelected.DataType = typeof(System.Boolean);

			DataColumn valueColumn = new DataColumn();
			valueColumn.DefaultValue = string.Empty;
			valueColumn.ColumnName = CommonStrings.Value;
			valueColumn.DataType = typeof(System.String);

			DataColumn barcodeColumn = new DataColumn(CommonStrings.BarcodeTag, typeof(System.Object)); // di tipo Barcode

			Columns.Add(isSelected);
			Columns.Add(valueColumn);
			Columns.Add(barcodeColumn);
		}
	}

	//Security integration
	//================================================================================
	public class ERPDocumentsGrant : DataTable
	{
		//--------------------------------------------------------------------------------
		public ERPDocumentsGrant()
		{
			Columns.Add(new DataColumn(CommonStrings.DocNamespace, typeof(System.String)));
			Columns.Add(new DataColumn(CommonStrings.CanShowAttachments, typeof(System.Boolean)));

			// TBDocNamespace e' chiave primaria nel DataTable
			DataColumn[] keys = new DataColumn[1];
			keys[0] = Columns[CommonStrings.DocNamespace];
			PrimaryKey = keys;
		}		
	}

	//================================================================================
	public class CommonStrings
	{
		public const string EasyAttachmentTitle = "EasyAttachment";
		public const string Name = "Name";
		public const string ValueType = "ValueType";
		public const string Description = "Description";
		public const string FieldDescription = "FieldDescription";
        public const string FieldData = "FieldData";
        public const string FormattedValue = "FormattedValue";        
		public const string Value = "Value";
		public const string ValueSet = "ValueSet";
		public const string ControlName = "ControlName";
		public const string PhysicalName = "PhysicalName";
		public const string OCRPosition = "OCRPosition";
		public const string AttachmentID = "AttachmentID";
		public const string Tags = "Tags";
		public const string AllBookmarks = "AllBookmarks";
		public const string Selected = "Selected";
		public const string Key = "Key";
		public const string Binding = "Binding";
		public const string Category = "Category";
		public const string FieldGroup = "Group";
		public const string MRecord = "MRecord";
		public const string Content = "Content";
		public const string Collector = "Collector";
		public const string CollectorID = "CollectorID";
		public const string Collection = "Collection";
		public const string CollectionID = "CollectionID";
		public const string Everywhere = "Everywhere";
		public const string AttachmentDate = "AttachmentDate";
		public const string ExtensionType = "DocExtensionType";
		public const string DocNamespace = "DocNamespace";
		public const string DocStatus = "DocStatus";
		public const string TBPrimaryKey = "TBPrimaryKey";
		public const string ArchivedDocID = "ArchivedDocID";
        public const string SearchIndexID = "SearchIndexID";        
        public const string Path = "DocPath";
		public const string DocContent = "DocContent";
		public const string Disable = "Disable";
		public const string Color = "Color";
		public const string DefaultTemplate = "Standard";
		public const string AttachmentInfo = "AttachmentInfo";
		public const string Collections = "Collections";
		public const string ShowAsDescription = "ShowAsDescription";
		public const string HotKeyLink = "HotKeyLink";
		public const string DocKeyDescription = "DocKeyDescription";
		public const string RowState = "RowState";
		public const string InUse = "InUse";
		public const string IsDefault = "IsDefault";
        public const string IsStandard = "IsStandard";    
        public const string Values = "Values";
        public const string MostUsedValues = "MostUsedValues";
        public const string Count = "Count";
        public const string TBModified = "TBModified";
        public const string ModifierID = "ModifierID";
        public const string TBModifiedID = "TBModifiedID";
        public const string TBCreated = "TBCreated";
        public const string TBCreatedID = "TBCreatedID";
        public const string Attached = "Attached";
        public const string EACollector = "EasyAttachment";
        public const string EACollection = "Repository";
        public const string AllExtensions = "*.*";
		public const string WorkerName = "WorkerName";
		public const string CanShowAttachments = "ShowAttachments";
		public const string FileNameTag = "FileNameTag";
		public const string DescriptionTag = "DescriptionTag";
		public const string BarcodeTag = "BarcodeTag";
		public const string BarcodeEAPrefix = "EA";
		public const string IsNew = "IsNew";
		public const string Image = "Image";
		public const string Preview = "Preview";
		public const string EAScanPrefix = "EAScan_";
        public const string StorageType = "StorageType";
		public const string IsWoormReport = "IsWoormReport";

		//external fields constant string
		public const string CompanyName = "CompanyName";
		public const string TaxIdNumber = "TaxIdNumber";
		public const string FiscalCode = "FiscalCode";

		// SosConnector costant string
		public const string CodSog = "CodSog";
		public const string FiscalYear = "FiscalYear";
		public const string Suffix = "Suffix";
		public const string TaxJournal = "TaxJournal";
		public const string SOSDocumentType = "SOSDocumentType";
		public const string AllClasses = "ALLCLASSES";
		public const string SosPosition = "SosPosition";
		public const string SosMandatory = "SosMandatory";
		public const string SosKeyCode = "SosKeyCode";
		public const string SosTemplate = "SosConnector";

		// per gestire casistiche diverse quando si fa checkin e replace 
		public const string CheckinSender = "Checkin";
		public const string ReplaceSender = "Replace";
	}

	///<summary>
	/// Enumerativo, ad uso degli oggetti di classe Barcode, che tiene traccia del suo stato
	///</summary>
	//---------------------------------------------------------------------------------------------------
	public enum BarcodeStatus
	{
		OK,
		SyntaxNotValid, // sintassi non corretta: deve iniziare con EA e deve contenere al max 17 chr uppercase
		TypeNotValid, // valore non compatibile con il Code39/Alfa39
		AlreadyExists, // il valore e' gia' stato assegnato ad un altro doc archiviato
		DrawingError // il componente GdPicture ha riscontrato un errore in scrittura del codice a barre
	}

	//---------------------------------------------------------------------------------------------------
	public enum CategoryType
	{
		String,
		Date,
		Color
	}

	//---------------------------------------------------------------------------------------------------
	public enum ErrorCode
	{
		ExistDocument,
		NotExistDocument
	}

	//---------------------------------------------------------------------------------------------------
	public enum FieldGroup
	{
		Key,
		Binding,
		Category,
		External,
		SosSpecial,
        Variable
	}

	//for search algorithm
	//---------------------------------------------------------------------------------------------------
	public enum SearchContext
	{
		Current,
		Collection,
		Collector,
		Repository,
		Custom
	}

	//---------------------------------------------------------------------------------------------------
	public enum SearchLocation
	{
		None = 0x0000,
		All = 0x0001,
		Tags = 0x0002,
		AllBookmarks = 0x0004,
		NameAndDescription = 0x008,
		Barcode = 0x0010,
		Content = 0x0020
	}

	//---------------------------------------------------------------------------------------------------
	public enum SearchFilterType
	{
		SearchContext,
		FreeText,
		DateRange,
		DocType,
		SearchFields
	}

	//the result of duplicate checking procedure
	//---------------------------------------------------------------------------------------------------
	public enum CheckDuplicateResult
	{
		SameFile,
		DifferentPath,
		MoreRecent,
		LessRecent,
		DifferentFile,
		SameBarcode
	}

	//use if the file exists in repository - used also for barcode duplicates
	//---------------------------------------------------------------------------------------------------
	public enum DuplicateDocumentAction
	{
		AskMeBeforeAttachDoc,
		ReplaceExistingDoc,
		ArchiveAndKeepBothDocs,
		UseExistingDoc,
		RefuseAttachmentOperation,
		Cancel
	}

	// action when delete attachment
	//---------------------------------------------------------------------------------------------------
	public enum DeletingAttachmentAction
	{
		DeleteArchivedDoc,
		KeepArchivedDoc,
		AskBeforeDeleteArchivedDoc
	}

	// action when barcode detection starts
	//---------------------------------------------------------------------------------------------------
	public enum BarcodeDetectionAction
	{
		DetectOnlyInFirstPage,
		DetectTillOneValidBarcodeIsFound
	}

	//---------------------------------------------------------------------------------------------------
	public enum ArchiveResult
	{
		TerminatedSuccess,
		TerminatedWithError,
		Cancel
	}

	//use to filter the query that extracts the document's attachments
	//---------------------------------------------------------------------------------------------------
	public enum AttachmentFilterType
	{
		OnlyAttachment, //extract only attachments
		OnlyPapery,		//extract only paperies
		Both,			//extract both attachments and paperies
        OnlyMainDoc,    //estrai solo i main doc ovvero gli allegati che rappresentano il documento gestionale
		OnlyForMail     //estrai solo gli allegati con IsForMail = true, ovvero quegli allegati che possono essere usati come allegati alle mail
	}

	//---------------------------------------------------------------------------------------------------
	public struct DateRange
	{
		public string Description;
		public DateTime StartDate;
		public DateTime EndDate;

		//---------------------------------------------------------------------
		public DateRange(DateTime startDate, DateTime endDate, string description)
		{
			if (startDate <= endDate)
			{
				StartDate = startDate;
				EndDate = endDate;
			}
			else
			{
				EndDate = startDate;
				StartDate = endDate;
			}
			Description = description;
		}
		//---------------------------------------------------------------------
		public DateRange(bool empty)
		{
			StartDate = DateTime.MinValue;
			EndDate = StartDate = DateTime.MaxValue;
			Description = string.Empty;
		}
	}

	/// <summary>
	/// EnumComboItem
	/// Classe di appoggio per la gestione delle combo con gli enumerativi nei Settings
	/// </summary>
	// ========================================================================
	public class EnumComboItem
	{
		//---------------------------------------------------------------------
		private string enumDescription = string.Empty; // testo localizzato dell'enumerativo
		private object enumValue = null; // generico object che ospita uno specifico enum

		public string EnumDescription { get { return enumDescription; } set { enumDescription = value; } }
		public object EnumValue { get { return enumValue; } set { enumValue = value; } }

		//---------------------------------------------------------------------
		public EnumComboItem(string description, object value)
		{
			enumDescription = description;
			enumValue = value;
		}
	}

	//used between EasyAttachment and TaskBuilder clientDoc CDDMS to know the document bookmark values changed by user
	//================================================================================
	public class BookmarkToObserve
	{
		public string FieldName { get; set; }
		public MDataObj DataObj { get; set; }
		public bool Changed { get; set; }

		public BookmarkToObserve(string name, MDataObj data)
		{
			FieldName = name;
			DataObj = data;	
		}
	}

	//================================================================================
	public class SearchRules
	{
		private string collectorName = string.Empty;
		private string collectionName = string.Empty;
   
		private DateTime startDate = DateTime.MinValue;
		private DateTime endDate = DateTime.MaxValue;

        private SearchContext searchContext = SearchContext.Current;
        private SearchFilterType searchFilterType = SearchFilterType.SearchContext;

        private string docExtensionType = CommonStrings.AllExtensions;
		//Events
		public event EventHandler SearchContextChanged;

		//properties
		public Dictionary<int, IList<int>> CustomFilters = null;
		//--------------------------------------------------------------------------------
		//public string FreeText { get; set; }
		//--------------------------------------------------------------------------------
		public DateTime StartDate { get { return startDate; } set { startDate = value; } }
		//--------------------------------------------------------------------------------
		public DateTime EndDate { get { return endDate; } set { endDate = value; } }
		//--------------------------------------------------------------------------------
        public string DocExtensionType { get { return docExtensionType; } set { docExtensionType = value; } }

        //improvement #
        //--------------------------------------------------------------------------------
        public string DocumentNamespace { get; set; }
        public string DocumentPrimaryKey { get; set; }

        //SOSConnector
		//--------------------------------------------------------------------------------
		public string SosTaxJournal { get; set; }
		public List<int> CollectionIDs = null;
		public List<string> DocumentTypes = null;
        
		//--------------------------------------------------------------------------------
		public SearchContext SearchContext
		{
			get { return searchContext; }
			set { searchContext = value; SearchFilterType = SearchFilterType.SearchContext; }
		}

        //--------------------------------------------------------------------------------
        public string CollectorName
        {
            get { return collectorName; }
            set { collectorName = value; SearchFilterType = SearchFilterType.SearchContext; }
        }

        //--------------------------------------------------------------------------------
        public string CollectionName
        {
            get { return collectionName; }
            set { collectionName = value; SearchFilterType = SearchFilterType.SearchContext; }
        }

        //--------------------------------------------------------------------------------
        public SearchFilterType SearchFilterType
        {
            get { return searchFilterType; }
            set { searchFilterType = value; if (SearchContextChanged != null) SearchContextChanged(this, new EventArgs()); }
        }

        //---------------------------------------------------------------------------------------------------
        public SearchRules(string collector, string collection)
        {
            collectorName = collector;
            collectionName = collection;
        }
    }

	///<summary>
	/// Args utilizzato nella Diagnostica
	///</summary>
	//================================================================================
	public class MessageEventArgs : EventArgs
	{
		// a seconda del tipo visualizzo la bitmap di error/warning/information
		public DiagnosticType MessageType { get; set; }
		// spiegazione messaggio di errore
		public string Explain { get; set; }

		// queste sono proprietà per riempire l'ExtendedInfo
		//--------------------------------------------------------------------------------
		public string Message { get; set; } // omonima proprietà dell'exception
		public string Source { get; set; } // omonima proprietà dell'exception
		public string StackTrace { get; set; } // omonima proprietà dell'exception
		public string Function { get; set; } // nome del metodo nel quale si è verificata l'eccezione
		public string Library { get; set; } // nome del manager nel quale si è verificata l'eccezione
	}

	//================================================================================
	public class AddBookmarkEventArgs : EventArgs
	{
		public string FieldName { get; set; }
		public string ControlName { get; set; }
		public string PhysicalName { get; set; }
		public string FieldValue { get; set; }
		public string Description { get; set; }
		public string ValueType { get; set; }
		public FieldGroup FieldGroup { get; set; }
	}

	//================================================================================
	public class ChooseDocumentEventArgs : EventArgs
	{
		public string FilePath { get; set; }
	}

	//================================================================================
	public class ChangeSearchFilterEventArgs : EventArgs
	{
		public SearchFilterType SearchFilter { get; set; }
	}

	//================================================================================
	public class SearchResultAddingDataTableEventArgs : EventArgs
	{
		public string TBDocNamespace { get; set; }
		public string TBPrimaryKey { get; set; }
		public string DocKeyDescription { get; set; }
		public AttachmentsForErpDocument ResultDataTable { get; set; }
	}

	//================================================================================
	public class AddRowInResultEventArgs : EventArgs
	{
		public DataRow Row { get; set; }

		public AddRowInResultEventArgs(DataRow data)
		{
			Row = data;
		}
	}

	///<summary>
	/// Args utilizzato nella visualizzazione dell'attachment nella finestra di Search
	///</summary>
	//================================================================================
	public class AttachmentInfoEventArgs : EventArgs
	{
		public AttachmentInfo CurrentAttachment { get; set; }
	}

    ///<summary>
    /// Args utilizzato per l'evento di cancellazione di un attachment
    ///</summary>
    //================================================================================
    public class AttachmentEventArgs : EventArgs
    {
        public int AttachmentID { get; set; }
    }

    ///<summary>
    /// Args utilizzato per evento di inserimento\modifica collection
    ///</summary>
    //================================================================================
    public class CollectionEventArgs : EventArgs
    {
        public int CollectionID { get; set; }
    }

    ///<summary>
    /// Args utilizzato per l'evento di cambio stato (form mode) del documento erp
    ///</summary>
    //================================================================================
    public class FormModeEventArgs : EventArgs
	{
		public FormModeType NewFormMode { get; set; }
		public FormModeType OldFormMode { get; set; }
	}

	//================================================================================
	public class FilterEventArgs : EventArgs
	{
		public List<int> Workers { get; set; }
		public int TopDocsNumber { get; set; }

        #region da togliere
        public bool OnlyReport { get; set; }
        public bool OnlyAttached { get; set; }
        public bool OnlyPending { get; set; }
        public bool OnlyNonAttached { get; set; }
        #endregion


        public string DocExtensionType { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

        public int CollectionID { get; set; }

        public SearchFieldsDataTable SearchPatternDT { get; set; }
        public SearchFieldsConditionsDataTable SearchFieldsConditionsDT { get; set; }
        public string FreeTag { get; set; }
		public SearchLocation SearchLocation { get; set; }

        public string SecurityDocNamespaces { get; set; }

        //--------------------------------------------------------------------------------
        public FilterEventArgs()
		{
			Workers = null;
			TopDocsNumber = 0;//topDocsNumber;
			OnlyReport = false;
            OnlyPending = false;
            OnlyAttached = true;
            OnlyNonAttached = true;
			DocExtensionType = CommonStrings.AllExtensions;
			StartDate = DateTime.MinValue;
			EndDate = DateTime.MaxValue;
			SearchPatternDT = null;
            SearchFieldsConditionsDT = null;
            FreeTag = string.Empty;
			SearchLocation = SearchLocation.All;
		}
	}

	/// <summary>
	/// utilizzato per inviare msg durante l'elaborazione di invio dalla SOS al codice C++
	/// </summary>
	//================================================================================
	public class SOSEventArgs : EventArgs
	{
		public int Idx { get; set; }
		public string Message { get; set; }
		public string Notes { get; set; }
		public DiagnosticType MessageType  { get; set; }
	}

	//=========================================================================
	public class NamespaceDetails : IComparable
	{
		public int ErpDocId;
		public string PrimaryKey;
		public int ArchivedDocId;
		public AttachmentInfoOtherData AttInfoOtherData;
		public ERPDocumentBarcode ErpDocumentBarcode;

		//--------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null) return false;
			NamespaceDetails otheObj = obj as NamespaceDetails;
			if (otheObj == null) return false;

			return (
				String.Compare(otheObj.PrimaryKey, PrimaryKey, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				ErpDocId == otheObj.ErpDocId &&
				ArchivedDocId == otheObj.ArchivedDocId
				);
		}

		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return PrimaryKey.GetHashCode() + ErpDocId.GetHashCode() + ArchivedDocId.GetHashCode();
		}

		#region IComparable Members

		//--------------------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			if (obj == null) return -1;
			NamespaceDetails otheObj = obj as NamespaceDetails;
			if (otheObj == null) return -1;

			int i = String.Compare(otheObj.PrimaryKey, PrimaryKey, StringComparison.InvariantCultureIgnoreCase);
			if (i == 0)
				return otheObj.ArchivedDocId > ArchivedDocId ? -1 : (otheObj.ArchivedDocId == ArchivedDocId ? 0 : 1);
			return i;
		}

		#endregion
	}

    public class BarcodeMapping
    {
        //--------------------------------------------------------------------------------
        public static TBPicComponents.TBPicBarcode1DReaderType GetTBPicBarcode1DReaderType(string description)
        {
            switch (description)
            {
                case "UPCA": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderUPCA;
                case "UPCE": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderUPCE;
                case "EAN13": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN13;
                case "EAN8": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN8;
                case "CODE39": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCode39;
                case "INT25": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderInterleaved2of5;
                case "CODE128": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCode128;
                case "CODABAR": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCodeabar;
                case "CODE93": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCODE93;
                case "EAN128": return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN128;
                default: return TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCode39;
            }
        }

        ////--------------------------------------------------------------------------------
        //public static string GetTBPicBarcode1DReaderType(TBPicComponents.TBPicBarcode1DReaderType type)
        //{
        //    switch (type)
        //    {
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN8: return "EAN8";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderUPCA: return "UPCA";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderUPCE: return "UPCE";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN13: return "EAN13";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCode39: return "CODE39";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderInterleaved2of5: return "INT25";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCode128: return "CODE128";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCodeabar: return "CODABAR";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCODE93: return "CODE93";
        //        case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN128: return "EAN128";
        //        default: return "CODE39";
        //    }
        //}

        //
        //returns wether barcode if 2D type or not
        //--------------------------------------------------------------------------------
        public static bool If2DBarcode(BarCodeType barcodeType)
        {
            switch (barcodeType)
            {
                case BarCodeType.BC_PDF417:
                case BarCodeType.BC_DATAMATRIX:
                case BarCodeType.BC_MICROQR:
                case BarCodeType.BC_QR:
                    return true;
                default: return false;
            }
        }

        //--------------------------------------------------------------------------------
        public static TBPicComponents.TBPicBarcode1DWriterType GetTBPicBarcode1DWriterType(string name)
        {
            switch (name)
            {
                case "EAN128": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCode128;
                case "EAN8": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterEAN8;
                case "UPCA": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterUPCVersionA;
                case "UPCE": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterUPCVersionE;
                case "EAN13": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterEAN13;
                case ""://def
                case "CODE39": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCode39;
                case "INT25": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterInterleaved2of5;
                case "CODE128": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCode128;
                case "CODABAR": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCodabar;
                case "CODE93": return TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCode93;
            }

            throw new Exception(String.Format( "This barcode type ({0}) can not be displayed",name));
        }

        //--------------------------------------------------------------------------------
        public static TBPicComponents.TBPicBarcode2DWriterType GetTBPicBarcode2DWriterType(string name)
        {
            switch (name)
            {
                case ""://def
                case "DataMatrix": return TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterDataMatrix;
                case "MicroQR": return TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterMicroQR;
                case "QR": return TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterQR;
                case "PDF": return TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterPDF417;
            }

            throw new Exception(String.Format("This barcode type ({0}) can not be displayed", name));
        }


        //--------------------------------------------------------------------------------
        public static string GetBarCodeDescription(BarCodeType type)
        {
            switch (type)
            {
                case BarCodeType.BC_UPCA: return "UPCA";
                case BarCodeType.BC_UPCE: return "UPCE";
                case BarCodeType.BC_EAN13: return "EAN13";
                case BarCodeType.BC_EAN8: return "EAN8";
                case BarCodeType.BC_CODE39: return "CODE39";
                case BarCodeType.BC_EXT39: return "EXT39";
                case BarCodeType.BC_INT25: return "INT25";
                case BarCodeType.BC_CODE128: return "CODE128";
                case BarCodeType.BC_CODABAR: return "CODABAR";
                case BarCodeType.BC_ZIP: return "ZIP";
                case BarCodeType.BC_MSIPLESSEY: return "MSIPLESSEY";
                case BarCodeType.BC_CODE93: return "CODE93";
                case BarCodeType.BC_EXT93: return "EXT93";
                case BarCodeType.BC_UCC128: return "UCC128";
                case BarCodeType.BC_HIBC: return "HIBC";
                case BarCodeType.BC_PDF417: return "PDF417";
                case BarCodeType.BC_UPCE0: return "UPCE0";
                case BarCodeType.BC_UPCE1: return "UPCE1";
                case BarCodeType.BC_CODE128A: return "CODE128A";
                case BarCodeType.BC_CODE128B: return "CODE128B";
                case BarCodeType.BC_CODE128C: return "CODE128C";
                case BarCodeType.BC_EAN128: return "EAN128";
                case BarCodeType.BC_DATAMATRIX: return "DataMatrix";
                case BarCodeType.BC_MICROQR: return "MicroQR";
                case BarCodeType.BC_QR: return "QR";
                default: return "CODE39";
            }
        }

        //todo ilaria barcode confronta e unifica con \standard\TaskBuilder\TaskBuilderNet\Microarea.TaskBuilderNet.Woorm\WoormViewer\BarCode.cs
        //--------------------------------------------------------------------------------
        public static BarCodeType GetBarCodeType(string description)
        {
            switch (description)
            {
                case "UPCA": return BarCodeType.BC_UPCA;
                case "UPCE": return BarCodeType.BC_UPCE;
                case "EAN13": return BarCodeType.BC_EAN13;
                case "EAN8": return BarCodeType.BC_EAN8;
                case "CODE39": return BarCodeType.BC_CODE39;
                case "EXT39": return BarCodeType.BC_EXT39;
                case "INT25": return BarCodeType.BC_INT25;
                case "CODE128": return BarCodeType.BC_CODE128;
                case "CODABAR": return BarCodeType.BC_CODABAR;
                case "ZIP": return BarCodeType.BC_ZIP;
                case "MSIPLESSEY": return BarCodeType.BC_MSIPLESSEY;
                case "CODE93": return BarCodeType.BC_CODE93;
                case "EXT93": return BarCodeType.BC_EXT93;
                case "UCC128": return BarCodeType.BC_UCC128;
                case "HIBC": return BarCodeType.BC_HIBC;
                case "PDF417": return BarCodeType.BC_PDF417;
                case "UPCE0": return BarCodeType.BC_UPCE0;
                case "UPCE1": return BarCodeType.BC_UPCE1;
                case "CODE128A": return BarCodeType.BC_CODE128A;
                case "CODE128B": return BarCodeType.BC_CODE128B;
                case "CODE128C": return BarCodeType.BC_CODE128C;
                case "EAN128": return BarCodeType.BC_EAN128;
                case "DataMatrix": return BarCodeType.BC_DATAMATRIX;
                case "MicroQR": return BarCodeType.BC_MICROQR;
                case "QR": return BarCodeType.BC_QR;
                default: return BarCodeType.BC_CODE39;
            }

        }

        //--------------------------------------------------------------------------------
        internal static BarCodeType GetBarCodeType(TBPicComponents.TBPicBarcode1DReaderType barcodeType)
        {
            switch (barcodeType)
            {
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderUPCA: return BarCodeType.BC_UPCA;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderUPCE: return BarCodeType.BC_UPCE;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN13: return BarCodeType.BC_EAN13;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCode39: return BarCodeType.BC_CODE39;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderInterleaved2of5: return BarCodeType.BC_INT25;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCode128: return BarCodeType.BC_CODE128;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCodeabar: return BarCodeType.BC_CODABAR;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderCODE93: return BarCodeType.BC_CODE93;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN128: return BarCodeType.BC_EAN128;
                case TBPicComponents.TBPicBarcode1DReaderType.Barcode1DReaderEAN8: return BarCodeType.BC_EAN8;
                default: return BarCodeType.BC_CODE39;
            }
        }


         //--------------------------------------------------------------------------------
        internal static BarCodeType GetBarCodeType(TBPicComponents.TBPicBarcode2DReaderType barcodeType)
        {
            switch (barcodeType)
            {
                case TBPicComponents.TBPicBarcode2DReaderType.Barcode2DReaderDataMatrix: return BarCodeType.BC_DATAMATRIX;
                case TBPicComponents.TBPicBarcode2DReaderType.Barcode2DReaderMicroQR: return BarCodeType.BC_MICROQR;
                case TBPicComponents.TBPicBarcode2DReaderType.Barcode2DReaderQR: return BarCodeType.BC_QR;
                case TBPicComponents.TBPicBarcode2DReaderType.Barcode2DReaderPDF417: return BarCodeType.BC_PDF417;
                default: return BarCodeType.BC_DATAMATRIX;
            }
        }

        //--------------------------------------------------------------------------------
        internal static BarCodeType GetBarCodeType(TBPicComponents.TBPicBarcode1DWriterType barCodeType)
        {
            switch (barCodeType)
            {
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterEAN8: return BarCodeType.BC_EAN8;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterUPCVersionA: return BarCodeType.BC_UPCA;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterUPCVersionE: return BarCodeType.BC_UPCE;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterEAN13: return BarCodeType.BC_EAN13;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCode39: return BarCodeType.BC_CODE39;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterInterleaved2of5: return BarCodeType.BC_INT25;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCode128: return BarCodeType.BC_CODE128;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCodabar: return BarCodeType.BC_CODABAR;
                case TBPicComponents.TBPicBarcode1DWriterType.Barcode1DWriterCode93: return BarCodeType.BC_CODE93;
               
                default: return BarCodeType.BC_CODE39;
            }
        }

        //--------------------------------------------------------------------------------
        internal static BarCodeType GetBarCodeType(TBPicComponents.TBPicBarcode2DWriterType barCodeType)
        {

            switch (barCodeType)
            {
                case TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterDataMatrix: return BarCodeType.BC_DATAMATRIX;
                case TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterMicroQR: return BarCodeType.BC_MICROQR;
                case TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterQR: return BarCodeType.BC_QR;
                case TBPicComponents.TBPicBarcode2DWriterType.Barcode2DWriterPDF417: return BarCodeType.BC_PDF417;
                default: return BarCodeType.BC_DATAMATRIX;
            }


        }
    }

	//Classe di appoggio che è utile per raccogliere tutte le informazioni necessarie relative ad un attach massivo.
	//=========================================================================
	[Serializable]	
	public class AttachmentInfoOtherData
	{
		private int processedDocuments = 0;

		public AttachmentInfo Attachment;
		public MassiveStatus BarCodeStatus;

		public MassiveResult Result = MassiveResult.Todo;
		[XmlElement("ErpDocumentList")]
		public List<ERPDocumentBarcode> ERPDocumentsBarcode;

		[XmlIgnore]
		public Diagnostic Diagnostic = new Diagnostic("AttachmentInfoOtherData");
	
		[XmlIgnore]
		public int FailedDocuments = 0;
		
		[XmlIgnore]
		public int RowIndex = -1;
				
		public delegate void ProcessedEventHandler(object sender, EventArgs args);
		public event ProcessedEventHandler ProcessedAllDocuments;

		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public int ProcessedDocuments
		{
			get
			{
				return processedDocuments;
			}
			set
			{
				processedDocuments = value;
				if (ERPDocumentsBarcode != null && processedDocuments == ERPDocumentsBarcode.Count && ProcessedAllDocuments != null)
					ProcessedAllDocuments(this, EventArgs.Empty);
			}
		}

		//--------------------------------------------------------------------------------
		private MassiveAction actionToDo = MassiveAction.Undefined;
		public MassiveAction ActionToDo//l'azione viene decisa in base allo stato e si può solo annullare adesso.
		{
			get
			{
				//se undefined imposto il default.
				if (actionToDo == MassiveAction.Undefined)
				{
					if (BarCodeStatus == MassiveStatus.BCDuplicated)
						actionToDo = MassiveAction.None;
					else if (BarCodeStatus == MassiveStatus.Papery)
						actionToDo = MassiveAction.Attach;
					else if (Attachment != null  && Attachment.ArchivedDocId < 0)//se no è già archiviato
						actionToDo = MassiveAction.Archive;
					else //if (Attachment.ArchivedDocId >= 0)tutti gli altri casi non faccio niente
						actionToDo = MassiveAction.None;
				}
				return actionToDo;
			}
			set
			{ actionToDo = value; }
		}

        //--------------------------------------------------------------------------------
        public List<MassiveAction> PossibleActions = new List<MassiveAction>();
       


		//serve per la serializzazione in XML
		//--------------------------------------------------------------------------------
		public AttachmentInfoOtherData()
		{
			ERPDocumentsBarcode = new List<ERPDocumentBarcode>();
			Attachment = new AttachmentInfo();
		}

		//--------------------------------------------------------------------------------
		public AttachmentInfoOtherData(AttachmentInfo attachmentInfo)
		{
			Attachment = attachmentInfo;
		}
	}

	//Classe di appoggio 
	//=========================================================================
	[Serializable]
	public class ERPDocumentBarcode
	{
		[XmlAttribute("PrimaryKey", DataType = "string")]
		public string PK { get; set; }
		[XmlAttribute("Namespace", DataType = "string")]
		public string Namespace { get; set; }

		[XmlIgnore]
		public int ErpDocID;
		[XmlIgnore]
		public Diagnostic ErpDocDiagnostic = new Diagnostic("BarcodeDocument");

		//--------------------------------------------------------------------------------
		public ERPDocumentBarcode()
		{ 
		}
	}

	//classe usata per la deserializzazione in XML
	[Serializable]
	[XmlRoot("MassiveAttachInfo")]
	//=========================================================================
	public class MassiveAttachInfo
	{
		[XmlElement("AttachmentInfoList")]
		public List<AttachmentInfoOtherData> AttachmentInfoOtherDataList;

		//--------------------------------------------------------------------
		public MassiveAttachInfo()
		{
			AttachmentInfoOtherDataList = new List<AttachmentInfoOtherData>();
		}

		//--------------------------------------------------------------------
		public void AddAttachmentInfo(AttachmentInfoOtherData aiod)
		{
			AttachmentInfoOtherDataList.Add(aiod);
		}

		//--------------------------------------------------------------------
		public static MassiveAttachInfo Deserialize(string xmlString)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(MassiveAttachInfo));
			using (StringReader sr = new StringReader(xmlString))
			{
				MassiveAttachInfo ss = (MassiveAttachInfo)serializer.Deserialize(sr);

				if (ss == null)
				{
					Debug.Assert(false);
					return null;
				}
				return ss;
			}
		}

		//--------------------------------------------------------------------
		public string Serialize()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(MassiveAttachInfo));
			using (StringWriter sw = new StringWriter())
			{
				try
				{
					serializer.Serialize(sw, this);
					string result =  sw.ToString();
					return result;
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.ToString());
					return string.Empty;
				}	
			}
		}



	}

    //================================================================================
    public class MassiveEventArgs : EventArgs
    {
        public AttachmentInfoOtherData aiod = null;

        public MassiveEventArgs(AttachmentInfoOtherData aiod)
        {
            this.aiod = aiod;
        }
    }

    //enumerativo che indica lo stato del bar code  e relativo documento erp (Operazioni future) del documento elettronico
    //--------------------------------------------------------------------
    public enum MassiveStatus { NoBC, Papery, OnlyBC, BCDuplicated, ItemDuplicated }

	//Enumerativo che indica l'azione da eseguire sul documento elettronico
	//--------------------------------------------------------------------
	public enum MassiveAction { Undefined, None, Attach, Archive, Substitute }

	//Enumerativo che indica il risultato del processo di massiva
	//--------------------------------------------------------------------
	public enum MassiveResult { Todo, Done, Failed, WithError, PreFailed, Ignored }	

    //Enumerativo che indica il BarCodeType -> preso da barcode.cpp di woorviewer
	//--------------------------------------------------------------------
    public enum BarCodeType
    {
        BC_UPCA,
        BC_UPCE,
        BC_EAN13,
        BC_EAN8,
        BC_CODE39,
        BC_EXT39,
        BC_INT25,
        BC_CODE128,
        BC_CODABAR,
        BC_ZIP,
        BC_MSIPLESSEY,
        BC_CODE93,
        BC_EXT93,
        BC_UCC128,
        BC_HIBC,
        BC_PDF417,
        BC_UPCE0,
        BC_UPCE1,
        BC_CODE128A,
        BC_CODE128B,
        BC_CODE128C,
        BC_EAN128,
        BC_DATAMATRIX,
        BC_MICROQR,
        BC_QR,
        NOTSUPPORTED, 
        NONE,
        PAPERY
    }                        

      
}
