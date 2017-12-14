using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microarea.TaskBuilderNet.TbLoaderService
{
	internal class TBLoaderInstance
	{
		public string ClientId { get; set; }
		public TbLoaderClientInterface TbLoader { get; set; }
	}
	class TBLoaderCache
	{
		private List<TBLoaderInstance> tbLoaders = new List<TBLoaderInstance>();
        ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();
        public bool HasFreeSlot(TBLoaderInstance instanceToIgnore)
		{
			using (Locker l = new Locker(rwLock, false))
			{
				foreach (TBLoaderInstance item in tbLoaders)
				{
					if (item == instanceToIgnore)
						continue;
					if (item.ClientId == "" || item.TbLoader.GetLogins() == 0)//ne ho già uno disponibile
						return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Ritorna un'istanza di tbloader dato il process id
		/// </summary>
		/// <param name="processId"></param>
		/// <returns></returns>
		internal TBLoaderInstance GetTbLoader(int processId)
		{
            using (Locker l = new Locker(rwLock, false))
            {
				//prima provo a vedere se c'è il suo
				foreach (var item in tbLoaders)
				{
					if (item.TbLoader.TbProcessId == processId)
						return item;
				}
			}
			return null;
		}
		/// <summary>
		/// Ritorna la prima istanza di tbloader
		/// </summary>
		/// <param name="processId"></param>
		/// <returns></returns>
		internal TBLoaderInstance GetFirstTbLoader()
		{
            using (Locker l = new Locker(rwLock, false))
            {
				return tbLoaders.Count == 0 ? null : tbLoaders[0];
			}
		}
		/// <summary>
		/// Ritorna un'istanza di tbloader dato il client id (l'id del client che ha effettuato la richiesta di avere 
		/// un tbloader, di solito corrisponde all'id di sessione asp.net)
		/// </summary>
		/// <param name="clientId"></param>
		/// <returns></returns>
		internal TBLoaderInstance GetTbLoader(string clientId)
		{
            using (Locker l = new Locker(rwLock, false))
            {
				//prima provo a vedere se c'è il suo
				foreach (var item in tbLoaders)
				{
					if (item.ClientId == clientId)
						return item;
				}
				//poi ne cerco uno non ancora assegnato, oppure che è disconnesso
				foreach (var item in tbLoaders)
				{
					if (item.ClientId == "" || item.TbLoader.GetLogins() == 0)
					{
						item.ClientId = clientId;
						return item;
					}
				}
			}
			return null;
		}
		/// <summary>
		/// Rimuove un tbloader dalla cache
		/// </summary>
		/// <param name="instance"></param>
		private void RemoveTbLoader(TbLoaderClientInterface instance)
		{
            using (Locker l = new Locker(rwLock, true))
            {
				for (int i = 0; i < tbLoaders.Count; i++)
				{
					TBLoaderInstance tbInst = tbLoaders[i];
					if (tbInst.TbLoader == instance)
					{
						tbLoaders.RemoveAt(i);
						break;
					}
				}
			}
		}
		/// <summary>
		/// Aggiunge un tbloader alla cache
		/// </summary>
		/// <param name="tbInstance"></param>
		internal void AddTbLoader(TBLoaderInstance tbInstance)
		{
            using (Locker l = new Locker(rwLock, true))
            {
				tbLoaders.Add(tbInstance);
				tbInstance.TbLoader.TBLoaderExited += (sender, args) => RemoveTbLoader(sender as TbLoaderClientInterface);
			}
		}
	}

    class Locker : IDisposable
    {
        ReaderWriterLockSlim rwLock;
        bool forWrite;
        bool locked = false;
        public Locker(ReaderWriterLockSlim rwLock, bool forWrite)
        {
            this.rwLock = rwLock;
            this.forWrite = forWrite;
            if (forWrite)
                rwLock.EnterWriteLock();
            else
                rwLock.EnterReadLock();
            //potrebbe non passare di qui se viene lanciata un'eccezione
            locked = true;
        }
        public void Dispose()
        {
            if (!locked)
                return;

            if (forWrite)
                rwLock.ExitWriteLock();
            else
                rwLock.ExitReadLock();
        }
    }
}
