DECLARE @OLDTYPE int, @NEWTYPE int
 
set @OLDTYPE = (select TypeId from MSD_ObjectTypes where Type = 5)
set @NEWTYPE = (select TypeId from MSD_ObjectTypes where Type = 21)
 
UPDATE MSD_OBJECTS
set TypeId = @NEWTYPE
where TypeId = @OLDTYPE and
(
	NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CaricaOrdFor' OR
	NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CaricaBollaCarico' OR
	NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CaricaFatturaAcquisto' OR
	NameSpace = 'MagoNet.Configuratore.ConfiguratoreDocuments.CaricaDomanda' OR
	NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.CaricaDistintaBase' OR
	NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.CaricaOrdiniClienti' OR
	NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.CercaPianoProduzione' OR
	NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.CaricaOffCli' OR
	NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.CaricaOneriAccessori' OR
	NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.CaricaBollaCaricoOA' OR
	NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOffFor' OR
	NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOrdCli' OR
	NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOrdFor' OR
	NameSpace = 'MagoNet.Varianti.VariantiDocuments.CaricaDistinta' OR
	NameSpace = 'MagoNet.Varianti.VariantiDocuments.CaricaVariante' OR
	NameSpace = 'MagoNet.Vendite.VenditeDocuments.CaricaFattura' OR
	NameSpace = 'MagoNet.Vendite.VenditeDocuments.CaricaOrdCli'
)
GO
