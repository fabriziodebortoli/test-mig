#pragma once

#include <TbGes\EXTDOC.H>
 

//=============================================================================
class CNewDocument : public CDynamicFormDoc
{
	DECLARE_DYNCREATE			(CNewDocument)

public:
	CNewDocument				();
	virtual ~CNewDocument		();
	virtual	void OnFrameCreated	();
	BOOL InitDocument			();
	DECLARE_MESSAGE_MAP			()
};

//=============================================================================
class CNewBatchDocument : public CNewDocument
{
	DECLARE_DYNCREATE			(CNewBatchDocument)
public:
	CNewBatchDocument()		{ m_bBatch = TRUE; }
};
