
#pragma once

#include <afxtempl.h>
#include <initguid.h>

#include <TbGeneric\Array.h>
#include <TbNameSolver\chars.h>
#include <TbNameSolver\TbNamespaces.h>

#include "generic.h"			
#include "const.h"

//includere alla fine degli include del .H
#include "beginh.dex"
//=============================================================================

#define OSL_CAN_DO(pInf,grant) ((pInf) && (pInf)->CanDo(grant))
#define OSL_IS_PROTECTED(pInf) ((pInf) && (pInf)->IsProtected())

//-----------------------------------------------------------------------------

class CCustomConstraintOSL;
class CParsedCtrl;

//flag associati ai permessi
#define OSL_GRANT_NONE				0

#define OSL_GRANT_EXECUTE			1
#define OSL_GRANT_EDIT				2
#define OSL_GRANT_NEW				4
#define OSL_GRANT_DELETE			8

#define OSL_GRANT_BROWSE			16
#define OSL_GRANT_CUSTOMIZEFORM		32
#define OSL_GRANT_EDITQUERY			64

#define OSL_GRANT_BE_DELETEROW		8
#define OSL_GRANT_BE_ADDROW			16
#define OSL_GRANT_BE_SHOWROWVIEW	32

#define OSL_GRANT_IMPORT			128
#define OSL_GRANT_XMLIMPORT			128
#define OSL_GRANT_EXPORT			256
#define OSL_GRANT_XMLEXPORT			256
#define OSL_GRANT_SILENTEXECUTE		512

#define OSL_GRANT_BROWSE_EXTENDED	1024

#define OSL_GRANT_COUNT				11

//maschera di bit tale da comprendere tuti i flag precedenti
#define OSL_GRANT_ALL_GRANT		0xFFFF
#define OSL_GRANT_MAX_GRANT		(OSL_GRANT_ALL_GRANT+1)

//per convertire fra l'OSLDB in cui le costanti sono a base 0 e TB
#define OSL_GRANT_PROTECTION_FLAG_KNOWN		0x10000000
#define OSL_GRANT_PROTECTED_MISSING_GRANT	0x20000000
#define OSL_GRANT_NOT_PROTECTED				0x40000000


//-----------------------------------------------------------------------
/*
	ATTENZIONE:
	
	non modificare mai l'ordine degli elementi in quanto tale valore è memorizzato 
	sia nell'OSLDB (per la maggior parte degli elementi), che nel codice 
	dei componenti di OSL, che tra l'altro utilizzano una versione PRIVATA di questo file
	
	La modifica dell'ordine degli elementi anche non meorizzati nell'OSLDB crea una inutile
	incompatibilità di versione fra le build di TaskBuilder e OSL.
*/

enum TB_EXPORT OSLTypeObject 
		{ 
			OSLType_Dummy = -1,
			OSLType_Null = 0, 

			OSLType_AddOnApp, 
			OSLType_AddOnMod, 

			OSLType_Function			= 3,	
			OSLType_Report				= 4,

			OSLType_Template			= 5,		//Document
			OSLType_SlaveTemplate		= 6,		//Slave/Child Form
			OSLType_BatchTemplate		= 7,		//Batch Document
			OSLType_TabDialog			= 8, 
			//OSLType_Constraint			= 9, 
			OSLType_Table				= 10, 
			OSLType_HotLink				= 11, 
			OSLType_View				= 13,
			OSLType_RowSlaveTemplate	= 14, //Row Slave Form vista di riga
			OSLType_BodyEdit			= 15,
			OSLType_BodyEditColumn		= 16,
			OSLType_Control				= 17,
												
			//OSLType_DBT				= 20,				//UTILIZZATO da XML-Tech, NON SARA' GESTITO DA OSL

			OSLType_FinderDoc			= 21,			//Finder Document
			OSLType_BkgAdmTemplate		= 22,			//Registrazione Automativa, ADM di backGround
	
			WordDocument				= 23,
			ExcelDocument				= 24,
			WordTemplate				= 25,
			ExcelTemplate				= 26,

			ExeShortcut					= 27,
			Executable					= 28,
			Text						= 29,

			OSLType_Tabber				= 30,					
			OSLType_TileManager			= 31,					
			OSLType_Tile				= 32,					
			OSLType_Toolbar				= 33,					
			OSLType_ToolbarButton		= 34,					
			OSLType_EmbeddedView		= 35,
			OSLType_PropertyGrid		= 36,
			OSLType_TilePanel			= 37,
			OSLType_TilePanelTab		= 38,

			OSLType_Skip,				// da saltare in fase di inserimento nel database come vecchia gestione tabber
			OSLType_Wrong				//TERMINATORE enumerativo
		};


