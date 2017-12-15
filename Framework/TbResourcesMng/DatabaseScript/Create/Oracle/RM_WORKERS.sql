CREATE TABLE "RM_WORKERS"(
	"WORKERID" NUMBER(10) NOT NULL,
	"PASSWORD" VARCHAR2(128) DEFAULT (''),
	"PASSWORDMUSTBECHANGED" CHAR(1) DEFAULT ('0'),
	"PASSWORDCANNOTCHANGE" CHAR(1) DEFAULT ('0'),
	"PASSWORDNEVEREXPIRE" CHAR(1) DEFAULT ('1'),
	"PASSWORDNOTRENEWABLE" CHAR(1) DEFAULT ('0'),
	"PASSWORDEXPIRATIONDATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
	"PASSWORDATTEMPTSNUMBER" NUMBER(6) DEFAULT (0),
	"TITLE" VARCHAR2(8) DEFAULT (''),
	"NAME" VARCHAR2(32) DEFAULT (''),
	"LASTNAME" VARCHAR2(64) DEFAULT (''),
	"GENDER" NUMBER(10) DEFAULT(2097152),
	"COMPANYLOGIN" VARCHAR2(50) DEFAULT (''),
	"DOMICILYADDRESS" VARCHAR2(128) DEFAULT (''),
    "DOMICILYCITY" VARCHAR2 (64) DEFAULT (''),
	"DOMICILYCOUNTY" VARCHAR2(3) DEFAULT (''),
    "DOMICILYZIP" VARCHAR2 (10) DEFAULT (''),
	"DOMICILYCOUNTRY" VARCHAR2(64) DEFAULT (''),
    "DOMICILYFC" VARCHAR2 (20) DEFAULT (''),
	"DOMICILYISOCODE" VARCHAR2(2) DEFAULT (''),
	"TELEPHONE1" VARCHAR2(20) DEFAULT (''),
	"TELEPHONE2" VARCHAR2(20) DEFAULT (''),
	"TELEPHONE3" VARCHAR2(20) DEFAULT (''),
	"TELEPHONE4" VARCHAR2(20) DEFAULT (''),
	"EMAIL" VARCHAR2(64) DEFAULT (''),
	"URL" VARCHAR2(64) DEFAULT (''),
	"SKYPEID" VARCHAR2(64) DEFAULT (''),
	"COSTCENTER" VARCHAR2(8) DEFAULT (''),
	"HOURLYCOST" FLOAT(126)  DEFAULT (0),
	"NOTES" VARCHAR2(64) DEFAULT (''),
	"DATEOFBIRTH" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
	"CITYOFBIRTH" VARCHAR2(32) DEFAULT (''),
	"CIVILSTATUS" VARCHAR2(16) DEFAULT (''),
	"REGISTERNUMBER" VARCHAR2(16) DEFAULT (''),
	"EMPLOYMENTDATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
	"RESIGNATIONDATE" DATE  DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
	"IMAGEPATH" /*[SKIPUNICODE]*/ VARCHAR2(128) DEFAULT (''),
	"HIDEONLAYOUT"CHAR(1) DEFAULT ('0'),
	"DISABLED" CHAR(1) DEFAULT ('0'),
	"LATITUDE" VARCHAR2(16) DEFAULT (''),
	"LONGITUDE" VARCHAR2(16) DEFAULT (''),
    "PIN" VARCHAR2 (8) DEFAULT (''),
	"BRANCH" VARCHAR2 (8) DEFAULT (''),
    "ADDRESS2" VARCHAR2 (64) DEFAULT (''),
    "STREETNO" VARCHAR2 (10) DEFAULT (''),
    "DISTRICT" VARCHAR2 (64) DEFAULT (''),
    "FEDERALSTATE" VARCHAR2 (2) DEFAULT (''),
	"ISRSENABLED" CHAR (1) DEFAULT ('0'),
   CONSTRAINT "PK_RM_WORKERS" PRIMARY KEY 
    (
        "WORKERID"
    )
)
GO

CREATE INDEX "IX_RM_WORKERS_1" ON "RM_WORKERS" ("COMPANYLOGIN", "WORKERID")
GO

