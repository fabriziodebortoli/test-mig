
#include "stdafx.h"

#include <afxpriv.h>
#include <mapi.h>
#include <locale.h>

#include <TbNameSolver\chars.h>
#include <TbNameSolver\applicationcontext.h>
#include "globals.h"
#include "EnumsConst.h"

#include "Array.h"
#include "Dataobj.h"
#include "CMapi.h"
#include "ParametersSections.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char THIS_FILE[] = __FILE__;
#endif

///////////////////////////////////////////////////////////////////////////////

void CStringArray_ConcatAttachmentsWithTitle (const CStringArray& ar, const CStringArray& arTitle, CString& str)
{
	CString strT, strF;

	for (int i = 0; i <= ar.GetUpperBound(); i++)
	{
		strF = ar.GetAt(i);

		if (i < arTitle.GetSize())
			strT = arTitle.GetAt(i);
		else
		{
			int nPos = strF.ReverseFind(SLASH_CHAR);
			if (nPos < 0)
				strT = strF;
			else
				strT = strF.Right(strF.GetLength() - nPos);
		}

		str += strF + _T(" <") + strT + _T(">;\r\n");
	}
}

void CStringArray_SplitAttachmentsWithTitle (CStringArray& ar, CStringArray& arTitle, const CString& s)
{
	ar.RemoveAll();

	CString str(s);
		str.Remove('\r');
		str.Remove('\n');
		str.Replace('\t', ' ');
		str.Trim();
		if (!str.IsEmpty() && str[str.GetLength() - 1] != ';')
		{
			str += ';';
		}
		if (str.IsEmpty())
			return;

	TCHAR* psz = new TCHAR [str.GetLength()+1];
	TB_TCSCPY(psz, (LPCTSTR)str);
	CString strT;
	int nTprec = 0; 
	int nT = _tcscspn(psz, _T(";")); 
	for ( ; nT < str.GetLength(); (nTprec = nT + 1), nT = nTprec + _tcscspn(&psz[nTprec], _T(";")))
	{
		psz[nT] = NULL_CHAR;
		TCHAR* pszToken = &psz[nTprec];

		int nPos = (_tcschr(pszToken, _T('<')) - pszToken);
		if (nPos < 0)
		{
			nPos = (_tcsrchr(pszToken, SLASH_CHAR) - pszToken);
			if (nPos < 0)
				strT = pszToken;
			else
				strT = &pszToken[nPos + 1];
		}
		else
		{
			pszToken[nPos] = NULL_CHAR;
			if (pszToken[nPos-1] == BLANK_CHAR) 
				pszToken[nPos-1] = NULL_CHAR;

			int nPos2 = (_tcschr(&pszToken[nPos+1], _T('>')) - pszToken);
			if (nPos2 > 0) 
				pszToken[nPos2] = NULL_CHAR;
			strT = &pszToken[nPos+1];
		}

		//----
		if (CStringArray_Find(ar, pszToken) == -1)
		{
			ar.Add(pszToken);
			arTitle.Add(strT);
		}
	}

	delete [] psz;
}

///////////////////////////////////////////////////////////////////////////////
CString CMapiMessage::TAG_CERTIFIED = _T("[C]"); 

//-----------------------------------------------------------------------------
void CMapiMessage::Assign(const CMapiMessage& e)
{
	m_sSubject = e.m_sSubject;
	m_sBody = e.m_sBody;
	m_sFrom = e.m_sFrom;
	m_sIdentity = e.m_sIdentity;
	m_sFromName = e.m_sFromName;
	m_sReplayTo = e.m_sReplayTo;

	m_bBodyFromFile = e.m_bBodyFromFile;
	m_bHtmlFromFile = e.m_bHtmlFromFile;
	m_bNeedEncoding = e.m_bNeedEncoding;
	m_bAlreadyEncoded = e.m_bAlreadyEncoded;
	m_sHtml = e.m_sHtml;

	m_sTemplateFileName = e.m_sTemplateFileName;
	m_sMapiProfile = e.m_sMapiProfile;
	m_sAttachmentReportName = e.m_sAttachmentReportName;

	m_To.Copy(e.m_To);
	m_CC.Copy(e.m_CC);
	m_BCC.Copy(e.m_BCC);

	m_Attachments.Copy(e.m_Attachments);
	m_AttachmentTitles.Copy(e.m_AttachmentTitles);
	m_Images.Copy(e.m_Images);
	m_ImagesCIDs.Copy(e.m_ImagesCIDs);

	m_arstrBodyParameterNames.Copy(e.m_arstrBodyParameterNames);;
	m_arstrBodyParameters.Copy(e.m_arstrBodyParameters);

	m_PostaLiteMsg.Assign(e.m_PostaLiteMsg);

	m_deEmailAddressType = e.m_deEmailAddressType;
}

