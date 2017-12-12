#pragma once

//===========================================================================
//							CUserControlHandlerObjMan
//===========================================================================
class CUserControlHandlerObjMan : public CUserControlHandlerObj
{
public:
	virtual Control^ GetWinControl () = 0;
};

