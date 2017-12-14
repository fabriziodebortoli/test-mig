CREATE FUNCTION [dbo].[udf_GetUnusedFieldIndexes]()
returns table
as return (
SELECT SearchIndexID FROM dbo.DMS_SearchFieldIndexes AS S 
WHERE NOT EXISTS (SELECT SearchIndexID FROM DMS_AttachmentSearchIndexes AS A 
WHERE A.SearchIndexID = S.SearchIndexID) AND 
NOT EXISTS (SELECT SearchIndexID FROM DMS_ArchivedDocSearchIndexes AS A 
WHERE A.SearchIndexID = S.SearchIndexID)
);