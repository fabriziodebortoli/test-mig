if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MA_EI_ITParameters]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MA_EI_ITParameters] (
    [ParameterId]				[int]       NOT NULL,
	[TaxJournal]				[varchar]   (8) NULL CONSTRAINT DF_EIITParame_TaxJournal_00 DEFAULT(''),
	[TaxJournalCreditNote]		[varchar]   (8) NULL CONSTRAINT DF_EIITParame_TaxJournCN_00 DEFAULT(''),
    [ContractProjectFromOrders]  [char]	(1) NULL CONSTRAINT DF_EIITParame_ContractPr_00 DEFAULT('0'),
    [ContractProjectFromJobs]	[char]	(1) NULL CONSTRAINT DF_EIITParame_ContractPr_01 DEFAULT('0'),
    [SetItemCode] 				[char]	(1) NULL CONSTRAINT DF_EIITParame_SetItemCo_00 DEFAULT('0'),
    [ItemCodeType] 				[varchar]	(35) NULL CONSTRAINT DF_EIITParame_ItemCodeTy_00 DEFAULT(''),
    [Link] 					[varchar]	(256)	 NULL CONSTRAINT DF_EIITParame_Link_00 DEFAULT(''),
	[ManualRefManagement]	[char]	(1) NULL CONSTRAINT DF_EIITParame_ManRef_00 DEFAULT('0'),
	[OrderLineNumberReference]	[char]	(1) NULL CONSTRAINT DF_EIITParame_OrLiRef_01 DEFAULT('0'),
	[AlwaysReportSalesOrderData]	[char]	(1) NULL CONSTRAINT DF_EIITParame_AlwSOD_01 DEFAULT('0'),
	[StampChargesInSummary]	[char]	(1) NULL CONSTRAINT DF_EIITParame_StCha_01 DEFAULT('0')
   CONSTRAINT [PK_EIITParamers_IT] PRIMARY KEY NONCLUSTERED 
    (
        [ParameterId]
    ) ON [PRIMARY]
) ON [PRIMARY]

END
GO

