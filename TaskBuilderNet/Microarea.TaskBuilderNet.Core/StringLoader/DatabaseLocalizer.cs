
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.StringLoader
{
	//================================================================================
	public class DatabaseLocalizer : ObjectLocalizer
	{
		public const string fileID =  "scripts";
		
		//--------------------------------------------------------------------------------
		public DatabaseLocalizer(string tableName)
			: this(tableName, BasePathFinder.BasePathFinderInstance.GetDictionaryFilePathFromTableName(tableName))
		{
		}

		
		//--------------------------------------------------------------------------------
		public DatabaseLocalizer(string tableName, string dictionaryPath)
		{
			InitObjectIdentifiers(dictionaryPath, tableName, GlobalConstants.databaseScript, fileID);
			
		}

		/// <summary>
		/// Restituisce la descrizione localizzata dell'elemento selezionato
		/// </summary>
		//---------------------------------------------------------------------
		public static string GetLocalizedDescription(string itemToTranslate, string tableName)
		{
			if (itemToTranslate == null)
				return string.Empty;

			ILocalizer localizer = new DatabaseLocalizer(tableName);
			return localizer.Translate(itemToTranslate);
		}
	}
}