///////////////////////////////////////////////////////////////////////////////
//								CInfoOSL
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CInfoOSL
{
	friend class CSecurityInterface;
	friend class CBaseSecurityInterface;
	friend class COSLCacheGrantInfo;

public: 
	CTBNamespace	m_Namespace;
	CInfoOSL*		m_pParent;

protected:
	OSLTypeObject	m_eType;

	DWORD			m_dwGrant;
	DWORD			m_dwInheritMask;

public:
	CInfoOSL () 
		:
		m_eType			(OSLType_Null),
		m_pParent		(NULL),
		m_dwInheritMask (0),
		m_dwGrant		(0)
		{ 
			SetDefaultGrant ();
		}

	CInfoOSL (const CTBNamespace& ns, OSLTypeObject eType) 
		:
		m_eType			(eType),
		m_pParent		(NULL),
		m_dwInheritMask (0),
		m_dwGrant		(0)
		{ 
			m_Namespace.SetNamespace(ns);
			SetDefaultGrant ();
		}

	CInfoOSL (OSLTypeObject eType) 
		:
		m_eType			(eType),
		m_pParent		(NULL),
		m_dwInheritMask (0),
		m_dwGrant		(0)
		{ 
			SetDefaultGrant ();
		}

	CInfoOSL& operator = (const CInfoOSL& source)
		{
			if (&source != this) 
			{
				m_Namespace.SetNamespace(source.m_Namespace);
				m_eType			= source.GetType();
				
				m_dwGrant		= source.m_dwGrant; 
				m_dwInheritMask	= source.m_dwInheritMask;

				m_pParent		= source.m_pParent; 

				SetDefaultGrant();
			}
			return *this;
		}

	OSLTypeObject	GetType() const { return m_eType; }
	CString			FormatType() const { return FormatType(m_eType); }

	BOOL CanDo(DWORD grant); 
	BOOL IsProtected() const { return (m_dwGrant & OSL_GRANT_NOT_PROTECTED) == 0; }

	void SetDefaultGrant();
	void SetType(OSLTypeObject eType) { m_eType = eType; SetDefaultGrant(); }

	// valori costanti che abilitano ogni azione per tipo di oggetto
protected:
	static const DWORD* m_ardwAllGrantForObjectType;

public:
	static DWORD* GetAllGrantForObjectType ();
	static CString FormatType(OSLTypeObject	eType);
};

///////////////////////////////////////////////////////////////////////////////
//								IOSLObjectManager
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT IOSLObjectManager
{
private:
	CInfoOSL	m_InfoOSL;

public:
	IOSLObjectManager() : m_InfoOSL() {}
	IOSLObjectManager(OSLTypeObject eType) : m_InfoOSL(eType) {}
	IOSLObjectManager(const CInfoOSL& aInfoOSL) { m_InfoOSL = aInfoOSL; }

	CInfoOSL*		GetInfoOSL		() { return &m_InfoOSL; }

public:
	virtual BSTR GetObjectNamespace()
	{
		CString sNamespace = GetInfoOSL()->m_Namespace.ToString();
		return ::SysAllocString(sNamespace);
	}
};

/////////////////////////////////////////////////////////////////////////////
//							COslTreeItem
///////////////////////////////////////////////////////////////////////////////
//
class TB_EXPORT COslTreeItem: public CObject
{
	DECLARE_DYNAMIC(COslTreeItem);

public:
	CInfoOSL* m_pInfo;
	LPVOID	m_lpvAuxInfo;
	CString	m_sNickName;
	COslTreeItem* m_pParent;

	COslTreeItem(COslTreeItem* pParent, CInfoOSL* pInfo, const CString& sNickName, LPVOID pAux = NULL) 
		: 
		m_pInfo(pInfo),  
		m_lpvAuxInfo(pAux),
		m_sNickName(sNickName),
		m_pParent (pParent)
		{}
};

//=================================================================================================

TB_EXPORT CString DecodeCOMError(HRESULT hr, const CString& strAuxText);

//=============================================================================
#include "endh.dex"
