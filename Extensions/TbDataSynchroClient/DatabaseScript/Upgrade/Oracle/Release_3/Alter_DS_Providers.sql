ALTER TABLE "DS_PROVIDERS"
ADD	"IAFMODULES" VARCHAR2(256) DEFAULT('') 
GO

UPDATE "DS_PROVIDERS" SET "IAFMODULES" = ''  WHERE IAFMODULES is NULL
GO

