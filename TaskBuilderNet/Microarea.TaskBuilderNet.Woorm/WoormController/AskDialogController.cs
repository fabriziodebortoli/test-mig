using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Collections.Specialized;

using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;

namespace Microarea.TaskBuilderNet.Woorm.WoormController
{

	public class NameValueField 
	{
		public string name {get;set;}
		public string data {get;set;}
	}

	
	//=========================================================================
	internal class AskDialogController
	{
		private ReportController	reportController = null;
		private AskDialog			askDialog = null;
		private StringCollection	UserChanged				{ get { return askDialog != null ? askDialog.UserChanged : new StringCollection(); }}
		private TbReportSession		ReportSession			{ get { return askDialog.Report.ReportSession; }}
		private Enums				Enums					{ get { return ReportSession.Enums; }}
		
		//--------------------------------------------------------------------------
		public AskDialogController(ReportController controller)
		{
			reportController = controller;
		}
		
		//Metodo che riassegna i valori cambiati/digitati dall'utente ai rispettivi Field
		//--------------------------------------------------------------------------
		internal void AssignAllAskData()
		{	
			this.askDialog = reportController.Report.CurrentAskDialog;

			//TODO Silvano 
			string jsonString = reportController.RequestParams["fields"];

			JavaScriptSerializer ser = new JavaScriptSerializer();
			List<NameValueField> result = ser.Deserialize<List<NameValueField>>(jsonString);

			foreach(NameValueField nvf in result)
			{
				AssignAskData(nvf.name, nvf.data);
			}

			//valuto le espressioni di inizializzazione (che modificano solo i campi NON modificati)
			askDialog.EvalAllInitExpression(null, false);

			//come ultima cosa fa uno step di state machine
			reportController.StateMachine.Step();
		}


		//--------------------------------------------------------------------------
		internal void AssignAskData(string name, string data)
		{
			RSEngine sm = reportController.StateMachine;
				
			Field field = GetFieldByName(name, sm.Report.CurrentAskDialog);
			if (field != null)
			{
				object current;

				if (field.DataType == "DataEnum")
				{
					DataEnum template = (DataEnum)field.AskData;
					current = Enums.LocalizedParse(data, template, ReportSession.CompanyCulture);
				}
				else
				{
					current = ObjectHelper.Parse(data, field.AskData);
				}

				if (!ObjectHelper.IsEquals(current, field.AskData) && !UserChanged.Contains(field.Name))
					UserChanged.Add(field.Name);
				
				field.AskData = current;
				field.GroupByData = current;
			}
		}


		//--------------------------------------------------------------------------
		internal static Field GetFieldByName(string name, AskDialog askDialog)
		{
			foreach (AskGroup g in askDialog.Groups)
				foreach (AskEntry e in g.Entries)
					if (e.Field != null && e.Field.Name == name)
						return e.Field;
			return null;
		}


		
	}
}
