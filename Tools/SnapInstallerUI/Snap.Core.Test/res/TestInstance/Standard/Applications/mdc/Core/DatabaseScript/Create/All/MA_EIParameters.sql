if not exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MA_EIParameters]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 BEGIN
CREATE TABLE [dbo].[MA_EIParameters] (
    [ParameterId]		[int]       NOT NULL,
    [AttachReport]		[char]		(1) NULL CONSTRAINT DF_EIParamete_AttachRepo_00 DEFAULT ('0'),
    [DocumentPath]          [varchar]	       (128) NULL CONSTRAINT DF_EIParamete_DocumentPa_00 DEFAULT(''),
   CONSTRAINT [PK_EIParameters] PRIMARY KEY NONCLUSTERED 
    (
        [ParameterId]
    ) ON [PRIMARY]
) ON [PRIMARY]

END
GO

