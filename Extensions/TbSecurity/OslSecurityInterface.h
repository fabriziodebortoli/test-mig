
#pragma once

#include <TbGenlib\oslbaseinterface.h>	

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class SqlConnection;

///////////////////////////////////////////////////////////////////////////////
//								COSLCacheGrantInfo
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT COSLCacheGrantInfo : protected CMapStringToPtr, public CTBLockable
{
public:
	COSLCacheGrantInfo () {};
	virtual ~COSLCacheGrantInfo() { RemoveAll(); };

	BOOL Lookup (CInfoOSL* pInfoOSL, LPCTSTR pszNs = NULL) const; //cerca l'elemento e se lo trova valorizza i grant
	void SetAt (CInfoOSL* pInfoOSL, LPCTSTR pszNs = NULL);

	void RemoveAll();

	virtual LPCSTR  GetObjectName() const { return "COSLCacheGrantInfo"; }
};

///////////////////////////////////////////////////////////////////////////////
//								CSecurityInterfaceOSL
///////////////////////////////////////////////////////////////////////////////

class TB_EXPORT CSecurityInterface: public CBaseSecurityInterface
{
	DECLARE_DYNAMIC(CSecurityInterface);
public:
	CSecurityInterface ();
	virtual ~CSecurityInterface ();

	virtual BOOL	IsSecurityEnabled () const { return TRUE; } 

	virtual BOOL	IsSuperUser () const { return m_bIsSuperUser; }

	virtual BOOL	GetObjectGrant (CInfoOSL*);

	virtual BOOL	InsertObjectIntoOSL (CInfoOSL*, long*, LPCTSTR);

	virtual void	ResetKnownFlags ();

	virtual CPropertyPage* OpenOslAdminDlgProtectDoc(CLocalizablePropertySheet*, CBaseDocument*);

	BOOL IsConnected() { return m_pSqlConnection != NULL; }

protected:	
	BOOL				m_bIsSuperUser;
	COSLCacheGrantInfo	m_mapCacheGrant;
public:
	SqlConnection*		m_pSqlConnection;
};

//=============================================================================
#include "endh.dex"
