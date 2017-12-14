using System.Collections.Generic;
using System.Xml.Linq;
using Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM.Parsers;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM
{
    /// <summary>
    /// internal class InfiniteCRMRecoveryManager
    /// </summary>
    //================================================================================
    internal class InfiniteCRMRecoveryManager : RecoveryManager
    {
        //---------------------------------------------------------------------
        public InfiniteCRMRecoveryManager(InfiniteCRMSynchroProvider provider, string loginName, string loginPassword, bool loginWindowsAuthentication, string companyName, string companyConnectionString)
        {
            this.provider = provider;
            this.loginName = loginName;
            this.loginPassword = loginPassword;
            this.loginWindowsAuthentication = loginWindowsAuthentication;
            this.companyName = companyName;
            this.companyConnectionString = companyConnectionString;
        }

        /// <summary>
        /// il metodo specifico per la ricovery se provider e' CrmPat
        /// usa i metodi di CRMPatSynchroProvider
        /// </summary>
        //---------------------------------------------------------------------
        protected override void InboundRecovery()
        {
            foreach (EntityToImport rmData in rmList)
            {
                List<SetDataInfo> sdiList = new List<SetDataInfo>();

                XElement rootFragment = PatSynchroResponseParser.GetRootElement(rmData.XmlToImport);

                string resResponse = string.Empty;

                if (rootFragment != null && PatSynchroResponseParser.ParseResponse(rmData.XmlToImport, out resResponse))
                {

                    List<SetDataInfo> entitytList = null;
                    if (rmData.Name.Equals("Account"))
                        entitytList = ((InfiniteCRMSynchroProvider)provider).GetProspectEntities(rootFragment, this.companyConnectionString);
                    else if (rmData.Name.Equals("Order"))
                        entitytList = ((InfiniteCRMSynchroProvider)provider).GetOrderEntities(rootFragment, this.companyConnectionString);

                    if (entitytList != null && entitytList.Count > 0)
                    {
                        //vado a cercare nella tabella di transcodifica se i dati sono gia stati inseriti in Mago
                        entitytList = TranscodingCheck(entitytList);
                        if (entitytList.Count > 0)
                        {
                            sdiList.AddRange(entitytList);
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                if (sdiList != null && sdiList.Count > 0)
					rmData.Status = ((InfiniteCRMSynchroProvider)provider).SetData(loginName, loginPassword, loginWindowsAuthentication, companyName, companyConnectionString, sdiList);

                // inserimento in transcodifica
                TranscodingManager transcodingMng = new TranscodingManager(provider.ProviderName, provider.CompanyName, provider.LogWriter);
                foreach (SetDataInfo sdi in sdiList)
                    transcodingMng.InsertRow(this.companyConnectionString, sdi.MagoTableName, sdi.MagoID, sdi.EntityName, sdi.PatID, sdi.TBGuid);
            }
        }

        /// <summary>
        /// Controlla la tabella di transcodifica se id e' gia esistente
        /// se lo trovo in transcodifica non vado piu' ad inserirlo in mago
        /// </summary>
        /// <param name="accountList"></param>
        /// <param name="ds"></param>
        /// <returns></returns>
        //---------------------------------------------------------------------
        private List<SetDataInfo> TranscodingCheck(List<SetDataInfo> accountList)
        {
            TranscodingManager transcodingMng = new TranscodingManager(provider.ProviderName, provider.CompanyName, provider.LogWriter);
            List<SetDataInfo> diList = new List<SetDataInfo>();
            foreach (SetDataInfo sdtInfo in accountList)
            {
                string magoId = transcodingMng.GetRecordKey(companyConnectionString, sdtInfo.EntityName, sdtInfo.PatID);
                if (!string.IsNullOrEmpty(magoId))
                {
                    sdtInfo.MagoID = magoId;
                }
                diList.Add(sdtInfo);
            }

            return diList;
        }
    }
}
