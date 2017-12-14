using System;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Contiene l'informazione NomeProduttore ChiaveDiAttivazione UserId
	/// </summary>
	//=========================================================================
	[Serializable]
	public struct ActivationKeyInfo
	{
		public string Producer;
		public string InternalCode;
		public string Key;

		//---------------------------------------------------------------------
		public ActivationKeyInfo(
			string producer,
			string internalCode,
			string key
			)
		{
			Producer	 = producer;
			InternalCode = internalCode;
			Key			 = key;
		}
	}
}
