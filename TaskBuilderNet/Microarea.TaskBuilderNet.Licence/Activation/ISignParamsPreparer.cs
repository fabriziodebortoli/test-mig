using System.Xml;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// ISignParamsPreparer.
	/// </summary>
	//=========================================================================
	public interface ISignParamsPreparer
	{
		//---------------------------------------------------------------------
		XmlDocument PrepareParamsForSigning(XmlDocument xmlDoc);

		//---------------------------------------------------------------------
		void PrepareXmlConfigFile(ref string xmlConfigFile);
	}
}
