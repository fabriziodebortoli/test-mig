#pragma once

//includere alla fine degli include del .H
#include "beginh.dex"

class SqlRecord;
class SqlTable;
class DBTSlaveBuffered;
class CTBGridControl;
class RecordArray;

//=============================================================================
class TB_EXPORT TBGridControlDataSource : public CObject
{
protected:
	TBGridControlDataSource(CTBGridControl* pTBGridControl);
public:
	virtual SqlRecord*	GetPrototypeRecord	() = 0;
	virtual void		Reload				() = 0;
	virtual SqlRecord*	GetRecordAt			(int nRow) = 0;

protected:
	CTBGridControl*			m_pTBGridControl;
};

//=============================================================================
class TB_EXPORT DBTSlaveBufferedGridDataSource : public TBGridControlDataSource
{
private:
	DBTSlaveBuffered*		m_pDBT;

public:
	DBTSlaveBufferedGridDataSource(DBTSlaveBuffered* pDBT, CTBGridControl* pTBGridControl);

	virtual SqlRecord*	GetPrototypeRecord	();
	virtual void		Reload				();
	virtual SqlRecord*	GetRecordAt			(int nRow)	{ return NULL; }
};

//=============================================================================
class TB_EXPORT SqlTableGridDataSource : public TBGridControlDataSource
{
private:
	SqlTable*			m_pSqlTable;

public:
	SqlTableGridDataSource(SqlTable* pSqlTable, CTBGridControl* pTBGridControl);

	virtual SqlRecord*	GetPrototypeRecord	();
	virtual void		Reload				();
	virtual SqlRecord*	GetRecordAt			(int nRow)	{ return NULL; }
};

//=============================================================================
class TB_EXPORT RecordArrayGridDataSource : public TBGridControlDataSource
{
private:
	RecordArray*		m_pRecordArray;

public:
	RecordArrayGridDataSource(RecordArray* pRecordArray, CTBGridControl* pTBGridControl);

	virtual SqlRecord*	GetPrototypeRecord	();
	virtual void		Reload				();
	virtual SqlRecord*	GetRecordAt			(int nRow);
};

#include "endh.dex"
