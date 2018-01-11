using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.TaskBuilderNet.TbLoaderService
{
	internal class Engine
	{
		private Diagnostic maServerDiagnostic = new Diagnostic(Diagnostic.EventLogName);//maserver generico
		private TBLoaderCache cache = new TBLoaderCache();
		/// <summary>
		/// crea un tbloader da tenere a disposizione, per velocizzare il soddisfacimento delle richieste
		~Engine()
		{
			StopAll();
		}
		/// </summary>
		internal void CreateSlot(TBLoaderInstance tbInstanceToIgnore)
		{
			Task.Run(() =>
			{
				if (!cache.HasFreeSlot(tbInstanceToIgnore))
					Start(new TBLoaderCommand() { ClientId = "", Type = TBLoaderCommand.CommandType.Start }, true);
			});

		}

		internal void StopAll()
		{
			lock (this)
			{
				TBLoaderInstance tbloader = null;
				while ((tbloader = cache.GetFirstTbLoader()) != null)
					Close(tbloader, true);
			}

		}

		private void Close(TBLoaderInstance tbloader, bool safe)
		{
			try
			{
                tbloader.TbLoader.SetTimeout(TimeSpan.FromSeconds(30));
                tbloader.TbLoader.CloseTB();
			}
			catch
			{
				if (!safe)
					throw;
				try
				{
					tbloader.TbLoader.KillProcess();
				}
				catch
				{
					if (!safe)
						throw;
				}
			}
		}

		/// <summary>
		/// Fa partire un tbloader
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		internal TBLoaderResponse Start(TBLoaderCommand cmd, bool forLaterUse)
		{
			TBLoaderInstance tbInstance = null;
			try
			{
				tbInstance = forLaterUse ? null : cache.GetTbLoader(cmd.ClientId);
				if (tbInstance == null)
				{
					//semaforo; non devo lanciarli contemporaneamente altrimenti uno ruba la porta all'altro
					lock (this)
					{
						//ci riprovo, potrebbe essere stata aggiunta da un altro thread
						tbInstance = cache.GetTbLoader(cmd.ClientId);
						if (tbInstance == null)
						{
							Message("Starting new tbloader...", DiagnosticType.LogInfo);
							tbInstance = new TBLoaderInstance() { ClientId = cmd.ClientId };
							tbInstance.TbLoader = new TbLoaderClientInterface(BasePathFinder.BasePathFinderInstance, "");
							tbInstance.TbLoader.StartTbLoader("TbLoaderService", true);

							Diagnostic tbDiagnostic = tbInstance.TbLoader.GetApplicationContextDiagnostic(true);
							if (!tbInstance.TbLoader.Connected || tbDiagnostic.TotalErrors > 0)
							{
								string err = string.Concat("Error starting TBLOADER",
										Environment.NewLine,
										"'tbAppClientInterface.Connected' value: ",
										tbInstance.TbLoader.Connected,
										Environment.NewLine,
										(tbDiagnostic.TotalErrors > 0 ? "tbDiagnostic errors: " + tbDiagnostic.ToString() : "No errors in tbDiagnostic")
										);
								Close(tbInstance, true);
								throw new Exception(err);
							}

							cache.AddTbLoader(tbInstance);
						}

					}
				}
				else
				{
					tbInstance.TbLoader = tbInstance.TbLoader;
				}
			}
			catch (Exception ex)
			{
				Message(ex.Message, DiagnosticType.LogInfo | DiagnosticType.Error);
				TBLoaderResponse res = new TBLoaderResponse();
				res.Result = false;
				res.Message = ex.ToString();
				return res;
			}
			TBLoaderResponse res1 = new TBLoaderResponse();
			res1.Result = true;
			res1.ProcessId = tbInstance.TbLoader.TbProcessId;
			res1.Port = tbInstance.TbLoader.TbPort;
			Message(string.Format("Started tbloader, port:{0}, process id: {1}", res1.Port, res1.ProcessId), DiagnosticType.LogInfo);

			//creo un tb aggiuntivo, per velocizzare il soddisfacimento di future richieste
			//if (!forLaterUse)
			//	CreateSlot(tbInstance);
			return res1;
		}



		/// <summary>
		/// Arresta un tbloader
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		internal TBLoaderResponse Stop(TBLoaderCommand cmd)
		{
			TBLoaderInstance tbInstance = cache.GetTbLoader(cmd.ProcessId);
			if (tbInstance == null)
			{
				TBLoaderResponse res = new TBLoaderResponse();
				res.Result = false;
				res.Message = "Invalid process id: " + cmd.ProcessId;
				return res;
			}

			try
			{
				Close(tbInstance, false);
			}
			catch (Exception ex)
			{

				TBLoaderResponse res = new TBLoaderResponse();
				res.Result = false;
				res.Message = ex.ToString();
				return res;
			}
			TBLoaderResponse res1 = new TBLoaderResponse();
			res1.Result = true;
			return res1;
		}



		internal void Message(string msg, DiagnosticType type)
		{
			if (maServerDiagnostic != null)
			{
				maServerDiagnostic.Set(type, msg);
			}

			Console.WriteLine(msg);
		}
	}
}
