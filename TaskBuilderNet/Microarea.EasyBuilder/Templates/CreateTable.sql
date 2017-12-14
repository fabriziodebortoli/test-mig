IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = object_id(N'[dbo].[$TablePhysicalName]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
CREATE TABLE [dbo].[$TablePhysicalName] (
    CONSTRAINT [$PrimaryKeyConstraint] PRIMARY KEY NONCLUSTERED
    (
    ) ON [PRIMARY]
) ON [PRIMARY]
END
GO
