
#include "stdafx.h"

#include <TbGeneric\globals.h>
#include <TbGeneric\array.h>
#include <TbNameSolver\PathFinder.h>
#include <TbGenlibManaged\GlobalFunctions.h>

#include <TbOledb\sqltable.h>

#include "tabber.h"
#include "HotLink.h"
#include "ExtDoc.h"
#include "eventmng.h"

//includere come ultimo include all'inizio del cpp
#include "begincpp.dex"
//============================================================================

#ifdef _DEBUG
#undef THIS_FILE
static const char  THIS_FILE[] = __FILE__;
#endif

/*
DESCRIZIONE ALGORITMO per la determinazione degli eventi da eseguire durante importazione via XTech

Stuttura del file Actions.xml
<?xml version="1.0" encoding="utf-8"?>
<Actions owner="erp.sales.documents.Invoice">
   <Table name="MA_SaleDoc">
      <Field name="DocNo" afterchange="OnImportDocNoChanged"></Field>
      <Field name="StubBook" afterchange="OnStubBookChanged"></Field>
      <Field name="InvRsn" afterchange="OnInvRsnChanged">
         <ReleatedFields>
            <ReleatedField tablename="MA_SaleDoc" name="ShippingReason"></ReleatedField>
            <ReleatedField tablename="MA_SaleDoc" name="Specificator1Type"></ReleatedField>	 
            <ReleatedField tablename="MA_SaleDoc" name="StoragePhase2Return"></ReleatedField>	
            <ReleatedField tablename="MA_SaleDoc" name="Specificator2Type"></ReleatedField>	
            <ReleatedField tablename="MA_SaleDoc" name="StoragePhase1"></ReleatedField>		   
            <ReleatedField tablename="MA_SaleDoc" name="SpecificatorPhase2Return"></ReleatedField>  
            <ReleatedField tablename="MA_SaleDoc" name="SpecificatorPhase1"></ReleatedField>	    
            <ReleatedField tablename="MA_SaleDoc" name="StoragePhase2"></ReleatedField>		
            <ReleatedField tablename="MA_SaleDoc" name="SpecificatorPhase2"></ReleatedField>
         </ReleatedFields>
      </Field>
   </Table>
</Actions>

dove:
per ogni tabella propria del documento vengono elencate gli eventi da far scattare durante l'importazione.
Per ogni evento vi è l’elenco dei singoli campi modificati dall'evento stesso compresi anche i local field 
ovvero i campi non presenti sul database utilizzati dal documento per effettuare calcoli intermedi.
Durante la procedura di importazione vengono fatti scattare i soli eventi dei campi importati strettamente 
necessari alla valorizzazione dei campi mancanti.
Poichè gli eventi da eseguire durante l'importazione del documento dipendono strettamente dalla semantica del 
dato, è possibile in ogni momento modificare il file actions.xml mediante il tuning di un importazione. 


- Durante l'apertura del documento viene letto il suo Actions.xml ed eventualmente quelli dei suoi client 
document
- Per ogni tabella presente del file vengono considerati le coppie Field-Event e per ogni copia i propri 
releated field
- Per ottimizzare l'algoritmo durante il parsing dei file vengono salvati nell'array m_arReleatedFields di 
CXMLEventMng i releated field letti. Visto che lo stesso campo può essere modificato da più eventi, viene 
salvato una volta sola considerando l'ultimo evento che lo modifica. 
- Attraverso il metodo CXMLEventMng::ComposeEventFields, vengono determinati quali sono gli eventi che devono 
scattare per completare il SqlRecord passato come argomento.
Per ogni campo non presente nel file xml (quindi anche un local field del SqlRecord) viene verificato se ha un 
evento che lo modifica (considerando m_arReleatedFields di CXMLEventMng). In caso affermativo l'evento deve 
essere fatto scattare (m_ToBeFired posto a TRUE). L'evento può appartenere anche ad una tabella differenti 
rispetto a quella del campo, in questo caso il metodo può essere chiamato successivamente oppure mai chiamato 
a seconda sela tabella di appartenenza sia ancora da importare oppure già importata. 
Il metodo ComposeEventFields prepara le informazioni solo la prima volta che viene importata la tabella 
(e nel caso di DBTSlaveBuffered solo sul primo record). Nell'ambito della stessa importazione, per i documenti 
successivi vengono già utilizzate le informazioni in possesso, visto che la struttura del file xml non varia.
- Gli eventi con m_ToBeFire == TRUE vengono eseguiti solo se il suo campo è stato effettivamente importato 
(anche se con valore empty).
*/
/* 
L'algoritmo sopra descritto può portare ad una anomalia in importazione nel caso in cui l'evento necessario alla
modifica del reletedField sia relativo ad un campo non presente nell'envelope questo evento non verrà mai lanciato.
Una possibile soluzione è quella di salvarsi in modalità LIFO tutte le funzioni che modificano il campo. 
Se l'ultima funzione appartiene ad un campo non presente nell'envelope si esamina la precedente.
Il controllo della  funzione campo viene fatto in due modalità differenti a seconda se il suo campo appartenga 
o meno al record che si sta esaminando:
- se appartiene al record viene verificato se il dataobj corrispondente è locked
- se non appartiene al record viene chiedo al dom il nodo relativo alla tabella di appartenenza del campo e da qui 
il nodo del campo stesso (non conviene utilizzare XPath perchè più lento)
*/

