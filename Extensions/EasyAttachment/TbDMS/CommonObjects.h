#pragma once

#include <TbOleDb\SqlRec.h>
#include <TbGes\HotLink.h>
#include <TbGes\BODYEDIT.H>
#include <TbGes\DBT.h>
#include <TbGenlib\DMSAttachmentInfo.h>
#include <TbGes\JsonFormEngineEx.h>

//includere alla fine degli include del .H
#include "beginh.dex"

// estensioni file ammesse
static TCHAR szAllFiles[]	= _T("*.*");
static TCHAR szAvi[]		= _T("avi");
static TCHAR szBmp[]		= _T("bmp");
static TCHAR szConfig[]		= _T("config");
static TCHAR szDoc[]		= _T("doc");
static TCHAR szDocx[]		= _T("docx");
static TCHAR szGif[]		= _T("gif");
static TCHAR szGzip[]		= _T("gzip");
static TCHAR szHtml[]		= _T("html");
static TCHAR szHtm[]		= _T("htm");
static TCHAR szJpg[]		= _T("jpg");
static TCHAR szJpeg[]		= _T("jpeg");
static TCHAR szMp3[]		= _T("mp3");
static TCHAR szMpeg[]		= _T("mpeg");
static TCHAR szMsg[]		= _T("msg");
static TCHAR szPapery[]		= _T("papery");
static TCHAR szPdf[]		= _T("pdf");
static TCHAR szPng[]		= _T("png");
static TCHAR szPpt[]		= _T("ppt");
static TCHAR szPptx[]		= _T("pptx");
static TCHAR szRar[]		= _T("rar");
static TCHAR szRtf[]		= _T("rtf");
static TCHAR szTif[]		= _T("tif");
static TCHAR szTiff[]		= _T("tiff");
static TCHAR szTxt[]		= _T("txt");
static TCHAR szXml[]		= _T("xml");
static TCHAR szXls[]		= _T("xls");
static TCHAR szXlsx[]		= _T("xlsx");
static TCHAR szWmv[]		= _T("wmv");
static TCHAR szWav[]		= _T("wav");
static TCHAR szZip[]		= _T("zip");
static TCHAR szZip7z[]		= _T("7z");

// estensioni per salvare i documenti dopo la scansione 
static TCHAR szDotBMP[] = _T(".BMP");
static TCHAR szDotGIF[] = _T(".GIF");
static TCHAR szDotJPEG[]= _T(".JPEG");
static TCHAR szDotPNG[] = _T(".PNG");
static TCHAR szDotPDF[] = _T(".PDF");
static TCHAR szDotTIFF[]= _T(".TIFF");
//

class VCategoryValues;
class DBTBookmarks;

///////////////////////////////////////////////////////////////////////////////
//					DMSCollectionInfo definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT DMSCollectionInfo : public CObject
{
	DECLARE_DYNCREATE(DMSCollectionInfo)

public:
	DataLng		m_CollectorID;
	DataLng		m_CollectionID;
	DataStr		m_Name;
	DataBool	m_IsStandard;
	DataStr		m_DocNamespace;
	DataStr		m_DocTitle;

public:
	DMSCollectionInfo();
};

///////////////////////////////////////////////////////////////////////////////
//					DMSCollectionList definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DMSCollectionList : public ::Array
{
public:
	DMSCollectionInfo* GetAt(int nIdx) const { return (DMSCollectionInfo*)Array::GetAt(nIdx); }
	int Add(DMSCollectionInfo* att) { return Array::Add((CObject*)att); }

public:
	DMSCollectionInfo* GetCollectionInfoByDocNamespace(const CString& docNamespace);
	DMSCollectionInfo* GetCollectionInfoByCollectionID(int collectionID);
};

///////////////////////////////////////////////////////////////////////////////
//						DMSAttachmentInfo definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DMSAttachmentInfo : public CAttachmentInfo
{
	DECLARE_DYNCREATE(DMSAttachmentInfo)

public:
	DMSAttachmentInfo();
	~DMSAttachmentInfo();

public:
	DataStr	m_BarcodeValue;
	DataStr	m_BarcodeType;

	gcroot<Microarea::EasyAttachment::Components::AttachmentInfo^>	attachmentInfo;

public:
	//gestione bookmark
	void AddCategoryField	(const CString& name, const CString& value);
	void AddBindingField	(const CString& name);
	void ModifyBookmarks	(DBTBookmarks* pDBTBookmarks);

	void Clear			();
	BOOL IsValid		() const;
	BOOL IsOwnCheckOut	() const;
	BOOL CanCheckOut	() const;

public:
	// operators
	DMSAttachmentInfo& operator = (const DMSAttachmentInfo& attachInfo);
};

