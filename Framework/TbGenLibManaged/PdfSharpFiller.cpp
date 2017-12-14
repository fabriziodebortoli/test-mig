#include "StdAfx.h"

#include <TBGeneric\globals.h>
#include <TbNameSolver\FileSystemFunctions.h>

#include "PdfSharpFiller.h"

using namespace System;
using namespace PdfSharp::Pdf;
using namespace PdfSharp::Pdf::IO;
using namespace PdfSharp::Pdf::AcroForms;

//=============================================================================
class CInternalPdfSharpDocumentWrapper
{
private:
	gcroot<PdfDocument^>	m_pDocument;
public:
	CInternalPdfSharpDocumentWrapper(PdfDocument^ document)
	{
		m_pDocument = document;
	}
	~CInternalPdfSharpDocumentWrapper()
	{
		m_pDocument->Close();
	}

	PdfDocument^ GetPdfDocument()	{ return m_pDocument; }
};




//--------------------------------------------------------------------------
bool CPdfSharpFiller::LoadTemplateFile (CString sFilePath)
{
	if (!sFilePath.IsEmpty() && ExistFile(sFilePath))
	{
		PdfDocument^ doc = PdfReader::Open(gcnew String(sFilePath), PdfDocumentOpenMode::Modify);
		m_pDocumentWrapper = new CInternalPdfSharpDocumentWrapper(doc);
		return true;
	}

	return false;
}

//--------------------------------------------------------------------------
void CPdfSharpFiller::SetValue (CString sKey, CString sValue)
{
	if (!sKey.IsEmpty())
	{
		m_mapValues.SetAt(sKey, sValue);
	}
}

//--------------------------------------------------------------------------
bool CPdfSharpFiller::SaveOutputFile		(CString sFilePath)
{
	if (sFilePath.IsEmpty())
	{
		return false;
	}

	PdfDocument^ document = m_pDocumentWrapper->GetPdfDocument();
	POSITION	pos;
	CString		strKey, strValue;
	
	for (pos = m_mapValues.GetStartPosition(); pos != NULL;)
	{
		m_mapValues.GetNextAssoc(pos, strKey, strValue);
		if (!strValue.IsEmpty())
		{
			PdfObject^ o = document->AcroForm->Fields[gcnew String(strKey)];
			if (dynamic_cast<PdfTextField^>(o) == nullptr)
				continue;
			PdfTextField^ currentField = (PdfTextField^)o;
			currentField->Value = gcnew PdfString(gcnew String(strValue));
		}
	}
	
	if (!document->AcroForm->Elements->ContainsKey("/NeedAppearances"))
		document->AcroForm->Elements->Add("/NeedAppearances", gcnew PdfBoolean(true));
	else
		document->AcroForm->Elements["/NeedAppearances"] = gcnew PdfBoolean(true);

	document->Save(gcnew String(sFilePath));
	SAFE_DELETE(m_pDocumentWrapper);
	return true;
}
