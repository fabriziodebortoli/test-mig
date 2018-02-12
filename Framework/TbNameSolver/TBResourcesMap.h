#pragma once

#include "StaticTBResource.h"
#include "TBResourceLocker.h"
#include "TBNamespaces.h"

#include "beginh.dex"

enum TbResourceType {TbResources, TbControls, TbCommands };
class TB_EXPORT CJsonResource
{
public:
	BOOL m_sJsonContext = FALSE;
	CString m_strName;
	CString m_strContext;//contesto da cui sta caricando il json corrente (praticamente, la cartella del file json)
	CString m_strJsonContext; //contesto indicato nel file json (supporto per il pregresso, serve per far funzionare il motore di traduzione)
	DWORD m_dwId = 0;
	bool m_bExclude = false;//for client forms
	//in questo puntatore memorizzo il blocco di stringhe associato a questo particolare json, per non doverlo ricercare ogni volta
	//ogni CWndObjDescription proviene da un file, ma solo il root contiene l'informazione, a meno che l'albero
	//non provenga dalla concatenazione di più file (json di outline o client forms)
	void* m_pTranslationPrivateData = NULL;//usata dallo string loader per mettersi da parte le stringhe (efficienza)
	CString m_sPrivateDataCulture;//culture del blocco di stringhe salvato dentro m_pTranslationPrivateData

	CString m_strFile;//for caching purposes
	CTBNamespace m_OwnerNamespace;//for caching purposes
	CJsonResource()
	{
	}
	CJsonResource(const CJsonResource& r)
	{
		m_strName = r.m_strName;
		m_strContext = r.m_strContext;
		m_strJsonContext = r.m_strJsonContext;
		m_bExclude = r.m_bExclude;
		m_dwId = r.m_dwId;
		m_pTranslationPrivateData = r.m_pTranslationPrivateData;
		m_sPrivateDataCulture = r.m_sPrivateDataCulture;
	}
	CJsonResource(const CString& sFullResource)
	{
		SplitNamespace(sFullResource, m_strName, m_strContext);
	}
	static void SplitNamespace(CString sNamespace, CString& sResourceName, CString& sContext);
	BOOL IsEmpty() { return m_strName.IsEmpty(); }

	void GetInfo(CString& sFile, CTBNamespace& moduleNamespace) const;
	void PopulateFromFile(const CString& sFile);
	CString GetFile();
	CTBNamespace GetOwnerNamespace();
};
typedef CMap < CString, LPCTSTR, DWORD, DWORD > Map;
typedef CMap<DWORD, DWORD, CJsonResource, CJsonResource> ReverseMap;


#define MinTbResource	0x3000		//prima: 1000
#define MaxTbResource	0x6FFF

#define MinTbControl	0x8000		//prima: 2000
#define MaxTbControl	0xDFFF

#define MinTbCommand	0x8000
#define MaxTbCommand	0xDFFF


TB_EXPORT BOOL inline AfxIsStaticControl(int nIDC) { 
	return nIDC == IDC_STATIC || (nIDC >= IDC_STATIC_AREA && nIDC < IDC_STATIC_29);
}

class TB_EXPORT CStringEntry
{
public:
	LPCTSTR lpszString = _T("");
	LPCTSTR lpszFile = _T("");
};

#define _TB_STRING(s) _T(s) //solo per permettere al localizer di trovarla

 
//==================================================================================
class TB_EXPORT CTBResourcesMap : public CTBLockable
{
	friend class CTaskBuilderApp;
private:
	Map m_TbResources;			//mappa delle risorse generate dinamicamente
	ReverseMap m_ReversedTbResources;	//mappa inversa delle risorse generate dinamicamente
	Map m_TbControls;			//mappa delle risorse generate dinamicamente
	ReverseMap m_ReversedTbControls;	//mappa inversa delle risorse generate dinamicamente
//	Map m_TbCommands;			//mappa delle risorse generate dinamicamente
//	ReverseMap m_ReversedTbCommands;	//mappa inversa delle risorse generate dinamicamente
	Map m_TbFixedResources;		//mappa degli id statici
	CMapStringToPtr		m_ForbiddenResources;
	CMap<UINT, UINT, CStringEntry, CStringEntry>	m_StringTable;
	CMapStringToOb		m_DynamicAcceleratorTables;

	unsigned long	m_nLastTbResource;
	unsigned long	m_nLastTbControl;
	unsigned long	m_nLastTbCommand;

public:
	CTBResourcesMap		();
	~CTBResourcesMap	();

public:
	UINT			GetExistingTbResourceID(LPCTSTR sPartialNamespace, const TbResourceType aType); 
	UINT			GetTbResourceID(LPCTSTR sPartialNamespace, const TbResourceType aType, int nCount = 1, LPCTSTR sContext = NULL);
	CJsonResource	DecodeID(const TbResourceType aType, UINT nID);

	void			AddForbiddenID		(TbResourceType aType, UINT nValue);
	UINT			AddString(const CString sId, LPCTSTR lpszString, LPCTSTR lpszFile);
	void			AddAcceleratorTable(const CString& sName, LPACCEL accelTable, const int& nrOfEntries);
	
	CStringEntry	GetString(UINT nID);
	void			GetAcceleratorTable(UINT nID, LPACCEL accelTable, int& nEntries);
	void			GetAcceleratorTable(const CString& sTableName, LPACCEL accelTable, int& nEntries);
	BOOL			IsFixedResource(const CString& sName);	
private:
	static BOOL	IsOutOfRange		(const DWORD& nResourceID,  const TbResourceType aType);
	
	void			InitializeMap	();
	void			AddFixedResource(const TbResourceType aType, const CString& sName, UINT nID);

	UINT	GetNextTbResourceID		(const TbResourceType aType, UINT nValue = 0);
	Map&	GetCollection(const TbResourceType aType);
	ReverseMap&	GetReverseCollection(const TbResourceType aType);
	CString	GetOutOfRangeMessage	(const CString& sPartialNamespace, const DWORD& nResourceID, const TbResourceType aType);

public:
	virtual LPCSTR  GetObjectName() const { return "CTBResourcesMap"; }
};

//==================================================================================
TB_EXPORT CTBResourcesMap*	AFXAPI AfxGetTBResourcesMap (); 

//==================================================================================
#define GET_IDD(Name, Context)		AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbResources, 1, _T(#Context))
#define GET_ID(Name)				AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbCommands)
#define GET_ID_RANGE(Name, Count)	AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbCommands, Count)
#define GET_IDS(Name, szString, szFile)		AfxGetTBResourcesMap()->AddString(_T(#Name), szString, szFile)

#define GET_IDR(Name, Context)		AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbResources, 1, _T(#Context))
#define GET_IDC(Name)				AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbControls)
#define GET_IDC_EX(ParentName, Name)AfxGetTBResourcesMap()->GetTbResourceID(_T(#ParentName "_" #Name), TbControls)
#define GET_IDC_RANGE(Name, Count)	AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbControls, Count)
#define DEFINE_FIXED_CTRL(Name)		AfxGetTBResourcesMap()->AddFixedResource (TbControls, _T(#Name), Name);
#define ADD_FIXED_CTRL(nr)			AddFixedResource (TbControls, _T(#nr), nr);AddFixedResource (TbCommands, _T(#nr), nr);

//da usarsi a cura del programmatore
#define GETIDD(Name)				AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbResources)
#define GETIDC(Name)				AfxGetTBResourcesMap()->GetTbResourceID(_T(#Name), TbControls)

#include "endh.dex"
