CREATE TABLE "RM_RESOURCETYPES" (
	"RESOURCETYPE" VARCHAR2(8) NOT NULL ,
    "DESCRIPTION" VARCHAR2(64) DEFAULT('') ,
    "IMAGEPATH" /*[SKIPUNICODE]*/ VARCHAR2(128) DEFAULT('') ,
CONSTRAINT "PK_RM_RESOURCETYPES" PRIMARY KEY 
    (
        "RESOURCETYPE"
    )
)
GO

CREATE TABLE "RM_RESOURCES" (
	"RESOURCETYPE" VARCHAR2(8) NOT NULL ,
    "RESOURCECODE" VARCHAR2(8) NOT NULL ,
    "DESCRIPTION" VARCHAR2(64) DEFAULT('') ,
    "MANAGER" NUMBER(10) DEFAULT(0) ,
    "NOTES" VARCHAR2(64) DEFAULT('') ,
    "IMAGEPATH" /*[SKIPUNICODE]*/ VARCHAR2(128) DEFAULT('') ,
    "COSTCENTER" VARCHAR2(8) DEFAULT('') ,
    "DISABLED" CHAR(1) DEFAULT('') ,
	"HIDEONLAYOUT" CHAR(1) DEFAULT ('0'),
	"DOMICILYADDRESS" VARCHAR2(128) DEFAULT (''),
    "DOMICILYCITY" VARCHAR2 (64) DEFAULT (''),
	"DOMICILYCOUNTY" VARCHAR2(3) DEFAULT (''),
    "DOMICILYZIP" VARCHAR2 (10) DEFAULT (''),
	"DOMICILYCOUNTRY" VARCHAR2(64) DEFAULT (''),
	"TELEPHONE1" VARCHAR2(20) DEFAULT (''),
	"TELEPHONE2" VARCHAR2(20) DEFAULT (''),
	"TELEPHONE3" VARCHAR2(20) DEFAULT (''),
	"TELEPHONE4" VARCHAR2(20) DEFAULT (''),
	"EMAIL" VARCHAR2(64) DEFAULT (''),
	"URL" VARCHAR2(64) DEFAULT (''),
	"SKYPEID" VARCHAR2(64) DEFAULT (''),
	"BRANCH" VARCHAR2(8) DEFAULT (''),
	"LATITUDE" VARCHAR2(16) DEFAULT (''),
	"LONGITUDE" VARCHAR2(16) DEFAULT (''),
    "ADDRESS2" VARCHAR2 (64) DEFAULT (''),
    "STREETNO" VARCHAR2 (10) DEFAULT (''),
    "DISTRICT" VARCHAR2 (64) DEFAULT (''),
    "FEDERALSTATE" VARCHAR2 (2) DEFAULT (''),
    "ISOCOUNTRYCODE" VARCHAR2 (2) DEFAULT (''),
   CONSTRAINT "PK_RM_RESOURCES" PRIMARY KEY
    (
        "RESOURCETYPE",
        "RESOURCECODE"
    )
)
GO

CREATE TABLE "RM_RESOURCESDETAILS" (
	"RESOURCETYPE" VARCHAR2(8) NOT NULL ,
    "RESOURCECODE" VARCHAR2(8) NOT NULL ,
   	"ISWORKER" CHAR(1) NOT NULL,
	"CHILDRESOURCETYPE" VARCHAR2(8) NOT NULL ,
    "CHILDRESOURCECODE" VARCHAR2(8) NOT NULL ,   
    "CHILDWORKERID" NUMBER(10) NOT NULL ,
   CONSTRAINT "PK_RM_RESOURCESDETAILS" PRIMARY KEY 
    (
        "RESOURCETYPE",
        "RESOURCECODE",
        "ISWORKER",
        "CHILDRESOURCETYPE",
        "CHILDRESOURCECODE",
        "CHILDWORKERID"
    ),
   CONSTRAINT "FK_RM_RESOURCESDETAILS_00" FOREIGN KEY
    (
        "RESOURCETYPE",
        "RESOURCECODE"
    ) REFERENCES "RM_RESOURCES" (
        "RESOURCETYPE",
        "RESOURCECODE"
    )    
)
GO

CREATE TABLE "RM_RESOURCESFIELDS"(
	"RESOURCETYPE" VARCHAR2(8) NOT NULL ,
    "RESOURCECODE" VARCHAR2(8) NOT NULL ,
	"LINE" NUMBER(6) NOT NULL,
	"FIELDNAME" VARCHAR2(64) DEFAULT (''),
	"FIELDVALUE" VARCHAR2(256) DEFAULT (''),
	"NOTES" VARCHAR2(64) DEFAULT (''),
	"HIDEONLAYOUT" CHAR(1) DEFAULT ('0'),
   CONSTRAINT "PK_RM_RESOURCESFIELDS" PRIMARY KEY 
    (
        "RESOURCETYPE",
        "RESOURCECODE",
        "LINE"
    ),
   CONSTRAINT "FK_RM_RESOURCESFIELDS_00" FOREIGN KEY
    (
        "RESOURCETYPE",
        "RESOURCECODE"
    ) REFERENCES "RM_RESOURCES" (
        "RESOURCETYPE",
        "RESOURCECODE"
    )    
)
GO

CREATE TABLE "RM_RESOURCESABSENCES" (
	"RESOURCETYPE" VARCHAR2(8) NOT NULL ,
    "RESOURCECODE" VARCHAR2(8) NOT NULL ,
    "REASON" VARCHAR2(8) NOT NULL ,
	"STARTINGDATE" DATE NOT NULL ,
    "ENDINGDATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')) ,   
    "MANAGER" NUMBER(10) DEFAULT(0) ,
    "NOTES" VARCHAR2(64) DEFAULT('') ,
   CONSTRAINT "PK_RM_RESOURCESABSENCES" PRIMARY KEY 
    (
        "RESOURCETYPE",
        "RESOURCECODE",
        "REASON",
        "STARTINGDATE"
    ),
   CONSTRAINT "FK_RM_RESOURCESABSENCES_00" FOREIGN KEY
    (
        "RESOURCETYPE",
        "RESOURCECODE"
    ) REFERENCES "RM_RESOURCES" (
        "RESOURCETYPE",
        "RESOURCECODE"
    )    
)
GO