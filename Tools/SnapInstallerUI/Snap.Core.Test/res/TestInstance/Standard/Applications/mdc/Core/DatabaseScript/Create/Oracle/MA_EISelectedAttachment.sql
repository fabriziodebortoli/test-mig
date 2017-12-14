CREATE TABLE "MA_EISELECTEDATTACHMENT" ( 
	"MDCDOCUMENTID"		NUMBER (10)		NOT NULL,
	"DOCNAMESPACE"		VARCHAR2 (256)  NOT NULL,
	"PRIMARYKEYVALUE"	NUMBER (10)		NOT NULL,
	"DESCRIPTIONVALUE"	VARCHAR2 (256)	DEFAULT(''),
	"ISSETBYDEFAULT"		CHAR (1)		DEFAULT ('0'),
    CONSTRAINT "PK_EISELECTEDATTACHMEN" PRIMARY KEY
    (
		"MDCDOCUMENTID",
		"DOCNAMESPACE",
		"PRIMARYKEYVALUE"
    )
)
GO