/////////////////////////////////////////////////////////////////////////////
// CEventManager


IMPLEMENT_DYNCREATE (CEventManager, CBaseEventManager)


//---------------------------------------------------------------------------
CEventManager::CEventManager()
:
	m_pDocument (NULL)
{
}
//---------------------------------------------------------------------------
///aggiungo l'event manager nuovo; quello vecchio lo metto come 
// membro del nuovo (ereditarieta' per contenimento)
void CEventManager::AttachDocument(CAbstractFormDoc* pDocument)
{
	ASSERT(pDocument);

	m_pDocument		= pDocument;
	AddFunctionPointers();
	m_pParentEventMng = pDocument->m_pEventManager;
	pDocument->m_pEventManager = this;
}

//---------------------------------------------------------------------------
///aggiungo l'event manager nuovo; quello vecchio lo metto come 
// membro del nuovo (ereditarieta' per contenimento)
void CEventManager::AttachDocument(CClientDoc* pDocument)
{
	ASSERT(pDocument);

	m_pDocument	= pDocument;
	AddFunctionPointers();
	m_pParentEventMng = pDocument->m_pEventManager;
	pDocument->m_pEventManager = this;
}

///dato un puntatore CObject, recupera l'eventuale classe di documento
//cui questo e' legato 
//la funzione ha senso se pObj e` di tipo CAbstractFormDoc, CClientDoc, CEventManager
//---------------------------------------------------------------------------
CString CEventManager::GetClassDocument(CObject* pObj)
{
	ASSERT(pObj);
	
	if(!pObj) return _T("");

	if (pObj->IsKindOf (RUNTIME_CLASS(CAbstractFormDoc)))
	{
		USES_CONVERSION;
		return A2T((LPSTR)pObj->GetRuntimeClass()->m_lpszClassName);
	}
	
	if(pObj->IsKindOf (RUNTIME_CLASS(CClientDoc)))
	{
		USES_CONVERSION;
		return A2T((LPSTR)((CClientDoc*)pObj)->m_pServerDocument->GetRuntimeClass()->m_lpszClassName);
	}

	if(pObj->IsKindOf (RUNTIME_CLASS(CEventManager)))
	{
		return GetClassDocument(((CEventManager*)pObj)->m_pDocument);
	}
	
	return _T("");
}



//---------------------------------------------------------------------------
MappedFunction* CEventManager::GetFunctionPointer(const CString& funcName, CObject** pClass, const CString& strClassName /*= ""*/)
{
	MappedFunction* ptr = NULL;

	if(ptr=CBaseEventManager::GetFunctionPointer(funcName, pClass))
		return ptr;

	if((ptr=SearchFunctionInDoc(funcName, pClass)) && IsValidClass(pClass, strClassName))
		return ptr;

	if((ptr=SearchFunctionInDialog(funcName, pClass)) && IsValidClass(pClass, strClassName))
		return ptr;

	return NULL;
}


//---------------------------------------------------------------------------
// cerca la funzione nel documento
MappedFunction* CEventManager::SearchFunctionInDoc(CString funcName, CObject** pClass)
{
	USES_CONVERSION;

	if (!m_pDocument) return NULL;

	MappedFunction* ptr = NULL;

	CString strBaseDoc = m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) ? _T("CAbstractDoc") : _T("CCLientDoc");

	CRuntimeClass* pRuntimeClass = m_pDocument->GetRuntimeClass();
	ASSERT(pRuntimeClass);
	while(!ptr && pRuntimeClass && strBaseDoc.CompareNoCase (A2T((LPSTR)pRuntimeClass->m_lpszClassName)))
	{
		ptr = CBaseEventManager::GetFunctionPointer(A2T((LPSTR)pRuntimeClass->m_lpszClassName), funcName);
		pRuntimeClass = (*pRuntimeClass->m_pfnGetBaseClass)();
	}
	
	if(ptr)
		*pClass = m_pDocument;
	
	return ptr;
}

