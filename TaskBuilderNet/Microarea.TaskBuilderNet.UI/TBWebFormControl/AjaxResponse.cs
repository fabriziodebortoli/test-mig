using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	/// <summary>
	/// Classe utilizzata per serializzare al client gli update panel da aggiornare in formato JSon.
	/// Per passare meno bytes possibile lungo la rete, uso solo degli acronimi per la serializzazione
	/// delle properties
	/// </summary>
	//================================================================================
	[DataContract]
	internal class AjaxResponse
	{
		/// <summary>
		/// ID del campo che ha il fuoco
		/// </summary>
		[DataMember(Name = "F")]
		public string FocusId;

		/// <summary>
		/// Lista degli UpdatePanel da aggiornare
		/// </summary>
		[DataMember(Name = "P")]
		public List<UpdatePanelData> UpdatePanels = new List<UpdatePanelData>();
		/// <summary>
		/// Lista degli eventuali errori
		/// </summary>
		[DataMember(Name = "E")]
		public List<AjaxError> Errors = new List<AjaxError>();
		/// <summary>
		/// Lista degli eventuali script
		/// </summary>
		[DataMember(Name = "S")]
		public List<string> Scripts = new List<string>();

		//--------------------------------------------------------------------------------
		public void ClearPanels()
		{
			UpdatePanels.Clear();
		}
		//--------------------------------------------------------------------------------
		public void AddPanel(string id, string innerHTML)
		{
			UpdatePanels.Add(new UpdatePanelData(id, innerHTML));
		}

		//--------------------------------------------------------------------------------
		public void AddError(Exception ex)
		{
			Errors.Add(new AjaxError(ex));
		}


		//--------------------------------------------------------------------------------
		internal void Save(Stream writer)
		{

			DataContractJsonSerializer json = new DataContractJsonSerializer(GetType());
			using (XmlDictionaryWriter w = JsonReaderWriterFactory.CreateJsonWriter(writer))
			{
				json.WriteObject(writer, this);
				w.Flush();
			}
		}
	}
	//================================================================================
	/// <summary>
	/// Classe che contiene i dati relativi ad un singolo UpdatePanel da aggiornare
	/// </summary>
	[DataContract]
	internal class UpdatePanelData
	{
		/// <summary>
		/// ID dell'UpdatePanel
		/// </summary>
		[DataMember(Name = "P")]
		public string PanelId;
		/// <summary>
		/// HTML dell'UpdatePanel
		/// </summary>
		[DataMember(Name = "I")]
		public string InnerHTML;

		//--------------------------------------------------------------------------------
		public UpdatePanelData(string id, string innerHTML)
		{
			this.PanelId = id;
			this.InnerHTML = innerHTML;
		}
	}

	//================================================================================
	/// <summary>
	/// Classe che contiene un errore da mandare al client
	/// </summary>
	[DataContract]
	internal class AjaxError
	{
		/// <summary>
		/// Testo del messaggio
		/// </summary>
		[DataMember(Name = "M")]
		public string Message;
		/// <summary>
		/// Stacktrace dell'errore
		/// </summary>
		[DataMember(Name = "S")]
		public string StackTrace;

		//--------------------------------------------------------------------------------
		public AjaxError(Exception ex)
		{
			this.Message = ex.Message;
			this.StackTrace = ex.StackTrace;
		}
	}
}