//-----------------------------------------------------------------------------
void CMapiMessage::Clear()
{
	m_sSubject.Empty();
	m_sBody.Empty();
	m_sFrom.Empty();
	m_sIdentity.Empty();
	m_sHtml.Empty();
	m_bBodyFromFile = FALSE;
	m_bHtmlFromFile = FALSE;
	m_bNeedEncoding = FALSE;
	m_bAlreadyEncoded = FALSE;

	m_To.RemoveAll();
	m_CC.RemoveAll();
	m_BCC.RemoveAll();
	m_Attachments.RemoveAll();
	m_AttachmentTitles.RemoveAll(); 
	m_Images.RemoveAll();
	m_ImagesCIDs.RemoveAll(); 

	m_sMapiProfile.Empty();
	m_sTemplateFileName.Empty();
	m_sAttachmentReportName.Empty();

	m_arstrBodyParameters.RemoveAll(); 
	m_arstrBodyParameterNames.RemoveAll(); 

	m_PostaLiteMsg.Clear();

	m_deEmailAddressType = E_EMAIL_ADDRESS_TYPE_DEFAULT;
}

//-----------------------------------------------------------------------------
BOOL CMapiMessage::IsEmpty()
{
	return 
		m_sSubject.IsEmpty()			  &&
		m_sBody.IsEmpty()				  &&
		m_sFrom.IsEmpty()				  &&
		m_sIdentity.IsEmpty()			  &&
		m_sHtml.IsEmpty()				  &&
		m_bBodyFromFile == FALSE		  &&
		m_bHtmlFromFile == FALSE		  &&
		m_bNeedEncoding == FALSE		  &&
		m_bAlreadyEncoded == FALSE		  &&
		m_To.IsEmpty()					  &&
		m_CC.IsEmpty()					  &&
		m_BCC.IsEmpty()					  &&
		m_Attachments.IsEmpty()			  &&
		m_AttachmentTitles.IsEmpty()	  &&
		m_Images.IsEmpty()				  &&
		m_ImagesCIDs.IsEmpty()			  &&
		m_sMapiProfile.IsEmpty()		  &&
		m_sTemplateFileName.IsEmpty()	  &&
		m_sAttachmentReportName.IsEmpty() &&
		m_arstrBodyParameters.IsEmpty()   && 
		m_arstrBodyParameterNames.IsEmpty() &&
		m_PostaLiteMsg.IsEmpty(); 
}

