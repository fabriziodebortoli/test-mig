#pragma once

#include <afxtempl.h>

#include "beginh.dex"

class CSourceSafeItem;
typedef CArray<CSourceSafeItem> CSourceSafeItemCollectionTempl;

//================================================================================
class TB_EXPORT CSourceSafeItemCollection
{
	CSourceSafeItemCollectionTempl array;

public:
	CSourceSafeItemCollection();
	CSourceSafeItemCollection(const CSourceSafeItemCollection& sourceArray);

	CSourceSafeItemCollection operator = (const CSourceSafeItemCollection& sourceArray);

	void Add(CSourceSafeItem item);
};

#include "endh.dex"