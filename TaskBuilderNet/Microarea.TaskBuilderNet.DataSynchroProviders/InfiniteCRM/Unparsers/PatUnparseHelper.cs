using System.Collections.Generic;
using System.Xml.Linq;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers;
using Microarea.TaskBuilderNet.DataSynchroUtilities;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Unparsers
{
    ///<summary>
    /// Connettore per richiamare la classe di Unparser specifica in base al nome dell'azione
    ///</summary>
    //================================================================================
    internal class PatUnparseHelper
    {
        //--------------------------------------------------------------------------------
        internal static string UnparseStringForEntity(CRMEntity entity, SynchroActionType syncActionType, List<DTPatValuesToImport> dtValuesList = null, ActionDataInfo adi = null, string getResponseXml = "")
        {
            PatBaseUnparser unparser = new PatBaseUnparser();
            if (unparser == null)
                return string.Empty;


            // istanzio l'helper per i parametri e faccio la chiamata all'unparser specifico
            PatUnparserParams paramsHelper = new PatUnparserParams(entity, syncActionType, dtValuesList, adi, getResponseXml);
			return unparser.GetXml(paramsHelper);
        }

        ///<summary>
        /// Dato un id ritorna la stringa xml per effettuare la Get di un oggetto
        ///</summary>
        //--------------------------------------------------------------------------------
        internal static string BuildGetXmlForEntityById(string entityName, string id)
        {
            XElement xml = new XElement(InfiniteCRMConsts.Operation,
                            new XElement(InfiniteCRMConsts.Get,
                            new XElement(entityName, new XAttribute(InfiniteCRMConsts.ID.ToLowerInvariant(), id))));
            return xml.ToString(SaveOptions.DisableFormatting);
        }
    }
}
