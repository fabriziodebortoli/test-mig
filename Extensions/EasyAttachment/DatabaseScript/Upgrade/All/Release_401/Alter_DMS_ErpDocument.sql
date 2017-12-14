--adding TbGuid to manage the new ERP document Key
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_ErpDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('TBGuid'))
ALTER TABLE [dbo].[DMS_ErpDocument] 
ADD [TBGuid] [uniqueidentifier] NULL CONSTRAINT DF_DMS_ErpDocument_TBGuid DEFAULT ('00000000-0000-0000-0000-000000000000')
GO
UPDATE [dbo].[DMS_ErpDocument] SET [TBGuid] = '00000000-0000-0000-0000-000000000000' WHERE [TBGuid] IS NULL
GO