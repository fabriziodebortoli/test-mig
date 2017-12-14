if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DS_SynchronizationInfo' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('StartSynchDate'))
ALTER TABLE [dbo].DS_SynchronizationInfo 
	ADD StartSynchDate [datetime] NULL CONSTRAINT DF_SynchInfo_StartSynchDate DEFAULT ('17991231')
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DS_SynchronizationInfo' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('HasDirtyTbModified'))
ALTER TABLE [dbo].DS_SynchronizationInfo 
	ADD HasDirtyTbModified [char] (1) NOT NULL CONSTRAINT DF_SynchInfo_HasDirtyTbModified DEFAULT ('0')
GO
