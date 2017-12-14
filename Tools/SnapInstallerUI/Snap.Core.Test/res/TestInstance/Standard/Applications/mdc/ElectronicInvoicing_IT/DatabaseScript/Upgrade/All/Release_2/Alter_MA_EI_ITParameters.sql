if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
    dbo.sysobjects.name = 'MA_EI_ITParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
    and dbo.syscolumns.name = 'TaxJournalCreditNote')
ALTER TABLE [dbo].[MA_EI_ITParameters]
   ADD     [TaxJournalCreditNote] [varchar]   (8) NULL CONSTRAINT DF_EIITParame_TaxJournCN_00 DEFAULT('')
GO


UPDATE [dbo].[MA_EI_ITParameters] SET [dbo].[MA_EI_ITParameters].[TaxJournalCreditNote] = '' WHERE [dbo].[MA_EI_ITParameters].[TaxJournalCreditNote] IS NULL
GO
