
#pragma once

#include "parsobj.h"		// base ParsedControl class
#include "parsedt.h"		// Edit Control
#include "parslbx.h"		// ListBox
#include "parscbx.h"		// ComboBox
#include "parsbtn.h"		// Push Button & Enumerations
#include "parsobjManaged.h"		// Managed Control Classes
#include "TBBreadCrumb.h"		// Managed Control Classes


// Edit Control Notification Codes
#define ON_EN_VALUE_CHANGED(id, memberFxn) \
	ON_CONTROL(EN_VALUE_CHANGED, id, memberFxn)

#define ON_EN_VALUE_CHANGED_FOR_FIND(id, memberFxn) \
	ON_CONTROL(EN_VALUE_CHANGED_FOR_FIND, id, memberFxn)

#define ON_EN_VALUE_CHANGED_RANGE(idi, idf, memberFxn) \
	ON_CONTROL_RANGE(EN_VALUE_CHANGED, idi, idf, memberFxn)

#define ON_EN_VALUE_CHANGED_FOR_FIND_RANGE(idi, idf, memberFxn) \
	ON_CONTROL_RANGE(EN_VALUE_CHANGED_FOR_FIND, idi, idf, memberFxn)


#define ON_CTRL_STATE_CHANGED(id, memberFxn) \
	ON_CONTROL(EN_CTRL_STATE_CHANGED, id, memberFxn)

#define ON_CTRL_STATE_RANGE(idi, idf, memberFxn) \
	ON_CONTROL_RANGE(EN_CTRL_STATE_CHANGED, idi, idf, memberFxn)


// Edit Control Json Notification Codes
#define ON_JSON_VALUE_CHANGED(id, memberFxn) \
	ON_CONTROL(EN_VALUE_CHANGED, GET_IDC(id), memberFxn)

#define ON_JSON_VALUE_CHANGED_FOR_FIND(id, memberFxn) \
	ON_CONTROL(EN_VALUE_CHANGED_FOR_FIND, GET_IDC(id), memberFxn)


#define ON_JSON_VALUE_CHANGED_EX(parentID, id, memberFxn) \
	ON_CONTROL(EN_VALUE_CHANGED, GET_IDC(#parentID "_" #id), memberFxn)

#define ON_JSON_VALUE_CHANGED_FOR_FIND_EX(parentID, id, memberFxn) \
	ON_CONTROL(EN_VALUE_CHANGED_FOR_FIND, GET_IDC(#parentID "_" #id), memberFxn)


//includere alla fine degli include del .H
#include "beginh.dex"

//////////////////////////////////
//	INSERT HERE CLASS DEFINITION
//////////////////////////////////

#include "endh.dex"
