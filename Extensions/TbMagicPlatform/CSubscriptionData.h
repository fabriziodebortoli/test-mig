#pragma once

#include <TbGeneric\DataObj.h>

//includere alla fine degli include del .H
#include "beginh.dex"

class CSubscriptionData;
class CAbstractFormDoc;

typedef CTypedPtrMap<CMapStringToOb, CString, CSubscriptionData*> CEventManagementData;
typedef CTypedPtrMap<CMapStringToOb, CString, CEventManagementData*> CDocumentSubscritpionData;
typedef CTypedPtrMap<CMapStringToOb, CString, CDocumentSubscritpionData*> CSubscritpionsList;

/////////////////////////////////////////////////////////////////////////////
//								CSubscriptionData
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSubscriptionData : public CObject
{
	DECLARE_DYNAMIC(CSubscriptionData)

public:
	CSubscriptionData	(
							const CString& strExportProfile, 
							CPathFinder::PosType ePosType, 
							const CString& strWebServer, 
							const CString& strWebService, 
							const CString& strWebNS, 
							const CString& strWebMethod, 
							int dPort,
							const CString& strViewModes
						);
	CSubscriptionData(const CString& strOperation, const CString& strMessage, int dAction);

public:
	CString					m_strExportProfile;
	CPathFinder::PosType	m_ePosType;
	CString					m_strWebServer;
	CString					m_strWebService;
	CString					m_strWebNS;
	CString					m_strWebMethod;
	int						m_Port;
	CStringArray			m_tmpExportResult;
	CString					m_strOperation;
	CString					m_strMessage;
	int						m_Action;
	CString					m_strViewModes;

public:
	BOOL IsActiveInForegroundMode();
	BOOL IsActiveInBackgroundMode();
};

/////////////////////////////////////////////////////////////////////////////
//								CSubscriptionInfo
//////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT CSubscriptionInfo : public CObject, public CTBLockable
{
	DECLARE_DYNCREATE(CSubscriptionInfo)

public:
	CSubscriptionInfo();
	~CSubscriptionInfo();

private:
	CSubscritpionsList*		m_pSubscriptionList;
	CSubscritpionsList*		m_pBONEList;
	CStringArray			m_Producers;

public:
	virtual LPCSTR  GetObjectName() const { return "CSubscriptionInfo"; }

	void						Clear						();
	void						Load						();
	BOOL						ExistDocumentTrigger		(CAbstractFormDoc* pDoc);
	BOOL						ExistDocumentBONE			(CAbstractFormDoc* pDoc);
	CDocumentSubscritpionData*	GetDocumentSubscritpionData	(CAbstractFormDoc* pDoc, BOOL bIsBONE);
	CEventManagementData*		GetEventManagementData		(CAbstractFormDoc* pDoc, int dAction, CString aWhen, BOOL bIsBONE);
	BOOL Lookup					(CString strTagNS, CDocumentSubscritpionData*& pDocSubscrData, BOOL bIsBONE);
	void SetAt					(CString strTagNS, CDocumentSubscritpionData* pDocSubscrData, BOOL bIsBONE);

private:
	void			ClearBONEList					();
	void			ClearSubscriptionData			(CSubscriptionData*);
	void			ClearEventManagementData		(CEventManagementData*);
	void			ClearDocumentSubscritpionData	(CDocumentSubscritpionData*);
	BOOL			CheckProducer					(CString strProducer);
	void			Load							(CString strSubscriptionPath);
	BOOL			CheckFile			(	CString				strFile,
											CXMLDocumentObject&	aDocument);

	BOOL			ParseFile			(	CString				strProducer,
											CXMLDocumentObject&	aDocument);

	BOOL			ParseTagSubscription(	CString strProducer,
											CXMLNode* pTagNode);
};

//=============================================================================
TB_EXPORT CSubscriptionInfo* AfxGetSubscriptionInfo();

#include "endh.dex"
