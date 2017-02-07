using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.Common.TBExplorerControls
{
    //==============================================================================
    public class TBResourcesExplorer
    {

        ////--------------------------------------------------------------------------
        //public TBResourcesExplorer(NameSpace aNameSpace)//TBExplorerType aType, const CTBNamespace& aNameSpace)
        //{
        //    if (!aNameSpace.IsValid())              //costruzione del NameSpace 
        //    {
        //        if (aNameSpace.GetType() == CTBNamespace::NOT_VALID)
        //            m_NameSpace.SetType(CTBNamespace::REPORT);
        //        else
        //            m_NameSpace.SetType(aNameSpace.GetType());

        //        if (aNameSpace.GetApplicationName().IsEmpty())
        //        {
        //            AddOnApplication* pApp = AfxGetBaseApp()->GetMasterAddOnApp();
        //            m_NameSpace.SetApplicationName(pApp->m_strAddOnAppName);
        //        }
        //        else
        //            m_NameSpace.SetApplicationName(aNameSpace.GetApplicationName());

        //        if (aNameSpace.GetObjectName(CTBNamespace::MODULE).IsEmpty())
        //        {
        //            AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
        //            if (pAddOnApp)
        //                pMods = pAddOnApp->m_pAddOnModules;

        //            if (pMods && pMods->GetSize())
        //            {
        //                for (int i = 0; i <= pMods->GetUpperBound(); i++)
        //                {
        //                    //tra i moduli cerca il primo attivo e lo inizializza come primo namespace
        //                    if (!AfxIsActivated(pMods->GetAt(i)->GetApplicationName(), pMods->GetAt(i)->GetModuleName()))
        //                        continue;

        //                    m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMods->GetAt(i)->GetModuleName());
        //                    break;
        //                }
        //            }
        //        }
        //        else
        //            m_NameSpace.SetObjectName(CTBNamespace::MODULE, aNameSpace.GetObjectName(CTBNamespace::MODULE));

        //        CTBExplorerCachePtr cache = GetExplorerCache();

        //        //memorizza l'ultimo modulo selezionato
        //        if (!cache->m_LastUsedNameSpace.IsEmpty())
        //            m_NameSpace.SetObjectName(CTBNamespace::MODULE, cache->m_LastUsedNameSpace.GetModuleName());

        //    }
        //    else
        //        m_NameSpace = aNameSpace;
        //}



        //    {
        //        CString strApp;
        //        AddOnModsArray* pMods = NULL;


        //        m_ExplorerType = aType;
        //        m_bCanLink = FALSE;

        //        //Cancella le informazioni relative all'ultimo modulo selezionato
        //        CTBExplorer::ClearStoredInfo();

        //        if (!aNameSpace.IsValid())              //costruzione del NameSpace 
        //        {
        //            if (aNameSpace.GetType() == CTBNamespace::NOT_VALID)
        //                m_NameSpace.SetType(CTBNamespace::REPORT);
        //            else
        //                m_NameSpace.SetType(aNameSpace.GetType());

        //            if (aNameSpace.GetApplicationName().IsEmpty())
        //            {
        //                AddOnApplication* pApp = AfxGetBaseApp()->GetMasterAddOnApp();
        //                m_NameSpace.SetApplicationName(pApp->m_strAddOnAppName);
        //            }
        //            else
        //                m_NameSpace.SetApplicationName(aNameSpace.GetApplicationName());

        //            if (aNameSpace.GetObjectName(CTBNamespace::MODULE).IsEmpty())
        //            {
        //                AddOnApplication* pAddOnApp = AfxGetAddOnApp(m_NameSpace.GetApplicationName());
        //                if (pAddOnApp)
        //                    pMods = pAddOnApp->m_pAddOnModules;

        //                if (pMods && pMods->GetSize())
        //                {
        //                    for (int i = 0; i <= pMods->GetUpperBound(); i++)
        //                    {
        //                        //tra i moduli cerca il primo attivo e lo inizializza come primo namespace
        //                        if (!AfxIsActivated(pMods->GetAt(i)->GetApplicationName(), pMods->GetAt(i)->GetModuleName()))
        //                            continue;

        //                        m_NameSpace.SetObjectName(CTBNamespace::MODULE, pMods->GetAt(i)->GetModuleName());
        //                        break;
        //                    }
        //                }
        //            }
        //            else
        //                m_NameSpace.SetObjectName(CTBNamespace::MODULE, aNameSpace.GetObjectName(CTBNamespace::MODULE));

        //            CTBExplorerCachePtr cache = GetExplorerCache();

        //            //memorizza l'ultimo modulo selezionato
        //            if (!cache->m_LastUsedNameSpace.IsEmpty())
        //                m_NameSpace.SetObjectName(CTBNamespace::MODULE, cache->m_LastUsedNameSpace.GetModuleName());

        //        }
        //        else
        //            m_NameSpace = aNameSpace;
        //    }
    }
}