//creazione di un DMSAttachmentInfo a partire da un AttachmentInfo^
//=============================================================================
DMSAttachmentInfo* CreateDMSAttachmentInfo(Microarea::EasyAttachment::Components::AttachmentInfo^ attachment, BOOL bAllInformation = TRUE);

//tipizzo l'array degli attachmentinfo così i programmatori (vedi Germano) 
//non si devono preoccupare di effettuare la delete del contenuto delle celle
///////////////////////////////////////////////////////////////////////////////
//					DMSAttachmentsList definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DMSAttachmentsList : public ::Array
{
public:
	DMSAttachmentInfo* GetAt(int nIdx) const { return (DMSAttachmentInfo*)Array::GetAt(nIdx); }
	int Add(DMSAttachmentInfo* att) { return Array::Add((CObject*)att); }
	
	int GetAttachmentIdx(DMSAttachmentInfo* att);
};

///////////////////////////////////////////////////////////////////////////////
//						CDMSCategory definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CDMSCategory : public CObject
{
public:
	DataStr			m_Name;
	DataStr			m_Description;
	::Array			m_arCategoryValues;
	DataStr			m_DefaultValue;
	COLORREF		m_Color;
	DataBool		m_bDisabled;
	DataBool		m_bInUse;
	DataStr*		m_pDataStr;

public:
	CDMSCategory(CString strName = _T(""));
	~CDMSCategory();

public:
	void AddValue	(CString strValue, BOOL isDefault);
	void Clear		();

	const ::Array*		GetValues	()			const { return &m_arCategoryValues; }
	VCategoryValues*	GetValueAt	(int nIdx)	const { return (nIdx >= 0 && nIdx <= m_arCategoryValues.GetUpperBound()) ? (VCategoryValues*)m_arCategoryValues.GetAt(nIdx) : NULL; }

public:
	// operators
	CDMSCategory& operator = (const CDMSCategory& attachInfo);
};

///////////////////////////////////////////////////////////////////////////////
//						CDMSCategories definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CDMSCategories : public ::Array
{
public:
	CDMSCategory* GetAt(int nIdx) const { return (CDMSCategory*)Array::GetAt(nIdx); }
	void Add(CDMSCategory* cat) { Array::Add((CObject*)cat); }
};

//////////////////////////////////////////////////////////////////////////////
//						    CSearchField
// campo del DBTMaster da inserire come chiave di ricerca oppure di tipo categoria
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSearchField : public CObject
{
public:
	CString			m_FieldName;
	SqlRecordItem*	m_pRecItem;
	CDMSCategory*	m_pCategory;
	BOOL			m_bSelected;
	CStringArray*	m_pFieldValues;

public:
	CSearchField	(SqlRecordItem* pRecItem);
	CSearchField	(CDMSCategory* pCategory);
	~CSearchField	();

public:
	BOOL			IsCategory			() const;
	CString			GetDescription		() const;
	DataObj*		GetDataObj			() const;
	CString			GetFormattedValue	() const;
	CStringArray*	GetFieldValues		();
};

///////////////////////////////////////////////////////////////////////////////
//								CSearchFieldList definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSearchFieldList : public ::Array
{
private:
	CDMSCategories* m_pCategories;

public:
	CSearchFieldList	(SqlRecord* pRecord, CDMSCategories* pCategories);
	~CSearchFieldList	();

public:
	CSearchField* GetAt					(int nIdx) const { return (CSearchField*)Array::GetAt(nIdx); }
	CSearchField* GetSearchFieldByName	(const CString fieldName) const;

	int Add			(CSearchField* field);
	void SelectAll	(BOOL bSelect);
	void SetSelected(const CString fieldName, BOOL bSelect);
};

///////////////////////////////////////////////////////////////////////////////
//						VBookmark definition
//		SqlVirtualRecord per mappare le righe del DBT dei bookmarks
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VBookmark : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VBookmark)

