#include "stdafx.h"

#include "afxwinforms.h"
#include "atlimage.h"

#include <TbNameSolver\FileSystemFunctions.h>
#include <TbGeneric\Globals.h>
#include <TbGeneric\TBThemeManager.h>
#include <TbGeneric\FontsTable.h>
#include <TbGenlib\Generic.h>

#include "GanttWrapper.h"

#include "UserControlHandlers.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

using namespace AGCSN;

//===========================================================================
//									Errors
//===========================================================================
static const TCHAR szGanttNotInitialized []						= _T("Gantt control not initialized. Cannot call requested method");

static const TCHAR szActiveGanttColumnsStyle []					= _T("Columns");
static const TCHAR szActiveGanttRowsStyle []					= _T("Rows");
static const TCHAR szActiveGanttTasksStyle[]					= _T("Tasks");
static const TCHAR szActiveGanttMilestoneStyle[]				= _T("Milestones");
static const TCHAR szActiveGanttDefaultLayer[]					= _T("0");
static const TCHAR szDefaultStyle[]								= _T("DefaultStyle");
static const TCHAR szActiveGanttDefaultMoveableObjectsLayer[]	= _T("MoveableObjectsLayer");
static const TCHAR szActiveGanttDefaultUpperTierStyle[]			= _T("UpperTier");
static const TCHAR szActiveGanttDefaultMiddleTierStyle[]		= _T("MiddleTier");
static const TCHAR szActiveGanttDefaultLowerTierStyle[]			= _T("LowerTier");

//===========================================================================
//							GanttEventsHandler
//===========================================================================
IMPLEMENT_DYNAMIC (GanttEventArgs, UnmanagedEventsArgs)
//---------------------------------------------------------------------------------------
GanttEventArgs::GanttEventArgs (const CString& sError)
	:
	UnmanagedEventsArgs (sError)
{
}

//---------------------------------------------------------------------------------------
GanttEventArgs::GanttEventArgs (EventTarget eventTarget, int nObjectIndex /*-1*/, int nParentIndex /*-1*/)
	:
	UnmanagedEventsArgs		(_T("")),
	m_EventTarget			(eventTarget),
	m_nObjectIndex			(nObjectIndex),
	m_nParentObjectIndex	(nParentIndex)
{
}

