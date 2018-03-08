#pragma once

class SqlRecord;
class RecordArray;

class DBTJsonCache
{
	int m_nStart = 0;
	int m_nCount = 0; 
	DBTSlaveBuffered* m_pDBT;

	RecordArray* m_pClientRecords;
	int m_nCurrentRow = -2;//non -1, perché così la prima volta ne forzo la serializzazione
	int m_nRowCount = -1;
	int m_nRowsSent = -1;
	Bool3 m_bReadonly;
public:
	DBTJsonCache(DBTSlaveBuffered* pDBT);
	~DBTJsonCache();

	bool IsModified();
	void GetJsonPatch(CJsonSerializer& jsonSerializer);
	bool SetJson(CJsonParser& jsonParser);
	void SetJsonLimits(int nRowFrom, int nCount);
};

