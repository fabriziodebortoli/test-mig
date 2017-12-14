#pragma once

#define _TBNAME(name) _TB##name
class CRequestBag;
template <class KEY, class ARG_KEY, class VALUE, class ARG_VALUE>
class TMap : public CMap<KEY, ARG_KEY, VALUE, ARG_VALUE>
{
public:
	~TMap()
	{
		POSITION pos =  GetStartPosition();
		ARG_KEY key;
		ARG_VALUE pVal;
		while (pos)
		{
			GetNextAssoc(pos, key, pVal);
			delete pVal;
		}
	}
};

typedef CMap<int, int, CRequestBag*, CRequestBag*> RequestMap;
typedef CMap<CString, LPCTSTR, LONG, LONG> BaseUnitsMap;
#include "TBDC.h"
#include "TBWnd.h"
#include "TBMenu.h"
#include "TBImageList.h"
#include "TBMessageQueue.h"
#include "TBTimer.h"

#define DECLARE_GLOBAL_MAP(type)\
	type& Get_##type##();\
	CCriticalSection* Get_##type##Section();\
	LONG Get_New_##type##();

DECLARE_GLOBAL_MAP(CWindowMap)//mappa handle finestra
DECLARE_GLOBAL_MAP(CDeferMap)//mappa handle deferwindowpos
DECLARE_GLOBAL_MAP(CQueueMap)//mappa coda messaggi
DECLARE_GLOBAL_MAP(CHDCToTBDCMap)
DECLARE_GLOBAL_MAP(CImageListMap)
DECLARE_GLOBAL_MAP(CMenuMap)
DECLARE_GLOBAL_MAP(CImageMap)
DECLARE_GLOBAL_MAP(RequestMap)
DECLARE_GLOBAL_MAP(BaseUnitsMap)//mappa font/baseunits

#define THE_MONITOR (HMONITOR)0x17 //handle fittizio dell'unico monitor a disposizione
#define MONITOR_WIDTH 2560
#define MONITOR_HEIGHT 1600
CHGDIOBJMap& GetGDIMap(); 

class CHookedFunctionArray;
bool UseStandardAPIS();
CHookedFunctionArray& GetHooks();
HHOOK GetNewHOOK();
void GetBaseUnits(int &baseUnitX, int& baseUnitY, LPCTSTR szFontFace, int wPoint);

extern int BASE_UNIT_X;
extern int BASE_UNIT_Y;

//#define HOOK_IMAGES
