#pragma once

#include "UserControlWrappers.h"
#include "beginh.dex"

class CUserControlHandlerObj;

// Events
//===========================================================================
#define	UM_GANTT_TIMELINE_CHANGED		0x7008	
#define	UM_GANTT_BEGIN_OBJECT_MOVE		0x7009	
#define	UM_GANTT_END_OBJECT_MOVE		0x7010	
#define UM_GANTT_COMPLETE_OBJECT_MOVE	0x7011
#define	UM_GANTT_BEGIN_OBJECT_SIZE		0x7012	
#define	UM_GANTT_END_OBJECT_SIZE		0x7013	
#define	UM_GANTT_OBJECT_ADDED			0x7014	
#define	UM_GANTT_OBJECT_SELECTED		0x7015	
#define	UM_GANTT_OBJECT_MOVE			0x7016	
#define	UM_GANTT_OBJECT_SIZE			0x7017	
#define	UM_GANTT_SCROLL_HORIZONTAL1		0x7018	
#define	UM_GANTT_SCROLL_HORIZONTAL2		0x7019
#define	UM_GANTT_SCROLL_VERTICAL		0x7020
#define	UM_GANTT_ERROR					0x7021
#define UM_GANTT_UPPER_TIER				0x7022
#define UM_GANTT_MIDDLE_TIER			0x7023
#define UM_GANTT_LOWER_TIER				0x7024
#define UM_GANTT_MOUSE_DOUBLE_CLICK		0x7025

// wrapper class to ActiveGantt C# component
//===========================================================================
class TB_EXPORT GanttEventArgs : public UnmanagedEventsArgs
{
	DECLARE_DYNAMIC (GanttEventArgs);


public:
	enum EventTarget { 
						EVT_CELL, 
						EVT_CLIENTAREA, 
						EVT_COLUMN, 
						EVT_EMPTYAREA, 
						EVT_MILESTONE,  
						EVT_NONE, 
						EVT_PERCENTAGE,
						EVT_ROW,
						EVT_TABLE,
						EVT_TASK,
						EVT_TIMEBLOCK,
						EVT_TIMELINE
					};

private:
	EventTarget m_EventTarget;
	int			m_nObjectIndex;
	int			m_nParentObjectIndex;

public:
	GanttEventArgs	(const CString& sError);
	GanttEventArgs	(
						EventTarget eventTarget, 
						int nObjectIndex = -1, 
						int nParentIndex = -1
					);

	EventTarget GetEventTarget		() const { return m_EventTarget; }
	int			GetObjectIndex		() const { return m_nObjectIndex; }
	int			GetParentObjectIndex() const { return m_nParentObjectIndex; }
};

// wrapper class to ActiveGantt C# component
//===========================================================================
class TB_EXPORT CGanttWrapper : public CUserControlWrapperObj
{
public:
	enum E_BORDERSTYLE			{ BS_NONE, BS_SINGLE, BS_3D };
	enum E_CONTROLMODE			{ CM_GRID, CM_TREEVIEW };
	enum E_SCROLLBARBEHAVIOUR	{ SB_DISABLE, SB_HIDE };
	enum E_TIMEBLOCKBEHAVIOUR	{ TB_ROW_EXTENTS, TB_CONTROL_EXTENTS };
	enum E_STYLEAPPEREANCE		{ SA_RAISED, SA_SUNKEN, SA_FLAT, SA_GRAPHICAL, SA_CELL };
	//background mode
	enum GRE_BACKGROUNDMODE		{ FP_SOLID, FP_TRANSPARENT, FP_GRADIENT, FP_PATTERN, FP_HATCH };
	enum GRE_GRADIENTFILLMODE	{ GDT_HORIZONTAL, GDT_VERTICAL };
	enum E_TIMELINE_TYPE		{ TT_DAYOFWEEK = 1, TT_MONTH, TT_QUARTER, TT_YEAR, TT_WEEK, TT_CUSTOM, TT_DAY, TT_DAYOFYEAR, TT_HOUR, TT_MINUTE };
	enum E_TICKMARKTYPES		{ TLT_BIG, TLT_MEDIUM, TLT_SMALL };
	enum E_FONTSTYLE			{ F_BOLD, F_ITALIC, F_REGULAR, F_STRIKEOUT, F_UNDERLINE};
	enum E_TIER					{ T_UPPER, T_MIDDLE, T_LOWER };
	enum E_OBJECT				{ O_TASK, O_MILESTONE, O_MARGIN };
	enum E_WEEKDAY				{ WD_SUNDAY, WD_MONDAY, WD_TUESDAY, WD_WEDNESDAY, WD_THURSDAY, WD_FRIDAY, WD_SATURDAY };
	//manage also predecessors
	enum E_CONSTRAINTTYPE		{ PCT_END_TO_START, PCT_START_TO_START, PCT_END_TO_END, PCT_START_TO_END };

public:
	CGanttWrapper	();
	~CGanttWrapper();
public:
	void	Enable									(BOOL bValue = TRUE);

