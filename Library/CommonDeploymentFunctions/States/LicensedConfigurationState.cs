using System;
using System.Xml;
using Microarea.Library.Licence;
using Microarea.Library.XmlPersister;
//
using CommConsts = Microarea.Library.CommonDeploymentFunctions.Strings.Consts;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia
	/// dello stato della configurazione del prodotto.
	/// Tutti i suoi membri sono public r/w perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer.
	/// </summary>
	[Serializable]
	public class LicensedConfigurationState : State
	{
		public string	Product;
		public ActivationObject ActivationObject;
		public bool IsFromSource = false;
		
		//---------------------------------------------------------------------
		public LicensedConfigurationState(){} // DO NOT REMOVE - used by remoting serialization
		
		//---------------------------------------------------------------------
		public LicensedConfigurationState(string product)
		{
			this.Product = product;
		}
	}
}