
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
	DECLARE @documentTypeId as integer 
	SET @documentTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 5)
	DECLARE @batchTypeId as integer 
	SET @batchTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 7)
	DECLARE  @finderTypeId as integer 
	SET @finderTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 21)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.CashOrderSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaRIBA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.Closing'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.ChiusuraMagazzino' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.Documents.SubcntOrdOutsourcedMOSteps'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroDocuments.OrdForElencoFasiEsterneOdP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMExplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.EsplosioneDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.CreditNote'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.NotaCredito' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Services.TaxJournalNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngServices.NumeratoriRegistriIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.CommissionsGeneration'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.GenerazioneProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Documents.Tax'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDocuments.IVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Services.EntriesDeleting'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaServices.EliminaMovimenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Services.StorageDeleting'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoServices.EliminaDepositi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Services.CustomerQuotationsDeleting'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiServices.EliminaOffCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Documents.SerialNumbers'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDocuments.NumeratoriMatricole' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.CommissionsSettlement'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.LiquidazioneProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Documents.JobsAddOnFly'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDocuments.CommesseSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Documents.RtgStepsGantt'
WHERE( NameSpace = 'MagoNet.CRP.CRPDocuments.GanttFasi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.BatchDocuments.TaxCodeAssignment'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliBatchDocuments.AssegnazioneCodIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Documents.ProRataPrint'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.RegimiIVASpecialiDocuments.StampaProRata' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.BatchDocuments.PriceListsWizard'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziBatchDocuments.WizardListini' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Documents.IntraReportGeneration'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDocuments.StampaReportIntra' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Documents.BalanceReclassifications'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioDocuments.RiclassificazioniPianoConti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Documents.ReceiptsAndRequirements'
WHERE( NameSpace = 'MagoNet.MRP.MRPDocuments.AnalisiFabbisogniVersamenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.BalanceRebuilding'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.RicostruzioneSaldiAgente' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MORollback'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.CorrezioneLavorazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.Documents.KEPYO'
WHERE( NameSpace = 'MagoNet.Contabilita_gr.Contabilita_GRDocuments.Kepyo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.SalesPeopleParameters'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.ParametriAgenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Documents.Segments'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.Segmenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.BillOfLadingMaintenance'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.ManutenzioneBollaCarico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.CorrectionAccInvoice'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.FatturaAccompagnatoriaACorrezione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Documents.MRP'
WHERE( NameSpace = 'MagoNet.MRP.MRPDocuments.MRP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.Invoice'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.FatturaImmediata' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ReorderMaterialsToSupplier'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.RiordinoMaterialiMancanti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.BalanceGeneration'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.GeneraBilancio' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Documents.ItemsMaterials'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDocuments.ArticoliMateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.VouchersPrint'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.StampaReversali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrders'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PagamentoMandati' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SuppQuota'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.CreditNoteMaintenance'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.ManutenzioneNotaCreditoRicevuta' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.Documents.PaymentTerms'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDocuments.CondizioniPagamento' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.IntangibleFixedAssetAddOnFly'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.CespitiImmaterialiSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Documents.FeesLinkedToJE'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDocuments.ParcellePrimaNota' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Services.TaxPlafond'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.RegimiIVASpecialiServices.PlafondIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Services.PyblsRcvblsParameters'
WHERE( NameSpace = 'MagoNet.Partite.PartiteServices.ParametriPartite' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Documents.QuestionLoad'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDocuments.CaricaDomanda' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.Clearing'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.SaldaConto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.IntangibleFixedAssets'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.CespitiImmateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Documents.AutomaticAssociation'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.AssociazioneAutomatica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.TangibleFixedAssetsAddOnFly'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.CespitiMaterialiSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Documents.Lots'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDocuments.Lotti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.BatchDocuments.CustomerActualRebuilding'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeBatchDocuments.RicostruzioneConsuntivoClienti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Documents.CustMaterialsExemption'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDocuments.ClientiMateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.BillsOfLadingDeleting'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.EliminaBollaCarico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.OwnedCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.Controllate' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMComponentsReplacement'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.SostituzioneComponenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.Areas'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.AreeVendita' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccountingTransaction.Documents.GenerateExtAccountingTpl'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.GenerateExtAccountingTemplate' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillsSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaEffetti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.DDSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaRID' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionDevelopment'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AvanzamentoProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseOrderLoading'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CaricaOrdFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Documents.StubBooks'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDocuments.Bollettari' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ShopPapersPrint'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.StampaDocumentiProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.BatchDocuments.ItemsCopy'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliBatchDocuments.CopiaArticolo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.AccountingParameters'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.ParametriContabilita' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.TaxSummaryJournalsPrint'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRiepiloghiIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Documents.MOGantt'
WHERE( NameSpace = 'MagoNet.CRP.CRPDocuments.GanttOdP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.NCReceiptsDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.EliminaRicevutaFiscaleNI' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Services.SmartCodesParameters'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteServices.ParametriCodiceParlante' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProducibilityAnalysis'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.AnalisiProducibilita' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.CostAccountingEntries'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDocuments.MovimentiAnalitici' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMConfirmation'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.RitornoDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.XGate.Services.XGateParameters'
WHERE( NameSpace = 'MagoNet.XGate.XGateServices.ParametriXGate' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Documents.Company'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDocuments.Azienda' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Documents.DistributionTemplates'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.ModelliRipartizione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Services.SaleOrdDefaults'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiServices.DefaultCodiciOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Documents.Ports'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDocuments.Porti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.ClosedPayables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PartiteFornitoreChiuse' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.CommodityCtgBySuppliers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.CatMerceologicaFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.PostableAssignment'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDocuments.AssegnaMovimentabile' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Documents.ItemsPriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDocuments.ArticoliListini' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrdersPrint'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.StampaMandati' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrdersSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaMandati' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Services.LotsDeleting'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleServices.EliminaLotti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMNavigation'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.DistintaBaseGrafica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.CommissionRationalization'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.RazionalizzazionePoliticheProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Documents.LIFOFIFOItems'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDocuments.ArticoliLifoFifo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.SuppliersAddOnFly'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.FornitoriSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.CommissionsInvoicing'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.FatturazioneProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Services.IDNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngServices.Identificatori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillUpdate'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.ModificaEffetto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Services.ReservedRebuilding'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiServices.RicostruzioneImpegnato' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.SalesOrdLoading'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.CaricaOrdiniClienti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Services.LocationsDefaults'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniServices.DefaultCodiciUbicazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Services.DepreciationDeleting'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiServices.EliminaAmmortamento' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.BalanceRebuilding'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.RicostruzioneSaldi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.TangibleFixedAssetsCtg'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.CategorieMateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Services.SalesPricePolicies'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziServices.PolitichePrezziCicloAttivo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Services.ChartOfAccountsParameters'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiServices.ParametriPianoConti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.Receipt'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.RicevutaFiscale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.BatchDocuments.LIFOFIFOProgress'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoBatchDocuments.FormazioneLifoFifo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.WithholdingTaxParameters'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ParametriRitenutaAcconto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.PickingList'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.BdP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.AddOnsPurchaseOrders.SubcontractorOrd'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroAddOnsOrdiniFornitori.OrdForLavorazioneEsterna' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Documents.DutyCodes'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDocuments.CodiciTributo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Documents.OneriAccessoriBatch'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.OneriAccessoriBatch' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.SuppliersCategories'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.CategorieFornitori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.Documents.SubcontractorOrdGeneration'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroDocuments.GenerazioneOrdForLavorazioneEsterna' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Components.PurchaseReqRequirementOrigin'
WHERE( NameSpace = 'MagoNet.MRP.MRPComponents.RDAProvenienzaFabbisogni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrderLoading'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOrdFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Documents.IntraDispatches'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDocuments.IntraCessioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Services.PurchasesPricePolicies'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziServices.PolitichePrezziCicloPassivo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.TemplateExport'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.EsportaModello' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Documents.MRPMOConfirmation'
WHERE( NameSpace = 'MagoNet.MRP.MRPDocuments.ConfermaOdPDaMRP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.PurchasesParameters'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.ParametriAcquisti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionLotEdit'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.ModificaLotti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.InvoicesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.EliminaFatturaImmediata' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemData'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.DatiArticolo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.BatchDocuments.DocumentPosting'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeBatchDocuments.StampaRegistraDocVendita' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.AllowanceCalculation'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.CalcoloIndennita' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseCorrectionInvoice'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.FatturaAcquistoACorrezione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Documents.TaxJournals'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDocuments.RegistriIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Documents.Contacts'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDocuments.Contatti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.BillsOfLadingPosting'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.RegistraBollaCarico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.SaleDocForecastJE'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPrevisionaleIVAEmessi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.SalesPeople'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.Agenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.AddOnsMaster.SaleOrdersPrint'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiAddOnsAnagrafiche.StampaOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Documents.ReasonCopy'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDocuments.CopiaCausale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Services.PriceListsDeleting'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziServices.EliminaListini' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.FixedAssetsReasons'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.CausaliCespiti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.BatchDocuments.SaleOrdFulfilmentEditing'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeBatchDocuments.EvasioneOrdCliModifica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.ProfitAndLossMonthlyClosing'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.ChiusureEconomiciMensili' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Services.MultiStorageParameters'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoServices.ParametriMultiDeposito' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.BillOfLading'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.BollaCarico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.ItemsDeleting'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.EliminaArticoli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BankTransferSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaBonifici' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Documents.eBanksAddOnFly'
WHERE( NameSpace = 'MagoNet.Banche.BancheDocuments.BancheRidotte' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Documents.PurchaseRequest'
WHERE( NameSpace = 'MagoNet.MRP.MRPDocuments.RDA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.LeadTimeCalculation'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CalcoloLeadTime' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Services.StubBookNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngServices.NumeratoriBolle' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.ReturnsFromCustomer'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.ResoCliente' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.AccInvoiceMaintenance'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaFatturaAccompagnatoria' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Documents.VariantCheck'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDocuments.ControlloVarianti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemsNavigation'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.GraficaArticoli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Documents.Transport'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDocuments.ModiTrasporto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Services.PurchaseOrdDefaults'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriServices.DefaultCodiciOrdFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.AddOnsSales.ConaiCalculation'
WHERE( NameSpace = 'MagoNet.Conai.ConaiAddOnsVendite.CalcoloConai' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Documents.CustSuppBanks'
WHERE( NameSpace = 'MagoNet.Banche.BancheDocuments.BancheCliFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.Items'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.Articoli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.ReturnToSupplierFromBoL'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.ResiFornitoreDaBdC' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.BalanceRebuilding'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.RicostruzioniContabili' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Documents.ReceiptsRebuilding'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.RicostruzioneCarichi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.CodeConfirm'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.ConfermaCodice' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Documents.Carriers'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDocuments.Vettori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMLoading'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.CaricaDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.CorrectionInvoice'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.FatturaImmediataACorrezione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Documents.StorageGroups'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDocuments.RaggruppamentiDepositi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ProductCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.CatProdotto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Services.ActualCostsCheck'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneServices.ControlloCostiConsuntivi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrdersPrint'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.StampaOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Services.ConsBalSheetParameters'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiServices.ParametriBilanciConsolidati' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Documents.WCChart'
WHERE( NameSpace = 'MagoNet.CRP.CRPDocuments.ChartCdL' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.Documents.CreditCard'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDocuments.CartaCredito' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Documents.Configuration'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDocuments.Configurazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.DeliveryNotes'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDT' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.BatchDocuments.DeferredInvoicing'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeBatchDocuments.FatturazioneDifferita' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InvoiceMng.Components.CorrectionDocument'
WHERE( NameSpace = 'MagoNet.InvoiceMng.InvoiceMngComponents.CorrezioneDocumento' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.CodeConfirm'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.ConfermaCodice' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Documents.LIFOReceipts'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.Carichi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.CommissionPoliciesCopy'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.CopiaPoliticheProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Documents.AdditionalCharges'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.OneriAccessori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.SalesPeopleAddOnFly'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.AgentiRidotta' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Services.FixedAssetsRebuilding'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiServices.RicostruzioniCespiti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Services.CollectionParameters'
WHERE( NameSpace = 'MagoNet.Partite.PartiteServices.ParametriIncassi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Services.CurrenciesParameters'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaServices.ParametriDivise' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Documents.ValuationUpToDate'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.ValorizzazioneAllaData' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.SuppQuotaLoading'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOffFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMImplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.ImplosioneDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.BatchDocuments.StandardCostRebuilding'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziBatchDocuments.RicalcoloCostoStd' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Documents.IntraDispatchesLinkedToJE'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDocuments.IntraCessioniPrimaNota' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.AddOnsMaster.CustomerQuotationsPrint'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiAddOnsAnagrafiche.StampaOffCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Documents.CompanyBanks'
WHERE( NameSpace = 'MagoNet.Banche.BancheDocuments.BancheAzienda' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Documents.ProspectiveSuppliers'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDocuments.FornitoriPotenziali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.CustomersCategories'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.CategorieClienti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.AddOnsSales.SubcontractorDN'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroAddOnsVendite.DDTForLavorazioneEsterna' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemsAddOnFly'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.ArticoliSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.ItemProductionData'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.DatiArticoloProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.SummaryByTaxJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.RiepiloghiPerRegistroIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.TangibleFixedAssets'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.CespitiMateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.BillOfLadingToInvoiceLoading'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CaricaMultiBdC' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Documents.Root'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.Radice' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.Documents.SubcontractorDNGeneration'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroDocuments.GenerazioneDDTForLavorazioneEsterna' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.AddOnsMaster.SupplierQuotationsPrint'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriAddOnsAnagrafiche.StampaOffFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.AddOnsMaster.PurchaseOrdersPrint'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriAddOnsAnagrafiche.StampaOrdFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrdersIssue'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.EmissioneMandati' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Services.SalesOrdParameters'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiServices.ParametriOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.Locations'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.Ubicazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMProduction'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.ProduzioneDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.SalesParameters'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ParametriVendite' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Documents.CRP'
WHERE( NameSpace = 'MagoNet.CRP.CRPDocuments.Crp' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.CreditNoteMaintenance'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaNotaCredito' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.OpenReceivables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PartiteClienteAperte' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillsCollection'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.IncassoEffetti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.DepreciationForecast'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.AmmortamentoPrevisionale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Services.LotsManagementSetting'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleServices.ImpostaGestioneLotto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Documents.ContactsDeleting'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDocuments.EliminaContatti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Documents.PurchaseOrdFromPurchaseReq'
WHERE( NameSpace = 'MagoNet.MRP.MRPDocuments.RdAGenerazioneOrdiniAFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Services.OrderedRebuilding'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriServices.RicostruzioneOrdinato' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.NCReceiptMaintenance'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaRicevutaFiscaleNI' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.IntangibleFixedAssetsCtg'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.CategorieImmateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Documents.BOMLoading'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDocuments.CaricaDistinta' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.InvoicesPosting'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.RegistraFatturaAcquisto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.CommissionsRebuilding'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.RicalcoloProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMRun'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.LancioDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MOConfirmationBoLList'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.ConsuntivazioneElencoBdC' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Services.PayeesDefaults'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiServices.ParametriPercipienti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.CreditNotesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.EliminaNotaCredito' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.JEDeleting'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.EliminaPrimaNotaDefinitiva' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.TaxDeclaration'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.DichiarazioneIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Documents.LotsAddOnFly'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDocuments.LottiSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Services.MailParameters'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaServices.ParametriMail' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.BatchDocuments.InventoryAdjustment'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoBatchDocuments.RettificaInventariale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.BillOfLadingLoading'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CaricaBollaCarico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.BOMPosting'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.MovimentazioneDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.Customers'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.Clienti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Services.NumberParameters'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngServices.ParametriNumeratori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Documents.MRPDefaultAssignation'
WHERE( NameSpace = 'MagoNet.MRP.MRPDocuments.ImpostazioniDefaultMRP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.BillOfLadingPosting'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdForRegistraMovMag' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Documents.DefinitiveValuation'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.ValorizzazioneDefinitiva' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.CommissionPolicies'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.PoliticheProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.DefinitiveClosing'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.ChiusurePatrimoniali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.Documents.TrialBalance'
WHERE( NameSpace = 'MagoNet.Contabilita_gr.Contabilita_GRDocuments.TrialBalance' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.Documents.SubcntBoLShopPapersList'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroDocuments.BdCForElencoDocumentiAlFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.Heading'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.Vidima' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.SalesDefaults'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.DefaultCodiciVendite' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Components.SmartCodeHelp'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteComponents.CodiceParlanteHelp' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemsLanguageDescri'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.ArticoliDescriLingua' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.FixedAssetsEntries'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.MovimentiCespiti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.AccInvoice'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.FatturaAccompagnatoria' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.CostCenters'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDocuments.CentriCosto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Services.ActualCosts'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneServices.RicalcoloCostiConsuntivi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BOMCosting'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.CostificazioneDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.InvoiceLoading'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.CaricaFatturaAcquisto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.TaxJournalsPrint'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRegistriIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PANSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaMAV' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Documents.VariantLoading'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDocuments.CaricaVariante' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.BatchDocuments.ReceiptsDeferredInvoicing'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeBatchDocuments.FatturazioneDifferitaRicevutaFiscale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillsApproval'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.AccettazioneEffetti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierPricesAdj.Documents.SupplierPricesAdj'
WHERE( NameSpace = 'MagoNet.AllineamentoPrezziFornitore.AllineamentoPrezziFornitoreDocuments.AllineamentoPrezziFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.GLJournalTotals'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.TotaliLibroGiornale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.CommissionsEntriesDeleting'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.EliminaMovimentiAgenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Documents.PriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDocuments.Listini' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.BalanceSheet-PL-Grouped'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioContoEconomicoSintetico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Services.MagoXpToProdBaseConversion'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneServices.ConversioneMagoNet11' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Documents.Fees'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDocuments.Parcelle' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.InvoicesDeleting'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.EliminaFatturaAcquisto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemsPurchaseBarCodes'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.ArticoliBarCodeAcquisto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.AccountingTemplates'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.ModelliContabili' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.PureForecastJE'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPrevisionalePura' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrderUpdate'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.ModificaMandato' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Documents.ConaiEntries'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDocuments.MovimentiConai' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.PickingRequests'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.RichiesteSpedizione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.AddOnsMaster.ContactsCopy'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiAddOnsAnagrafiche.CopiaContatto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Documents.ReceiptsRestore'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.RipristinoCarichi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.ReturnToSupplier'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.ResoFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Documents.Drawings'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDocuments.Disegni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.ProfitAndLossClosing'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.ChiusureEconomici' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.DNLoading'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.CaricaDDT' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Services.BalanceRebuilding'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaServices.RicostruzioniAnalitica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.Suppliers'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.Fornitori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BankTransferPrint'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.StampaBonifici' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierPricesAdj.Documents.SupplierPricesAdjConfig'
WHERE( NameSpace = 'MagoNet.AllineamentoPrezziFornitore.AllineamentoPrezziFornitoreDocuments.ConfigurazioneAPF' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccountingTransaction.Documents.ExtAccountingTemplateCopy'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.CopyExtAccountingTemplate' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemsCustomers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.ArticoliCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.PureJE'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPura' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Documents.IntraArrivalsLinkedToJE'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDocuments.IntraAcquistiPrimaNota' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.CommodityCtgByCustomers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.CatMerceologicaCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemsSuppliers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.ArticoliFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrdersPrint'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.StampaOrdFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.ForecastJEDeleting'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.EliminaPrimaNotaPrevisionale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Documents.FeeTemplates'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDocuments.ModelliParcelle' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Services.MOComponentsEntriesCheck'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneServices.ControlloMovimentazioneMateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Services.MOItemsInventoryEntriesCheck'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneServices.ControlloMovimentazioneProdotti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MOModification'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.ModificaLavorazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Documents.WC'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDocuments.CdL' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Services.SupplierQuotationsDeleting'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriServices.EliminaOffFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Services.BOMParameters'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseServices.ParametriDistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.CustSuppCopy'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.CopiaCliFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Documents.Languages'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDocuments.Lingue' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Documents.Combination'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.Possibilita' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Services.RecalculateReservedOrdered'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneServices.RicalcoloImpegnatoOrdinato' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Documents.Teams'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDocuments.Squadre' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.BalanceValuesResume'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.RipresaBilancio' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.TotalsByTaxJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.TotaliPerRegistroIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Documents.SmartCode'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.CodiceParlante' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.Invoice'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.FatturaAcquisto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Services.ItemsParameters'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliServices.ParametriArticoli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Services.NonFiscalNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngServices.NumeratoriNonFiscali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.DeliveryNoteMaintenance'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaDDT' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.PrivacyStatementPrint'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.StampaLettereConsenso' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotationsPrint'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.StampaOffCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.OpenPayables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PartiteFornitoreAperte' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.CompanyGroups'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.GruppoAziende' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustQuota'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillsPresentation'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PresentazioneEffetti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.Disposal'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.Dismissione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ItemsAdditionalCode.Documents.ItemsCodes'
WHERE( NameSpace = 'MagoNet.VTSZSZJ.VTSZSZJDocuments.VTSZSZJCodes' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChargePolicies.Documents.ChargePolicies'
WHERE( NameSpace = 'MagoNet.PoliticheSpese.PoliticheSpeseDocuments.PoliticheSpese' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MaterialRequirementsPicking'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.PrelievoMaterialiMancanti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.CorrectionReceipt'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.RicevutaFiscaleACorrezione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.TrialBalance-Grouped'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioVerificaSintetico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Services.LotsSerialsParameters'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleServices.ParametriLottiMatricole' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MagicDocuments.Documents.ExpenseNote'
WHERE( NameSpace = 'MagoNet.MagicDocuments.MagicDocumentsDocuments.ExpenseNote' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.Depreciation'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.AmmortamentoCespiti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Documents.PackageTypes'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDocuments.MaterialiTipologie' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.CreditNotesPosting'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.RegistraNotaCreditoRicevuta' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Services.SaleOrdersDeleting'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiServices.EliminaOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.Classes'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.Classi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.VouchersPresentation'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PresentazioneReversali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Documents.Packages'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDocuments.Imballi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.PurchaseDocForecastJE'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPrevisionaleIVARicevuti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Documents.InventoryReasons'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDocuments.CausaliMagazzino' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProductionPlanGeneration'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.GenerazionePianoProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Services.FixedAssetsParameters'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiServices.ParametriCespiti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.InventoryDefaults'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.DefaultCodiciMagazzino' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Services.SingleStepLifoFifoParameters'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiServices.ParametriLifoFifoScattiContinui' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.ReceiptsDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.EliminaRicevutaFiscale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.UnitsOfMeasure'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.UnitaMisura' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.Receivables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PartiteCliente' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Documents.Titles'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDocuments.Titoli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemType'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.TipoArticolo' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Services.CustSuppParameters'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriServices.ParametriClientiFornitori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.ActualAccrualDate'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.DataEffettivaMaturazione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.BalanceImport'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.ImportaBilancio' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.GroupingCodes'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.CodiciRaggruppamento' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.MultiCompanyBalances'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.BilanciConsolidati' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.CostAccEntriesGeneration'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDocuments.GeneraMovimenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.DocumentDetailImportExport'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ImportExportRigheDocumento' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Documents.ProspectiveSuppliersDeleting'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDocuments.EliminaFornitoriPotenziali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.ClosedReceivables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PartiteClienteChiuse' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.RetailSalesDistribution'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.VentilazioneCorrispettivi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Services.Form770Rebuilding'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiServices.Ricostruzioni770' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.AddOnsPurchases.SubcontractorBoL'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroAddOnsAcquisti.BdCForLavorazioneEsterna' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MRPMO'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.OdPDaMRP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.InventoryEntriesDeleting'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.EliminaMovMag' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.AccInvoicesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.EliminaFatturaAccompagnatoria' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustQuotaLoading'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.CaricaOffCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProductionPlan'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.PianoProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.BatchDocuments.CustomerPriceListUpdate'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziBatchDocuments.UpdateListinoCliente' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.InvoiceMaintenance'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.ManutenzioneFatturaAcquisto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.TemplateSave'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDocuments.SalvaModello' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Documents.Calendars'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDocuments.Calendari' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.GLJournalPrint'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaLibro' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Documents.IntraFileGeneration'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDocuments.StampaFileIntra' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MO'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.OdP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.BillOfMaterials'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.DistintaBase' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Documents.InitialValuesPosting'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDocuments.RiportoIniziali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Documents.ISOCountryCodes'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDocuments.ISOStati' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.Documents.AnnualTaxReporting'
WHERE( NameSpace = 'MagoNet.Contabilita_it.Contabilita_ITDocuments.StampaIVAAnnuale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Documents.BreakdownReasons'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDocuments.CausaliNonDisp' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.SaleDocJE'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaIVAEmessi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Services.ManufacturingParameters'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneServices.ParametriProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Documents.ChartOfAccountsAddOnFly'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDocuments.PianoContiRidotto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.OwnerCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.Controllanti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.Departments'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.Reparti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Documents.Jobs'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDocuments.Commesse' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.TemplateImport'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.ImportaModello' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Documents.Variants'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDocuments.Varianti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotationsPrint'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.StampaOffFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Documents.ItemToCtgAssociations'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.AssociazioniArticoliCategorie' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Documents.BillOfLadingLoading'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.CaricaBollaCaricoOA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.CreditNoteNegSignForm'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.FincatoNotaCreditoSegniNegativi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.BatchDocuments.StoragesEntries'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoBatchDocuments.MovimentiTraDepositi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.InventoryParameters'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.ParametriMagazzino' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Documents.Currencies'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDocuments.Divise' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.CreditNotes'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.NotaCreditoRicevuta' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.CommisionStateSetting'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.ImpostaStatoPoliticheProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Services.SaleOrdActualRebuilding'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiServices.RicostruzioneConsuntivoOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.SupplierReorder'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.RiordinoFornitori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.Payables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.PartiteFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Services.LotsRebuilding'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleServices.RicostruzioneLotti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.TemplateGeneration'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.GeneraModello' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ReorderMaterialsToProduction'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.RiordinoProduzioneMancanti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Documents.InventoryEntries'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDocuments.MovimentiMagazzino' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.InvoiceLoading'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.CaricaFattura' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Documents.ChartOfAccountsNavigation'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDocuments.GraficoPianoConti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.BalanceSheet'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioContrapposto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.ConsolidationTemplates'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.ModelliConsolidamento' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Documents.ItemsMaterialsAssociation'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDocuments.AssociazioneArticoliMateriali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.ItemsSearchByProducers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.RicercaArticoliPerProduttore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.Documents.AnnualTaxReporting-Year2004'
WHERE( NameSpace = 'MagoNet.Contabilita_it.Contabilita_ITDocuments.StampaIVAAnnuale2004' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.CostCentersAddOnFly'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDocuments.CentriCostoSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.SaleOrderLoading'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Documents.IntraArrivals'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDocuments.IntraAcquisti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.HomogeneousCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.CatOmogenee' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleOrderLoading'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.CaricaOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MOComponentsReplacement'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.SostituzioneComponentiOdP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Services.PaymentParameters'
WHERE( NameSpace = 'MagoNet.Partite.PartiteServices.ParametriPagamenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Services.NewYearGeneration'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaServices.CreaEsercizio' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InvoiceMng.Components.DocumentCopy'
WHERE( NameSpace = 'MagoNet.InvoiceMng.InvoiceMngComponents.CopiaDocumento' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.TaxExigibility'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.EsigibilitaIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MOMaintenance'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.ManutenzioneOdP' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.Documents.AccountingSimulations'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.ContabilitaPrevisionaleDocuments.SimulazioniContabili' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Services.ModifyElapsedTimePrecision'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaServices.ModificaPrecisioneElapsedTime' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.TaxExigibilityRebuilding'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.RicostruzioniEsigibilitaIVA' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Services.CostAccountingParameters'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaServices.ParametriAnalitica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MOConfirmation'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.Consuntivazione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Services.PayablesDeleting'
WHERE( NameSpace = 'MagoNet.Partite.PartiteServices.EliminaPartiteFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.CommodityCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.CatMerceologica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.TemporaryClosing'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.ChiusureTemporanee' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillsPrint'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.StampaEffetti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Documents.Operations'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDocuments.Operazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ShopPapersDeleting'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.CancellazioneDocumenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.SearchProductionPlan'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.CercaPianoProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.ItemsKit'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.ArticoliKit' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.GLJournalPrintOnDotMatrix'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaLibroAghi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Documents.Storages'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDocuments.Depositi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Documents.BOMWithVariant'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDocuments.DistintaConVariante' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.ReceiptMaintenance'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaRicevutaFiscale' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Documents.CostCenterGroups'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDocuments.GruppiCentriCosto' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.VouchersSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaReversali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.CreditNotesDeleting'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.EliminaNotaCreditoRicevuta' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccountingTransaction.Documents.ExtAccountingTemplate'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.ExtAccountingTemplate' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.Non-CollectedReceipt'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.RicevutaFiscaleNI' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccountingTransaction.Documents.InvAccTransParameters'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.InvAccTransParameters' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InvoiceMng.Documents.DocumentsParameters'
WHERE( NameSpace = 'MagoNet.InvoiceMng.InvoiceMngDocuments.ParametriDocumenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.AccountingDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.DefaultCodiciContabili' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.Documents.SubcntBoLMOComponentsList'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroDocuments.BdCForElencoMaterialiPressoFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Documents.ChartOfAccounts'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDocuments.PianoConti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Services.AccrualsDeferrals'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiServices.RateiRisconti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.DeliveryNotesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.EliminaDDT' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.CommissionCategories'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.CategorieProvv' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Documents.Locations'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDocuments.Ubicazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Services.ConaiParameters'
WHERE( NameSpace = 'MagoNet.Conai.ConaiServices.ParametriConai' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.BatchDocuments.SaleOrdFulfilment'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeBatchDocuments.EvasioneOrdCli' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.AccountingReasons'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.CausaliContabili' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.FIRRCalculation'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.CalcoloFIRR' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Documents.CommissionsEntries'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDocuments.MovimentiAgenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.InvoiceMaintenance'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaFatturaImmediata' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.PurchaseDocJE'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaIVARicevuti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.BatchDocuments.ABCAnalysis'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoBatchDocuments.AnalisiABC' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.Documents.OutstandingBills'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.PartiteAvanzatoDocuments.EffettiInsoluti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.Documents.SubcntBoLMOStepToProcessList'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroDocuments.BdCForElencoFasiOdPDaConsuntivare' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.BatchDocuments.LocationsEntries'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniBatchDocuments.MovimentiTraUbicazioni' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.SubstituteItems'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.ArticoliEquivalenti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.BalanceSheet-FinStat-Grouped'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioStatoPatrimonialeSintetico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.InventoryEntriesMaintenance'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.ManutenzioneMovMag' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.Documents.BalanceExport'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.EsportaBilancio' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Documents.ConfirmationCRPMO'
WHERE( NameSpace = 'MagoNet.CRP.CRPDocuments.ConfermaOdPDaCrp' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionRun'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.LancioInProduzione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Services.ReceivablesDeleting'
WHERE( NameSpace = 'MagoNet.Partite.PartiteServices.EliminaPartiteCliente' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Documents.JobGroups'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDocuments.GruppiCommesse' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillOExchangeSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaCambiali' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.SupplierActualUpdating'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.AggConsuntivoFornitore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.CustomersAddOnFly'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.ClientiSintetica' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Documents.Producers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDocuments.Produttori' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Documents.ReclassificationCopy'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioDocuments.CopiaRiclassificazione' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Documents.Questions'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDocuments.Domande' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Services.PurchaseOrdersDeleting'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriServices.EliminaOrdFor' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.PurchasesDefaults'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.DefaultCodiciAcquisti' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Documents.LoadComposition'
WHERE( NameSpace = 'MagoNet.CRP.CRPDocuments.ComposizioneCarico' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Services.ConfiguratorParameters'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreServices.ParametriConfiguratore' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.Branches'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.Sedi' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Services.ENASARCORebuilding'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiServices.RicostruzioniEnasarco' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Documents.CoordinatesDescriptions'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDocuments.DescriCoordinate' AND 
(
TypeId = @documentTypeId OR 
TypeId = @finderTypeId OR 
TypeId = @batchTypeId
))
END
GO