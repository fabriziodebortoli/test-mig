#include "StdAfx.h"

#include <TBGeneric\globals.h>
#include <TBGeneric\FunctionCall.h>
#include <TbGeneric\EnumsTable.h>

#include <TbNameSolver\FileSystemFunctions.h>

#include "StaticFunctions.h"

#include "ExpandTemplateMng.h"

using namespace System;
using namespace Microarea::TaskBuilderNet::UI::DocumentMerge;

//-----------------------------------------------------------------------------
CExpandTemplateMng::CExpandTemplateMng(CFunctionDescription* pFD)
	:
	m_pFD	(pFD),
	m_Type	(TPL_EMPTY),
	m_nsType(CTBNamespace::NOT_VALID)
{
	ASSERT_VALID(pFD);
}

//-----------------------------------------------------------------------------
BOOL CExpandTemplateMng::SetTemplate (const CString& sTemplate)
{
	int idx = sTemplate.ReverseFind('.');
	if (idx < 0) 
	{
		m_sError = _TB("Template Document required name extension: ") + sTemplate;
		return FALSE;
	}
	
	CString sExt = sTemplate.Mid(idx+1);
	sExt.MakeLower();
	if (sExt.Compare(L"rtf") == 0)
	{
		m_nsType = CTBNamespace::RTF;
		m_Type = TPL_RTF;
	}
	else if (sExt.Compare(L"pdf") == 0)
	{
		m_nsType = CTBNamespace::PDF;
		m_Type = TPL_PDF;
	}

	else if (sExt.Compare(L"doc") == 0)
	{
		m_nsType = CTBNamespace::WORDDOCUMENT;
		m_Type = TPL_MSWORD;
	}
	else if (sExt.Compare(L"dot") == 0)
	{
		m_nsType = CTBNamespace::WORDTEMPLATE;
		m_Type = TPL_MSWORD;
	}
	else if (sExt.Compare(L"docx") == 0)
	{
		m_nsType = CTBNamespace::WORDDOCUMENT;
		m_Type = TPL_MSWORDX;
	}
	else if (sExt.Compare(L"dotx") == 0)
	{
		m_nsType = CTBNamespace::WORDTEMPLATE;
		m_Type = TPL_MSWORDX;
	}

	else if (sExt.Compare(L"xls") == 0)
	{
		m_nsType = CTBNamespace::EXCELDOCUMENT;
		m_Type = TPL_MSEXCEL;
	}
	else if (sExt.Compare(L"xlt") == 0)
	{
		m_nsType = CTBNamespace::EXCELTEMPLATE;
		m_Type = TPL_MSEXCEL;
	}
	else if (sExt.Compare(L"xlsx") == 0)
	{
		m_nsType = CTBNamespace::EXCELDOCUMENT;
		m_Type = TPL_MSEXCELX;
	}
	else if (sExt.Compare(L"xltx") == 0)
	{
		m_nsType = CTBNamespace::EXCELTEMPLATE;
		m_Type = TPL_MSEXCELX;
	}

	else if (sExt.Compare(L"odt") == 0 )
	{
		m_nsType = CTBNamespace::ODT;
		m_Type = TPL_ODT;
	}
	else if (sExt.Compare(L"ods") == 0)
	{
		m_nsType = CTBNamespace::ODS;
		m_Type = TPL_ODS;
	}
	//else if (sExt.Compare(L"sxw") == 0 || sExt.Compare(L"stw") == 0)
	//{
	//	m_nsType = CTBNamespace::ODF;
	//	m_Type = TPL_SXW;
	//}

	else
	{
		m_sError = _TB("Template Document has unsupported type: ") + sExt;
		return FALSE;
	}

	m_sTemplate = AfxGetPathFinder()->FromNs2Path(sTemplate, m_nsType, CTBNamespace::FILE);
	if (m_sTemplate.IsEmpty())
	{
		m_sError = _TB("Template Document not found: ") + sTemplate;
		return FALSE;
	}
	if (!::ExistFile(m_sTemplate))
	{
		m_sError = _TB("Template Document not exists: ") + m_sTemplate;
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
BOOL CExpandTemplateMng::SetTarget (const CString& sTarget)
{
	if (!m_sError.IsEmpty() || m_sTemplate.IsEmpty())
	{
		m_sError = _TB("You have to set template name before");
		return FALSE;
	}

	m_sTarget = AfxGetPathFinder()->FromNs2Path(sTarget, m_nsType, CTBNamespace::FILE);
	if (m_sTarget.IsEmpty())
	{
		m_sError = _TB("Target Document has wrong name: ") + sTarget;
		return FALSE;
	}

	return TRUE;
}

//-----------------------------------------------------------------------------
void CExpandTemplateMng::Clear (BOOL bAll)
{
	m_sTemplate.Empty();
	m_sTarget.Empty();
	m_sError.Empty();

	if (bAll)
		m_pFD = NULL;
}


//-----------------------------------------------------------------------------
BOOL CExpandTemplateMng::Expand()
{
	if (m_sTemplate.IsEmpty() || m_sTarget.IsEmpty() || !m_pFD || m_Type == TPL_EMPTY || !m_sError.IsEmpty())
		return FALSE;
	ASSERT_VALID(m_pFD);

	Collections::Generic::Dictionary<String^, Object^>^ dict = gcnew Collections::Generic::Dictionary<String^, Object^>(StringComparer::OrdinalIgnoreCase);

	for (int i = 0; i < m_pFD->GetParameters().GetSize(); i++)
	{
		CDataObjDescription* pDescr = m_pFD->GetParamDescription(i);
		DataObj* pObj = pDescr->GetValue();

		Object^ o = ConverDataObj(pObj);

		dict->Add(gcnew String(pDescr->GetName()), o);
	}

	System::String^ s = gcnew System::String(m_sTemplate);
	System::String^ d = gcnew System::String(m_sTarget);
	DocumentMerge^ dm = nullptr;;

	switch (m_Type)
	{
		case TPL_PDF:
		{
			dm = gcnew DocumentMerge (s, d, TemplateType::TPL_PDF, dict);
			break;
		}
		case TPL_RTF:
		{
			dm = gcnew DocumentMerge (s, d, TemplateType::TPL_RTF, dict);
			break;
		}
		case TPL_MSEXCEL:
		case TPL_MSEXCELX:
		{
			dm = gcnew DocumentMerge (s, d, TemplateType::TPL_MSEXCEL, dict);
			break;
		}
		case TPL_MSWORD:
		{
			dm = gcnew DocumentMerge (s, d, TemplateType::TPL_MSWORD, dict);
			break;
		}
		case TPL_MSWORDX:
		{
			dm = gcnew DocumentMerge (s, d, TemplateType::TPL_MSWORDX, dict);
			break;
		}
		case TPL_ODT:
		{
			dm = gcnew DocumentMerge (s, d, TemplateType::TPL_ODT, dict);
			break;
		}
		default:
		{
			m_sError = _TB("Template Document type unsupported: ") + m_sTemplate;
			return FALSE;
		}

		if (dm != nullptr)
			dm->ReplacePlaceHolders();
	}

	return TRUE;
}
//-----------------------------------------------------------------------------


//=============================================================================