//---------------------------------------------------------------------------
// cerca la funzione nelle tabdialog (se la trova, attiva la tab dialog corrispondente)
MappedFunction* CEventManager::SearchFunctionInDialog(CString funcName, CObject** pClass)
{
	if (!m_pDocument || !m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc))) return NULL;
	
	MappedFunction* ptr = NULL;

	POSITION pos = ((CAbstractFormDoc*)m_pDocument)->GetFirstViewPosition();
	while (pos != NULL)
	{
		CView* pView = ((CAbstractFormDoc*)m_pDocument)->GetNextView(pos);
		ASSERT_VALID(pView);
		if (pView != NULL && pView->IsKindOf(RUNTIME_CLASS(CAbstractFormView)))
		{
			TabManagers* pTabs = ((CAbstractFormView*) pView)->m_pTabManagers;
			if(!pTabs)
			{
				ASSERT(FALSE);
				continue;
			}
			for (int i=0; i<pTabs->GetSize(); i++)
			{
				CTabManager *pTab = pTabs->GetAt (i);
				if(!pTab)
				{
					ASSERT(FALSE);
					continue;
				}
				DlgInfoArray* pInfoArray = pTab->GetDlgInfoArray();
				if(!pInfoArray)
				{
					ASSERT(FALSE);
					continue;
				}
				for (int j=0; j<pInfoArray->GetSize(); j++)
				{
					DlgInfoItem* pDlgItem = pInfoArray->GetAt(j);
					if(!pDlgItem || !pDlgItem->GetDialogClass())
					{
						ASSERT(FALSE);
						continue;
					}

					USES_CONVERSION;
					ptr = CBaseEventManager::GetFunctionPointer(A2T((LPSTR)pDlgItem->GetDialogClass()->m_lpszClassName), funcName);
					if(ptr)
					{
						int nRet = pTab->TabDialogActivate(pTab->GetDlgCtrlID(), pDlgItem->GetDialogID());
						if(nRet > 0)
						{
							*pClass = pTab->GetActiveDlg();
							return ptr;
						}
					}
				}
			}
		}
	}

	return NULL;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CEventManager::FireAction(const CString& funcName, CString *pstrInputOutput)
{
	int result = FUNCTION_NOT_FOUND, currentResult;

	FOREACH_CLIENT_DOC(CD_MSG_BEFORE)
		DO_CLIENT_ACTION((funcName, pstrInputOutput), CClientDoc::ON_BEFORE_MSG)
	END_FOREACH

	CBaseEventManager::FireAction(funcName, pstrInputOutput);

	FOREACH_CLIENT_DOC(CD_MSG_AFTER)
		DO_CLIENT_ACTION((funcName, pstrInputOutput), CClientDoc::ON_AFTER_MSG)
	END_FOREACH

	return result;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CEventManager::FireAction(const CString& funcName, void* pVoidInputOutput)
{
	int result = FUNCTION_NOT_FOUND, currentResult;

	FOREACH_CLIENT_DOC(CD_MSG_BEFORE)
		DO_CLIENT_ACTION((funcName, pVoidInputOutput), CClientDoc::ON_BEFORE_MSG)
	END_FOREACH

	currentResult = CBaseEventManager::FireAction(funcName, pVoidInputOutput);
	if (currentResult < result) 
		result = currentResult;	

	FOREACH_CLIENT_DOC(CD_MSG_AFTER)
		DO_CLIENT_ACTION((funcName, pVoidInputOutput), CClientDoc::ON_AFTER_MSG)
	END_FOREACH

	return result;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CEventManager::FireAction(const CString& funcName)
{
	int result = FUNCTION_NOT_FOUND, currentResult;

	FOREACH_CLIENT_DOC(CD_MSG_BEFORE)
		DO_CLIENT_ACTION((funcName), CClientDoc::ON_BEFORE_MSG)
	END_FOREACH

	CBaseEventManager::FireAction(funcName);

	FOREACH_CLIENT_DOC(CD_MSG_AFTER)
		DO_CLIENT_ACTION((funcName), CClientDoc::ON_AFTER_MSG)
	END_FOREACH

	return result;
}

//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
int CEventManager::FireAction(const CString& funcName, CFunctionDescription* pRDI)
{
	int result = FUNCTION_NOT_FOUND, currentResult;

	FOREACH_CLIENT_DOC(CD_MSG_BEFORE)
		//DO_CLIENT_ACTION(pClientDoc->m_pEventManager, (funcName, pRDI))
		if(pClientDoc->m_pEventManager)
		{
			pClientDoc->SetMsgState(CClientDoc::ON_BEFORE_MSG);
			currentResult = pClientDoc->m_pEventManager->FireAction (funcName, pRDI);
			if (currentResult == FUNCTION_OK)
				return FUNCTION_OK;
			if (currentResult < result) result = currentResult;
		}
		END_FOREACH

	currentResult = CBaseEventManager::FireAction(funcName, pRDI);
	if (currentResult == FUNCTION_OK)
		return FUNCTION_OK;

	FOREACH_CLIENT_DOC(CD_MSG_AFTER)
		//DO_CLIENT_ACTION(pClientDoc->m_pEventManager, (funcName, pRDI))
		if(pClientDoc->m_pEventManager)
		{
			pClientDoc->SetMsgState(CClientDoc::ON_AFTER_MSG);
			currentResult = pClientDoc->m_pEventManager->FireAction (funcName, pRDI);
			if (currentResult == FUNCTION_OK)
				return FUNCTION_OK;
			if (currentResult < result) result = currentResult;
		}
	END_FOREACH

	return result;
}

//---------------------------------------------------------------------------
///verifica che il documento
BOOL CEventManager::ExistAction(const CString& funcName, MappedFunction** ppMappedFunction /*= NULL*/, CObject** ppObj /*= NULL*/)
{
	if (CBaseEventManager::ExistAction(funcName, ppMappedFunction, ppObj))
		return TRUE;

	CClientDocArray* pClientDocs = NULL;
	if(m_pDocument && m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)))
		pClientDocs = ((CAbstractFormDoc*)m_pDocument)->m_pClientDocs;
	for (int i=0; pClientDocs && i < pClientDocs->GetSize (); i++)
	{
		CClientDoc* pClientDoc = pClientDocs->GetAt(i);
		if (pClientDoc && pClientDoc->m_pEventManager)
			if ( pClientDoc->m_pEventManager->ExistAction(funcName, ppMappedFunction, ppObj))
				return TRUE;
	}
	return FALSE;
}

