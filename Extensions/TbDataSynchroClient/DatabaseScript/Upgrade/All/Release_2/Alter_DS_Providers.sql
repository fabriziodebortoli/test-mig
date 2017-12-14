if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DS_Providers' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('IsEAProvider'))
ALTER TABLE [dbo].[DS_Providers] 
	ADD [IsEAProvider] [char](1) NULL CONSTRAINT DF_Providers_IsEAProvider DEFAULT('0')
GO

UPDATE [dbo].[DS_Providers] SET [IsEAProvider] = '0' WHERE [IsEAProvider] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DS_Providers' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('TBGuid'))
ALTER TABLE [dbo].[DS_Providers]
ADD [TBGuid] [uniqueidentifier] NULL CONSTRAINT DF_Providers_TBGuid DEFAULT(0x00)
GO

UPDATE [dbo].[DS_Providers] SET [dbo].[DS_Providers].[TBGuid] = newid() WHERE [dbo].[DS_Providers].[TBGuid] IS NULL
GO

IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = 'DS_Transcoding' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = 'EntityID')
BEGIN
ALTER TABLE [dbo].[DS_Transcoding]
   ALTER COLUMN [EntityID] [varchar] (256)
END
GO