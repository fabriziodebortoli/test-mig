using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

using Microarea.Common.NameSolver;

namespace Microarea.Common.Generic
{
	//================================================================================
	/// <summary>
	/// Classe che scrive/legge il file di versione dell'installazione
	/// </summary>
	[Serializable]
	public class InstallationVersion
	{
		private const string buildDateFormat = "yyyyMMdd";
		private const string installationDateFormat = "yyyyMMddHHmmss";
        private string productName = "TbAppManager";
		private int build = 0;
        private string version = Assembly.GetEntryAssembly().GetName().Version.ToString();//Assembly.GetExecutingAssembly().GetName().Version.ToString();    todo rsweb
		private DateTime bDate = DateTime.MinValue;
		private DateTime iDate = DateTime.Now;
        private DateTime cDate = DateTime.Now;
		
		//--------------------------------------------------------------------------------
		public string ProductName { get {return productName;} set {productName = value;} }
		//--------------------------------------------------------------------------------
		public string Version { get {return version;} set {version = value;} }
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public DateTime IDate { get { return iDate; } set { iDate = value; } }
		//--------------------------------------------------------------------------------
		[XmlIgnore]
		public DateTime BDate { get {return bDate;} set { bDate = value;} }
        //--------------------------------------------------------------------------------
        [XmlIgnore]
        public DateTime CDate { get { return cDate; } set { cDate = value; } }
		
		//--------------------------------------------------------------------------------
		//usato per controllare il formato di serializzazione della data 
		public string BuildDate
		{
			get { return BDate.ToString(buildDateFormat, CultureInfo.InvariantCulture); }
			set { BDate = DateTime.ParseExact(value, buildDateFormat, CultureInfo.InvariantCulture); }
		}

		//--------------------------------------------------------------------------------
		public string InstallationDate
		{
			get { return IDate.ToString(installationDateFormat, CultureInfo.InvariantCulture); }
			set { IDate = DateTime.ParseExact(value, installationDateFormat, CultureInfo.InvariantCulture); }
		}
        //--------------------------------------------------------------------------------
        public string CacheDate
        {
            get { return CDate.ToString(installationDateFormat, CultureInfo.InvariantCulture); }
            set { CDate = DateTime.ParseExact(value, installationDateFormat, CultureInfo.InvariantCulture); }
        }
		//--------------------------------------------------------------------------------
		public int Build { get {return build;} set {build = value;} }

		//--------------------------------------------------------------------------------
		public override string ToString ()
		{
			return (bDate == DateTime.MinValue)
			? Version.ToString()
			: string.Format("{0} - Build {1} - {2}", Version, Build, BDate);
		}

		//--------------------------------------------------------------------------------
		public void Save (string file)
		{
			try
			{
				XmlSerializer x = new XmlSerializer(GetType());
				using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write))
					x.Serialize(fs, this);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format(GenericStrings.ErrorSavingFile, file), ex);
			}
		}

		//--------------------------------------------------------------------------------
		public static InstallationVersion LoadFromOrCreate (string file)
		{

			try
			{
                if (!PathFinder.PathFinderInstance.ExistFile(file))//PathFinder.PathFinderInstance.ExistFile(file))
                {
                    InstallationVersion iv = new InstallationVersion();
                    iv.Save(file);
                    return iv;
                }
				XmlSerializer x = new XmlSerializer(typeof(InstallationVersion));
				using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
					return (InstallationVersion)x.Deserialize(fs);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format(GenericStrings.ErrorLoadingFile, file), ex);
			}
		}

		//--------------------------------------------------------------------------------
		public void UpdateCachedDateAndSave()
		{
			CDate = DateTime.Now;
			this.Save(PathFinder.PathFinderInstance.GetInstallationVersionPath());
		}
	}
}
