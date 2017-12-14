if exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_EI_ITFatelWebParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'UserName')

	ALTER TABLE MA_EI_ITFatelWebParameters
	ALTER COLUMN UserName VARCHAR(128)
GO