//---------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CEventManager::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CEventManager\n");
}

void CEventManager::AssertValid() const
{
	CBaseEventManager::AssertValid();
}
#endif //_DEBUG

//===============================================================================

//da sposare nell'h @@BAUZI
#define XML_RELEATED_FIELDS_TAG			_T("ReleatedFields")
#define XML_RELEATED_FIELD_TAG			_T("ReleatedField")
#define XML_TABLE_NAME_ATTRIBUTE		_T("tablename")

CReleatedField* GetReleatedField(CObArray* pReleatedFields, const CString& strTableName, const CString& strFieldName)
{
	CReleatedField* pReleatedField = NULL;
	for (int i = 0; i < pReleatedFields->GetSize(); i++)
	{
		pReleatedField = (CReleatedField*)pReleatedFields->GetAt(i);
		if (
				pReleatedField &&
				!pReleatedField->m_strTableName.CompareNoCase(strTableName) &&
				!pReleatedField->m_strFieldName.CompareNoCase(strFieldName)
			)
			return pReleatedField;
	}

	return NULL;
}

//=============================================================================							
//SEZIONE SPECIFICA DELL'IMPORTAZIONE DI DOCUMENTI XML
//=============================================================================							

/////////////////////////////////////////////////////////////////////////////
//				XMLMappedFunction Declaration
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(XMLMappedFunction, MappedFunction);

/////////////////////////////////////////////////////////////////////////////
//				CFieldFunction Declaration
/////////////////////////////////////////////////////////////////////////////
//

//---------------------------------------------------------------------------
 CFieldFunction::~CFieldFunction()
 {
	 m_arReleatedFields.RemoveAll();
 }

//---------------------------------------------------------------------------
void CFieldFunction::AddReleatedField(CObArray* pGlobalReleatedFields, const CString& strTableName, const CString& strFieldName)
{
	CReleatedField* pGlobalRelField = GetReleatedField(pGlobalReleatedFields, strTableName, strFieldName);
	if (!pGlobalRelField)
	{
		pGlobalRelField = new CReleatedField(strTableName, strFieldName);
		pGlobalReleatedFields->Add(pGlobalRelField); //lo inserisco sia nell'array dell'event manager (optimiziation)
		m_arReleatedFields.Add(pGlobalRelField);
	}
	else
	{
		//se esiste già nell'array dell'event manager verifico solo la sua esistenza nell'array della funzione poichè lo stesso campo può
		//essere modificato da più funzione
		if (!GetReleatedField(&m_arReleatedFields, strTableName, strFieldName))
			m_arReleatedFields.Add(pGlobalRelField);	

	}
		
	//in ogni caso setto questa funzione come l'ultima che modifica il campo
	pGlobalRelField->AddFunction(this);
	//pGlobalRelField->SetFunction(this); 
}

//---------------------------------------------------------------------------
BOOL CFieldFunction::XMLParse(CXMLNode* pFieldNode, CObArray* pGlobalReleatedFields, CClientDoc* pClientDoc)
{
	CXMLNode* pChangingNode = pFieldNode->GetChildByName(XML_RELEATED_FIELDS_TAG);
	if (pChangingNode)
	{
		for (int nChild = 0; nChild < pChangingNode->GetChildsNum(); nChild++)
		{
			CXMLNode* pFieldNode = pChangingNode->GetChildAt(nChild);
			if ( pFieldNode)
			{
				CString strFieldName;
				CString strTableName;				
				pFieldNode->GetAttribute(XML_TABLE_NAME_ATTRIBUTE, strTableName);	
				pFieldNode->GetAttribute(XML_NAME_ATTRIBUTE, strFieldName);	
				
				if (strFieldName.IsEmpty() || strTableName.IsEmpty())
					continue;
				
				AddReleatedField(pGlobalReleatedFields, strTableName, strFieldName);	
				
			}		
		}
	}

	return TRUE;
}