	CString	GetTag									();
	CString GetTaskKey								(const CString& taskIndex);
	CString GetRowKey								(const CString& rowIndex);
	CString GetColumnKey							(const CString& colIndex);

	// ActiveGantt Properties 
	void	SetAllowAdd								(const BOOL bValue = TRUE);
	void	SetAllowColumnMove						(const BOOL bValue = TRUE);
	void	SetAllowColumnSize						(const BOOL bValue = TRUE);
	void	SetAllowEdit							(const BOOL bValue = TRUE);
	void	SetAllowHeaderSize						(const BOOL bValue = TRUE);
	void	SetAllowPredecessorAdd					(const BOOL bValue = TRUE);
	void	SetAllowRowMove							(const BOOL bValue = TRUE);
	void	SetAllowRowSize							(const BOOL bValue = TRUE);
	void	SetAllowSplitterMove					(const BOOL bValue = TRUE);
	void	SetAllowTimeLineScroll					(const BOOL bValue = TRUE);
	void	SetBackColor							(const COLORREF color);
	void	SetBorderStyle							(const E_BORDERSTYLE style);
	void	SetControlMode							(const E_CONTROLMODE mode);
	void	SetScrollBarBehaviour					(const E_SCROLLBARBEHAVIOUR behaviour);
	void	SetTimeBlockBehaviour					(const E_TIMEBLOCKBEHAVIOUR behaviour);
	void	SetCurrentLayer							(const int& nLayerIdx);
	void	SetCurrentView							(const int& nViewIdx);
	void	SetDoubleBuffering						(const BOOL bValue = TRUE);
	void	SetCulture								(const CString& sCulture);
	void	SetHeaderSelected						(const BOOL bValue = TRUE);
	void	SetMinColumnWidth						(const int& nWidth);
	void	SetMinRowHeight							(const int& nHeight);
	void	SelectCellIndex							(const int& nIndex);
	void	SelectColumnIndex						(const int& nIndex);
	void	SelectRowIndex							(const int& nIndex);
	void	SelectTaskIndex							(const int& nIndex);
	void	SetTooltip								(const CString& sTooltip); 
	void	SetTooltip								(
														const CString& sTooltip, 
														const COLORREF aBackColor,  
														const COLORREF aBorderColor,  
														const COLORREF aForeColor  
													);
	void	SetTooltipVisible						(const BOOL& bVisible);

	void	SetDefaultAddMode						();
	void	SetDefaultLayers						();
	void	SetDefaultStyle							();
	void	SetDefaultErrorReports					();
	void	SetDefaultFont							();
	void	SetSplitterWidth						(const int& width);
	void	SetToDate								(const  DataDate& aDate);
	void	SetFontForStyle							(
														const CString& sFontName, 
														const int& fontSize, 
														const E_FONTSTYLE 
														eFontStyle, 
														const CString& sStyleName
													);
	void	SetHorizontalScrollValue               (const int& nValueScroll);
	int		GetHorizontalScrollValue				();

