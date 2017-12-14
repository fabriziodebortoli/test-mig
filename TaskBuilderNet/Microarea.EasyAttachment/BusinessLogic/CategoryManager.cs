using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Microarea.EasyAttachment.Components;
using Microarea.EasyAttachment.Core;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyAttachment.BusinessLogic
{
	///<summary>
	/// Manager delle categorie e relativa gestione nel database
	///</summary>
	//================================================================================
	public class CategoryManager : BaseManager
	{
		private DMSModelDataContext dc = null;

		///<summary>
		/// Constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public CategoryManager(DMSOrchestrator dmsOrchestrator)
		{
			DMSOrchestrator = dmsOrchestrator;
			ManagerName = "CategoryManager";
			dc = DMSOrchestrator.DataContext;
		}

        
        //--------------------------------------------------------------------------------
        public List<string> GetCategoriesName()
        {
            List<string> nameList = new List<string>();

            // seleziono le categorie sulla tabella DMS_Fields
            var categories = from cats in dc.DMS_Fields
                             where cats.IsCategory == true
                             select cats;

            // se non trovo record ritorno
            if (categories == null || !categories.Any())
                return nameList;

            try
            {
                foreach (DMS_Field cat in categories)
                    nameList.Add(cat.FieldName);
            }
            catch (Exception e)
            {
                SetMessage(Strings.ErrorLoadingCategories, e, "GetCategoriesName");
            }

            return nameList;
        }


		///<summary>
		/// Ritorna un datatable con l'elenco delle categorie
		///</summary>
		//-----------------------------------------------------------------------------------------------
		public DTCategories GetCategories()
		{
			DTCategories catDataTable = new DTCategories();

			// seleziono le categorie sulla tabella DMS_Fields
			var categories = from cats in dc.DMS_Fields
							 where cats.IsCategory == true
							 select cats;

			// se non trovo record ritorno
			if (categories == null || !categories.Any())
				return catDataTable;

			try
			{
				foreach (DMS_Field cat in categories)
				{
					DataRow row = catDataTable.NewRow();

					row[CommonStrings.Name] = cat.FieldName;
					row[CommonStrings.FieldDescription] = cat.FieldDescription;
                    DTCategoriesValues catValuesDataTable = new DTCategoriesValues();
                    string defValue = string.Empty;
                    if (cat.DMS_FieldProperty != null && cat.DMS_FieldProperty.XMLValues != null)
                    {
                        // estraggo tutti i valori e li metto in un DataTable, controllo anche se sono in uso come bookmark
                        List<FieldValueState> fieldValues = FieldPropertiesState.Deserialize(cat.DMS_FieldProperty.XMLValues.ToString());
                        foreach (FieldValueState vField in fieldValues)
                        {
                            DataRow fRow = catValuesDataTable.NewRow();
                            fRow[CommonStrings.InUse] = IsASearchField(cat.FieldName, vField.FieldValue);
                            fRow[CommonStrings.Value] = vField.FieldValue;
                            fRow[CommonStrings.IsDefault] = vField.IsDefault;
                            if (vField.IsDefault)
                                defValue = vField.FieldValue;
                            catValuesDataTable.Rows.Add(fRow);
                        }
                    }
					row[CommonStrings.ValueSet] = catValuesDataTable;
                    row[CommonStrings.Value]    = defValue;
					row[CommonStrings.Disable]  = (cat.DMS_FieldProperty != null) ? cat.DMS_FieldProperty.Disabled : false;
					row[CommonStrings.Color]    = (cat.DMS_FieldProperty != null) ?cat.DMS_FieldProperty.FieldColor : 0;
					catDataTable.Rows.Add(row);
				}
			}
			catch (System.FormatException fe)
			{
				SetMessage(Strings.ErrorLoadingCategories, fe, "GetCategories");
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorLoadingCategories, e, "GetCategories");
			}

			return catDataTable;
		}

		///<summary>
		/// Ritorna il record di una categoria dato il nome
		///</summary>
		//-----------------------------------------------------------------------------------------------
		public DMS_Field GetCategory(string categoryName)
		{
			DMS_Field category = null;

			var cat = from c in dc.DMS_Fields where (c.FieldName == categoryName) select c;

			//the field doesn't exist: I return null
			if (cat == null || !cat.Any())
				return category;

			try
			{
				category = (DMS_Field)cat.Single();
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorLoadingCategory, categoryName), e, "GetCategory");
				return null;
			}

			return category;
		}

		/// <summary>
		/// return the possible values of categoryName
		/// </summary>
		//-----------------------------------------------------------------------------------------------
		public List<FieldValueState> GetCategoryValues(string categoryName)
		{
			DMS_Field category = GetCategory(categoryName);

			if (category != null && category.DMS_FieldProperty.XMLValues != null)
			{
				try
				{
					return FieldPropertiesState.Deserialize(category.DMS_FieldProperty.XMLValues.ToString());
				}
				catch (Exception e)
				{
					SetMessage(string.Format(Strings.ErrorLoadingCategoryValues, categoryName), e, "GetCategoryValues");
					return null;
				}
			}
			return null;
		}

		///<summary>
		/// Salvataggio di una categoria (inserimento di una nuova o aggiornamento di una esistente)
		///</summary>
		//-----------------------------------------------------------------------------------------------
		public bool SaveCategory(CategoryInfo catInfo)
		{
			if (catInfo == null)
				return false;

			// se il vecchio nome ha un valore (ovvero sto modificando una categoria esistente) e
			// se il vecchio nome e il nuovo nome sono diversi allora devo andare ad aggiornare la PK FieldName
			if (!string.IsNullOrWhiteSpace(catInfo.OldName) &&
				string.Compare(catInfo.Name, catInfo.OldName, StringComparison.InvariantCultureIgnoreCase) != 0)
				return UpdateCategory(catInfo);

			// altrimenti procedo con l'inserimento/aggiornamento dei record
			var cat = from c in dc.DMS_Fields where (c.FieldName == catInfo.Name) select c;

			//the field doesn't exist: I add it
			if (cat == null || !cat.Any())
			{
				DMS_Field category = new DMS_Field();
				category.FieldName = catInfo.Name;
				category.FieldDescription = catInfo.Description;
				category.ValueType = GetStringFromCategoryType(CategoryType.String);
				category.IsCategory = true;

				category.DMS_FieldProperty = new DMS_FieldProperty();
				category.DMS_FieldProperty.FieldName = catInfo.Name;
				category.DMS_FieldProperty.Disabled = catInfo.Disabled;

				try
				{
					category.DMS_FieldProperty.FieldColor = (catInfo.TheColor != null) ? catInfo.TheColor.ToArgb() : -1;

					if (catInfo.CategoriesValuesDataTable != null && catInfo.CategoriesValuesDataTable.Rows.Count > 0)
					{
						FieldPropertiesState fProperties = new FieldPropertiesState();
						foreach (DataRow val in catInfo.CategoriesValuesDataTable.Rows)
						{
							if (string.IsNullOrWhiteSpace(val[CommonStrings.Value].ToString()))
								continue;

							FieldValueState fValue = new FieldValueState();
							fValue.FieldValue = val[CommonStrings.Value].ToString();
							fValue.IsDefault = ((bool)(val[CommonStrings.IsDefault]));
							fProperties.FieldValues.Add(fValue);
						}

						using (StringReader sr = new StringReader(fProperties.Serialize()))
							category.DMS_FieldProperty.XMLValues = XElement.Load(sr);
					}

					dc.DMS_Fields.InsertOnSubmit(category);
					dc.SubmitChanges();
				}
				catch (Exception e)
				{
					SetMessage(string.Format(Strings.ErrorSavingCategory, catInfo.Name), e, "SaveCategory");
					return false;
				}
			}
			else //the field exists
			{
				try
				{
					DMS_Field category = (DMS_Field)cat.Single();

					// prima controllo se si tratta di una categoria, altrimenti non procedo
					if (category == null)
						return false;

					if (category.IsCategory == false)
					{
						SetMessage(string.Format(Strings.CategoryHasSameNameOfField, category.FieldName), null, "SaveCategory");
						return false;
					}

					var cFields = from cf in dc.DMS_CollectionsFields where (cf.FieldName == category.FieldName) select cf;
					string lockMsg = string.Empty;

					if (
							DMSOrchestrator.LockManager.LockRecord(category, DMSOrchestrator.LockContext, ref lockMsg) &&
							((cFields == null) || DMSOrchestrator.LockManager.LockRecord("DMS_CollectionsField", category.FieldName, DMSOrchestrator.LockContext, ref lockMsg))
						)
					{
						category.FieldDescription = catInfo.Description;

						if (catInfo.CategoriesValuesDataTable != null && catInfo.CategoriesValuesDataTable.Rows.Count > 0)
						{
							FieldPropertiesState fProperties = new FieldPropertiesState();
							for (int i = 0; i < catInfo.CategoriesValuesDataTable.Rows.Count; i++)
							{
								DataRow val = catInfo.CategoriesValuesDataTable.Rows[i];

								// le righe invariate sono da serializzare
								if (val.RowState == DataRowState.Unchanged)
								{
									FieldValueState fValue = new FieldValueState();
									fValue.FieldValue = val[CommonStrings.Value].ToString();
									fValue.IsDefault = ((bool)(val[CommonStrings.IsDefault]));
									fProperties.FieldValues.Add(fValue);
								}

								// skippo i valori vuoti e le righe cancellate
								if (val.RowState != DataRowState.Deleted && string.IsNullOrWhiteSpace(val[CommonStrings.Value].ToString()))
									continue;

								// le righe aggiunte sono da serializzare
								if (val.RowState == DataRowState.Added)
								{
									FieldValueState fValue = new FieldValueState();
									fValue.FieldValue = val[CommonStrings.Value].ToString();
									fValue.IsDefault = ((bool)(val[CommonStrings.IsDefault]));
									fProperties.FieldValues.Add(fValue);
								}

								// per le righe modificate devo modificare i valori nella DMS_SearchFieldIndexes e serializzare
								if (val.RowState == DataRowState.Modified)
								{
									// devo aggiornare gli indici e serializzare
									if (UpdateSearchFieldValueIndex
										(
										category.FieldName,
										val[CommonStrings.Value, DataRowVersion.Original].ToString(),
										val[CommonStrings.Value].ToString()
										))
									{
										FieldValueState fValue = new FieldValueState();
										fValue.FieldValue = val[CommonStrings.Value].ToString();
										fValue.IsDefault = ((bool)(val[CommonStrings.IsDefault]));
										fProperties.FieldValues.Add(fValue);
									}
								}

								// per le righe cancellate devo solo eliminare i valori nella DMS_SearchFieldIndexes e DMS_AttachmentSearchIndexes
								if (val.RowState == DataRowState.Deleted)
								{
									// (essendo una riga Deleted devo considerare la versione Original, quindi leggere l'OldValue)
									DeleteIndexes(category.FieldName, val[CommonStrings.Value, DataRowVersion.Original].ToString());
								}
							}

							using (StringReader sr = new StringReader(fProperties.Serialize()))
								category.DMS_FieldProperty.XMLValues = XElement.Load(sr);
						}

						category.DMS_FieldProperty.FieldColor = (catInfo.TheColor != null) ? catInfo.TheColor.ToArgb() : -1;
						category.DMS_FieldProperty.Disabled = catInfo.Disabled;

						// aggiorno il flag Disabled su tutti i riferimenti nella DMS_CollectionsFields (indipendentemente dal CollectionID)
						if (cFields != null)
						{
							foreach (DMS_CollectionsField collField in cFields)
								collField.Disabled = catInfo.Disabled;
						}

						dc.SubmitChanges();

						DMSOrchestrator.LockManager.UnlockRecord(category, DMSOrchestrator.LockContext);
						if (cFields != null)
							DMSOrchestrator.LockManager.UnlockRecord("DMS_CollectionsField", category.FieldName, DMSOrchestrator.LockContext);
					}
					else
						SetMessage(string.Format(Strings.ErrorSavingCategory), new Exception(lockMsg), "SaveCategory");
				}
				catch (Exception e)
				{
					SetMessage(string.Format(Strings.ErrorSavingCategory, catInfo.Name), e, "SaveCategory");
					return false;
				}
			}

			// e' andato tutto a buon e reimposto il valore all'old
			catInfo.OldName = catInfo.Name;
			return true;
		}

		///<summary>
		/// Metodo richiamato internamente solo per fare l'Update di una categoria esistente che non e' stata
		/// ancora utilizzata come bookmark
		///</summary>
		//-----------------------------------------------------------------------------------------------
		internal bool UpdateCategory(CategoryInfo catInfo)
		{
			if (catInfo == null)
				return false;

			// se la categoria e' stata utilizzata (ovvero e' presente nella DMS_CollectionsFields)
			// tento di disabilitarla
			if (IsCategoryUsed(catInfo.OldName))
			{
				if (!DisableCategory(catInfo.OldName))
					return false;
			}

			// se non riesco ad eliminare la vecchia categoria non procedo
			if (!DeleteCategory(catInfo.OldName))
				return false;

			// ora posso inserire il nuovo record
			DMS_Field category = new DMS_Field();
			category.FieldName = catInfo.Name;
			category.FieldDescription = catInfo.Description;
			category.ValueType = GetStringFromCategoryType(CategoryType.String);
			category.IsCategory = true;

			category.DMS_FieldProperty = new DMS_FieldProperty();
			category.DMS_FieldProperty.FieldName = catInfo.Name;
			category.DMS_FieldProperty.Disabled = catInfo.Disabled;

			try
			{
				category.DMS_FieldProperty.FieldColor = (catInfo.TheColor != null) ? catInfo.TheColor.ToArgb() : -1;

				if (catInfo.CategoriesValuesDataTable != null && catInfo.CategoriesValuesDataTable.Rows.Count > 0)
				{
					FieldPropertiesState fProperties = new FieldPropertiesState();
					foreach (DataRow val in catInfo.CategoriesValuesDataTable.Rows)
					{
						if (string.IsNullOrWhiteSpace(val[CommonStrings.Value].ToString()))
							continue;

						FieldValueState fValue = new FieldValueState();
						fValue.FieldValue = val[CommonStrings.Value].ToString();
						fValue.IsDefault = ((bool)(val[CommonStrings.IsDefault]));
						fProperties.FieldValues.Add(fValue);
					}

					using (StringReader sr = new StringReader(fProperties.Serialize()))
						category.DMS_FieldProperty.XMLValues = XElement.Load(sr);
				}

				dc.DMS_Fields.InsertOnSubmit(category);
				dc.SubmitChanges();
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorSavingCategory, catInfo.Name), e, "UpdateCategory");
				return false;
			}

			// e' andato tutto a buon fine e reimposto il valore all'old
			catInfo.OldName = catInfo.Name;
			return true;
		}

		///<summary>
		/// Eliminazione di una categoria dato il suo nome
		/// viene fatta una query nella tabella DMS_CollectionsFields per verificare se esistono righe 
		/// con quel nome categoria e il tipo categoria (== 2)
		/// 1. se non ci sono righe procedo alla cancellazione eliminando il record nelle tabelle DMS_Fields e DMS_FieldProperties
		/// 2. se ci sono righe devo andare a leggere con il CollectionID se ci sono record nella tabella DMS_Attachment:
		/// se non sono presenti allegati allora posso procedere con la cancellazione
		///</summary>
		//--------------------------------------------------------------------------------
		public bool Old_DeleteCategory(string categoryName)
		{
			try
			{
				// cerco nella tabella DMS_CollectionsFields se esistono righe con quel nome categoria e il tipo categoria (== 2)
				var cField = from c in dc.DMS_CollectionsFields
							 where
								 (c.FieldName == categoryName && (FieldGroup)c.FieldGroup == FieldGroup.Category)
							 select c;

				// se non ci sono righe procedo alla cancellazione
				if (cField == null || !cField.Any())
					return DeleteCategory(categoryName);

				bool canDelete = true;

				// devo controllare che nessuna riga abbia legami nella tabella DMS_Attachment (tramite il collectioID)
				foreach (DMS_CollectionsField f in cField)
				{
					if (canDelete)
						canDelete = !f.DMS_Collection.DMS_Attachments.Any();
					else
						break;
				}

				if (canDelete)
					return DeleteCategory(categoryName);
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorDeletingCategory, categoryName), e, "DeleteCategory");
				return false;
			}

			return true;
		}

		///<summary>
		/// Cancellazione di una categoria eliminando il record nelle tabelle DMS_Fields e DMS_FieldProperties
		/// Questo metodo non si pone di eventuali legami negli allegati
		/// Esternamente bisogna usare la DeleteCategory
		///</summary>
		//--------------------------------------------------------------------------------
		internal bool DeleteCategory(string categoryName)
		{
			try
			{
				DMS_Field category = GetCategory(categoryName);

				string lockMsg = string.Empty;

				if (DMSOrchestrator.LockManager.LockRecord(category, DMSOrchestrator.LockContext, ref lockMsg))
				{
					if (category != null)
					{
						// elimino la riga nella DMS_CollectionsFields
						var cFields = (from cf in dc.DMS_CollectionsFields
									   where cf.FieldName == category.FieldName
									   select cf);

						foreach (DMS_CollectionsField field in cFields)
							dc.DMS_CollectionsFields.DeleteOnSubmit(field);

						// elimino la riga dei valori della categoria nella DMS_FieldProperties
						var row = (from t in dc.DMS_FieldProperties
								   where t.FieldName == category.FieldName
								   select t).Single();
						dc.DMS_FieldProperties.DeleteOnSubmit(row);
					}

					// elimino la testa nella DMS_Field
					dc.DMS_Fields.DeleteOnSubmit(category);
					dc.SubmitChanges();

					DMSOrchestrator.LockManager.UnlockRecord(category, DMSOrchestrator.LockContext);
				}
				else
					SetMessage(string.Format(Strings.ErrorDeletingCategory), new Exception(lockMsg), "DeleteCategory");
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorDeletingCategory, categoryName), e, "DeleteCategory");
				return false;
			}

			return true;
		}

		///<summary>
		/// imposta il flag Disable = true alla Categoria sulla tabella DMS_CollectionsFields
		///</summary>
		//--------------------------------------------------------------------------------
		public bool DisableCategory(string categoryName)
		{
			try
			{
				DMS_Field category = GetCategory(categoryName);

				if (category == null)
					return true;

				string lockMsg = string.Empty;
				var cFields = from cf in dc.DMS_CollectionsFields where (cf.FieldName == category.FieldName) select cf;

				if (
						DMSOrchestrator.LockManager.LockRecord(category, DMSOrchestrator.LockContext, ref lockMsg) &&
						((cFields == null) || DMSOrchestrator.LockManager.LockRecord("DMS_CollectionsField", category.FieldName, DMSOrchestrator.LockContext, ref lockMsg))
					)
				{
					category.DMS_FieldProperty.Disabled = true;
					if (cFields != null)
					{
						foreach (DMS_CollectionsField collField in cFields)
							collField.Disabled = true;
					}

					dc.SubmitChanges();
					DMSOrchestrator.LockManager.UnlockRecord(category, DMSOrchestrator.LockContext);
					if (cFields != null)
						DMSOrchestrator.LockManager.UnlockRecord("DMS_CollectionsField", category.FieldName, DMSOrchestrator.LockContext);
				}
				else
					SetMessage(string.Format(Strings.ErrorDisablingCategory), new Exception(lockMsg), "DisableCategory");
			}
			catch (Exception e)
			{
				SetMessage(string.Format(Strings.ErrorDisablingCategory, categoryName), e, "DisableCategory");
				return false;
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		public bool IsCategoryUsed(string categoryName)
		{
			try
			{
                //prima verifico se la categoria è utilizzata nei documenti archiviati (sono meno rispetto agli attachment)
                var searchFields = from s in dc.DMS_SearchFieldIndexes
                                   join a in dc.DMS_ArchivedDocSearchIndexes
                                   on s.FieldName equals a.DMS_SearchFieldIndex.FieldName
                                   where s.FieldName == categoryName
                                   select s;


                if (searchFields != null && searchFields.Count() > 0)
                    return true; //è usato 

                //se non usata nei documenti archiviati allora verifico negli attachment
                searchFields = from s in dc.DMS_SearchFieldIndexes
                                   join a in dc.DMS_AttachmentSearchIndexes
                                   on s.FieldName equals a.DMS_SearchFieldIndex.FieldName
                                   where s.FieldName == categoryName
                                   select s;


                return (searchFields != null && searchFields.Count() > 0);             			
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorQueryingDatabase, e, "IsCategoryUsed");
				return true;
			}

			
		}

		///<summary>
		/// viene effettuata una query su database per capire se la categoria esiste su database
		/// nella tabella DMS_Field
		///</summary>
		//--------------------------------------------------------------------------------
		public bool CategoryExists(string categoryName)
		{
			try
			{
				int count = dc.DMS_Fields.Count(f => f.FieldName == categoryName && f.IsCategory == true);

				if (count > 0)
					return true;
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorQueryingDatabase, e, "CategoryExists");
				return false;
			}

			return false;
		}

		///<summary>
		/// Fa una query sulla DMS_FieldProperties e ritorna se la categoria e' disabilitata
		///</summary>
		//--------------------------------------------------------------------------------
		public bool IsCategoryDisabled(string categoryName)
		{
			try
			{
				DMS_Field category = GetCategory(categoryName);

				if (category != null)
					return (bool)category.DMS_FieldProperty.Disabled;
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorQueryingDatabase, e, "IsCategoryDisabled");
				return false;
			}

			return false;
		}

		/// <summary>
		/// ritorna i valori di default della categoria passata come parametro
		/// essi vengono deserializzati dalla colonna XMLValues
		/// viene ritornato il valore con l'attributo IsDefault = true
		/// </summary>
		//--------------------------------------------------------------------------------
		public string GetCategoryDefaultValue(string categoryName)
		{
			string defaultCategory = string.Empty;

			DMS_Field category = GetCategory(categoryName);

			if (category != null && category.DMS_FieldProperty.XMLValues != null)
			{
				try
				{
					List<FieldValueState> fValues = FieldPropertiesState.Deserialize(category.DMS_FieldProperty.XMLValues.ToString());

					if (fValues != null)
					{
						foreach (FieldValueState fv in fValues)
						{
							if (fv.IsDefault)
							{
								defaultCategory = fv.FieldValue;
								break;
							}
						}
					}
				}
				catch (Exception e)
				{
					SetMessage(Strings.ErrorLoadingCategoryDefaultValue, e, "GetCategoryDefaultValue");
					return defaultCategory;
				}
			}

			return defaultCategory;
		}

		///<summary>
		/// Dato un CategoryType ritorna la relativa stringa (per la memorizzazione sulla tabella)
		///</summary>
		//--------------------------------------------------------------------------------
		private string GetStringFromCategoryType(CategoryType cType)
		{
			string category = string.Empty;

			switch (cType)
			{
				case CategoryType.Date:
					category = DataType.DataTypeStrings.Date;
					break;

				case CategoryType.Color:
					category = DataType.DataTypeStrings.Enum;
					break;

				case CategoryType.String:
				default:
					category = DataType.DataTypeStrings.String;
					break;
			}

			return category;
		}

		///<summary>
		/// Dati il nome di un field e il suo valore, viene effettuata una query sulla tabella
		/// DMS_SearchFieldIndexes per vedere se questo valori sono gia' stati scelti come bookmark
		///</summary>
		//--------------------------------------------------------------------------------
		internal bool IsASearchField(string fieldName, string fieldValue)
		{
			try
			{
				var count = (from sField in dc.DMS_SearchFieldIndexes
							where (sField.FieldName == fieldName && sField.FieldValue == fieldValue)
							select sField).Count();
				
				return (count > 0);
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.Fail(e.Message);
				return false;
			}
		}

		///<summary>
		/// Eseguo un update sulla tabella DMS_SearchFieldIndexes
		/// sostituendo la stringa contenuta nella colonna FieldValue con la nuova stringa inserita dall'utente
		/// (richiamata per i valori in uso nei bookmark che sono stati modificati dall'utente)
		///</summary>
		//--------------------------------------------------------------------------------
        internal bool UpdateSearchFieldValueIndex(string fieldName, string formattedValue, string newFieldValue)
        {
			try
			{
				var searchFieldIndexes = from sField in dc.DMS_SearchFieldIndexes
                                         where (sField.FieldName == fieldName && sField.FieldValue == formattedValue)
                                         select sField;

				if (searchFieldIndexes == null || !searchFieldIndexes.Any())
					return true;

				foreach (DMS_SearchFieldIndex sField in searchFieldIndexes)
					sField.FieldValue = newFieldValue;

				dc.SubmitChanges();
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorUpdatingSearchIndexes, e, "UpdateSearchFieldValueIndex");
				return false;
			}

			return true;
		}

		///<summary>
		/// Eseguo una cancellazione di un riferimento negli indici di ricerca:
		/// - prima elimino le righe nella tabella DMS_AttachmentSearchIndexes corrispondenti al SearchIndexID, il cui
		/// FieldName e FieldValue (nella tabella DMS_SearchFieldIndexes) e' stato cancellato
		/// - poi elimino il record nella tabella DMS_SearchFieldIndexes con quel FieldName e FieldValue
		/// (richiamata per i valori in uso nei bookmark che sono stati eliminati dall'utente)
		///</summary>
		//--------------------------------------------------------------------------------
		internal bool DeleteIndexes(string fieldName, string fieldValue)
		{
			try
			{
				// elimino tutte le righe con quel SearchIndexID nella tabella DMS_AttachmentSearchIndexes
				var attSearchFieldIndexes = from aSearch in dc.DMS_AttachmentSearchIndexes
											where
											(aSearch.SearchIndexID == aSearch.DMS_SearchFieldIndex.SearchIndexID &&
											aSearch.DMS_SearchFieldIndex.FieldValue == fieldValue &&
											aSearch.DMS_SearchFieldIndex.FieldName == fieldName)
											select aSearch;

				if (attSearchFieldIndexes != null && attSearchFieldIndexes.Any())
				{
					foreach (DMS_AttachmentSearchIndex attSearch in attSearchFieldIndexes)
						dc.DMS_AttachmentSearchIndexes.DeleteOnSubmit(attSearch);

					dc.SubmitChanges();
				}

				// elimino tutte le righe con quel FieldName e FieldValue nella tabella DMS_SearchFieldIndexes
				var searchFieldIndexes = from sField in dc.DMS_SearchFieldIndexes
										 where (sField.FieldName == fieldName && sField.FieldValue == fieldValue)
										 select sField;

				if (searchFieldIndexes != null && searchFieldIndexes.Any())
				{
					foreach (DMS_SearchFieldIndex searchField in searchFieldIndexes)
						dc.DMS_SearchFieldIndexes.DeleteOnSubmit(searchField);

					dc.SubmitChanges();
				}
			}
			catch (Exception e)
			{
				SetMessage(Strings.ErrorDeletingSearchIndexes, e, "DeleteIndexes");
				return false;
			}

			return true;
		}
	}
}