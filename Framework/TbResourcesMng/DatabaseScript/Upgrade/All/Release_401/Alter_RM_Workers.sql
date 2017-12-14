-- Campo IsRSEnabled: char(1)
if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
	dbo.sysobjects.name = 'RM_Workers' and dbo.sysobjects.id = dbo.syscolumns.id
	and dbo.syscolumns.name = 'IsRSEnabled')
BEGIN
	ALTER TABLE [RM_Workers]
	ADD [IsRSEnabled] [char] (1) NOT NULL CONSTRAINT [DF_RM_Workers_IsRSEnabled] DEFAULT('0')
END
GO