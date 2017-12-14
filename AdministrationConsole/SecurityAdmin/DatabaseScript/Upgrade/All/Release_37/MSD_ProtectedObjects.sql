if not exists (select dbo.syscolumns.name from dbo.syscolumns, dbo.sysobjects 
where dbo.sysobjects.name = 'MSD_ProtectedObjects' and dbo.sysobjects.id = dbo.syscolumns.id and dbo.syscolumns.name = 'FromGrant')
ALTER TABLE [dbo].[MSD_ProtectedObjects] 
ADD [FromGrant] [bit] NOT NULL  DEFAULT (0)
GO

