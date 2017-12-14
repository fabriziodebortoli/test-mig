using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.CoreTypes;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Core.Lexan;
using Microarea.TaskBuilderNet.Woorm.WoormEngine;

namespace Microarea.TaskBuilderNet.Woorm.WoormController
{
    class HttpGenericHandler
	{

		//********************************************************************************************************************************
		//siccome quei furboni di microzozz non hanno fatto derivare i due oggetti da una stessa interfaccia o classe base, devo fare uno
		//switch sugli oggetti contenuti per non duplicare il codice
		//********************************************************************************************************************************
		HttpListenerRequest listenerRequest;
		HttpListenerResponse listenerResponse;
		HttpRequest request;
		HttpResponse response;
		internal Stream OutputStream { get { return listenerResponse == null ? response.OutputStream : listenerResponse.OutputStream; } }
		internal Uri RequestUrl { get { return listenerRequest == null ? request.Url : listenerRequest.Url; } }
		internal NameValueCollection Params { get { return listenerRequest == null ? request.Params : listenerRequest.QueryString; } }
		internal bool IsMySession { get { return listenerRequest == null ? true : Params["sessionID"] == TBWebContext.Current.SessionID; } }

		internal String ContentType { set { if (listenerResponse == null) response.ContentType = value; else listenerResponse.ContentType = value; } }

		//********************************************************************************************************************************

		//-----------------------------------------------------------------------------
		internal HttpGenericHandler(HttpListenerRequest listenerRequest,HttpListenerResponse listenerResponse)
		{
			this.listenerRequest = listenerRequest;
			this.listenerResponse = listenerResponse;
		}

		//-----------------------------------------------------------------------------
		internal HttpGenericHandler(HttpRequest request,HttpResponse response)
		{
			this.request = request;
			this.response = response;
		}

		//-----------------------------------------------------------------------------
		internal bool ProcessRequest()
		{
			string subPath = RequestUrl.LocalPath;
			int idx = -1;
			if ((idx = subPath.IndexOf("WoormHandler.axd")) != -1)
			{
				subPath = subPath.Substring(idx + 16);//lunghezza di WoormHandler.axd
			}

			//nel caso non stia usando Asp.Net, devo controllare io la corrispondenza fra i cookie di sessione
			if (!IsMySession)
				return false;

			if (subPath == "/RunReport")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(false);
				controller.StateMachine.Step();

				return Render(controller);
			}
			if (subPath == "/OkAsk")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				controller.StateMachine.Step();
				controller.StateMachine.CurrentState = State.ExecuteAsk;
				controller.StateMachine.Step();
				return Render(controller);
			}