public:
	DataStr  l_FieldName;
	DataStr	 l_Description;
	DataStr	 l_FormattedValue;
	DataEnum l_GroupType;	

public:
	VBookmark(BOOL bCallInit = TRUE);

public:
	virtual void BindRecord	();

public:
	void AssignMGroupType	(int groupType);
	int  GetMGroupType		() const;
	BOOL IsNotEditableField	() const;

public:
	static LPCTSTR   GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//								DBTBookmarks definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT DBTBookmarks : public DBTSlaveBuffered
{
	DECLARE_DYNAMIC(DBTBookmarks)

public:
	CSearchFieldList*	m_pSearchFields;

public:
	DBTBookmarks(CRuntimeClass*	pClass, CAbstractFormDoc* pDocument);
	~DBTBookmarks();

public:
	VBookmark*	GetRecordByFieldName(const CString& strFieldName) const;
	VBookmark*	GetBookmarkRow		() const { return (VBookmark*)GetRecord(); }
	VBookmark*	GetBookmarkRow		(int nRow) const { return (VBookmark*)GetRow(nRow); }

	void		LoadFromBookmarkDT	(DMSAttachmentInfo* pAttachmentInfo);

public:
	virtual void	OnDisableControlsForEdit();

protected:
	// Gestiscono la query
	virtual	void		OnDefineQuery		() {};
	virtual	void		OnPrepareQuery		() {};
	virtual DataObj*	OnCheckPrimaryKey	(int nRow, SqlRecord*);
	virtual DataObj*	GetDuplicateKeyPos	(SqlRecord* pRec);
	virtual CString		GetDuplicateKeyMsg	(SqlRecord* pRec);
	virtual BOOL		OnBeforeDeleteRow	(int nRow);
};

///////////////////////////////////////////////////////////////////////////////
//						VSettings definition
//		SqlVirtualRecord per mappare i settings del DMS
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VSettings : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VSettings)

public:
	DataLng	  	l_WorkerID;
	DataLng   	l_SettingType;

	//AttachmentDeleteOptions
	DataEnum  	l_DeletingAttachmentAction;

	//DuplicateDocOptions
	DataEnum  	l_DuplicateActionForDoc;
	DataEnum  	l_DuplicateActionForBatch;
	
	//GenericOptions (ex-BookmarksOptions)
	DataBool	l_EnableBookmarkEmptyValues;
	DataInt		l_DpiQualityImage;

	//SecurityOptions (ex-RepositoryOptions)
	DataBool	l_EnableToAttachFromRepository;
	DataBool	l_ShowOnlyMyArchivedDocs;

	//BarcodeOptions (ex -BarcodeDetectionOptions)
	DataBool	l_EnableBarcode;
	DataBool	l_AutomaticBarcodeDetection;
	DataEnum	l_BarcodeDetectionAction;
	DataEnum	l_BCActionForDocument;
	DataEnum	l_BCActionForBatch;
	DataEnum	l_BarcodeType;
	DataStr		l_BarcodePrefix;
	DataBool	l_PrintBarcodeInReport;

	//FTSOptions
	DataBool	l_EnableFTS;
	DataBool	l_FTSNotConsiderPdF; // x ottimizzare lo spazio l'utente può decidere di non applicare il Full-Text Search ai documenti con estensione pdf ed image. 
									  //Questo per evitare di salvare nella tabella DMS_ArchivedDocTextContent il testo estrapolato mediante OCR

	//SOSOptions
	DataBool	l_EnableSOS;
	DataInt		l_MaxElementsInEnvelope;
	DataStr		l_ExcludedExtensions;
	DataBool	l_DisableAttachFromReport;

	//StorageOptions (only for OFM)
	DataBool	l_StorageToFileSystem;
	DataStr		l_StorageFolderPath;

	//AttachmentPanelOptions
	DataInt		l_MaxDocNumber;

public:
	VSettings(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR  GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//						VExtensionMaxSize definition
//	SqlVirtualRecord per mappare l'elenco delle dimensioni ammesse per estensione
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VExtensionMaxSize : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VExtensionMaxSize)

public:
	DataStr   	l_Extension;	//k
	DataInt		l_MaxSize;

public:
	VExtensionMaxSize(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR  GetStaticName();
};

