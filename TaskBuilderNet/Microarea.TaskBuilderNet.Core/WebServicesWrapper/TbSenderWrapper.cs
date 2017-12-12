using System;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	/// <summary>
	/// Wrapper per l'utilizzo di lock manager
	/// </summary>
	//============================================================================
	public class TbSenderWrapper : ITbWebService
	{
		private tbSender.PLProxy tbSender = new tbSender.PLProxy();

		/// <summary>
		/// Costruttore
		/// </summary>
		/// <param name="lockManagerUrl">Indirizzo di lock manager</param>
		//---------------------------------------------------------------------------
		public TbSenderWrapper(string tbSenderUrl)
		{
			tbSender.Url = tbSenderUrl;
		}

		//---------------------------------------------------------------------------
		public void WakeUp()
		{
			try
			{
				tbSender.WakeUpAsync(Guid.NewGuid());
			}
			catch {}
		}

        //---------------------------------------------------------------------------
        public bool Init()
        {
            try
            {
              return tbSender.Init();
            }
            catch { return false; }
        }

        //---------------------------------------------------------------------------
        public bool IsAlive()
        {
            try
            {
                return tbSender.IsAlive();
            }
            catch { return false; }
        }
		string ITbWebService.Name { get { return NameSolverStrings.TbSender; } }
		string ITbWebService.Url { get { return this.tbSender.Url; } }
	}
}

