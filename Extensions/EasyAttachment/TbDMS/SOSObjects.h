#pragma once

#include <TbOleDb\SqlRec.h>
#include <TbGes\HotLink.h>
#include <TbGes\BODYEDIT.H>
#include <TbGes\DBT.h>

//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
enum CanBeSentToSOSType { BeSent, NoPDFA, EmptySosKeyValue }; //corrisponde all'enum managed ArchiveResult presente in EasyAttachment\BusinessLogic\SosManager.cs

//////////////////////////////////////////////////////////////////////////////
//						    CSOSEventArgs
// classe di appoggio per il passaggio delle informazioni del SOSEventsArgs del C#
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSOSEventArgs : public CObject
{
	DECLARE_DYNCREATE(CSOSEventArgs)

public:
	int		m_nIdx;
	CString m_sMessage;
	CString m_sNotes;

	Microarea::TaskBuilderNet::Interfaces::DiagnosticType m_MessageType;

public:
	CSOSEventArgs();
};

//creazione di un CSOSEventArgs a partire da un SOSEventArgs^
CSOSEventArgs* CreateSOSEventArgs(Microarea::EasyAttachment::Components::SOSEventArgs^);

///////////////////////////////////////////////////////////////////////////////
//						VSOSConfiguration definition
//	SqlVirtualRecord per mappare i parametri di configurazione della SOS
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VSOSConfiguration : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VSOSConfiguration)

public:
	DataLng	  	l_ParamID;
	DataStr   	l_SubjectCode;
	DataStr		l_AncestorCode;
	DataStr		l_KeeperCode;
	DataStr		l_MySOSUser;
	DataStr		l_MySOSPassword;
	DataStr		l_SOSWebServiceUrl;
	DataLng		l_ChunkDimension;
	DataLng		l_EnvelopeDimension;
	DataBool	l_FTPSend;
	DataStr		l_FTPSharedFolder;
	DataInt		l_FTPUpdateDayOfWeek;

public:
	VSOSConfiguration(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR   GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//						VSOSDocClass definition
//	SqlVirtualRecord per mappare l'elenco delle classi documentali
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VSOSDocClass : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VSOSDocClass)

public:
	DataLng	  	l_ParamID; //k
	DataStr   	l_Code; //k
	DataStr		l_Description;

	gcroot<Microarea::EasyAttachment::Core::DocClass^> docClass; // puntatore all'oggetto DocClass del C#

public:
	VSOSDocClass(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR   GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//						VSOSDocument definition
//	SqlVirtualRecord per mappare un record della tabella DMS_SOSDocument
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VSOSDocument : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VSOSDocument)

public:
	DataBool	l_IsSelected;
	DataLng	  	l_AttachmentID;
	DataLng		l_EnvelopeID;
	DataStr   	l_FileName;
	DataLng		l_Size;
	DataStr		l_DescriptionKeys;
	DataStr		l_HashCode;
	DataStr		l_DocumentStatus;
	DataStr		l_AbsoluteCode;
	DataStr		l_LotID;
	DataDate	l_ArchivedDate;
	DataDate	l_RegistrationDate;
	DataStr		l_TaxJournal;
	DataStr		l_DocumentType;
	DataStr		l_FiscalYear;

	gcroot<Microarea::EasyAttachment::Components::AttachmentInfo^> attachInfo; // puntatore all'AttachmentInfo del C#

public:
	VSOSDocument(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR   GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//						VSOSElaboration definition
//	SqlVirtualRecord per riempire il DBT con i msg di elaborazione della SOS
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VSOSElaboration : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VSOSElaboration)

public:
	DataStr	l_MsgBmp;
	DataStr	l_Message;
	DataStr	l_Notes;

public:
	VSOSElaboration(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord();

public:
	static LPCTSTR GetStaticName();
};

//////////////////////////////////////////////////////////////////////////////
//						    CSOSConfiguration
// classe di appoggio per la gestione dei parametri di configurazione SOS
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSOSConfiguration : public CObject
{
	DECLARE_DYNCREATE(CSOSConfiguration)

public:
	VSOSConfiguration*	m_pRecSOSConfiguration;
	RecordArray*		m_pSOSDocClassesArray;

public:
	CSOSConfiguration();
	~CSOSConfiguration();

public:
	CSOSConfiguration& operator = (const CSOSConfiguration& sosConfiguration);
};

///////////////////////////////////////////////////////////////////////////////
//					CERPSOSDocumentType definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CERPSOSDocumentType : public CObject
{
	DECLARE_DYNCREATE(CERPSOSDocumentType)

public:
	CERPSOSDocumentType();

public:
	DataStr	m_DocNamespace;
	DataStr	m_DocType;

public:
	CERPSOSDocumentType& operator = (const CERPSOSDocumentType& erpSOSDocumentType);
};

///////////////////////////////////////////////////////////////////////////////
//					CERPFieldRule definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CERPFieldRule : public CObject
{
	DECLARE_DYNCREATE(CERPFieldRule)

public:
	CERPFieldRule();

public:
	DataStr	m_FieldName;
	DataStr	m_FromValue;
	DataStr	m_ToValue;
};

