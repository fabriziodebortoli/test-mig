DECLARE @OLDTYPE int, @NEWTYPE int
 
set @OLDTYPE = (select TypeId from MSD_ObjectTypes where Type = 5)
set @NEWTYPE = (select TypeId from MSD_ObjectTypes where Type = 3)
 
if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Acquisti.AcquistiServices.PostazioneAcquisti' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Acquisti.AcquistiServices.PostazioneAcquisti' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Articoli.ArticoliServices.PostazioneArticoli' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Articoli.ArticoliServices.PostazioneArticoli' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Conai.ConaiServices.PostazioneConai' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Conai.ConaiServices.PostazioneConai' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Contabilita.ContabilitaServices.PostazioneContabilita' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Contabilita.ContabilitaServices.PostazioneContabilita' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Core.CoreServices.AnagraficheRidotte' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Core.CoreServices.AnagraficheRidotte' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Magazzino.MagazzinoServices.PostazioneMagazzino' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Magazzino.MagazzinoServices.PostazioneMagazzino' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Partite.PartiteServices.PostazionePartite' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Partite.PartiteServices.PostazionePartite' 
	)
END

if NOT EXISTS (select * from MSD_OBJECTS where namespace ='MagoNet.Vendite.VenditeServices.PostazioneVendite' AND TypeId = @NEWTYPE)
BEGIN
	UPDATE MSD_OBJECTS
		set TypeId = @NEWTYPE
		where TypeId = @OLDTYPE and
	(
	NameSpace = 'MagoNet.Vendite.VenditeServices.PostazioneVendite' 
	)
END
GO
