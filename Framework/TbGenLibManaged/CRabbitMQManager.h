#pragma once

#include <TbNameSolver\InterfaceClasses.h>
//NOW INCLUDED IN COMMON PCH: #include <TbGeneric\DataObj.h>

#include "beginh.dex"
class TB_EXPORT CRabbitMQManager : public IRabbitMQ
{
public:
	//-----------------------------------------------------------------------------
	CRabbitMQManager:: CRabbitMQManager()
		:
		IRabbitMQ()
	{
	}
	//-----------------------------------------------------------------------------
	virtual BOOL PublishRabbitMQMessage(CString message, CString queueName);
};

#include "endh.dex"

