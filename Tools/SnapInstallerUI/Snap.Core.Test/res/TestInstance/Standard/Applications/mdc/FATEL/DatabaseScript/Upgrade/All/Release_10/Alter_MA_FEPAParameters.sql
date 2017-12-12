IF EXISTS (SELECT dbo.sysobjects.name FROM dbo.sysobjects WHERE
	dbo.sysobjects.name = 'DF_FEPAParame_ProcCodePA_00')
BEGIN
ALTER TABLE [dbo].[MA_FEPAParameters]
   DROP CONSTRAINT [DF_FEPAParame_ProcCodePA_00]
END
GO

IF EXISTS (SELECT dbo.syscolumns.name FROM dbo.syscolumns, dbo.sysobjects WHERE
    dbo.sysobjects.name = 'MA_FEPAParameters' AND dbo.sysobjects.id = dbo.syscolumns.id
    AND dbo.syscolumns.name = 'ProcessCodePA')
BEGIN
ALTER TABLE [dbo].[MA_FEPAParameters]
   DROP COLUMN [ProcessCodePA]
END
GO