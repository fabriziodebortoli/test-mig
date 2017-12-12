#include "stdafx.h" 

#include <TbGeneric\Array.h>
#include <TbGenlib\BarcodeEnums.h>
#include <ExtensionsImages\CommonImages.h>

#include "TBDMSEnums.h"
#include "CDDMS.h"
#include "TbRepositoryManager.h"
#include "CommonObjects.h"

#include "UIAttachment.hjson"

using namespace System;
using namespace System::Data;
using namespace System::Xml;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::TBPicComponents;

///////////////////////////////////////////////////////////////////////////////
//					DMSCollectionInfo declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DMSCollectionInfo, CObject)

//-----------------------------------------------------------------------------
DMSCollectionInfo::DMSCollectionInfo()
	:
	m_IsStandard(TRUE)
{}

///////////////////////////////////////////////////////////////////////////////
//					DMSCollectionList declaration
///////////////////////////////////////////////////////////////////////////////
DMSCollectionInfo* DMSCollectionList::GetCollectionInfoByDocNamespace(const CString& docNamespace)
{
	DMSCollectionInfo* pCollection = NULL;
	for (int j = 0; j < GetCount(); j++)
	{
		pCollection = GetAt(j);
		if (pCollection && pCollection->m_DocNamespace.CompareNoCase(docNamespace) == 0)
			return pCollection;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
DMSCollectionInfo* DMSCollectionList::GetCollectionInfoByCollectionID(int collectionID)
{
	DMSCollectionInfo* pCollectionnfo = NULL;
	for (int j = 0; j < GetCount(); j++)
	{
		pCollectionnfo = GetAt(j);
		if (pCollectionnfo && pCollectionnfo->m_CollectionID == collectionID)
			return pCollectionnfo;
	}

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//						DMSAttachmentInfo declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(DMSAttachmentInfo, CAttachmentInfo)

//-----------------------------------------------------------------------------
DMSAttachmentInfo::DMSAttachmentInfo()
	:
	attachmentInfo		(nullptr)	
{	
	m_BarcodeValue.SetUpperCase();
}

//-----------------------------------------------------------------------------
DMSAttachmentInfo::~DMSAttachmentInfo()
{
}

//-----------------------------------------------------------------------------
void DMSAttachmentInfo::Clear()
{
	CAttachmentInfo::Clear();	
	m_BarcodeValue.Clear();
	m_BarcodeType.Clear();
	m_StorageFile.Clear();
	m_TemporaryPathFile.Clear();
	m_FreeTag.Clear();
	m_CurrentCheckOutWorker.Clear();
	m_IsWoormReport.Clear();
	attachmentInfo = nullptr;
}

//-----------------------------------------------------------------------------
BOOL DMSAttachmentInfo::IsValid() const
{
	return m_ArchivedDocId > -1 || m_attachmentID > -1 || m_IsAPapery;
}

//-----------------------------------------------------------------------------
void DMSAttachmentInfo::AddCategoryField(const CString& name, const CString& value)
{
	if ((AttachmentInfo^)attachmentInfo !=  nullptr)
		attachmentInfo->AddCategoryField(gcnew String(name), gcnew String(value));
}

//-----------------------------------------------------------------------------
void DMSAttachmentInfo::AddBindingField(const CString& name)
{
	if ((AttachmentInfo^)attachmentInfo != nullptr)
		attachmentInfo->AddBindingField(gcnew String(name));
}

//-----------------------------------------------------------------------------
void DMSAttachmentInfo::ModifyBookmarks(DBTBookmarks* pDBTBookmarks)
{
	if ((AttachmentInfo^)attachmentInfo == nullptr || attachmentInfo->BookmarksDT == nullptr)
		return;

	//verifico quelle cancellate
	//for (int i = attachmentInfo->BookmarksDT->Rows->Count - 1; i >= 0; i--)
	for each(DataRow^ row in attachmentInfo->BookmarksDT->Rows)
	{
		//DataRow^ row = attachmentInfo->BookmarksDT->Rows[i];
		if ((FieldGroup)row[CommonStrings::FieldGroup] != FieldGroup::Binding && (FieldGroup)row[CommonStrings::FieldGroup] != FieldGroup::Category)
			continue;

		if (!pDBTBookmarks->GetRecordByFieldName(row[CommonStrings::Name]->ToString()))
			row->Delete();
	}

	//considero ciascuna riga di tipo Binding e Category del DBT e verifico le modifiche effettuate rispetto al BookmarkDataTable
	for (int i = 0; i < pDBTBookmarks->GetSize(); i++)
	{
		VBookmark* pBookmarkRec = (VBookmark*)pDBTBookmarks->GetRecords()->GetAt(i);
		if (pBookmarkRec->l_FieldName.IsEmpty() || (pBookmarkRec->l_GroupType != E_BOOKMARK_BINDING && pBookmarkRec->l_GroupType != E_BOOKMARK_CATEGORY))
			continue;
		
		DataRow^ row = attachmentInfo->BookmarksDT->FindByName(gcnew String(pBookmarkRec->l_FieldName.Str()));
		
		//esiste
		if (row)
		{
			//nel caso di binding verifico il valore presente nel BookmarkDataTable con quello del campo del master record (non serve xchè ci pensa il EASynchro a tenere i valori aggiornati)
			/*if (pBookmarkRec->l_FieldGroup == 1)
			{
				FieldData^ fieldData = (FieldData^)row[CommonStrings::FieldData];
				DataObj* pBookDataObj = ((DataObj*)(fieldData->DataObj->DataObjPtr.ToInt64()));
				DataObj* pRecDataObj = GetServerDoc()->m_pDBTMaster->GetRecord()->GetDataObjFromColumnName(pBookmarkRec->l_FieldName);
				if (pBookDataObj && pRecDataObj && *pBookDataObj != *pRecDataObj)
					fieldData->AssignValue(MDataObj::Create((System::IntPtr)pRecDataObj));
			}*/

			//nel caso di categoria verifico il formattedValue
			if (pBookmarkRec->l_GroupType == E_BOOKMARK_CATEGORY)
			{
				//verifico se il valore della categoria è stato modificato
				if (pBookmarkRec->l_FormattedValue.CompareNoCase(CString(row[CommonStrings::FormattedValue]->ToString())) != 0)
				{	
					//sono differenti assegno il nuovo valore al dt
					FieldData^ fieldData = (FieldData^)row[CommonStrings::FieldData];
					fieldData->DataValue = gcnew String(pBookmarkRec->l_FormattedValue.Str());
					// imposto la riga come modificata
					row->SetModified();
				}
			}
		}
		else
		{
			//non esiste
			//aggiungo il nuovo bookmark
			if (pBookmarkRec->l_GroupType == E_BOOKMARK_CATEGORY) //categoria
				AddCategoryField(pBookmarkRec->l_FieldName.Str(), pBookmarkRec->l_FormattedValue.Str());

			if (pBookmarkRec->l_GroupType == E_BOOKMARK_BINDING) //binding
				AddBindingField(pBookmarkRec->l_FieldName);
		}
	}
}

//-----------------------------------------------------------------------------
DMSAttachmentInfo& DMSAttachmentInfo::operator= (const DMSAttachmentInfo& attInfo)
{
	Assign(*(CAttachmentInfo*)&attInfo);
	m_BarcodeValue = attInfo.m_BarcodeValue;
	m_BarcodeType = attInfo.m_BarcodeType;
	attachmentInfo = attInfo.attachmentInfo;
	return *this;
}

//-----------------------------------------------------------------------------
BOOL DMSAttachmentInfo::IsOwnCheckOut() const
{
	return ((AttachmentInfo^)attachmentInfo != nullptr && attachmentInfo->ModifierID > -1 && attachmentInfo->ModifierID == AfxGetWorkerId());
}

//-----------------------------------------------------------------------------
BOOL DMSAttachmentInfo::CanCheckOut() const
{
	return ((AttachmentInfo^)attachmentInfo != nullptr && attachmentInfo->ModifierID == -1);
}

//-----------------------------------------------------------------------------
DMSAttachmentInfo* CreateDMSAttachmentInfo(AttachmentInfo^ attachment, BOOL bAllInformation /*= TRUE*/)
{
	DMSAttachmentInfo* attInfo = new DMSAttachmentInfo();

	DataDate archivedDate(attachment->ArchivedDate.Day, attachment->ArchivedDate.Month, attachment->ArchivedDate.Year, attachment->ArchivedDate.Hour, attachment->ArchivedDate.Minute, attachment->ArchivedDate.Second);
	DataDate attachedDate(attachment->AttachedDate.Day, attachment->AttachedDate.Month, attachment->AttachedDate.Year, attachment->AttachedDate.Hour, attachment->AttachedDate.Minute, attachment->AttachedDate.Second);
	DataDate modifiedDate(attachment->ModifiedDate.Day, attachment->ModifiedDate.Month, attachment->ModifiedDate.Year, attachment->ModifiedDate.Hour, attachment->ModifiedDate.Minute, attachment->ModifiedDate.Second);

	attInfo->m_attachmentID = DataLng(attachment->AttachmentId);
	attInfo->m_ArchivedDocId = DataLng(attachment->ArchivedDocId);
	attInfo->m_Name = DataStr((CString)attachment->Name);
	attInfo->m_Description = DataStr((CString)attachment->Description);
	attInfo->m_ExtensionType = DataStr((CString)attachment->ExtensionType);
	attInfo->m_IsAPapery = DataBool(attachment->IsAPapery);
	attInfo->m_IsWoormReport = DataBool(attachment->IsWoormReport);
	attInfo->m_IsMainDoc = DataBool(attachment->IsMainDoc);
	attInfo->m_IsForMail = DataBool(attachment->IsForMail);

	if (attInfo->m_IsAPapery)
	{
		attInfo->m_BarcodeValue = DataStr((CString)attachment->TBarcode->Value);
		attInfo->m_BarcodeType = DataStr((CString)attachment->TBarcode->TypeDescription);	
		attInfo->attachmentInfo = attachment;
	}

	if (bAllInformation && !attInfo->m_IsAPapery)
	{
		attInfo->m_CollectionId = DataLng(attachment->CollectionID);
		attInfo->m_ErpDocumentId = DataLng(attachment->ErpDocumentID);
		attInfo->m_OriginalPath = DataStr((CString)attachment->OriginalPath);
		attInfo->m_Size = DataLng((long)attachment->KBSize);
		attInfo->m_ArchivedDate = archivedDate;
		attInfo->m_AttachedDate = attachedDate;
		attInfo->m_ModifiedDate = modifiedDate;
		attInfo->m_ModifiedBy = DataStr((CString)attachment->ModifiedBy);
		attInfo->m_CreatedBy = DataStr((CString)attachment->CreatedBy);
		attInfo->m_ERPDocNamespace = DataStr((CString)attachment->ERPDocNamespace);
		attInfo->m_ERPPrimaryKeyValue = DataStr((CString)attachment->ERPPrimaryKeyValue);
		attInfo->m_StorageFile = DataStr((CString)attachment->StorageFile);
		if (attachment->ModifierID > -1)
		{
			CWorker* pWorker = AfxGetWorkersTable()->GetWorker(attachment->ModifierID);
			attInfo->m_CurrentCheckOutWorker = (pWorker) ? pWorker->GetName() + _T(" ") + pWorker->GetLastName() : _T("");
		}
		attInfo->m_BarcodeValue = DataStr((CString)attachment->TBarcode->Value);
		attInfo->m_BarcodeType = DataStr((CString)attachment->TBarcode->TypeDescription);
		attInfo->m_FreeTag.Assign(CString(attachment->Tags));
		attInfo->attachmentInfo = attachment;
	}

	return attInfo;
}

///////////////////////////////////////////////////////////////////////////////
//					DMSAttachmentsList declaration
///////////////////////////////////////////////////////////////////////////////
//--------------------------------------------------------------------------------
int DMSAttachmentsList::GetAttachmentIdx(DMSAttachmentInfo* att)
{
	DMSAttachmentInfo* pAttachInfo = NULL;
	for (int j = 0; j < GetCount(); j++)
	{
		pAttachInfo = GetAt(j);
		if (
				pAttachInfo &&
				(
					(
						(pAttachInfo->m_IsAPapery && att->m_IsAPapery) && 
						pAttachInfo->m_BarcodeValue.CompareNoCase(att->m_BarcodeValue) == 0  && 
						pAttachInfo->m_BarcodeType.CompareNoCase(att->m_BarcodeType) == 0
					) 
					||
					(
						(
							!pAttachInfo->m_IsAPapery && !att->m_IsAPapery &&
							pAttachInfo->m_attachmentID == att->m_attachmentID
						)
					)
				)
			)
			return j;
	}
	return -1;
}

///////////////////////////////////////////////////////////////////////////////
//						CDMSCategory declaration
///////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
CDMSCategory::CDMSCategory(CString strName)
	:
	m_Name		(strName), 
	m_bDisabled	(FALSE),
	m_bInUse	(FALSE)
{
	m_pDataStr = (DataStr*)DataObj::DataObjCreate(DataType::String);
}

//------------------------------------------------------------------------------
CDMSCategory::~CDMSCategory()
{
	if (m_pDataStr)
		delete m_pDataStr;
}

//------------------------------------------------------------------------------
void CDMSCategory::AddValue(CString strValue, BOOL isDefault)
{
	VCategoryValues* pCatRec = new VCategoryValues();
	pCatRec->l_Name = m_Name;
	pCatRec->l_Value.Assign(strValue);
	pCatRec->l_IsDefault = isDefault;
	if (pCatRec->l_IsDefault)
	{
		m_DefaultValue = strValue;
		m_pDataStr->Assign(m_DefaultValue);
	}
	m_arCategoryValues.Add(pCatRec);
}

//------------------------------------------------------------------------------
void CDMSCategory::Clear()
{
	m_Name.Clear();
	m_Description.Clear();
	m_arCategoryValues.RemoveAll();
	m_DefaultValue.Clear();
	//m_Color;
	m_bDisabled = FALSE;
	m_bInUse = FALSE;
	if (m_pDataStr)
		m_pDataStr->Clear();
}

//------------------------------------------------------------------------------
CDMSCategory& CDMSCategory::operator =(const CDMSCategory& dmsCat)
{
	m_Name = dmsCat.m_Name;
	m_Description = dmsCat.m_Description;
	m_DefaultValue = dmsCat.m_DefaultValue;
	m_Color = dmsCat.m_Color;
	m_bDisabled = dmsCat.m_bDisabled;
	m_bInUse = dmsCat.m_bInUse;
	
	m_arCategoryValues.RemoveAll();
	for (int i = 0; i < dmsCat.m_arCategoryValues.GetCount(); i++)
	{
		VCategoryValues* pRec = new VCategoryValues();
		*pRec = *dmsCat.GetValueAt(i);
		m_arCategoryValues.Add(pRec);
	}

	if (!m_pDataStr)
		m_pDataStr = (DataStr*)DataObj::DataObjCreate(DataType::String);

	*m_pDataStr = *dmsCat.m_pDataStr;

	return *this;
}

///////////////////////////////////////////////////////////////////////////////
//						CSearchField declaration
///////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
CSearchField::CSearchField(SqlRecordItem* pRecItem)
	:
	m_pRecItem	(pRecItem),
	m_pCategory	(NULL),
	m_bSelected	(FALSE)
{
	m_FieldName = (m_pRecItem) ? m_pRecItem->GetColumnName() : _T("");
	m_pFieldValues = new CStringArray();
}

//------------------------------------------------------------------------------
CSearchField::CSearchField(CDMSCategory* pCategory)
	:
	m_pRecItem	(NULL),
	m_pCategory	(pCategory),
	m_bSelected	(FALSE)
{
	m_FieldName = (pCategory) ? pCategory->m_Name : _T("");
	m_pFieldValues = new CStringArray();
	if (pCategory)
	{
		for (int i = 0; i < pCategory->m_arCategoryValues.GetSize(); i++)
		{
			VCategoryValues* pCatValue = (VCategoryValues*)pCategory->m_arCategoryValues.GetAt(i);
			if (pCatValue)
				m_pFieldValues->Add(pCatValue->l_Value.Str());
		}
	}
}

//------------------------------------------------------------------------------
CSearchField::~CSearchField()
{
	if (m_pFieldValues)
		delete m_pFieldValues;
}

//------------------------------------------------------------------------------
BOOL CSearchField::IsCategory() const
{ 
	return m_pCategory != NULL;
}

//------------------------------------------------------------------------------
CString CSearchField::GetDescription() const
{
	return IsCategory() ? m_pCategory->m_Name : m_pRecItem->GetColumnInfo()->GetColumnTitle();
}

//------------------------------------------------------------------------------
DataObj* CSearchField::GetDataObj() const
{
	return IsCategory() ? m_pCategory->m_pDataStr : m_pRecItem->GetDataObj();
}

//------------------------------------------------------------------------------
CString	CSearchField::GetFormattedValue() const
{
	return IsCategory() ? m_pCategory->m_DefaultValue : m_pRecItem->GetDataObj()->FormatData();
}

//------------------------------------------------------------------------------
CStringArray* CSearchField::GetFieldValues()
{
	if (!IsCategory())
	{
		m_pFieldValues->RemoveAll();
		m_pFieldValues->Add(m_pRecItem->GetDataObj()->FormatData());
	}
	return m_pFieldValues;
}

///////////////////////////////////////////////////////////////////////////////
//						CSearchFieldList declaration
///////////////////////////////////////////////////////////////////////////////
//------------------------------------------------------------------------------
CSearchFieldList::CSearchFieldList(SqlRecord* pRecord, CDMSCategories* pCategories)
	:
	m_pCategories(pCategories)
{
	if (pRecord)
	{
		for (int i = 0; i < pRecord->GetSize(); i++)
		{
			SqlRecordItem* pRecItem = pRecord->GetAt(i);
			
			// skippo le colonne mandatory, pk e virtuali
			if (
				pRecItem->GetColumnInfo()->GetColumnName().CompareNoCase(GUID_COL_NAME) == 0 ||
				pRecItem->GetColumnInfo()->GetColumnName().CompareNoCase(CREATED_COL_NAME) == 0 ||
				pRecItem->GetColumnInfo()->GetColumnName().CompareNoCase(MODIFIED_COL_NAME) == 0 ||
				pRecItem->GetColumnInfo()->GetColumnName().CompareNoCase(CREATED_ID_COL_NAME) == 0 ||
				pRecItem->GetColumnInfo()->GetColumnName().CompareNoCase(MODIFIED_ID_COL_NAME) == 0 ||
				pRecItem->IsSpecial() || 
				pRecItem->GetColumnInfo()->m_bVirtual
				)
				continue;

			Add(new CSearchField(pRecItem));
		}
	}

	if (m_pCategories)
	{
		for (int i = 0; i < m_pCategories->GetSize(); i++)
			Add(new CSearchField(m_pCategories->GetAt(i)));
	}
}

//------------------------------------------------------------------------------
CSearchFieldList::~CSearchFieldList()
{
	if (m_pCategories)
		delete m_pCategories;
}

//------------------------------------------------------------------------------
int CSearchFieldList::Add(CSearchField* field)
{ 
	CSearchField* pField = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		pField = GetAt(i);
		if (pField->m_FieldName.CompareNoCase(field->m_FieldName) == 0)
		{
			ASSERT(FALSE);
			return i;
		}		
	}
	return Array::Add((CObject*)field);
}

//------------------------------------------------------------------------------
CSearchField* CSearchFieldList::GetSearchFieldByName(const CString fieldName) const
{
	CSearchField* pField = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		pField = GetAt(i);
		if (pField->m_FieldName.CompareNoCase(fieldName) == 0)
			return pField;
	}

	return NULL;
}

//------------------------------------------------------------------------------
void CSearchFieldList::SelectAll(BOOL bSelect)
{
	CSearchField* pField = NULL;
	for (int i = 0; i < GetSize(); i++)
		GetAt(i)->m_bSelected = bSelect;
}

//------------------------------------------------------------------------------
void CSearchFieldList::SetSelected(const CString fieldName, BOOL bSelect)
{
	CSearchField* pField = NULL;

	for (int i = 0; i < GetSize(); i++)
	{
		pField = (CSearchField*)GetAt(i);
		if (pField->m_FieldName.CompareNoCase(fieldName) == 0)
		{
			pField->m_bSelected = bSelect;
			break;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////
//	class VBookmark implementation: rappresenta un singolo bookmark di ricerca
////////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VBookmark, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VBookmark::VBookmark(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VBookmark")),
	l_GroupType		(E_BOOKMARK_TYPE_DEFAULT)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VBookmark::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_KEY_STR	(_NS_FLD("VFieldName"),			l_FieldName,		80);
		LOCAL_STR		(_NS_FLD("VFieldDescription"),	l_Description,		256);
		LOCAL_STR		(_NS_FLD("VFormattedValue"),	l_FormattedValue,	512);
		LOCAL_DATA		(_NS_FLD("VFieldGroup"),		l_GroupType);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
void VBookmark::AssignMGroupType(int groupType)
{
	switch (groupType)
	{
	case 0:
		l_GroupType = E_BOOKMARK_KEY; break;
	case 1:
		l_GroupType = E_BOOKMARK_BINDING; break;
	case 2:
		l_GroupType = E_BOOKMARK_CATEGORY; break;
	case 3:
		l_GroupType = E_BOOKMARK_EXTERNAL; break;
	case 4:
		l_GroupType = E_BOOKMARK_SOS_SPECIAL; break;
	case 5:
		l_GroupType = E_BOOKMARK_VARIABLE; break;
	default:
		l_GroupType = E_BOOKMARK_BINDING;
	}
}

//-----------------------------------------------------------------------------
int VBookmark::GetMGroupType() const
{
	if (l_GroupType == E_BOOKMARK_KEY) return 0;
	if (l_GroupType == E_BOOKMARK_BINDING) return 1;
	if (l_GroupType == E_BOOKMARK_CATEGORY) return 2;
	if (l_GroupType == E_BOOKMARK_EXTERNAL) return 3;
	if (l_GroupType == E_BOOKMARK_SOS_SPECIAL) return 4;
	if (l_GroupType == E_BOOKMARK_VARIABLE) return 5;

	return 1;
}

//-----------------------------------------------------------------------------
BOOL VBookmark::IsNotEditableField() const
{
	return l_GroupType == E_BOOKMARK_KEY || l_GroupType == E_BOOKMARK_EXTERNAL ||l_GroupType == E_BOOKMARK_SOS_SPECIAL || l_GroupType == E_BOOKMARK_VARIABLE;
}

//-----------------------------------------------------------------------------
LPCTSTR VBookmark::GetStaticName() { return _NS_TBL("VBookmark"); }

///////////////////////////////////////////////////////////////////////////////
//		class DBTBookmarks implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTBookmarks, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTBookmarks::DBTBookmarks
	(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("Bookmarks"), ALLOW_EMPTY_BODY, FALSE),
	m_pSearchFields(NULL)
{
	m_pSearchFields = new CSearchFieldList((pDocument && pDocument->m_pDBTMaster) ? pDocument->m_pDBTMaster->GetRecord() : NULL, AfxGetTbRepositoryManager()->GetCategories());
}

//-----------------------------------------------------------------------------	
DBTBookmarks::~DBTBookmarks()
{
	SAFE_DELETE(m_pSearchFields);
}

//-----------------------------------------------------------------------------
VBookmark* DBTBookmarks::GetRecordByFieldName(const CString& strFieldName) const
{
	VBookmark* pBookmarkRec = NULL;
	for (int i = 0; i < GetSize(); i++)
	{
		pBookmarkRec = (VBookmark*)GetRecords()->GetAt(i);
		if (pBookmarkRec->l_FieldName.IsEqual(DataStr(strFieldName)))
			return pBookmarkRec;
	}

	return NULL;	
}

//-----------------------------------------------------------------------------
void DBTBookmarks::LoadFromBookmarkDT(DMSAttachmentInfo* pAttachmentInfo)	
{
	RemoveAll(TRUE);
	m_pSearchFields->SelectAll(FALSE);
	BookmarksDataTable^ bookmarkDT = ((AttachmentInfo^)pAttachmentInfo->attachmentInfo != nullptr) ? pAttachmentInfo->attachmentInfo->BookmarksDT : nullptr;
	if (bookmarkDT == nullptr)
		return;
	
	for each (DataRow^ bookmark in bookmarkDT->Rows)
	{
		VBookmark* pNewRec = (VBookmark*)AddRecord();
		pNewRec->l_FieldName.Assign(CString(bookmark[CommonStrings::Name]->ToString()));
		pNewRec->l_Description.Assign(CString(bookmark[CommonStrings::FieldDescription]->ToString()));
		pNewRec->l_FormattedValue.Assign(CString(bookmark[CommonStrings::FormattedValue]->ToString()));
		pNewRec->AssignMGroupType((int)bookmark[CommonStrings::FieldGroup]);	
		m_pSearchFields->SetSelected(pNewRec->l_FieldName, TRUE);
		pNewRec->SetReadOnly();
	}
}

//-----------------------------------------------------------------------------	
DataObj* DBTBookmarks::OnCheckPrimaryKey(int /*nRow*/, SqlRecord* /*pRec*/)
{
	return NULL;
}

//-----------------------------------------------------------------------------
DataObj* DBTBookmarks::GetDuplicateKeyPos(SqlRecord* pRec)
{
	return &(((VBookmark*)pRec)->l_FieldName);
}

//-----------------------------------------------------------------------------
CString DBTBookmarks::GetDuplicateKeyMsg(SqlRecord* pRec)
{
	ASSERT(pRec->IsKindOf(RUNTIME_CLASS(VBookmark)));
	VBookmark* pBookmarkRec = (VBookmark*)pRec;

	return cwsprintf(_TB("The {0-%s} field already exists:\r\n choose a different field."), (LPCTSTR)pBookmarkRec->l_FieldName.Str());
}

// viene chiamata in modo esplicito quando si va in stato di Edit dell'allegato corrente
// ii bookmark già inseriti non si possono modificare ma solo cancellare 
//-----------------------------------------------------------------------------
void DBTBookmarks::OnDisableControlsForEdit()
{
	for (int i = 0; i <= GetUpperBound(); i++)
	{
		VBookmark* pBookRecord = GetBookmarkRow(i);
		pBookRecord->SetReadOnly(TRUE);
		if (pBookRecord->l_GroupType == E_BOOKMARK_CATEGORY)
			pBookRecord->l_FormattedValue.SetReadOnly(FALSE);
	}
}

//-----------------------------------------------------------------------------
BOOL DBTBookmarks::OnBeforeDeleteRow(int nRow)
{
	VBookmark* pRec = GetBookmarkRow(nRow);

	// non posso eliminare le righe di tipo Key, External, SosSpecial, Variable
	if (pRec->IsNotEditableField())
		return FALSE;

	m_pSearchFields->SetSelected(pRec->l_FieldName, FALSE);
	return TRUE;
}

///////////////////////////////////////////////////////////////////////////////
//						VSettings declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VSettings, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VSettings::VSettings(BOOL bCallInit  /* = TRUE */)
:
	SqlVirtualRecord(_T("VSettings")),
	l_DeletingAttachmentAction		(E_DELETE_ATTACH_ACTION_DEFAULT),
	l_DuplicateActionForDoc			(E_DUPLICATE_ACTION_DEFAULT),
	l_DuplicateActionForBatch		(E_DUPLICATE_KEEP_BOTH),
	l_EnableBookmarkEmptyValues		(TRUE),
	l_DpiQualityImage				(300),
	l_EnableToAttachFromRepository	(TRUE),
	l_ShowOnlyMyArchivedDocs		(TRUE),
	l_EnableBarcode					(FALSE),
	l_AutomaticBarcodeDetection		(TRUE),
	l_BarcodeDetectionAction		(E_BARCODE_DETECT_ACTION_DEFAULT),
	l_BCActionForDocument			(E_DUPLICATE_ACTION_DEFAULT), 
	l_BCActionForBatch				(E_DUPLICATE_KEEP_BOTH),
	l_BarcodeType					(E_BARCODE_TYPE_ALFA39),
	l_BarcodePrefix					(_T("EA")),
	l_PrintBarcodeInReport			(FALSE),
	l_EnableFTS						(FALSE),
	l_FTSNotConsiderPdF				(FALSE),
	l_EnableSOS						(FALSE),
	l_MaxElementsInEnvelope			(200),
	l_DisableAttachFromReport		(FALSE),
	l_StorageToFileSystem			(FALSE),
	l_MaxDocNumber					(10)
{
	l_BarcodePrefix.SetUpperCase();

	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VSettings::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_KEY	(_NS_FLD("VWorkerID"),						l_WorkerID);
		LOCAL_KEY	(_NS_FLD("VSettingType"),					l_SettingType);
		LOCAL_DATA	(_NS_FLD("VDeletingAttachmentAction"),		l_DeletingAttachmentAction);
		LOCAL_DATA	(_NS_FLD("VDuplicateActionForDoc"),			l_DuplicateActionForDoc);
		LOCAL_DATA	(_NS_FLD("VDuplicateActionForBatch"),		l_DuplicateActionForBatch);
		LOCAL_DATA	(_NS_FLD("VEnableBookmarkEmptyValues"),		l_EnableBookmarkEmptyValues);
		LOCAL_DATA	(_NS_FLD("VDpiQualityImage"),				l_DpiQualityImage);
		LOCAL_DATA	(_NS_FLD("VEnableToAttachFromRepository"),	l_EnableToAttachFromRepository);
		LOCAL_DATA	(_NS_FLD("VShowOnlyMyArchivedDocs"),		l_ShowOnlyMyArchivedDocs);
		LOCAL_DATA	(_NS_FLD("VEnableBarcode"),					l_EnableBarcode);
		LOCAL_DATA	(_NS_FLD("VAutomaticBarcodeDetection"),		l_AutomaticBarcodeDetection);
		LOCAL_DATA	(_NS_FLD("VBarcodeDetectionAction"),		l_BarcodeDetectionAction);
		LOCAL_DATA	(_NS_FLD("VBCActionForDocument"),			l_BCActionForDocument);
		LOCAL_DATA	(_NS_FLD("VBCActionForBatch"),				l_BCActionForBatch);
		LOCAL_DATA	(_NS_FLD("VBarcodeType"),					l_BarcodeType);
		LOCAL_STR	(_NS_FLD("VBarcodePrefix"),					l_BarcodePrefix,		5);
		LOCAL_DATA	(_NS_FLD("VPrintBarcodeInReport"),			l_PrintBarcodeInReport);
		LOCAL_DATA	(_NS_FLD("VEnableFTS"),						l_EnableFTS);
		LOCAL_DATA	(_NS_FLD("VSNotConsiderPdF"),				l_FTSNotConsiderPdF);
		LOCAL_DATA	(_NS_FLD("VEnableSOS"),						l_EnableSOS);
		LOCAL_DATA	(_NS_FLD("VMaxElementsInEnvelope"),			l_MaxElementsInEnvelope);
		LOCAL_STR	(_NS_FLD("VExcludedExtensions"),			l_ExcludedExtensions,	100);
		LOCAL_DATA	(_NS_FLD("VDisableAttachFromReport"),		l_DisableAttachFromReport);
		LOCAL_DATA	(_NS_FLD("VStorageToFileSystem"),			l_StorageToFileSystem);
		LOCAL_STR	(_NS_FLD("VStorageFolderPath"),				l_StorageFolderPath,	256);
		LOCAL_DATA	(_NS_FLD("VMaxDocNumber"),					l_MaxDocNumber);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VSettings::GetStaticName() { return _NS_TBL("VSettings"); }

///////////////////////////////////////////////////////////////////////////////
//						VExtensionMaxSize declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VExtensionMaxSize, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VExtensionMaxSize::VExtensionMaxSize(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VExtensionMaxSize")),
	l_MaxSize(1)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VExtensionMaxSize::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_KEY_STR	(_NS_FLD("VExtension"), l_Extension,	10);
		LOCAL_DATA		(_NS_FLD("VMaxSize"),	l_MaxSize);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VExtensionMaxSize::GetStaticName() { return _NS_TBL("VExtensionMaxSize"); }

//////////////////////////////////////////////////////////////////////////////
//					CDMSSettings implementation								//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDMSSettings, CObject)

//-----------------------------------------------------------------------------
CDMSSettings::CDMSSettings()
	:
	m_pRecSettings			(NULL),
	m_pExtensionMaxSizeArray(NULL)
{
	m_pRecSettings = new VSettings();
	m_pExtensionMaxSizeArray = new RecordArray();
}

//-----------------------------------------------------------------------------
CDMSSettings::~CDMSSettings()
{
	if (m_pRecSettings)
		delete m_pRecSettings;

	if (m_pExtensionMaxSizeArray)
	{
		m_pExtensionMaxSizeArray->RemoveAll();
		delete m_pExtensionMaxSizeArray;
	}
}

//------------------------------------------------------------------------------
CDMSSettings& CDMSSettings::operator =(const CDMSSettings& settings)
{
	*m_pRecSettings	= *settings.m_pRecSettings;

	if (m_pExtensionMaxSizeArray)
		m_pExtensionMaxSizeArray->RemoveAll();

	VExtensionMaxSize* pNewRec;
	for (int i = 0; i < settings.m_pExtensionMaxSizeArray->GetSize(); i++)
	{
		pNewRec = new VExtensionMaxSize();
		*pNewRec = *(VExtensionMaxSize*)settings.m_pExtensionMaxSizeArray->GetAt(i);
		m_pExtensionMaxSizeArray->Add(pNewRec);
	}
	return *this;
}

///////////////////////////////////////////////////////////////////////////////
//						VCategoryValues declaration
//					SqlVirtualRecord per mappare la categoria
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(VCategoryValues, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VCategoryValues::VCategoryValues(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VCategoryValues")),
	l_IsDefault	(FALSE),
	l_Checked	(FALSE)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VCategoryValues::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_KEY_STR(_NS_FLD("VName"),		l_Name, 80);
		LOCAL_KEY_STR(_NS_FLD("VValue"),	l_Value, 512);
		LOCAL_DATA(_NS_FLD("VIsDefault"),	l_IsDefault); 
		LOCAL_DATA(_NS_FLD("VChecked"),		l_Checked);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VCategoryValues::GetStaticName() { return _NS_TBL("VCategoryValues"); }

//////////////////////////////////////////////////////////////////////////////
//					CDMSCategoryTreeNode implementation					//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDMSCategoryNode, CObject)

//-----------------------------------------------------------------------------
CDMSCategoryNode::CDMSCategoryNode()
	:
	m_pDMSCategory	(NULL),
	m_pCatValue		(NULL)
{
}

//-----------------------------------------------------------------------------
CDMSCategoryNode::CDMSCategoryNode(CDMSCategory*pDMSCategory, const CString& strParentKey)
	:
	m_pDMSCategory	(pDMSCategory),
	m_pCatValue		(NULL),
	m_strParentKey	(strParentKey)
{
	m_strKey = m_pDMSCategory->m_Name;
	m_strText = m_pDMSCategory->m_Name;
}

//-----------------------------------------------------------------------------
CDMSCategoryNode::CDMSCategoryNode(VCategoryValues* pCatValue, const CString& strParentKey)
	:
	m_pDMSCategory	(NULL),
	m_pCatValue		(pCatValue),
	m_strParentKey	(strParentKey)
{
	m_strKey = m_pCatValue->l_Name + m_pCatValue->l_Value;
	m_strText = m_pCatValue->l_Value;
}

static TCHAR szRootKey			[] = _T("RootKey");

//////////////////////////////////////////////////////////////////////////////
//					CDMSCategoriesTreeViewAdv implementation				//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDMSCategoriesTreeViewAdv, CTreeViewAdvCtrl)

//-----------------------------------------------------------------------------
CDMSCategoriesTreeViewAdv::CDMSCategoriesTreeViewAdv()
	:
	CTreeViewAdvCtrl	(),
	m_pAllNodes			(NULL),
	m_bTreeViewLoaded	(FALSE),
	m_bUseCheckBox		(FALSE)
{
}

//-----------------------------------------------------------------------------
CDMSCategoriesTreeViewAdv::~CDMSCategoriesTreeViewAdv()
{
	if (m_pAllNodes)
		delete m_pAllNodes;
}

//-----------------------------------------------------------------------------
void CDMSCategoriesTreeViewAdv::OnInitControl()
{
	CTreeViewAdvCtrl::OnInitControl();
	SetNodeStateIcon(TRUE);
	
	AddImage(szGlyphCategory, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphCategory), _T("")));
	AddImage(szGlyphCategoryDisabled, AfxGetPathFinder()->GetFileNameFromNamespace(ExtensionsGlyph(szGlyphCategoryDisabled), _T("")));

	if (m_bUseCheckBox)
		SetCheckBoxControls(TRUE);
	AddControls();
	SetAllowDrop(FALSE);
	SetViewContextMenu(TRUE);
}

//-----------------------------------------------------------------------------
void CDMSCategoriesTreeViewAdv::SetAllNodes(::Array* pAllNodes)
{
	if (m_pAllNodes)
		delete m_pAllNodes;
	m_pAllNodes = pAllNodes;
	Load();
}

//-----------------------------------------------------------------------------
CDMSCategoryNode* CDMSCategoriesTreeViewAdv::GetNodeByKey(const CString& strKey)
{
	for (int i = 0; i <= m_pAllNodes->GetUpperBound(); i++)
	{
		CDMSCategoryNode* pNode = (CDMSCategoryNode*)m_pAllNodes->GetAt(i);
		if (pNode->m_strKey.CompareNoCase(strKey) == 0)
			return pNode;
	}

	return NULL;
}

//-----------------------------------------------------------------------------
void CDMSCategoriesTreeViewAdv::Load()
{ 
	ClearTree();

	if (m_pAllNodes->GetSize() == 0)
		return;

	m_bTreeViewLoaded = FALSE;
	
	AddNode(_TB("DMS Categories"), szRootKey, szGlyphCategory);
	for (int i = 0; i < m_pAllNodes->GetSize(); i++)
	{
		CDMSCategoryNode* pNode = (CDMSCategoryNode*)m_pAllNodes->GetAt(i);
		SetNodeAsSelected(pNode->m_strParentKey);
		AddNode(pNode->m_strText, pNode->m_strKey, (pNode->m_pCatValue) ? m_bUseCheckBox : FALSE);
		if (pNode->m_pDMSCategory)
			SetImage(pNode->m_strKey, pNode->m_pDMSCategory->m_bDisabled ? szGlyphCategoryDisabled : szGlyphCategory);
		if (pNode->m_pCatValue && pNode->m_pCatValue->l_IsDefault)
			SetStyleForNode(pNode->m_strKey, TRUE);
	}

	SetNodeAsSelected(szRootKey);
	CollapseAllFromSelectedNode();
	ExpandLevels(1);
	SetFocus();
	m_bTreeViewLoaded = TRUE;
}

//-----------------------------------------------------------------------------
CDMSCategoryNode* CDMSCategoriesTreeViewAdv::GetSelectedTreeNode()
{
	CString aKey = GetSelectedNodeKey();
	if (aKey.IsEmpty())
		return NULL;

	for (int i = 0; i <= m_pAllNodes->GetUpperBound(); i++)
	{
		CDMSCategoryNode* pNode = (CDMSCategoryNode*)m_pAllNodes->GetAt(i);
		if (pNode && (pNode->m_strKey.CompareNoCase(aKey) == 0))
			return pNode;
	}

	return NULL;
}

///////////////////////////////////////////////////////////////////////////////
//						VArchivedDocument declaration
//	SqlVirtualRecord per mappare un record nel DBT dei risultati della ricerca
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(VArchivedDocument, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VArchivedDocument::VArchivedDocument(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VArchivedDocument")),
	l_IsAttachment		(FALSE),
	l_IsWoormReport		(FALSE),
	l_IsSelected		(FALSE),
	m_pAttachmentInfo	(NULL),
	m_pAttachmentLinks	(NULL)
{
	l_CreationDate.SetFullDate();
	l_ModifiedDate.SetFullDate();

	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
VArchivedDocument::~VArchivedDocument()
{
	SAFE_DELETE(m_pAttachmentInfo);
	SAFE_DELETE(m_pAttachmentLinks);
}

//-----------------------------------------------------------------------------
void VArchivedDocument::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_DATA	(_NS_FLD("VIsSelected"),		l_IsSelected);
		LOCAL_DATA	(_NS_FLD("VArchivedDocId"),		l_ArchivedDocId);
		LOCAL_STR	(_NS_FLD("VIsAttachmentBmp"),	l_IsAttachmentBmp,	32);
		LOCAL_DATA	(_NS_FLD("VIsAttachment"),		l_IsAttachment);
		LOCAL_STR	(_NS_FLD("VIsWoormReportBmp"),  l_IsWoormReportBmp, 32);
		LOCAL_DATA	(_NS_FLD("VIsWoormReport"),		l_IsWoormReport);
		LOCAL_STR	(_NS_FLD("VName"),				l_Name,				200);
		LOCAL_STR	(_NS_FLD("VDescription"),		l_Description,		350); 
		LOCAL_STR	(_NS_FLD("VCheckOutWorkerBmp"), l_CheckOutWorkerBmp,32);
		LOCAL_STR	(_NS_FLD("VCheckOutWorker"),	l_CheckOutWorker,	10);
		LOCAL_STR	(_NS_FLD("VWorker"),			l_Worker,			50);
		LOCAL_DATA	(_NS_FLD("VCreationDate"),		l_CreationDate);
		LOCAL_DATA	(_NS_FLD("VModifiedDate"),		l_ModifiedDate);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VArchivedDocument::GetStaticName() { return _NS_TBL("VArchivedDocument"); }

//-----------------------------------------------------------------------------
DMSAttachmentInfo* VArchivedDocument::GetAttachmentInfo()
{
	SAFE_DELETE(m_pAttachmentInfo);
	if (l_ArchivedDocId > 0)
		m_pAttachmentInfo = AfxGetTbRepositoryManager()->GetAttachmentInfoFromArchivedDocId(l_ArchivedDocId);
	return m_pAttachmentInfo;
}

//-----------------------------------------------------------------------------
::Array* VArchivedDocument::GetAttachmentLinks()
{
	SAFE_DELETE(m_pAttachmentLinks);
	if (l_ArchivedDocId > 0)
		m_pAttachmentLinks =  AfxGetTbRepositoryManager()->GetERPDocumentAttachment(l_ArchivedDocId);

	return m_pAttachmentLinks;
}

///////////////////////////////////////////////////////////////////////////////
//						VAttachmentLink declaration
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(VAttachmentLink, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VAttachmentLink::VAttachmentLink(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VAttachmentLink"))
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
VAttachmentLink::~VAttachmentLink()
{
}

//-----------------------------------------------------------------------------
void VAttachmentLink::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_STR	(_NS_LFLD("VImage"),				l_Image,				32);
		LOCAL_DATA	(_NS_FLD("VArchivedDocId"),			l_ArchivedDocId);
		LOCAL_DATA	(_NS_FLD("VAttachmentId"),			l_AttachmentId);
		LOCAL_STR	(_NS_FLD("VTBPrimaryKey"),			l_TBPrimaryKey,			50);
		LOCAL_STR	(_NS_FLD("VTBDocNamespace"),		l_TBDocNamespace,		50);
		LOCAL_STR	(_NS_FLD("VDocumentDescription"),	l_DocumentDescription,	200);
		LOCAL_STR	(_NS_FLD("VDocKeyDescription"),		l_DocKeyDescription,	100);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VAttachmentLink::GetStaticName() { return _NS_TBL("VAttachmentLink"); }

//////////////////////////////////////////////////////////////////////////////
//						    CBookmarkTypeItemSource
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBookmarkTypeItemSource, CItemSource)

//-----------------------------------------------------------------------------
CBookmarkTypeItemSource::CBookmarkTypeItemSource()
	:
	CItemSource		(),
	m_bShowBinding	(TRUE)
{}

//-----------------------------------------------------------------------------
BOOL CBookmarkTypeItemSource::IsValidItem(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (
		((DataEnum&)aDataObj) == E_BOOKMARK_KEY			||
		((DataEnum&)aDataObj) == E_BOOKMARK_EXTERNAL	||
		((DataEnum&)aDataObj) == E_BOOKMARK_SOS_SPECIAL ||
		((DataEnum&)aDataObj) == E_BOOKMARK_VARIABLE
		)
		return FALSE;

	if (((DataEnum&)aDataObj) == E_BOOKMARK_BINDING)
		return m_bShowBinding;

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//					   CSearchFieldsItemSource
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSearchFieldsItemSource, CItemSource)

//-----------------------------------------------------------------------------
CSearchFieldsItemSource::CSearchFieldsItemSource()
	:
	CItemSource		(),
	m_bCategory		(FALSE),
	m_pSearchFields	(NULL)
{
}

//-----------------------------------------------------------------------------
void CSearchFieldsItemSource::SetSearchFieldList(CSearchFieldList* pSearchFields)
{
	m_pSearchFields = pSearchFields;
}

//-----------------------------------------------------------------------------
void CSearchFieldsItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pSearchFields || m_pSearchFields->GetSize() < 1)
		return;

	CSearchField* pField = NULL;
	for (int i = 0; i < m_pSearchFields->GetSize(); i++)
	{
		pField = m_pSearchFields->GetAt(i);
		if (
			((pField->IsCategory() && m_bCategory) || (!pField->IsCategory() && !m_bCategory)) &&
			!pField->m_bSelected
			)
		{
			values.Add(new DataStr(pField->m_FieldName));
			descriptions.Add(pField->GetDescription());
		}
	}
}

//////////////////////////////////////////////////////////////////////////////
//						CSearchFieldValuesItemSource
// contiene l'elenco dei valori corrispondenti al bookmark selezionato
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CSearchFieldValuesItemSource, CItemSource)

//-----------------------------------------------------------------------------
CSearchFieldValuesItemSource::CSearchFieldValuesItemSource()
	:
	CItemSource			(),
	m_pSearchFieldValues(NULL)
{
}

//-----------------------------------------------------------------------------
void CSearchFieldValuesItemSource::SetSearchFieldValuesList(CStringArray* pSearchFieldValues)
{
	m_pSearchFieldValues = pSearchFieldValues;
}

//-----------------------------------------------------------------------------
void CSearchFieldValuesItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pSearchFieldValues)
		return;

	CString strValue;
	for (int i = 0; i < m_pSearchFieldValues->GetSize(); i++)
	{
		strValue = m_pSearchFieldValues->GetAt(i);
		values.Add(new DataStr(strValue));
		descriptions.Add(strValue);
	}
}

//////////////////////////////////////////////////////////////////////////////
//						    CTBDMSViewerCtrl
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CTBDMSViewerCtrl, CTBPicViewerAdvCtrl)

//-----------------------------------------------------------------------------
void CTBDMSViewerCtrl::SetValue(const DataObj& aValue)
{
	CString strValue = ((DataStr&)aValue).GetString();
	if (strValue.IsEmpty() || !ExistFile(strValue))
	{
		CTBPicViewerAdvWrapper::CloseDocument();
		return;
	}

	CTBPicViewerAdvWrapper::DisplayFromFile(strValue);
}

//////////////////////////////////////////////////////////////////////////////
//					    CTBDMSBarcodeViewerCtrl
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CTBDMSBarcodeViewerCtrl, CTBPicViewerAdvCtrl)

//-----------------------------------------------------------------------------
void CTBDMSBarcodeViewerCtrl::SetValue(const DataObj& aValue)
{
	CString barcodeValue = ((DataStr&)aValue).GetString();
	if (barcodeValue.IsEmpty())
	{
		CTBPicViewerAdvCtrl::CloseDocument();
		return;
	}

	TBPicImaging^ tbPicImaging = gcnew TBPicImaging();
	
	int imageId = tbPicImaging->GetBarcodeImageId(gcnew String(m_BarcodeType.Str()), 1, -1, gcnew String(barcodeValue), false);
	
	if (imageId > 0)
	{
		CTBPicViewerAdvWrapper::E_TBPICTURESTATUS status = CTBPicViewerAdvCtrl::DisplayFromGdPictureImage(imageId);
		if (status == CTBPicViewerAdvWrapper::E_TBPICTURESTATUS::OK)
			return;
	}
	else
		CTBPicViewerAdvCtrl::CloseDocument(); // se non riesco ad estrapolare un imageid valido non visualizzo nulla
}

/////////////////////////////////////////////////////////////////////////////
//				class CBookmarksBodyEdit implementation
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CBookmarksBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------
CBookmarksBodyEdit::CBookmarksBodyEdit()
{
}

//-----------------------------------------------------------------------------	
void CBookmarksBodyEdit::Customize()
{
	__super::Customize();
}

//-----------------------------------------------------------------------------	
void CBookmarksBodyEdit::SetOnlyCategoryField()
{
	if (m_pSearchFieldsItemSource)
		m_pSearchFieldsItemSource->SetFilterCategory();
}

//-----------------------------------------------------------------------------
BOOL CBookmarksBodyEdit::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID == 0 || hWndCtrl == NULL)
		return CBodyEdit::OnCommand(wParam, lParam);

	if (nCode == EN_VALUE_CHANGED)
	{
		if (nID == IDC_ATT_BE_BOOKMARK_TYPE)
			OnBookmarkTypeChanged();

		if (nID == IDC_ATT_BE_BOOKMARK_DESCRI)
			OnSearchFieldChanged();
	}

	return CBodyEdit::OnCommand(wParam, lParam);
}

