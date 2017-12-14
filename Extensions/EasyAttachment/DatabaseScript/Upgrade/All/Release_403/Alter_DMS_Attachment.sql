
--[DMS_Attachment]
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_Attachment' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('IsForMail'))
ALTER TABLE [dbo].[DMS_Attachment] 
ADD [IsForMail] [bit] NULL CONSTRAINT DF_DMS_Attachmentg_IsForMail DEFAULT(0)
GO

UPDATE [dbo].[DMS_Attachment] SET [dbo].[DMS_Attachment].[IsForMail] = 0
GO