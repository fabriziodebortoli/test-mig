
namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Summary description for SignParamsPrepFactoryForVer2_Prot4.
	/// </summary>
	//=========================================================================
	public class SignParamsPrepFactoryForVer2_Prot4 : SignParamsPrepFactory
	{
		private int protocolVersion;

		//---------------------------------------------------------------------
		public SignParamsPrepFactoryForVer2_Prot4(int protocolVersion)
		{
			this.protocolVersion = protocolVersion;
		}

		//---------------------------------------------------------------------
		public override ISignParamsPreparer GetSignParamsPreparer(int activationVersion)
		{
			return GetSignParamsPreparer(activationVersion, false);
		}

		//---------------------------------------------------------------------
		public override ISignParamsPreparer GetSignParamsPreparer(int activationVersion, bool forDemo)
		{
			if (forDemo)
				return new SignParametersPreparerForDemo();

			if (activationVersion < 2)
				return new SignParametersPreparerForVer1();
			else
			{
				if (protocolVersion < 4)
					return new SignParametersPreparerForVer2();
				else
					return new SignParametersPreparerForVer2_Prot4();
			}
		}
	}
}