//-----------------------------------------------------------------------------
void CBookmarksBodyEdit::OnBookmarkTypeChanged()
{
	VBookmark* pRec = GetCurrentBookmarkRow();
	if (!pRec) return;

	// originale
	if (pRec->l_GroupType == E_BOOKMARK_BINDING)
		m_pSearchFieldsItemSource->SetFilterBinding();
	if (pRec->l_GroupType == E_BOOKMARK_CATEGORY)
		m_pSearchFieldsItemSource->SetFilterCategory();

	pRec->l_FieldName.Clear();
	pRec->l_Description.Clear();
	pRec->l_FormattedValue.Clear();
}

//-----------------------------------------------------------------------------
void CBookmarksBodyEdit::OnSearchFieldChanged()
{
	VBookmark* pRec = GetCurrentBookmarkRow();
	if (!pRec) return;

	CSearchFieldList* pFieldList = ((DBTBookmarks*)GetDBT())->m_pSearchFields;
	if (!pFieldList) return;
	CSearchField* pField = pFieldList->GetSearchFieldByName(pRec->l_FieldName);
	if (!pField) return;

	// deseleziono il vecchio valore e seleziono il nuovo
	DataStr& aOldDescri = (DataStr&)AfxGetBaseApp()->GetOldCtrlData();
	pFieldList->SetSelected(aOldDescri.Str(), FALSE);
	pFieldList->SetSelected(pRec->l_FieldName, TRUE);

	CStringArray* pFieldValues = pField->GetFieldValues();

	if (m_pSearchFieldValuesItemSource)
		m_pSearchFieldValuesItemSource->SetSearchFieldValuesList(pFieldValues);

	// tutto questo (!) per forzare la selezione di un item nella combo
	if (pField->IsCategory())
	{
		CString fVal;
		for (int i = 0; i < pFieldValues->GetSize(); i++)
		{
			fVal = pFieldValues->GetAt(i);

			// seleziono il valore di default della categoria
			if (fVal.CompareNoCase(pField->m_pCategory->m_DefaultValue.Str()) == 0)
			{
				pRec->l_FormattedValue = fVal;
				break;
			}
		}
	}
	else // seleziono l'unico valore possibile (e' sempre uno)
		pRec->l_FormattedValue = (pFieldValues->GetSize() > 0) ? pFieldValues->GetAt(0) : _T("");
}

