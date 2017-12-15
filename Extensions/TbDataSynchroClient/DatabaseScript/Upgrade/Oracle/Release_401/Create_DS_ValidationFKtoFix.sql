﻿CREATE TABLE "DS_VALIDATIONFKTOFIX"
(	
	"FKTOFIXID" NUMBER(10,0),  
	"PROVIDERNAME" VARCHAR2(20)  DEFAULT (''),  	
	"DOCNAMESPACE" VARCHAR2(255) DEFAULT (''),	
	"TABLENAME" VARCHAR2(255) DEFAULT (''),	
	"FIELDNAME" VARCHAR2(255) DEFAULT (''),	
	"VALUETOFIX" VARCHAR2(255) DEFAULT (''),	
	"RELATEDERRORS" NUMBER(10) DEFAULT(0),
	"VALIDATIONDATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),	
	CONSTRAINT "PK_DS_VALIDATIONFKTOFIX" PRIMARY KEY
	(
		"FKTOFIXID"
	)	
)
GO