//===========================================================================
//							GanttEventsHandler
//===========================================================================
ref class GanttEventsHandler : public ManagedEventsHandlerObj
{	
public:
	//---------------------------------------------------------------------------------------
	GanttEventsHandler () 
	{
	}
	
	//---------------------------------------------------------------------------------------
	GanttEventArgs::EventTarget DecodeEventTarget (AGCSN::E_EVENTTARGET eventTarget)
	{
		switch (eventTarget)
		{
			case E_EVENTTARGET::EVT_CELL:					return GanttEventArgs::EVT_CELL;
			case AGCSN::E_EVENTTARGET::EVT_CLIENTAREA:		return GanttEventArgs::EVT_CLIENTAREA;
			case AGCSN::E_EVENTTARGET::EVT_COLUMN:			return GanttEventArgs::EVT_COLUMN;
			case AGCSN::E_EVENTTARGET::EVT_EMPTYAREA:		return GanttEventArgs::EVT_EMPTYAREA;
			case AGCSN::E_EVENTTARGET::EVT_MILESTONE:		return GanttEventArgs::EVT_MILESTONE;
			case AGCSN::E_EVENTTARGET::EVT_PERCENTAGE:		return GanttEventArgs::EVT_PERCENTAGE;
			case AGCSN::E_EVENTTARGET::EVT_ROW:				return GanttEventArgs::EVT_ROW;
			case AGCSN::E_EVENTTARGET::EVT_TASK:			return GanttEventArgs::EVT_TASK;
			case AGCSN::E_EVENTTARGET::EVT_TIMEBLOCK:		return GanttEventArgs::EVT_TIMEBLOCK;
			case AGCSN::E_EVENTTARGET::EVT_TIMELINE:		return GanttEventArgs::EVT_TIMELINE;
			default:										return GanttEventArgs::EVT_NONE;
		}
	}

	//---------------------------------------------------------------------------------------
	AGCSN::E_EVENTTARGET CodeEventTarget (GanttEventArgs::EventTarget eventTarget)
	{
		switch (eventTarget)
		{
			case GanttEventArgs::EVT_CELL:				return AGCSN::E_EVENTTARGET::EVT_CELL;
			case GanttEventArgs::EVT_CLIENTAREA:  		return AGCSN::E_EVENTTARGET::EVT_CLIENTAREA;
			case GanttEventArgs::EVT_COLUMN:			return AGCSN::E_EVENTTARGET::EVT_COLUMN;
			case GanttEventArgs::EVT_EMPTYAREA:			return AGCSN::E_EVENTTARGET::EVT_EMPTYAREA;
			case GanttEventArgs::EVT_MILESTONE:			return AGCSN::E_EVENTTARGET::EVT_MILESTONE;
			case GanttEventArgs::EVT_PERCENTAGE:		return AGCSN::E_EVENTTARGET::EVT_PERCENTAGE;
			case GanttEventArgs::EVT_ROW:				return AGCSN::E_EVENTTARGET::EVT_ROW;
			case GanttEventArgs::EVT_TASK:				return AGCSN::E_EVENTTARGET::EVT_TASK;
			case GanttEventArgs::EVT_TIMEBLOCK:			return AGCSN::E_EVENTTARGET::EVT_TIMEBLOCK;
			case GanttEventArgs::EVT_TIMELINE:			return AGCSN::E_EVENTTARGET::EVT_TIMELINE;
			default:									return AGCSN::E_EVENTTARGET::EVT_NONE;
		}
	}

	//---------------------------------------------------------------------------------------
	virtual void MapEvents (Object^ pControl) override
	{
		ActiveGanttCSNCtl^ pGanttControl = (ActiveGanttCSNCtl^) pControl;

		pGanttControl->HandleDestroyed		+= gcnew EventHandler (this, &GanttEventsHandler::HandleDestroyed);
		pGanttControl->ActiveGanttError		+= gcnew ActiveGanttErrorEventHandler (this, &GanttEventsHandler::OnError);
		pGanttControl->TimeLineChanged		+= gcnew TimeLineChangedEventHandler(this, &GanttEventsHandler::TimeLineChanged);
		pGanttControl->BeginObjectMove		+= gcnew BeginObjectMoveEventHandler(this, &GanttEventsHandler::BeginObjectMove);
		pGanttControl->EndObjectMove		+= gcnew EndObjectMoveEventHandler(this, &GanttEventsHandler::EndObjectMove);
		pGanttControl->BeginObjectSize		+= gcnew BeginObjectSizeEventHandler(this, &GanttEventsHandler::BeginObjectSize);
		pGanttControl->EndObjectSize		+= gcnew EndObjectSizeEventHandler(this, &GanttEventsHandler::EndObjectSize);
		pGanttControl->ObjectAdded			+= gcnew ObjectAddedEventHandler(this, &GanttEventsHandler::ObjectAdded);
		pGanttControl->ObjectSelected		+= gcnew ObjectSelectedEventHandler(this, &GanttEventsHandler::ObjectSelected);
		pGanttControl->ObjectMove			+= gcnew ObjectMoveEventHandler(this, &GanttEventsHandler::ObjectMove);
		pGanttControl->ObjectSize			+= gcnew ObjectSizeEventHandler(this, &GanttEventsHandler::ObjectSize);
		pGanttControl->ControlScroll		+= gcnew ControlScrollEventHandler(this, &GanttEventsHandler::ControlScroll);

		pGanttControl->CustomTierDraw		+= gcnew CustomTierDrawEventHandler(this, &GanttEventsHandler::CustomTierDraw);
		pGanttControl->CompleteObjectMove	+= gcnew CompleteObjectMoveEventHandler(this, &GanttEventsHandler::CompleteObjectMove);

		pGanttControl->MouseDoubleClick		+= gcnew System::Windows::Forms::MouseEventHandler(this, &GanttEventsHandler::MouseDoubleClick);
		
	}

	//---------------------------------------------------------------------------------------
	void TimeLineChanged (Object^ sender) 
	{
		SendAsControl(UM_GANTT_TIMELINE_CHANGED);
	}

	//---------------------------------------------------------------------------------------
	void GanttEventsHandler::CustomTierDraw (Object^ sender, CustomTierDrawEventArgs^ eventArgs)
	{
		int nMessage = 0;

		//System::DayOfWeek dStartDay = ((DateTime^)eventArgs->StartDate)->DayOfWeek;
		//System::Globalization::CultureInfo^ cu = ((ActiveGanttCSNCtl^)sender)->Culture;
		//System::DayOfWeek dStartWeekDay = cu->DateTimeFormat->FirstDayOfWeek;
		//int diff = (7 - (int)(dStartDay - dStartWeekDay));

		if (eventArgs->Interval == "12h")
			eventArgs->Text = ((ActiveGanttCSNCtl^)sender)->StrLib->StrFormat(eventArgs->StartDate, "tt")->ToUpper();
		if (eventArgs->Interval == "1ww")
		{
			//eventArgs->StartDate = ((DateTime^)eventArgs->StartDate)->AddDays(diff);
			eventArgs->Text = ((ActiveGanttCSNCtl^)sender)->StrLib->StrFormat(eventArgs->StartDate, "ddd d");
			//eventArgs->Text = ((ActiveGanttCSNCtl^)sender)->StrLib->StrFormat(((DateTime^)eventArgs->StartDate)->AddDays(-diff), "ddd d");
		}
		if (eventArgs->Interval == "1m")
		{
			eventArgs->Text = ((ActiveGanttCSNCtl^)sender)->StrLib->StrFormat(eventArgs->StartDate, "MMMM yyyy");
		}
		if (eventArgs->Interval == "1yyyy")
		{
			eventArgs->Text = ((ActiveGanttCSNCtl^)sender)->StrLib->StrFormat(eventArgs->StartDate, "yyyy");
		}
		if (eventArgs->Interval == "1q")
		{
			eventArgs->Text = ((ActiveGanttCSNCtl^)sender)->MathLib->GetYear(eventArgs->StartDate) + " Q" + ((ActiveGanttCSNCtl^)sender)->MathLib->GetQuarter(eventArgs->StartDate);
		}
		if (eventArgs->Interval == "1d")
		{
			String^ sDate = ((ActiveGanttCSNCtl^)sender)->StrLib->StrFormat(eventArgs->StartDate, "ddd d");
			eventArgs->Text = sDate->Substring(sDate->Length - 2);
		}
		switch (eventArgs->TierPosition)
		{
		case AGCSN::E_TIERPOSITION::SP_UPPER:
			nMessage = UM_GANTT_UPPER_TIER;
			eventArgs->StyleIndex = gcnew String(szActiveGanttDefaultUpperTierStyle);
			break;
		case AGCSN::E_TIERPOSITION::SP_MIDDLE:
			nMessage = UM_GANTT_MIDDLE_TIER;
			eventArgs->StyleIndex = gcnew String(szActiveGanttDefaultMiddleTierStyle);
			break;
		case AGCSN::E_TIERPOSITION::SP_LOWER:
			nMessage = UM_GANTT_LOWER_TIER;
			eventArgs->StyleIndex = gcnew String(szActiveGanttDefaultLowerTierStyle);
			break;
		}
		SendAsControl(nMessage);
	}

	//---------------------------------------------------------------------------------------
	void OnError (Object^ sender, ErrorEventArgs^ eventArgs) 
	{
		//CString s;
		int nMess = 0;

		//GanttEventArgs aEventArgs (s);

		nMess = UM_GANTT_ERROR;
		//SendAsMessage(UM_GANTT_ERROR, aEventArgs);
		SendAsControl(nMess);
	
	}

	//----------------------------------------------------------------------------------------
	void MouseDoubleClick(Object^ sender, System::Windows::Forms::MouseEventArgs^ eventArgs)
	{
		ActiveGanttCSNCtl^ ganttUC = (ActiveGanttCSNCtl^)sender;

		String^ taskIdx		= ganttUC->MathLib->GetTaskIndexByPosition(eventArgs->X, eventArgs->Y).ToString();
		String^ rowIdx		= ganttUC->MathLib->GetRowIndexByPosition(eventArgs->Y).ToString();
		String^ colIdx;
		//manage column
		if (eventArgs->X < ganttUC->Splitter->Position)
		{
			int widthTot = 0;
			for (int i = 1; i <= ganttUC->Columns->Count; i++)
			{
				clsColumn^ oColumn = ganttUC->Columns->Item(i.ToString());
				widthTot += oColumn->Width;

				if (eventArgs->X <= widthTot)
				{
					colIdx = i.ToString();
					break;
				}
			}

			ganttUC->Tag	= taskIdx + _T("#") + rowIdx + _T("#") + colIdx;
		}
		else
			ganttUC->Tag	= taskIdx + _T("#") + rowIdx;

		SendAsControl(UM_GANTT_MOUSE_DOUBLE_CLICK);
	}

	//---------------------------------------------------------------------------------------
	void BeginObjectMove (Object^ sender, ObjectStateChangedEventArgs^ eventArgs) 
	{
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->Index);
		((ActiveGanttCSNCtl^)sender)->Tag = ((ActiveGanttCSNCtl^)sender)->Tasks->Item(eventArgs->Index.ToString())->Key;
		
		nMess = UM_GANTT_BEGIN_OBJECT_MOVE;
		//SendAsMessage(UM_GANTT_BEGIN_OBJECT_MOVE, aEventArgs);
		SendAsControl(nMess);
	}

	//---------------------------------------------------------------------------------------
	void EndObjectMove (Object^ sender, ObjectStateChangedEventArgs^ eventArgs) 
	{
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->Index);
		
		nMess = UM_GANTT_END_OBJECT_MOVE;
		//SendAsMessage(UM_GANTT_END_OBJECT_MOVE, aEventArgs);
		SendAsControl(nMess);
	}

	//---------------------------------------------------------------------------------------
	void CompleteObjectMove(Object^ sender, ObjectStateChangedEventArgs^ eventArgs) 
	{
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->Index);
		
		nMess = UM_GANTT_COMPLETE_OBJECT_MOVE;
		//SendAsMessage(UM_GANTT_COMPLETE_OBJECT_MOVE, aEventArgs);
		SendAsControl(nMess);
	}

	//---------------------------------------------------------------------------------------
	void BeginObjectSize (Object^ sender, ObjectStateChangedEventArgs^ eventArgs) 
	{
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->Index);

		nMess = UM_GANTT_BEGIN_OBJECT_SIZE;
		//SendAsMessage(UM_GANTT_BEGIN_OBJECT_SIZE, aEventArgs);
		SendAsControl(nMess);
	}

	//---------------------------------------------------------------------------------------
	void EndObjectSize (Object^ sender, ObjectStateChangedEventArgs^ eventArgs) 
	{
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->Index);

		nMess = UM_GANTT_END_OBJECT_SIZE;
		//SendAsMessage(UM_GANTT_END_OBJECT_SIZE, aEventArgs);
		SendAsControl(nMess);
	}

	//---------------------------------------------------------------------------------------
	void ObjectAdded (Object^ sender, ObjectAddedEventArgs^ eventArgs)
    {
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->TaskIndex);

		nMess = UM_GANTT_OBJECT_ADDED;
		//SendAsMessage(UM_GANTT_OBJECT_ADDED, aEventArgs);
		SendAsControl(nMess);
    }

	//---------------------------------------------------------------------------------------
    void ObjectSelected (Object^ sender, ObjectSelectedEventArgs^ eventArgs)
    {
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->ObjectIndex, eventArgs->ParentObjectIndex);

		nMess = UM_GANTT_OBJECT_SELECTED;
		//SendAsMessage(UM_GANTT_OBJECT_SELECTED, aEventArgs);
		SendAsControl(nMess);
    }

	//---------------------------------------------------------------------------------------
	void GanttEventsHandler::ObjectMove (Object^ sender, ObjectStateChangedEventArgs^ eventArgs)
    {
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->Index);

		nMess = UM_GANTT_OBJECT_MOVE;
		//SendAsMessage(UM_GANTT_OBJECT_MOVE, aEventArgs);
		SendAsControl(nMess);
    }

	//---------------------------------------------------------------------------------------
	void GanttEventsHandler::ObjectSize (Object^ sender, ObjectStateChangedEventArgs^ eventArgs)
    {
		int nMess = 0;

		GanttEventArgs aEventArgs (DecodeEventTarget(eventArgs->EventTarget), eventArgs->Index);

		nMess = UM_GANTT_OBJECT_SIZE;
		//SendAsMessage(UM_GANTT_OBJECT_SIZE, aEventArgs);
		SendAsControl(nMess);
    }

	//---------------------------------------------------------------------------------------
	void GanttEventsHandler::ControlScroll (Object^ sender, AGCSN::ScrollEventArgs^ eventArgs)
    {
		int nMess = 0;
		switch (eventArgs->ScrollBarType)
		{
			case AGCSN::E_SCROLLBAR::SCR_HORIZONTAL2:	nMess = UM_GANTT_SCROLL_HORIZONTAL2;	break;
			case AGCSN::E_SCROLLBAR::SCR_VERTICAL:		nMess = UM_GANTT_SCROLL_VERTICAL;		break;
			default:									nMess = UM_GANTT_SCROLL_HORIZONTAL1;	break;
		}
		
		SendAsControl(nMess);
	}
};

//===========================================================================
// macro in order to identify Control used
//===========================================================================

#define ENSURE_GANTT_CONTROL(r) ENSURE_USER_CONTROL(r, ActiveGanttCSNCtl, ganttUC, szGanttNotInitialized)

#define VOID_ENSURE_GANTT_CONTROL() VOID_ENSURE_USER_CONTROL(ActiveGanttCSNCtl, ganttUC, szGanttNotInitialized)

#define DECLARE_GANTT_HANDLER() DECLARE_CTRL_HANDLER(ActiveGanttCSNCtl, pGanttHandler) 

//===========================================================================
//								CGanttWrapper
//===========================================================================

