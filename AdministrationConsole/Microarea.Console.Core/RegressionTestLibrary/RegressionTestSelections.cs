using System.Collections;

using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Core.RegressionTestLibrary
{
	#region enums
	// gestione errori durante la specifica migrazione dei dati
	// CONTINUE: continua la migrazione
	// STOP_LAST_COMPANY: stoppa l'elaborazione dell'azienda corrente e continuo con le altre
	// STOP_ALL_COMPANIES: stoppa l'intero processo
	public enum TypeRecovery { CONTINUE, STOP_LAST_COMPANY, STOP_ALL_COMPANIES };
	#endregion
	
	# region Classe per la gestione delle selezioni del wizard di migrazione dati
	/// <summary>
	/// Summary description for RegressionTestSelections
	/// </summary>
	//=========================================================================
	public class RegressionTestSelections
	{
		# region Variabili e Properties
		public string RepositoryPath = string.Empty;
		public string ExtraUpdateFilePath = string.Empty;
		public string WinZipPath = string.Empty;
		public Hashtable AreaItems = new Hashtable();
		public bool IsOk = false;
		private ContextInfo contextInfo = null;

		// Property
		//---------------------------------------------------------------------------	
		public ContextInfo ContextInfo { get { return contextInfo ;} }
		# endregion

		# region Costruttore
		/// <summary>
		/// costruttore
		/// </summary>
		//---------------------------------------------------------------------------	
		public RegressionTestSelections(ContextInfo context)
		{
			contextInfo = context;
		}
		# endregion
	}
	# endregion

	//=========================================================================
	public class AreaItem : NRTObject
	{
		public string		Name			= string.Empty;
		public string		Description		= string.Empty;
		public Hashtable	DataSetItems	= new Hashtable();

		public AreaItem(string name, string description)
		{
			Name		= name;
			Description = description;
		}

		public void AddDataSet(string name, string dataSetName, string dataSetDescription, string dataSetPath)
		{
			if (DataSetItems.ContainsKey(name))
				return;
			
			DataSetItems.Add(name, new DataSetItem(dataSetName, dataSetDescription, dataSetPath));
		}
	}

	//=========================================================================
	public class DataSetItem : NRTObject
	{
		public string Name			= string.Empty;
		public string Description	= string.Empty;
		public string Path			= string.Empty;

		public DataSetItem(string name, string description, string path)
		{
			Name		= name;
			Description = description;
			Path		= path;
		}
	}

	//=========================================================================
	public abstract class NRTObject
	{
	}
}
