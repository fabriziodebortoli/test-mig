
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.CashOrderSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaRIBA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Services.Closing'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoServices.ChiusuraMagazzino' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.Documents.SubcntOrdOutsourcedMOSteps'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroDocuments.OrdForElencoFasiEsterneOdP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMExplosion'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.EsplosioneDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.CreditNote'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.NotaCredito' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.Services.TaxJournalNumbers'
WHERE( Command = 'MagoNet.IdsMng.IdsMngServices.NumeratoriRegistriIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.CommissionsGeneration'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.GenerazioneProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Documents.Tax'
WHERE( Command = 'MagoNet.Azienda.AziendaDocuments.IVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Services.EntriesDeleting'
WHERE( Command = 'MagoNet.Analitica.AnaliticaServices.EliminaMovimenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.Services.StorageDeleting'
WHERE( Command = 'MagoNet.MultiDeposito.MultiDepositoServices.EliminaDepositi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.Services.CustomerQuotationsDeleting'
WHERE( Command = 'MagoNet.OfferteClienti.OfferteClientiServices.EliminaOffCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.Documents.SerialNumbers'
WHERE( Command = 'MagoNet.LottiMatricole.LottiMatricoleDocuments.NumeratoriMatricole' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.CommissionsSettlement'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.LiquidazioneProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Jobs.Documents.JobsAddOnFly'
WHERE( Command = 'MagoNet.Commesse.CommesseDocuments.CommesseSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.Documents.RtgStepsGantt'
WHERE( Command = 'MagoNet.CRP.CRPDocuments.GanttFasi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.BatchDocuments.TaxCodeAssignment'
WHERE( Command = 'MagoNet.Articoli.ArticoliBatchDocuments.AssegnazioneCodIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SpecialTax.Documents.ProRataPrint'
WHERE( Command = 'MagoNet.RegimiIvaSpeciali.RegimiIVASpecialiDocuments.StampaProRata' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.BatchDocuments.PriceListsWizard'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziBatchDocuments.WizardListini' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.Documents.IntraReportGeneration'
WHERE( Command = 'MagoNet.Intrastat.IntrastatDocuments.StampaReportIntra' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.Documents.BalanceReclassifications'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiBilancioDocuments.RiclassificazioniPianoConti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.Documents.ReceiptsAndRequirements'
WHERE( Command = 'MagoNet.MRP.MRPDocuments.AnalisiFabbisogniVersamenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.BalanceRebuilding'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.RicostruzioneSaldiAgente' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MORollback'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.CorrezioneLavorazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_GR.Documents.KEPYO'
WHERE( Command = 'MagoNet.Contabilita_gr.Contabilita_GRDocuments.Kepyo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.SalesPeopleParameters'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.ParametriAgenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.Documents.Segments'
WHERE( Command = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.Segmenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.BillOfLadingMaintenance'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.ManutenzioneBollaCarico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.CorrectionAccInvoice'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.FatturaAccompagnatoriaACorrezione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.Documents.MRP'
WHERE( Command = 'MagoNet.MRP.MRPDocuments.MRP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.Invoice'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.FatturaImmediata' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.ReorderMaterialsToSupplier'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.RiordinoMaterialiMancanti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.BalanceGeneration'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.GeneraBilancio' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.Documents.ItemsMaterials'
WHERE( Command = 'MagoNet.Conai.ConaiDocuments.ArticoliMateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.VouchersPrint'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.StampaReversali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.PaymentOrders'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PagamentoMandati' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.Documents.SuppQuota'
WHERE( Command = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.CreditNoteMaintenance'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.ManutenzioneNotaCreditoRicevuta' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PaymentTerms.Documents.PaymentTerms'
WHERE( Command = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDocuments.CondizioniPagamento' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.IntangibleFixedAssetAddOnFly'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.CespitiImmaterialiSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Documents.FeesLinkedToJE'
WHERE( Command = 'MagoNet.Percipienti.PercipientiDocuments.ParcellePrimaNota' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SpecialTax.Services.TaxPlafond'
WHERE( Command = 'MagoNet.RegimiIvaSpeciali.RegimiIVASpecialiServices.PlafondIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Services.PyblsRcvblsParameters'
WHERE( Command = 'MagoNet.Partite.PartiteServices.ParametriPartite' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Configurator.Documents.QuestionLoad'
WHERE( Command = 'MagoNet.Configuratore.ConfiguratoreDocuments.CaricaDomanda' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.Clearing'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.SaldaConto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.IntangibleFixedAssets'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.CespitiImmateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AdditionalCharges.Documents.AutomaticAssociation'
WHERE( Command = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.AssociazioneAutomatica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.TangibleFixedAssetsAddOnFly'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.CespitiMaterialiSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.Documents.Lots'
WHERE( Command = 'MagoNet.LottiMatricole.LottiMatricoleDocuments.Lotti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.BatchDocuments.CustomerActualRebuilding'
WHERE( Command = 'MagoNet.Vendite.VenditeBatchDocuments.RicostruzioneConsuntivoClienti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.Documents.CustMaterialsExemption'
WHERE( Command = 'MagoNet.Conai.ConaiDocuments.ClientiMateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.BillsOfLadingDeleting'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.EliminaBollaCarico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.OwnedCompanies'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.Controllate' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMComponentsReplacement'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.SostituzioneComponenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.Areas'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.AreeVendita' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.InventoryAccounting.Documents.GenerateExtAccountingTpl'
WHERE( Command = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.GenerateExtAccountingTemplate' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.DDSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaRID' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.ProductionDevelopment'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.AvanzamentoProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.PurchaseOrderLoading'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.CaricaOrdFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.Documents.StubBooks'
WHERE( Command = 'MagoNet.IdsMng.IdsMngDocuments.Bollettari' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.ShopPapersPrint'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.StampaDocumentiProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.BatchDocuments.ItemsCopy'
WHERE( Command = 'MagoNet.Articoli.ArticoliBatchDocuments.CopiaArticolo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.AccountingParameters'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.ParametriContabilita' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.TaxSummaryJournalsPrint'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRiepiloghiIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.Documents.MOGantt'
WHERE( Command = 'MagoNet.CRP.CRPDocuments.GanttOdP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.NCReceiptsDeleting'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.EliminaRicevutaFiscaleNI' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.Services.SmartCodesParameters'
WHERE( Command = 'MagoNet.CodiceParlante.CodiceParlanteServices.ParametriCodiceParlante' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.ProducibilityAnalysis'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.AnalisiProducibilita' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Documents.CostAccountingEntries'
WHERE( Command = 'MagoNet.Analitica.AnaliticaDocuments.MovimentiAnalitici' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMConfirmation'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.RitornoDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.XGate.Services.XGateParameters'
WHERE( Command = 'MagoNet.XGate.XGateServices.ParametriXGate' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Documents.Company'
WHERE( Command = 'MagoNet.Azienda.AziendaDocuments.Azienda' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AdditionalCharges.Documents.DistributionTemplates'
WHERE( Command = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.ModelliRipartizione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Services.SaleOrdDefaults'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiServices.DefaultCodiciOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Documents.Ports'
WHERE( Command = 'MagoNet.Spedizioni.SpedizioniDocuments.Porti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.ClosedPayables'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PartiteFornitoreChiuse' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.CommodityCtgBySuppliers'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.CatMerceologicaFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Documents.PostableAssignment'
WHERE( Command = 'MagoNet.Analitica.AnaliticaDocuments.AssegnaMovimentabile' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.Documents.ItemsPriceLists'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziDocuments.ArticoliListini' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.PaymentOrdersPrint'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.StampaMandati' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.PaymentOrdersSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaMandati' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.Services.LotsDeleting'
WHERE( Command = 'MagoNet.LottiMatricole.LottiMatricoleServices.EliminaLotti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMNavigation'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.DistintaBaseGrafica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.CommissionRationalization'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.RazionalizzazionePoliticheProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Documents.LIFOFIFOItems'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoDocuments.ArticoliLifoFifo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.SuppliersAddOnFly'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.FornitoriSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.CommissionsInvoicing'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.FatturazioneProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.Services.IDNumbers'
WHERE( Command = 'MagoNet.IdsMng.IdsMngServices.Identificatori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BillUpdate'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.ModificaEffetto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Services.ReservedRebuilding'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiServices.RicostruzioneImpegnato' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.SalesOrdLoading'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.CaricaOrdiniClienti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Locations.Services.LocationsDefaults'
WHERE( Command = 'MagoNet.Ubicazioni.UbicazioniServices.DefaultCodiciUbicazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Services.DepreciationDeleting'
WHERE( Command = 'MagoNet.Cespiti.CespitiServices.EliminaAmmortamento' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Services.BalanceRebuilding'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoServices.RicostruzioneSaldi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.TangibleFixedAssetsCtg'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.CategorieMateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.Services.SalesPricePolicies'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziServices.PolitichePrezziCicloAttivo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ChartOfAccounts.Services.ChartOfAccountsParameters'
WHERE( Command = 'MagoNet.PianoDeiConti.PianoDeiContiServices.ParametriPianoConti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.Receipt'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.RicevutaFiscale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.BatchDocuments.LIFOFIFOProgress'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoBatchDocuments.FormazioneLifoFifo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.WithholdingTaxParameters'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ParametriRitenutaAcconto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.PickingList'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.BdP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.AddOnsPurchaseOrders.SubcontractorOrd'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroAddOnsOrdiniFornitori.OrdForLavorazioneEsterna' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Documents.DutyCodes'
WHERE( Command = 'MagoNet.Percipienti.PercipientiDocuments.CodiciTributo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AdditionalCharges.Documents.OneriAccessoriBatch'
WHERE( Command = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.OneriAccessoriBatch' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.SuppliersCategories'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.CategorieFornitori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.Documents.SubcontractorOrdGeneration'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroDocuments.GenerazioneOrdForLavorazioneEsterna' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.Components.PurchaseReqRequirementOrigin'
WHERE( Command = 'MagoNet.MRP.MRPComponents.RDAProvenienzaFabbisogni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.PurchaseOrderLoading'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOrdFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.Documents.IntraDispatches'
WHERE( Command = 'MagoNet.Intrastat.IntrastatDocuments.IntraCessioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.Services.PurchasesPricePolicies'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziServices.PolitichePrezziCicloPassivo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.TemplateExport'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.EsportaModello' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.Documents.MRPMOConfirmation'
WHERE( Command = 'MagoNet.MRP.MRPDocuments.ConfermaOdPDaMRP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.PurchasesParameters'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.ParametriAcquisti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.ProductionLotEdit'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.ModificaLotti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.InvoicesDeleting'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.EliminaFatturaImmediata' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Components.ItemData'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoComponents.DatiArticolo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.BatchDocuments.DocumentPosting'
WHERE( Command = 'MagoNet.Vendite.VenditeBatchDocuments.StampaRegistraDocVendita' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.AllowanceCalculation'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.CalcoloIndennita' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.PurchaseCorrectionInvoice'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.FatturaAcquistoACorrezione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.Documents.TaxJournals'
WHERE( Command = 'MagoNet.IdsMng.IdsMngDocuments.RegistriIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.Documents.Contacts'
WHERE( Command = 'MagoNet.Contatti.ContattiDocuments.Contatti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.BillsOfLadingPosting'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.RegistraBollaCarico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.SaleDocForecastJE'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPrevisionaleIVAEmessi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.SalesPeople'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.Agenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.AddOnsMaster.SaleOrdersPrint'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiAddOnsAnagrafiche.StampaOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Documents.ReasonCopy'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoDocuments.CopiaCausale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.Services.PriceListsDeleting'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziServices.EliminaListini' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.FixedAssetsReasons'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.CausaliCespiti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.BatchDocuments.SaleOrdFulfilmentEditing'
WHERE( Command = 'MagoNet.Vendite.VenditeBatchDocuments.EvasioneOrdCliModifica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.ProfitAndLossMonthlyClosing'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.ChiusureEconomiciMensili' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.Services.MultiStorageParameters'
WHERE( Command = 'MagoNet.MultiDeposito.MultiDepositoServices.ParametriMultiDeposito' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.BillOfLading'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.BollaCarico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Services.ItemsDeleting'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoServices.EliminaArticoli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BankTransferSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaBonifici' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Banks.Documents.eBanksAddOnFly'
WHERE( Command = 'MagoNet.Banche.BancheDocuments.BancheRidotte' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.Documents.PurchaseRequest'
WHERE( Command = 'MagoNet.MRP.MRPDocuments.RDA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.LeadTimeCalculation'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.CalcoloLeadTime' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.Services.StubBookNumbers'
WHERE( Command = 'MagoNet.IdsMng.IdsMngServices.NumeratoriBolle' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.ReturnsFromCustomer'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.ResoCliente' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.AccInvoiceMaintenance'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaFatturaAccompagnatoria' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.Documents.VariantCheck'
WHERE( Command = 'MagoNet.Varianti.VariantiDocuments.ControlloVarianti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemsNavigation'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.GraficaArticoli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Documents.Transport'
WHERE( Command = 'MagoNet.Spedizioni.SpedizioniDocuments.ModiTrasporto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Services.PurchaseOrdDefaults'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriServices.DefaultCodiciOrdFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.AddOnsSales.ConaiCalculation'
WHERE( Command = 'MagoNet.Conai.ConaiAddOnsVendite.CalcoloConai' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Banks.Documents.CustSuppBanks'
WHERE( Command = 'MagoNet.Banche.BancheDocuments.BancheCliFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.Items'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.Articoli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.ReturnToSupplierFromBoL'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.ResiFornitoreDaBdC' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.BalanceRebuilding'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.RicostruzioniContabili' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BillsSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaEffetti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.CodeConfirm'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.ConfermaCodice' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Documents.Carriers'
WHERE( Command = 'MagoNet.Spedizioni.SpedizioniDocuments.Vettori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMLoading'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.CaricaDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.CorrectionInvoice'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.FatturaImmediataACorrezione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.Documents.StorageGroups'
WHERE( Command = 'MagoNet.MultiDeposito.MultiDepositoDocuments.RaggruppamentiDepositi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ProductCtg'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.CatProdotto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Services.ActualCostsCheck'
WHERE( Command = 'MagoNet.Produzione.ProduzioneServices.ControlloCostiConsuntivi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Documents.SaleOrdersPrint'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.StampaOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Services.ConsBalSheetParameters'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiServices.ParametriBilanciConsolidati' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.Documents.WCChart'
WHERE( Command = 'MagoNet.CRP.CRPDocuments.ChartCdL' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PaymentTerms.Documents.CreditCard'
WHERE( Command = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDocuments.CartaCredito' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Configurator.Documents.Configuration'
WHERE( Command = 'MagoNet.Configuratore.ConfiguratoreDocuments.Configurazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.DeliveryNotes'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.DDT' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.BatchDocuments.DeferredInvoicing'
WHERE( Command = 'MagoNet.Vendite.VenditeBatchDocuments.FatturazioneDifferita' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.InvoiceMng.Components.CorrectionDocument'
WHERE( Command = 'MagoNet.InvoiceMng.InvoiceMngComponents.CorrezioneDocumento' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Documents.CodeConfirm'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.ConfermaCodice' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.Documents.Receipts'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.Carichi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.CommissionPoliciesCopy'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.CopiaPoliticheProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AdditionalCharges.Documents.AdditionalCharges'
WHERE( Command = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.OneriAccessori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.SalesPeopleAddOnFly'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.AgentiRidotta' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Services.FixedAssetsRebuilding'
WHERE( Command = 'MagoNet.Cespiti.CespitiServices.RicostruzioniCespiti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Services.CollectionParameters'
WHERE( Command = 'MagoNet.Partite.PartiteServices.ParametriIncassi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Services.CurrenciesParameters'
WHERE( Command = 'MagoNet.Azienda.AziendaServices.ParametriDivise' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.Documents.ValuationUpToDate'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.ValorizzazioneAllaData' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.SuppQuotaLoading'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOffFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMImplosion'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.ImplosioneDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.BatchDocuments.StandardCostRebuilding'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziBatchDocuments.RicalcoloCostoStd' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.Documents.IntraDispatchesLinkedToJE'
WHERE( Command = 'MagoNet.Intrastat.IntrastatDocuments.IntraCessioniPrimaNota' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.AddOnsMaster.CustomerQuotationsPrint'
WHERE( Command = 'MagoNet.OfferteClienti.OfferteClientiAddOnsAnagrafiche.StampaOffCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Banks.Documents.CompanyBanks'
WHERE( Command = 'MagoNet.Banche.BancheDocuments.BancheAzienda' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.Documents.ProspectiveSuppliers'
WHERE( Command = 'MagoNet.Contatti.ContattiDocuments.FornitoriPotenziali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.CustomersCategories'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.CategorieClienti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.AddOnsSales.SubcontractorDN'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroAddOnsVendite.DDTForLavorazioneEsterna' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemsAddOnFly'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.ArticoliSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Components.ItemProductionData'
WHERE( Command = 'MagoNet.Produzione.ProduzioneComponents.DatiArticoloProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.SummaryByTaxJournal'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.RiepiloghiPerRegistroIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.TangibleFixedAssets'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.CespitiMateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.BillOfLadingToInvoiceLoading'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.CaricaMultiBdC' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.Documents.Root'
WHERE( Command = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.Radice' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.Documents.SubcontractorDNGeneration'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroDocuments.GenerazioneDDTForLavorazioneEsterna' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.AddOnsMaster.SupplierQuotationsPrint'
WHERE( Command = 'MagoNet.OfferteFornitori.OfferteFornitoriAddOnsAnagrafiche.StampaOffFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.AddOnsMaster.PurchaseOrdersPrint'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriAddOnsAnagrafiche.StampaOrdFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.PaymentOrdersIssue'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.EmissioneMandati' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Services.SalesOrdParameters'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiServices.ParametriOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.Locations'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.Ubicazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMProduction'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.ProduzioneDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.SalesParameters'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ParametriVendite' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.Documents.CRP'
WHERE( Command = 'MagoNet.CRP.CRPDocuments.Crp' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.CreditNoteMaintenance'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaNotaCredito' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.OpenReceivables'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PartiteClienteAperte' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BillsCollection'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.IncassoEffetti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.DepreciationForecast'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.AmmortamentoPrevisionale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.Services.LotsManagementSetting'
WHERE( Command = 'MagoNet.LottiMatricole.LottiMatricoleServices.ImpostaGestioneLotto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.Documents.ContactsDeleting'
WHERE( Command = 'MagoNet.Contatti.ContattiDocuments.EliminaContatti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.Documents.PurchaseOrdFromPurchaseReq'
WHERE( Command = 'MagoNet.MRP.MRPDocuments.RdAGenerazioneOrdiniAFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Services.OrderedRebuilding'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriServices.RicostruzioneOrdinato' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.NCReceiptMaintenance'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaRicevutaFiscaleNI' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.IntangibleFixedAssetsCtg'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.CategorieImmateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.Documents.BOMLoading'
WHERE( Command = 'MagoNet.Varianti.VariantiDocuments.CaricaDistinta' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.InvoicesPosting'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.RegistraFatturaAcquisto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.CommissionsRebuilding'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.RicalcoloProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMRun'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.LancioDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MOConfirmationBoLList'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.ConsuntivazioneElencoBdC' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Services.PayeesDefaults'
WHERE( Command = 'MagoNet.Percipienti.PercipientiServices.ParametriPercipienti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.CreditNotesDeleting'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.EliminaNotaCredito' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Documents.SaleOrd'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.JEDeleting'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.EliminaPrimaNotaDefinitiva' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.TaxDeclaration'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.DichiarazioneIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.Documents.LotsAddOnFly'
WHERE( Command = 'MagoNet.LottiMatricole.LottiMatricoleDocuments.LottiSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Services.MailParameters'
WHERE( Command = 'MagoNet.Azienda.AziendaServices.ParametriMail' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.BatchDocuments.InventoryAdjustment'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoBatchDocuments.RettificaInventariale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.BillOfLadingLoading'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.CaricaBollaCarico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.BOMPosting'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.MovimentazioneDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.Customers'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.Clienti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.Services.NumberParameters'
WHERE( Command = 'MagoNet.IdsMng.IdsMngServices.ParametriNumeratori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.Documents.MRPDefaultAssignation'
WHERE( Command = 'MagoNet.MRP.MRPDocuments.ImpostazioniDefaultMRP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.BillOfLadingPosting'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdForRegistraMovMag' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.Documents.DefinitiveValuation'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.ValorizzazioneDefinitiva' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.CommissionPolicies'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.PoliticheProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.DefinitiveClosing'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.ChiusurePatrimoniali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_GR.Documents.TrialBalance'
WHERE( Command = 'MagoNet.Contabilita_gr.Contabilita_GRDocuments.TrialBalance' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.Documents.SubcntBoLShopPapersList'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroDocuments.BdCForElencoDocumentiAlFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.Heading'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.Vidima' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.SalesDefaults'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.DefaultCodiciVendite' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.Components.SmartCodeHelp'
WHERE( Command = 'MagoNet.CodiceParlante.CodiceParlanteComponents.CodiceParlanteHelp' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemsLanguageDescri'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.ArticoliDescriLingua' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.FixedAssetsEntries'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.MovimentiCespiti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.AccInvoice'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.FatturaAccompagnatoria' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Documents.CostCenters'
WHERE( Command = 'MagoNet.Analitica.AnaliticaDocuments.CentriCosto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Services.ActualCosts'
WHERE( Command = 'MagoNet.Produzione.ProduzioneServices.RicalcoloCostiConsuntivi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BOMCosting'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.CostificazioneDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.InvoiceLoading'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.CaricaFatturaAcquisto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.TaxJournalsPrint'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRegistriIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.PANSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaMAV' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.Documents.VariantLoading'
WHERE( Command = 'MagoNet.Varianti.VariantiDocuments.CaricaVariante' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.BatchDocuments.ReceiptsDeferredInvoicing'
WHERE( Command = 'MagoNet.Vendite.VenditeBatchDocuments.FatturazioneDifferitaRicevutaFiscale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BillsApproval'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.AccettazioneEffetti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierPricesAdj.Documents.SupplierPricesAdj'
WHERE( Command = 'MagoNet.AllineamentoPrezziFornitore.AllineamentoPrezziFornitoreDocuments.AllineamentoPrezziFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.GLJournalTotals'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.TotaliLibroGiornale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.CommissionsEntriesDeleting'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.EliminaMovimentiAgenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.Documents.PriceLists'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziDocuments.Listini' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.BalanceSheet-PL-Grouped'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioContoEconomicoSintetico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Services.MagoXpToProdBaseConversion'
WHERE( Command = 'MagoNet.Produzione.ProduzioneServices.ConversioneMagoNet11' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Documents.Fees'
WHERE( Command = 'MagoNet.Percipienti.PercipientiDocuments.Parcelle' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.InvoicesDeleting'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.EliminaFatturaAcquisto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemsPurchaseBarCodes'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.ArticoliBarCodeAcquisto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.AccountingTemplates'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.ModelliContabili' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.PureForecastJE'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPrevisionalePura' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.PaymentOrderUpdate'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.ModificaMandato' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.Documents.ConaiEntries'
WHERE( Command = 'MagoNet.Conai.ConaiDocuments.MovimentiConai' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.PickingRequests'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.RichiesteSpedizione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.AddOnsMaster.ContactsCopy'
WHERE( Command = 'MagoNet.Contatti.ContattiAddOnsAnagrafiche.CopiaContatto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.Documents.ReceiptsRestore'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.RipristinoCarichi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.ReturnToSupplier'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.ResoFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.Documents.Drawings'
WHERE( Command = 'MagoNet.Cicli.CicliDocuments.Disegni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.ProfitAndLossClosing'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.ChiusureEconomici' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.DNLoading'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.CaricaDDT' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Services.BalanceRebuilding'
WHERE( Command = 'MagoNet.Analitica.AnaliticaServices.RicostruzioniAnalitica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.Suppliers'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.Fornitori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BankTransferPrint'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.StampaBonifici' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierPricesAdj.Documents.SupplierPricesAdjConfig'
WHERE( Command = 'MagoNet.AllineamentoPrezziFornitore.AllineamentoPrezziFornitoreDocuments.ConfigurazioneAPF' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.InventoryAccounting.Documents.ExtAccountingTemplateCopy'
WHERE( Command = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.CopyExtAccountingTemplate' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemsCustomers'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.ArticoliCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.PureJE'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPura' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.Documents.IntraArrivalsLinkedToJE'
WHERE( Command = 'MagoNet.Intrastat.IntrastatDocuments.IntraAcquistiPrimaNota' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.CommodityCtgByCustomers'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.CatMerceologicaCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemsSuppliers'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.ArticoliFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.PurchaseOrdersPrint'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.StampaOrdFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.ForecastJEDeleting'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.EliminaPrimaNotaPrevisionale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Documents.FeeTemplates'
WHERE( Command = 'MagoNet.Percipienti.PercipientiDocuments.ModelliParcelle' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Services.MOComponentsEntriesCheck'
WHERE( Command = 'MagoNet.Produzione.ProduzioneServices.ControlloMovimentazioneMateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Services.MOItemsInventoryEntriesCheck'
WHERE( Command = 'MagoNet.Produzione.ProduzioneServices.ControlloMovimentazioneProdotti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MOModification'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.ModificaLavorazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.Documents.WC'
WHERE( Command = 'MagoNet.Cicli.CicliDocuments.CdL' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.Services.SupplierQuotationsDeleting'
WHERE( Command = 'MagoNet.OfferteFornitori.OfferteFornitoriServices.EliminaOffFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Services.BOMParameters'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseServices.ParametriDistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.CustSuppCopy'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.CopiaCliFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Languages.Documents.Languages'
WHERE( Command = 'MagoNet.Lingue.LingueDocuments.Lingue' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.Documents.Combination'
WHERE( Command = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.Possibilita' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Services.RecalculateReservedOrdered'
WHERE( Command = 'MagoNet.Produzione.ProduzioneServices.RicalcoloImpegnatoOrdinato' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.Documents.Teams'
WHERE( Command = 'MagoNet.Cicli.CicliDocuments.Squadre' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.BalanceValuesResume'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.RipresaBilancio' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.TotalsByTaxJournal'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.TotaliPerRegistroIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.Documents.SmartCode'
WHERE( Command = 'MagoNet.CodiceParlante.CodiceParlanteDocuments.CodiceParlante' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.Invoice'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.FatturaAcquisto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Services.ItemsParameters'
WHERE( Command = 'MagoNet.Articoli.ArticoliServices.ParametriArticoli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.Services.NonFiscalNumbers'
WHERE( Command = 'MagoNet.IdsMng.IdsMngServices.NumeratoriNonFiscali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.DeliveryNoteMaintenance'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaDDT' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.Documents.CustomerQuotationsPrint'
WHERE( Command = 'MagoNet.OfferteClienti.OfferteClientiDocuments.StampaOffCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.OpenPayables'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PartiteFornitoreAperte' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.CompanyGroups'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.GruppoAziende' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.Documents.CustQuota'
WHERE( Command = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BillsPresentation'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PresentazioneEffetti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.Disposal'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.Dismissione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ItemCodes.Documents.ItemCodes'
WHERE( Command = 'MagoNet.VTSZSZJ.VTSZSZJDocuments.VTSZSZJCodes' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ChargePolicies.Documents.ChargePolicies'
WHERE( Command = 'MagoNet.PoliticheSpese.PoliticheSpeseDocuments.PoliticheSpese' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MaterialRequirementsPicking'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.PrelievoMaterialiMancanti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.CorrectionReceipt'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.RicevutaFiscaleACorrezione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.TrialBalance-Grouped'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioVerificaSintetico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.Services.LotsSerialsParameters'
WHERE( Command = 'MagoNet.LottiMatricole.LottiMatricoleServices.ParametriLottiMatricole' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.Depreciation'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.AmmortamentoCespiti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.Documents.PackageTypes'
WHERE( Command = 'MagoNet.Conai.ConaiDocuments.MaterialiTipologie' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.CreditNotesPosting'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.RegistraNotaCreditoRicevuta' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Services.SaleOrdersDeleting'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiServices.EliminaOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.Classes'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.Classi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.VouchersPresentation'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PresentazioneReversali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Documents.Packages'
WHERE( Command = 'MagoNet.Spedizioni.SpedizioniDocuments.Imballi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.PurchaseDocForecastJE'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaPrevisionaleIVARicevuti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Documents.InventoryReasons'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoDocuments.CausaliMagazzino' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.ProductionPlanGeneration'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.GenerazionePianoProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Services.FixedAssetsParameters'
WHERE( Command = 'MagoNet.Cespiti.CespitiServices.ParametriCespiti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Services.InventoryDefaults'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoServices.DefaultCodiciMagazzino' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.Services.SingleStepLifoFifoParameters'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiServices.ParametriLifoFifoScattiContinui' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.ReceiptsDeleting'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.EliminaRicevutaFiscale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.UnitsOfMeasure'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.UnitaMisura' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.PrivacyStatementPrint'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.StampaLettereConsenso' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Documents.Titles'
WHERE( Command = 'MagoNet.Azienda.AziendaDocuments.Titoli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemType'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.TipoArticolo' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Services.CustSuppParameters'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriServices.ParametriClientiFornitori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.ActualAccrualDate'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.DataEffettivaMaturazione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.BalanceImport'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.ImportaBilancio' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.GroupingCodes'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.CodiciRaggruppamento' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.MultiCompanyBalances'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.BilanciConsolidati' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Documents.CostAccEntriesGeneration'
WHERE( Command = 'MagoNet.Analitica.AnaliticaDocuments.GeneraMovimenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Components.DocumentDetailImportExport'
WHERE( Command = 'MagoNet.Articoli.ArticoliComponents.ImportExportRigheDocumento' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.Documents.ProspectiveSuppliersDeleting'
WHERE( Command = 'MagoNet.Contatti.ContattiDocuments.EliminaFornitoriPotenziali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.ClosedReceivables'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PartiteClienteChiuse' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.RetailSalesDistribution'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.VentilazioneCorrispettivi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Services.Form770Rebuilding'
WHERE( Command = 'MagoNet.Percipienti.PercipientiServices.Ricostruzioni770' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.AddOnsPurchases.SubcontractorBoL'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroAddOnsAcquisti.BdCForLavorazioneEsterna' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MRPMO'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.OdPDaMRP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Services.InventoryEntriesDeleting'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoServices.EliminaMovMag' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.AccInvoicesDeleting'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.EliminaFatturaAccompagnatoria' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.Documents.CustQuotaLoading'
WHERE( Command = 'MagoNet.OfferteClienti.OfferteClientiDocuments.CaricaOffCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.ProductionPlan'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.PianoProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.BatchDocuments.CustomerPriceListUpdate'
WHERE( Command = 'MagoNet.PolitichePrezzi.PolitichePrezziBatchDocuments.UpdateListinoCliente' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.InvoiceMaintenance'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.ManutenzioneFatturaAcquisto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Documents.TemplateSave'
WHERE( Command = 'MagoNet.Analitica.AnaliticaDocuments.SalvaModello' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.Documents.Calendars'
WHERE( Command = 'MagoNet.Cicli.CicliDocuments.Calendari' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.GLJournalPrint'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaLibro' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.Documents.IntraFileGeneration'
WHERE( Command = 'MagoNet.Intrastat.IntrastatDocuments.StampaFileIntra' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MO'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.OdP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.BillOfMaterials'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.DistintaBase' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Documents.InitialValuesPosting'
WHERE( Command = 'MagoNet.Cespiti.CespitiDocuments.RiportoIniziali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Documents.ISOCountryCodes'
WHERE( Command = 'MagoNet.Azienda.AziendaDocuments.ISOStati' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_IT.Documents.AnnualTaxReporting'
WHERE( Command = 'MagoNet.Contabilita_it.Contabilita_ITDocuments.StampaIVAAnnuale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.Documents.BreakdownReasons'
WHERE( Command = 'MagoNet.Cicli.CicliDocuments.CausaliNonDisp' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.SaleDocJE'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaIVAEmessi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Services.ManufacturingParameters'
WHERE( Command = 'MagoNet.Produzione.ProduzioneServices.ParametriProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ChartOfAccounts.Documents.ChartOfAccountsAddOnFly'
WHERE( Command = 'MagoNet.PianoDeiConti.PianoDeiContiDocuments.PianoContiRidotto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.OwnerCompanies'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.Controllanti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.Departments'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.Reparti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Jobs.Documents.Jobs'
WHERE( Command = 'MagoNet.Commesse.CommesseDocuments.Commesse' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.TemplateImport'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.ImportaModello' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.Documents.Variants'
WHERE( Command = 'MagoNet.Varianti.VariantiDocuments.Varianti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.Documents.SupplierQuotationsPrint'
WHERE( Command = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.StampaOffFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AdditionalCharges.Documents.ItemToCtgAssociations'
WHERE( Command = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.AssociazioniArticoliCategorie' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AdditionalCharges.Documents.BillOfLadingLoading'
WHERE( Command = 'MagoNet.OneriAccessori.OneriAccessoriDocuments.CaricaBollaCaricoOA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.CreditNoteNegSignForm'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.FincatoNotaCreditoSegniNegativi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.BatchDocuments.StoragesEntries'
WHERE( Command = 'MagoNet.MultiDeposito.MultiDepositoBatchDocuments.MovimentiTraDepositi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Services.InventoryParameters'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoServices.ParametriMagazzino' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Documents.Currencies'
WHERE( Command = 'MagoNet.Azienda.AziendaDocuments.Divise' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Documents.CreditNotes'
WHERE( Command = 'MagoNet.Acquisti.AcquistiDocuments.NotaCreditoRicevuta' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.CommisionStateSetting'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.ImpostaStatoPoliticheProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.Services.SaleOrdActualRebuilding'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniClientiServices.RicostruzioneConsuntivoOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.SupplierReorder'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.RiordinoFornitori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.Payables'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PartiteFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.Services.LotsRebuilding'
WHERE( Command = 'MagoNet.LottiMatricole.LottiMatricoleServices.RicostruzioneLotti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.TemplateGeneration'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.GeneraModello' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.ReorderMaterialsToProduction'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.RiordinoProduzioneMancanti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.PurchaseOrd'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Documents.InventoryEntries'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoDocuments.MovimentiMagazzino' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.InvoiceLoading'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.CaricaFattura' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ChartOfAccounts.Documents.ChartOfAccountsNavigation'
WHERE( Command = 'MagoNet.PianoDeiConti.PianoDeiContiDocuments.GraficoPianoConti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.BalanceSheet'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioContrapposto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.ConsolidationTemplates'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.ModelliConsolidamento' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.Documents.ItemsMaterials'
WHERE( Command = 'MagoNet.Conai.ConaiDocuments.AssociazioneArticoliMateriali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Components.ItemsSearchByProducers'
WHERE( Command = 'MagoNet.Articoli.ArticoliComponents.RicercaArticoliPerProduttore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_IT.Documents.AnnualTaxReporting-Year2004'
WHERE( Command = 'MagoNet.Contabilita_it.Contabilita_ITDocuments.StampaIVAAnnuale2004' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Documents.CostCentersAddOnFly'
WHERE( Command = 'MagoNet.Analitica.AnaliticaDocuments.CentriCostoSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Documents.SaleOrderLoading'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.CaricaOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.Documents.IntraArrivals'
WHERE( Command = 'MagoNet.Intrastat.IntrastatDocuments.IntraAcquisti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.HomogeneousCtg'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.CatOmogenee' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.SaleOrderLoading'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.CaricaOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MOComponentsReplacement'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.SostituzioneComponentiOdP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Services.PaymentParameters'
WHERE( Command = 'MagoNet.Partite.PartiteServices.ParametriPagamenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Services.NewYearGeneration'
WHERE( Command = 'MagoNet.Azienda.AziendaServices.CreaEsercizio' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.InvoiceMng.Components.DocumentCopy'
WHERE( Command = 'MagoNet.InvoiceMng.InvoiceMngComponents.CopiaDocumento' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.TaxExigibility'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.EsigibilitaIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MOMaintenance'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.ManutenzioneOdP' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ForecastAccounting.Documents.AccountingSimulations'
WHERE( Command = 'MagoNet.ContabilitaPrevisionale.ContabilitaPrevisionaleDocuments.SimulazioniContabili' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Services.ModifyElapsedTimePrecision'
WHERE( Command = 'MagoNet.Azienda.AziendaServices.ModificaPrecisioneElapsedTime' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.TaxExigibilityRebuilding'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.RicostruzioniEsigibilitaIVA' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Services.CostAccountingParameters'
WHERE( Command = 'MagoNet.Analitica.AnaliticaServices.ParametriAnalitica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.MOConfirmation'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.Consuntivazione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Services.PayablesDeleting'
WHERE( Command = 'MagoNet.Partite.PartiteServices.EliminaPartiteFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.CommodityCtg'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.CatMerceologica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.TemporaryClosing'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.ChiusureTemporanee' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BillsPrint'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.StampaEffetti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.Receivables'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.PartiteCliente' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.Documents.Operations'
WHERE( Command = 'MagoNet.Cicli.CicliDocuments.Operazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.ShopPapersDeleting'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.CancellazioneDocumenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.Documents.SearchProductionPlan'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBaseDocuments.CercaPianoProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.ItemsKit'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.ArticoliKit' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.GLJournalPrintOnDotMatrix'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaLibroAghi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.Documents.Storages'
WHERE( Command = 'MagoNet.MultiDeposito.MultiDepositoDocuments.Depositi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.Documents.BOMWithVariant'
WHERE( Command = 'MagoNet.Varianti.VariantiDocuments.DistintaConVariante' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.ReceiptMaintenance'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaRicevutaFiscale' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Documents.CostCenterGroups'
WHERE( Command = 'MagoNet.Analitica.AnaliticaDocuments.GruppiCentriCosto' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.VouchersSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaReversali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.CreditNotesDeleting'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.EliminaNotaCreditoRicevuta' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.InventoryAccounting.Documents.ExtAccountingTemplate'
WHERE( Command = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.ExtAccountingTemplate' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Documents.Non-CollectedReceipt'
WHERE( Command = 'MagoNet.Vendite.VenditeDocuments.RicevutaFiscaleNI' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.InventoryAccounting.Documents.InvAccTransParameters'
WHERE( Command = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDocuments.InvAccTransParameters' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.InvoiceMng.Documents.DocumentsParameters'
WHERE( Command = 'MagoNet.InvoiceMng.InvoiceMngDocuments.ParametriDocumenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Services.AccountingDefaults'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaServices.DefaultCodiciContabili' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.Documents.SubcntBoLMOComponentsList'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroDocuments.BdCForElencoMaterialiPressoFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ChartOfAccounts.Documents.ChartOfAccounts'
WHERE( Command = 'MagoNet.PianoDeiConti.PianoDeiContiDocuments.PianoConti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AccrualsDeferrals.Services.AccrualsDeferrals'
WHERE( Command = 'MagoNet.RateiRisconti.RateiRiscontiServices.RateiRisconti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.DeliveryNotesDeleting'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.EliminaDDT' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.CommissionCategories'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.CategorieProvv' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Locations.Documents.Locations'
WHERE( Command = 'MagoNet.Ubicazioni.UbicazioniDocuments.Ubicazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.Services.ConaiParameters'
WHERE( Command = 'MagoNet.Conai.ConaiServices.ParametriConai' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.BatchDocuments.SaleOrdFulfilment'
WHERE( Command = 'MagoNet.Vendite.VenditeBatchDocuments.EvasioneOrdCli' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.AccountingReasons'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.CausaliContabili' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.FIRRCalculation'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.CalcoloFIRR' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Documents.CommissionsEntries'
WHERE( Command = 'MagoNet.Agenti.AgentiDocuments.MovimentiAgenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Services.InvoiceMaintenance'
WHERE( Command = 'MagoNet.Vendite.VenditeServices.ManutenzioneDocVenditaFatturaImmediata' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Documents.PurchaseDocJE'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaDocuments.PrimaNotaIVARicevuti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.BatchDocuments.ABCAnalysis'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoBatchDocuments.AnalisiABC' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.Documents.OutstandingBills'
WHERE( Command = 'MagoNet.PartiteAvanzato.PartiteAvanzatoDocuments.EffettiInsoluti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.Documents.SubcntBoLMOStepToProcessList'
WHERE( Command = 'MagoNet.ContoLavoro.ContoLavoroDocuments.BdCForElencoFasiOdPDaConsuntivare' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.Documents.ReceiptsRebuilding'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.RicostruzioneCarichi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Locations.BatchDocuments.LocationsEntries'
WHERE( Command = 'MagoNet.Ubicazioni.UbicazioniBatchDocuments.MovimentiTraUbicazioni' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.SubstituteItems'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.ArticoliEquivalenti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BatchDocuments.BalanceSheet-FinStat-Grouped'
WHERE( Command = 'MagoNet.Contabilita.ContabilitaBatchDocuments.BilancioStatoPatrimonialeSintetico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Services.InventoryEntriesMaintenance'
WHERE( Command = 'MagoNet.Magazzino.MagazzinoServices.ManutenzioneMovMag' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.Documents.BalanceExport'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDocuments.EsportaBilancio' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.Documents.ConfirmationCRPMO'
WHERE( Command = 'MagoNet.CRP.CRPDocuments.ConfermaOdPDaCrp' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.Documents.ProductionRun'
WHERE( Command = 'MagoNet.Produzione.ProduzioneDocuments.LancioInProduzione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Services.ReceivablesDeleting'
WHERE( Command = 'MagoNet.Partite.PartiteServices.EliminaPartiteCliente' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Jobs.Documents.JobGroups'
WHERE( Command = 'MagoNet.Commesse.CommesseDocuments.GruppiCommesse' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Documents.BillOExchangeSlip'
WHERE( Command = 'MagoNet.Partite.PartiteDocuments.DistintaCambiali' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.SupplierActualUpdating'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.AggConsuntivoFornitore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.CustomersAddOnFly'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.ClientiSintetica' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Documents.Producers'
WHERE( Command = 'MagoNet.Articoli.ArticoliDocuments.Produttori' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.Documents.ReclassificationCopy'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiBilancioDocuments.CopiaRiclassificazione' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Configurator.Documents.Questions'
WHERE( Command = 'MagoNet.Configuratore.ConfiguratoreDocuments.Domande' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.Services.PurchaseOrdersDeleting'
WHERE( Command = 'MagoNet.OrdiniFornitori.OrdiniFornitoriServices.EliminaOrdFor' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Services.PurchasesDefaults'
WHERE( Command = 'MagoNet.Acquisti.AcquistiServices.DefaultCodiciAcquisti' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.Documents.LoadComposition'
WHERE( Command = 'MagoNet.CRP.CRPDocuments.ComposizioneCarico' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Configurator.Services.ConfiguratorParameters'
WHERE( Command = 'MagoNet.Configuratore.ConfiguratoreServices.ParametriConfiguratore' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.Documents.Branches'
WHERE( Command = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.Sedi' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Services.ENASARCORebuilding'
WHERE( Command = 'MagoNet.Agenti.AgentiServices.RicostruzioniEnasarco' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Locations.Documents.CoordinatesDescriptions'
WHERE( Command = 'MagoNet.Ubicazioni.UbicazioniDocuments.DescriCoordinate' AND 
(
Type = 1 OR 
Type = 7 OR 
Type = 8
))
END
GO