ALTER TABLE "MA_CUSTSUPP"
ADD     "ADMINISTRATIONREFERENCE" VARCHAR2(6) DEFAULT('')
GO

UPDATE "MA_CUSTSUPP" SET "ADMINISTRATIONREFERENCE" = '' WHERE "ADMINISTRATIONREFERENCE" IS NULL
GO

ALTER TABLE "MA_CUSTSUPP"
ADD     "IMMEDIATELIKEACCOMPANYING"  CHAR(1) DEFAULT('0')
GO

UPDATE "MA_CUSTSUPP" SET "IMMEDIATELIKEACCOMPANYING" = '0' WHERE "IMMEDIATELIKEACCOMPANYING" IS NULL
GO
