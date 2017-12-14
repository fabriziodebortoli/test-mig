if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ArchivedDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('ModifierID'))
ALTER TABLE [dbo].[DMS_ArchivedDocument] 
ADD [ModifierID] [int] NULL CONSTRAINT DF_DMS_ArchivedDocument_ModifierID DEFAULT(0)
GO


