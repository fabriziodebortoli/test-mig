#pragma once
#include "BusinessServiceProvider.h"

class CAbstractFormDoc;

//-----------------------------------------------------------------------------
class CJsonModelGenerator : public CBusinessServiceProviderObj
{
private:
	CMasterFrame* m_pFrame = NULL;
	DataInt m_NrOfDocuments = 10;
public:
	
	CJsonModelGenerator(CAbstractFormDoc* pDoc);
	void Generate();
	DECLARE_TB_EVENT_MAP();
	DECLARE_DYNAMIC(CJsonModelGenerator);
	~CJsonModelGenerator();
private:
	void OnGenerate();	
};
