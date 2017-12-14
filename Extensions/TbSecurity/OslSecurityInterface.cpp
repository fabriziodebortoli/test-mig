
#include "stdafx.h"
#include <OleDbErr.h>

#include <TbNameSolver\PathFinder.h>

#include <TbClientCore\ClientObjects.h>
#include <TbWebServicesWrappers\LoginManagerInterface.h>

#include <TbGeneric\tools.h>
#include <TbGeneric\Crypt.h>

#include <TbGenlib\generic.h>
#include <TbGenlib\baseapp.h>

#include <TbOledb\sqlConnect.h>	
#include <TbOledb\sqltable.h>	

#include <TbGes\extdoc.h>
#include "OslSecurityInterface.h"
#include "OslDlg.h"

#ifdef _DEBUG
#undef THIS_FILE
static const char BASED_CODE THIS_FILE[] = __FILE__;
#define new DEBUG_NEW
#endif

//=============================================================================

SqlConnection* AfxGetSecuritySqlConnection() { return ((CSecurityInterface*)AfxGetSecurityInterface())->m_pSqlConnection; }

/////////////////////////////////////////////////////////////////////////////////////
//	class COSLCacheGrantInfo : public CMapStringToPtr

struct SOSLGrantInfo
{
public:
	DWORD m_dwGrant;
	DWORD m_dwInheritMask;

	SOSLGrantInfo (DWORD dwGrant, DWORD dwInheritMask) 
		: 
		m_dwGrant (dwGrant),
		m_dwInheritMask (dwInheritMask)
	{}
};

//------------------------------------------------------------------
//cerca l'elemento e se lo trova valorizza i grant
BOOL COSLCacheGrantInfo::Lookup (CInfoOSL* pInfoOSL, LPCTSTR pszNamespace /*= NULL*/) const 
{
	CString ns(pszNamespace ? pszNamespace : pInfoOSL->m_Namespace.ToString());
	ns.MakeLower(); ns.Trim();
	if (ns.IsEmpty())
		return FALSE;

	TB_LOCK_FOR_READ();

	SOSLGrantInfo* pGrant;
	BOOL bFinded = CMapStringToPtr::Lookup ((LPCTSTR)ns, (void*&)pGrant);

	if (bFinded)
	{
		pInfoOSL->m_dwGrant			= pGrant->m_dwGrant;
		pInfoOSL->m_dwInheritMask	= pGrant->m_dwInheritMask;
	}
	return bFinded;
}

//------------------------------------------------------------------
//cerca l'elemento e se lo trova valorizza i grant
void COSLCacheGrantInfo::SetAt (CInfoOSL* pInfoOSL, LPCTSTR pszNamespace /*= NULL*/) 
{
	CString ns(pszNamespace ? pszNamespace : pInfoOSL->m_Namespace.ToString());
	ns.MakeLower(); ns.Trim();
	if (ns.IsEmpty())
		return;

	TB_LOCK_FOR_WRITE();

	CMapStringToPtr::SetAt ((LPCTSTR)ns, new SOSLGrantInfo(pInfoOSL->m_dwGrant, pInfoOSL->m_dwInheritMask));
}

//------------------------------------------------------------------
void COSLCacheGrantInfo::RemoveAll() 
{
	TB_LOCK_FOR_WRITE();

	CString strKey; SOSLGrantInfo* pGrant = NULL;
	for (POSITION pos = GetStartPosition(); pos != NULL; pGrant = NULL, strKey.Empty())
	{
		GetNextAssoc (pos, strKey, (void*&)pGrant );
		SAFE_DELETE (pGrant);
	}
	CMapStringToPtr::RemoveAll();
};

//==================================================================================

///////////////////////////////////////////////////////////////////////////////
//								CSecurityInterface
///////////////////////////////////////////////////////////////////////////////
IMPLEMENT_DYNAMIC(CSecurityInterface, CBaseSecurityInterface);