//-----------------------------------------------------------------------------
void CBookmarksBodyEdit::OnBookmarkRowChanged()
{
	VBookmark* pRec = GetCurrentBookmarkRow();
	if (!pRec) return;
		
	if (pRec->l_FieldName.IsEmpty())
	{
		OnBookmarkTypeChanged();
		return;
	}

	// tutto questo (!) per forzare la selezione di un item nella combo
	if (pRec->l_GroupType == E_BOOKMARK_CATEGORY)
	{
		CSearchFieldList* pFieldList = ((DBTBookmarks*)GetDBT())->m_pSearchFields;
		if (!pFieldList) return; 
		CSearchField* pField = pFieldList->GetSearchFieldByName(pRec->l_FieldName);
		if (!pField) return;
		CStringArray* pFieldValues = pField->GetFieldValues();
		if (m_pSearchFieldValuesItemSource)
			m_pSearchFieldValuesItemSource->SetSearchFieldValuesList(pFieldValues);
	}
}

/////////////////////////////////////////////////////////////////////////////
//			CAttachmentLinksBodyEdit implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------	
IMPLEMENT_DYNCREATE(CAttachmentLinksBodyEdit, CJsonBodyEdit)

//-----------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAttachmentLinksBodyEdit, CJsonBodyEdit)
	//{{AFX_MSG_MAP(CAttachmentLinksBodyEdit)
	ON_WM_LBUTTONUP()
	ON_WM_MOUSEMOVE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-----------------------------------------------------------------------------
