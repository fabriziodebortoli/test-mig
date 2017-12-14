#pragma once
//#include "stdafx.h"

#include "beginh.dex"

class CFunctionDescription;
//=============================================================================
class TB_EXPORT CExpandTemplateMng
{
private:
	enum ETemplateType { TPL_EMPTY, TPL_RTF, TPL_PDF, TPL_MSWORD, TPL_MSWORDX, TPL_MSEXCEL, TPL_MSEXCELX, TPL_ODT, TPL_ODS };	// , TPL_HTML, TPL_TXT, TPL_XML, TPL_SXW

	CString					m_sTemplate;
	CString					m_sTarget;
	CFunctionDescription*	m_pFD;
	ETemplateType			m_Type;
	CTBNamespace::NSObjectType m_nsType;
	CString					m_sError;

public:
	CExpandTemplateMng(CFunctionDescription*);

	virtual ~CExpandTemplateMng(void) {}

	BOOL	SetTemplate	(const CString& sTemplate);
	BOOL	SetTarget	(const CString& sTarget);

	CString	GetError	() { return m_sError; }

	BOOL	Expand		();
	void	Clear		(BOOL bAll);
};

//=============================================================================

#include "endh.dex"