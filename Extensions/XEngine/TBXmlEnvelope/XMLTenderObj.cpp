
#include "stdafx.h"

#include <comdef.h>

#include <TBGes\XMLLogManager.h>
#include "TXEParameters.h"
#include "XMLTenderObj.h"
#include "XENgineObject.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static TCHAR	szBackSlash[]		= _T("\\");
static TCHAR	szXMLSubpath[]		= _T("XmlData");
static TCHAR	szImportSubpath[]	= _T("Import");
static TCHAR	szExportSubpath[]	= _T("Export");
static TCHAR	szTXSubpath[]		= _T("TX");
static TCHAR	szRXSubpath[]		= _T("RX");
static TCHAR	szPendingSubpath[]	= _T("Pending");
static TCHAR	szFailureSubpath[]	= _T("Failure");
static TCHAR	szSuccessSubpath[]	= _T("Success");

//----------------------------------------------------------------
//	 CXTender implementation
//----------------------------------------------------------------
CXTender::CXTender() 
	:
	m_pIXTender	(NULL),
	m_bCreateFailed	(FALSE)
{
	if (!Initialize())
		m_bCreateFailed = TRUE;
}

//----------------------------------------------------------------------------
CXTender::~CXTender()
{
	Close();
}

//----------------------------------------------------------------------------
void CXTender::Close()
{
	if (m_pIXTender)
        m_pIXTender->Release();

	m_pIXTender = NULL;
}

//----------------------------------------------------------------------------
BOOL CXTender::Initialize()
{
	//è gia fallita una volta la sua istanziazione
	if (m_bCreateFailed)
		return FALSE;

	Close();

	HRESULT hr = ::CoCreateInstance
						(
							CLSID_XMLTender,
							NULL,								
							CLSCTX_INPROC_SERVER,
							IID_IXTender,
							(void **)&m_pIXTender
							);
	if (FAILED(hr))
	{
		TRACE("CXTender::Initialize: XTender creation failed\n");
		if (m_pIXTender)
			m_pIXTender->Release();
		
		m_pIXTender = NULL;	
		return FALSE;
	}

	if (!IsTenderActivated())
	{
		AfxMessageBox (_TB("The XTender component is not enabled."));
		if (m_pIXTender)
			m_pIXTender->Release();

		m_pIXTender = NULL;	
		return FALSE;
	}
	
	return TRUE;
}
	
//-----------------------------------------------------------------------------
BOOL CXTender::InitParameter()
{
	CString strRoot = AfxGetDynamicInstancePath();

	if (
			strRoot.IsEmpty() ||
			!(PutAppRoot(AfxGetDynamicInstancePath()) && SetRepositoyProperty())
		)
	{
		AfxMessageBox (_TB("Unable to initialize the XTender component.\r\nCheck site configuration parameters."));
		return FALSE;
	}


	BOOL bOk = FALSE;

	TXEParameters* pRec = AfxGetParameters();
	if (!pRec)
		AfxMessageBox(_TB("You must set the site parameters"));
	else
		bOk = SetParameter(pRec);
	
	return bOk;
}

//-----------------------------------------------------------------------------
BOOL CXTender::SetRepositoyProperty()  
{
	//per farmi dare il nome del file XSL di default
	CXMLLogSession aLogSession;

	return 	AfxGetDynamicInstancePath() &&
			PutTXSubpath (CString(szXMLSubpath) + szBackSlash + szExportSubpath + szBackSlash + szTXSubpath) &&
			PutPendingSubpath (CString(szXMLSubpath) + szBackSlash + szExportSubpath + szBackSlash + szPendingSubpath) &&
			PutFailureSubpath (CString(szXMLSubpath) + szBackSlash + szExportSubpath + szBackSlash + szFailureSubpath) &&
			PutSuccessSubpath (CString(szXMLSubpath) + szBackSlash + szExportSubpath + szBackSlash + szSuccessSubpath) &&
			PutRXSubpath (CString(szXMLSubpath) + szBackSlash + szImportSubpath + szBackSlash + szRXSubpath) &&
			PutXSLFile(aLogSession.GetXSLFile());
}

