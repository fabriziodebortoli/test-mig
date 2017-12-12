if not exists (	select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
				where dbo.sysobjects.name = 'MA_EI_ITFatelWebParameters' and dbo.sysobjects.id = dbo.syscolumns.id 
				and dbo.syscolumns.name = 'SOSManagement')
	ALTER TABLE [dbo].[MA_EI_ITFatelWebParameters] 
	ADD [SOSManagement] [char]      (1) NULL CONSTRAINT DF_EIITFatelw_SOSMan_00 DEFAULT('1')
GO

	UPDATE [dbo].[MA_EI_ITFatelWebParameters] SET [dbo].[MA_EI_ITFatelWebParameters].[SOSManagement] = '1' WHERE [dbo].[MA_EI_ITFatelWebParameters].[SOSManagement] IS NULL
GO