CAttachmentLinksBodyEdit::CAttachmentLinksBodyEdit()
{
	m_bMouseInside = FALSE;
	HINSTANCE hInst = AfxFindResourceHandle(MAKEINTRESOURCE(IDC_TB_HAND), RT_GROUP_CURSOR);
	m_hHandCursor = ::LoadCursor(hInst, MAKEINTRESOURCE(IDC_TB_HAND));
}

//-----------------------------------------------------------------------------	
void CAttachmentLinksBodyEdit::Customize()
{
	__super::Customize();
	
	/*
	SetMultipleLinesPerRow(3); 
	// metto su tre righe (per ovviare a errori per stringa troppo lunga)
	*/
}

//-----------------------------------------------------------------------------
void CAttachmentLinksBodyEdit::OnLButtonUp(UINT nFlags, CPoint point)
{
	::SetCursor(m_hOldCursor);
	ReleaseCapture();
	m_bMouseInside = FALSE;

	CBodyEdit::OnLButtonUp(nFlags, point);

	int nCol, xColOffs, nRow, nNewCurrRec;

	CursorPosArea cpa = GetCursorBodyPos(point, nCol, xColOffs, nRow, nNewCurrRec);
	if (cpa != IN_BODY && cpa != IN_RESIZE_GRIP)
		return;

	// sul click del link apro il documento
	VAttachmentLink* pRec = (VAttachmentLink*)m_pDBT->GetCurrentRow();
	if (pRec)
		OpenERPDocument(pRec->l_TBDocNamespace, pRec->l_TBPrimaryKey);
}

