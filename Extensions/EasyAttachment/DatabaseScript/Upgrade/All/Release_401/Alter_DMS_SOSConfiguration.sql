if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSConfiguration' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('ChunkDimension'))
ALTER TABLE [dbo].[DMS_SOSConfiguration] 
ADD [ChunkDimension] [int] NULL CONSTRAINT DF_DMS_SOSConfig_ChunkDim DEFAULT(20)
GO

UPDATE [dbo].[DMS_SOSConfiguration] SET [dbo].[DMS_SOSConfiguration].[ChunkDimension] = 20
WHERE [dbo].[DMS_SOSConfiguration].[ChunkDimension] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSConfiguration' and dbo.sysobjects.id = dbo.syscolumns.id 
and dbo.syscolumns.name IN ('EnvelopeDimension'))
ALTER TABLE [dbo].[DMS_SOSConfiguration] 
ADD [EnvelopeDimension] [int] NULL CONSTRAINT DF_DMS_SOSConfig_MaxEnvDim DEFAULT(600)
GO

UPDATE [dbo].[DMS_SOSConfiguration] SET [dbo].[DMS_SOSConfiguration].[EnvelopeDimension] = 600
WHERE [dbo].[DMS_SOSConfiguration].[EnvelopeDimension] IS NULL
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSConfiguration' and dbo.sysobjects.id = dbo.syscolumns.id)
UPDATE [DMS_SOSConfiguration]
SET DocClasses.modify('replace value of (SOSConfigurationState/DocumentClasses/DocClassesList/DocClass/ERPDocNamespaces/ERPSOSDocumentType/@erpDocNS[.="Document.ERP.AccountingDMS.AddOnsAccounting.ArchiveDeclarationIntentJournal"])[1] with "Document.ERP.Accounting_IT.Documents.DeclarationIntentJournal"')
WHERE DocClasses IS NOT NULL
GO

if exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'DMS_SOSConfiguration' and dbo.sysobjects.id = dbo.syscolumns.id)
UPDATE [DMS_SOSConfiguration]
SET DocClasses.modify('replace value of (SOSConfigurationState/DocumentClasses/DocClassesList/DocClass/ERPDocNamespaces/ERPSOSDocumentType/@erpDocNS[.="Document.ERP.AccountingDMS.AddOnsAccounting.ArchiveFixedAssetsJournal"])[1] with "Document.ERP.FixedAssets.Documents.FixedAssetsJournal"')
WHERE DocClasses IS NOT NULL
GO