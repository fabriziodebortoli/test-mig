using System;
using System.Globalization;
using System.Xml;
//
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.Library.Licence;
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Tutti i suoi membri sono public r/w, perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer
	/// </summary>
	public class WizardState : State
	{
		public ProcedureType	ProcedureType;
		public string			Installation	= string.Empty;
		public string			Product			= string.Empty;
		public string			BrandedProduct	= string.Empty;
		public DataSourceState	InstallationDataSource		= new DataSourceState();
		public UserInfo			UserInfo		= new UserInfo();
		public LicensedConfigurationState[]	LicensedConfs	= null;
//		public string			WceFile			= string.Empty;
//		public ImportSourceInfo	ImportedSource	= null;
//		public bool				UseConfiguration= false;
		public bool				ExistCustom		= false;
		public bool				InstallAsDemo	= true;

		//---------------------------------------------------------------------
		public WizardState(ProcedureType procedureType)
		{
			this.ProcedureType = procedureType;
			InstallationDataSource.DataSource = DataSourceType.CompactDisc;
			InstallationDataSource.WebServiceMachineAddress = string.Empty;
		}

		//---------------------------------------------------------------------
		public LicensedConfigurationState GetLicensedConfigurationState()
		{
			return GetLicensedConfigurationState(this.Product);
		}

		//---------------------------------------------------------------------
		public LicensedConfigurationState GetLicensedConfigurationState(string product)
		{
			LicensedConfigurationState productLicensed = null;
			if (LicensedConfs != null)
				foreach (LicensedConfigurationState aLicensed in LicensedConfs)
					if (string.Compare(aLicensed.Product, product, true, CultureInfo.InvariantCulture) == 0)
					{
						productLicensed = aLicensed;
						break;
					}
			return productLicensed;
		}
	}
	
	//=========================================================================
	public enum ProcedureType {NewInstallation, AddProduct, /*Export, Import,*/ None};
}