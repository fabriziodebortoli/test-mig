ALTER TABLE "MA_CUSTSUPP"
ADD     "IPACODE" VARCHAR2(7) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "IPACODE" = '' WHERE "IPACODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD     "EORICODE" VARCHAR2(17) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "EORICODE" = '' WHERE "EORICODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPPBRANCHES"
ADD     "IPACODE" VARCHAR2(7) DEFAULT('')
GO

UPDATE "MA_CUSTSUPPBRANCHES" SET "IPACODE" = '' WHERE "IPACODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD     "ADMINISTRATIONREFERENCE" VARCHAR2(6) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "ADMINISTRATIONREFERENCE" = '' WHERE "ADMINISTRATIONREFERENCE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPPBRANCHES"
ADD     "ADMINISTRATIONREFERENCE" VARCHAR2(6) DEFAULT('')
GO

UPDATE "MA_CUSTSUPPBRANCHES" SET "ADMINISTRATIONREFERENCE" = '' WHERE "ADMINISTRATIONREFERENCE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD     "IMMEDIATELIKEACCOMPANYING"  CHAR(1) DEFAULT('0')
GO

UPDATE "MA_CUSTSUPP" SET "IMMEDIATELIKEACCOMPANYING" = '0' WHERE "IMMEDIATELIKEACCOMPANYING" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD     "ELECTRONICINVOICING"  CHAR(1) DEFAULT('0')
GO

UPDATE "MA_CUSTSUPP" SET "ELECTRONICINVOICING" = '0' WHERE "ELECTRONICINVOICING" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "PERMANENTBRANCHCODE" VARCHAR2(8) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "PERMANENTBRANCHCODE" = '' WHERE "PERMANENTBRANCHCODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDISOCOUNTRYCODE" VARCHAR2(2) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDISOCOUNTRYCODE" = '' WHERE "FDISOCOUNTRYCODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDFISCALCODE" VARCHAR2(16) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDFISCALCODE" = '' WHERE "FDFISCALCODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDNATURALPERSON" CHAR (1) DEFAULT ('0')
GO

UPDATE "MA_CUSTSUPP" SET "FDNATURALPERSON" = '0' WHERE "FDNATURALPERSON" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDCOMPANYNAME" VARCHAR2(80) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDCOMPANYNAME" = '' WHERE "FDCOMPANYNAME" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDNAME" VARCHAR2(60) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDNAME" = '' WHERE "FDNAME" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDLASTNAME" VARCHAR2(60) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDLASTNAME" = '' WHERE "FDLASTNAME" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FISCALREGIME" VARCHAR2(8) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FISCALREGIME" = '' WHERE "FISCALREGIME" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDFISCALCODEID" VARCHAR2(28) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDFISCALCODEID" = '' WHERE "FDFISCALCODEID" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDEORICODE" VARCHAR2(17) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDEORICODE" = '' WHERE "FDEORICODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD "FDTITLECODE" VARCHAR2(8) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "FDTITLECODE" = '' WHERE "FDTITLECODE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD     "SENDBYCERTIFIEDEMAIL"  CHAR(1) DEFAULT('0')
GO

UPDATE "MA_CUSTSUPP" SET "SENDBYCERTIFIEDEMAIL" = '0' WHERE "SENDBYCERTIFIEDEMAIL" IS NULL
GO
