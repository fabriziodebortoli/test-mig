CREATE FUNCTION [dbo].[udf_FTS_SearchArchivedDoc]
(
	@ExtensionType varchar(10),
	@DataFrom datetime,
	@DataTo	 datetime,
	@OnlyWoormReport bit,
	@OnlyNotAttached bit,	
	@OnlyAttached bit,
	@OnlyPending bit,
	@keywords nvarchar(4000)
)
returns table
as
  return  
  (SELECT DISTINCT D.ArchivedDocID
  FROM  
		DMS_ArchivedDocContent C, DMS_ArchivedDocument D, DMS_ArchivedDocTextContent T
  WHERE
		--Join
		(D.ArchivedDocID = C.ArchivedDocID)
		--Data
		AND (D.TBModified BETWEEN @DataFrom AND @DataTo OR D.TBCreated BETWEEN @DataFrom AND @DataTo)
		--Extension Type
		AND (@ExtensionType = 'All' OR D.ExtensionType = @ExtensionType) 
		--Extract Only WoormReport
		AND (@OnlyWoormReport = 0 OR D.IsWoormReport = 1) 
		--Extract Only Pending
		AND (@OnlyPending = 0 OR D.Barcode IN (SELECT DISTINCT Barcode FROM DMS_ErpDocBarcodes))
		--Extract All: Attached and Not Attached
		AND ((@OnlyNotAttached = 1 AND @OnlyAttached = 1)
			 --Extract Only Not Attached		
			 OR (@OnlyNotAttached = 1 AND NOT EXISTS(SELECT DISTINCT ArchivedDocID FROM DMS_Attachment where DMS_Attachment.ArchivedDocID = D.ArchivedDocID))
			  --Extract Only Attached		
			 OR (@OnlyAttached = 1 AND EXISTS(SELECT DISTINCT ArchivedDocID FROM DMS_Attachment where DMS_Attachment.ArchivedDocID = D.ArchivedDocID)))
		--FullTextSearch
		AND (C.OCRProcess = 1 AND CONTAINS(C.BinaryContent, @keywords))
	)
UNION
	(SELECT DISTINCT D.ArchivedDocID  
  FROM  
		DMS_ArchivedDocContent C, DMS_ArchivedDocument D, DMS_ArchivedDocTextContent T
  WHERE
		--Join
		(D.ArchivedDocID = C.ArchivedDocID AND D.ArchivedDocID = T.ArchivedDocID)
		--Data
		AND (D.TBModified BETWEEN @DataFrom AND @DataTo OR D.TBCreated BETWEEN @DataFrom AND @DataTo)
		--Extension Type
		AND (@ExtensionType = 'All' OR D.ExtensionType = @ExtensionType) 
		--Extract Only WoormReport
		AND (@OnlyWoormReport = 0 OR D.IsWoormReport = 1) 
		--Extract Only Pending
		AND (@OnlyPending = 0 OR D.Barcode IN (SELECT DISTINCT Barcode FROM DMS_ErpDocBarcodes))
		--Extract All: Attached and Not Attached
		AND ((@OnlyNotAttached = 1 AND @OnlyAttached = 1)
			 --Extract Only Not Attached		
			 OR (@OnlyNotAttached = 1 AND NOT EXISTS(SELECT DISTINCT ArchivedDocID FROM DMS_Attachment where DMS_Attachment.ArchivedDocID = D.ArchivedDocID))
			  --Extract Only Attached		
			 OR (@OnlyAttached = 1 AND EXISTS(SELECT DISTINCT ArchivedDocID FROM DMS_Attachment where DMS_Attachment.ArchivedDocID = D.ArchivedDocID)))
		--FullTextSearch
		AND (C.OCRProcess = 1 AND CONTAINS(T.TextContent, @keywords))	
	);
