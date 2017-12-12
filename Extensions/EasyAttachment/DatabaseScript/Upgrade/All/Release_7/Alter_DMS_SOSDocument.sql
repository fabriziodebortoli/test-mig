IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = 'DMS_SOSDocument' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = 'DocumentType')
BEGIN
ALTER TABLE [dbo].[DMS_SOSDocument] ALTER COLUMN [DocumentType] [varchar] (50)
END
GO

UPDATE [dbo].[DMS_SOSDocument] SET [DocumentType] = 'Registrazione ricevuti' WHERE [DocumentType] = 'Registrazione recevuti'
GO