using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers
{
	///<summary>
	/// <ExecuteSyncroResult>
	/// <Process id="790558900" AtomicLevel="ENTITY" GenericResult="ok" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	///		<MAGO_PAYMENT Result="(Insert/Update/Delete):Entities processed:1 - Errors:0 "/>
	/// </Process>
	/// <MAGO_PAYMENT applicationId="DEMO">
	///		<Update_MAGO_PAYMENT PAPAYMENTID_K="ACP" Crc=""/>
	///		<Add_MAGO_PAYMENT_00002 PAPAYMENTID_K="ACP" PDNUMRAT_K="003" Crc=""/>
	/// </MAGO_PAYMENT>
	/// </summary>
	/// </ExecuteSyncroResult>
	
	/// <ExecuteSyncroResult>
	/// <Process id="695025147" AtomicLevel="ENTITY" GenericResult="ok" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	///		<MAGO_PAYMENT Result="(Insert/Update/Delete):Entities processed:1 - Errors:1 "/>
	/// </Process>
	/// <MAGO_PAYMENT applicationId="DEMO">
	///		<Add_MAGO_PAYMENT PAPAYMENTID_K="CASH" Crc="" ErrorCode="422@@@" ErrorOwnerCode="001@@@" ErrorTable="ba_pagdett@@@" 
	///		Error="Errore durante l&apos;esecuzione della trascodifica per il campo [PDTIPPAG]. Dettaglio tecnico [Il valore originale [2686976   ] non è specificato nella trascodifica [Tipo pagamento codifica]]@@@"/>
	///		<Add_MAGO_PAYMENT_00002 PAPAYMENTID_K="CASH" PDNUMRAT_K="001" Crc="" ErrorCode="422@@@" ErrorOwnerCode="001@@@" ErrorTable="ba_pagdett@@@" 
	///		Error="Errore durante l&apos;esecuzione della trascodifica per il campo [PDTIPPAG]. Dettaglio tecnico [Il valore originale [2686976   ] non è specificato nella trascodifica [Tipo pagamento codifica]]@@@"/>
	///	</MAGO_PAYMENT>
	/// </summary>
	/// </ExecuteSyncroResult>

	//================================================================================
	internal class KeyAttribute
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}

	//================================================================================
	internal class ActionOperation
	{
		public string Name { get; set; }
		public List<KeyAttribute> Keys = new List<KeyAttribute>();
		public string ErrorCode { get; set; }
		public string ErrorOwnerCode { get; set; }
		public string ErrorTable { get; set; }
		public string Error { get; set; }
	}

	//================================================================================
	internal class ActionDetail
	{
		public string Name { get; set; }
		public string ApplicationId { get; set; }

		public List<ActionOperation> Operations = new List<ActionOperation>();

		//--------------------------------------------------------------------------------
		public ActionDetail() { }
	}

	//================================================================================
	internal class ActionResult
	{
		public string Name { get; set; }
		public string Result { get; set; } // bisognerebbe distinguere il nr. delle entita' processate e il nr. degli errori
		public string EntitiesProcessed { get; set; }
		public string Errors { get; set; } 
	}

	//================================================================================
	internal class ProcessInfo
	{
		public string Id { get; set; }
		public string AtomicLevel { get; set; }
		public string GenericResult { get; set; }
		public string ErrorMessage { get; set; }

		public ActionResult ActionResult = new ActionResult();

		//--------------------------------------------------------------------------------
		public ProcessInfo() 
		{
			Id = string.Empty;
			AtomicLevel = string.Empty;
			GenericResult = string.Empty;
			ErrorMessage = string.Empty;
		}
	}

	//================================================================================
	internal class SynchroResponseInfo
	{
		public ProcessInfo ProcessInfo = new ProcessInfo();
		public ActionDetail ActionDetail = new ActionDetail();

		//--------------------------------------------------------------------------------
		public SynchroResponseInfo() {}
	}
}
