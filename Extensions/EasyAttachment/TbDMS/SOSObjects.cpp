#include "stdafx.h" 

#include <TbGeneric\Array.h>

#include "CDDMS.h"

#include "TbRepositoryManager.h"
#include "SOSObjects.h"

using namespace System;
using namespace System::Data;
using namespace System::Xml;
using namespace Microarea::EasyAttachment::Components;
using namespace Microarea::EasyAttachment::Core;
using namespace Microarea::TaskBuilderNet::Interfaces;
using namespace Microarea::Framework::TBApplicationWrapper;
using namespace Microarea::TBPicComponents;
using namespace Microarea::EasyAttachment::BusinessLogic;

//////////////////////////////////////////////////////////////////////////////
//					CSOSEventArgs implementation							//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSEventArgs, CObject)

//-----------------------------------------------------------------------------
CSOSEventArgs::CSOSEventArgs()
	:
	m_nIdx(-1),
	m_MessageType(DiagnosticType::None)
{
}

//creazione di un CSOSEventArgs a partire da un SOSEventArgs^
//-----------------------------------------------------------------------------
CSOSEventArgs* CreateSOSEventArgs(Microarea::EasyAttachment::Components::SOSEventArgs^ args)
{
	CSOSEventArgs* sosArgs = new CSOSEventArgs();
	sosArgs->m_nIdx = args->Idx;
	sosArgs->m_sMessage = args->Message;
	sosArgs->m_MessageType = args->MessageType;
	return sosArgs;
}

///////////////////////////////////////////////////////////////////////////////
//						VSOSConfiguration declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VSOSConfiguration, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VSOSConfiguration::VSOSConfiguration(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord	(_T("VSOSConfiguration")),
	l_ParamID			(0),
	l_ChunkDimension	(20),
	l_EnvelopeDimension	(600),
	l_FTPSend			(FALSE),
	l_FTPUpdateDayOfWeek(7)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VSOSConfiguration::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_KEY	(_NS_FLD("VParamID"),				l_ParamID);
		LOCAL_STR	(_NS_FLD("VSubjectCode"),			l_SubjectCode,		20);
		LOCAL_STR	(_NS_FLD("VAncestorCode"),			l_AncestorCode,		20);
		LOCAL_STR	(_NS_FLD("VKeeperCode"),			l_KeeperCode,		20);
		LOCAL_STR	(_NS_FLD("VMySOSUser"),				l_MySOSUser,		50);
		LOCAL_STR	(_NS_FLD("VMySOSPassword"),			l_MySOSPassword,	40);
		LOCAL_STR	(_NS_FLD("VSOSWebServiceUrl"),		l_SOSWebServiceUrl, 128);
		LOCAL_DATA	(_NS_FLD("VChunkDimension"),		l_ChunkDimension);
		LOCAL_DATA	(_NS_FLD("VEnvelopeDimension"),		l_EnvelopeDimension);
		LOCAL_DATA	(_NS_FLD("VFTPSend"),				l_FTPSend);
		LOCAL_STR	(_NS_FLD("VFTPSharedFolder"),		l_FTPSharedFolder,	200);
		LOCAL_DATA	(_NS_FLD("VFTPUpdateDayOfWeek"),	l_FTPUpdateDayOfWeek);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VSOSConfiguration::GetStaticName() { return _NS_TBL("VSOSConfiguration"); }


///////////////////////////////////////////////////////////////////////////////
//						VSOSDocClass declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VSOSDocClass, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VSOSDocClass::VSOSDocClass(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VSOSDocClass")),
	l_ParamID		(0),
	docClass		(nullptr)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VSOSDocClass::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_KEY		(_NS_FLD("VParamID"),		l_ParamID);
		LOCAL_KEY_STR	(_NS_FLD("VCode"),			l_Code,			20);
		LOCAL_STR		(_NS_FLD("VDescription"),	l_Description,	50);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VSOSDocClass::GetStaticName() { return _NS_TBL("VSOSDocClass"); }

