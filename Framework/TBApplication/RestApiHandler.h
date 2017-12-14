#pragma once


#include <tbgenlib\CEFClasses.h>

class CTbRequestHandler;
//classe che gestisce la chiamate rest usate per il prototipo mobile
class CRestApiHandler : public CTBRequestHandlerObj
{
	CTbRequestHandler* m_pRequestHandler = NULL;
public:
	CRestApiHandler(CTbRequestHandler* pRequestHandler);
	~CRestApiHandler();

	void ProcessRequest(const CString& path, const CNameValueCollection& params, CTBResponse& response);
};

