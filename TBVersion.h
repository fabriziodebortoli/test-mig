#pragma once

#define QUOTE(name) #name
#define STR(macro) QUOTE(macro)


#define TB_FILEVERSION				1,0,0,0
#define TB_PRODUCTVERSION			1.0.0.0
#define TB_PRODUCTNAME				"TaskBuilder"
#define TB_COPYRIGHT				"Task Builder .NET: (c) Microarea S.p.A.  All rights reserved."
#define TB_COMPANYNAME				"Microarea S.p.A."
#define TB_FILEVERSIONSTRING		STR(TB_FILEVERSION)
#define TB_PRODUCTVERSIONSTRING 	STR(TB_PRODUCTVERSION)
