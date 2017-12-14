
namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// ISignParamsPrepFactory.
	/// </summary>
	//=========================================================================
	public interface ISignParamsPrepFactory
	{
		//---------------------------------------------------------------------
		ISignParamsPreparer GetSignParamsPreparer(int activationVersion);
	}
}
