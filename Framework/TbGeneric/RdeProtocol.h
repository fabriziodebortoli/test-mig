#pragma once

#include <TbGeneric\DataObj.h>
//includere alla fine degli include del .H
#include "beginh.dex"

//=============================================================================
class TB_EXPORT SpecialReportField
{
public:
	//RDE NULL ID 
	static const WORD  SpecialReportField::NO_INTERNAL_ID = ((WORD) 0xFFFF);
	class SRFid 
	{
	public:
		//Woorm special fields id
		const WORD  STATUS					= ((WORD) 0x7FFe);
		const WORD  OWNER					= ((WORD) 0x7FFd);
		const WORD  LAYOUT					= ((WORD) 0x7FFc);
		const WORD  PAGE					= ((WORD) 0x7FFb);
		const WORD  LINKED_DOC				= ((WORD) 0x7FFa);
		const WORD  IS_PRINTING				= ((WORD) 0x7FF9);
		const WORD  USE_DEFAULT_ATTRIBUTE	= ((WORD) 0x7FF8);
		const WORD  LAST_PAGE				= ((WORD) 0x7FF7);
		const WORD  EA_BARCODE				= ((WORD) 0x7FF6);
		const WORD  IS_ARCHIVING			= ((WORD) 0x7FF5);
		const WORD  IS_EXPORTING			= ((WORD) 0x7FF4);
		const WORD  FUNCTION_RETURN_VALUE	= ((WORD) 0x7FF3);
		const WORD  HIDE_ALL_ASK_DIALOGS	= ((WORD) 0x7FF2);
		const WORD  PRINT_ON_LETTERHEAD		= ((WORD) 0x7FF1);
		const WORD  IS_FIRST_TUPLE			= ((WORD) 0x7FF0);
		const WORD  IS_LAST_TUPLE			= ((WORD) 0x7FEF);
		const WORD  CURRENT_COPY			= ((WORD) 0x7FEE);
		const WORD  EMPTY_COLUMN			= ((WORD) 0x7FED);
	};
	static SRFid ID;
	//the latest used id
	static const WORD  REPORT_LOWER_SPECIAL_ID = ((WORD) 0x7FEC);

	class SRFname
	{
	public:
		CString		STATUS;					
		CString	    OWNER;
		CString	    LAYOUT;
		CString	    PAGE;
		CString	    LAST_PAGE;
		CString	    IS_PRINTING;
		CString	    IS_ARCHIVING;
		CString	    IS_EXPORTING;
		CString	    PRINT_ON_LETTERHEAD;
		CString	    USE_DEFAULT_ATTRIBUTE;
		CString	    LINKED_DOC;
		CString	    FUNCTION_RETURN_VALUE;
		CString	    HIDE_ALL_ASK_DIALOGS;
		CString	    EMPTY_COLUMN;
		CString	    IS_FIRST_TUPLE;
		CString	    IS_LAST_TUPLE;
		CString	    CURRENT_COPY;

		SRFname()
		{
			STATUS = _T("ReportStatus");
			OWNER = _T("OwnerID");
			LAYOUT = _T("ReportLayout");
			PAGE = _T("ReportCurrentPageNumber");
			LAST_PAGE = _T("ReportLastPageNumber");
			IS_PRINTING = _T("ReportIsPrinting");
			IS_ARCHIVING = _T("ReportIsArchiving");
			IS_EXPORTING = _T("ReportIsExporting");
			PRINT_ON_LETTERHEAD = _T("PrintOnLetterHead");
			USE_DEFAULT_ATTRIBUTE = _T("UseDefaultAttribute");
			LINKED_DOC = _T("LinkedDocumentID");
			FUNCTION_RETURN_VALUE = _T("_ReturnValue");
			HIDE_ALL_ASK_DIALOGS = _T("_HideAllAskDialogs");
			EMPTY_COLUMN = _T("_EmptyColumn");
			IS_FIRST_TUPLE = _T("IsFirstTuple");
			IS_LAST_TUPLE = _T("IsLastTuple");
			CURRENT_COPY = _T("CurrentCopyNumber");
		}
	};
	static SRFname NAME;
};

