
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.StringLoader
{
	//================================================================================
	public abstract class ObjectLocalizer : ILocalizer 
	{
		private string dictionaryPath	= string.Empty;
		private string objectName		= string.Empty;				
		private string objectType		= string.Empty;	
		private string objectID			= string.Empty;
		
		//--------------------------------------------------------------------------------
		protected void InitObjectIdentifiers(
			string dictionaryPath,
			string objectName,
			string objectType,
			string objectID
			)
		{
			this.dictionaryPath = Helper.GetSpecificDictionaryFilePath(dictionaryPath);
			this.objectName	= objectName;
			this.objectType	= objectType;
			this.objectID	= objectID;
		}

		#region ILocalizer Members
		
		//--------------------------------------------------------------------------------
		public string Translate(string baseString)
		{
			if (dictionaryPath == string.Empty || objectName == string.Empty)
				return baseString;

			DictionaryStringBlock dictionary = StringLoader.GetDictionary(dictionaryPath, objectType, objectID, objectName);
			if (dictionary != null)
				return Helper.FindString(dictionary, baseString);

			return baseString;
		}

        //--------------------------------------------------------------------------------
        public void Build(string filepath, IBasePathFinder pathFinder) 
        { }

		#endregion

	}
	//================================================================================
	public class GlobalConstants
	{
		public const string xml				= "xml";
		public const string other			= "other";
		public const string databaseScript	= "databasescript";
		public const string report			= "report";
	}

	

	
}
