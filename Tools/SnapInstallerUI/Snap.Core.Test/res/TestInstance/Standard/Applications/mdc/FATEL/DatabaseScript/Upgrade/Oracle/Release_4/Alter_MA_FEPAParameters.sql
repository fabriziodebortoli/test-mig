ALTER TABLE "MA_FEPAPARAMETERS"
ADD    "LINKFEPA" 			VARCHAR2	(256)  DEFAULT('')
GO

UPDATE "MA_FEPAPARAMETERS" SET "LINKFEPA" = '' WHERE "LINKFEPA" IS NULL
GO