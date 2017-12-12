#include "StdAfx.h"
#include ".\globalfunctions.h"
#include ".\sourcesafeitem.h"
#include ".\sourcesafeitemcollection.h"
#include ".\SourceControlWrapper.h"

#using <mscorlib.dll>

using namespace Microarea::Library::SourceControl;

SourceSafeDBWrapper* GetDatabaseObject(long handle)
{
	return dynamic_cast<SourceSafeDBWrapper*>(SourceControlWrapper::ObjectRepository::GetAt(handle));
}


CSourceSafeItem ConvertItem(SourceSafeItem* item)
{
	long handle = SourceControlWrapper::ObjectRepository::Add(item);
	return CSourceSafeItem(handle);
}


CSourceSafeItem ConvertItem(Object* item)
{
	long handle = SourceControlWrapper::ObjectRepository::Add(item);
	return CSourceSafeItem(handle);
}

CSourceSafeItemCollection ConvertItem(SourceSafeItemCollection* collection)
{
	CSourceSafeItemCollection ar;
	
	IEnumerator *enumerator = collection->GetEnumerator();
	
	while (enumerator->MoveNext())
		ar.Add(ConvertItem(enumerator->Current));
	
	return ar;
}


SourceSafeItem* GetItemObject(long handle)
{
	return dynamic_cast<SourceSafeItem*>(SourceControlWrapper::ObjectRepository::GetAt(handle));
}
