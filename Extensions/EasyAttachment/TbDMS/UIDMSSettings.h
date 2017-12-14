#pragma once

#include <TbGenlib\parsctrl.h>
#include <TbGes\ExtDocView.h>
#include <TbGes\ItemSource.h>

#include  "beginh.dex"

///////////////////////////////////////////////////////////////////////////////
//					class CDMSSettingsView Definition
///////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CDMSSettingsView : public CJsonFormView
{
	DECLARE_DYNCREATE(CDMSSettingsView)

public:
	CDMSSettingsView();
};

/////////////////////////////////////////////////////////////////////////////
// CAttachOptionForDocItemSource
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachOptionForDocItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CAttachOptionForDocItemSource)

protected:
	virtual BOOL IsValidItem(const DataObj&);
};

/////////////////////////////////////////////////////////////////////////////
// CAttachOptionForBatchItemSource
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CAttachOptionForBatchItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CAttachOptionForBatchItemSource)

protected:
	virtual BOOL IsValidItem(const DataObj&);
};

/////////////////////////////////////////////////////////////////////////////
// CBCOptionForDocItemSource 
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
//=============================================================================
class CBCOptionForDocItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CBCOptionForDocItemSource)

protected:
	virtual BOOL IsValidItem(const DataObj&);
}; 

/////////////////////////////////////////////////////////////////////////////
// CBCOptionForBatchItemSource
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CBCOptionForBatchItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CBCOptionForBatchItemSource)

protected:
	virtual BOOL IsValidItem(const DataObj& aDataObj);
};

/////////////////////////////////////////////////////////////////////////////
// CBCTypeItemSource 
/////////////////////////////////////////////////////////////////////////////
//=============================================================================
class CBCTypeItemSource : public CItemSource
{
	DECLARE_DYNCREATE(CBCTypeItemSource)

protected:
	virtual BOOL IsValidItem(const DataObj& aDataObj);
};

#include  "endh.dex"
