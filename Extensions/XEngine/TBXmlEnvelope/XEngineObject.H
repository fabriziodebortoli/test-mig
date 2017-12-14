
#pragma once

#include "TXEParameters.h"

#include "beginh.dex"

class CXTender;
class CXEngineSrvObj;
class CInstanceMng;
class XEngineActivation;
class CTBNamespace;

//-----------------------------------------------------------------------------
//		XEngineObject declaration
//-----------------------------------------------------------------------------
class TB_EXPORT XEngineObject : public CObject, public CTBLockable
{
	friend class DXEParameters;

	DECLARE_DYNCREATE(XEngineObject)

private:
	TXEParameters*			m_pXEParameters;

	BOOL					m_bUseOldXTechMode;
	
	DECLARE_LOCKABLE(Array, m_arCachedDocuments);
	int						m_nCachedDocuments;

public:
	XEngineObject	();
	~XEngineObject	();

public:
	const TXEParameters*	GetXEParameters		();
	const BOOL				GetUseUTF8			()			{ return GetXEParameters()->f_EncodTypeUTF8; }
	const CString			GetSiteName			()			{ return GetXEParameters()->f_SiteName; }
	const CString			GetDomainName		()			{ return GetXEParameters()->f_DomainName; }
	const CString			GetSiteCode			()			{ return GetXEParameters()->f_SiteCode; }
	
	BOOL					UseOldXTechMode		()	const	{ return m_bUseOldXTechMode;}
	
	CBaseDocument*			GetCachedDocument	(const CString& strDocNamespace);
	BOOL					AddCachedDocument	(CBaseDocument* pDoc);
	void					CloseLatestDocument	();
	
	virtual CString			GetDynamicInstancePath();

private:
	virtual LPCSTR  GetObjectName() const { return "XEngineObject"; }

			void	UpdateParameters(TXEParameters* pNewParams = NULL);
};

//-----------------------------------------------------------------------------
TB_EXPORT XEngineObject*			AFXAPI AfxGetXEngineObject(BOOL bCreate = TRUE);
TB_EXPORT const TXEParameters*		AFXAPI AfxGetParameters();
TB_EXPORT CString					AFXAPI AfxGetDomainName();
TB_EXPORT CString					AFXAPI AfxGetSiteName();				
TB_EXPORT CString					AFXAPI AfxGetSiteCode();				
TB_EXPORT CString					AFXAPI AfxGetDynamicInstancePath();	

//============================================================================
// Global Windows state data helper functions (inlines)
//-----------------------------------------------------------------------------

// restituisce i parametri di XEngine (sono di singola connessione)
//-----------------------------------------------------------------------------
inline const TXEParameters*	AFXAPI AfxGetParameters()
{
	return AfxGetXEngineObject()->GetXEParameters();
}

//-----------------------------------------------------------------------------
inline CString AFXAPI AfxGetDomainName()
{ 
	return AfxGetXEngineObject()->GetDomainName(); 
}

//-----------------------------------------------------------------------------
inline CString AFXAPI AfxGetSiteName()
{
	return AfxGetXEngineObject()->GetSiteName(); 
}

//-----------------------------------------------------------------------------
inline CString AFXAPI AfxGetSiteCode()
{
	return AfxGetXEngineObject()->GetSiteCode(); 
}

//-----------------------------------------------------------------------------
inline CString AFXAPI AfxGetDynamicInstancePath()
{ 
	return AfxGetXEngineObject()->GetDynamicInstancePath(); 
}

//-----------------------------------------------------------------------------
inline void AFXAPI AfxInitWithXEngineEncoding(CXMLDocumentObject &aDocumentObject)
{ 
	if (AfxGetXEngineObject()->GetUseUTF8())
		aDocumentObject.CreateInitialProcessingInstruction();
	else
		aDocumentObject.CreateInitialProcessingInstruction(_T("xml"), _T("version=\"1.0\" encoding=\"UTF-16\""));
}

#include "endh.dex"
