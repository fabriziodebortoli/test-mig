
ALTER TABLE "MA_TAXCODES" 
	ADD "EICODE" VARCHAR2 (8) DEFAULT ('')
GO

UPDATE "MA_TAXCODES" SET "EICODE" = '' WHERE "EICODE" IS NULL
GO


