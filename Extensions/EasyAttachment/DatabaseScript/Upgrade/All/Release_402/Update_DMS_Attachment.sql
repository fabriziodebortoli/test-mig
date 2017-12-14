UPDATE [dbo].[DMS_Attachment] 
SET [dbo].[DMS_Attachment].[IsMainDoc] = [dbo].[DMS_ArchivedDocument].[IsWoormReport] 
FROM [dbo].[DMS_ArchivedDocument] WHERE  [dbo].[DMS_Attachment].[ArchivedDocID] = [dbo].[DMS_ArchivedDocument].[ArchivedDocID]
GO