
#pragma once

#include <TBXMLCore\XMLDocObj.h>
#include <TBGES\XMLGesInfo.h>
#include <TbNameSolver\TBNamespaces.h>

//includere alla fine degli include del .H
#include "beginh.dex"

#define DESCRI	_T("Descri")

class CXMLExportCriteria;

// per la descrizione dei profili
//----------------------------------------------------------------
//class CXMLProfileInfo
//----------------------------------------------------------------
class TB_EXPORT CXMLProfileInfo : public CXMLDocObjectInfo
{
	DECLARE_DYNAMIC(CXMLProfileInfo)

private:
	CAbstractFormDoc*	m_pDocument;
	BOOL				m_bInExport;
	BOOL				m_bPreferred;
	BOOL				m_bModified;
	CString				m_strXSLTFileName;

protected:
	CTBNamespace 		m_nsProfile;

public:
	BOOL				m_bUseFieldSel;
	CXMLExportCriteria*	m_pExportCriteria; // per la gestione dei criteri di estrazione in fase
										   // di esportazione
	CXMLExportCriteria*	m_pCurrExportCriteria; // utilizzato per le modifiche fatte dall'utente in fase di esportazione
	BOOL				m_bNewProfile;
	CString				m_strProfileName;
	CString				m_strDocumentPath;
	CString				m_strDocProfilePath;
	CString				m_strExpCriteriaFileName;
	CString				m_strUsrCriteriaFileName;
	CString				m_strFieldInfoFileName;
	CString				m_strHKLInfoFileName;
	CString				m_strProfileXRefFileName;
	CString				m_strSchemaFileName;

	

public:
	// se il secondo parametro è vuoto allora carichiamo i dati dati dalla descrizione del
	// documento. Serve per creare nuovi profili a partire dalla descrizione del documento
	CXMLProfileInfo		(const CTBNamespace&, LPCTSTR = NULL, const CString& = _T(""));
	CXMLProfileInfo		(CAbstractFormDoc*, LPCTSTR = NULL, const CString& = _T(""));
	CXMLProfileInfo		(const CXMLProfileInfo&);
	~CXMLProfileInfo	();

private:
	BOOL LoadProfileXRefFiles	();
	BOOL ParseXRefFile			(const CString&);
	BOOL SaveXRefFile			();	
	BOOL LoadExportCriteriaFile	(CAutoExpressionMng* = NULL);
	BOOL LoadUsrCriteriaFile	();
	BOOL SaveUsrCriteriaFile	();
	BOOL LoadFieldInfoFile		();
	BOOL SaveFieldInfoFile		();
	BOOL LoadHotKeyLinkInfoFile ();
	BOOL SaveHotKeyLinkInfoFile ();

protected:
	void Assign			(const CTBNamespace&, LPCTSTR);
	void Assign			(const CXMLProfileInfo&);

	void SetDocument	(CAbstractFormDoc*);

public:
	static CString GetProfileNameFromPath(const CString& strProfilePath);

public:
	virtual	BOOL LoadAllFiles	(CAutoExpressionMng* = NULL);
	virtual void SetAllFilesName();

public:
	CString			GetName				()	const	{ return m_strProfileName; }
	void			SetName				(const CString& strProfileName);
	CTBNamespace	GetProfileNamespace	()	const	{ return m_nsProfile;}
	void			SetProfileNamespace	(const CString& strProfileName);

	BOOL			SaveProfile			(const CString& strPath, const CString& strNewName);

	CXMLExportCriteria*		GetXMLExportCriteria		()	const { return m_pExportCriteria; }
	CXMLExportCriteria*		GetCurrentXMLExportCriteria	()	const { return m_pCurrExportCriteria; }
 
	void					SetXMLExportCriteria		(const CXMLExportCriteria*);
	void					SetXMLExportCriteria		(const CXMLProfileInfo&);
	void					SetCurrentXMLExportCriteria	(CXMLExportCriteria*);

public:
	BOOL	SaveAsItIs				();
	BOOL	RemoveProfilePath		() const;
	BOOL	RemoveOptProfileFiles	() const;
	BOOL	RenameProfile			(const CString& strNewName);
	void	ModifySmartDocumentSchema(const CString& strOldSchema);
	
	BOOL	IsPredefined		() const;
	BOOL	IsPreferred			() const	{ return m_bPreferred; }
	void	SetPreferred		(BOOL = TRUE);	
	BOOL	IsModified			() const	{ return m_bModified; }
	void	SetModified			(BOOL = TRUE);	
	void	SetXRefExportFlag	(BOOL = TRUE);
	BOOL	IsValidXRef			(CXMLXRefInfo*);

	CString GetSmartNamespaceURI() const;

	//transform document
	BOOL				IsTransformProfile() const		{ return m_pHeaderInfo->m_bTransform; }

	CString GetXSLTFileName() const;

	//Impr: 6393
	BOOL	CanRunOnlyBusinessObject();

public: //operator
	CXMLProfileInfo&	operator = (const CXMLProfileInfo&);
	BOOL				operator ==	(const CXMLProfileInfo&) const;
	BOOL				operator !=	(const CXMLProfileInfo&) const;

// diagnostics
#ifdef _DEBUG
public:	
	void Dump(CDumpContext&) const;
	void AssertValid() const;
#endif
};

#include "endh.dex"

