CREATE TABLE "MA_EI_ITADDITIONALDATA" (
    "FIELDNAME" VARCHAR2 (256) NOT NULL,
    "XMLSECTION" NUMBER (10) DEFAULT (31981568),
    "MULTIPLE" CHAR (1) DEFAULT ('0'),
    "MANDATORY" CHAR (1) DEFAULT ('0'),
    "DATATYPE" VARCHAR2 (256) DEFAULT (''),
    "MINLENGTH" NUMBER (6) DEFAULT (0),
    "MAXLENGTH" NUMBER (6) DEFAULT (0),
    "UPPERCASE" CHAR (1) DEFAULT ('0'),
    "MINVALUE" NUMBER (6) DEFAULT (0),
    "MAXVALUE" NUMBER (6) DEFAULT (0),
    "DISABLED" CHAR (1) DEFAULT ('0'),
    "FROMVERSION" VARCHAR2 (8) DEFAULT (''),
	"TOVERSION" VARCHAR2 (8) DEFAULT (''),
	"VIEWTYPE" VARCHAR2 (32) DEFAULT (''),
	"NODENUMBER" VARCHAR2 (16) DEFAULT (''),
    CONSTRAINT "PK_EI_ITADDITIONALDATA" PRIMARY KEY
    (
        "FIELDNAME"
    )
)
GO