//-----------------------------------------------------------------------------
BOOL CMapiMessage::IsEmptyRecipients()
{
	return 	m_To.GetSize() == 0 && m_CC.GetSize() == 0 &&  m_BCC.GetSize() == 0;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetSubject (LPCTSTR pszSubject, BOOL bAppend/*=FALSE*/, BOOL bSkipDuplicate/*=FALSE*/)
{
  	if (bAppend)
	{
		if(bSkipDuplicate)
		{
			
			CString s(m_sSubject);

			int first = s.Find(pszSubject);
			if (first > 0 && m_sSubject != _T(""))
				return;
		}

		m_sSubject += " ";
		m_sSubject += pszSubject;
	}
	else if (*pszSubject)
		m_sSubject = pszSubject;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetBody (LPCTSTR pszContents, BOOL bAppend/*=TRUE*/, BOOL bSkipDuplicate/*=FALSE*/)
{
	if (bAppend)
	{
		if (bSkipDuplicate)
		{
			CString s(m_sBody);

			int first = s.Find(pszContents);
			if (first > 0 && m_sSubject != _T(""))
				return;
		}

		m_sBody += "\r\n";
		m_sBody += pszContents;
	}
	else
		m_sBody = pszContents;

	m_bBodyFromFile = FALSE;
}
//-----------------------------------------------------------------------------
void CMapiMessage::SetHtml (LPCTSTR pszContents, BOOL bNeedEncoding, BOOL bAppend/*=TRUE*/)
{
	ASSERT_TRACE(pszContents,"Parameter pszContents cannot be null");

	if (bAppend)
	{
		m_sHtml += _T("<br>");
		m_sHtml += pszContents;
	}
	else 
		m_sHtml = pszContents;

	CString s(m_sHtml);	s.MakeLower();
	if (s.Find(_T("<br")) < 0  && s.Find(_T("<p>")) < 0 )
	{
		m_sHtml.Remove('\r');
		m_sHtml.Replace(_T("\n"), _T("<br>"));
	}

	m_bHtmlFromFile = FALSE;
	m_bNeedEncoding = bNeedEncoding;

	if (bNeedEncoding)
		m_bAlreadyEncoded = FALSE;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetBodyFileName (LPCTSTR pszContents)
{
	m_sBody = pszContents;
	m_bBodyFromFile = TRUE;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetHtmlFileName (LPCTSTR pszContents, BOOL bNeedEncoding)
{
	m_sHtml = pszContents;
	m_bHtmlFromFile = TRUE;
	m_bNeedEncoding = bNeedEncoding;

	if (bNeedEncoding)
		m_bAlreadyEncoded = FALSE;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetFrom (LPCTSTR pszFrom)
{
	if (*pszFrom)
	{
		m_sFrom = pszFrom;
		int nPos = m_sFrom.Find(';');
		if (nPos > 0)
			m_sFrom = m_sFrom.Left(nPos);
	}
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetFromName (LPCTSTR psz)
{
	if (*psz)
		m_sFromName = psz;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetReplayTo (LPCTSTR psz)
{
	if (*psz)
		m_sReplayTo = psz;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetTemplateFileName (LPCTSTR pszTemplateFileName)
{
	if (*pszTemplateFileName)
		m_sTemplateFileName = pszTemplateFileName;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetMapiProfile (LPCTSTR pszMapiProfile)
{
	m_sMapiProfile = pszMapiProfile;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetIdentity (LPCTSTR pszIdentity)
{
	if (*pszIdentity)
		m_sIdentity = pszIdentity;
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetAttachmentReportName (LPCTSTR pszAttachmentReportName)
{
	if (*pszAttachmentReportName)
	{
		m_sAttachmentReportName = pszAttachmentReportName;

		m_sAttachmentReportName.Replace('/', '-');
		m_sAttachmentReportName.Replace('\\', '-');
		m_sAttachmentReportName.Replace('*', ' ');
		m_sAttachmentReportName.Replace('?', ' ');
		m_sAttachmentReportName.Replace('"', '\'');
		m_sAttachmentReportName.Replace('<', '(');
		m_sAttachmentReportName.Replace('>', ')');
		m_sAttachmentReportName.Replace('|', '-');
		m_sAttachmentReportName.Replace(':', ' ');
		m_sAttachmentReportName.Replace('.', ' ');
	}
}

//-----------------------------------------------------------------------------
void SetTo (CStringArray& addrs, LPCTSTR pszTo, BOOL bAppend, LPCTSTR szTag)
{
	if (!bAppend)
		addrs.RemoveAll();

	CString str (pszTo);
	str.Trim();
	if (str.IsEmpty()) 
		return;

	if (str.Find(';') >= 0)
	{
		int l = str.GetLength() - 1;
		if (str[l] != ';' && str[l] != '\n')
			str += ';';

		CStringArray a;
		CStringArray_Split (a, str);

		for (int i = 0; i < a.GetSize(); i++)
		{
			SetTo (addrs, a[i], bAppend || i > 0, szTag);
		}
		return;
	}

	if (szTag)
	{
		if (_tcsnicmp((LPCTSTR)str, szTag, _tcsclen(szTag)))
			str = szTag + str;
	}

	if (bAppend)
	{
		if (CStringArray_Find(addrs, str) == -1)
			addrs.Add(str);
	}
	else
	{
		addrs.Add(str);
	}
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetTo (LPCTSTR pszTo, BOOL bAppend/*=TRUE*/, LPCTSTR szTag/* = NULL*/)
{
	::SetTo(m_To, pszTo, bAppend, szTag);
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetCc (LPCTSTR pszCc, BOOL bAppend/*=TRUE*/, LPCTSTR szTag/* = NULL*/)
{
	::SetTo(m_CC, pszCc, bAppend, szTag);
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetBcc (LPCTSTR pszBcc, BOOL bAppend/*=TRUE*/, LPCTSTR szTag/* = NULL*/)
{
	::SetTo(m_BCC, pszBcc, bAppend, szTag);
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetAttachment (LPCTSTR pszAttachment, BOOL bAppend/*=TRUE*/)
{
	if (!bAppend)
	{
		m_Attachments.RemoveAll();
		m_AttachmentTitles.RemoveAll();
	}

	CString str (pszAttachment);
	str.Trim();

	if (str.Find(';') >= 0)
	{
		if (str.Right(1) != ';' && str.Right(1) != '\n')
			str += ';';

		CStringArray ar;
		CStringArray_Split(ar, str);

		for (int i = 0; i < ar.GetSize(); i++)
		{
			SetAttachment (ar[i], bAppend || i > 0);
		}
		return;
	}

	if (str.IsEmpty()) 
		return;
		
	m_Attachments.Add(str);

	int nPos = str.ReverseFind('\\');
	if (nPos < 0)
		nPos = str.ReverseFind('/');

	if (nPos >= 0 && nPos < (str.GetLength() - 1))
		str = str.Mid(nPos + 1);

	m_AttachmentTitles.Add (str);
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetAttachment (LPCTSTR pszAttachment, LPCTSTR pszAttachmentTitle, BOOL bAppend, int pos/* = -1*/)
{
	if (!bAppend)
	{
		m_Attachments.RemoveAll();
		m_AttachmentTitles.RemoveAll();
	}

	if (CStringArray_Find(m_Attachments, pszAttachment) == -1)
	{
		if (pos < 0)
			m_Attachments.Add(pszAttachment);
		else
			m_Attachments.InsertAt(pos, pszAttachment);

		CString strTitle (pszAttachmentTitle); strTitle.Trim();
		if (!strTitle.IsEmpty())
		{
			CString sAttachment (pszAttachment);

			int nPosA = sAttachment.ReverseFind('.');
			int nPosT = strTitle.ReverseFind('.');
			if (nPosA >= 0)
			{
				CString sExtA = sAttachment.Mid(nPosA);
				if (nPosT >= 0)
				{
					CString sExtT = strTitle.Mid(nPosT);
					if (sExtA.CompareNoCase(sExtT))
						strTitle += sExtA;
				}
				else
					strTitle += sExtA;
			}

			if (pos < 0)
				m_AttachmentTitles.Add(strTitle);
			else
				m_AttachmentTitles.InsertAt(pos, strTitle);
		}
		else
		{
			strTitle = pszAttachment;

			int nPos = strTitle.ReverseFind('\\');
			if (nPos < 0)
				nPos = strTitle.ReverseFind('/');

			if (nPos >= 0 && nPos < (strTitle.GetLength() - 1))
				strTitle = strTitle.Mid(nPos + 1);
			
			if (pos < 0)
				m_AttachmentTitles.Add (strTitle);
			else
				m_AttachmentTitles.InsertAt(pos, strTitle);
		}
	}
}

void CMapiMessage::InsertTopAttachment (LPCTSTR pszAttachment, LPCTSTR pszAttachmentTitle)
{
	SetAttachment (pszAttachment, pszAttachmentTitle, TRUE, 0);
}

//-----------------------------------------------------------------------------
void CMapiMessage::RemoveAllPdfAttachments()
{
	ASSERT(m_Attachments.GetSize() == m_AttachmentTitles.GetSize());

	for (int i = m_Attachments.GetUpperBound(); i >= 0; i--)
	{
		CString sA = m_Attachments[i];
		if (sA.GetLength() > 4 && sA.Right(4).CompareNoCase(L".pdf") == 0)
		{
			m_Attachments.RemoveAt(i);
			m_AttachmentTitles.RemoveAt(i);
		}
	}
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetImage (LPCTSTR pszImage, LPCTSTR pszImageCID, BOOL bAppend/*=TRUE*/)
{
	if (!bAppend)
	{
		m_Images.RemoveAll();
		m_ImagesCIDs.RemoveAll();
	}
	if (CStringArray_Find(m_Images, pszImage) == -1)
	{
		m_Images.Add(pszImage);

		if (pszImageCID && *pszImageCID)
			m_ImagesCIDs.Add(pszImageCID);
		else
		{
			CString strImageCID = pszImage;
			int nPos = strImageCID.ReverseFind('\\');
			if (nPos >= 0)
				m_ImagesCIDs.Add (strImageCID.Right(strImageCID.GetLength() - nPos));
			else
				m_ImagesCIDs.Add (pszImageCID);
		}
	}
}

//-----------------------------------------------------------------------------
void CMapiMessage::SetBodyParameter (LPCTSTR pszName, LPCTSTR pValue)
{
	int nPos = CStringArray_Find(m_arstrBodyParameterNames, pszName);
	if (nPos == -1)
	{
		m_arstrBodyParameters.Add(pValue);
		m_arstrBodyParameterNames.Add(pszName);
	}
}

//-----------------------------------------------------------------------------
BOOL CMapiMessage::GetBodyParameter (LPCTSTR pszName, CString& sValue)
{
	int nPos = CStringArray_Find(m_arstrBodyParameterNames, pszName);
	if (nPos == -1)	return FALSE;

	sValue = m_arstrBodyParameters.GetAt(nPos);
	return TRUE;
}

//-----------------------------------------------------------------------------
int CMapiMessage::AddPostaLiteMsg
		(
			const CString&	sFax,
			const CString&	sAddressee,
			const CString&	sAddress,
			const CString&	sZipCode,	
			const CString&	sCity,	
			const CString&	sCounty,	
			const CString&	sCountry,
			const CString&	sISOCode,

			const DataEnum&	diDeliveryType, 
			const DataEnum&	diPrintType,
			const CString&	sAuxkey
		)
{
	return m_PostaLiteMsg.Add
				(
					sFax,
					sAddressee,
					sAddress,
					sZipCode,	
					sCity,	
					sCounty,	
					sCountry,
					sISOCode,
					diDeliveryType,
					diPrintType,
					sAuxkey
				);
}

//////////////////////////////////////////////////////////////////////////////////
void CMapiMessage::SplitAllAddress()
{
	CString str;
	
	CStringArray_Concat(m_To, str);
	CStringArray_Split(m_To, str);
	
	CStringArray_Concat(m_CC, str);
	CStringArray_Split(m_CC, str);
	
	CStringArray_Concat(m_BCC, str);
	CStringArray_Split(m_BCC, str);
}

//-----------------------------------------------------------------------------
BOOL CMapiMessage::SplitCertifiedAddress(CMapiMessage& msgNormal, CMapiMessage& msgCertified)
{
	//un singolo indirizzo per ogni elemento dell'array (potevano essere multipli separati dal ;)
	SplitAllAddress();

	//duplico il messaggio
	msgNormal.Assign(*this);
	msgCertified.Assign(*this);

	//suddivido gli indirizzi tagged/no tagged
	CStringArray_RemoveWhenPrefixed		(msgNormal.m_To, TAG_CERTIFIED);
	CStringArray_RemoveWhenPrefixed		(msgNormal.m_CC, TAG_CERTIFIED);
	CStringArray_RemoveWhenPrefixed		(msgNormal.m_BCC, TAG_CERTIFIED);
	
	CStringArray_RemoveWhenNotPrefixed	(msgCertified.m_To, TAG_CERTIFIED);
	CStringArray_RemoveWhenNotPrefixed	(msgCertified.m_CC, TAG_CERTIFIED);
	CStringArray_RemoveWhenNotPrefixed	(msgCertified.m_BCC, TAG_CERTIFIED);
	
	//ora sono filtrati, posso togliere il TAG
	CStringArray_RemovePrefix			(msgCertified.m_To, TAG_CERTIFIED);
	CStringArray_RemovePrefix			(msgCertified.m_CC, TAG_CERTIFIED);
	//CStringArray_RemovePrefix			(msgCertified.m_BCC, TAG_CERTIFIED);
	msgCertified.m_BCC.RemoveAll();	//non ammessi

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////////

CPostaLiteAddress::CPostaLiteAddress()
	: m_deDeliveryType	(E_POSTALITE_DELIVERY_TYPE_DEFAULT)
	, m_dePrintType		(E_POSTALITE_PRINT_TYPE_DEFAULT)
{
}


CPostaLiteAddress::CPostaLiteAddress (const CPostaLiteAddress& pa)
{
	*this = pa;
}

CPostaLiteAddress& CPostaLiteAddress::operator = (const CPostaLiteAddress& pa)
{
	m_sFax			= pa.m_sFax;
	m_sAddressee	= pa.m_sAddressee;
	m_sAddress		= pa.m_sAddress;
	m_sZipCode		= pa.m_sZipCode;	
	m_sCity			= pa.m_sCity;	
	m_sCounty		= pa.m_sCounty;	
	m_sCountry		= pa.m_sCountry;
	m_sISOCode		= pa.m_sISOCode;
	m_deDeliveryType = pa.m_deDeliveryType; 
	m_dePrintType	= pa.m_dePrintType;
	m_sAuxKey		= pa.m_sAuxKey;

	return *this;
}

//-----------------------------------------------------------------------------
CPostaLiteAddress::Delivery CPostaLiteAddress::ConvertDeliveryType(DataEnum deDelivery)
{
	switch(deDelivery.GetValue())
	{
		case E_POSTALITE_DELIVERY_TYPE_POSTA_MASSIVA:
			return PostaMassiva;
		case E_POSTALITE_DELIVERY_TYPE_POSTA_PRIORITARIA:
			return PostaPrioritaria;
		case E_POSTALITE_DELIVERY_TYPE_RACCOMANDATA:
			return PostaRaccomandata;
		case E_POSTALITE_DELIVERY_TYPE_RACCOMANDATA_AR:
			return PostaRaccomandataAR;
		case E_POSTALITE_DELIVERY_TYPE_FAX:
		{
			AfxMessageBox(_TB("FAX delivery type was not supported"));
			return PostaMassiva; //Fax;
		}
	}
	return PostaMassiva;
}
//-----------------------------------------------------------------------------

CPostaLiteAddress::Print CPostaLiteAddress::ConvertPrintType(DataEnum dePrint)
{
	switch(dePrint.GetValue())
	{
		case E_POSTALITE_PRINT_TYPE_FRONT_BW:
			return Front_BlackWhite;
		case E_POSTALITE_PRINT_TYPE_FRONTBACK_BW:
			return FrontBack_BlackWhite;
		case E_POSTALITE_PRINT_TYPE_FRONT_COLOR:
			return Front_Color;
		case E_POSTALITE_PRINT_TYPE_FRONTBACK_COLOR:
			return FrontBack_Color;
	}
	return Front_BlackWhite;
}


//////////////////////////////////////////////////////////////////////////////////

void CPostaLiteMsg::Assign(const CPostaLiteMsg& pm)
{
	RemoveAll();
	//__super::Assign(pm);

	m_bUsePostaLite = pm.m_bUsePostaLite;

	m_sDocNamespace = pm.m_sDocNamespace;
	m_sDocPrimaryKey = pm.m_sDocPrimaryKey;

	m_sAddresseeNamespace = pm.m_sAddresseeNamespace;
	m_sAddresseePrimaryKey = pm.m_sAddresseePrimaryKey;

	m_nFilePages = pm.m_nFilePages;
	m_nFileSize = pm.m_nFileSize;

	for (int i = 0; i < pm.GetSize(); i++)
	{
		CPostaLiteAddress* pa = (CPostaLiteAddress*) pm.GetAt(i); 
		Add (new CPostaLiteAddress(*pa));
	}
}

//-----------------------------------------------------------------------------
void CPostaLiteMsg::Clear()
{
	RemoveAll();

	m_bUsePostaLite = FALSE;

	m_sDocNamespace.Empty();
	m_sDocPrimaryKey.Empty();

	m_sAddresseeNamespace.Empty();
	m_sAddresseePrimaryKey.Empty();
}

//-----------------------------------------------------------------------------
BOOL CPostaLiteMsg::IsEmpty()
{
	return !m_bUsePostaLite || GetSize() == 0;
}

//-----------------------------------------------------------------------------
int CPostaLiteMsg::Add
		(
			const CString&	sFax,
			const CString&	sAddressee,
			const CString&	sAddress,
			const CString&	sZipCode,	
			const CString&	sCity,	
			const CString&	sCounty,	
			const CString&	sCountry,
			const CString&	sISOCode,
			const DataEnum&	deDeliveryType, 
			const DataEnum&	dePrintType,
			const CString&	sAuxkey
		)
{
	CPostaLiteAddress* pAddr = new CPostaLiteAddress
										(
											sFax,
											sAddressee,
											sAddress,
											sZipCode,	
											sCity,	
											sCounty,	
											sCountry,
											sISOCode,
											deDeliveryType,
											dePrintType,
											sAuxkey
										);
	 return __super::Add(pAddr);
}

//-----------------------------------------------------------------------------
int CPostaLiteMsg::Add (const CString& sFax, const CString&	sISOCode)
{
	CPostaLiteAddress* pAddr = new CPostaLiteAddress(sFax, sISOCode);
	return __super::Add(pAddr);
}

//////////////////////////////////////////////////////////////////////////////////

IMapiSession::IMapiSession(BOOL /*bMultiThreaded = FALSE*/, BOOL /*bNoLogonUI  = FALSE*/, BOOL /*bNoInitializeMAPI = FALSE*/, CDiagnostic* /*pDiagnostic = NULL*/)
{
}

//-----------------------------------------------------------------------------
IMapiSession::~IMapiSession()
{
}

//-----------------------------------------------------------------------------
BOOL IMapiSession::Logon(const CString& /*sProfileName*/, const CString& /*sPassword*/, CWnd* /*pParentWnd*/)
{
  return FALSE;
}

//-----------------------------------------------------------------------------
BOOL IMapiSession::LoggedOn() const
{
  return FALSE;
}

//-----------------------------------------------------------------------------
BOOL IMapiSession::MapiInstalled() const
{
  return FALSE;
}

//-----------------------------------------------------------------------------
BOOL IMapiSession::Logoff()
{
  return FALSE;
}
  
//-----------------------------------------------------------------------------
BOOL IMapiSession::ShowAddressBook(const CStringArray& /*arOldNames*/, CStringArray& /*arNewNames*/, HWND  /*parent = 0*/)
{
	return FALSE;
}

//-----------------------------------------------------------------------------
BOOL IMapiSession::Send(CMapiMessage& /*message*/)
{
  return FALSE;
}

//-----------------------------------------------------------------------------
ULONG IMapiSession::GetLastError() const
{
  return 0;
}

// by A.R.: decodifica codici di errore
//-----------------------------------------------------------------------------
CString IMapiSession::GetLastErrorMessage()
{
	return _T("");
}

///////////////////////////////////////////////////////////////////////////////

IMPLEMENT_DYNAMIC(IMailConnector, CObject)

MailConnectorParams* IMailConnector::GetParams()
{
	if (m_pParams == NULL)
		m_pParams = new MailConnectorParams();
	return m_pParams;
}

//-----------------------------------------------------------------------------
IMailConnector::~IMailConnector() 
{
	delete m_pParams; 
}

//-----------------------------------------------------------------------------
IMailConnector* AfxGetIMailConnector()
{
	return AfxGetApplicationContext()->GetObject<IMailConnector>(&CApplicationContext::GetMailConnector);
}

//=============================================================================

//BOOL EmptyGetCompanyAddressInfos(CCompanyAddressInfo*) { return FALSE;  }

FGetCompanyAddressInfos* PostaLiteStaticMethods::s_pfGetCompanyInfos = NULL; // EmptyGetCompanyInfos;

BOOL PostaLiteStaticMethods::IsValidEmailAddress(CString emailAddress)
{
	int nAtIndex = emailAddress.Find(_T("@"));
	int nPeriodIndex = emailAddress.Find(_T("."));
	if (nAtIndex < 0 || nPeriodIndex < 0)
		return FALSE;

	return TRUE;
}

//---------------------------------------------------------------------------------------
BOOL PostaLiteStaticMethods::TelephoneNumberHasInvalidChars(CString strNumber)
{
	return
		(
		strNumber.Find(_T("+")) >= 0 ||
		strNumber.Find(_T(".")) >= 0 ||
		strNumber.Find(_T(":")) >= 0 ||
		strNumber.Find(_T("-")) >= 0
		);
}

//---------------------------------------------------------------------------------------
DataStr PostaLiteStaticMethods::RemoveInvalidCharsFromTelephone(CString strNumber)
{
	strNumber.Replace(_T("."), _T(""));
	strNumber.Replace(_T(":"), _T(""));
	strNumber.Replace(_T("-"), _T(""));
	strNumber.Replace(_T("+"), _T("00"));
	return DataStr(strNumber);
}

//---------------------------------------------------------------------------------------
BOOL PostaLiteStaticMethods::IsValidTaxIdNumber(DataStr strTaxIdNumber)
{
	return TRUE;
}

//---------------------------------------------------------------------------------------
BOOL PostaLiteStaticMethods::IsValidFiscalCode(DataStr strFiscalCode)
{
	return TRUE;
}

//---------------------------------------------------------------------------------------
