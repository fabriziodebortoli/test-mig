--DMS_SOSConfiguration
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSConfiguration' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('FTPSend'))
ALTER TABLE [dbo].[DMS_SOSConfiguration] 
ADD [FTPSend] [bit] NULL CONSTRAINT DF_DMS_SOSConfig_FTPSend DEFAULT(0)
GO

UPDATE [dbo].[DMS_SOSConfiguration] SET [dbo].[DMS_SOSConfiguration].[FTPSend] = 0
WHERE [dbo].[DMS_SOSConfiguration].[FTPSend] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSConfiguration' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('FTPSharedFolder'))
ALTER TABLE [dbo].[DMS_SOSConfiguration] 
ADD [FTPSharedFolder] [varchar](512) NULL CONSTRAINT DF_DMS_SOSConfig_FTPSharedFolder DEFAULT('')
GO

UPDATE [dbo].[DMS_SOSConfiguration] SET [dbo].[DMS_SOSConfiguration].[FTPSharedFolder] = ''
WHERE [dbo].[DMS_SOSConfiguration].[FTPSharedFolder] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSConfiguration' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('FTPUpdateDayOfWeek'))
ALTER TABLE [dbo].[DMS_SOSConfiguration] 
ADD [FTPUpdateDayOfWeek] [int] NULL CONSTRAINT DF_DMS_SOSConfig_FTPUpdateDayOfWeek DEFAULT(7)
GO

UPDATE [dbo].[DMS_SOSConfiguration] SET [dbo].[DMS_SOSConfiguration].[FTPUpdateDayOfWeek] = '7'
WHERE [dbo].[DMS_SOSConfiguration].[FTPUpdateDayOfWeek] IS NULL
GO

--DMS_SOSDocument
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSDocument' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('SendingType'))
BEGIN
ALTER TABLE [dbo].DMS_SOSDocument ADD [SendingType] [int] NULL CONSTRAINT [DF_DMS_SOSDocument_SendingType]  DEFAULT (0)
END
GO

UPDATE [dbo].[DMS_SOSDocument] SET [dbo].[DMS_SOSDocument].[SendingType] = 0
WHERE [dbo].[DMS_SOSDocument].[SendingType] IS NULL
GO

--DMS_SOSEnvelope
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSEnvelope' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('SendingType'))
BEGIN
ALTER TABLE [dbo].[DMS_SOSEnvelope] ADD [SendingType] [int] NULL CONSTRAINT [DF_DMS_SOSEnvelope_SendingType]  DEFAULT (0)
END
GO

UPDATE [dbo].[DMS_SOSEnvelope] SET [dbo].[DMS_SOSEnvelope].[SendingType] = 0
WHERE [dbo].[DMS_SOSEnvelope].[SendingType] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSEnvelope' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('CreationDate'))
BEGIN
ALTER TABLE [dbo].[DMS_SOSEnvelope] ADD [CreationDate] [datetime] NULL CONSTRAINT DF_DMS_SOSEnvelope_CreationDate DEFAULT (getdate())
END
GO

UPDATE [dbo].[DMS_SOSEnvelope] SET [dbo].[DMS_SOSEnvelope].[CreationDate] = [dbo].[DMS_SOSEnvelope].[DispatchDate]
WHERE [dbo].[DMS_SOSEnvelope].[CreationDate] IS NULL
GO