	// timeline methods
	void	SetTimeLineScrollBarStartDate			(const DataDate& dDate);
	void	SetTimeLineScrollBarInterval			(const CString& sInterval);
	void	SetTimeLineScrollBarMax					(const int& nMax);
	void	SetTimeLineScrollBarSmallChange			(const int& nSmallChange);
	void	SetTimeLineScrollBarLargeChange			(const int& nLargeChange);
	void	SetTimeLineScrollBarEnabled				(const BOOL& bEnabled);
	void	SetTimeLineScrollBarVisible				(const BOOL& bVisible);

	void	SetTimeLineTickMarkAreaVisible			(const BOOL& visible);
	void	SetTimeLineTickMarkAreaStyle			(
														const CString& sStyle, 
														const E_STYLEAPPEREANCE eStyleApp, 
														const COLORREF aBackColor, 
														const COLORREF aBorderColor, 
														const COLORREF aForeColor
													);
	void	SetTimeLineTickMarkAreaHeight			(const int& height);
	void	ClearTimeLineTickMarkAreaTickMarks		();
	void	AddTimeLineTickMarkAreaTickMarks		(
														const CString& sInterval, 
														const E_TICKMARKTYPES eTickMarkType, 
														const BOOL& bDisplayText, 
														const CString& sTextFormat
													);
	void	SetTimeLineType							(const E_TIER eTier, const E_TIMELINE_TYPE type);
	void    SetTimeLineInterval						(const E_TIER eTier, const CString& sInterval);
	void	SetTimeLineHeight						(const E_TIER eTier, int height);
	void	SetTimeLineVisible						(const E_TIER eTier, const BOOL& visible);
	void	SetTimeLineAppearance					(const E_STYLEAPPEREANCE eStyleApp);
	void	SetClientAreaGridInterval				(const CString& sInterval);
	void	SetClientAreaGridVerticalLines			(const BOOL& bValue = TRUE);
	void	SetClientAreaGridSnapToGrid				(const BOOL& bValue = TRUE);
	void	SetTierStyle							(
														const E_TIER eTierType,
														const CString& sFontName, 
														const int& nFontSize, 
														const E_FONTSTYLE eFontStyle, 
														const COLORREF aBackColor, 
														const COLORREF aBorderColor, 
														const COLORREF aForeColor
													);
	void	SetTierGradientStyle					(
														const E_TIER eTierType,
														const CString& sFontName,
														const int& nFontSize,
														const E_FONTSTYLE eFontStyle,
														const GRE_GRADIENTFILLMODE eGradientFillMode,
														const COLORREF aStartGradientColor,
														const COLORREF aEndGradientColor,
														const COLORREF aBorderColor,
														const COLORREF aForeColor
													);
	void	RecurringStartWeekDay					(const E_WEEKDAY eStartWeekDay, const E_WEEKDAY eEndWeekDay);

	// drawing
	void	SetCurrentViewInterval	(const CString& sInterval);
	void	RedrawControl			();
	void	RemoveAllTasks			();

	// objects management
	void	SetSplitterAppearance(const E_BORDERSTYLE& eStyleApp);
	int		AddLayer		(const CString& sLayerKey, const BOOL& bVisible = TRUE);
	BOOL	ExistsLayer		(const CString& sLayerKey);

	void	AddStyleHatch	(
								const CString& sStyleName,
								const E_STYLEAPPEREANCE eStyleApp, 
								const COLORREF aBackColor,
								const COLORREF aHatchColor,
								const int& offsetTop,
								const int& offsetBottom
							);

	void	AddStyle		(
								const CString& sStyleName, 
								const E_STYLEAPPEREANCE eStyleApp,  
								const COLORREF aBackColor,  
								const COLORREF aBorderColor,  
								const COLORREF aForeColor  
							);

	void	AddStyle		(
								const CString& sStyleName,
								const E_STYLEAPPEREANCE eStyleApp,
								const GRE_GRADIENTFILLMODE eGradientFillMode,
								const COLORREF aStartGradientColor,
								const COLORREF aEndGradientColor,
								const COLORREF aBorderColor,
								const COLORREF aForeColor
							);

