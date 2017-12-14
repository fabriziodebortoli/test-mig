IF NOT EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
	dbo.sysobjects.name = 'MA_FEPAParameters' AND dbo.sysobjects.id = dbo.syscolumns.id
	AND dbo.syscolumns.name = 'AttachReport')
BEGIN
ALTER TABLE [dbo].[MA_FEPAParameters]
	ADD [AttachReport] [char] (1) NULL CONSTRAINT DF_FEPAParame_AttachRepo_00 DEFAULT ('0')
END
GO


UPDATE [dbo].[MA_FEPAParameters] SET [dbo].[MA_FEPAParameters].[AttachReport] = '0' WHERE [dbo].[MA_FEPAParameters].[AttachReport] IS NULL
GO