//---------------------------------------------------------------------------
void CFieldFunction::XMLUnparse(CXMLNode* pFieldNode, CClientDoc* pClientDoc)
{
	if (
			!pFieldNode || 
			m_strFunction.IsEmpty() || 
			(pClientDoc && pClientDoc != m_pClientDoc)	||
			(!pClientDoc && m_pClientDoc)
		)
		return;
	
	CReleatedField* pReleatedField = NULL;
	CXMLNode* pReleatedFieldNode = NULL;
	
	pFieldNode->SetAttribute(XML_AFTER_CHANGE_ATTRIBUTE, m_strFunction);
	if (m_arReleatedFields.GetSize() <= 0)
		return;

	CXMLNode* pChangingNode = pFieldNode->CreateNewChild(XML_RELEATED_FIELDS_TAG);	
	if (pChangingNode)
	{
		for (int i = 0; i < m_arReleatedFields.GetSize(); i++)
		{
			pReleatedField = (CReleatedField*)m_arReleatedFields.GetAt(i);

			if (pReleatedField && !pReleatedField->m_strFieldName.IsEmpty())
			{
				pReleatedFieldNode = pChangingNode->CreateNewChild(XML_RELEATED_FIELD_TAG);
				pReleatedFieldNode->SetAttribute(XML_TABLE_NAME_ATTRIBUTE, pReleatedField->m_strTableName);	
				pReleatedFieldNode->SetAttribute(XML_NAME_ATTRIBUTE, pReleatedField->m_strFieldName);				
			}		
		}
	}	
}

/////////////////////////////////////////////////////////////////////////////
//				CFieldEvent Declaration
/////////////////////////////////////////////////////////////////////////////
//
//---------------------------------------------------------------------------
BOOL CFieldEvent::XMLParse(CXMLNode* pFieldNode, CObArray* pGlobalReleatedFields, CClientDoc* pClientDoc)
{
	CString strFunction;

	if (!pFieldNode || !pFieldNode->GetAttribute (XML_AFTER_CHANGE_ATTRIBUTE, strFunction))
		return FALSE;

	CFieldFunction* pFunction = new CFieldFunction(strFunction, this, pClientDoc);
	if (pFunction->XMLParse(pFieldNode, pGlobalReleatedFields, pClientDoc))
		m_arFunctions.Add(pFunction);
	else
		delete(pFunction);
	
	return TRUE;
}

//---------------------------------------------------------------------------
void CFieldEvent::XMLUnparse(CXMLNode* pNode, CClientDoc* pClientDoc)
{
	if (!pNode || m_strFieldName.IsEmpty())
		return;
	
	CXMLNode* pFieldNode = pNode->CreateNewChild(XML_FIELD_TAG);
	pFieldNode->SetAttribute(XML_NAME_ATTRIBUTE, m_strFieldName);

	CFieldFunction* pFieldFunction = NULL;
	for (int i = 0; i < m_arFunctions.GetSize(); i++)
	{
		pFieldFunction = (CFieldFunction*)m_arFunctions.GetAt(i);
		if (pFieldFunction && !pFieldFunction->m_strFunction.IsEmpty())
			pFieldFunction->XMLUnparse(pFieldNode, pClientDoc);
	}
	if (!pFieldNode->HasChildNodes())
		pNode->RemoveChild(pFieldNode);	
}


/////////////////////////////////////////////////////////////////////////////
// CTableEvents
//-----------------------------------------------------------------------------

//---------------------------------------------------------------------------
void CTableEvents::Clear()
{
	m_arFieldEvents.RemoveAll();
	ClearImportingFields();
}

//---------------------------------------------------------------------------
void CTableEvents::ClearImportingFields()
{
	m_bImportingFieldsLoaded = FALSE;
	m_arImportingFields.RemoveAll();
}

//---------------------------------------------------------------------------
BOOL CTableEvents::IsAnImportingField(const CString& strFieldName)
{
	CString strImportingField;
	if (m_arImportingFields.GetSize() > 0)
	{
		for (int i = 0; i < m_arImportingFields.GetSize(); i++)
		{
			strImportingField = m_arImportingFields.GetAt(i);
			if (!strImportingField.CompareNoCase(strFieldName))
				return TRUE;
		}		
	}
	return FALSE;
}

//---------------------------------------------------------------------------
CFieldEvent* CTableEvents::GetFieldEvents(const CString& strFieldName)
{
	CFieldEvent* pFieldEvents = NULL;
	for (int i = 0; i < m_arFieldEvents.GetSize(); i++)
	{
		pFieldEvents = (CFieldEvent*)m_arFieldEvents.GetAt(i);
		if (!pFieldEvents->m_strFieldName.CompareNoCase(strFieldName))
			return pFieldEvents;
	}
	return NULL;
}

//---------------------------------------------------------------------------
BOOL CTableEvents::XMLParse(CXMLNode* pTableNode, CObArray* pGlobalReleatedFields, CClientDoc* pClientDoc)
{
	CString strFieldName;
	for (int i = 0; i < pTableNode->GetChildsNum(); i++)
	{
		CXMLNode* pFieldNode = pTableNode->GetChildAt(i);
		if (!pFieldNode || !pFieldNode->GetAttribute(XML_NAME_ATTRIBUTE, strFieldName)) 
			continue;
				
		CFieldEvent* pFieldEvents = GetFieldEvents(strFieldName);
		if (!pFieldEvents)
		{
			pFieldEvents = new CFieldEvent(strFieldName, m_strTableName, this);
			if (pFieldEvents->XMLParse(pFieldNode, pGlobalReleatedFields, pClientDoc))
				m_arFieldEvents.Add(pFieldEvents);
		}
		else
			pFieldEvents->XMLParse(pFieldNode, pGlobalReleatedFields, pClientDoc);
	}

	return TRUE;		

}

