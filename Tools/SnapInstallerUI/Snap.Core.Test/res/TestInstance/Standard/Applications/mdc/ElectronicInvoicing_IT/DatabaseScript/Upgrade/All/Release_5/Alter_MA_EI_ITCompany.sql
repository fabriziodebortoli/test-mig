if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
    dbo.sysobjects.name = 'MA_EI_ITCompany' and dbo.sysobjects.id = dbo.syscolumns.id 
    and dbo.syscolumns.name = 'SenderTelephone')
ALTER TABLE [dbo].MA_EI_ITCompany
   ADD [SenderTelephone]	[varchar]	(20) NULL CONSTRAINT DF_EIITCompany_SenderTelephone_00 DEFAULT('')
GO

UPDATE [dbo].[MA_EI_ITCompany] SET [dbo].[MA_EI_ITCompany].[SenderTelephone] = '' WHERE [dbo].[MA_EI_ITCompany].[SenderTelephone] IS NULL
GO

if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects where 
    dbo.sysobjects.name = 'MA_EI_ITCompany' and dbo.sysobjects.id = dbo.syscolumns.id 
    and dbo.syscolumns.name = 'Email')
ALTER TABLE [dbo].MA_EI_ITCompany
   ADD [Email]	[varchar]	(64) NULL CONSTRAINT DF_EIITCompany_Email_00 DEFAULT('')
GO


UPDATE [dbo].[MA_EI_ITCompany] SET [dbo].[MA_EI_ITCompany].[Email] = '' WHERE [dbo].[MA_EI_ITCompany].[Email] IS NULL
GO
