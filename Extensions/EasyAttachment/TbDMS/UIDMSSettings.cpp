#include "stdafx.h"

#include <TbGenlib\BarcodeEnums.h>

#include "TBDMSEnums.h"
#include "UIDMSSettings.h"

/////////////////////////////////////////////////////////////////////////////
//					class CDMSSettingsView Implementation
/////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CDMSSettingsView, CJsonFormView)

//-----------------------------------------------------------------------------
CDMSSettingsView::CDMSSettingsView()
{
}

//////////////////////////////////////////////////////////////////////////////
//				    CAttachOptionForDocItemSource
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachOptionForDocItemSource, CItemSource)

//-----------------------------------------------------------------------------
BOOL CAttachOptionForDocItemSource::IsValidItem(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if ((DataEnum&)aDataObj == DataEnum(E_DUPLICATE_CANCEL))
		return FALSE;
	else
		return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//					   CAttachOptionForBatchItemSource
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CAttachOptionForBatchItemSource, CItemSource)

//-----------------------------------------------------------------------------
BOOL CAttachOptionForBatchItemSource::IsValidItem(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if ((DataEnum&)aDataObj == DataEnum(E_DUPLICATE_ASK) || (DataEnum&)aDataObj == DataEnum(E_DUPLICATE_CANCEL))
		return FALSE;
	else
		return TRUE;
}

//////////////////////////////////////////////////////////////////////////////
//						    CBCOptionForDocItemSource
//////////////////////////////////////////////////////////////////////////////
//=============================================================================
IMPLEMENT_DYNCREATE(CBCOptionForDocItemSource, CItemSource)

//-----------------------------------------------------------------------------
BOOL CBCOptionForDocItemSource::IsValidItem(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if ((DataEnum&)aDataObj == DataEnum(E_DUPLICATE_ASK) ||
		(DataEnum&)aDataObj == DataEnum(E_DUPLICATE_REPLACE) ||
		(DataEnum&)aDataObj == DataEnum(E_DUPLICATE_KEEP_BOTH) ||
		(DataEnum&)aDataObj == DataEnum(E_DUPLICATE_CANCEL))
		return TRUE;
	else
		return FALSE;
}

//////////////////////////////////////////////////////////////////////////////
//					    CBCOptionForBatchItemSource
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBCOptionForBatchItemSource, CItemSource)

//-----------------------------------------------------------------------------
BOOL CBCOptionForBatchItemSource::IsValidItem(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if ((DataEnum&)aDataObj == DataEnum(E_DUPLICATE_REPLACE) ||
		(DataEnum&)aDataObj == DataEnum(E_DUPLICATE_KEEP_BOTH) ||
		(DataEnum&)aDataObj == DataEnum(E_DUPLICATE_CANCEL))
		return TRUE;
	else
		return FALSE;
}

//////////////////////////////////////////////////////////////////////////////
//						    CBCTypeItemSource
//////////////////////////////////////////////////////////////////////////////
//-----------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CBCTypeItemSource, CItemSource)

//-----------------------------------------------------------------------------
BOOL CBCTypeItemSource::IsValidItem(const DataObj& aDataObj)
{
	if (!aDataObj.IsKindOf(RUNTIME_CLASS(DataEnum)))
	{
		ASSERT(FALSE);
		return FALSE;
	}

	if (
		(DataEnum&)aDataObj == DataEnum(E_BARCODE_TYPE_ALFA39)		  ||
		(DataEnum&)aDataObj == DataEnum(E_BARCODE_TYPE_CODE_128_AUTO) ||
		(DataEnum&)aDataObj == DataEnum(E_BARCODE_TYPE_EAN128)		/*  ||
		(DataEnum&)aDataObj == DataEnum(E_BARCODE_TYPE_DATAMATRIX)	  ||
		(DataEnum&)aDataObj == DataEnum(E_BARCODE_TYPE_MICROQR)		  ||
		(DataEnum&)aDataObj == DataEnum(E_BARCODE_TYPE_QR)			  ||
		(DataEnum&)aDataObj == DataEnum(E_BARCODE_TYPE_PDF417)*/
		)		  
		return TRUE;
	else
		return FALSE;
}
