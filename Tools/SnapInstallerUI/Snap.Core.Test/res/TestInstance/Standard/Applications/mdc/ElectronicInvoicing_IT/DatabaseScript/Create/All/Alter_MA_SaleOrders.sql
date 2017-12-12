if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_SaleOrdDetails' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ContractCode')
	ALTER TABLE [dbo].[MA_SaleOrdDetails] 
	ADD [ContractCode] [varchar] (15) NULL CONSTRAINT DF_SaleOrdDet_ContractCo_00 DEFAULT ('')
GO

UPDATE [dbo].[MA_SaleOrdDetails] SET [dbo].[MA_SaleOrdDetails].[ContractCode] = '' WHERE [dbo].[MA_SaleOrdDetails].[ContractCode] IS NULL
GO


UPDATE [dbo].[MA_SaleOrdDetails] SET [dbo].[MA_SaleOrdDetails].[ContractCode] = 
(SELECT [dbo].[MA_SaleOrd].[ContractCode] FROM [dbo].[MA_SaleOrd] WHERE ([dbo].[MA_SaleOrd].[SaleOrdId] = [dbo].[MA_SaleOrdDetails].[SaleOrdId]))
WHERE [dbo].[MA_SaleOrdDetails].[Invoiced] = '0' AND [dbo].[MA_SaleOrdDetails].[Cancelled] = '0'
GO



if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_SaleOrdDetails' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'ProjectCode')
	ALTER TABLE [dbo].[MA_SaleOrdDetails] 
	ADD [ProjectCode] [varchar] (16) NULL CONSTRAINT DF_SaleOrdDet_ProjectCod_00 DEFAULT ('')
GO

UPDATE [dbo].[MA_SaleOrdDetails] SET [dbo].[MA_SaleOrdDetails].[ProjectCode] = '' WHERE [dbo].[MA_SaleOrdDetails].[ProjectCode] IS NULL
GO

UPDATE [dbo].[MA_SaleOrdDetails] SET [dbo].[MA_SaleOrdDetails].[ProjectCode] = 
(SELECT [dbo].[MA_SaleOrd].[ProjectCode] FROM [dbo].[MA_SaleOrd] WHERE ([dbo].[MA_SaleOrd].[SaleOrdId] = [dbo].[MA_SaleOrdDetails].[SaleOrdId]))
WHERE [dbo].[MA_SaleOrdDetails].[Invoiced] = '0' AND [dbo].[MA_SaleOrdDetails].[Cancelled] = '0'
GO