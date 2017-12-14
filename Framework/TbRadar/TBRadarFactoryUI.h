#pragma once

#include "radardoc.h"
#include "radarfrm.h"
#include "radarvw.h"
#include "WrmRdrdoc.h"
#include "WrmRdrfrm.h"
#include "WrmRdrvw.h"

#include "WrmMaker.h"
#include "beginh.dex"

//=============================================================================
class TB_EXPORT CTBRadarFactoryUI : public CTBRadarFactory
{
	DECLARE_DYNAMIC(CTBRadarFactoryUI)
public:
	CTBRadarFactoryUI() {}

	virtual ITBRadar* CreateInstance(HotKeyLink*, SqlTable*, SqlRecord*, HotKeyLinkObj::SelectionType);
	virtual ITBRadar* CreateInstance(CAbstractFormDoc*) ;
	virtual ITBRadar* CreateInstance(CAbstractFormDoc*, const CString&, BOOL = FALSE);
	virtual CString BuildWoormRadar(CAbstractFormDoc*);
};

#include "endh.dex"