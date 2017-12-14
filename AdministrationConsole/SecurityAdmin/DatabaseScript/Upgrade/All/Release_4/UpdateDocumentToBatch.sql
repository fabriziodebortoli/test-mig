DECLARE @OLDTYPE int, @NEWTYPE int
 
set @OLDTYPE = (select TypeId from MSD_ObjectTypes where Type = 5)
set @NEWTYPE = (select TypeId from MSD_ObjectTypes where Type = 7)
 
UPDATE MSD_OBJECTS
set TypeId = @NEWTYPE
where TypeId = @OLDTYPE and
(
	NameSpace = 'MagoNet.MRP.MRPDocuments.MRP' OR
	NameSpace = 'MagoNet.MRP.MRPDocuments.ImpostazioniDefaultMRP' OR
	NameSpace = 'MagoNet.MRP.MRPDocuments.ConfermaOdPDaMRP' OR
	NameSpace = 'MagoNet.MRP.MRPDocuments.RdAGenerazioneOrdiniAFornitore' OR
	NameSpace = 'MagoNet.Cespiti.CespitiDocuments.Dismissione' OR
	NameSpace = 'MagoNet.Conai.ConaiDocuments.AssociazioneArticoliMateriali'
)
GO