///////////////////////////////////////////////////////////////////////////////
//						VSOSDocument declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VSOSDocument, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VSOSDocument::VSOSDocument(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VSOSDocument")),
	l_IsSelected	(FALSE),
	attachInfo		(nullptr)
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VSOSDocument::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_DATA	(_NS_FLD("VIsSelected"),		l_IsSelected);
		LOCAL_KEY	(_NS_FLD("VAttachmentID"),		l_AttachmentID);
		LOCAL_DATA	(_NS_FLD("VEnvelopeID"),		l_EnvelopeID);
		LOCAL_STR	(_NS_FLD("VFileName"),			l_FileName,			256);
		LOCAL_DATA	(_NS_FLD("VSize"),				l_Size);
		LOCAL_STR	(_NS_FLD("VDescriptionKeys"),	l_DescriptionKeys,	512); 
		LOCAL_STR	(_NS_FLD("VHashCode"),			l_HashCode,			128);
		LOCAL_STR	(_NS_FLD("VDocumentStatus"),	l_DocumentStatus,	20);
		LOCAL_STR	(_NS_FLD("VAbsoluteCode"),		l_AbsoluteCode,		50);
		LOCAL_STR	(_NS_FLD("VLotID"),				l_LotID,			50);
		LOCAL_DATA	(_NS_FLD("VArchivedDate"),		l_ArchivedDate);
		LOCAL_DATA	(_NS_FLD("VRegistrationDate"),	l_RegistrationDate);
		LOCAL_STR	(_NS_FLD("VTaxJournal"),		l_TaxJournal,		8);
		LOCAL_STR	(_NS_FLD("VDocumentType"),		l_DocumentType,		50);
		LOCAL_STR	(_NS_FLD("VFiscalYear"),		l_FiscalYear,		4);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VSOSDocument::GetStaticName() { return _NS_TBL("VSOSDocument"); }

///////////////////////////////////////////////////////////////////////////////
//						VSOSElaboration declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(VSOSElaboration, SqlVirtualRecord)

//-----------------------------------------------------------------------------
VSOSElaboration::VSOSElaboration(BOOL bCallInit  /* = TRUE */)
	:
	SqlVirtualRecord(_T("VSOSElaboration"))
{
	BindRecord();
	if (bCallInit) Init();
}

//-----------------------------------------------------------------------------
void VSOSElaboration::BindRecord()
{
	BEGIN_BIND_DATA();
		LOCAL_STR	(_NS_FLD("VMsgBmp"),	l_MsgBmp,	32);
		LOCAL_STR	(_NS_FLD("VMessage"),	l_Message,	100);
		LOCAL_STR	(_NS_FLD("VNotes"),		l_Notes,	200);
	END_BIND_DATA();
}

//-----------------------------------------------------------------------------
LPCTSTR VSOSElaboration::GetStaticName() { return _NS_TBL("VSOSElaboration"); }

//////////////////////////////////////////////////////////////////////////////
//					CSOSConfiguration implementation					//
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSConfiguration, CObject)

//-----------------------------------------------------------------------------
CSOSConfiguration::CSOSConfiguration()
	:
	m_pRecSOSConfiguration(NULL),
	m_pSOSDocClassesArray(NULL)
{
	m_pRecSOSConfiguration = new VSOSConfiguration();
	m_pSOSDocClassesArray = new RecordArray();
}

//-----------------------------------------------------------------------------
CSOSConfiguration::~CSOSConfiguration()
{
	if (m_pRecSOSConfiguration)
		delete m_pRecSOSConfiguration;
	
	if (m_pSOSDocClassesArray)
	{
		m_pSOSDocClassesArray->RemoveAll();
		delete m_pSOSDocClassesArray;
	}
}

//------------------------------------------------------------------------------
CSOSConfiguration& CSOSConfiguration::operator =(const CSOSConfiguration& sosConfiguration)
{
	*m_pRecSOSConfiguration = *sosConfiguration.m_pRecSOSConfiguration;

	if (m_pSOSDocClassesArray)
		m_pSOSDocClassesArray->RemoveAll();

	VSOSDocClass* pNewRec;
	for (int i = 0; i < sosConfiguration.m_pSOSDocClassesArray->GetSize(); i++)
	{
		pNewRec = new VSOSDocClass();
		*pNewRec = *(VSOSDocClass*)sosConfiguration.m_pSOSDocClassesArray->GetAt(i);
		m_pSOSDocClassesArray->Add(pNewRec);
	}

	return *this;
}

///////////////////////////////////////////////////////////////////////////////
//						CERPSOSDocumentType implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CERPSOSDocumentType, CObject)

//-----------------------------------------------------------------------------
CERPSOSDocumentType::CERPSOSDocumentType()
{
}

//------------------------------------------------------------------------------
CERPSOSDocumentType& CERPSOSDocumentType::operator =(const CERPSOSDocumentType& erpSOSDocumentType)
{
	m_DocType = erpSOSDocumentType.m_DocType;
	m_DocNamespace = erpSOSDocumentType.m_DocNamespace;
	return *this;
}

///////////////////////////////////////////////////////////////////////////////
//						CERPFieldRule implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CERPFieldRule, CObject)

//-----------------------------------------------------------------------------
CERPFieldRule::CERPFieldRule()
{
}

///////////////////////////////////////////////////////////////////////////////
//						CSOSSearchRules implementation
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSSearchRules, CObject)

//-----------------------------------------------------------------------------
CSOSSearchRules::CSOSSearchRules()
{
}

