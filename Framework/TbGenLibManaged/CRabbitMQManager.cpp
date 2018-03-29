#include "stdafx.h"

// For WinHttp
#include <Windows.h>
#include <WinHttp.h>
#include <stdio.h>
#include <iostream>
#include <fstream>

#include "CRabbitMQManager.h"
#pragma comment(lib, "winhttp.lib")


#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace Microarea::TaskBuilderNet::Core::RabbitMQManager;
using namespace System;




//-----------------------------------------------------------------------------
BOOL CRabbitMQManager::PublishRabbitMQMessage(CString message, CString queueName)
{
	
	CString hostname = _T("localhost");
	int		port = 5672;

	return Microarea::TaskBuilderNet::Core::RabbitMQManager::RabbitMQManager::PublishMessage(gcnew String(hostname), port, gcnew String(queueName), gcnew String(message));
}