//---------------------------------------------------------------------------
void CTableEvents::XMLUnparse(CXMLNode* pNode, CClientDoc* pClientDoc /*=NULL*/)
{
	CFieldEvent* pFieldEvents = NULL;
	CXMLNode* pTableNode = pNode->CreateNewChild(XML_TABLE_TAG); 
	pTableNode->SetAttribute(XML_NAME_ATTRIBUTE, m_strTableName);

	for (int i = 0; i <  m_arFieldEvents.GetSize(); i++)
	{
		pFieldEvents = (CFieldEvent*)m_arFieldEvents.GetAt(i);
		if (pFieldEvents && !pFieldEvents->m_strFieldName.IsEmpty())
			pFieldEvents->XMLUnparse(pTableNode, pClientDoc);
	}
	if (!pTableNode->HasChildNodes())
		pNode->RemoveChild(pTableNode);
}

//---------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CTableEvents::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CTableEvents\n");
}

void CTableEvents::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG


/////////////////////////////////////////////////////////////////////////////
// CXMLEventManager


IMPLEMENT_DYNCREATE (CXMLEventManager, CEventManager)

//---------------------------------------------------------------------------
CXMLEventManager::CXMLEventManager() 
	: 
	m_pEventsToFire (NULL)
	{}

//---------------------------------------------------------------------------
CXMLEventManager::~CXMLEventManager()
{	
}

///carica la lista di nodi associati agli eventi
//---------------------------------------------------------------------------
CXMLDocumentObject* CXMLEventManager::OpenXMLActionsDocument(BOOL bIsSaving, CClientDoc* pClientDoc /*=NULL*/)
{
	if(!m_pDocument)
	{
		ASSERT(FALSE);
		return NULL;
	}

	CXMLDocumentObject* pXMLDoc = new CXMLDocumentObject(bIsSaving, bIsSaving); 

	CTBNamespace aNamespace;
	if (pClientDoc)
		aNamespace = pClientDoc->m_Namespace;
	else
		aNamespace = ((CAbstractFormDoc*) m_pDocument)->GetNamespace();
	
	CString strActionsFileName = AfxGetPathFinder()->GetDocumentActionsFullName(aNamespace, CPathFinder::STANDARD);
	
	//anche in fase di salvataggio, se non esiste il file standard non faccio niente
	if (!ExistFile(strActionsFileName))
	{
		delete pXMLDoc;
		return NULL;
	}

	// save does not load the file
	if (bIsSaving)
	{
		//in user configuration the modified file should be saved in allusers custom folder
		if (!AfxGetBaseApp()->IsDevelopment())
			strActionsFileName = AfxGetPathFinder()->GetDocumentActionsFullName(aNamespace, CPathFinder::CUSTOM);
		pXMLDoc->SetFileName (strActionsFileName);
		return pXMLDoc;
	}
	

	if (!pXMLDoc->LoadXMLFile(strActionsFileName))
	{
		ASSERT(FALSE);
		TRACE("CXMLEventManager::LoadEventsFromXML: it's impossible to load the file %s\n",strActionsFileName); 
		delete pXMLDoc;
		return NULL;
	}
	
	return pXMLDoc;
}

///carica gli eventi da gestire da actions.xml
//---------------------------------------------------------------------------
BOOL CXMLEventManager::LoadFileActionsEvents(CClientDoc* pClientDoc)
{
	if (!m_pDocument || !m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) ) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLDocumentObject* pXMLDoc = OpenXMLActionsDocument(FALSE, pClientDoc);

	if (!pXMLDoc)
		return TRUE;

	CXMLNodeChildsList* pTableList = pXMLDoc->GetNodeListByTagName(XML_TABLE_TAG); 
	
	if(!pTableList) 
		return FALSE;

	for (int i=0; i < pTableList->GetSize(); i++)
	{
		CXMLNode *pTableNode = pTableList->GetAt(i);
		CString strTableName;
		if (pTableNode && pTableNode->GetAttribute (XML_NAME_ATTRIBUTE, strTableName))
		{
			CTableEvents* pAfterChange = GetEvents(strTableName, AFTER_CHANGE);
			if (pAfterChange == NULL)
			{
				pAfterChange = new CTableEvents(strTableName);
				if (!pAfterChange->XMLParse(pTableNode, &m_arReleatedFields, pClientDoc))
					delete pAfterChange;				
				else
					AddEvent(pAfterChange, AFTER_CHANGE);
			}
			else
				pAfterChange->XMLParse(pTableNode, &m_arReleatedFields, pClientDoc);
		}
	}	
	
	SAFE_DELETE(pTableList)
	SAFE_DELETE(pXMLDoc);

	return m_AfterChangeFieldEvents.GetSize() > 0;
}

