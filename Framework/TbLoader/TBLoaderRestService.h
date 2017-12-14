#pragma once
using namespace System::Net;
using namespace Microarea::TaskBuilderNet::Core::SoapCall;

public ref class CTBLoaderRestService : IRestService
{
public:
	CTBLoaderRestService();

	virtual bool ProcessRequest(HttpListenerRequest^ request, HttpListenerResponse^ response);
	
};