//-----------------------------------------------------------------------------
CSOSSearchRules::~CSOSSearchRules()
{
}

//-----------------------------------------------------------------------------
void CSOSSearchRules::Clear()
{
	m_SosTaxJournal.Clear();
	m_arSosDocumentTypes.RemoveAll();
	m_SosFiscalYear.Clear();
	m_bOnlyMainDoc = FALSE;	
	if ((ERPFieldRuleList^)erpFieldRules != nullptr)
		erpFieldRules->Clear();

}

//-----------------------------------------------------------------------------------------       
void CSOSSearchRules::AddERPFilter(CString sFieldName, CString sFromValue, CString sToValue)
{
	CERPFieldRule* fieldRule;

	for (int i = 0; i < m_arERPFieldsRule.GetSize(); i++)
	{
		fieldRule = (CERPFieldRule*)m_arERPFieldsRule.GetAt(i);
		if (fieldRule->m_FieldName.CompareNoCase(sFieldName) == 0)
			break;
	}

	if (!fieldRule)
		fieldRule = new CERPFieldRule();

	fieldRule->m_FromValue = sFromValue;
	fieldRule->m_ToValue = sToValue;
	m_arERPFieldsRule.Add(fieldRule);
}

//-----------------------------------------------------------------------------------------       
void CSOSSearchRules::AddERPFilter(CString sFieldName, DataObj* pFromValue, DataObj* pToValue)
{
	if ((ERPFieldRuleList^)erpFieldRules == nullptr)
		erpFieldRules = gcnew Microarea::EasyAttachment::BusinessLogic::ERPFieldRuleList();

	if (pFromValue != NULL && pToValue != NULL)
		erpFieldRules->AddBetweenERPFilter(gcnew String(sFieldName), MDataObj::Create((System::IntPtr)(pFromValue)), MDataObj::Create((System::IntPtr)(pToValue)));
	else
		if (pFromValue != NULL)
			erpFieldRules->AddFromERPFilter(gcnew String(sFieldName), MDataObj::Create((System::IntPtr)(pFromValue)));
		else
			if (pToValue != NULL)
				erpFieldRules->AddToERPFilter(gcnew String(sFieldName), MDataObj::Create((System::IntPtr)(pToValue)));
}

///////////////////////////////////////////////////////////////////////////////
//					SOSDocClassInfo declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(SOSDocClassInfo, CObject)

//-----------------------------------------------------------------------------
SOSDocClassInfo::SOSDocClassInfo()
	:
	docClass(nullptr)
{
}

///////////////////////////////////////////////////////////////////////////////
//					DMSCollectionList declaration
///////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
SOSDocClassInfo* SOSDocClassList::GetSOSDocClassInfoByCode(const CString code)
{
	SOSDocClassInfo* pInfo = NULL;

	for (int j = 0; j < GetCount(); j++)
	{
		pInfo = GetAt(j);
		if (pInfo && pInfo->m_Code.CompareNoCase(code) == 0)
			return pInfo;
	}

	return NULL;
}

//////////////////////////////////////////////////////////////////////////////
//				DBTSOSDocuments implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTSOSDocuments, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTSOSDocuments::DBTSOSDocuments
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DBTSOSDocuments"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
DataObj* DBTSOSDocuments::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTSOSDocuments::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VSOSDocument)));
}

//-----------------------------------------------------------------------------
void DBTSOSDocuments::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

// aggiungo manualmente i SqlRecord al DBT
//-----------------------------------------------------------------------------	
void DBTSOSDocuments::LoadSOSDocuments(RecordArray* pSOSDocumentArray)
{
	RemoveAll();
	
	if (!pSOSDocumentArray)
		return;

	for (int i = 0; i < pSOSDocumentArray->GetCount(); i++)
	{
		VSOSDocument* pRec = (VSOSDocument*)pSOSDocumentArray->GetAt(i);
		VSOSDocument* pNewRec = (VSOSDocument*)AddRecord();
		*pNewRec = *pRec;
	}
}

//////////////////////////////////////////////////////////////////////////////
//				DBTSOSElaboration implementation
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(DBTSOSElaboration, DBTSlaveBuffered)

//-----------------------------------------------------------------------------	
DBTSOSElaboration::DBTSOSElaboration
(
	CRuntimeClass*		pClass,
	CAbstractFormDoc*	pDocument
	)
	:
	DBTSlaveBuffered(pClass, pDocument, _NS_DBT("DBTSOSElaboration"), ALLOW_EMPTY_BODY, FALSE)
{
}

//-----------------------------------------------------------------------------
DataObj* DBTSOSElaboration::OnCheckPrimaryKey(int /*nRow*/, SqlRecord*)
{
	return NULL;
}

