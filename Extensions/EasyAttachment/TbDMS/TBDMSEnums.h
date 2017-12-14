﻿//=================================================================
// module name	: TBDMSEnums.h 
//=================================================================

#pragma once

const WORD    E_DELETE_ATTACH_ACTION									= 750;
const DWORD   E_DELETE_ATTACH_DELETE									= MAKELONG(0, 750); //49152000
const DWORD   E_DELETE_ATTACH_KEEP										= MAKELONG(1, 750); //49152001
const DWORD   E_DELETE_ATTACH_ASK										= MAKELONG(2, 750); //49152002
const DWORD   E_DELETE_ATTACH_ACTION_DEFAULT							= E_DELETE_ATTACH_KEEP;

//usato anche per le opzioni di duplicazione del barcode, ma con diversa descrizione
const WORD    E_DUPLICATE_ACTION										= 751;
const DWORD   E_DUPLICATE_ASK											= MAKELONG(0, 751); //49217536
const DWORD   E_DUPLICATE_REPLACE										= MAKELONG(1, 751); //49217537
const DWORD   E_DUPLICATE_KEEP_BOTH										= MAKELONG(2, 751); //49217538
const DWORD   E_DUPLICATE_USE_EXISTING									= MAKELONG(3, 751); //49217539
const DWORD   E_DUPLICATE_REFUSE_ATTACH									= MAKELONG(4, 751); //49217540
const DWORD   E_DUPLICATE_CANCEL										= MAKELONG(5, 751); //49217541  opzione da non mostrare nella combo.
const DWORD   E_DUPLICATE_ACTION_DEFAULT								= E_DUPLICATE_ASK;

const WORD    E_BARCODE_DETECT_ACTION									= 752;
const DWORD   E_BARCODE_DETECT_FIRST_PAGE								= MAKELONG(0, 752);  //49283072
const DWORD   E_BARCODE_DETECT_TILL_ONE_VALID							= MAKELONG(1, 752);  //49283073
const DWORD   E_BARCODE_DETECT_ACTION_DEFAULT							= E_BARCODE_DETECT_FIRST_PAGE;


const WORD    E_BOOKMARK_TYPE											= 753;
const DWORD   E_BOOKMARK_KEY											= MAKELONG(0, 753);  //49348608
const DWORD   E_BOOKMARK_BINDING										= MAKELONG(1, 753);  //49348609
const DWORD   E_BOOKMARK_CATEGORY										= MAKELONG(2, 753);  //49348610
const DWORD   E_BOOKMARK_EXTERNAL										= MAKELONG(3, 753);  //49348611
const DWORD   E_BOOKMARK_SOS_SPECIAL									= MAKELONG(4, 753);  //49348612
const DWORD   E_BOOKMARK_VARIABLE										= MAKELONG(5, 753);  //49348613
const DWORD   E_BOOKMARK_TYPE_DEFAULT									= E_BOOKMARK_BINDING;


const WORD    E_OPERATION_TYPE											= 754;
const DWORD   E_OPERATION_EQUAL											= MAKELONG(0, 754);  //49414144
const DWORD   E_OPERATION_DIFFERENT										= MAKELONG(1, 754);  //49414145
const DWORD   E_OPERATION_LIKE											= MAKELONG(2, 754);  //49414146
const DWORD   E_OPERATION_TYPE_DEFAULT									= E_OPERATION_EQUAL;


const WORD    E_LOGIC_OPERATOR_TYPE										= 755;
const DWORD   E_LOGIC_OPERATOR_AND										= MAKELONG(0, 755);  //49479680
const DWORD   E_LOGIC_OPERATOR_OR										= MAKELONG(1, 755);  //49479681
const DWORD   E_LOGIC_OPERATOR_TYPE_DEFAULT								= E_LOGIC_OPERATOR_AND;


