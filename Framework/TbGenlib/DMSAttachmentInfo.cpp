#include "stdafx.h" 

#include <TbNameSolver\FileSystemFunctions.h>

#include "DMSAttachmentInfo.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


///////////////////////////////////////////////////////////////////////////////
//								CTypedBarcode declaration
///////////////////////////////////////////////////////////////////////////////
//
//---------------------------------------------------------------------------
CTypedBarcode::CTypedBarcode()
{
	Clear();	
}

//---------------------------------------------------------------------------
CTypedBarcode::CTypedBarcode(CString barcodeValue, CString barcodeType)
{ 
	m_strBarcodeValue = barcodeValue; 
	m_strBarcodeType = barcodeType; 
}

//---------------------------------------------------------------------------
void CTypedBarcode::Clear()
{
	m_strBarcodeValue = _T("");
	m_strBarcodeType = _T("CODE39");
}

//---------------------------------------------------------------------------
CTypedBarcode&	CTypedBarcode::operator = (const CTypedBarcode& barcode)
{
	m_strBarcodeValue = barcode.m_strBarcodeValue;
	m_strBarcodeType = barcode.m_strBarcodeType;

	return *this;
}

///////////////////////////////////////////////////////////////////////////////
//								CAttachmentInfo declaration
///////////////////////////////////////////////////////////////////////////////
//
IMPLEMENT_DYNCREATE(CAttachmentInfo, CObject)

//-----------------------------------------------------------------------------
CAttachmentInfo::CAttachmentInfo()
	:
	b_LateBinding(false),
	m_attachmentID(-1),
	m_ArchivedDocId(-1),
	m_CollectionId(-1),
	m_ErpDocumentId(-1),
	m_IsWoormReport(FALSE),
	m_IsMainDoc(FALSE),
	m_IsForMail(FALSE)
{
	m_ArchivedDate.SetFullDate();
	m_AttachedDate.SetFullDate();
	m_ModifiedDate.SetFullDate();
}

//-----------------------------------------------------------------------------
void CAttachmentInfo::Clear()
{
	b_LateBinding = false;
	m_attachmentID = -1;
	m_ArchivedDocId = -1;
	m_CollectionId = -1;
	m_ErpDocumentId = -1;
	m_Size.Clear();
	m_Name.Clear();
	m_Description.Clear();
	m_OriginalPath.Clear();
	m_ExtensionType.Clear();
	m_ArchivedDate.Clear();
	m_AttachedDate.Clear();
	m_ModifiedDate.Clear();
	m_ModifiedBy.Clear();
	m_CreatedBy.Clear();
	m_IsAPapery.Clear();
	m_ERPDocNamespace.Clear();
	m_ERPPrimaryKeyValue.Clear();
	m_Barcode.Clear();
	m_StorageFile.Clear();
	m_TemporaryPathFile.Clear();
	m_FreeTag.Clear();
	m_CurrentCheckOutWorker.Clear();
	m_IsWoormReport.Clear();
	m_IsWoormReport.Clear();
	m_IsMainDoc.Clear();
	m_IsForMail.Clear();

}


//-----------------------------------------------------------------------------
void CAttachmentInfo::Assign(const CAttachmentInfo& attInfo)
{
	b_LateBinding = attInfo.b_LateBinding;
	m_attachmentID = attInfo.m_attachmentID;
	m_ArchivedDocId = attInfo.m_ArchivedDocId;
	m_CollectionId = attInfo.m_CollectionId;
	m_ErpDocumentId = attInfo.m_ErpDocumentId;
	m_Size = attInfo.m_Size;
	m_Name = attInfo.m_Name;
	m_Description = attInfo.m_Description;
	m_OriginalPath = attInfo.m_OriginalPath;
	m_ExtensionType = attInfo.m_ExtensionType;
	m_ArchivedDate = attInfo.m_ArchivedDate;
	m_AttachedDate = attInfo.m_AttachedDate;
	m_ModifiedDate = attInfo.m_ModifiedDate;
	m_ModifiedBy = attInfo.m_ModifiedBy;
	m_CreatedBy = attInfo.m_CreatedBy;
	m_IsAPapery = attInfo.m_IsAPapery;
	m_ERPDocNamespace = attInfo.m_ERPDocNamespace;
	m_ERPPrimaryKeyValue = attInfo.m_ERPPrimaryKeyValue;
	m_Barcode = attInfo.m_Barcode;
	m_StorageFile = attInfo.m_StorageFile;
	m_TemporaryPathFile = attInfo.m_TemporaryPathFile;
	m_FreeTag = attInfo.m_FreeTag;
	m_CurrentCheckOutWorker = attInfo.m_CurrentCheckOutWorker;
	m_IsWoormReport = attInfo.m_IsWoormReport;
	m_IsMainDoc = attInfo.m_IsMainDoc;
	m_IsForMail = attInfo.m_IsForMail;
}

//-----------------------------------------------------------------------------
CAttachmentInfo& CAttachmentInfo::operator= (const CAttachmentInfo& attInfo)
{
	Assign(attInfo);	
	return *this;
}


///////////////////////////////////////////////////////////////////////////////
//								CAttachmentsArray declaration
///////////////////////////////////////////////////////////////////////////////
CAttachmentsArray::~CAttachmentsArray()
{
	RemoveAll();
}

//--------------------------------------------------------------------------------
CAttachmentInfo* CAttachmentsArray::GetAttachmentByID(int attachmentID) const
{
	CAttachmentInfo* pAttachInfo = NULL;
	for (int j = 0; j < GetCount(); j++)
	{
		pAttachInfo = GetAt(j);
		if (pAttachInfo && pAttachInfo->m_attachmentID == attachmentID)
			return pAttachInfo;
	}

	return NULL;
}