//-----------------------------------------------------------------------------------
CSecurityInterface::CSecurityInterface ()
	:
	m_bIsSuperUser		(FALSE),
	m_pSqlConnection	(NULL)
{
	SqlSession* pSqlS = AfxOpenSqlSession(AfxGetLoginManager()->GetSystemDBConnectionString());
	m_pSqlConnection = pSqlS->GetSqlConnection();
}

//------------------------------------------------------------------------------
CSecurityInterface::~CSecurityInterface ()
{
	if (m_pSqlConnection)
	{
		ASSERT(m_pSqlConnection->CanClose());
		m_pSqlConnection->Close();
	}
	ResetKnownFlags();
}

//---------------------------------------------------------------------------------------
void CSecurityInterface::ResetKnownFlags()
{
	m_mapCacheGrant.RemoveAll();

	//clear security info on database objects 
	if (AfxGetDefaultSqlConnection())
	{
		POSITION pos;
		CString key;
		SqlCatalogEntry* pCatalogEntry;
		{ //lock scope
			SqlCatalogConstPtr pCatalog = AfxGetDefaultSqlConnection()->GetCatalog();
			for (pos = pCatalog->GetStartPosition(); pos != NULL;)
			{
				pCatalog->GetNextAssoc(pos, key, (CObject*&) pCatalogEntry);
				ASSERT(pCatalogEntry);
				if (pCatalogEntry)
					pCatalogEntry->GetInfoOSL()->SetDefaultGrant();
			}
		}
	}
}

//------------------------------------------------------------------------------
class SPGetUserGrant : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SPGetUserGrant) 

public:
	DataLng		f_In_CompanyId;
	DataLng		f_In_UserId;
	DataStr		f_In_ObjectNamespace;
	DataLng		f_In_ObjectType;

	DataLng		f_Out_Grant;
	DataLng		f_Out_Inheritmask;

	DataLng		f_Ret_Result;

public:
	SPGetUserGrant();

public:
	virtual void	BindRecord	();	

public:
	static LPCTSTR GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//						SPGetUserGrant
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(SPGetUserGrant, SqlRecordProcedure) 

//-----------------------------------------------------------------------------
SPGetUserGrant::SPGetUserGrant()
:
	SqlRecordProcedure(GetStaticName(), AfxGetSecuritySqlConnection())
{
	BindRecord();
}

//-----------------------------------------------------------------------------
LPCTSTR SPGetUserGrant::GetStaticName() { return _T("MSD_GetUserGrant"); }

//-----------------------------------------------------------------------------
void SPGetUserGrant::BindRecord()
{
	BEGIN_BIND_PARAM_DATA()	

		BIND_PARAM(_T("@RETURN_VALUE"),					f_Ret_Result); 

		BIND_PARAM(_T("@par_companyid"),				f_In_CompanyId);
		BIND_PARAM(_T("@par_userid"),					f_In_UserId); 
		BIND_PARAM(_T("@par_objectNameSpace"),			f_In_ObjectNamespace); 
		BIND_PARAM(_T("@par_objectType"),				f_In_ObjectType); 

		BIND_PARAM(_T("@parout_object_grant"),			f_Out_Grant); 
		BIND_PARAM(_T("@parout_object_inheritmask"),	f_Out_Inheritmask); 

	END_BIND_PARAM_DATA()	
}

