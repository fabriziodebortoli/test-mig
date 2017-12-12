using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace Microarea.EasyAttachment.Core
{
	public class EALockManager
	{
		private string companyDB = string.Empty;
		private string autenticationToken = string.Empty;
		private string userName = string.Empty;

        private LockManager lockMng = new LockManager(BasePathFinder.BasePathFinderInstance.LockManagerUrl);
		
		
		//--------------------------------------------------------------------------------------
		public EALockManager(string company, string autentication, string user)
		{
			companyDB = company;
			autenticationToken = autentication;
			userName = user;
			lockMng.InitLock(company, autentication);	
		}
		
		//--------------------------------------------------------------------------------------
		public bool LockRecord(ILockable lockObject, string address, ref string lockMsg)
		{
			return LockRecord(lockObject.TableName, lockObject.LockKey, address, ref lockMsg);			
		}

		//--------------------------------------------------------------------------------------
		public bool LockRecord(string tableName, string lockKey, string address, ref string lockMsg)
		{
			int tryCount = 0;
			bool bLock = false;
			string lockUser = string.Empty;
			string lockApp = string.Empty;

			while 
				(
					!(bLock = lockMng.LockRecordEx(companyDB, autenticationToken, userName, tableName, lockKey, address, "DMS", out lockUser, out lockApp)) 
					&&
					tryCount < 6
				)
			{
				Wait();
				tryCount++;
			}

			if (!bLock&& !string.IsNullOrEmpty(lockUser)&& !!string.IsNullOrEmpty(lockApp))
				lockMsg = string.Format("Data {0} of {1} table of {2} database has been locked by {3} throught {4}",
									lockKey,
									tableName,
									companyDB,
									lockUser,
									lockApp
								);
			return bLock;
		}

		//--------------------------------------------------------------------------------------
		public bool UnlockRecord(ILockable lockObject, string address)
		{
			return UnlockRecord(lockObject.TableName, lockObject.LockKey, address);
		}

		//--------------------------------------------------------------------------------------
		public bool UnlockRecord(string tableName, string lockKey, string address)
		{
			return lockMng.UnlockRecord(companyDB, autenticationToken, tableName, lockKey, address);
		}

		//--------------------------------------------------------------------------------------
		public void Wait()
		{ 
			int count = 0;
			while (count < 5000) count++;
		}
	}
}
