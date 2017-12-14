using System;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.Generic;
using System.IO;
using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.Core.SerializableTypes
{
	[Serializable]
	public class LoggedUser
	{
		public string User;
		public string Company;
        public string Key = null;//se null mai impostata se stringa vuota = password blank
        public bool Remember;

		//---------------------------------------------------------------------------
		public LoggedUser()
			: this ("", "")
		{
		}
		//---------------------------------------------------------------------------
		public LoggedUser(string user, string company)
		{
			this.User = user;
			this.Company = company;
		}

		//---------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return
				(string.Compare(((LoggedUser)obj).Company, this.Company, StringComparison.InvariantCultureIgnoreCase) == 0)
			&&
				(string.Compare(((LoggedUser)obj).User, this.User, StringComparison.InvariantCultureIgnoreCase) == 0);
		}
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return User.GetHashCode() + Company.GetHashCode();
		}

        [XmlIgnore]
        //cripta la password che arriva in chiaro
        //---------------------------------------------------------------------------
        public string Password
        {
            get
            {
                return
                    (Key == null) ?
                    null :
                    Crypto.Decrypt(Key, "Hhy58", "gdGTHa784H");
            }
            set
            {
                Key = (value == null) ? 
                    null : 
                    Crypto.Encrypt(value, "Hhy58", "gdGTHa784H");
            }
        }

		//---------------------------------------------------------------------------
		public void SaveForMago()
		{
			ApplicationCache cache = ApplicationCache.Load();
			cache.PutObject<LoggedUser>(this);
			cache.SaveLoggedUserForMagoFromProvisioningConfidurator();
		}

		//---------------------------------------------------------------------------
		public void Save()
		{
			ApplicationCache cache = ApplicationCache.Load();
			cache.PutObject<LoggedUser>(this);
			cache.Save();
		}

		//---------------------------------------------------------------------------
		public static LoggedUser Load()
		{
			ApplicationCache cache = ApplicationCache.Load();
			LoggedUser user = cache.GetObject<LoggedUser>();
			return user == null ? new LoggedUser() : user;
		}
	}

	[Serializable]
	public class RecentForm
	{

		//-----------------------------------------------------------------------------
		public string FormFullPath { get; set; }


		//-----------------------------------------------------------------------------
		public RecentForm()
			: this("")
		{
		}

		//-----------------------------------------------------------------------------
		public RecentForm(string fullPath)
		{
			this.FormFullPath = fullPath;
		}
		//---------------------------------------------------------------------------
		public override string ToString()
		{
			return Path.GetFileNameWithoutExtension(FormFullPath);
		}
		//---------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return string.Compare(((RecentForm)obj).FormFullPath, this.FormFullPath, StringComparison.InvariantCultureIgnoreCase) == 0;
		}
		//---------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return FormFullPath.GetHashCode();
		}

	}

	[Serializable]
	public class ListRecentForms : SynchronizedCollection<RecentForm>
	{
	}

}