//-----------------------------------------------------------------------------
BOOL CSecurityInterface::GetObjectGrant (CInfoOSL* pInfoOSL) 
{ 
	ASSERT(pInfoOSL);
	if ( pInfoOSL->m_dwGrant & OSL_GRANT_PROTECTION_FLAG_KNOWN )
		return TRUE;

	//int kk = 0;
	//if (pInfoOSL->GetType() == OSLType_TabDialog)
	//	kk = 1;

	if 
		( 
			! IsSecurityEnabled() ||
			AfxGetLoginInfos()->m_bAdmin || 
			IsSuperUser()
		)
		return CBaseSecurityInterface::GetObjectGrant (pInfoOSL);

	if 
		( 
			pInfoOSL->GetType() == OSLType_AddOnMod ||
			pInfoOSL->GetType() == OSLType_AddOnApp 
		)
		return CBaseSecurityInterface::GetObjectGrant (pInfoOSL);

	if (m_mapCacheGrant.Lookup(pInfoOSL, pInfoOSL->m_Namespace.ToString()))
		return TRUE;

	if 
		(
			pInfoOSL->m_pParent 
			&& 
			(pInfoOSL->m_pParent->m_dwGrant & OSL_GRANT_PROTECTION_FLAG_KNOWN) == 0
		)
	{
		BOOL bRet = GetObjectGrant (pInfoOSL->m_pParent);
		if (!bRet)
			return FALSE;
	}
	//----

	CBaseSecurityInterface::GetObjectGrant (pInfoOSL);
	
	//---- Lettura permessi 
	SPGetUserGrant aSpUserGrant;
	aSpUserGrant.SetValid();
	aSpUserGrant.f_Ret_Result = 0;
	SqlTable aTable(&aSpUserGrant, AfxGetSecuritySqlConnection()->GetDefaultSqlSession());
	
	const CLoginInfos *pInfos = AfxGetLoginInfos();

	aSpUserGrant.f_In_CompanyId				= pInfos->m_nCompanyId;
	aSpUserGrant.f_In_UserId				= pInfos->m_nLoginId;
	CString sNs = pInfoOSL->m_Namespace.ToUnparsedString();
	sNs.MakeLower();

	if (pInfoOSL->GetType() == OSLType_Report)
	{ 
		if (sNs.Right(4) == _T(".wrm"))
			sNs = sNs.Left(sNs.GetLength() - 4);
		else if (sNs.Right(4) == _T(".rde"))
			sNs = sNs.Left(sNs.GetLength() - 4);
		else if (sNs.Right(5) == _T(".wrmt"))
			sNs = sNs.Left(sNs.GetLength() - 5);
	}
	//else if (pInfoOSL->GetType() == OSLType_OfficeDocument)
	//{
	//	if (sNs.Right(4) == _T(".doc"))
	//		sNs = sNs.Left(sNs.GetLength() - 4);
	//	else if (sNs.Right(4) == _T(".xls"))
	//		sNs = sNs.Left(sNs.GetLength() - 4);

	//	else if (sNs.Right(5) == _T(".docx"))
	//		sNs = sNs.Left(sNs.GetLength() - 5);
	//	else if (sNs.Right(5) == _T(".xlsx"))
	//		sNs = sNs.Left(sNs.GetLength() - 5);

	//	if (sNs.Right(4) == _T(".dot"))
	//		sNs = sNs.Left(sNs.GetLength() - 4);
	//	else if (sNs.Right(4) == _T(".xlt"))
	//		sNs = sNs.Left(sNs.GetLength() - 4);

	//	else if (sNs.Right(5) == _T(".dotx"))
	//		sNs = sNs.Left(sNs.GetLength() - 5);
	//	else if (sNs.Right(5) == _T(".xltx"))
	//		sNs = sNs.Left(sNs.GetLength() - 5);
	//}

	aSpUserGrant.f_In_ObjectNamespace		= sNs;
	aSpUserGrant.f_In_ObjectType			= pInfoOSL->GetType();

	DWORD nRetVal = 0;
	try 
	{
		aTable.Open();
		aTable.Call();

		pInfoOSL->m_dwGrant = (long) (aSpUserGrant.f_Out_Grant);
		pInfoOSL->m_dwInheritMask = (long) (aSpUserGrant.f_Out_Inheritmask);

		nRetVal = (long) (aSpUserGrant.f_Ret_Result);

		aTable.Close();

		if (aSpUserGrant.f_Ret_Result != 0)
		{
			TRACE("\nSecurity.InsertObject returns error code %d\n", aSpUserGrant.f_Ret_Result);
			ASSERT(FALSE);
		}
	}
	catch(SqlException* e)
	{
		TCHAR szError[8000];
		CString strErr;
		e->GetErrorString(e->m_nHResult, strErr);
		e->GetErrorMessage(szError, 8000);
		TRACE(_T("%s ; %s\n"), (LPCTSTR)e->m_strError, szError, (LPCTSTR)strErr);

		aTable.Close();
		AfxGetSecuritySqlConnection()->m_pContext->ShowMessage((LPCTSTR)e->m_strError);
	}
	
	//----

	if (pInfoOSL->m_dwGrant & (OSL_GRANT_NOT_PROTECTED | OSL_GRANT_PROTECTED_MISSING_GRANT))
	{
		BOOL bNotProtected = (pInfoOSL->m_dwGrant & OSL_GRANT_NOT_PROTECTED) != 0;
		BOOL bMissingGrant = (pInfoOSL->m_dwGrant & OSL_GRANT_PROTECTED_MISSING_GRANT) != 0;
		
		CInfoOSL* pI = NULL;
		//if (pInfoOSL->GetType() != OSLType_Constraint)	
		{
			//oggetto senza grant specifici: cerco il primo parent protetto
			for
				( 
					pI = pInfoOSL->m_pParent
					; 
					pI && ((pI->m_dwGrant & OSL_GRANT_NOT_PROTECTED) != 0)
					; 
					pI = pI->m_pParent 
				); 
		}

		if (pI)	//eredita da un parent con grant specifici
		{
			if (pI->GetType() == OSLType_BatchTemplate || pI->GetType() == OSLType_FinderDoc)
			{
				if (bNotProtected)
					pInfoOSL->SetDefaultGrant();
				else
				{
					pInfoOSL->m_dwGrant = pI->m_dwGrant & OSL_GRANT_EXECUTE;
					pInfoOSL->m_dwInheritMask = 0;
				}
			}
			else
			{
				pInfoOSL->m_dwGrant = pI->m_dwGrant;
				pInfoOSL->m_dwInheritMask = pI->m_dwInheritMask;
			}

			if (bNotProtected)
				pInfoOSL->m_dwGrant = pInfoOSL->m_dwGrant | OSL_GRANT_NOT_PROTECTED;

			if (pInfoOSL->GetType() == OSLType_BodyEdit)
			{
				pInfoOSL->m_dwInheritMask = pInfoOSL->m_dwInheritMask & ~(OSL_GRANT_BE_ADDROW|OSL_GRANT_BE_DELETEROW|OSL_GRANT_BE_SHOWROWVIEW);
				if (bNotProtected)
					pInfoOSL->m_dwGrant = pInfoOSL->m_dwGrant | (OSL_GRANT_BE_ADDROW|OSL_GRANT_BE_DELETEROW|OSL_GRANT_BE_SHOWROWVIEW);
				else
					pInfoOSL->m_dwGrant = pInfoOSL->m_dwGrant & ~(OSL_GRANT_BE_ADDROW|OSL_GRANT_BE_DELETEROW|OSL_GRANT_BE_SHOWROWVIEW);
			}
		}
		else	//grant di default dipendenti dalla protezione esistente sull'oggetto
		{
			pInfoOSL->m_dwGrant = 
							( 
								(pInfoOSL->m_dwGrant & OSL_GRANT_NOT_PROTECTED)
								? 
								(OSL_GRANT_NOT_PROTECTED | OSL_GRANT_ALL_GRANT)
								:
								OSL_GRANT_PROTECTED_MISSING_GRANT
							);
		}
	}
	else if 
		(
			pInfoOSL->m_dwInheritMask && 
			pInfoOSL->m_pParent 
			//&& pInfoOSL->GetType() != OSLType_Constraint
		)
	{
		CInfoOSL* pFirstProtectParent = pInfoOSL->m_pParent;
		for(; pFirstProtectParent && (pFirstProtectParent->m_dwGrant & OSL_GRANT_NOT_PROTECTED); pFirstProtectParent = pFirstProtectParent->m_pParent);
		if (pFirstProtectParent)
		{
			unsigned ui, bit;
			for( ui = 0, bit = 1; ui < OSL_GRANT_COUNT; ui++, bit <<= 1)
			{
				if( (bit & pInfoOSL->m_dwInheritMask) == 0)
					continue;
				if (
					pInfoOSL->GetType() == OSLType_BodyEdit &&
					(bit == OSL_GRANT_BE_ADDROW || bit == OSL_GRANT_BE_DELETEROW || bit == OSL_GRANT_BE_SHOWROWVIEW)
					) continue;

				//devo ereditare
				pInfoOSL->m_dwGrant &= ~bit;
				pInfoOSL->m_dwInheritMask &= ~bit;
				pInfoOSL->m_dwGrant |= (pInfoOSL->m_pParent->m_dwGrant & bit);
			}
		}
	}
	else if (pInfoOSL->GetType() == OSLType_FinderDoc)
	{
		//A. 18622 TODO: sarebbe meglio aggiungere nella tabella MSD_ObjectTypeGrants la riga del grant
		//INSERT INTO MSD_ObjectTypeGrants (TypeId, GrantMask, GrantName) 
		//Values ((select typeid from MSD_ObjectTypes where type = 21), 2, 'Edit')
		pInfoOSL->m_dwGrant |= OSL_GRANT_EDIT;
	}

	pInfoOSL->m_dwGrant |= OSL_GRANT_PROTECTION_FLAG_KNOWN;

	m_mapCacheGrant.SetAt(pInfoOSL, pInfoOSL->m_Namespace.ToString());

	return TRUE;
};

