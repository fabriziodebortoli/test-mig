
#include "stdafx.h" 

#include <TBNameSolver\LoginContext.h>

#include <TBGES\extdoc.h>

#include <TbOleDb\OleDbMng.h>

#include "XEngineObject.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

//==============================================================================
const TCHAR szTbXmlEnvelope[]	= _T("Module.Extensions.TBXmlEnvelope");
const CTBNamespace snsTbXmlEnvelope	= szTbXmlEnvelope;
//==============================================================================

//.RdeProtocol........................................................
const TCHAR szDocumentCache[]			= _T("DocumentCache");
const TCHAR szCachedDocuments[]			= _T("CachedDocuments");

/////////////////////////////////////////////////////////////////////////////
//				implementazione della classe  di XEngineObject
/////////////////////////////////////////////////////////////////////////////
//
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(XEngineObject, CObject)

//-----------------------------------------------------------------------------
XEngineObject::XEngineObject()
	:
	m_pXEParameters		(NULL),
	m_bUseOldXTechMode	(FALSE)
{
	UpdateParameters();

	DataObj* pSetting	= AfxGetSettingValue(snsTbGeneric, szPreferenceSection, _T("UseOldXTechMode"));
	m_bUseOldXTechMode	= pSetting ?  *((DataBool*) pSetting) : FALSE;

	m_nCachedDocuments = *((DataInt*) AfxGetSettingValue(snsTbXmlEnvelope, szDocumentCache, szCachedDocuments, DataInt(10)));
}

//-----------------------------------------------------------------------------
XEngineObject::~XEngineObject()
{
	CloseLatestDocument();

	delete m_pXEParameters;
}

//-----------------------------------------------------------------------------
void XEngineObject::CloseLatestDocument()
{
	TB_OBJECT_LOCK(&m_arCachedDocuments);	
	
	for (int i = m_arCachedDocuments.GetUpperBound(); i >=0; i--)
	{
		BaseDocumentPtr* ppCachedDoc =  (BaseDocumentPtr*)m_arCachedDocuments.GetAt(i);
		if (!ppCachedDoc)
			continue;

		if (!(*ppCachedDoc))
			m_arCachedDocuments.RemoveAt(i);
		else if (!(*ppCachedDoc)->GetXMLDataManager()->GetCachedDocumentBusy())
			{
				(*ppCachedDoc)->m_pExternalControllerInfo = NULL;
				const CBaseDocument *pDoc = (*ppCachedDoc);
				AfxGetTbCmdManager()->CloseDocument(pDoc);
				m_arCachedDocuments.RemoveAt(i);
			}
	}
}

//-----------------------------------------------------------------------------
CBaseDocument* XEngineObject::GetCachedDocument(const CString& strDocNamespace)
{
	TB_OBJECT_LOCK(&m_arCachedDocuments);	
	
	for (int i = m_arCachedDocuments.GetUpperBound(); i >=0; i--)
	{
		BaseDocumentPtr *ppCachedDoc = (BaseDocumentPtr*) m_arCachedDocuments.GetAt(i);
		if (!ppCachedDoc)
			continue;

		if (!(*ppCachedDoc))
		{
			m_arCachedDocuments.RemoveAt(i);
			continue;
		}
		
		if	(
				(*ppCachedDoc)->GetNamespace() == strDocNamespace && 
				!(*ppCachedDoc)->GetXMLDataManager()->GetCachedDocumentBusy()
			)
		{
			(*ppCachedDoc)->GetXMLDataManager()->SetCachedDocumentBusy();
			return (*ppCachedDoc);
		}
	}
	return NULL;
}

//-----------------------------------------------------------------------------
BOOL XEngineObject::AddCachedDocument(CBaseDocument* pDoc)
{	
	TB_OBJECT_LOCK(&m_arCachedDocuments);	

	if (m_arCachedDocuments.GetSize() == m_nCachedDocuments)
		return FALSE;

	m_arCachedDocuments.Add(new BaseDocumentPtr(pDoc));
	pDoc->GetXMLDataManager()->SetCachedDocumentBusy();
	return TRUE;
}

//-----------------------------------------------------------------------------
const TXEParameters* XEngineObject::GetXEParameters()
{
	return m_pXEParameters;
}

//-------------------------------------------------------------------------------
void XEngineObject::UpdateParameters(TXEParameters* pNewParams /*= NULL*/)
{
	SqlSession *pSession = AfxGetDefaultSqlSession();
	if (!pSession) return;

	if (!m_pXEParameters)
	{
		m_pXEParameters = new TXEParameters();
		m_pXEParameters->f_ExportPath = AfxGetPathFinder()->TransformInRemotePath(GetDynamicInstancePath());
		m_pXEParameters->f_ImportPath = AfxGetPathFinder()->TransformInRemotePath(GetDynamicInstancePath());
	}

	if (pNewParams)
	{
		*m_pXEParameters = *pNewParams;
		return;
	}

	SqlSession* pSqlSession = AfxGetDefaultSqlConnection()->GetNewSqlSession();
	if (!pSqlSession)
		return;

	SqlTable aTable(m_pXEParameters, pSqlSession);
	aTable.Open(TRUE);
	aTable.SelectAll();

	TRY
	{
		aTable.Query();
		if (aTable.IsEmpty())
			m_pXEParameters->Init();
		else
		{
			if (m_pXEParameters->f_ImportPath.IsEmpty())
			{
				aTable.SetAutocommit();
				aTable.LockCurrent(FALSE);
				aTable.Edit();
				if (m_pXEParameters->f_ImportPath.IsEmpty())
					m_pXEParameters->f_ImportPath = AfxGetPathFinder()->TransformInRemotePath(GetDynamicInstancePath());
				aTable.Update();
				aTable.UnlockCurrent();
			}
		}
		aTable.Close();

		if (pSqlSession->CanClose())
		{
			pSqlSession->Close();
			delete pSqlSession;
		}
	}
	CATCH(SqlException, e)
	{
		ASSERT(FALSE);
		AfxGetDiagnostic()->Add(cwsprintf(_TB("Error during XEngine Parameters update: {0-%s}"), e->m_strError));
		if (aTable.IsOpen())
			aTable.Close();
		if (pSqlSession->CanClose())
		{
			pSqlSession->Close();
			delete pSqlSession;
		}
		return;
	}
	END_CATCH
}

//-----------------------------------------------------------------------------
CString	XEngineObject::GetDynamicInstancePath()
{
	CString strPath = AfxGetPathFinder()->GetAppXTechDataIOPath(TRUE); 
	if (strPath.Right(1) != SLASH_CHAR) strPath += SLASH_CHAR;
	return strPath;
}

//restituisce l'istanza della classe di XEngineObject
//-----------------------------------------------------------------------------
XEngineObject* AFXAPI AfxGetXEngineObject(BOOL bCreate /*= TRUE*/)
{ 
	return AfxGetLoginContext()->GetObject<XEngineObject>(bCreate);
}          