#include "StdAfx.h"
#include ".\sourcesafeitemcollection.h"
#include ".\sourcesafeitem.h"

//--------------------------------------------------------------------------------
CSourceSafeItemCollection::CSourceSafeItemCollection()
{
}

//--------------------------------------------------------------------------------
CSourceSafeItemCollection::CSourceSafeItemCollection(const CSourceSafeItemCollection& sourceArray)
{
	for (int i = 0; i < sourceArray.array.GetCount(); i++)
		array.Add(sourceArray.array[i]);
}

//--------------------------------------------------------------------------------
CSourceSafeItemCollection CSourceSafeItemCollection::operator = (const CSourceSafeItemCollection& sourceArray)
{
	array.RemoveAll();

	for (int i = 0; i < sourceArray.array.GetCount(); i++)
		array.Add(sourceArray.array[i]);
	return *this;
}

//--------------------------------------------------------------------------------
void CSourceSafeItemCollection::Add(CSourceSafeItem item)
{
	array.Add(item);
}