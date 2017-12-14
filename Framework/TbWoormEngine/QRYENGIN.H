
#pragma once

#include "RepField.h"
#include "datafunc.h"
#include "rulengin.h"

//includere alla fine degli include del .H
#include "beginh.dex"

//============================================================================
class QueryEngine;
class CTBContext;

//============================================================================
class TB_EXPORT QueryEngine : public RuleEngine
{
	friend  class CRSTreeCtrl;

	DECLARE_DYNAMIC(QueryEngine);

private:
	BOOL						m_bFirstTuple = TRUE;
	GroupBy*					m_pGroupingTupleEngine = NULL;
	ExpressionWithDebugBlocks*	m_pHavingTupleFilterEngine = NULL;

public:
	QueryEngine (WoormTable&, EngineScheduler&);
	~QueryEngine ();

protected:
	virtual BOOL	QueryProcessing	() = 0;

			BOOL	ExecQuery		(const Array* arOrderOnThisColumns = NULL, CString* pstrAuxWhereClause = NULL);
			BOOL	Compile			(Parser&, CTBContext*);
			virtual BOOL	ResolveQueries	(CString& sErr);
			BOOL	BuildTree		(Parser&);
private:
			BOOL	TupleProcessing	();
			void	CopyData		();
			BOOL	PutData			();
			BOOL	GroupInMem		();

			BOOL	ParseQueryRules	(Parser&, CTBContext*);
			BOOL	ParseGroupBy	(Parser&);
			BOOL	ParseHavingFilter(Parser&);
			BOOL	ParseTupleFilter(Parser&);
};

//---------------------------------------------------------------------------
#include "endh.dex"

