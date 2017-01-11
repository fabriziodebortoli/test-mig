
namespace Microarea.TaskBuilderNet.Woorm.StringLoader
{
	
	//================================================================================
	public class EnumLocalizer : ObjectLocalizer
	{
		public const string fileID =  "enums";
		
		//--------------------------------------------------------------------------------
		public EnumLocalizer(string enumName, string dictionaryPath)
		{
			InitObjectIdentifiers(dictionaryPath, enumName, GlobalConstants.other, fileID);
		}
	}
}
