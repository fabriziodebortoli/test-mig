/****** Object:  Table [RS_Configuration] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RS_Configuration]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[RS_Configuration](
	[OfficeID] [int] NOT NULL,
	[UsedEntities] [varchar](512) NOT NULL,
	[IsValid] [char](1) NOT NULL
 CONSTRAINT [PK_RS_Configuration] PRIMARY KEY CLUSTERED 
(
	[OfficeID] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [RS_Subjects] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RS_Subjects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[RS_Subjects](
	[SubjectID] [int] IDENTITY(1,1) NOT NULL,
	[IsWorker] [char](1) NOT NULL,
	[ResourceType] [varchar](8) NULL,
	[ResourceCode] [varchar](8) NULL,
	[WorkerID] [int] NULL,
	[Description] [varchar](128) NOT NULL,
 CONSTRAINT [PK_RS_Subjects] PRIMARY KEY CLUSTERED 
(
	[SubjectID] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [RS_SubjectsGrants] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RS_SubjectsGrants]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[RS_SubjectsGrants](
	[SubjectID] [int] NOT NULL,
	[EntityName] [varchar](50) NOT NULL,
	[RowSecurityID] [int] NOT NULL,
	[GrantType] [int] NOT NULL,
	[Inherited] [char](1) NOT NULL,
	[IsImplicit] [char](1) NOT NULL,
	[WorkerID] [int] NOT NULL	
 CONSTRAINT [PK_RS_SubjectsGrants] PRIMARY KEY CLUSTERED 
(
	[SubjectID] ASC,
	[EntityName] ASC,
	[RowSecurityID] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Index [IX_RS_SubjectsGrants] ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[RS_SubjectsGrants]') AND name = N'IX_RS_SubjectsGrants')
CREATE NONCLUSTERED INDEX [IX_RS_SubjectsGrants] ON [dbo].[RS_SubjectsGrants] 
(
	[WorkerID] ASC,
	[EntityName] ASC,
	[RowSecurityID] ASC
) ON [PRIMARY]
GO

/****** Object:  Table [RS_SubjectsHierarchy] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RS_SubjectsHierarchy]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[RS_SubjectsHierarchy](
	[MasterSubjectID] [int] NOT NULL,
	[SlaveSubjectID] [int] NOT NULL,
	[NrLevel] [smallint] NOT NULL,
 CONSTRAINT [PK_RS_SubjectsHierarchy] PRIMARY KEY CLUSTERED 
(
	[MasterSubjectID] ASC,
	[SlaveSubjectID] ASC,	
	[NrLevel] ASC
)) ON [PRIMARY]
END
GO

/****** Object:  Table [RS_TmpOldHierarchies] ******/
if not  exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[RS_TmpOldHierarchies]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[RS_TmpOldHierarchies](
	[MasterSubjectID] [int] NOT NULL,
	[SlaveSubjectID] [int] NOT NULL,
	[NrLevel] [smallint] NOT NULL,
 CONSTRAINT [PK_RS_TmpOldHierarchies] PRIMARY KEY CLUSTERED 
(
	[MasterSubjectID] ASC,
	[SlaveSubjectID] ASC,	
	[NrLevel] ASC
)) ON [PRIMARY]
END
GO