//////////////////////////////////////////////////////////////////////////////
//						    CDMSSettings
// classe di appoggio per la gestione dei parametri del DMS
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CDMSSettings : public CObject
{
	DECLARE_DYNCREATE(CDMSSettings)

private:
	VSettings*		m_pRecSettings;
	RecordArray*	m_pExtensionMaxSizeArray; // array di SqlRecord di tipo VExtensionMaxSize

public:
	CDMSSettings();
	~CDMSSettings();

public:
	VSettings*	GetSettings		() { return m_pRecSettings; }
	RecordArray* GetExtensions	() { return m_pExtensionMaxSizeArray; }

public:
	CDMSSettings& operator = (const CDMSSettings& settings);
};

///////////////////////////////////////////////////////////////////////////////
//						VCategoryValues definition
//			SqlVirtualRecord per mappare i valori di una categoria
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VCategoryValues : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VCategoryValues)

public:
	DataStr		l_Name;
	DataStr		l_Value;
	DataBool	l_IsDefault;
	DataBool	l_Checked;

public:
	VCategoryValues(BOOL bCallInit = TRUE);

public:
	virtual void	BindRecord();

public:
	static LPCTSTR  GetStaticName();
};

////////////////////////////////////////////////////////////////////////////////
////             class CDMSCategoryNode definition                    
////////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CDMSCategoryNode : public CObject
{
	DECLARE_DYNCREATE(CDMSCategoryNode)

public:
	CString				m_strKey;
	CString				m_strText;
	CDMSCategory*		m_pDMSCategory;
	VCategoryValues*	m_pCatValue;
	CString				m_strParentKey;

public:
	CDMSCategoryNode();
	CDMSCategoryNode(CDMSCategory*pDMSCategory, const CString& strParentKey);
	CDMSCategoryNode(VCategoryValues* pCatValue, const CString& strParentKey);

	BOOL IsCategory() const { return m_pDMSCategory != NULL; }
};

////////////////////////////////////////////////////////////////////////////////
////             class CDMSCategoriesTreeViewAdv definition                    
////////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CDMSCategoriesTreeViewAdv : public CTreeViewAdvCtrl
{
	DECLARE_DYNCREATE(CDMSCategoriesTreeViewAdv)

protected:
	::Array*	m_pAllNodes;
	BOOL		m_bTreeViewLoaded;
	BOOL		m_bUseCheckBox;

public:
	CDMSCategoriesTreeViewAdv();
	~CDMSCategoriesTreeViewAdv();

public:
	void SetAllNodes(::Array* pAllNode);
	void Load		();

	CDMSCategoryNode* GetSelectedTreeNode	();
	CDMSCategoryNode* GetNodeByKey			(const CString& strKey);

	virtual void Enable(const BOOL bValue = TRUE) { CTreeViewAdvCtrl::Enable(bValue); }

protected:
	virtual void OnInitControl();
};

///////////////////////////////////////////////////////////////////////////////
//						VArchivedDocument definition
//	SqlVirtualRecord per i documenti archiviati da mostrare nel Repository Explorer/Browser
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VArchivedDocument : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VArchivedDocument)

private:
	DMSAttachmentInfo*	m_pAttachmentInfo;
	::Array*			m_pAttachmentLinks; // array di VAttachmentLink

public:
	DataBool	l_IsSelected;
	DataLng		l_ArchivedDocId;
	DataStr		l_IsAttachmentBmp;
	DataBool	l_IsAttachment;
	DataStr		l_IsWoormReportBmp;
	DataBool	l_IsWoormReport;
	DataStr		l_CheckOutWorkerBmp;
	DataStr		l_CheckOutWorker;
	DataStr		l_Name;
	DataStr		l_Description;
	DataStr		l_Worker;
	DataDate	l_CreationDate; // TbCreated archiviato
	DataDate	l_ModifiedDate; // TbModified archiviato

public:
	VArchivedDocument(BOOL bCallInit = TRUE);
	~VArchivedDocument();

public:
	virtual void	BindRecord();

public:
	DMSAttachmentInfo*	GetAttachmentInfo();
	::Array*			GetAttachmentLinks();

public:
	static LPCTSTR   GetStaticName();
};