CREATE TABLE "RM_WORKERSDETAILS" (
    "WORKERID" NUMBER(10) NOT NULL ,
   	"ISWORKER" CHAR(1) NOT NULL,
	"CHILDRESOURCETYPE" VARCHAR2(8) NOT NULL ,
    "CHILDRESOURCECODE" VARCHAR2(8) NOT NULL ,   
    "CHILDWORKERID" NUMBER(10) NOT NULL ,
   CONSTRAINT "PK_RM_WORKERSDETAILS" PRIMARY KEY 
    (
        "WORKERID",
        "ISWORKER",
        "CHILDRESOURCETYPE",
        "CHILDRESOURCECODE",
        "CHILDWORKERID"
    ),
   CONSTRAINT "FK_RM_WORKERSDETAILS_00" FOREIGN KEY
    (
        "WORKERID"
    ) REFERENCES "RM_WORKERS" (
       "WORKERID"
    )    
)
GO

CREATE TABLE "RM_WORKERSFIELDS"(
	"WORKERID" NUMBER(10) NOT NULL,
	"LINE" NUMBER(6) NOT NULL,
	"FIELDNAME" VARCHAR2(64) DEFAULT (''),
	"FIELDVALUE" VARCHAR2(256) DEFAULT (''),
	"NOTES" VARCHAR2(64) DEFAULT (''),
	"HIDEONLAYOUT" CHAR(1) DEFAULT ('0'),
   CONSTRAINT "PK_RM_WORKERSFIELDS" PRIMARY KEY 
    (
        "WORKERID",
        "LINE"
    ),
   CONSTRAINT "FK_RM_WORKERSFIELDS_00" FOREIGN KEY
    (
        "WORKERID"
    ) REFERENCES "RM_WORKERS" (
        "WORKERID"
    )    
)
GO

CREATE TABLE "RM_WORKERSARRANGEMENTS"(
	"WORKERID" NUMBER(10) NOT NULL,
	"LINE" NUMBER(6) NOT NULL,
	"ARRANGEMENT" VARCHAR2(8) DEFAULT (''),
	"ARRANGEMENTLEVEL" VARCHAR2(32) DEFAULT (''),
	"BASICPAY" FLOAT(126) DEFAULT (0),
	"TOTALPAY" FLOAT(126) DEFAULT (0),
	"FROMDATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
	"TODATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
	"NOTES" VARCHAR2(64) DEFAULT (''),
   CONSTRAINT "PK_RM_WORKERSARRANGEMENTS" PRIMARY KEY 
    (
        "WORKERID",
        "LINE"
    ),
   CONSTRAINT "FK_RM_WORKERSARRANGEMENTS_00" FOREIGN KEY
    (
        "WORKERID"
    ) REFERENCES "RM_WORKERS" (
        "WORKERID"
    )    
)
GO

CREATE TABLE "RM_WORKERSABSENCES"(
	"WORKERID" NUMBER(10) NOT NULL,
	"REASON" VARCHAR2(10) NOT NULL,
	"STARTINGDATE" DATE NOT NULL,
	"ENDINGDATE" DATE DEFAULT (TO_DATE('31-12-1799','DD-MM-YYYY')),
	"MANAGER" NUMBER(10) DEFAULT (0),
	"NOTES" VARCHAR2(64) DEFAULT (''),
	CONSTRAINT "PK_RM_WORKERSABSENCES" PRIMARY KEY 
	(
		"WORKERID" ,
		"REASON" ,
		"STARTINGDATE" 
	),
    CONSTRAINT "FK_RM_WORKERSABSENCES_00" FOREIGN KEY
    (
        "WORKERID"
    ) REFERENCES "RM_WORKERS" (
        "WORKERID"
    )    
)
GO

CREATE TABLE "RM_ARRANGEMENTS"(
	"ARRANGEMENT" VARCHAR2(8) NOT NULL,
	"DESCRIPTION" VARCHAR2(32) DEFAULT (''),
	"ARRANGEMENTLEVEL" VARCHAR2(32) DEFAULT (''),
	"BASICPAY" FLOAT(126) DEFAULT (0),
	"TOTALPAY" FLOAT(126) DEFAULT (0),
	"WORKINGHOURS" NUMBER(10) DEFAULT (0),
	"NOTES" VARCHAR2(64) DEFAULT (''),
	CONSTRAINT "PK_RM_ARRANGEMENTS" PRIMARY KEY  
	(
		"ARRANGEMENT" 
	)
)
GO