// apre il documento di ERP utilizzando il suo namespace e la chiave
//-----------------------------------------------------------------------------
void CAttachmentLinksBodyEdit::OpenERPDocument(DataStr documentNamespace, DataStr documentKey)
{
	CAbstractFormDoc* pDoc = (CAbstractFormDoc*)AfxGetTbCmdManager()->RunDocument(documentNamespace.Str());
	if (pDoc)
		pDoc->GoInBrowserMode(documentKey);
}

//-----------------------------------------------------------------------------
void CAttachmentLinksBodyEdit::OnMouseMove(UINT nFlags, CPoint point)
{
	CRect rect;
	GetClientRect(rect);

	BOOL bInside = rect.PtInRect(point);
	if (m_bMouseInside == bInside)
		return;

	m_bMouseInside = bInside;

	if (m_bMouseInside)
	{
		m_hOldCursor = ::SetCursor(m_hHandCursor);
		SetCapture();
	}
	else
	{
		::SetCursor(m_hOldCursor);
		ReleaseCapture();
	}
	__super::OnMouseMove(nFlags, point);
}

//-----------------------------------------------------------------------------	
BOOL CAttachmentLinksBodyEdit::OnGetCustomColor(CBodyEditRowSelected* pCurrentRow)
{
	ASSERT(pCurrentRow->m_pRec->IsKindOf(RUNTIME_CLASS(VAttachmentLink)));
	pCurrentRow->m_crTextColor = AfxGetThemeManager()->GetHyperLinkForeColor();
	return TRUE;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CODICE DUPLICATO PER FARE FUNZIONARE IL PANNELLO DEGLI ALLEGATI ALLA VECCHIA MANIERA
// HO DOVUTO RIMETTERE IN PIEDI IL CODICE DELLE COMBO E DEL BODYEDIT IN MODO DA DIFFERENZIARLI DA QUELLI NUOVI CHE SONO UTILIZZATI NEL REPO EXPLORER
// CBookmarksBodyEdit			--> CAttachmentBookmarksBodyEdit
// CBookmarkTypeItemSource		--> CAttachmentBookmarkTypeCombo
// CSearchFieldsItemSource		--> CAttachmentSearchFieldsCombo
// CSearchFieldValuesItemSource	--> CAttachmentSearchFieldValuesCombo
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentBookmarkTypeCombo
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBookmarkTypeCombo, CEnumCombo)