//=============================================================================
class SPInsertObject : public SqlRecordProcedure
{
	DECLARE_DYNCREATE(SPInsertObject) 

public:
	DataLng		f_In_CompanyId;
	DataLng		f_In_UserId;
	DataStr		f_In_ObjectNamespace;
	DataLng		f_In_ObjectType;
	DataStr		f_In_ParentNamespace;
	DataLng		f_In_ParentType;
	DataStr		f_In_Localize;

	DataLng		f_Ret_Result;

public:
	SPInsertObject();

public:
	virtual void	BindRecord	();	

public:
	static LPCTSTR GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//						SPInsertObject
//////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(SPInsertObject, SqlRecordProcedure) 

//-----------------------------------------------------------------------------
SPInsertObject::SPInsertObject()
:
	SqlRecordProcedure(GetStaticName(), AfxGetSecuritySqlConnection())
{
	BindRecord();
}

//-----------------------------------------------------------------------------
LPCTSTR SPInsertObject::GetStaticName() { return _T("MSD_InsertObject"); }

//-----------------------------------------------------------------------------
void SPInsertObject::BindRecord()
{
	BEGIN_BIND_PARAM_DATA()	

		BIND_PARAM(_T("@RETURN_VALUE"),					f_Ret_Result); 

		BIND_PARAM(_T("@par_companyid"),				f_In_CompanyId);
		BIND_PARAM(_T("@par_userid"),					f_In_UserId); 
		BIND_PARAM(_T("@par_objectNamespace"),			f_In_ObjectNamespace); 
		BIND_PARAM(_T("@par_objectType"),				f_In_ObjectType); 
		BIND_PARAM(_T("@par_parentNamespace"),			f_In_ParentNamespace); 
		BIND_PARAM(_T("@par_parentType"),				f_In_ParentType); 
		BIND_PARAM(_T("@par_localize"),					f_In_Localize); 

	END_BIND_PARAM_DATA()	
}

//-----------------------------------------------------------------------------
BOOL CSecurityInterface::InsertObjectIntoOSL (CInfoOSL* pInfoOSL, long* nFlags, LPCTSTR sLocalize)
{ 
	ASSERT(pInfoOSL);
	if (pInfoOSL == NULL)
		return FALSE;

	if (!IsSecurityEnabled())
		return FALSE;	

	BOOL bOk = 
				(
					pInfoOSL->GetType() != OSLType_AddOnApp
				&&
					pInfoOSL->GetType() != OSLType_AddOnMod
				&&
					pInfoOSL->GetType() != OSLType_Wrong
				&&
					pInfoOSL->GetType() != OSLType_Null
				&&
					pInfoOSL->GetType() != OSLType_Skip
				&& 
					pInfoOSL->m_Namespace.IsValid()
				);

	if (!bOk)
	{
		TRACE(_TB("It fails to add object named %s\n"), pInfoOSL->m_Namespace.ToUnparsedString());
		//ASSERT(FALSE);
		return FALSE;
	}
	//----

	CInfoOSL* pI = pInfoOSL; 

	CString strGuid;
	CString strNickName;
	
	//---- Inserimento dell'oggetto 
	SPInsertObject aSpInsertObject;
	SqlTable aTable(&aSpInsertObject, AfxGetSecuritySqlConnection()->GetDefaultSqlSession());

	const CLoginInfos *pInfos = AfxGetLoginInfos();

	aSpInsertObject.f_In_CompanyId			= pInfos->m_nCompanyId;
	aSpInsertObject.f_In_UserId				= pInfos->m_nLoginId;
	aSpInsertObject.f_In_ObjectNamespace	= pI->m_Namespace.ToUnparsedString();
	aSpInsertObject.f_In_ObjectType			= pI->GetType();
	aSpInsertObject.f_In_Localize			= sLocalize;

	if (pI->m_pParent)
	{
		if (pI->m_pParent->GetType() == OSLType_Skip)
		{
			if (pI->m_pParent->m_pParent)
			{
				aSpInsertObject.f_In_ParentNamespace	= pI->m_pParent->m_pParent->m_Namespace.ToUnparsedString();
				aSpInsertObject.f_In_ParentType			= pI->m_pParent->m_pParent->GetType();
			}
			else
			{
				ASSERT(pI->m_pParent->m_pParent);
				TRACE(_TB("It fails to add object named %s\n"), pInfoOSL->m_Namespace.ToUnparsedString());
				//ASSERT(FALSE);
				return FALSE;
			}
		}
		else
		{
			aSpInsertObject.f_In_ParentNamespace	= pI->m_pParent->m_Namespace.ToUnparsedString();
			aSpInsertObject.f_In_ParentType			= pI->m_pParent->GetType();
		}
	}

	long  nRetVal = 0;
	try 
	{
		aTable.Open();
		aTable.Call();

		nRetVal = (long) (aSpInsertObject.f_Ret_Result);

		aTable.Close();
	}
	catch (SqlException* e)
	{
		TCHAR szError[4000];
		CString strErr;
		e->GetErrorString(e->m_nHResult, strErr);
		e->GetErrorMessage(szError, 4000);
		TRACE(_T("%s ; %s; %s\n"), (LPCTSTR)e->m_strError, szError, (LPCTSTR)strErr, (LPCTSTR)aSpInsertObject.f_In_ParentNamespace.GetString());

		aTable.Close();
		if (e->m_nHResult != DB_E_INTEGRITYVIOLATION)
			AfxGetSecuritySqlConnection()->m_pContext->ShowMessage((LPCTSTR)(e->m_strError + _T(" ") + aSpInsertObject.f_In_ParentNamespace.GetString()));
	}
	if (nRetVal)
	{
		CString sErr; sErr.Format(L"%s - %d", (LPCTSTR)aSpInsertObject.f_In_ParentNamespace.GetString(), nRetVal);
		TRACE(L"%s\n", (LPCTSTR)sErr);
	}
	return TRUE;
};

//-----------------------------------------------------------------------------
CPropertyPage* CSecurityInterface::OpenOslAdminDlgProtectDoc(CLocalizablePropertySheet* pS, CBaseDocument* pDoc)
{
	return (CPropertyPage*) new COslDlgDoc (pS, (CAbstractFormDoc*)pDoc);
}

//===========================================================================================
