#include "StdAfx.h"

#include <TBGeneric\globals.h>
#include <TBGeneric\FunctionCall.h>
#include <TbGeneric\EnumsTable.h>

#include <TbNameSolver\FileSystemFunctions.h>

#include "StaticFunctions.h"

using namespace System;

//-----------------------------------------------------------------------------
System::Object^ ConverDataObj(DataObj* pObj)
{
	Object^ o;// = Microarea::TaskBuilderNet::Core::CoreTypes::ObjectHelper::CreateObject(gcnew String(pObj->GetDataType().ToString()));
		
	switch (pObj->GetDataType().m_wType)
	{
		case DATA_STR_TYPE:
		case DATA_TXT_TYPE:
		case DATA_GUID_TYPE:
			o = gcnew String(pObj->Str());
			break;

		case DATA_MON_TYPE:
		case DATA_QTA_TYPE:
		case DATA_PERC_TYPE:
		case DATA_DBL_TYPE:
			o = (double) *dynamic_cast<DataDbl*>(pObj);
			break;

		case DATA_INT_TYPE:
			o = (short) *dynamic_cast<DataInt*>(pObj);
			break;

		case DATA_LNG_TYPE:
		{
			if (pObj->IsATime())			
			{
				double el = dynamic_cast<DataLng*>(pObj)->GetTotalSeconds();
				System::TimeSpan ts = System::TimeSpan::FromSeconds(el);
				o = ts;
			}
			else
			{
				long l = (long)*dynamic_cast<DataLng*>(pObj);
				o = l;
			}
			break;
		}

		case DATA_DATE_TYPE:
			{
				if (pObj->IsATime())	//TODO TimeSpan ?
				{
					DataDate* dd= dynamic_cast<DataDate*>(pObj);
					
					System::TimeSpan ts = System::TimeSpan(dd->Hour(), dd->Minute(), dd->Second());
					o = ts;
				}
				else
				{
					DataDate* pD = dynamic_cast<DataDate*>(pObj);
					
					if (pD->IsFullDate())
						o = gcnew System::DateTime(pD->Year(), pD->Month(), pD->Day(), pD->Hour(), pD->Minute(), pD->Second());
					else
						o = gcnew System::DateTime(pD->Year(), pD->Month(), pD->Day());
				}
			}
			break;

		case DATA_BOOL_TYPE:
			{
				DataBool* pB = dynamic_cast<DataBool*>(pObj);
				o = pB ? true : false;
			}
			break;

		case DATA_ENUM_TYPE:
			{
				DataEnum* pE = dynamic_cast<DataEnum*>(pObj);
				o = gcnew String(AfxGetEnumsTable()->GetEnumItemTitle (pE->GetValue()));
			}
			break;

		default:
			{
				TRACE1("Data type unsupported %s", pObj->GetDataType().ToString());
				ASSERT(FALSE);
			}
	}
	return o;
}

//-----------------------------------------------------------------------------
String^ StaticFunctions::GetFileFromJsonFileId(String ^ jsonFileId)
{
	CJsonResource res(jsonFileId);
	return gcnew String(res.GetFile());
}