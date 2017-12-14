using System;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers
{
	///<summary>
	/// Esempio di risposta
	/// In caso di invii multipli vengono ritornati tutti i risultati in ordine posizionale.
	/// 
	/// <Results xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" userid="1" xmlns="">
	/// <Result code="OtherError" operation="Set" desc="Input string was not in a correct format.">
    /// <Exception>
    ///  <Message>System.FormatException: Input string was not in a correct format.   
	///  at System.Number.StringToNumber(String str, NumberStyles options, NumberBuffer&amp; number, NumberFormatInfo info, Boolean parseDecimal)   
	///  at System.Number.ParseInt32(String s, NumberStyles style, NumberFormatInfo info)   at System.String.System.IConvertible.ToInt32(IFormatProvider provider)   
	///  at System.Convert.ChangeType(Object value, Type conversionType, IFormatProvider provider)   at System.Convert.ChangeType(Object value, Type conversionType)   
	///  at PAT.CRM.Global.Record.ConvertToPrimaryKeyType(Object oValue)   at PAT.CRM.CustomerAct.Record.FindRecord(Object ID)   at PAT.CRM.Global.Record.Find(Object ID)   
	///  at PAT.CRM.WSC4.ICRM.uniSetProvider.SetObject(SetProviderRequest oRequest, BaseObject Obj) in C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.ICRM\UniversalProvider\SetProvider.vb:line 797   
	///  at PAT.CRM.WSC4.Runtime.Operations.SetProvider.Execute(SetProviderRequest request) in C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.Runtime\Operations\Set\SetProvider.vb:line 15   
	///  at PAT.CRM.WSC4.Runtime.WSCLegacyOperationProvider`2.ExecuteOperation(WSCContext Context, WSCOperationRequest Request) in 
	///  C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.Runtime\Engine\Description\WSCOperationProvider.vb:line 116   
	///  at PAT.CRM.WSC4.Runtime.WSCEngine.ExecuteOperation(WSCContext Context, WSCOperation operation) in C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.Runtime\Engine\WSCEngine.vb:line 196</Message>
    ///  <StackTrace>   at System.Number.StringToNumber(String str, NumberStyles options, NumberBuffer&amp; number, NumberFormatInfo info, Boolean parseDecimal)   
	///  at System.Number.ParseInt32(String s, NumberStyles style, NumberFormatInfo info)   at System.String.System.IConvertible.ToInt32(IFormatProvider provider)  
	///  at System.Convert.ChangeType(Object value, Type conversionType, IFormatProvider provider)   at System.Convert.ChangeType(Object value, Type conversionType)   
	///  at PAT.CRM.Global.Record.ConvertToPrimaryKeyType(Object oValue)   at PAT.CRM.CustomerAct.Record.FindRecord(Object ID)   at PAT.CRM.Global.Record.Find(Object ID)  
	///  at PAT.CRM.WSC4.ICRM.uniSetProvider.SetObject(SetProviderRequest oRequest, BaseObject Obj) in C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.ICRM\UniversalProvider\SetProvider.vb:line 797   
	/// at PAT.CRM.WSC4.Runtime.Operations.SetProvider.Execute(SetProviderRequest request) in C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.Runtime\Operations\Set\SetProvider.vb:line 15   
	/// at PAT.CRM.WSC4.Runtime.WSCLegacyOperationProvider`2.ExecuteOperation(WSCContext Context, WSCOperationRequest Request) 
	/// in C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.Runtime\Engine\Description\WSCOperationProvider.vb:line 116   
	/// at PAT.CRM.WSC4.Runtime.WSCEngine.ExecuteOperation(WSCContext Context, WSCOperation operation) in C:\Development\PAT.CRM.vNext\PAT.CRM.WSC4.Runtime\Engine\WSCEngine.vb:line 196</StackTrace>
    /// </Exception>
	/// </Result>
	/// <Result code="OK" operation="Set">
    ///		<PriceList id="25" />
	/// </Result>
	/// </Results>
	///
	/// OK 	                Operazione completata con successo
	/// LogonFailed 	    Username o password errata
	/// NotSupported	    Operazione non supportata
	/// NotSpecified	    Attributo obbligatorio non specificato
	/// MalformedOperation	Formato XML dell’operazione errata
	/// NotAllowed	        Operazione non permessa
	/// UpdateFailed	    Errore durante l’aggiornamento dell’informazione
	/// ObjectNotFound	    L’oggetto richiesto non è stato trovato
	/// ObjectNotDefined	L’oggetto richiesto non è definite
	/// OtherError	        Eventuali altri errori
	///</summary>

	//================================================================================
	internal class PatSynchroResponseInfo
	{
		// contiene una lista di oggetti di tipo Result perche' a fronte di un invio multiplo 
		// potrei avere una lista di risultati ordinati
		public List<Result> Results = new List<Result>();

		//--------------------------------------------------------------------------------
		public SynchroStatusType GetSynchroStatusByPosition(int position)
		{
			// TODO: gestire tutti i codici possibili (da chiedere a Pat)
			if (Results.Count > position)
				return (String.Compare(Results[position].Code, "ok", StringComparison.InvariantCultureIgnoreCase) == 0) ? SynchroStatusType.Synchro : SynchroStatusType.Error;

			return SynchroStatusType.Error;
		}

		//--------------------------------------------------------------------------------
		public Result GetResultByPosition(int position)
		{
			if (Results.Count > position)
				return Results[position];

			return null;
		}
	}

	///<summary>
	/// Classe che identifica il singolo nodo di tipo Result
	/// 
	/// Normalmente una Set senza errori ha un testo di questo tipo:
	/// <Result code="OK" operation="Set">
	///		<PriceList id="25" />
	/// </Result>
	/// 
	/// Altrimenti:
	/// <Result code="OtherError" operation="Set" desc="Input string was not in a correct format.">
	/// <Exception>
	///  <Message>System.FormatException: Input string was not in a correct format...</Message>
	///  <StackTrace>   at System.Number.StringToNumber(String str, NumberStyles options, NumberBuffer&amp; number, NumberFormatInfo info, Boolean parseDecimal)... </StackTrace>
	/// </Exception>
	/// </Result>
	/// 
	///</summary>
	//================================================================================
	internal class Result
	{
		public string Code { get; set; }
		public string Operation { get; set; }
		public string Description { get; set; }

		public string Message { get; set; }
		public string StackTrace { get; set; }

		public string EntityName { get; set; }
		public string Id { get; set; }

		public string ExtendedErrorDescription
		{
			get { return InfiniteCRMSynchroResponseXML.Attribute.Code + "=" + Code + " " + InfiniteCRMSynchroResponseXML.Attribute.Operation + "=" + Operation + " " + InfiniteCRMSynchroResponseXML.Attribute.Desc + "=" + Description; }
		}

		//--------------------------------------------------------------------------------
		public Result()
		{
		}

		//--------------------------------------------------------------------------------
		public SynchroStatusType GetResultSynchroStatus()
		{
			// TODO: gestire tutti i codici possibili (da chiedere a Pat)
			return (String.Compare(Code, "ok", StringComparison.InvariantCultureIgnoreCase) == 0) ? SynchroStatusType.Synchro : SynchroStatusType.Error;
		}
	}
}