//-----------------------------------------------------------------------------
CAttachmentBookmarkTypeCombo::CAttachmentBookmarkTypeCombo()
	:
	CEnumCombo(),
	m_bShowBinding(TRUE)
{
}

//-----------------------------------------------------------------------------
BOOL CAttachmentBookmarkTypeCombo::IsValidItemListBox(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (
		((DataEnum&)aDataObj) == E_BOOKMARK_KEY ||
		((DataEnum&)aDataObj) == E_BOOKMARK_EXTERNAL ||
		((DataEnum&)aDataObj) == E_BOOKMARK_SOS_SPECIAL ||
		((DataEnum&)aDataObj) == E_BOOKMARK_VARIABLE
		)
		return FALSE;

	if (((DataEnum&)aDataObj) == E_BOOKMARK_BINDING)
		return m_bShowBinding;

	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentSearchFieldsCombo
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentSearchFieldsCombo, CStrCombo)

//-----------------------------------------------------------------------------
CAttachmentSearchFieldsCombo::CAttachmentSearchFieldsCombo()
	:
	CStrCombo(),
	m_bCategory(FALSE),
	m_pSearchFields(NULL)
{
	m_bSorted = TRUE;
}

//-----------------------------------------------------------------------------
void CAttachmentSearchFieldsCombo::SetSearchFieldList(CSearchFieldList* pSearchFields)
{
	m_pSearchFields = pSearchFields;
}