ActiveGanttCSNCtl^ GetUserControl(CUserControlWrapperObj* pWrapper)
{
	if (pWrapper == NULL || pWrapper->GetHandler() == NULL)
											{
		ASSERT_TRACE (FALSE, _T("Managed User Control not initialized!"));
		return nullptr;
											}
	CUserControlHandler<ActiveGanttCSNCtl>* p =  (CUserControlHandler<ActiveGanttCSNCtl>*) pWrapper->GetHandler();
	return p->GetControl();
}
//---------------------------------------------------------------------------------------
CGanttWrapper::CGanttWrapper()
{
	m_pManHandler = new CUserControlHandler<ActiveGanttCSNCtl>(gcnew GanttEventsHandler());
}
//---------------------------------------------------------------------------------------
CGanttWrapper::~CGanttWrapper()
{
	delete m_pManHandler;
}
//---------------------------------------------------------------------------------------
void CGanttWrapper::OnInitControl ()
{
	InitDefaultValues ();
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::InitDefaultValues ()
{
	SetTimeBlockBehaviour	(TB_CONTROL_EXTENTS);
	SetBorderStyle			(BS_3D);
	SetDefaultFont			();
	SetAllowSplitterMove	(TRUE);
	SetAllowAdd				(FALSE);
	SetDefaultAddMode		();
	SetDefaultLayers		();
	SetDefaultStyle			();
	SetCulture				(AfxGetCulture());
	SetAllowColumnMove		(TRUE);
	SetAllowColumnSize		(TRUE);
	SetAllowEdit			(TRUE);
	SetAllowHeaderSize		(TRUE);
	SetAllowPredecessorAdd	(TRUE);
	SetAllowRowMove			(FALSE);
	SetAllowRowSize			(TRUE);
	SetDefaultErrorReports	();
}

//---------------------------------------------------------------------------------------
CString CGanttWrapper::GetTag()
{
	ENSURE_GANTT_CONTROL(_T(""))
	
	return ganttUC->Tag->ToString();
}

//----------------------------------------------------------------------------------------
CString CGanttWrapper::GetTaskKey(const CString& taskIndex)
{
	ENSURE_GANTT_CONTROL(_T(""))

	String^ sTaskIndex = gcnew String(taskIndex);

	if (!ExistsObject(taskIndex))
		return _T("");

	return ganttUC->Tasks->Item(sTaskIndex)->Key;
}

//----------------------------------------------------------------------------------------------
CString CGanttWrapper::GetRowKey(const CString& rowIndex)
{
	ENSURE_GANTT_CONTROL(_T(""))

	String^ sRowIndex = gcnew String(rowIndex);

	if (!ExistsRow(rowIndex))
		return _T("");

	return ganttUC->Rows->Item(sRowIndex)->Key;
}

//----------------------------------------------------------------------------------------------
CString CGanttWrapper::GetColumnKey(const CString& colIndex)
{
	ENSURE_GANTT_CONTROL(_T(""))

	String^ sColIndex = gcnew String(colIndex);

	if (!ExistsColumn(colIndex))
		return _T("");

	return ganttUC->Columns->Item(sColIndex)->Key;
}

//---------------------------------------------------------------------------------------
CString CGanttWrapper::GetObjectRowKey (const CString& sObjectKey)
{
	ENSURE_GANTT_CONTROL(_T(""))
	
	String^ key = gcnew String(sObjectKey);
	
	return ganttUC->Tasks->Item(key)->RowKey;
}

//---------------------------------------------------------------------------------------
DataDate CGanttWrapper::GetObjectStartDate (const CString& sObjectKey)
{
	DataDate dDate = MIN_GIULIAN_DATE; 
	dDate.SetFullDate ();

	ENSURE_GANTT_CONTROL(dDate)

	if (!ExistsObject(sObjectKey))
		return dDate;

	String^ objectKey = gcnew String(sObjectKey);
	clsTask^ task = ganttUC->Tasks->Item(objectKey);
	DateTime^ myDate = task->StartDate;

	if (task != nullptr)
	{
		dDate.SetDate(((System::DateTime^)myDate)->Day, ((System::DateTime^)myDate)->Month, ((System::DateTime^)myDate)->Year);
		dDate.SetTime(((System::DateTime^)myDate)->Hour, ((System::DateTime^)myDate)->Minute, ((System::DateTime^)myDate)->Second);
	}
	
	return dDate;
}

//---------------------------------------------------------------------------------------
DataDate CGanttWrapper::GetObjectEndDate(const CString& sObjectKey)
{
	DataDate dDate = MIN_GIULIAN_DATE; 
	dDate.SetFullDate ();
	
	ENSURE_GANTT_CONTROL(dDate)
	
	if (!ExistsObject(sObjectKey))
		return dDate;

	String^ objectKey = gcnew String(sObjectKey);
	clsTask^ task = ganttUC->Tasks->Item(objectKey);

	DateTime^ myDate = task->EndDate;

	if (task != nullptr)
	{
		dDate.SetDate(((System::DateTime^)myDate)->Day, ((System::DateTime^)myDate)->Month, ((System::DateTime^)myDate)->Year);
		dDate.SetTime(((System::DateTime^)myDate)->Hour, ((System::DateTime^)myDate)->Minute, ((System::DateTime^)myDate)->Second);
	}
	
	return dDate;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::Enable (BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->Enabled = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
BOOL CGanttWrapper::ExistsStyle (const CString& sStyleKey)
{
	ENSURE_GANTT_CONTROL (FALSE)

	if (ganttUC->Styles == nullptr)
		return FALSE;

	String^ requestedKey = gcnew String(sStyleKey);

	E_REPORTERRORS oldErrValue = ganttUC->ErrorReports;
	
	if (ganttUC->ErrorReports == E_REPORTERRORS::RE_MSGBOX)
		ganttUC->ErrorReports = E_REPORTERRORS::RE_RAISEEVENT;

	clsStyle^ style = ganttUC->Styles->Item(requestedKey);

	ganttUC->ErrorReports = oldErrValue;
	
	return style != nullptr;
}

//---------------------------------------------------------------------------------------
BOOL CGanttWrapper::ExistsObject (const CString& sTaskKey)
{
	ENSURE_GANTT_CONTROL (FALSE)

	if (ganttUC->Tasks == nullptr)
		return FALSE;

	String^ requestedKey = gcnew String(sTaskKey);

	E_REPORTERRORS oldErrValue = ganttUC->ErrorReports;
	
	if (ganttUC->ErrorReports == E_REPORTERRORS::RE_MSGBOX)
		ganttUC->ErrorReports = E_REPORTERRORS::RE_RAISEEVENT;

	clsTask^ task = ganttUC->Tasks->Item(requestedKey);

	ganttUC->ErrorReports = oldErrValue;
	
	return task != nullptr;
}

//---------------------------------------------------------------------------------------
BOOL CGanttWrapper::ExistsRow (const CString& sRowKey)
{
	ENSURE_GANTT_CONTROL (FALSE)

	if (ganttUC->Rows == nullptr)
		return FALSE;

	String^ requestedKey = gcnew String(sRowKey);

	E_REPORTERRORS oldErrValue = ganttUC->ErrorReports;
	
	if (ganttUC->ErrorReports == E_REPORTERRORS::RE_MSGBOX)
		ganttUC->ErrorReports = E_REPORTERRORS::RE_RAISEEVENT;

	clsRow^ row = ganttUC->Rows->Item(requestedKey);

	ganttUC->ErrorReports = oldErrValue;

	return row != nullptr;
}

//---------------------------------------------------------------------------------------
BOOL CGanttWrapper::ExistsColumn (const CString& sTextKey)
{
	ENSURE_GANTT_CONTROL (FALSE)

	if (ganttUC->Columns == nullptr)
		return FALSE;

	String^ requestedKey = gcnew String(sTextKey);

	for (int i=1; i<=ganttUC->Columns->Count; i++)
	{
		CString sConv = cwsprintf(_T("%d"), i); 
		String^ strConv = gcnew String(sConv);
		clsColumn^ cColumn = ganttUC->Columns->Item(strConv);
		if (cColumn->Text->ToLower() == requestedKey->ToLower())
		{
			return TRUE;
		}
	}

	return FALSE;
}

//---------------------------------------------------------------------------------------
BOOL CGanttWrapper::ExistsLayer (const CString& sLayerKey)
{
	ENSURE_GANTT_CONTROL (FALSE)

	if (ganttUC->Layers == nullptr)
		return FALSE;

	String^ requestedKey = gcnew String(sLayerKey);

	E_REPORTERRORS oldErrValue = ganttUC->ErrorReports;
	
	if (ganttUC->ErrorReports == E_REPORTERRORS::RE_MSGBOX)
		ganttUC->ErrorReports = E_REPORTERRORS::RE_RAISEEVENT;

	clsLayer^ layer = ganttUC->Layers->Item(requestedKey);

	ganttUC->ErrorReports = oldErrValue;

	return layer != nullptr;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTierStyle	(
										const E_TIER eTier,
										const CString& sFontName, 
										const int& nFontSize, 
										const E_FONTSTYLE eFontStyle, 
										const COLORREF aBackColor, 
										const COLORREF aBorderColor, 
										const COLORREF aForeColor
									)
{
	switch (eTier)
	{
		case T_UPPER:
			AddStyle (szActiveGanttDefaultUpperTierStyle, SA_FLAT, aBackColor, aBorderColor, aForeColor);
			SetFontForStyle(sFontName, nFontSize, eFontStyle, szActiveGanttDefaultUpperTierStyle);
			break;
		case T_MIDDLE:
			AddStyle (szActiveGanttDefaultMiddleTierStyle, SA_FLAT, aBackColor, aBorderColor, aForeColor);
			SetFontForStyle(sFontName, nFontSize, eFontStyle, szActiveGanttDefaultMiddleTierStyle);
			break;
		case T_LOWER:
			AddStyle (szActiveGanttDefaultLowerTierStyle, SA_FLAT, aBackColor, aBorderColor, aForeColor);
			SetFontForStyle(sFontName, nFontSize, eFontStyle, szActiveGanttDefaultLowerTierStyle);
			break;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTierGradientStyle	(
													const E_TIER eTier,
													const CString& sFontName,
													const int& nFontSize,
													const E_FONTSTYLE eFontStyle,
													const GRE_GRADIENTFILLMODE eGradientFillMode,
													const COLORREF aStartGradientColor,
													const COLORREF aEndGradientColor,
													const COLORREF aBorderColor,
													const COLORREF aForeColor
											)
{
	switch (eTier)
	{
		case T_UPPER:
			AddStyle (szActiveGanttDefaultUpperTierStyle, SA_FLAT, eGradientFillMode, aStartGradientColor, aEndGradientColor, aBorderColor, aForeColor);
			SetFontForStyle(sFontName, nFontSize, eFontStyle, szActiveGanttDefaultUpperTierStyle);
			break;
		case T_MIDDLE:
			AddStyle (szActiveGanttDefaultMiddleTierStyle, SA_FLAT, eGradientFillMode, aStartGradientColor, aEndGradientColor, aBorderColor, aForeColor);
			SetFontForStyle(sFontName, nFontSize, eFontStyle, szActiveGanttDefaultMiddleTierStyle);
			break;
		case T_LOWER:
			AddStyle (szActiveGanttDefaultLowerTierStyle, SA_FLAT, eGradientFillMode, aStartGradientColor, aEndGradientColor, aBorderColor, aForeColor);
			SetFontForStyle(sFontName, nFontSize, eFontStyle, szActiveGanttDefaultLowerTierStyle);
			break;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetHorizontalScrollValue (const int& nValueScroll)
{
    VOID_ENSURE_GANTT_CONTROL()

    if (ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Max > ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Value)
		ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Value += nValueScroll;

    ganttUC->Redraw();
}

//---------------------------------------------------------------------------------------
int CGanttWrapper::GetHorizontalScrollValue()
{
	ENSURE_GANTT_CONTROL(0)

	return ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Value;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowAdd (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowAdd = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetDefaultAddMode ()
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AddMode = E_ADDMODE::AT_BOTH;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetDefaultErrorReports ()
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->ErrorReports = E_REPORTERRORS::RE_RAISEEVENT;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetDefaultFont ()
{
	VOID_ENSURE_GANTT_CONTROL ( )

	CFont* pControlFont = AfxGetThemeManager()->GetControlFont();
	if (!pControlFont)
	{
		ASSERT_TRACE(FALSE, _T("Cannot initialize Gantt control font!"))
		return;
	}

	DECLARE_GANTT_HANDLER ();
	//ganttUC->Font = pGanttHandler->ConvertFont (pControlFont);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowColumnMove (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowColumnMove = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowColumnSize (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowColumnSize = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowEdit (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowEdit = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowHeaderSize (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowHeaderSize = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowPredecessorAdd (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowPredecessorAdd = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowRowMove (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowRowMove = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowRowSize (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL ( )

	ganttUC->AllowRowSize = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetCulture (const CString& sCulture)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ culture = gcnew String(sCulture);

	// initialize culture
//	ganttUC->Culture = System::Globalization::CultureInfo::CreateSpecificCulture(culture);
	ganttUC->Culture = System::Globalization::CultureInfo::GetCultureInfo(culture);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowSplitterMove (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->AllowSplitterMove = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetDefaultLayers ()
{
	VOID_ENSURE_GANTT_CONTROL()

	AddLayer(szActiveGanttDefaultMoveableObjectsLayer, TRUE);

	ganttUC->CurrentLayer = gcnew String(szActiveGanttDefaultLayer);
	ganttUC->LayerEnableObjects = E_LAYEROBJECTENABLE::EC_INALLLAYERS;
	ganttUC->HorizontalScrollBar->Visible = true;

}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetDefaultStyle()
{
	VOID_ENSURE_GANTT_CONTROL()

	//set DefaultStyle - managed for task Width = 0
	AddStyle(O_TASK, szDefaultStyle, SA_FLAT, 10, 10, CLR_ACQUAMARINE, CLR_ACQUAMARINE, CLR_WHITE,0,0,0);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetAllowTimeLineScroll (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->AllowTimeLineScroll = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetBackColor (const COLORREF color)
{
	VOID_ENSURE_GANTT_CONTROL()
	DECLARE_GANTT_HANDLER ();
	
	ganttUC->BackColor = pGanttHandler->ConvertColor(color);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetBorderStyle (const E_BORDERSTYLE style)
{
	VOID_ENSURE_GANTT_CONTROL()

	switch (style)
	{
		case BS_SINGLE: 
			ganttUC->BorderStyle = AGCSN::E_BORDERSTYLE::TLB_SINGLE;
			break;
		case BS_3D: 
			ganttUC->BorderStyle = AGCSN::E_BORDERSTYLE::TLB_3D;
			break;
		default: 
			ganttUC->BorderStyle = AGCSN::E_BORDERSTYLE::TLB_NONE;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetControlMode (const E_CONTROLMODE mode)
{
	VOID_ENSURE_GANTT_CONTROL()

	if (mode == CM_TREEVIEW)
		ganttUC->ControlMode = AGCSN::E_CONTROLMODE::CM_TREEVIEW;
	else
		ganttUC->ControlMode = AGCSN::E_CONTROLMODE::CM_GRID;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetCurrentLayer (const int& nLayerIdx)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ layer = gcnew String (cwsprintf(_T("%d"), nLayerIdx));

	ganttUC->CurrentLayer = layer;
}

//----------------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineAppearance(const E_STYLEAPPEREANCE eStyleApp)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->TimeLine->Style->Appearance = (AGCSN::E_STYLEAPPEARANCE) eStyleApp;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetCurrentView (const int& nViewIdx)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ view = gcnew String (cwsprintf(_T("%d"), nViewIdx));
	ganttUC->CurrentView = view;
}

//----------------------------------------------------------------------------------------
void CGanttWrapper::SetCurrentViewInterval(const CString& sInterval)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ interval = gcnew String(sInterval);
	ganttUC->CurrentViewObject->Interval = interval;
}

//----------------------------------------------------------------------------------------
void CGanttWrapper::SetClientAreaGridInterval(const CString& sInterval)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ interval = gcnew String(sInterval);
	ganttUC->CurrentViewObject->ClientArea->Grid->Interval = interval;
}

//----------------------------------------------------------------------------------------
void CGanttWrapper::SetClientAreaGridVerticalLines(const BOOL& bValue)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->ClientArea->Grid->VerticalLines = (bValue == TRUE);
}

//----------------------------------------------------------------------------------------
void CGanttWrapper::SetClientAreaGridSnapToGrid(const BOOL& bValue)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->ClientArea->Grid->SnapToGrid = (bValue == TRUE);
}

//----------------------------------------------------------------------------------------
void CGanttWrapper::RedrawControl()
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->Redraw();
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::RemoveAllTasks()
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->Tasks->Clear();
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetDoubleBuffering (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->DoubleBuffering = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetHeaderSelected (const BOOL bValue /*TRUE*/)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->HeaderSelected = (bValue == TRUE);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetMinColumnWidth (const int& nWidth)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->MinColumnWidth = nWidth;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetMinRowHeight	(const int& nHeight)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->MinRowHeight = nHeight;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetScrollBarBehaviour (const E_SCROLLBARBEHAVIOUR behaviour)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	switch (behaviour)
	{
	case SB_HIDE:
		ganttUC->ScrollBarBehaviour = AGCSN::E_SCROLLBEHAVIOUR::SB_HIDE;
		break;
	default:
		ganttUC->ScrollBarBehaviour = AGCSN::E_SCROLLBEHAVIOUR::SB_DISABLE;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeBlockBehaviour (const E_TIMEBLOCKBEHAVIOUR behaviour)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	switch (behaviour)
	{
	case TB_ROW_EXTENTS:
		ganttUC->TimeBlockBehaviour = AGCSN::E_TIMEBLOCKBEHAVIOUR::TBB_ROWEXTENTS;
		break;
	default:
		ganttUC->TimeBlockBehaviour = AGCSN::E_TIMEBLOCKBEHAVIOUR::TBB_CONTROLEXTENTS;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SelectCellIndex	(const int& nIndex)
{
	VOID_ENSURE_GANTT_CONTROL()
	ganttUC->SelectedCellIndex = nIndex;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SelectColumnIndex (const int& nIndex)
{
	VOID_ENSURE_GANTT_CONTROL()
	ganttUC->SelectedColumnIndex = nIndex;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SelectRowIndex (const int& nIndex)
{
	VOID_ENSURE_GANTT_CONTROL()
	ganttUC->SelectedRowIndex = nIndex;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SelectTaskIndex (const int& nIndex)
{
	VOID_ENSURE_GANTT_CONTROL()
	ganttUC->SelectedTaskIndex = nIndex;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetFontForStyle (
										const CString& sFontName, 
										const int& fontSize, 
										const E_FONTSTYLE eFontStyle, 
										const CString& sStyleName
									)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ styleName = gcnew String(sStyleName);
	if (!ExistsStyle(sStyleName))
		ganttUC->Styles->Add(styleName);

	clsStyle^ style = ganttUC->Styles->Item(styleName);

	if (style == nullptr)
		return;

	System::Drawing::FontStyle fontStyle;
	switch (eFontStyle)
	{
		case F_BOLD:
			fontStyle = System::Drawing::FontStyle::Bold;
			break;
		case F_ITALIC:
			fontStyle = System::Drawing::FontStyle::Italic;
			break;
		case F_REGULAR:
			fontStyle = System::Drawing::FontStyle::Regular;
			break;
		case F_STRIKEOUT:
			fontStyle = System::Drawing::FontStyle::Strikeout;
			break;
		case F_UNDERLINE:
			fontStyle = System::Drawing::FontStyle::Underline;
			break;
	}

	String^ fontName = gcnew String(sFontName);
	style->Font = gcnew System::Drawing::Font(fontName, (float) fontSize, fontStyle);
}

//---------------------------------------------------------------------------------------
int	CGanttWrapper::AddLayer (const CString& sLayerKey, const BOOL& bVisible /*TRUE*/)
{
	ENSURE_GANTT_CONTROL(-1)

	if (ExistsLayer(sLayerKey))
		return 0;

	String^ layer = gcnew String(sLayerKey);
	ganttUC->Layers->Add (layer, bVisible == TRUE);

	return 1;
}

//--------------------------------------------------------------------------------------
void CGanttWrapper::AddStyleHatch	
							(
									const CString& sStyleName,
									const E_STYLEAPPEREANCE eStyleApp, 
									const COLORREF aBackColor,
									const COLORREF aHatchColor,
									const int& offsetTop,
									const int& offsetBottom
							)
{
	VOID_ENSURE_GANTT_CONTROL()
	DECLARE_GANTT_HANDLER ();

	String^ styleName = gcnew String (sStyleName);

	if (!ExistsStyle(sStyleName))
		ganttUC->Styles->Add(styleName);

	clsStyle^ style = ganttUC->Styles->Item(styleName);
	
	if (style == nullptr)
		return;

	style->Appearance				= (AGCSN::E_STYLEAPPEARANCE) eStyleApp;
	style->Placement				= E_PLACEMENT::PLC_OFFSETPLACEMENT;
	style->HatchBackColor			= pGanttHandler->ConvertColor (aBackColor);
	style->BackGroundMode			= AGCSN::GRE_BACKGROUNDMODE::FP_HATCH;
	style->HatchForeColor			= pGanttHandler->ConvertColor (aHatchColor);
	style->HatchStyle				= AGCSN::GRE_HATCHSTYLE::HS_DIAGONALCROSS;
	style->OffsetTop				= offsetTop;
	style->OffsetBottom				= offsetBottom;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::AddStyle	(
									const CString& sStyleName, 
									const E_STYLEAPPEREANCE eStyleApp,
									const COLORREF aBackColor,
									const COLORREF aBorderColor,
									const COLORREF aForeColor
								)
{
	VOID_ENSURE_GANTT_CONTROL()
	DECLARE_GANTT_HANDLER ();

	String^ styleName = gcnew String (sStyleName);

	if (!ExistsStyle(sStyleName))
		ganttUC->Styles->Add(styleName);

	clsStyle^ style = ganttUC->Styles->Item(styleName);
	
	if (style == nullptr)
		return;

	style->Appearance				= (AGCSN::E_STYLEAPPEARANCE) eStyleApp;
	style->BackColor				= pGanttHandler->ConvertColor (aBackColor);
	style->BackGroundMode			= AGCSN::GRE_BACKGROUNDMODE::FP_SOLID;
	style->BorderColor				= pGanttHandler->ConvertColor (aBorderColor);
	style->BorderStyle				= GRE_BORDERSTYLE::SBR_SINGLE;
	style->ButtonStyle				= GRE_BUTTONSTYLE::BT_LIGHTWEIGHT;
	style->ClipText					= true;
	style->DrawTextInVisibleArea	= false;
	style->FillMode					= GRE_FILLMODE::FM_COMPLETELYFILLED;
	style->Font						= ganttUC->Font;
	style->ForeColor				= pGanttHandler->ConvertColor (aForeColor);
	style->ImageAlignmentHorizontal	= GRE_HORIZONTALALIGNMENT::HAL_LEFT;
	style->ImageAlignmentVertical	= GRE_VERTICALALIGNMENT::VAL_CENTER;
	style->TextAlignmentHorizontal	= GRE_HORIZONTALALIGNMENT::HAL_CENTER;
	style->TextAlignmentVertical	= GRE_VERTICALALIGNMENT::VAL_CENTER;
	style->TextVisible				= true;
	style->UseMask					= true;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::AddStyle		(
										const CString& sStyleName,
										const E_STYLEAPPEREANCE eStyleApp,
										const GRE_GRADIENTFILLMODE eGradientFillMode,
										const COLORREF aStartGradientColor,
										const COLORREF aEndGradientColor,
										const COLORREF aBorderColor,
										const COLORREF aForeColor
									)
{
	VOID_ENSURE_GANTT_CONTROL()
	DECLARE_GANTT_HANDLER ();

	String^ styleName = gcnew String (sStyleName);

	if (!ExistsStyle(sStyleName))
		ganttUC->Styles->Add(styleName);

	clsStyle^ style = ganttUC->Styles->Item(styleName);
	
	if (style == nullptr)
		return;

	style->Appearance				= (AGCSN::E_STYLEAPPEARANCE) eStyleApp;
	style->BackGroundMode			= AGCSN::GRE_BACKGROUNDMODE::FP_GRADIENT;
	style->BorderColor				= pGanttHandler->ConvertColor (aBorderColor);
	style->BorderStyle				= GRE_BORDERSTYLE::SBR_SINGLE;
	style->ButtonStyle				= GRE_BUTTONSTYLE::BT_LIGHTWEIGHT;
	style->ClipText					= true;
	style->DrawTextInVisibleArea	= false;
	style->GradientFillMode			= (AGCSN::GRE_GRADIENTFILLMODE)eGradientFillMode;
	style->StartGradientColor		= pGanttHandler->ConvertColor (aStartGradientColor);
	style->EndGradientColor			= pGanttHandler->ConvertColor (aEndGradientColor);
	style->Font						= ganttUC->Font;
	style->ForeColor				= pGanttHandler->ConvertColor (aForeColor);
	style->ImageAlignmentHorizontal	= GRE_HORIZONTALALIGNMENT::HAL_LEFT;
	style->ImageAlignmentVertical	= GRE_VERTICALALIGNMENT::VAL_CENTER;
	style->TextAlignmentHorizontal	= GRE_HORIZONTALALIGNMENT::HAL_CENTER;
	style->TextAlignmentVertical	= GRE_VERTICALALIGNMENT::VAL_CENTER;
	style->TextVisible				= true;
	//style->UseMask					= true;
}

//------------------------------------------------------------------------------------------------------
void CGanttWrapper::SetSplitterAppearance(const E_BORDERSTYLE& eStyleApp)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	DECLARE_GANTT_HANDLER();

	ganttUC->Splitter->Appearance = (AGCSN::E_BORDERSTYLE) eStyleApp;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::AddStyle	(
									const E_OBJECT& oOwner,
									const CString& sStyleName, 
									const E_STYLEAPPEREANCE eStyleApp,
									const int& offsetTop,
									const int& offsetBottom,
									const COLORREF aBackColor,  
									const COLORREF aBorderColor,  
									const COLORREF aForeColor,  
									const int& nStartShapeIdx,
									const int& nEndShapeIdx,
									const int& nShapeIdx
								)
{
	AddStyle (sStyleName, eStyleApp, aBackColor, aBorderColor, aForeColor);

	VOID_ENSURE_GANTT_CONTROL()

	String^ styleName = gcnew String (sStyleName);

	clsStyle^ style = ganttUC->Styles->Item(styleName);
	
	if (style != nullptr)
	{
		style->OffsetTop = offsetTop;
		style->OffsetBottom = offsetBottom;
		style->Appearance = E_STYLEAPPEARANCE::SA_FLAT;
		style->Placement = E_PLACEMENT::PLC_OFFSETPLACEMENT;
		switch (oOwner)
		{
			case O_TASK:
				style->TaskStyle->StartShapeIndex = (AGCSN::GRE_FIGURETYPE) nStartShapeIdx;
				style->TaskStyle->EndShapeIndex = (AGCSN::GRE_FIGURETYPE) nEndShapeIdx;
				break;
			case O_MILESTONE:
				style->MilestoneStyle->ShapeIndex = (AGCSN::GRE_FIGURETYPE) nShapeIdx;
				break;
			case O_MARGIN:
				break;
		}
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::AddStyle(
								const E_OBJECT& oOwner,
								const CString& sStyleName, 
								const E_STYLEAPPEREANCE eStyleApp,
								const int& offsetTop,
								const int& offsetBottom,
								const GRE_GRADIENTFILLMODE eGradientFillMode,
								const COLORREF aStartGradientColor,
								const COLORREF aEndGradientColor,
								const COLORREF aBorderColor,  
								const COLORREF aForeColor,  
								const int& nStartShapeIdx,
								const int& nEndShapeIdx,
								const int& nShapeIdx
							)
{
	AddStyle(sStyleName, SA_FLAT, eGradientFillMode, aStartGradientColor, aEndGradientColor, aBorderColor, aForeColor);


	VOID_ENSURE_GANTT_CONTROL()

	String^ styleName = gcnew String (sStyleName);

	clsStyle^ style = ganttUC->Styles->Item(styleName);
	
	if (style != nullptr)
	{
		style->OffsetTop = offsetTop;
		style->OffsetBottom = offsetBottom;
		style->Appearance = E_STYLEAPPEARANCE::SA_FLAT;
		style->Placement = E_PLACEMENT::PLC_OFFSETPLACEMENT;
		switch (oOwner)
		{
			case O_TASK:
				style->TaskStyle->StartShapeIndex = (AGCSN::GRE_FIGURETYPE) nStartShapeIdx;
				style->TaskStyle->EndShapeIndex = (AGCSN::GRE_FIGURETYPE) nEndShapeIdx;
				break;
			case O_MILESTONE:
				//style->MilestoneStyle->ShapeIndex = (AGCSN::GRE_FIGURETYPE) nShapeIdx;
				break;
			case O_MARGIN:
				break;
		}
	}
}

//--------------------------------------------------------------------------
void CGanttWrapper::ClearRows()
{
	VOID_ENSURE_GANTT_CONTROL()

		ganttUC->Rows->Clear();
}

//---------------------------------------------------------------------
void CGanttWrapper::ClearColumns()
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->Columns->Clear();
}

//---------------------------------------------------------------------
void CGanttWrapper::SetColumnWidth(const CString& sColKey, const int& nWidth)
{
	VOID_ENSURE_GANTT_CONTROL()

	clsColumn^ cColumn;

	String^ requestedKey = gcnew String(sColKey);

	for (int i = 1; i <= ganttUC->Columns->Count; i++)
	{
		CString sConv = cwsprintf(_T("%d"), i); 
		String^ strConv = gcnew String(sConv);
		cColumn = ganttUC->Columns->Item(strConv);
		if (cColumn->Text->ToLower() == requestedKey->ToLower())
			break;
	}
	
	if (cColumn != nullptr)
		cColumn->Width = nWidth;

}

//---------------------------------------------------------------------------------------
int CGanttWrapper::AddColumn	(
									const CString& sText, 
									const int& nWidth /*-1*/, 
									const BOOL& bAllowMove /*FALSE*/, 
									const BOOL& bAllowSize /*TRUE*/, 
									const CString sStyle /*_T("")*/
								)
{
	if (ExistsColumn(sText)) 
		return 1;

	ENSURE_GANTT_CONTROL(-1)

	CString aStyle = sStyle;
	if (aStyle.IsEmpty())
		aStyle = szActiveGanttColumnsStyle;
	
	String^ style = gcnew String(aStyle);
	if (!ExistsStyle(style))
		AddStyle (aStyle, SA_RAISED, RGB(192,192,192), CLR_BLACK, CLR_BLACK);

	String^ text = gcnew String(sText);
	ganttUC->Columns->Add
		(
			text, 
			text,
			nWidth > 0 ? nWidth : text->Length,
			style
		);
	return ganttUC->Columns->Count;
}

//---------------------------------------------------------------------------------------
int CGanttWrapper::AddRow (
								const CString& sRowKey, 
								const CString& sText, 
								const int& height,
								const BOOL& bTaskContainer,
								const CString sStyle /*_T("")*/,
								const CString sClientAreaStyle /*_T("")*/
							)
{
	if (ExistsRow(sRowKey)) 
		return 1;
	
	ENSURE_GANTT_CONTROL(-1)

	CString aStyle = sStyle;
	if (aStyle.IsEmpty())
		aStyle = szActiveGanttRowsStyle;

	String^ style = gcnew String(aStyle);
	if (!ExistsStyle(style))
		AddStyle (aStyle, SA_RAISED, CLR_WHITE, CLR_BLACK, CLR_BLACK);

	String^ key = gcnew String(sRowKey);

	clsRow^ row = ganttUC->Rows->Add(key, gcnew String(sText), false, bTaskContainer == TRUE, style);
	if (row != nullptr)
	{
		row->Height = height;
		if (!sClientAreaStyle.IsEmpty())
			row->ClientAreaStyleIndex = gcnew String(sClientAreaStyle);
	}

	return ganttUC->Rows->Count;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetRowColCell	(
										const CString& sRowKey, 
										const int& nColIndex, 
										const CString& sText,
										const CString sStyle /*_T("")*/
									)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	String^ key = gcnew String(sRowKey);
	clsRow^ item = ganttUC->Rows->Item(key);
	if (item == nullptr)
	{
		ASSERT(FALSE);
		return;
	}

	String^ colKey = gcnew String(cwsprintf(_T("%d"), nColIndex));
	
	clsCell^ cell = item->Cells->Item(colKey);
	if (cell == nullptr)
	{
		ASSERT(FALSE);
		return;
	}

	cell->Text = gcnew String(sText);

	if (sStyle.IsEmpty()) return;

	CString aStyle = sStyle;
	if (aStyle.IsEmpty())
		aStyle = szActiveGanttRowsStyle;
	
	String^ style = gcnew String(aStyle);
	if (!ExistsStyle(style))
		AddStyle (aStyle, SA_RAISED, CLR_WHITE, CLR_BLACK, CLR_BLACK);

	
	cell->StyleIndex = style;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTooltip (const CString& sTooltip)
{
	return SetTooltip (sTooltip, RGB(255,255,128), RGB(0,0,160), RGB(0,0,0));
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTooltip	(
									const CString& sTooltip, 
									const COLORREF aBackColor,  
									const COLORREF aBorderColor,  
									const COLORREF aForeColor  
								)
{
	VOID_ENSURE_GANTT_CONTROL()
	DECLARE_GANTT_HANDLER()
	
	ganttUC->ToolTip->BackColor = pGanttHandler->ConvertColor(aBackColor);
	ganttUC->ToolTip->BorderColor = pGanttHandler->ConvertColor(aBorderColor);
	ganttUC->ToolTip->ForeColor = pGanttHandler->ConvertColor(aForeColor);
	ganttUC->ToolTip->Text = gcnew String(sTooltip);
	ganttUC->ToolTip->Visible = TRUE;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTooltipVisible (const BOOL& bVisible)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	ganttUC->ToolTip->Visible = bVisible == TRUE;
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineScrollBarStartDate(const DataDate& aDate)
{
	VOID_ENSURE_GANTT_CONTROL()

	DateTime^ date = gcnew DateTime(
										aDate.Year(), 
										aDate.Month(), 
										aDate.Day(), 
										aDate.Hour(), 
										aDate.Minute(), 
										aDate.Second()
									);
	
	ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->StartDate = *date;
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineScrollBarInterval(const CString& sInterval)
{
	VOID_ENSURE_GANTT_CONTROL()

	String ^ interval = gcnew String(sInterval);

	ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Interval = interval;
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineScrollBarMax(const int& nMax)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Max = nMax;
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineScrollBarSmallChange(const int& nSmallChange)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->SmallChange = nSmallChange;
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineScrollBarLargeChange(const int& nLargeChange)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->LargeChange = nLargeChange;
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineScrollBarEnabled(const BOOL& bEnabled)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Enabled = (bEnabled == TRUE);
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineScrollBarVisible(const BOOL& bVisible)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->TimeLine->TimeLineScrollBar->Visible = (bVisible == TRUE);
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineTickMarkAreaVisible(const BOOL& visible)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	ganttUC->CurrentViewObject->TimeLine->TickMarkArea->Visible = (visible == TRUE);
}

//------------------------------------------------------------------------------------------
void CGanttWrapper::SetRowStyle		(
											const CString& sRowKey,
											const CString& sStyleName, 
											const E_STYLEAPPEREANCE eStyleAppearance,
											const COLORREF aBackColor, 
											const COLORREF aBorderColor, 
											const COLORREF aForeColor
									)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	if (!ExistsStyle(sStyleName))
		AddStyle (sStyleName, eStyleAppearance, aBackColor, aBorderColor, aForeColor);

	String^ key		= gcnew String(sRowKey);
	clsRow^ row		= ganttUC->Rows->Item(key);
	
	if (row != nullptr)
	{
		String^ style	= gcnew String(sStyleName);
		row->StyleIndex = style;
	}
}

//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetSplitterWidth(const int& width)
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->Splitter->Position = width;
}


//-----------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineTickMarkAreaStyle	(
														const CString& sStyle, 
														const E_STYLEAPPEREANCE eStyleApp, 
														const COLORREF aBackColor, 
														const COLORREF aBorderColor, 
														const COLORREF aForeColor
													)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	if (!ExistsStyle(sStyle))
		AddStyle (sStyle, eStyleApp, aBackColor, aBorderColor, aForeColor);

	String^ style = gcnew String(sStyle);
	ganttUC->CurrentViewObject->TimeLine->TickMarkArea->StyleIndex = style;
	
}

//---------------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineTickMarkAreaHeight(const int& height)
{
	VOID_ENSURE_GANTT_CONTROL()
	
	ganttUC->CurrentViewObject->TimeLine->TickMarkArea->Height = height;
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::ClearTimeLineTickMarkAreaTickMarks()
{
	VOID_ENSURE_GANTT_CONTROL()

	ganttUC->CurrentViewObject->TimeLine->TickMarkArea->TickMarks->Clear();
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::AddTimeLineTickMarkAreaTickMarks	(
															const CString& sInterval, 
															const E_TICKMARKTYPES eTickMarkType, 
															const BOOL& bDisplayText, 
															const CString& sTextFormat
														)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ interval = gcnew String(sInterval);
	String^ textFormat = gcnew String(sTextFormat);

	ganttUC->CurrentViewObject->TimeLine->TickMarkArea->TickMarks->Add
			(
				interval, 
				(AGCSN::E_TICKMARKTYPES)eTickMarkType, 
				bDisplayText == TRUE, 
				textFormat
			);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineType (const E_TIER eTier, const E_TIMELINE_TYPE type)
{
	VOID_ENSURE_GANTT_CONTROL()
	switch (eTier)
	{
	case CGanttWrapper::T_LOWER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->LowerTier->TierType = (AGCSN::E_TIERTYPE) type;
		break;
	case CGanttWrapper::T_MIDDLE:
		ganttUC->CurrentViewObject->TimeLine->TierArea->MiddleTier->TierType = (AGCSN::E_TIERTYPE) type;
		break;
	case CGanttWrapper::T_UPPER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->UpperTier->TierType = (AGCSN::E_TIERTYPE) type;
		break;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineInterval (const E_TIER eTier, const CString& sInterval)
{
	VOID_ENSURE_GANTT_CONTROL()

	String^ interval = gcnew String(sInterval);

	switch (eTier)
	{
	case CGanttWrapper::T_LOWER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->LowerTier->Interval = interval;
		break;
	case CGanttWrapper::T_MIDDLE:
		ganttUC->CurrentViewObject->TimeLine->TierArea->MiddleTier->Interval = interval;
		break;
	case CGanttWrapper::T_UPPER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->UpperTier->Interval = interval;
		break;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineHeight (const E_TIER eTier, int height)
{
	VOID_ENSURE_GANTT_CONTROL()
	switch (eTier)
	{
	case CGanttWrapper::T_LOWER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->LowerTier->Height = height;
		break;
	case CGanttWrapper::T_MIDDLE:
		ganttUC->CurrentViewObject->TimeLine->TierArea->MiddleTier->Height = height;
		break;
	case CGanttWrapper::T_UPPER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->UpperTier->Height = height;
		break;
	}
}

//------------------------------------------------------------------------------------------
void CGanttWrapper::SetTimeLineVisible(const E_TIER eTier, const BOOL& visible)
{
	VOID_ENSURE_GANTT_CONTROL()
	switch (eTier)
	{
	case CGanttWrapper::T_LOWER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->LowerTier->Visible = (visible == TRUE);
		break;
	case CGanttWrapper::T_MIDDLE:
		ganttUC->CurrentViewObject->TimeLine->TierArea->MiddleTier->Visible = (visible == TRUE);
		break;
	case CGanttWrapper::T_UPPER:
		ganttUC->CurrentViewObject->TimeLine->TierArea->UpperTier->Visible = (visible == TRUE);
		break;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::SetToDate (const  DataDate& aDate)
{
	VOID_ENSURE_GANTT_CONTROL()

	DateTime^ date = gcnew DateTime(aDate.Year(), aDate.Month(), aDate.Day(), aDate.Hour(), aDate.Minute(), aDate.Second());
	ganttUC->CurrentViewObject->TimeLine->Position(*date);
}

//-------------------------------------------------------------------------------------------
void CGanttWrapper::RecurringStartWeekDay(const E_WEEKDAY eStartWeekDay, const E_WEEKDAY eEndWeekDay)
{
	VOID_ENSURE_GANTT_CONTROL()

	DateTime^ dateStart = gcnew DateTime(0);
	DateTime^ dateEnd	= gcnew DateTime(0);
	
	clsTimeBlock^ timeBlock = ganttUC->TimeBlocks->Add(*dateStart, *dateEnd);
	switch (eStartWeekDay)
	{
		case WD_SUNDAY:
			timeBlock->RecurringStartWeekDay = AGCSN::E_WEEKDAY::WD_SUNDAY;
			break;
		case WD_MONDAY:
			timeBlock->RecurringStartWeekDay = AGCSN::E_WEEKDAY::WD_MONDAY;
			break;
		case WD_TUESDAY:
			timeBlock->RecurringStartWeekDay = AGCSN::E_WEEKDAY::WD_TUESDAY;
			break;
		case WD_WEDNESDAY:
			timeBlock->RecurringStartWeekDay = AGCSN::E_WEEKDAY::WD_WEDNESDAY;
			break;
		case WD_THURSDAY:
			timeBlock->RecurringStartWeekDay = AGCSN::E_WEEKDAY::WD_THURSDAY;
			break;
		case WD_FRIDAY:
			timeBlock->RecurringStartWeekDay = AGCSN::E_WEEKDAY::WD_FRIDAY;
			break;
		case WD_SATURDAY:
			timeBlock->RecurringStartWeekDay = AGCSN::E_WEEKDAY::WD_SATURDAY;
			break;
	}
	switch (eEndWeekDay)
	{
		case WD_SUNDAY:
			timeBlock->RecurringEndWeekDay = AGCSN::E_WEEKDAY::WD_SUNDAY;
			break;
		case WD_MONDAY:
			timeBlock->RecurringEndWeekDay = AGCSN::E_WEEKDAY::WD_MONDAY;
			break;
		case WD_TUESDAY:
			timeBlock->RecurringEndWeekDay = AGCSN::E_WEEKDAY::WD_TUESDAY;
			break;
		case WD_WEDNESDAY:
			timeBlock->RecurringEndWeekDay = AGCSN::E_WEEKDAY::WD_WEDNESDAY;
			break;
		case WD_THURSDAY:
			timeBlock->RecurringEndWeekDay = AGCSN::E_WEEKDAY::WD_THURSDAY;
			break;
		case WD_FRIDAY:
			timeBlock->RecurringEndWeekDay = AGCSN::E_WEEKDAY::WD_FRIDAY;
			break;
		case WD_SATURDAY:
			timeBlock->RecurringEndWeekDay = AGCSN::E_WEEKDAY::WD_SATURDAY;
			break;
	}

	timeBlock->RecurringStartDate->SetTime(0, 0, 0);
	timeBlock->RecurringEndDate->SetTime(0, 0, 0);
}

//------------------------------------------------------------------------------------------
CString	CGanttWrapper::GetObjectStyle(const CString& sObjectStyle)
{
	ENSURE_GANTT_CONTROL(_T(""))

	if (!ExistsObject(sObjectStyle))
		return _T("");

	String^ objectKey = gcnew String(sObjectStyle);
	clsTask^ task = ganttUC->Tasks->Item(objectKey);
	
	return (task == nullptr ? _T("") : task->StyleIndex);
}

//------------------------------------------------------------------------------------------
void CGanttWrapper::SwitchObjectStyle	(
											const CString& sObjectKey, 
											const CString& sOldStyle, 
											const CString& sNewStyle
										)
{
	VOID_ENSURE_GANTT_CONTROL()

	if (!ExistsObject(sObjectKey) || !ExistsStyle(sOldStyle) || !ExistsStyle(sNewStyle))
	{
		ASSERT (FALSE);
		return;
	}

	String^ objectKey  = gcnew String(sObjectKey);
	String^ oldStyle = gcnew String(sOldStyle);

	clsTask^ task = ganttUC->Tasks->Item(objectKey);

	if (task != nullptr && task->StyleIndex == oldStyle)
	{
		String^ newStyle = gcnew String(sNewStyle);
		task->StyleIndex = newStyle;
	}
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::RemoveObject (const CString& sTaskKey)
{
	VOID_ENSURE_GANTT_CONTROL()

	if (!ExistsObject(sTaskKey))
		return;

	String^ key = gcnew String(sTaskKey);
	clsTask^ task = ganttUC->Tasks->Item(key);

	if (task != nullptr)
		ganttUC->Tasks->Remove(key);
}

//---------------------------------------------------------------------------------------
CString CGanttWrapper::AddTask	(
									const CString&	sRowKey, 
									const CString&	sTaskKey, 
									const CString&	sText,
									const DataDate& aStartDate,
									const DataDate& aEndDate,
									const CString	sTaskStyle /*_T("")*/,
									const CString	sLayer /*_T("")*/
								)
{
	return AddObject (O_TASK, sRowKey, sTaskKey, sText, aStartDate, aEndDate, sTaskStyle, sLayer);
}

//--------------------------------------------------------------------------------------------
void CGanttWrapper::SetTaskMoveDisabled(const CString& sTaskKey)
{
	if (!ExistsObject(sTaskKey))
		return;

	VOID_ENSURE_GANTT_CONTROL()

	String^ taskKey = gcnew String(sTaskKey);

	ganttUC->Tasks->Item(taskKey)->AllowedMovement = E_MOVEMENTTYPE::MT_MOVEMENTDISABLED;

}

//---------------------------------------------------------------------------------------
CString	CGanttWrapper::AddMargin	(
										const CString&	sRowKey, 
										const CString&	sMarginKey, 
										const CString&	sText,
										const DataDate& aStartDate,
										const DataDate& aEndDate,
										const CString	sMarginStyle /*_T("")*/,
										const CString	sLayer		/*= _T("")*/
									)
{
	return AddObject (O_MARGIN, sRowKey, sMarginKey, sText, aStartDate, aEndDate, sMarginStyle, sLayer);
}

//---------------------------------------------------------------------------------------------------
CString CGanttWrapper::AddPredecessor			
									(
										const CString&	sTaskKey,
										const CString&	sPredecessorTaskKey,
										const E_CONSTRAINTTYPE eConstraintType,
										const CString&	sPredecessorKey,
										const CString&	sPredecessorStyle
									)
{
	if (sTaskKey.IsEmpty() || sPredecessorTaskKey.IsEmpty() || sPredecessorKey.IsEmpty())
	{
		ASSERT_TRACE(FALSE, cwsprintf(_T("CGanttWrapper::AddPredecessor empty function parameter! {%s, %s, %s}"), sTaskKey, sPredecessorTaskKey, sPredecessorKey));
		return _T("");
	}

	ENSURE_GANTT_CONTROL(_T(""))

	if (!ExistsObject(sTaskKey) || !ExistsObject(sPredecessorTaskKey))
	{
		ASSERT_TRACE(FALSE, cwsprintf(_T("CGanttWrapper::AddPredecessor null task {%s} or {%s}!"), sTaskKey, sPredecessorTaskKey));
		return _T("");
	}

	String^ taskKey = gcnew String(sTaskKey);
	String^ predecessorTaskKey = gcnew String(sPredecessorTaskKey);
	
	CString aStyle = sPredecessorStyle;
	if (aStyle.IsEmpty())
		aStyle = szActiveGanttTasksStyle;

	String^ style = gcnew String(aStyle);
	if (!ExistsStyle(style))
		AddStyle(aStyle, SA_FLAT, CLR_BLUE, CLR_BLACK, CLR_BLACK);
	
	String^ predecessorKey = gcnew String(sPredecessorKey);

	clsPredecessor^ predecessor = ganttUC->Tasks->Item(taskKey)->Predecessors->Add(predecessorTaskKey, (AGCSN::E_CONSTRAINTTYPE)eConstraintType, predecessorKey, style);

	if (predecessor != nullptr)
		return CString(sPredecessorTaskKey);

	return _T("");
}

//---------------------------------------------------------------------------------------
CString CGanttWrapper::AddObject(
									const E_OBJECT& eObjectType,
									const CString&	sRowKey, 
									const CString&	sTaskKey, 
									const CString&	sText,
									const DataDate& aStartDate,
									const DataDate& aEndDate,
									const CString	sTaskStyle /*_T("")*/,
									const CString	sLayer /*_T("")*/
								)
{
	if (sRowKey.IsEmpty() || sTaskKey.IsEmpty() || aStartDate.IsEmpty() || aEndDate.IsEmpty())
	{
		ASSERT_TRACE(FALSE, cwsprintf(_T("CGanttWrapper::AddTask empty function parameter! {%s, %s, %s, %s}"), sRowKey, sTaskKey, aStartDate.FormatData(), aEndDate.FormatData()));
		return _T("");
	}

	if (aStartDate > aEndDate)
	{
		ASSERT_TRACE(FALSE, cwsprintf(_T("CGanttWrapper::AddTask empty function parameter! {%s, %s}"), aStartDate.FormatData(), aEndDate.FormatData()));
		return _T("");
	}

	ENSURE_GANTT_CONTROL(_T(""))

	if (ExistsObject(sTaskKey)) 
		return sTaskKey;

	String^ rowKey = gcnew String(sRowKey);
	String^ taskKey = gcnew String(sTaskKey);
	String^ text = gcnew String(sText);

	CString aStyle = sTaskStyle;
	if (aStyle.IsEmpty())
		aStyle = szActiveGanttTasksStyle;

	String^ style = gcnew String(aStyle);
	if (!ExistsStyle(style))
	{
		AddStyle(eObjectType, aStyle, SA_FLAT, 10, 10, CLR_BLUE, CLR_BLACK, CLR_BLACK);
		//style = gcnew String(defaultStyle);
	}

	DateTime^ startDate = gcnew DateTime(aStartDate.Year(), aStartDate.Month(),aStartDate.Day(), aStartDate.Hour(), aStartDate.Minute(), aStartDate.Second());
	DateTime^ endDate = gcnew DateTime(aEndDate.Year(), aEndDate.Month(),aEndDate.Day(), aEndDate.Hour(), aEndDate.Minute(), aEndDate.Second());

	// Task could be on a different Layer
	CString aLayer = sLayer.IsEmpty() ? szActiveGanttDefaultMoveableObjectsLayer : sLayer;
	if (!ExistsLayer(aLayer))
		AddLayer (aLayer);
	
	Int32 nrTasks = ganttUC->Tasks->Count;
	clsTask^ task;
	if (eObjectType == O_MARGIN || eObjectType == O_MILESTONE)
		task = ganttUC->Tasks->Add(text, rowKey, *startDate, *endDate, taskKey, style, gcnew String(szActiveGanttDefaultLayer));
	else
	{
		if (!ExistsStyle(szDefaultStyle))
			SetDefaultStyle();

		task = ganttUC->Tasks->Add(text, rowKey, *startDate, *endDate, taskKey, gcnew String(szDefaultStyle), gcnew String(aLayer));
		//test for width for styles with gradient
		if ( (task->Right - task->Left) > 1 )
			task->StyleIndex = style;
	}

	if (task != nullptr)
	{
		switch (eObjectType)
		{
			case O_TASK:
				task->AllowedMovement = E_MOVEMENTTYPE::MT_RESTRICTEDTOROW;
				break;
			case O_MILESTONE:
				task->AllowedMovement = E_MOVEMENTTYPE::MT_MOVEMENTDISABLED;
				break;
			case O_MARGIN:
				task->AllowedMovement = E_MOVEMENTTYPE::MT_MOVEMENTDISABLED;
				break;

		}

		task->AllowStretchLeft = false;
		task->AllowStretchRight = false;
	}
	return CString(taskKey);
}

int CGanttWrapper::ObjectWidth(const CString& sObjectName)
{
	ENSURE_GANTT_CONTROL(0)

	String^ objectName = gcnew String(sObjectName);
	clsTask^ task = ganttUC->Tasks->Item(objectName);
	if (task == nullptr)
		return 0;

	return (int)(task->Right - task->Left);
}

//---------------------------------------------------------------------------------------
CString	CGanttWrapper::AddMilestone	(
										const CString&	sRowKey, 
										const CString&	sMilestoneKey, 
										const CString&	sText,
										const DataDate& aDate,
										const CString	sMilestoneStyle /*_T("")*/
									)
{
	if (sRowKey.IsEmpty() || sMilestoneKey.IsEmpty() || aDate.IsEmpty())
	{
		ASSERT_TRACE(FALSE, cwsprintf(_T("CGanttWrapper::AddMilestone empty function parameter! {%s, %s, %s}"), sRowKey, sMilestoneKey, aDate.FormatData()));
		return _T("");
	}

	ENSURE_GANTT_CONTROL(_T(""))

	CString aStyle = sMilestoneStyle;
	if (aStyle.IsEmpty())
		aStyle = szActiveGanttMilestoneStyle;

	String^ style = gcnew String(aStyle);
	
	// default style
	if (!ExistsStyle(style))
	{
		AddStyle(O_MILESTONE, aStyle, SA_RAISED, 8, 10, CLR_BLUE, CLR_BLACK, CLR_BLACK);

		clsStyle^ msStyle = ganttUC->Styles->Item (style);
		if (msStyle == nullptr)
		{
			ASSERT_TRACE(FALSE, _T("CGanttWrapper::AddMilestone style not added!"));
			return _T("");
		}
		msStyle->MilestoneStyle->ShapeIndex = GRE_FIGURETYPE::FT_CIRCLEDIAMOND;
	}
	
	return AddObject (O_MILESTONE, sRowKey, sMilestoneKey, sText, aDate, aDate, aStyle);
}

//---------------------------------------------------------------------------------------
void CGanttWrapper::GetAsBmpImage (CImage* pImage)
{
	VOID_ENSURE_GANTT_CONTROL()
	DECLARE_GANTT_HANDLER ();

	try
	{
		System::Drawing::Bitmap^ bitmap = gcnew System::Drawing::Bitmap(ganttUC->ClientRectangle.Width, ganttUC->ClientRectangle.Height);
		ganttUC->DrawToBitmap(bitmap, ganttUC->ClientRectangle);
		HBITMAP hBmp = (HBITMAP)bitmap->GetHbitmap().ToInt32(); 
		pImage->Attach (hBmp);
	}
	catch (Exception^ e)
	{
		CString sMessage(e->Message);

		AfxGetDiagnostic()->Add (cwsprintf(_TB("The following exception has occurred saving Gantt image for Web application. Gantt control will not be displayed."), sMessage));
		ASSERT_TRACE1(FALSE, "\n\rThe following exception has occurred saving Gantt image for Web application. Gantt control with ID %d will not be displayed.\n\r%", pGanttHandler->GetWnd()->GetDlgCtrlID());
	}
}
