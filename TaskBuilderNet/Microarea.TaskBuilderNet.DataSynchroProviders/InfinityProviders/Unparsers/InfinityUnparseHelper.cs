using System.Collections.Generic;

using Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Unparsers
{
	///<summary>
	/// Connettore per richiamare la classe di Unparser specifica in base al nome dell'azione
	///</summary>
	//================================================================================
	internal class InfinityUnparseHelper
	{
		//--------------------------------------------------------------------------------
		public static ActionToExport UnparseStringForAction
			(string registeredApp, CRMAction action, SynchroActionType syncActionType, List<DTValuesToImport> dtValuesList = null, ActionDataInfo adi = null)
		{
			InfinityBaseUnparser unparser = new InfinityBaseUnparser();
			if (unparser == null)
				return new ActionToExport();

			// istanzio l'helper per i parametri e faccio la chiamata all'unparser specifico
			InfinityUnparserParams paramsHelper = new InfinityUnparserParams(registeredApp, action, syncActionType, dtValuesList, adi);
			return unparser.GetXml(paramsHelper);
		}
	}
}