///////////////////////////////////////////////////////////////////////////////
//						VAttachmentLink definition
// per visualizzare le righe degli allegati di un documento archiviato
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT VAttachmentLink : public SqlVirtualRecord
{
	DECLARE_DYNCREATE(VAttachmentLink)

public:
	DataStr		l_Image;
	DataLng		l_ArchivedDocId;
	DataLng		l_AttachmentId;
	DataStr		l_TBPrimaryKey;
	DataStr		l_TBDocNamespace;
	// solo per la visualizzazione
	DataStr		l_DocumentDescription;
	DataStr		l_DocKeyDescription;

public:
	VAttachmentLink	(BOOL bCallInit = TRUE);
	~VAttachmentLink();

public:
	virtual void	BindRecord();

public:
	static LPCTSTR   GetStaticName();
};

//////////////////////////////////////////////////////////////////////////////
//						    CBookmarkTypeItemSource
// contiene l'elenco dei tipi di bookmark ammessi (chiave, binding, category, etc)
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CBookmarkTypeItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CBookmarkTypeItemSource)

private:
	BOOL m_bShowBinding;

public:
	CBookmarkTypeItemSource();

public:
	void ShowBindingType(BOOL bShow)   { m_bShowBinding = bShow; }

protected:
	virtual BOOL IsValidItem(const DataObj&);
};

//////////////////////////////////////////////////////////////////////////////
//						    CSearchFieldsItemSource
//contiene l'elenco dei campi del DBTMaster che possono essere inseriti come chiave di ricerca
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSearchFieldsItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CSearchFieldsItemSource)

public:
	CSearchFieldsItemSource();

private:
	CSearchFieldList*	m_pSearchFields;
	BOOL				m_bCategory;

protected:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);

public:
	void SetSearchFieldList	(CSearchFieldList* pSearchFields);
	void SetFilterCategory	()	{ m_bCategory = TRUE; }
	void SetFilterBinding	()	{ m_bCategory = FALSE; }
};

//////////////////////////////////////////////////////////////////////////////
//						CSearchFieldValuesItemSource
// contiene l'elenco dei valori corrispondenti al bookmark selezionato
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CSearchFieldValuesItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CSearchFieldValuesItemSource)

private:
	CStringArray* m_pSearchFieldValues;

public:
	CSearchFieldValuesItemSource();

public:
	virtual void GetData(DataArray& values, CStringArray& descriptions, CString strCurrentValue);

public:
	void SetSearchFieldValuesList(CStringArray* pSearchFieldValues);
};

//////////////////////////////////////////////////////////////////////////////
//						    CTBDMSViewerCtrl
//			Control per visualizzare l'anteprima dell'allegato
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CTBDMSViewerCtrl : public CTBPicViewerAdvCtrl
{
	DECLARE_DYNCREATE(CTBDMSViewerCtrl)

public:
	virtual	void SetValue(const DataObj& aValue);
};

//////////////////////////////////////////////////////////////////////////////
//						  CTBDMSBarcodeViewerCtrl
//			Control per visualizzare l'anteprima del barcode
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CTBDMSBarcodeViewerCtrl : public CTBPicViewerAdvCtrl
{
	DECLARE_DYNCREATE(CTBDMSBarcodeViewerCtrl)

private:
	DataStr m_BarcodeValue;
	DataStr m_BarcodeType;

public:
	virtual	void SetValue(const DataObj& aValue);

	void SetBarcode(DataStr aBarcodeValue, DataStr aBarcodeType) { m_BarcodeValue = aBarcodeValue; m_BarcodeType = aBarcodeType; }
};

/////////////////////////////////////////////////////////////////////////////
//					class CBookmarksBodyEdit definition						/
//			BodyEdit per la visualizzazione dell'elenco dei bookmarks		/
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CBookmarksBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CBookmarksBodyEdit)

public:
	CSearchFieldsItemSource*		m_pSearchFieldsItemSource;
	CSearchFieldValuesItemSource*	m_pSearchFieldValuesItemSource;

public:
	CBookmarksBodyEdit();

public:
	VBookmark* GetBookmarkRow		() { return (VBookmark*)m_pDBT->GetRecord(); }
	VBookmark* GetCurrentBookmarkRow() { return (VBookmark*)m_pDBT->GetCurrentRow(); }
	
	void SetOnlyCategoryField();
	void OnBookmarkRowChanged();

private:
	void OnBookmarkTypeChanged	();
	void OnSearchFieldChanged	();

public:
	virtual void Customize();
	virtual	BOOL OnCommand(WPARAM wParam, LPARAM lParam);
};

