using System;
using System.Threading;
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia
	/// dello stato dell'installazione.
	/// Tutti i suoi membri sono public r/w perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer.
	/// </summary>
	[Serializable]
	public class LanguageSettings : State
	{
		public string	Language;
		public string	LocalCulture;

		//---------------------------------------------------------------------
		public LanguageSettings()
		{
			this.Language		= Thread.CurrentThread.CurrentUICulture.Name;
			this.LocalCulture	= Thread.CurrentThread.CurrentCulture.Name;
		}
	}
}