			if (subPath == "/GetReportData")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				controller.StateMachine.Step();
				WriteReportContent(controller);
				return true;
			}

			if (subPath == "/ScrollNextPage")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				controller.StateMachine.Step();
				controller.NextPage();
				WriteReportContent(controller);
				return true;
			}

			if (subPath == "/ScrollPrevPage")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				controller.StateMachine.Step();
				controller.PrevPage();
				WriteReportContent(controller);
				return true;
			}

			if (subPath == "/ScrollFirstPage")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				controller.StateMachine.Step();
				controller.FirstPage();
				WriteReportContent(controller);
				return true;
			}

			if (subPath == "/ScrollLastPage")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				controller.StateMachine.Step();
				controller.LastPage();
				WriteReportContent(controller);
				return true;
			}

			if (subPath == "/SaveReport")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				
				string filePath = PathFunctions.WoormTempFilePath("aaa", "aaa") + Path.DirectorySeparatorChar + "b.wrm";
				using (Unparser unparser = new Unparser())
				{
					unparser.Open(filePath);
					controller.StateMachine.Woorm.Unparse(unparser);
					controller.StateMachine.Report.Unparse(unparser);
				}

				//write file to stream
				ContentType = "text/plain"; 
				FileInfo file = new FileInfo(filePath);
				int len = (int)file.Length, bytes;
				byte[] buffer = new byte[1024];
				try
				{
					using(Stream stream = File.OpenRead(filePath)) {
						while (len > 0 && (bytes =	stream.Read(buffer, 0, buffer.Length)) > 0)
						{
							OutputStream.Write(buffer, 0, bytes);
							len -= bytes;
						}
					}
				}
				catch(Exception)
				{
				}
				return true;
			}

			if (subPath == "/SetReportData")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
				if (controller.StateMachine == null)
					return false;
				
				string JsonObject =  Params["JsonData"];
				DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(ReportData));
				MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonObject));
				object woormObject = json.ReadObject(stream);

				//recuperato woormdocument dalla statemachine, si aggiorna l'array degli oggetti 
				controller.StateMachine.Woorm.Objects.UpdateObject(woormObject);
				return true;
			}

			if (subPath == "/GetReportEngine")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
 
				WriteReportEngine(controller);
			}

			if (subPath == "/GetDBObjects")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
 
				WriteDatabaseObjects(controller);
			}

			if (subPath == "/GetColumns")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
 
				string tableName = Params["table"];
				WriteTableColumns(controller, tableName);
			}

			if (subPath == "/GetAskDialogData")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
 
				WriteAskDialog(controller);
			}

			if (subPath == "/UpdateAskDialogData")
			{
				ReportController controller = ReportController.FromSession(Params);
				controller.InitStateMachine(true);
 
				//TODO Silvano: aggiornare i field referenziati prima di renderizzare di nuovo l'askdialog
				AskDialogController askdialogController = new AskDialogController(controller);
				askdialogController.AssignAllAskData();
				WriteAskDialog(controller);
			}
			
			return false;
		}


		//-----------------------------------------------------------------------------
		private void WriteTableColumns(ReportController controller, string tableName)
		{
			try
			{
				ContentType = "application/json";
				DBTableColumns dbCols = new DBTableColumns();
				RSEngine sm = controller.StateMachine;
				if (sm != null && sm.Report != null)
				{
					//@@@@@@@Legge Tabelle da DB
					TBConnection connection = new TBConnection(sm.Report.ReportSession.CompanyDbConnection, TBDatabaseType.GetDBMSType(sm.Report.ReportSession.Provider));
					connection.Open();
					CatalogInfo catalogInfo = new CatalogInfo();
					catalogInfo.Load(connection, true);
					dbCols.columns = catalogInfo.GetColumnsInfo(tableName, connection);
				}
				dbCols.ready = true;
			
				DataContractJsonSerializer json = new DataContractJsonSerializer(dbCols.GetType());
				json.WriteObject(OutputStream, dbCols);
			}
			catch (Exception)
			{
			}
		}

		//-----------------------------------------------------------------------------
		private void WriteDatabaseObjects(ReportController controller)
		{
			try
			{
				ContentType = "application/json";
				DBObjects dbo = new DBObjects();
				RSEngine sm = controller.StateMachine;
				if (sm != null && sm.Report != null)
				{
					//@@@@@@@Legge Tabelle da DB
					TBConnection connection = new TBConnection(sm.Report.ReportSession.CompanyDbConnection, TBDatabaseType.GetDBMSType(sm.Report.ReportSession.Provider));
					connection.Open();
					dbo.catalog = new CatalogInfo();
					dbo.catalog.Load(connection, true);
					connection.Close();
					connection.Dispose();
				}
				dbo.ready = true;
			
				DataContractJsonSerializer json = new DataContractJsonSerializer(dbo.GetType());
				json.WriteObject(OutputStream, dbo);
			}
			catch (Exception)
			{
			}

		}
		//-----------------------------------------------------------------------------
		private void WriteReportEngine(ReportController controller)
		{
			try
			{
				ContentType = "application/json";
				ReportEngineData d = new ReportEngineData();
				RSEngine sm = controller.StateMachine;
				if (sm != null && sm.Report != null)
				{
					d.report = sm.Report;
				
					using (Unparser unparser = new Unparser())
					{
						foreach (RuleObj obj in d.report.Engine.SortedRules)
						{	
							unparser.Open();
							obj.Unparse(unparser);
							d.rules.Add(unparser.GetResultString());
							unparser.Close();
						}
					}
				}
				d.ready = true;
			
				DataContractJsonSerializer json = new DataContractJsonSerializer(d.GetType());
				json.WriteObject(OutputStream, d);
			}
			catch (Exception)
			{
			}

		}

		//-----------------------------------------------------------------------------
		private void WriteReportContent(ReportController controller)
		{
			if (controller.RenderingStep() == HtmlPageType.Error)
			{
				Render(controller);
				return;
			}
			try
			{
				ContentType = "application/json";
				ReportData d = new ReportData();
				RSEngine sm = controller.StateMachine;
				if (controller.IsPageReady())
				{
					//prova, carica solo la prima pagina
					sm.Woorm.RdeReader.LoadPage();
					d.ready = true;
					d.reportObjects = sm.Woorm.Objects;
		
					//converto sul server le dimensioni in unita logiche
					d.paperLength = (short)UnitConvertions.MUtoLP(sm.Woorm.PageInfo.DmPaperLength, UnitConvertions.MeasureUnits.CM, 100, 5);
					d.paperWidth = (short)UnitConvertions.MUtoLP(sm.Woorm.PageInfo.DmPaperWidth, UnitConvertions.MeasureUnits.CM, 100, 5);
				}
				else
				{
					d.ready = false;
				}
				DataContractJsonSerializer json = new DataContractJsonSerializer(d.GetType());
				json.WriteObject(OutputStream, d);
			}
			catch (Exception e)
			{
				//TODOPERASSO clear
				ReportData d = new ReportData();
				d.ready = true;
				d.error = true;
				d.message = e.Message;
				DataContractJsonSerializer json = new DataContractJsonSerializer(d.GetType());
				json.WriteObject(OutputStream, d);
			}

		}

		//-----------------------------------------------------------------------------
		private bool Render(ReportController controller)
		{
			switch (controller.RenderingStep())
			{
				case HtmlPageType.Error:
					{
						ReportData d = new ReportData();
						d.ready = true;
						d.error = true;
						StringBuilder sb = new StringBuilder();
						//TODOPERASSO distinguere errori da warning
						foreach (string line in controller.StateMachine.Errors)
						{
							sb.AppendLine(line);
						}

						foreach (string line in controller.StateMachine.Warnings)
						{
							sb.AppendLine(line);
						}
						d.message = sb.ToString();
						DataContractJsonSerializer json = new DataContractJsonSerializer(d.GetType());
						json.WriteObject(OutputStream, d);
						break;
					}
				
				case HtmlPageType.Viewer:
					{
						//inietto lo script che mi chiederà periodicamente se il report è pronto
						ContentType = "application/json";
						string sessionData = string.Concat("{\"", ReportController.StateMachineSessionTagIdentifier, "\":\"", controller.StateMachineSessionTag, "\", \"sessionID\": \"", TBWebContext.Current.SessionID, "\"}");
						using (StreamWriter sw = new StreamWriter(OutputStream))
							sw.Write(sessionData);
						return true;
					}

				case HtmlPageType.Print:
					{
						break;
					}

				case HtmlPageType.Form:
					{
						WriteRequestAsk(controller);
						return true;
					}

				case HtmlPageType.HotLink:
					{
						break;
					}

				case HtmlPageType.Persister:
					{
						break;
					}
			}

			return false;
		}

		//manda al client informazioni sulla statemachine e gli dice se deve renderizzare un'askdialog o no
		//-----------------------------------------------------------------------------
		private void WriteRequestAsk(ReportController controller)
		{	
			string sessionData = string.Concat("{\"ask\": \"true\", \"", ReportController.StateMachineSessionTagIdentifier, "\":\"", controller.StateMachineSessionTag, "\", \"sessionID\": \"", TBWebContext.Current.SessionID, "\"}");
			using (StreamWriter sw = new StreamWriter(OutputStream))
			{
				ContentType = "application/json";
				sw.Write(sessionData);
			}
		}

		//-----------------------------------------------------------------------------
		private void WriteAskDialog(ReportController controller)
		{
			if (controller.RenderingStep() == HtmlPageType.Error)
			{
				Render(controller);
				return;
			}
			try
			{
				ContentType = "application/json";
				AskDialogData d = new AskDialogData();
				RSEngine sm = controller.StateMachine;
				
				d.ready = true;
				d.askDialog = sm.Report.CurrentAskDialog;
		
				DataContractJsonSerializer json = new DataContractJsonSerializer(d.GetType());
				json.WriteObject(OutputStream, d);
			}
			catch (Exception ex)
			{
				AskDialogData d = new AskDialogData();
				d.message = ex.ToString();
				d.error = true;
				DataContractJsonSerializer json = new DataContractJsonSerializer(d.GetType());
				json.WriteObject(OutputStream, d);
			}
		}
	}
}