	void	AddStyle		(
								const E_OBJECT& oOwner,
								const CString& sStyleName, 
								const E_STYLEAPPEREANCE eStyleApp,
								const int& offsetTop,
								const int& offsetBottom,
								const COLORREF aBackColor,  
								const COLORREF aBorderColor,  
								const COLORREF aForeColor,  
								const int& nStartShapeIdx = 0,
								const int& nEndShapeIdx = 0,
								const int& nShapeIdx = 0
							);
	
	void	AddStyle		(
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
								const int& nStartShapeIdx = 0,
								const int& nEndShapeIdx = 0,
								const int& nShapeIdx = 0
							);
	
	BOOL	ExistsStyle		(const CString& sStyleKey);

	int		AddColumn		(
								const CString& sText, 
								const int& nWidth = -1, 
								const BOOL& bAllowMove = FALSE, 
								const BOOL& bAllowSize = TRUE, 
								const CString sStyle = _T("")
							);
	BOOL	ExistsColumn	(const CString& sColKey);

	void	ClearColumns	();

	void	ClearRows		();

	void	SetColumnWidth	(const CString& sColKey, const int& nWidth);

	int		AddRow			(
								const CString& sRowKey, 
								const CString& sText, 
								const int& height,
								const BOOL& bTaskContainer,
								const CString sStyle = _T(""),
								const CString sClientAreaStyle= _T("")
							);
	BOOL	ExistsRow		(const CString& sRowKey);

	void	SetRowStyle		(
								const CString& sRowKey,
								const CString& sStyleName, 
								const E_STYLEAPPEREANCE eStyleAppearance,
								const COLORREF aBackColor, 
								const COLORREF aBorderColor, 
								const COLORREF aForeColor
							);
	void	SetRowColCell	(
								const CString& sRowKey, 
								const int& nColIndex, 
								const CString& sText,
								const CString sStyle = _T("")
							);

	CString	AddObject			(
									const E_OBJECT& eObjectType,
									const CString&	sRowKey, 
									const CString&	sTaskKey, 
									const CString&	sText,
									const DataDate& aStartDate,
									const DataDate& aEndDate,
									const CString	sTaskStyle	= _T(""),
									const CString	sLayer		= _T("")
								);

	int		ObjectWidth			(const CString& sObjectName);

	BOOL	ExistsObject		(const CString& sObjectKey);
	void	RemoveObject		(const CString& sObjectKey);
	CString	GetObjectStyle		(const CString& sObjectKey);
	CString	GetObjectRowKey		(const CString& sObjectKey);
	void	SwitchObjectStyle	(
									const CString& sObjectKey, 
									const CString& sOldStyle, 
									const CString& sNewStyle
								);
	DataDate GetObjectStartDate	(const CString& sObjectKey);
	DataDate GetObjectEndDate	(const CString& sObjectKey);


	CString	AddMilestone	(
								const CString&	sRowKey, 
								const CString&	sMilestoneKey, 
								const CString&	sText,
								const DataDate& aDate,
								const CString	sMilestoneStyle = _T("")
							);
	CString	AddMargin		(
								const CString&	sRowKey, 
								const CString&	sMarginKey, 
								const CString&	sText,
								const DataDate& aStartDate,
								const DataDate& aEndDate,
								const CString	sMarginStyle = _T(""),
								const CString	sLayer		= _T("")
							);

	void	SetTaskMoveDisabled
								(
									const CString& sTaskKey
								);

	CString	AddTask			(
								const CString&	sRowKey, 
								const CString&	sTaskKey, 
								const CString&	sText,
								const DataDate& aStartDate,
								const DataDate& aEndDate,
								const CString	sTaskStyle	= _T(""),
								const CString	sLayer		= _T("")
							);

	CString AddPredecessor	(
								const CString&	sTaskKey,
								const CString&	sPredecessorTaskKey,
								const E_CONSTRAINTTYPE eConstraintType,
								const CString&	sPredecessorKey,
								const CString&	sPredecessorStyle
							);

	// export 
	void	GetAsBmpImage	(CImage* pImage);


protected:
	virtual void OnInitControl ();

private:
	void InitDefaultValues	();
};

#include "endh.dex"