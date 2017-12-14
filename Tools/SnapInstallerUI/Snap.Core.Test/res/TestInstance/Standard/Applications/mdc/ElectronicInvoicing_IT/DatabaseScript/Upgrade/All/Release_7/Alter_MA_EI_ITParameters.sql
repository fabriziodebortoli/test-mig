if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
    dbo.sysobjects.name = 'MA_EI_ITParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
    and dbo.syscolumns.name = 'StampChargesInSummary')
ALTER TABLE [dbo].[MA_EI_ITParameters]
   ADD [StampChargesInSummary]	[char]	(1) NULL CONSTRAINT DF_EIITParame_StCha_01 DEFAULT('0')
GO


UPDATE [dbo].[MA_EI_ITParameters] SET [dbo].[MA_EI_ITParameters].[StampChargesInSummary] = '' WHERE [dbo].[MA_EI_ITParameters].[StampChargesInSummary] IS NULL
GO
