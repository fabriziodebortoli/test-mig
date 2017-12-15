CREATE TABLE "MA_EIEVENTVIEWER" ( 
	"DOCCRTYPE" NUMBER(10) NOT NULL,
	"DOCID" NUMBER(10) NOT NULL,
	"LINE" NUMBER(6) NOT NULL,
	"EVENTDATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
    "EVENT_TYPE" NUMBER(10) DEFAULT(32243712),
	"EVENT_DESCRIPTION" VARCHAR2(255) DEFAULT (''),
	"EVENT_XML" NCLOB DEFAULT(''),
	"EVENT_STRING1" VARCHAR2 (15) DEFAULT (''),
	"EVENT_STRING2" VARCHAR2 (3) DEFAULT (''),
    CONSTRAINT "PK_EIEVENTVIEWER" PRIMARY KEY
    (
		"DOCCRTYPE",
        "DOCID",
		"LINE"
    )
)
GO