///////////////////////////////////////////////////////////////////////////////
//					CSOSSearchRules definition
//		struttura per mappare i SOSSearchRules di EasyAttachment lato C#
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSOSSearchRules : public CObject
{
	DECLARE_DYNCREATE(CSOSSearchRules)

public:
	CSOSSearchRules();
	~CSOSSearchRules();

public:
	CStringArray	m_arSosDocumentTypes;
	DataStr			m_SosTaxJournal; 
	DataStr			m_SosFiscalYear;
	DataBool		m_bOnlyMainDoc;
	
	DataBool		m_bDocIdle;
	DataBool		m_bDocToResend;

	//filtraggi sul campi di ERP
	DataDate		m_StartDocDate;
	DataDate		m_EndDocDate;
	DataDate		m_StartPostingDate;
	DataDate		m_EndPostingDate;
	DataStr			m_FromDocNum;
	DataStr			m_ToDocNum;
	DataStr			m_FromSupplierDocNum;
	DataStr			m_ToSupplierDocNum;
	DataStr			m_FromCustSupp;
	DataStr			m_ToCustSupp;

	// usati nella SearchAttachmentsForAdjust
	CERPSOSDocumentType m_ERPSosDocumentType;
	CObArray			m_arERPFieldsRule;		

	gcroot<Microarea::EasyAttachment::BusinessLogic::ERPFieldRuleList^> erpFieldRules;
	//

public:
	void Clear			();

	// usati nella SearchAttachmentsForAdjust
	void AddERPFilter	(CString sFieldName, CString sFromValue, CString sToValue);
	void AddERPFilter	(CString sFieldName, DataObj* pFromValue, DataObj* pToValue);
	//
};

///////////////////////////////////////////////////////////////////////////////
//						SOSDocClassInfo definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT SOSDocClassInfo : public CObject
{
	DECLARE_DYNCREATE(SOSDocClassInfo)

public:
	DataStr		m_Code;
	DataStr		m_Description;

	gcroot<Microarea::EasyAttachment::Core::DocClass^> docClass; // puntatore all'oggetto DocClass del C#
	
public:
	SOSDocClassInfo();
};

///////////////////////////////////////////////////////////////////////////////
//						SOSDocClassList definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT SOSDocClassList : public ::Array
{
public:
	SOSDocClassInfo* GetAt(int nIdx) const { return (SOSDocClassInfo*)Array::GetAt(nIdx); }
	int Add(SOSDocClassInfo* att) { return Array::Add((CObject*)att); }

public:
	SOSDocClassInfo* GetSOSDocClassInfoByCode(const CString code);
};

//////////////////////////////////////////////////////////////////////////////
//			       class DBTSOSDocuments definition
//		DBT per visualizzare un elenco di documenti archiviati
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTSOSDocuments : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTSOSDocuments)

public:
	DBTSOSDocuments(CRuntimeClass*	pClass, CAbstractFormDoc* pDocument);

public:
	VSOSDocument*	GetCurrent		() 			const { return (VSOSDocument*)GetCurrentRow(); }
	VSOSDocument*	GetSOSDocument	(int nRow)	const { return (VSOSDocument*)GetRow(nRow); }
	VSOSDocument*	GetSOSDocument	()			const { return (VSOSDocument*)GetRecord(); }

	int				GetCurrentRowIdx()			const { return m_nCurrentRow; }
	virtual void	SetCurrentRow	(int nRow);

	void			LoadSOSDocuments(RecordArray* pSOSDocumentArray);

protected:
	virtual	void		OnDefineQuery		() {}
	virtual	void		OnPrepareQuery		() {}
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//			       class DBTSOSElaboration definition
//		DBT per visualizzare l'elenco dei msg dell'elaborazione
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTSOSElaboration : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTSOSElaboration)

public:
	DBTSOSElaboration(CRuntimeClass* pClass, CAbstractFormDoc* pDocument);

public:
	VSOSElaboration*	GetCurrent			() 			const { return (VSOSElaboration*)GetCurrentRow(); }
	VSOSElaboration*	GetSOSElaboration	(int nRow)	const { return (VSOSElaboration*)GetRow(nRow); }
	VSOSElaboration*	GetSOSElaboration	()			const { return (VSOSElaboration*)GetRecord(); }

	int					GetCurrentRowIdx	()			const { return m_nCurrentRow; }

	virtual void		SetCurrentRow		(int nRow);
	virtual BOOL		LocalFindData		(BOOL bPrepareOld);

protected:
	virtual	void		OnDefineQuery		() {}
	virtual	void		OnPrepareQuery		() {}
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*);
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*);
};

//////////////////////////////////////////////////////////////////////////////
//			       class CSOSDocClassesItemSource definition
//				ItemSource con elenco delle classi documentali
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CSOSDocClassesItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CSOSDocClassesItemSource)

public:
	CSOSDocClassesItemSource();

private:
	SOSDocClassList* m_pDocClasses;

public:
	void SetDocClassList(SOSDocClassList*);

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
};

//////////////////////////////////////////////////////////////////////////////
//			       class CSOSDocTypeItemSource definition
//	ItemSource con elenco dei tipi documento per una specifica classe documentale
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CSOSDocTypeItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CSOSDocTypeItemSource)

public:
	CSOSDocTypeItemSource();

private:
	CStringArray* m_pDocTypes;

public:
	void SetDocTypesList(CStringArray*);

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
};

//////////////////////////////////////////////////////////////////////////////
//			       class CSOSTaxJournalItemSource definition
//	ItemSource con elenco dei libri giornale disponibili per tipo documento 
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CSOSTaxJournalItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CSOSTaxJournalItemSource)

public:
	CSOSTaxJournalItemSource();
	~CSOSTaxJournalItemSource();

private:
	CStringArray* m_pTaxJournals;

public:
	void SetTaxJournalsList(CStringArray*);

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
};

//////////////////////////////////////////////////////////////////////////////
//			       class CSOSFiscalYearItemSource definition
//	ItemSource con elenco dei libri giornale disponibili per tipo documento 
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CSOSFiscalYearItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CSOSFiscalYearItemSource)

public:
	CSOSFiscalYearItemSource();

private:
	CStringArray* m_pFiscalYears;

public:
	void SetFiscalYearsList(CStringArray*);

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);
};

#include "endh.dex"
