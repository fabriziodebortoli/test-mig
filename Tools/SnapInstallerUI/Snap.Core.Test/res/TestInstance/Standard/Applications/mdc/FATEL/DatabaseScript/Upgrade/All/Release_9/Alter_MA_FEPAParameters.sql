IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_FEPAParameters' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'ProcessCodePA')
BEGIN
ALTER TABLE [dbo].[MA_FEPAParameters]
	ADD [ProcessCodePA] [varchar] (2) NULL CONSTRAINT DF_FEPAParame_ProcCodePA_00 DEFAULT('')
END
GO


UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[ProcessCodePA] = '' WHERE [dbo].[MA_FEPAParameters].[ProcessCodePA] IS NULL
GO

IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_FEPAParameters' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'ProcessCodeB2B')
BEGIN
ALTER TABLE [dbo].[MA_FEPAParameters]
	ADD [ProcessCodeB2B] [varchar] (2) NULL CONSTRAINT DF_FEPAParame_ProcCodeB2B_00	DEFAULT('')
END
GO


UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[ProcessCodeB2B] = '' WHERE [dbo].[MA_FEPAParameters].[ProcessCodeB2B] IS NULL
GO
