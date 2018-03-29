#pragma once
#include "tbresourcelocker.h"
#include "beginh.dex"

class TB_EXPORT IMenuIcon
{
public:
	virtual BOOL OnDrawMenuImage(CDC* pDC,
		const CBCGPToolbarMenuButton* pMenuButton,
		const CRect& rectImage) = 0;
};

class TB_EXPORT IRabbitMQ
{
public:
	virtual BOOL PublishRabbitMQMessage(CString message, CString queueName) = 0;
};



#include "endh.dex"