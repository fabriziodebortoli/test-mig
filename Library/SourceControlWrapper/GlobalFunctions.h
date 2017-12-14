#pragma once


using namespace Microarea::Library::SourceControl;
using namespace System;
using namespace System::Collections;

class CSourceSafeItem;
class CSourceSafeItemCollection;

SourceSafeDBWrapper* GetDatabaseObject(long handle);
CSourceSafeItem ConvertItem(SourceSafeItem* item);
CSourceSafeItem ConvertItem(Object* item);
CSourceSafeItemCollection ConvertItem(SourceSafeItemCollection* collection);
SourceSafeItem* GetItemObject(long handle);

