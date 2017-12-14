if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
    dbo.sysobjects.name = 'MA_EI_ITParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
    and dbo.syscolumns.name = 'OrderLineNumberReference')
ALTER TABLE [dbo].[MA_EI_ITParameters]
   ADD [OrderLineNumberReference]	[char]	(1) NULL CONSTRAINT DF_EIITParame_OrLiRef_01 DEFAULT('0')
GO


UPDATE [dbo].[MA_EI_ITParameters] SET [dbo].[MA_EI_ITParameters].[OrderLineNumberReference] = '' WHERE [dbo].[MA_EI_ITParameters].[OrderLineNumberReference] IS NULL
GO
