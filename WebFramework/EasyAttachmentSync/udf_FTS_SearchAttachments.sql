CREATE FUNCTION [dbo].[udf_FTS_SearchAttachments]
(
	@CollectorID int,
	@CollectionID int,
	@ErpDocumentID int,
	@ExtensionType varchar(10),
	@DataFrom datetime,
	@DataTo	 datetime,
	@keywords nvarchar(4000)
)
returns table
as
  return 
(SELECT DISTINCT A.AttachmentID 
  FROM  
		DMS_Attachment A, DMS_Collection O, DMS_ArchivedDocContent C, DMS_ArchivedDocument D
  WHERE
		--join
		(A.ArchivedDocID = C.ArchivedDocID AND A.ArchivedDocID = D.ArchivedDocID)
		--CurrentLevel
		AND (@ERPDocumentID = -1 OR A.ErpDocumentID = @ErpDocumentID) 
		--CollectionLevel
		AND (@CollectionID = -1 OR A.CollectionID = @CollectionID)
		--CollectorLevel
		AND (@CollectorID = -1 OR (A.CollectionID = O.CollectionID AND O.CollectorID = @CollectorID))		
		--Data
		AND (A.TBModified BETWEEN @DataFrom AND @DataTo OR A.TBCreated BETWEEN @DataFrom AND @DataTo)
		--Extension Type
		 AND (@ExtensionType = 'All' OR D.ExtensionType = @ExtensionType)
		--FullTextSearch
		AND (C.OCRProcess = 1 AND CONTAINS(C.BinaryContent, @keywords))
UNION
SELECT DISTINCT A.AttachmentID 
  FROM  
		DMS_Attachment A, DMS_Collection O, DMS_ArchivedDocContent C, DMS_ArchivedDocument D, DMS_ArchivedDocTextContent T
  WHERE
		--join
		(A.ArchivedDocID = C.ArchivedDocID AND C.ArchivedDocID = T.ArchivedDocID AND A.ArchivedDocID = D.ArchivedDocID)
		--CurrentLevel
		AND (@ERPDocumentID = -1 OR A.ErpDocumentID = @ErpDocumentID) 
		--CollectionLevel
		AND (@CollectionID = -1 OR A.CollectionID = @CollectionID)
		--CollectorLevel
		AND (@CollectorID = -1 OR (A.CollectionID = O.CollectionID AND O.CollectorID = @CollectorID))		
		--Data
		AND (A.TBModified BETWEEN @DataFrom AND @DataTo OR A.TBCreated BETWEEN @DataFrom AND @DataTo)
		--Extension Type
		AND (@ExtensionType = 'All' OR D.ExtensionType = @ExtensionType) 
		--FullTextSearch
		AND (C.OCRProcess = 1 AND CONTAINS(T.TextContent, @keywords))
);