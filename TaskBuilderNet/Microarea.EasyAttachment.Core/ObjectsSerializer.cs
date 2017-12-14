using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Microarea.EasyAttachment.Core
{
	# region ChangedFieldsState
	///<summary>
	/// Classe serializzabile che identifica un record della tabella DMS_IndexesSynchronization	
	///</summary>
	//================================================================================
	[Serializable]
	public class ChangedFieldsState
	{
		[XmlIgnore]
		public int ErpDocumentID = -1;

		private List<ChangedField> fields = new List<ChangedField>();

		//--------------------------------------------------------------------------------
		public List<ChangedField> Fields { get { return fields; } set { fields = value; } }

		//--------------------------------------------------------------------
		public ChangedFieldsState() { }

		/// <summary>
		/// Trasforma la colonna ChangedFields della tabella DMS_IndexesSynchronization dalla sua versione in XML 
		/// ad un oggetto di tipo ChangedFieldsState in memoria
		/// </summary>
		//--------------------------------------------------------------------
		public static ChangedFieldsState Deserialize(string xmlString)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(ChangedFieldsState));
			using (StringReader sr = new StringReader(xmlString))
			{
				ChangedFieldsState ss = (ChangedFieldsState)serializer.Deserialize(sr);

				if (ss == null)
				{
					Debug.Assert(false);
					return null;
				}
				return ss;
			}
		}

		/// <summary>
		/// Trasforma l'oggetto di tipo ChangedFieldsState caricato in memoria in una stringa da salvare su database
		/// </summary>
		//--------------------------------------------------------------------
		public string Serialize()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(ChangedFieldsState));
			using (StringWriter sw = new StringWriter())
			{
				serializer.Serialize(sw, this);
				return sw.ToString();
			}
		}
	}

	///<summary>
	/// Classe serializzabile con i valori di un singolo field
	///</summary>
	//================================================================================
	[Serializable]
	public class ChangedField
	{
		private string name = string.Empty;
		private string fieldValue = string.Empty;

		//--------------------------------------------------------------------------------
		[XmlAttribute(AttributeName = "name")]
		public string Name { get { return name; } set { name = value; } }
		//--------------------------------------------------------------------------------
		[XmlAttribute(AttributeName = "value")]
		public string Value { get { return fieldValue; } set { fieldValue = value; } }

		//--------------------------------------------------------------------
		public ChangedField() { }
	}
	# endregion		
}
