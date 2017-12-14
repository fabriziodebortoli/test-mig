--TaxJournal in  DMS_SOSDocument
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('TaxJournal'))
ALTER TABLE [dbo].[DMS_SOSDocument] 
ADD [TaxJournal] varchar(8) NULL CONSTRAINT DF_DMS_SOSDocument_TaxJournal DEFAULT('')
GO

UPDATE [dbo].[DMS_SOSDocument] SET [dbo].[DMS_SOSDocument].[TaxJournal] = ''
WHERE [dbo].[DMS_SOSDocument].[TaxJournal] IS NULL
GO

--DocumentType in  DMS_SOSDocument
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('DocumentType'))
ALTER TABLE [dbo].[DMS_SOSDocument] 
ADD [DocumentType] varchar(25) NULL CONSTRAINT DF_DMS_SOSDocument_DocumentType DEFAULT('')
GO

UPDATE [dbo].[DMS_SOSDocument] SET [dbo].[DMS_SOSDocument].[DocumentType] = ''
WHERE [dbo].[DMS_SOSDocument].[DocumentType] IS NULL
GO


CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument4] ON [dbo].[DMS_SOSDocument] ([TaxJournal] ASC) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument5] ON [dbo].[DMS_SOSDocument] ([DocumentType] ASC) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_DMS_SOSDocument6] ON [dbo].[DMS_SOSDocument] ([TaxJournal] ASC, [DocumentType] ASC) ON [PRIMARY]
GO