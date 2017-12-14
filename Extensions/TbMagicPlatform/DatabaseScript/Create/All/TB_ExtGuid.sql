if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[TB_ExtGuid]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[TB_ExtGuid] (
    [DocGuid] [uniqueidentifier] NOT NULL,
   CONSTRAINT [PK_ExtGuid] PRIMARY KEY NONCLUSTERED 
    (
        [DocGuid]
    ) ON [PRIMARY]
) ON [PRIMARY]

END
GO