//-----------------------------------------------------------------------------
void CAttachmentSearchFieldsCombo::OnFillListBox()
{
	if (!m_pSearchFields || m_pSearchFields->GetSize() < 1)
		return;

	CSearchField* pField = NULL;
	for (int i = 0; i < m_pSearchFields->GetSize(); i++)
	{
		pField = m_pSearchFields->GetAt(i);
		if (
			((pField->IsCategory() && m_bCategory) || (!pField->IsCategory() && !m_bCategory)) &&
			!pField->m_bSelected
			)
			AddAssociation(pField->GetDescription(), pField->m_FieldName);
	}
}

//////////////////////////////////////////////////////////////////////////////
//						CAttachmentSearchFieldValuesCombo
// contiene l'elenco dei valori corrispondenti al bookmark selezionato
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentSearchFieldValuesCombo, CStrCombo)

//-----------------------------------------------------------------------------
CAttachmentSearchFieldValuesCombo::CAttachmentSearchFieldValuesCombo()
	:
	CStrCombo(),
	m_pSearchFieldValues(NULL)
{
	m_bSorted = TRUE;
}

//-----------------------------------------------------------------------------
void CAttachmentSearchFieldValuesCombo::SetSearchFieldValuesList(CStringArray* pSearchFieldValues)
{
	m_pSearchFieldValues = pSearchFieldValues;
}