///salva gli eventi gestiti da actions.xml
//---------------------------------------------------------------------------
BOOL CXMLEventManager::SaveFileActionsEvents(CClientDoc* pClientDoc /*= NULL*/, Array* pChanges)
{
	if (!m_pDocument || !m_pDocument->IsKindOf(RUNTIME_CLASS(CAbstractFormDoc)) ) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	CXMLDocumentObject* pXMLDoc = OpenXMLActionsDocument(TRUE, pClientDoc);

	if (!pXMLDoc)
		return TRUE;

	CXMLNode* pRoot = pXMLDoc->CreateRoot(XML_ACTIONS_TAG);

	CTBNamespace aNamespace;
	CString	strTitle;
	if (pClientDoc)
	{
		aNamespace = pClientDoc->m_Namespace;
		strTitle = pClientDoc->m_Namespace.GetObjectNameForTag();
	}
	else
	{
		aNamespace = ((CAbstractFormDoc*) m_pDocument)->GetNamespace();
		strTitle =  ((CAbstractFormDoc*) m_pDocument)->GetTitle();
	}

	pRoot->SetAttribute (XML_OWNER_ATTRIBUTE, aNamespace.ToUnparsedString());
	
	CTableEvents* pTableEvents = NULL;

	CChangedTables* pChangedTables = new CChangedTables(pXMLDoc->GetFileName(), strTitle);
	for (int i = 0; i < m_AfterChangeFieldEvents.GetSize(); i++)
	{
		pTableEvents = (CTableEvents*)m_AfterChangeFieldEvents.GetAt(i);
		if (pTableEvents->m_bModified)
		{
			pChangedTables->m_arChangedTables.Add(pTableEvents->m_strTableName);
			pTableEvents->m_bModified = FALSE;
		}
		if (pTableEvents)
			pTableEvents->XMLUnparse(pRoot, pClientDoc);
	}
	
	if (pChanges && pChangedTables->m_arChangedTables.GetSize() > 0)
		pChanges->Add(pChangedTables);
	else
		delete pChangedTables;
	
	pXMLDoc->SaveXMLFile(pXMLDoc->GetFileName(), TRUE);
	delete pXMLDoc;
	return TRUE;
}
//---------------------------------------------------------------------------
BOOL CXMLEventManager::LoadEvents()
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)m_pDocument; 	

	//Carico gli eventi propri del documento
	BOOL bOK = LoadFileActionsEvents();

	//carico gli eventi propri dei client docs
	for (int j=0; j < pDoc->m_pClientDocs->GetSize(); j++)
	{
		CXMLEventManager *pEvMng = (CXMLEventManager*)pDoc->m_pClientDocs->GetAt(j)->m_pEventManager;
		if(pEvMng && pEvMng->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
			bOK = LoadFileActionsEvents(pDoc->m_pClientDocs->GetAt(j)) && bOK;
	}
	
	return bOK;
}

//---------------------------------------------------------------------------
BOOL CXMLEventManager::SaveEvents(Array* pChanges)
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)m_pDocument; 	
	
	CTableEvents* pTableEvents =  NULL;	
	BOOL bModified = FALSE;
	for (int i = 0; i < m_AfterChangeFieldEvents.GetSize(); i++)
	{
		pTableEvents = (CTableEvents*)m_AfterChangeFieldEvents.GetAt(i);
		if (pTableEvents->m_bModified)
		{
			bModified = TRUE;
			break;
		}
			
	}

	if (bModified)
	{
		//Carico gli eventi propri del documento
		BOOL bOK = SaveFileActionsEvents(NULL, pChanges);

		//carico gli eventi propri dei client docs
		for (int j=0; j < pDoc->m_pClientDocs->GetSize(); j++)
		{
			CXMLEventManager *pEvMng = (CXMLEventManager*)pDoc->m_pClientDocs->GetAt(j)->m_pEventManager;
			if(pEvMng && pEvMng->IsKindOf(RUNTIME_CLASS(CXMLEventManager)))
				bOK = SaveFileActionsEvents(pDoc->m_pClientDocs->GetAt(j), pChanges) && bOK;
		}
		
		return bOK;
	}
	return TRUE;
}

//---------------------------------------------------------------------------
void CXMLEventManager::InitTablesEvents()
{
	for (int i = 0; i < m_AfterChangeFieldEvents.GetSize(); i++)
		InitSingleTableEvents((CTableEvents*)m_AfterChangeFieldEvents.GetAt(i));
}

//---------------------------------------------------------------------------
void CXMLEventManager::InitSingleTableEvents(CTableEvents* pTableEvents)
{
	if (!pTableEvents)
		return;

	CFieldEvent* pFieldEvent = NULL;
	CFieldFunction* pFieldFunction = NULL;

	pTableEvents->m_bPrepared = FALSE;		
	pTableEvents->ClearImportingFields();
	for (int k = 0; k <= pTableEvents->m_arFieldEvents.GetUpperBound(); k++)
	{
		pFieldEvent = (CFieldEvent*)pTableEvents->m_arFieldEvents.GetAt(k);
		for (int j = 0; j < pFieldEvent->m_arFunctions.GetSize(); j++)
		{
			pFieldFunction = (CFieldFunction*)pFieldEvent->m_arFunctions.GetAt(j);
			if (pFieldFunction)
				pFieldFunction->m_ToBeFired = FALSE; 
		}
	}	
}