#define REPORT_DEFAULT_LAYOUT_NAME _T("default")

//=============================================================================
class TB_EXPORT IRDEManager: public CObject
{
public:

	BEGIN_TB_STRING_MAP(ErrorMessages)
		TB_LOCALIZED(CANT_OPEN_FILE,	"RDE : {0-%s}\r\n\tError opening file:\r\n\t{1-%s}")
		TB_LOCALIZED(SHARE_NOT_EXISTS,	"RDE : {0-%s}\r\n\tSHARE.EXE not installed using file:\r\n\t{1-%s}")
		TB_LOCALIZED(DISK_FULL,			"RDE : {0-%s}\r\n\tDisk full while writing on file:\r\n\t{1-%s}")
		TB_LOCALIZED(CANT_READ_FILE,	"RDE : {0-%s}\r\n\tError reading from file:\r\n\t{1-%s}")
		TB_LOCALIZED(CANT_SEEK_FILE,	"RDE : {0-%s}\r\n\tPositioning error in file:\r\n\t{1-%s} ")
		TB_LOCALIZED(CANT_WRITE_FILE,	"RDE : {0-%s}\r\n\tWrite error on file:\r\n\t{1-%s}\r\n\t")
		TB_LOCALIZED(CANT_DELETE_FILE,	"RDE : {0-%s}\r\n\tError erasing file:\r\n\t{1-%s}\r\n\t")
		TB_LOCALIZED(FILENAME_NOT_VALID,"RDE : {0-%s}Illegal filename:\r\n\t\"{1-%s}\"")
		TB_LOCALIZED(FILE_NOT_VALID,	"RDE : {0-%s}File incorrectly completed:\r\n\t{1-%s}")
		TB_LOCALIZED(CANT_RENAME_FILE,	"RDE : {0-%s}\r\n\tError renaming file:\r\n\t{1-%s}")
	END_TB_STRING_MAP()

	enum Command	//RDEcmd
	{
		NONE			= 0,

		SUB_TOTAL		= (RDEcmd) 0x0000,	// data related commands
		COL_TOTAL		= (RDEcmd) 0x8000,	// data related commands

		// object related commands
		NEXT_LINE		= 1,	// for display tables
		TITLE_LINE		= 2,	// for display tables
		INTER_LINE		= 3,	// for display tables

		INPUT_LOWER_DATA = 4,	// for rect fields
		INPUT_UPPER_DATA = 5,	// for rect fields

		NEW_PAGE		= 6,	// general commands

		CUSTOM_TITLE_LINE = 7,	// for display tables
		ARRAY_DATA = 8,			// for array fields

		//NEW_PAGE_CHANGE_LAYOUT		= 9,	//TODO LAYOUT general commands
		MESSAGE_BOX		= 10,	// general commands

		END_OF_REPORT	= 11,	// general commands
	};

	enum Direction
	{
    	FORWARD,
		BACKWARD
	};

	enum Origin
	{
		FROM_CURRENT,
        ENTIRE_SCOPE
	};

	enum Case
	{
		CASE_SENSITIVE,
        IGNORE_CASE
	};

	enum Match
	{
		EXACTLY,
		CONTAINED
	};

	enum RDEStatus
	{
		NO_ERROR_,
		LOCKED,
		BAD_READ,
		BAD_SEEK,
		END_OF_FILE,
		FILE_CLOSED,
		SHARING_VIOLATION,
		DISK_FULL,
		FATAL_ERROR
	};

	enum RDEDataType
	{
		UNKNOWN,
		INTERNAL_ID,
		ID_COMMAND,
		GEN_COMMAND
	};

	// constructos
	IRDEManager		() {}
};

#include "endh.dex"