//-----------------------------------------------------------------------------
void CAttachmentSearchFieldValuesCombo::OnFillListBox()
{
	if (!m_pSearchFieldValues)
		return;

	CString strValue;
	Clear();
	for (int i = 0; i < m_pSearchFieldValues->GetSize(); i++)
	{
		strValue = m_pSearchFieldValues->GetAt(i);
		AddAssociation(strValue, strValue);
	}
}

/////////////////////////////////////////////////////////////////////////////
//			class CAttachmentBookmarksBodyEdit implementation
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachmentBookmarksBodyEdit, CBodyEdit)

//-----------------------------------------------------------------------------
CAttachmentBookmarksBodyEdit::CAttachmentBookmarksBodyEdit()
	:
	CBodyEdit(_NS_BE("AttachmentBookmarks"))
{
	BERemoveExStyle(BE_STYLE_ALLOW_MULTIPLE_SEL);
	BERemoveExStyle(BE_STYLE_ALLOW_CALLDIALOG);
	BERemoveExStyle(BE_STYLE_ALLOW_SEARCH);
}

//-----------------------------------------------------------------------------	
void CAttachmentBookmarksBodyEdit::Customize()
{
	VBookmark* pRec = GetBookmarkRow();

	m_pBookmarkTypeCombo = (CAttachmentBookmarkTypeCombo*)AddColumn
	(
		_NS_CLN("GroupType"),
		_TB("Type"),
		CBS_DROPDOWNLIST,
		IDC_ATT_BE_BOOKMARK_TYPE,
		&pRec->l_GroupType,
		RUNTIME_CLASS(CAttachmentBookmarkTypeCombo)
	);

	ColumnInfo* pColumnInfo = AddColumn
	(
		_NS_CLN("Description"),
		_TB("Description"),
		CBS_DROPDOWNLIST,
		IDC_ATT_BE_BOOKMARK_DESCRI,
		&pRec->l_FieldName,
		//&pRec->l_Description, // TODOMICHI: in C# come colonna visualizziamo la FieldDescription. il FieldName e' il nome della colonna, la Description sarebbe il nome tradotto???
		RUNTIME_CLASS(CAttachmentSearchFieldsCombo)
	);
	pColumnInfo->SetCtrlSize(30, 1);
	m_pSearchFieldsCombo = ((CAttachmentSearchFieldsCombo*)pColumnInfo->GetParsedCtrl());

	pColumnInfo = AddColumn
	(
		_NS_CLN("FormattedValue"),
		_TB("Value"),
		CBS_DROPDOWNLIST,
		IDC_ATT_BE_BOOKMARK_VALUE,
		&pRec->l_FormattedValue,
		RUNTIME_CLASS(CAttachmentSearchFieldValuesCombo)
	);
	pColumnInfo->SetCtrlSize(40, 1);

	m_pSearchFieldValuesCombo = ((CAttachmentSearchFieldValuesCombo*)pColumnInfo->GetParsedCtrl());

	m_pSearchFieldsCombo->SetSearchFieldList(((DBTBookmarks*)GetDBT())->m_pSearchFields);
}

//-----------------------------------------------------------------------------	
void CAttachmentBookmarksBodyEdit::SetOnlyCategoryField()
{
	m_pSearchFieldsCombo->SetFilterCategory();
}

//-----------------------------------------------------------------------------
BOOL CAttachmentBookmarksBodyEdit::OnCommand(WPARAM wParam, LPARAM lParam)
{
	DECLARE_WM_COMMAND_PARAMS(wParam, lParam, nID, nCode, hWndCtrl);

	if (nID == 0 || hWndCtrl == NULL)
		return CBodyEdit::OnCommand(wParam, lParam);

	if (nCode == EN_VALUE_CHANGED)
	{
		if (nID == IDC_ATT_BE_BOOKMARK_TYPE)
			OnBookmarkTypeChanged();

		if (nID == IDC_ATT_BE_BOOKMARK_DESCRI)
			OnSearchFieldChanged();
	}

	return CBodyEdit::OnCommand(wParam, lParam);
}

//-----------------------------------------------------------------------------
void CAttachmentBookmarksBodyEdit::OnBookmarkTypeChanged()
{
	VBookmark* pRec = GetCurrentBookmarkRow();

	// originale
	if (pRec->l_GroupType == E_BOOKMARK_BINDING)
		m_pSearchFieldsCombo->SetFilterBinding();
	if (pRec->l_GroupType == E_BOOKMARK_CATEGORY)
		m_pSearchFieldsCombo->SetFilterCategory();

	pRec->l_FieldName.Clear();
	pRec->l_Description.Clear();
	pRec->l_FormattedValue.Clear();
}

//-----------------------------------------------------------------------------
void CAttachmentBookmarksBodyEdit::OnSearchFieldChanged()
{
	VBookmark* pRec = GetCurrentBookmarkRow();
	if (!pRec) return;

	CSearchFieldList* pFieldList = ((DBTBookmarks*)GetDBT())->m_pSearchFields;
	CSearchField* pField = pFieldList->GetSearchFieldByName(pRec->l_FieldName);
	if (!pField) return;

	// deseleziono il vecchio valore e seleziono il nuovo
	DataStr& aOldDescri = (DataStr&)AfxGetBaseApp()->GetOldCtrlData();
	pFieldList->SetSelected(aOldDescri.Str(), FALSE);
	pFieldList->SetSelected(pRec->l_FieldName, TRUE);

	CStringArray* pFieldValues = pField->GetFieldValues();

	m_pSearchFieldValuesCombo->SetSearchFieldValuesList(pFieldValues);
	m_pSearchFieldValuesCombo->FillListBox(); // per caricare subito i valori nella combo

	// tutto questo (!) per forzare la selezione di un item nella combo
	if (pField->IsCategory())
	{
		CString fVal;
		for (int i = 0; i < pFieldValues->GetSize(); i++)
		{
			fVal = pFieldValues->GetAt(i);

			// seleziono il valore di default della categoria
			if (fVal.CompareNoCase(pField->m_pCategory->m_DefaultValue.Str()) == 0)
			{
				pRec->l_FormattedValue = fVal;
				break;
			}
		}
	}
	else // seleziono l'unico valore possibile (e' sempre uno)
		pRec->l_FormattedValue = (pFieldValues->GetSize() > 0) ? pFieldValues->GetAt(0) : _T("");
}

//-----------------------------------------------------------------------------
void CAttachmentBookmarksBodyEdit::OnBookmarkRowChanged()
{
	VBookmark* pRec = GetCurrentBookmarkRow();
	if (!pRec)
		return;

	if (pRec->l_FieldName.IsEmpty())
	{
		OnBookmarkTypeChanged();
		return;
	}

	// tutto questo (!) per forzare la selezione di un item nella combo
	if (pRec->l_GroupType == E_BOOKMARK_CATEGORY)
	{
		CSearchFieldList* pFieldList = ((DBTBookmarks*)GetDBT())->m_pSearchFields;
		CSearchField* pField = pFieldList->GetSearchFieldByName(pRec->l_FieldName);
		if (!pField) return;
		CStringArray* pFieldValues = pField->GetFieldValues();
		m_pSearchFieldValuesCombo->SetSearchFieldValuesList(pFieldValues);
		m_pSearchFieldValuesCombo->FillListBox(); // per caricare subito i valori nella combo
	}
}

