
#pragma once

#include <TbNameSolver\Diagnostic.h>
#include <TbGeneric\FontsTable.h>

#include "Parsobj.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//to manage compatibility it stores constructor parameters and settings
//=============================================================================
class CMessagesUIInterface : public CObject
{
	friend class CInternalMessagesViewer;
	friend class CDefaultMessagesViewer;
	friend class CMessages;

private:
	CString		m_strFileName;
	CString		m_strTitle;
	UINT		m_nID;
	UINT		m_nIDDefault;
	CString		m_strOKText;
	CString		m_strFixText;

public:
	void Init (UINT nID, const CString& strTitle, const CString& strFileName)
	{
		m_strTitle			= strTitle;
		m_nID				= nID;
		m_strFileName		= strFileName;
		m_nIDDefault		= 0;
	}

public:
	void SetDefaultButton	(UINT nID)		{ m_nIDDefault	= nID; }
	void SetTextButtonOK	(CString str)	{ m_strOKText	= str; }
	void SetTextButtonFix	(CString str)	{ m_strFixText	= str; }
};

// Wrapper Class To old diagnostic features
//=============================================================================
class TB_EXPORT CMessages : public CDiagnostic
{
	DECLARE_DYNAMIC(CMessages)
	friend class CInternalMessagesViewer;
	friend class CDefaultMessagesViewer;

public:
	//enum redefined using the real one defined in CDiagnostic (for backwards compatibility issues)
	enum MessageType { MSG_ERROR = Error, MSG_WARNING = Warning, MSG_FATAL = FatalError, MSG_HINT = Info };	
	CBaseDocument* m_pDocument = NULL;
private:
	CMessagesUIInterface m_UIInterface;

public:
	CMessages (const CString& strTitle = _T(""), const CString& strFileName = _T(""));
	CMessages (UINT nID, const CString& strFileName = _T(""));

public:
	BOOL IsFatal	()	const;
	BOOL HintFound	()	const;

	void SetDefaultButton	(UINT nID);
	void SetHelpId 			(UINT nID);
	void SetTextButtonOK	(CString str);
	void SetTextButtonFix	(CString str);
		
	void Add (const CString& strMessage, MessageType type = MSG_ERROR, BOOL bOnlyIfNotExist = FALSE);
	void Add (const CStringArray& strMessages, MessageType type = MSG_ERROR);
	void Add (const CString& strMessage, const CString& strDetail, MessageType type = MSG_ERROR);

	virtual BOOL ShowAndLogTo	(const CString& sFileName, BOOL bClearMessages = TRUE);
	virtual BOOL LogToFile		(const CString& sFileName);
	virtual BOOL LoadFromFile	(const CString& sFileName);
	virtual void ToJson			(CJsonSerializer& ser);

	void ToArray (DataObjArray& arValues);
protected:
	virtual void UpdateDataView();//forza l'updatedataview del documento prima della visualizzazione
};

#include "endh.dex"

