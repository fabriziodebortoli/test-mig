IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[TB_AutoincrementEntities]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[TB_AutoincrementEntities] (
    [Entity]  [varchar] (128) NOT NULL,
	[LastNumber] [int] NULL CONSTRAINT DF_TB_AutoIncrEntities_LastNr_00 DEFAULT(0),
    CONSTRAINT [PK_TB_AutoincrementEntities] PRIMARY KEY NONCLUSTERED
    (
		[Entity]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[TB_AutonumberEntities]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[TB_AutonumberEntities] (
    [Entity]  [varchar] (128) NOT NULL,
	[FormattedMask] [varchar] (10) NULL CONSTRAINT DF_TB_AutonumberEntities_FormattedMasky_00 DEFAULT (''),
	[IsYearEntity] [char] (1) NULL CONSTRAINT DF_TB_AutonumberEntities_IsYearEntity_00 DEFAULT ('0'),
	[LastNumber] [int] NULL CONSTRAINT DF_TB_AutonumberEntities_LastNr_00 DEFAULT(0),
	[Disabled] [char] (1) NULL CONSTRAINT DF_TB_AutonumberEntities_Disabled_00 DEFAULT ('0'),
    CONSTRAINT [PK_TB_AutonumberEntities] PRIMARY KEY NONCLUSTERED
    (
		[Entity]
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO


IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[TB_AutonumberEntitiesYears]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[TB_AutonumberEntitiesYears] (
    [Entity]  [varchar] (128) NOT NULL,
	[Year] [smallint] NOT NULL,
	[LastNumber] [int] NULL CONSTRAINT DF_TB_NumbEntitiesYears_LastNr_00 DEFAULT(0)
    CONSTRAINT [PK_TB_AutonumberEntitiesYears] PRIMARY KEY NONCLUSTERED
    (
		[Entity],
		[Year]
    ) ON [PRIMARY],
	CONSTRAINT [FK_TB_AutonumberEntitiesYears_00] FOREIGN KEY
    (
        [Entity]
    ) REFERENCES [dbo].[TB_AutonumberEntities] 
	(
        [Entity]
	)
) ON [PRIMARY]
END
GO