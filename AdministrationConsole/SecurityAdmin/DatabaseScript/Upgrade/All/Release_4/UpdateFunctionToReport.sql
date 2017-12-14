DECLARE @OLDTYPE int, @NEWTYPE int
 
set @OLDTYPE = (select TypeId from MSD_ObjectTypes where Type = 4)
set @NEWTYPE = (select TypeId from MSD_ObjectTypes where Type = 3)
 
if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Partite.PartiteDocuments.DistintaRIBA' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaRIBA' 
	)
END
 
if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Partite.PartiteDocuments.DistintaMAV' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaMAV' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Partite.PartiteDocuments.DistintaRID' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaRID' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Partite.PartiteDocuments.DistintaCambiali' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaCambiali' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Partite.PartiteDocuments.DistintaBonifici' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaBonifici' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Produzione.ProduzioneDocuments.AnalisiStatoProduzione' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AnalisiStatoProduzione' 
	)
END

GO
