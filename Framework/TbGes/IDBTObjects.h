#pragma once

#include <TbOledb\sqltable.h>
#include "beginh.dex"

class DBTSlave;
////////////////////////////////////////////////////////////////////////////////
//				class(interface) IDBTObject definition
////////////////////////////////////////////////////////////////////////////////
//===========================================================================
class TB_EXPORT IDBTObject
{
public:
	virtual ~IDBTObject(){}
	virtual void 	Init			()= 0;

	virtual void	OnEnableControlsForFind		() = 0;
	virtual void	OnDisableControlsForEdit	() = 0;

	virtual	void	OnDefineQuery				() = 0;
	virtual	void	OnPrepareQuery				() = 0;
	virtual	BOOL	OnOkTransaction				() = 0;
	virtual void	OnDisableControlsForAddNew	() = 0;
	virtual void	OnDisableControlsAlways		() = 0;

	virtual	BOOL	OnCheckPrimaryKey			() = 0;
	virtual	void	OnPreparePrimaryKey			() = 0;
};

////////////////////////////////////////////////////////////////////////////////
//				class(interface) IDBTMaster definition
////////////////////////////////////////////////////////////////////////////////
//===========================================================================
class TB_EXPORT IDBTMaster : virtual public IDBTObject
{
public:
	virtual	void	OnPrepareBrowser	(SqlTable*) = 0;
	virtual void	OnPrepareFindQuery	(SqlTable*) = 0;

	virtual	void	OnPrepareForXImportExport(SqlTable*) = 0;
	virtual void 	OnBeforeXMLExport	() = 0;
	virtual void 	OnAfterXMLExport	() = 0;
	virtual BOOL 	OnOkXMLExport		() = 0;

	virtual void 	OnBeforeXMLImport	() = 0;
	virtual void 	OnAfterXMLImport	() = 0;
	virtual BOOL 	OnOkXMLImport		() = 0;
};


////////////////////////////////////////////////////////////////////////////////
//				class(interface) IDBTSlave definition
////////////////////////////////////////////////////////////////////////////////
//===========================================================================
class TB_EXPORT IDBTSlave : virtual public IDBTObject
{
public:
	virtual BOOL	IsEmptyData	() = 0;
};


////////////////////////////////////////////////////////////////////////////////
//				class(interface) IDBTSlaveBuffered definition
////////////////////////////////////////////////////////////////////////////////
//===========================================================================
class TB_EXPORT IDBTSlaveBuffered : virtual public IDBTSlave
{
public:
	virtual DataObj*	OnCheckPrimaryKey	(int /*nRow*/, SqlRecord*) = 0;
	virtual void		OnPreparePrimaryKey	(int /*nRow*/, SqlRecord*) = 0;

	virtual void		OnPrepareAuxColumns		(SqlRecord*) = 0;
	virtual void		OnPrepareOldAuxColumns	(SqlRecord*) = 0;

	virtual void		OnRecordAdded		(SqlRecord*, int nRow) = 0;
	
	virtual	void		OnSetCurrentRow		()	 = 0;
	virtual void		OnPrepareRow		(int /*nRow*/, SqlRecord*) = 0;

	virtual BOOL		OnBeforeAddRow		(int /*nRow*/) = 0;
	virtual void		OnAfterAddRow		(int /*nRow*/, SqlRecord*) = 0;

	virtual BOOL		OnBeforeInsertRow	(int /*nRow*/) = 0;
	virtual void		OnAfterInsertRow	(int /*nRow*/, SqlRecord*) = 0;

	virtual BOOL		OnBeforeDeleteRow	(int /*nRow*/) = 0;
	virtual void		OnAfterDeleteRow	(int /*nRow*/) = 0;
	
	virtual	DataObj*	GetDuplicateKeyPos	(SqlRecord*) = 0;
	virtual	CString		GetDuplicateKeyMsg	(SqlRecord*) = 0;

	virtual DataObj*	OnDefaultIsDuplicateKey(SqlRecord* pRecord, int nRow) = 0;
	virtual void		OnRemovingRecord(SqlRecord* pRecord, int nRow) = 0;

	virtual void		OnBeforeLocalFindData() = 0;
	virtual void		OnAfterLocalFindData() = 0;
	virtual void		OnCreatedDBTSlave(DBTSlave* pSlave) = 0;
};