//---------------------------------------------------------------------------
BOOL CXMLEventManager::AddEvent(CTableEvents* pEvents, EventListType type)
{
	if (!pEvents) return FALSE;

	switch(type)
	{
	case AFTER_CHANGE:
		m_AfterChangeFieldEvents.Add(pEvents);
		break;
	default:
		return FALSE;
	}

	return TRUE;
}

//---------------------------------------------------------------------------
void CXMLEventManager::OnAfterChangeField(SqlRecord *pRecord, CTableEvents *pEvents)
{
	ASSERT(pRecord);
	CFieldEvent* pFieldEvents = NULL;
	CFieldFunction* pFieldFunction = NULL;
	CStringArray arFiredFunctions;
	BOOL bFired = FALSE;

	if(!pRecord->GetTableName().CompareNoCase (pEvents->m_strTableName))
	{
		for (int i = 0; i < pEvents->m_arFieldEvents.GetSize(); i++)
		{
			pFieldEvents = (CFieldEvent*)pEvents->m_arFieldEvents.GetAt(i);
			if (!pFieldEvents && pFieldEvents->m_strFieldName.IsEmpty())
				continue;
			arFiredFunctions.RemoveAll();
			bFired = FALSE;
			DataObj *pDataObj = pRecord->GetDataObjFromColumnName(pFieldEvents->m_strFieldName);
			if(pDataObj && pDataObj->IsValueLocked())
			{
				for (int j = 0; j < pFieldEvents->m_arFunctions.GetSize(); j++)
				{
					pFieldFunction = (CFieldFunction*)pFieldEvents->m_arFunctions.GetAt(j);
					if (pFieldFunction)
					{
						if (arFiredFunctions.GetCount() > 0)
						{
							for (int k = 0; k < arFiredFunctions.GetCount(); k++)
							{
								if (arFiredFunctions.GetAt(k).CompareNoCase(pFieldFunction->m_strFunction) == 0)
								{
									bFired = TRUE;
									break;
								}
							}
						}
						if (!bFired)
						{
							FireAction(pFieldFunction->m_strFunction, pDataObj);
							arFiredFunctions.Add(pFieldFunction->m_strFunction);
						}
					}
				}
			}
		}
	}
}

/// events of a specified table name
//---------------------------------------------------------------------------
CTableEvents* CXMLEventManager::GetEvents(const CString& tableName, EventListType type)
{
	Array* pAllEvents = NULL;
	switch(type)
	{
	case AFTER_CHANGE:
		pAllEvents = &m_AfterChangeFieldEvents;
		break;
	}

	if (!pAllEvents) 
		return NULL;

	for (int i=0; i < pAllEvents->GetSize(); i++)
	{
		CTableEvents* pEvent = (CTableEvents*)pAllEvents->GetAt(i);
		if (pEvent && !pEvent->m_strTableName.CompareNoCase(tableName))
			return pEvent;
	}

	return NULL;
}



//---------------------------------------------------------------------------
///esegue una funzione a partire dal suo nome in formato stringa
// ed effettua la find sull'HotLink associato all'evento
int CXMLEventManager::FireAction(const CString& funcName, DataObj* pDataObj)
{
	ASSERT(pDataObj);

	CObject* pObj = NULL;
	MappedFunction* pMappedFunc = GetFunctionPointer (funcName, &pObj);
	if(pMappedFunc && pMappedFunc->IsKindOf (RUNTIME_CLASS(XMLMappedFunction)))
	{
		HotKeyLink **ppHKL = ((XMLMappedFunction*)pMappedFunc)->m_ppHKL;
		if(ppHKL && *ppHKL && AfxIsValidAddress(*ppHKL, sizeof(HotKeyLink)))
		{
			(*ppHKL)->FindRecord(pDataObj);
		}
	}

	return CEventManager::FireAction (funcName);
}


//---------------------------------------------------------------------------
void CXMLEventManager::AddReleatedField(CFieldFunction* pFieldFunction, const CString& strTableName, const CString& strFieldName)
{
	if (!pFieldFunction)
		return;

	pFieldFunction->AddReleatedField(&m_arReleatedFields, strTableName, strFieldName);
}

//---------------------------------------------------------------------------
CFieldEvent* CXMLEventManager::GetFieldEvents(const CString& tableName, const CString& fieldName)
{
	CTableEvents* pEvents = NULL;
	CFieldEvent* pFieldEvent = NULL;

	for (int i = 0; i < m_AfterChangeFieldEvents.GetSize(); i++)
	{
		pEvents = (CTableEvents*) m_AfterChangeFieldEvents.GetAt(i);
		
		if (!pEvents || tableName.CompareNoCase(pEvents->m_strTableName))
			continue;
		
		return pEvents->GetFieldEvents(fieldName);
	}

	return NULL;
}

//---------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// Diagnostics

#ifdef _DEBUG
void CXMLEventManager::Dump(CDumpContext& dc) const
{
	ASSERT_VALID(this);
	AFX_DUMP0(dc, "CXMLEventManager\n");
}

void CXMLEventManager::AssertValid() const
{
	CObject::AssertValid();
}
#endif //_DEBUG

//===============================================================================




