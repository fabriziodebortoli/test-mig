--SosCodeType in  DMS_Collection
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_Collection' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('SosDocClass'))
ALTER TABLE [dbo].[DMS_Collection] 
ADD [SosDocClass] varchar(15) NULL CONSTRAINT DF_DMS_Collection_SosDocClass DEFAULT('')
GO

UPDATE [dbo].[DMS_Collection] SET [dbo].[DMS_Collection].[SosDocClass] = ''
WHERE [dbo].[DMS_Collection].[SosDocClass] IS NULL
GO

--Version in  DMS_Collection
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_Collection' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('Version'))
ALTER TABLE [dbo].[DMS_Collection] 
ADD [Version] [int] NULL CONSTRAINT DF_DMS_Collection_Version DEFAULT(1)
GO

UPDATE [dbo].[DMS_Collection] SET [dbo].[DMS_Collection].[Version] = 1
WHERE [dbo].[DMS_Collection].[Version] IS NULL
GO	

--adding  SosPosition for SosConnector functionalty
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_CollectionsFields' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('SosPosition'))
ALTER TABLE [dbo].[DMS_CollectionsFields] 
ADD [SosPosition] [int] NULL CONSTRAINT DF_DMS_CollectionsFields_SosPosition DEFAULT(-1)
GO

UPDATE [dbo].[DMS_CollectionsFields] SET [dbo].[DMS_CollectionsFields].[SosPosition] = -1
WHERE [dbo].[DMS_CollectionsFields].[SosPosition] IS NULL
GO

--adding HKLName for SosConnector functionalty
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_CollectionsFields' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('HKLName'))
ALTER TABLE [dbo].[DMS_CollectionsFields] 
ADD [HKLName] [varchar] (128) NULL CONSTRAINT DF_DMS_CollectionsFields_HKLName DEFAULT('')
GO

UPDATE [dbo].[DMS_CollectionsFields] SET [dbo].[DMS_CollectionsFields].[HKLName] = ''
WHERE [dbo].[DMS_CollectionsFields].[HKLName] IS NULL
GO

--adding SosMandatory for SosConnector functionalty
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_CollectionsFields' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('SosMandatory'))
ALTER TABLE [dbo].[DMS_CollectionsFields] 
ADD [SosMandatory] [bit] NULL CONSTRAINT DF_DMS_CollectionsFields_SosMandatory DEFAULT(0)
GO

UPDATE [dbo].[DMS_CollectionsFields] SET [dbo].[DMS_CollectionsFields].[SosMandatory] = 0
WHERE [dbo].[DMS_CollectionsFields].[SosMandatory] IS NULL
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[DMS_TextExtensions]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.xls')
INSERT INTO [dbo].[DMS_TextExtensions]([ExtensionType]) VALUES ('.xlsx')
END
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_Attachment' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('AbsoluteCode'))
ALTER TABLE [dbo].[DMS_Attachment]
ADD [AbsoluteCode] [varchar](50) NULL CONSTRAINT DF_DMS_Attachment_AbsoluteCode DEFAULT ('')
GO

UPDATE [dbo].[DMS_Attachment] SET [dbo].[DMS_Attachment].[AbsoluteCode] = ''
WHERE [dbo].[DMS_Attachment].[AbsoluteCode] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_Attachment' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('LotID'))
ALTER TABLE [dbo].[DMS_Attachment]
ADD [LotID] [varchar](50) NULL CONSTRAINT DF_DMS_Attachment_LotID DEFAULT ('')
GO

UPDATE [dbo].[DMS_Attachment] SET [dbo].[DMS_Attachment].[LotID] = ''
WHERE [dbo].[DMS_Attachment].[LotID] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_Attachment' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('RegistrationDate'))
ALTER TABLE [dbo].[DMS_Attachment]
ADD [RegistrationDate] [datetime] NULL
GO