//-----------------------------------------------------------------------------
void DBTSOSElaboration::OnPreparePrimaryKey(int nRow, SqlRecord* pSqlRec)
{
	ASSERT(pSqlRec->IsKindOf(RUNTIME_CLASS(VSOSElaboration)));
}

//-----------------------------------------------------------------------------
void DBTSOSElaboration::SetCurrentRow(int nRow)
{
	DBTSlaveBuffered::SetCurrentRow(nRow);
}

// aggiungo manualmente i SqlRecord al DBT
//-----------------------------------------------------------------------------	
BOOL DBTSOSElaboration::LocalFindData(BOOL bPrepareOld)
{
	RemoveAll();
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//			     class CSOSDocClassesItemSource implementation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSDocClassesItemSource, CItemSource)

//-----------------------------------------------------------------------------
CSOSDocClassesItemSource::CSOSDocClassesItemSource()
	:
	CItemSource		(),
	m_pDocClasses	(NULL)
{
}

//-----------------------------------------------------------------------------
void CSOSDocClassesItemSource::SetDocClassList(SOSDocClassList* pDocClasses)
{
	m_pDocClasses = pDocClasses;
}

//-----------------------------------------------------------------------------
void CSOSDocClassesItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pDocClasses)
		return;

	for (int i = 0; i < m_pDocClasses->GetSize(); i++)
	{
		SOSDocClassInfo* pInfo = m_pDocClasses->GetAt(i);
		if (pInfo)
		{
			// skippo la classi documentali non gestite dalla SOS (ad esempio quelle di Fatel)
			if (pInfo->docClass->InternalDocClass == "" || pInfo->docClass->ERPDocNamespaces->Count == 0)
				continue;

			values.Add(new DataStr(pInfo->m_Code));
			descriptions.Add(pInfo->m_Description);
		}
	}
}

//////////////////////////////////////////////////////////////////////////////
//			     class CSOSDocTypeItemSource implementation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSDocTypeItemSource, CItemSource)

//-----------------------------------------------------------------------------
CSOSDocTypeItemSource::CSOSDocTypeItemSource()
	:
	CItemSource(),
	m_pDocTypes(NULL)
{
}

//-----------------------------------------------------------------------------
void CSOSDocTypeItemSource::SetDocTypesList(CStringArray* pDocTypes)
{
	m_pDocTypes = pDocTypes;
}

//-----------------------------------------------------------------------------
void CSOSDocTypeItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pDocTypes)
		return;

	for (int i = 0; i < m_pDocTypes->GetSize(); i++)
	{
		values.Add(new DataStr(m_pDocTypes->GetAt(i)));
		descriptions.Add(m_pDocTypes->GetAt(i));
	}
}

//////////////////////////////////////////////////////////////////////////////
//			     class CSOSTaxJournalItemSource implementation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSTaxJournalItemSource, CItemSource)

//-----------------------------------------------------------------------------
CSOSTaxJournalItemSource::CSOSTaxJournalItemSource()
	:
	CItemSource(),
	m_pTaxJournals(NULL)
{
}

//-----------------------------------------------------------------------------
CSOSTaxJournalItemSource::~CSOSTaxJournalItemSource()
{}

//-----------------------------------------------------------------------------
void CSOSTaxJournalItemSource::SetTaxJournalsList(CStringArray* pTaxJournals)
{
	// vedere se riesco a toglierla
	m_pTaxJournals = pTaxJournals;
}

//-----------------------------------------------------------------------------
void CSOSTaxJournalItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pTaxJournals)
		return;

	for (int i = 0; i < m_pTaxJournals->GetSize(); i++)
	{
		values.Add(new DataStr(m_pTaxJournals->GetAt(i)));
		descriptions.Add(m_pTaxJournals->GetAt(i));
	}
}

//////////////////////////////////////////////////////////////////////////////
//			     class CSOSFiscalYearItemSource implementation
//////////////////////////////////////////////////////////////////////////////
//----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CSOSFiscalYearItemSource, CItemSource)

//-----------------------------------------------------------------------------
CSOSFiscalYearItemSource::CSOSFiscalYearItemSource()
	:
	CItemSource(),
	m_pFiscalYears(NULL)
{
}

//-----------------------------------------------------------------------------
void CSOSFiscalYearItemSource::SetFiscalYearsList(CStringArray* pFiscalYears)
{
	m_pFiscalYears = pFiscalYears;
}

//-----------------------------------------------------------------------------
void CSOSFiscalYearItemSource::GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue)
{
	if (!m_pFiscalYears)
		return;

	for (int i = 0; i < m_pFiscalYears->GetSize(); i++)
	{
		values.Add(new DataStr(m_pFiscalYears->GetAt(i)));
		descriptions.Add(m_pFiscalYears->GetAt(i));
	}
}
