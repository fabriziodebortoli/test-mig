using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace Microarea.EasyAttachment.Components
{
	///<summary>
	/// Classe di appoggio con le info minime per una categoria
	///</summary>
	//================================================================================
	public class CategoryInfo
	{
		public string Name = string.Empty;
		public string OldName = string.Empty;
		public string Description = string.Empty;
		public bool Disabled = false;
		public Color TheColor = Color.White;
		public bool InUse = false;

		public DTCategoriesValues CategoriesValuesDataTable = new DTCategoriesValues();

		//--------------------------------------------------------------------------------
		/*public DTCategoriesValues CategoriesValuesDataTable 
		{
			get { return categoriesValuesDataTable; }
			set { categoriesValuesDataTable = value; }
		}*/

		//--------------------------------------------------------------------------------
		public CategoryInfo()
		{
		}
	}

	///<summary>
	/// Classe serializzabile che identifica un record della tabella DMS_FieldProperties	
	///</summary>
	//================================================================================
	[Serializable]
	public class FieldPropertiesState
	{
		[XmlIgnore]
		public string FieldName = string.Empty;

		private List<FieldValueState> fieldValues = new List<FieldValueState>();
		public List<FieldValueState> FieldValues { get { return fieldValues; } set { fieldValues = value; } }

		//--------------------------------------------------------------------
		public FieldPropertiesState() { }

		/// <summary>
		/// Trasforma i valori di una categoria dalla versione in XML ad una lista di FieldValueState
		/// </summary>
		//--------------------------------------------------------------------
		public static List<FieldValueState> Deserialize(string xmlString)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(FieldPropertiesState));
			using (StringReader sr = new StringReader(xmlString))
			{
				FieldPropertiesState fps = (FieldPropertiesState)serializer.Deserialize(sr);

				if (fps == null)
				{
					System.Diagnostics.Debug.Assert(false);
					return null;
				}
				return fps.FieldValues;
			}
		}

		/// <summary>
		/// Trasforma i valori di una categoria caricati in memoria in una stringa da salvare su database
		/// </summary>
		//--------------------------------------------------------------------
		public string Serialize()
		{
			// prima di serializzare faccio pulizia nella lista dei valori eliminando le righe prive di caratteri
			for (int i = FieldValues.Count - 1; i >= 0; i--)
			{
				FieldValueState fvs = FieldValues[i];
				if (string.IsNullOrWhiteSpace(fvs.FieldValue))
					FieldValues.RemoveAt(i);
			}

			XmlSerializer serializer = new XmlSerializer(typeof(FieldPropertiesState));
			using (StringWriter sw = new StringWriter())
			{
				serializer.Serialize(sw, this);
				return sw.ToString();
			}
		}
	}

	///<summary>
	/// Classe serializzabile che identifica un valore della categoria
	///</summary>
	//================================================================================
	[Serializable]
	public class FieldValueState
	{
		private string fieldValue = string.Empty;
		private bool isDefault = false;

		public string FieldValue { get { return fieldValue; } set { fieldValue = value; } }

		[XmlAttribute(AttributeName = "isDefault")]
		public bool IsDefault { get { return isDefault; } set { isDefault = value; } }

		//--------------------------------------------------------------------
		public FieldValueState() { }
	}
}