//////////////////////////////////////////////////////////////////////////////
//			class CAttachmentLinksBodyEdit definition						//
// BodyEdit per la visualizzazione dei link ai documenti gestionali, con	//
// possibilita' di aprire il dataentry passando il namespace + PK			//
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentLinksBodyEdit : public CJsonBodyEdit
{
	DECLARE_DYNCREATE(CAttachmentLinksBodyEdit)

public:
	CAttachmentLinksBodyEdit();

private:
	BOOL	m_bMouseInside;
	HCURSOR	m_hOldCursor;
	HCURSOR	m_hHandCursor;

public:
	void OpenERPDocument(DataStr documentNamespace, DataStr documentKey);

public:
	virtual void Customize();
	virtual BOOL OnGetCustomColor(CBodyEditRowSelected* pCurrentRow);

	//{{AFX_MSG(CAttachmentLinksBodyEdit)
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnMouseMove(UINT nFlags, CPoint point);
	//}}AFX_MSG

	DECLARE_MESSAGE_MAP()
};

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
// contiene l'elenco dei tipi di bookmark ammessi (chiave, binding, category, etc)
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentBookmarkTypeCombo : public CEnumCombo, public ResizableCtrl
{
	DECLARE_DYNCREATE(CAttachmentBookmarkTypeCombo)

private:
	BOOL m_bShowBinding;

public:
	CAttachmentBookmarkTypeCombo();

public:
	void ShowBindingType(BOOL bShow) { m_bShowBinding = bShow; }

protected:
	BOOL	IsValidItemListBox(const DataObj&);
};

//////////////////////////////////////////////////////////////////////////////
//						    CAttachmentSearchFieldsCombo
//contiene l'elenco dei campi del DBTMaster che possono essere inseriti come chiave di ricerca
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentSearchFieldsCombo : public CStrCombo, public ResizableCtrl
{
	DECLARE_DYNCREATE(CAttachmentSearchFieldsCombo)

public:
	CAttachmentSearchFieldsCombo();

private:
	CSearchFieldList* m_pSearchFields;
	BOOL m_bCategory;

public:
	virtual void OnFillListBox();

public:
	void SetSearchFieldList(CSearchFieldList* pSearchFields);
	void SetFilterCategory() { m_bCategory = TRUE; }
	void SetFilterBinding() { m_bCategory = FALSE; }
};

//////////////////////////////////////////////////////////////////////////////
//						CAttachmentSearchFieldValuesCombo
// contiene l'elenco dei valori corrispondenti al bookmark selezionato
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentSearchFieldValuesCombo : public CStrCombo, public ResizableCtrl
{
	DECLARE_DYNCREATE(CAttachmentSearchFieldValuesCombo)

private:
	CStringArray* m_pSearchFieldValues;

public:
	CAttachmentSearchFieldValuesCombo();

public:
	virtual void OnFillListBox();

public:
	void SetSearchFieldValuesList(CStringArray* pSearchFieldValues);
};

/////////////////////////////////////////////////////////////////////////////
//			class CAttachmentBookmarksBodyEdit definition						
//	BodyEdit per la visualizzazione dell'elenco dei bookmarks nel panel degli Allegati
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class TB_EXPORT CAttachmentBookmarksBodyEdit : public CBodyEdit
{
	DECLARE_DYNCREATE(CAttachmentBookmarksBodyEdit)

public:
	CAttachmentBookmarkTypeCombo*		m_pBookmarkTypeCombo;
	CAttachmentSearchFieldsCombo*		m_pSearchFieldsCombo;
	CAttachmentSearchFieldValuesCombo*	m_pSearchFieldValuesCombo;

public:
	CAttachmentBookmarksBodyEdit();

public:
	VBookmark* GetBookmarkRow		()	{ return (VBookmark*)m_pDBT->GetRecord(); }
	VBookmark* GetCurrentBookmarkRow() { return (VBookmark*)m_pDBT->GetCurrentRow(); }

	void SetOnlyCategoryField();
	void OnBookmarkRowChanged();

private:
	void OnBookmarkTypeChanged();
	void OnSearchFieldChanged();

public:
	virtual void	Customize();
	virtual	BOOL	OnCommand(WPARAM wParam, LPARAM lParam);
};

#include "endh.dex"
