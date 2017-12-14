#pragma once

#include <TbGeneric\DataObj.h>
//includere alla fine degli include del .H
#include "beginh.dex"




//corrisponde all'enum managed AttachmentFilterType presente in EasyAttachment\Components\CommonObjects.cs
enum AttachmentFilterTypeEnum { OnlyAttachment, OnlyPapery, Both, OnlyMainDoc, OnlyForMail };


//corrisponde all'enum managed ArchiveResult presente in EasyAttachment\Components\CommonObjects.cs
enum ArchiveResult { TerminatedSuccess, TerminatedWithError, Cancel };

enum DMSEventTypeEnum { NewDMSCollection, UpdateDMSCollection, NewDMSAttachment, DeleteDMSAttachment };

enum SearchLocationEnum
{
	None = 0x0000,
	All = 0x0001,
	Tags = 0x0002,
	AllBookmarks = 0x0004,
	NameAndDescription = 0x008,
	Barcode = 0x0010,
	Content = 0x0020
};


///////////////////////////////////////////////////////////////////////////////
//					CTypedBarcode definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CTypedBarcode
{
public:
	CString		m_strBarcodeValue;
	CString		m_strBarcodeType;

public:
	CTypedBarcode();
	CTypedBarcode(CString barcodeValue, CString barcodeType);

public:
	void Clear();

public:
	// operators
	CTypedBarcode&	operator =	(const CTypedBarcode& attachInfo);
};


///////////////////////////////////////////////////////////////////////////////
//								CAttachmentInfo definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CAttachmentInfo : public CObject
{
	DECLARE_DYNCREATE(CAttachmentInfo)
public:
	CAttachmentInfo();

public:
	bool			b_LateBinding;
	DataLng			m_attachmentID;
	DataLng			m_ArchivedDocId;
	DataLng			m_CollectionId;
	DataLng			m_ErpDocumentId;
	DataLng			m_Size;
	DataStr			m_Name;
	DataStr			m_Description;
	DataStr			m_OriginalPath;
	DataStr			m_ExtensionType;
	DataDate		m_ArchivedDate;
	DataDate		m_AttachedDate;
	DataDate		m_ModifiedDate;
	DataStr			m_ModifiedBy;
	DataStr			m_CreatedBy;
	DataBool		m_IsAPapery;
	DataStr			m_ERPDocNamespace;
	DataStr			m_ERPPrimaryKeyValue;
	CTypedBarcode	m_Barcode;
	DataStr			m_StorageFile; //se vuoto il binario è salvato su DB
	DataStr			m_TemporaryPathFile;
	DataStr			m_FreeTag;
	DataStr			m_CurrentCheckOutWorker; //contiene il nome il worker che sta modificando il documento
	DataBool		m_IsWoormReport;
	DataBool		m_IsMainDoc;
	DataBool		m_IsForMail;
public:
	virtual void Clear();

protected:
	void Assign(const CAttachmentInfo& item);

public:
	// operators
	virtual CAttachmentInfo& operator =	(const CAttachmentInfo& attachInfo);
};



//tipizzo l'array degli attachmentinfo così i programmatori (vedi Germano) 
//non si devono preoccupare di effettuare la delete del contenuto delle celle
///////////////////////////////////////////////////////////////////////////////
//								CAttachmentsArray definition
///////////////////////////////////////////////////////////////////////////////
class TB_EXPORT CAttachmentsArray : public Array
{
public:
	CAttachmentsArray() {};
	virtual ~CAttachmentsArray();

public:
	CAttachmentInfo* GetAt(int nIdx) const { return (CAttachmentInfo*)Array::GetAt(nIdx); }
	void Add(CAttachmentInfo* att) { Array::Add((CObject*)att); }

	CAttachmentInfo* GetAttachmentByID(int attachmentID) const;
};




TB_EXPORT CAttachmentInfo* GetAttachmentInfo(int attachmentId);



#include "endh.dex"