//-----------------------------------------------------------------------------
BOOL CXTender::SetParameter(TXEParameters* pParamRec)
{
	return 
			pParamRec &&
			PutDomain(pParamRec->f_DomainName.GetString()) &&	
			PutSite(pParamRec->f_SiteName.GetString()) &&	
			PutUserID (pParamRec->f_UserID.GetString()) &&	
			PutPassword (pParamRec->f_Password.GetString()) &&	
			PutRepositoryServerURL(pParamRec->f_Url.GetString()) &&
			PutMaxTime(pParamRec->f_TimeOut) &&
			PutMaxAttempts(pParamRec->f_TryNumber) &&
			PutEncoding(pParamRec->f_EncodTypeUTF8 ? E_UTF8 : E_UTF16) &&
			PutCompressSize (pParamRec->f_CompressSize);			 			
}

//-----------------------------------------------------------------------------
BOOL CXTender::SendEnvelope(const CString& strEnvName, const CString& strEnvClass, DWORD& dwThread, BOOL bIsPending /*=FALSE*/, const CString& strEnvFileName /* = "" */)
{
	if (!m_pIXTender || strEnvName.IsEmpty() || strEnvClass.IsEmpty()) 
		return FALSE;
	
	BSTR bstrEnvName		 = NULL;
	BSTR bstrEnvClass		 = NULL;
	BSTR bstrEnvFileName	 = NULL;
	
	bstrEnvName		= strEnvName.AllocSysString();
	bstrEnvClass	= strEnvClass.AllocSysString();
	bstrEnvFileName	= strEnvFileName.AllocSysString();
	
	long lThread;
	HRESULT hr ;

	if (bIsPending)
		hr = m_pIXTender->SendPendingEnvelope(bstrEnvName, bstrEnvClass, bstrEnvFileName, &lThread);		
	else
		hr = m_pIXTender->SendEnvelope(bstrEnvName, bstrEnvClass, bstrEnvFileName, &lThread);	

	if (FAILED(hr))
	{
		TRACE("CXTender::SendEnvelope: the posting of the envelope is failed%s\n",GetLastError());

		::SysFreeString(bstrEnvName);
		::SysFreeString(bstrEnvClass);
		::SysFreeString(bstrEnvFileName);
		dwThread = 0;

		return FALSE;
	}
	
	dwThread = lThread;

	::SysFreeString(bstrEnvName);
	::SysFreeString(bstrEnvClass);
	::SysFreeString(bstrEnvFileName);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXTender::SendPendingEnvelopes()
{
	if (!m_pIXTender) 
		return FALSE;
	
	HRESULT hr = m_pIXTender->SendPendingData(FALSE);		
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::SendPendingEnvelopes: the posting of the pending envelope is failed:%s\n",GetLastError());
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXTender::GetAvailableEnvelope(const CString& strEnvClass, const CString& strEnvSite, DWORD& dwThread)
{
	dwThread = 0;

	if (!m_pIXTender) 
		return FALSE;
		
	BSTR bstrEnvClass = NULL;
	BSTR bstrEnvSite = NULL;

	BOOL bThreadStarted;
	long lThread;
	
	bstrEnvClass = strEnvClass.AllocSysString();
	bstrEnvSite = strEnvSite.AllocSysString();

	HRESULT hr = m_pIXTender->GetAvailableEnvelope(bstrEnvClass, bstrEnvSite, &lThread, &bThreadStarted);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetAvailableEnvelope: the receipt of the envelope is failed:%s\n",GetLastError());

		::SysFreeString(bstrEnvClass);
		::SysFreeString(bstrEnvSite);

		return FALSE;
	};

	if (bThreadStarted)
		dwThread = lThread;

	::SysFreeString(bstrEnvClass);
	::SysFreeString(bstrEnvSite);

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXTender::GetAvailableSites(CStringArray* pSites, const CString& strEnvClass /*= ""*/) 
{
	if (!pSites)
	{
		ASSERT(FALSE);
		return FALSE;
	}

	pSites->RemoveAll();

	if (!m_pIXTender) 
		return FALSE;

	VARIANT varSites;
	BSTR bstrEnvClass = NULL;
	
	VariantInit (&varSites);
	bstrEnvClass = strEnvClass.AllocSysString();	

	HRESULT hr = m_pIXTender->GetAvailableSites (bstrEnvClass, &varSites);
	
	if (FAILED(hr))
	{
		TRACE("CXTender::GetAvailableSites: the receipt of the sites list is failed: %s\n",GetLastError());

		::SysFreeString(bstrEnvClass);

		return FALSE;
	};
	
	if (varSites.vt & VT_ARRAY)
	{
		long nLen = varSites.parray->rgsabound->cElements;
		for (long i = 0; i < nLen; i++)
		{
			BSTR bstrS;
			SafeArrayGetElement (varSites.parray, &i, &bstrS);

			pSites->Add(_bstr_t(bstrS));
		}
		VariantClear(&varSites);
	}

	::SysFreeString(bstrEnvClass);

	return TRUE;		

}

//-----------------------------------------------------------------------------
CString CXTender::GetAppRoot()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrAppRoot;
	HRESULT hr = m_pIXTender->get_AppRoot(&bstrAppRoot);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetAppRoot: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrAppRoot);

	::SysFreeString(bstrAppRoot);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutAppRoot(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrAppRoot = NULL;
	bstrAppRoot = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_AppRoot(bstrAppRoot);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutAppRoot: %s\n",GetLastError());

		::SysFreeString(bstrAppRoot);

		return FALSE;
	}

	::SysFreeString(bstrAppRoot);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetRepositoryServerURL()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrRepServerURL;
	HRESULT hr = m_pIXTender->get_RepositoryServerURL(&bstrRepServerURL);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetRepositoryServerURL: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrRepServerURL);

	::SysFreeString(bstrRepServerURL);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutRepositoryServerURL	(const CString& strNewVal)
{
	if (strNewVal.IsEmpty())
		return TRUE;

	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrRepServerURL = NULL;
	bstrRepServerURL = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_RepositoryServerURL(bstrRepServerURL);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutRepositoryServerURL: %s\n",GetLastError());

		::SysFreeString(bstrRepServerURL);

		return FALSE;
	}

	::SysFreeString(bstrRepServerURL);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetDomain ()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_Domain(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetDomain: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutDomain(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = NULL;
	bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_Domain(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutDomain: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetSite()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrSite;
	HRESULT hr = m_pIXTender->get_Site(&bstrSite);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetSite: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrSite);

	::SysFreeString(bstrSite);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutSite(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrSite = NULL;
	bstrSite = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_Site(bstrSite);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("Errore CXTender::PutSite: %s",GetLastError());

		::SysFreeString(bstrSite);

		return FALSE;
	}

	::SysFreeString(bstrSite);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetRXSubpath()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_RxSubPath(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetRXSubpath: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutRXSubpath(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_RxSubPath(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutTXSubpath: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetTXSubpath()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_TxSubPath(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetTXSubpath: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutTXSubpath(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_TxSubPath(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::SetTXSubpath: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetPendingSubpath()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_PendingSubPath(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetPendingSubpath: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutPendingSubpath(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_PendingSubPath(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutPendingSubpath: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetFailureSubpath()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_FailureSubPath(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetFailureSubpath: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutFailureSubpath(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_FailureSubPath(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::SetFailureSubpath: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetSuccessSubpath()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_SuccessSubPath(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetSuccessSubpath: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutSuccessSubpath(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_SuccessSubPath(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutSuccessSubpath: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetUserID()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_UserID(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetUserID: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutUserID(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_UserID(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutUserID: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetPassword()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_Password(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetPassword: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutPassword(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_Password(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutPassword: %s\n",GetLastError());

		::SysFreeString(bstrVal);

		return FALSE;
	}

	::SysFreeString(bstrVal);

	return TRUE;
}
//-----------------------------------------------------------------------------
long CXTender::GetMaxTime()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	long val;
	HRESULT hr = m_pIXTender->get_MaxTime(&val);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetMaxTime: %s\n",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutMaxTime(long nNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (nNewVal > GetSupMaxTime() || nNewVal < GetInfMaxTime())
	{
		AfxMessageBox(_TB("Invalid put_MaxTime parameters"));
		return FALSE;
	}

	HRESULT hr = m_pIXTender->put_MaxTime(nNewVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutMaxTime: %s\n",GetLastError());
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
long CXTender::GetMaxAttempts()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	long val;
	HRESULT hr = m_pIXTender->get_MaxAttempts(&val);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetMaxAttempts: %s\n",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutMaxAttempts(long nNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (nNewVal > GetSupMaxAttempts() || nNewVal < GetInfMaxAttempts())
	{
		AfxMessageBox(_TB("Invalid put_MaxAttempts parameters"));
		return FALSE;
	}	

	HRESULT hr = m_pIXTender->put_MaxAttempts(nNewVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutMaxAttempts: %s\n",GetLastError());
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
long CXTender::GetCompressSize()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	long val=-1;
	HRESULT hr = m_pIXTender->get_CompressSize(&val);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetCompressSize: %s\n",GetLastError());
	}

	return val;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutCompressSize(long nNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	HRESULT hr = m_pIXTender->put_CompressSize(nNewVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutCompressSize: %s\n",GetLastError());
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
EncodingType CXTender::GetEncoding()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return E_UTF8;
	}

	EncodingType eType;
	HRESULT hr = m_pIXTender->get_Encoding(&eType);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetEncoding: %s\n",GetLastError());
		return E_UTF8;
	}

	return eType;
}

//-----------------------------------------------------------------------------
BOOL CXTender::PutEncoding(EncodingType eType)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	HRESULT hr = m_pIXTender->put_Encoding(eType);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutEncoding: %s\n",GetLastError());
		return FALSE;
	}

	return TRUE;
}


//-----------------------------------------------------------------------------
BOOL CXTender::PutXSLFile(const CString& strNewVal)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}

	BSTR bstrVal = NULL;
	bstrVal = strNewVal.AllocSysString();

	HRESULT hr = m_pIXTender->put_XSLFile(bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::PutXSLFile: %s\n",GetLastError());
		::SysFreeString(bstrVal);
		return FALSE;
	}

	::SysFreeString(bstrVal);
	return TRUE;
}

//-----------------------------------------------------------------------------
CString CXTender::GetXSLFile()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrVal;
	HRESULT hr = m_pIXTender->get_XSLFile(&bstrVal);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetXSLFile: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrVal);

	::SysFreeString(bstrVal);

	return ret;

}

//-----------------------------------------------------------------------------
CString CXTender::GetLastError()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrLastError;
	HRESULT hr = m_pIXTender->get_LastError(&bstrLastError);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetLastError");
		return _T("");
	}

	CString ret = CString(bstrLastError);

	::SysFreeString(bstrLastError);

	return ret;
}

//-----------------------------------------------------------------------------
long CXTender::GetSupMaxTime()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	long val;
	HRESULT hr = m_pIXTender->get_SupMaxTime(&val);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetSupMaxTime: %s\n",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
long CXTender::GetInfMaxTime()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	long val;
	HRESULT hr = m_pIXTender->get_InfMaxTime(&val);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetInfMaxTime: %s\n",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
long CXTender::GetInfMaxAttempts()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	long val;
	HRESULT hr = m_pIXTender->get_InfMaxAttempts(&val);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetInfMaxAttempts: %s\n",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
long CXTender::GetSupMaxAttempts()
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	long val;
	HRESULT hr = m_pIXTender->get_SupMaxAttempts(&val);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::GetSupMaxAttempts: %s\n",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
CString CXTender::GetDescription(DWORD dwID)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrDescription;
	HRESULT hr = m_pIXTender->GetDescription(dwID, &bstrDescription);
	if (FAILED(hr))
	{
		TRACE("CXTender::GetDescription: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrDescription);

	::SysFreeString(bstrDescription);

	return ret;	
}

//-----------------------------------------------------------------------------
float CXTender::GetEnvelopeProgress(DWORD dwID)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	float val;
	HRESULT hr = m_pIXTender->GetEnvelopeProgress(dwID, &val);
	if (FAILED(hr))
	{
		TRACE("CXTender::GetSupMaxAttempts: %s\no",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
float CXTender::GetClassProgress(DWORD dwID)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return -1;
	}

	float val;
	HRESULT hr = m_pIXTender->GetClassProgress(dwID, &val);
	if (FAILED(hr))
	{
		TRACE("CXTender::GetClassProgress: %s\n",GetLastError());
		return -1;
	}

	return val;
}

//-----------------------------------------------------------------------------
CString CXTender::GetCurrentEnvelope(DWORD dwID)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrDescription;
	HRESULT hr = m_pIXTender->GetCurrentEnvelope(dwID, &bstrDescription);
	if (FAILED(hr))
	{
		TRACE("CXTender::GetCurrentEnvelope: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrDescription);

	::SysFreeString(bstrDescription);

	return ret;	
}

//-----------------------------------------------------------------------------
CString CXTender::GetCurrentFile(DWORD dwID)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrDescription;
	HRESULT hr = m_pIXTender->GetCurrentFile(dwID, &bstrDescription);
	if (FAILED(hr))
	{
		TRACE("CXTender::GetCurrentFile: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrDescription);

	::SysFreeString(bstrDescription);

	return ret;	
}

//-----------------------------------------------------------------------------
CString CXTender::GetCurrentClass(DWORD dwID)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrDescription;
	HRESULT hr = m_pIXTender->GetCurrentClass(dwID, &bstrDescription);
	if (FAILED(hr))
	{
		TRACE("CXTender::GetCurrentClass: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrDescription);

	::SysFreeString(bstrDescription);

	return ret;	
}

//-----------------------------------------------------------------------------
CString CXTender::GetCurrentSenderSite(DWORD dwID)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return _T("");
	}

	BSTR bstrDescription;
	HRESULT hr = m_pIXTender->GetCurrentSenderSite(dwID, &bstrDescription);
	if (FAILED(hr))
	{
		TRACE("CXTender::GetCurrentSenderSite: %s\n",GetLastError());
		return _T("");
	}

	CString ret = CString(bstrDescription);

	::SysFreeString(bstrDescription);

	return ret;	
}

//-----------------------------------------------------------------------------
BOOL CXTender::Disconnect(long aCookie)
{
	if (!m_pIXTender) 
		return FALSE;

	HRESULT hr = m_pIXTender->Disconnect(aCookie, GetCurrentThreadId());
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::Disconnect: %s\n",GetLastError());
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXTender::Connect(IUnknown* pIUnknown, long* pCookie)
{
	if (!m_pIXTender || !pIUnknown) 
	{
		ASSERT(FALSE);
		return FALSE;
	}


	HRESULT hr = m_pIXTender->Connect(pIUnknown, GetCurrentThreadId(), pCookie);
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		TRACE("CXTender::Connect: %s\n",GetLastError());
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXTender::CanDestroyTender()
{
	if (!m_pIXTender) 
		return TRUE;
	
	BOOL bCanDestroy = FALSE;

	HRESULT hr = m_pIXTender->CanDestroyTender(&bCanDestroy);	
	return FAILED(hr) || bCanDestroy;
}


//-----------------------------------------------------------------------------
BOOL CXTender::IsThreadAlive (long dwThreadID)
{
	if (!m_pIXTender) 
		return FALSE;
	
	BOOL bIsThreadAlive = FALSE;

	HRESULT hr = m_pIXTender->IsThreadAlive(dwThreadID, &bIsThreadAlive);	
	if (FAILED(hr))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	return bIsThreadAlive;
}

//-----------------------------------------------------------------------------
BOOL CXTender::StopThread (long dwThreadID)
{
	if (!m_pIXTender) 
		return FALSE;
	
	HRESULT hr = m_pIXTender->StopThread(dwThreadID);	

	if (FAILED(hr))
	{
		return FALSE;
	}
	return TRUE;
}

//-----------------------------------------------------------------------------
HANDLE CXTender::GetThreadHandle (long dwThreadID)
{
	if (!m_pIXTender) 
		return NULL;
	
	LONGLONG aHandle;

	HRESULT hr = m_pIXTender->GetThreadHandle(dwThreadID, &aHandle);	

	if (FAILED(hr))
	{
		ASSERT(FALSE);
		return NULL;
	}
	return (HANDLE) aHandle;
}

//-----------------------------------------------------------------------------
BOOL CXTender::TestURL (CString& strLastError)
{
	if (!m_pIXTender) 
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	if (FAILED(m_pIXTender->TestURL()))
	{
		strLastError = GetLastError ();
		return FALSE;
	}
	
	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CXTender::IsTenderActivated ()
{
	if (!m_pIXTender) 
		return FALSE;
	
	BOOL bResult;
	if (FAILED(m_pIXTender->IsActivationValid(&bResult)))
	{
		ASSERT(FALSE);
		return FALSE;
	}
	
	return bResult;
}
