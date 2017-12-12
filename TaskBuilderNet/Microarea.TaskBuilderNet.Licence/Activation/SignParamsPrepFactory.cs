
namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Returns <code>ISignParamsPreparer</code> for activation version 1 or 2.
	/// </summary>
	//=========================================================================
	public class SignParamsPrepFactory : ISignParamsPrepFactory
	{
		//---------------------------------------------------------------------
		public SignParamsPrepFactory()
		{}

		//---------------------------------------------------------------------
		public virtual ISignParamsPreparer GetSignParamsPreparer(int activationVersion)
		{
			return GetSignParamsPreparer(activationVersion, false);
		}

		//---------------------------------------------------------------------
		public virtual ISignParamsPreparer GetSignParamsPreparer(int activationVersion, bool forDemo)
		{
			if (forDemo)
				return new SignParametersPreparerForDemo();

			if (activationVersion < 2)
				return new SignParametersPreparerForVer1();
			else
				return new SignParametersPreparerForVer2();
		}
	}
}
