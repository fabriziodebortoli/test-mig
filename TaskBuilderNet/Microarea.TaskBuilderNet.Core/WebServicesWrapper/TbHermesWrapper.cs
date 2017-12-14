using System;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.tbHermes;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	public class TbHermesWrapper : ITbWebService
	{
		tbHermes.TbHermes tbHermes;

		string company = "todo";

		public TbHermesWrapper()
		{
			IBasePathFinder pathFinder = BasePathFinder.BasePathFinderInstance;
			this.tbHermes = new tbHermes.TbHermes();
			this.tbHermes.Url = pathFinder.TbHermesUrl;
		}

		//---------------------------------------------------------------------------
		public void WakeUp()
		{
			try
			{
				this.tbHermes.WakeUpAsync(Guid.NewGuid());
			}
			catch { }
		}

		//---------------------------------------------------------------------------
		public bool Init()
		{
			try
			{
				return this.tbHermes.Init();
			}
			catch { return false; }
		}

		//---------------------------------------------------------------------------
		public bool IsAlive()
		{
			try
			{
				return this.tbHermes.IsAlive();
			}
			catch { return false; }
		}
		string ITbWebService.Name { get { return NameSolverStrings.TbHermes; } }
		string ITbWebService.Url { get { return this.tbHermes.Url; } }

		//---------------------------------------------------------------------------
		public ServiceStatus GetUpdatedStatus(ClientIdentifier clientIdentifier)
		{
			return this.tbHermes.GetUpdatedStatus(this.company, clientIdentifier);
		}

		//---------------------------------------------------------------------------
		public void UploadMessage(MailMessage mailMessage)
		{
			this.tbHermes.UploadMessage(this.company, mailMessage);
		}
	}
}
