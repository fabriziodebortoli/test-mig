if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 
	DECLARE @reportTypeId as integer 
	SET @reportTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 4)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.InvoicesPortfolio'
WHERE( NameSpace = 'MagoNet.Vendite.PortafoglioClientiFattureImmediate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.RetailSalesToBeDistributedSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoRegistroIVACorrispettiviVentilazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.SummarizedCosting'
WHERE( NameSpace = 'MagoNet.DistintaBase.CostificazioneSintetica' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.OrderedByItemTot'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.TotaliOrdinatoArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.EUTaxJournalByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVACEEPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Det-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriDettGrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BankDraftsRequest'
WHERE( NameSpace = 'MagoNet.Partite.RichiestaAssegniCircolari' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CreditNotesReview'
WHERE( NameSpace = 'MagoNet.Vendite.SpuntaCompletaNoteDiCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.DistributionSpread'
WHERE( NameSpace = 'MagoNet.Contabilita.RipartoVentilazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Non-DistribAccountsInEntries'
WHERE( NameSpace = 'MagoNet.Analitica.TestContiNonRipartiti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.CustomersByCommodityCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ClientiPerCategorieMerceologiche' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrdersToProcessList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoOrdiniDiProduzioneDaProdurre' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.OrderedByCustomer-Detailed'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdinatoClientePerAnno' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.BillsOfLadingPortfolioTot'
WHERE( NameSpace = 'MagoNet.Acquisti.TotaliPortafoglioFornitoriBolleCarico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.OwnerCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.Controllanti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.PrivacyStatementLetter'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.LetteraConsenso' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrdersPortfolioTot'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.TotaliPortafoglioOrdiniAFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Reasons'
WHERE( NameSpace = 'MagoNet.Cespiti.CausaliMovimento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.CashOrderSlip-OnPaper'
WHERE( NameSpace = 'MagoNet.Partite.DistintaRIBASuCarta' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.IntraArrivals1A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiBis' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesJournal-EUAnnotationsByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVAVenditeAcquistiCEEPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.JournalEntryCheck'
WHERE( NameSpace = 'MagoNet.Percipienti.ControlloParcelle' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsByJob'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiPerCommessa' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesTaxByLog'
WHERE( NameSpace = 'MagoNet.Contabilita.SpuntaPrimeNoteIVAPerProtocollo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersYearly-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.budgetescentrigrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BillOfExchange'
WHERE( NameSpace = 'MagoNet.Partite.Cambiale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MagicDocuments.ExpenseNote'
WHERE( NameSpace = 'MagoNet.MagicDocuments.ExpenseNote' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BankTransferRequest'
WHERE( NameSpace = 'MagoNet.Partite.RichiestaBonifici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchaseJournalNumCheck'
WHERE( NameSpace = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAAcquisti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.CustomersCategories'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.CategorieClienti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ManufacturingItemSheet'
WHERE( NameSpace = 'MagoNet.DistintaBase.SchedaArticoloProd' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrdersSchedule'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.ScadenzarioOrdiniDettaglioTeste' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxExigibility'
WHERE( NameSpace = 'MagoNet.Contabilita.EsigibilitaIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.JobsByGroup'
WHERE( (NameSpace = 'MagoNet.Analitica.CommessePerGruppo' OR NameSpace = 'MagoNet.Commesse.CommessePerGruppo') AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.BaseProductionProgressAnalysis'
WHERE( NameSpace = 'MagoNet.Produzione.AnalisiStatoProduzioneBase' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersFullCosting'
WHERE( NameSpace = 'MagoNet.Analitica.CentriFullCost' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.JobTicketsList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoBolleDiLavorazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.CashOrder'
WHERE( NameSpace = 'MagoNet.Partite.RIBA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.GLJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.GeneralLedger' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.CreditNotesPortfolioTot'
WHERE( NameSpace = 'MagoNet.Acquisti.TotaliPortafoglioFornitoriNoteCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrdersPortfolio'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.PortafoglioOrdini' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SaleJournalNumCheck'
WHERE( NameSpace = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAVendite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.BillsOfLadingPortfolio'
WHERE( NameSpace = 'MagoNet.Acquisti.PortafoglioFornitoriBolleCarico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DeliveryNotesPortfolio'
WHERE( NameSpace = 'MagoNet.Vendite.PortafoglioClientiDDT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Non-RegAccountsInEntries'
WHERE( NameSpace = 'MagoNet.Analitica.TestContiMovimenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.EntriesToAccrualDeferral'
WHERE( NameSpace = 'MagoNet.RateiRisconti.OperazioniSoggetteARateoRisconto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ProductionPlanSheet'
WHERE( NameSpace = 'MagoNet.DistintaBase.SchedaPianiDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.SerialNosLabels'
WHERE( NameSpace = 'MagoNet.Magazzino.EtichetteMatricole' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheet-FinStat-Det'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioStatoPatrimonialeAnalitico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.InterestsOnArrearCalculation'
WHERE( NameSpace = 'MagoNet.Partite.CalcoloInteressiMora' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DeliveryNotes'
WHERE( NameSpace = 'MagoNet.Vendite.ListaDDTBolleAccompagnatorie' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Det-Grp-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseDettGrpProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PapersPrintSubcontactorDeliveryNoteMaterials'
WHERE( NameSpace = 'MagoNet.Produzione.StampaDocMaterialiDDT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.CreditNotesDeleting'
WHERE( NameSpace = 'MagoNet.Acquisti.EliminaNotaCreditoRicevuta' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CreditNoteNegSignForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoNotaCreditoSegniNegativi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.BalanceByFixedAsset'
WHERE( NameSpace = 'MagoNet.Cespiti.BilancioPerCespite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersDirectCosting'
WHERE( NameSpace = 'MagoNet.Analitica.CentriDirectCost' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.CurrenciesFixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseFixing' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesByDocumentData'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabileperDatiDocumento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsSheetByReason'
WHERE( NameSpace = 'MagoNet.Magazzino.SchedaArticoloPerCausale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.TaxJournals'
WHERE( NameSpace = 'MagoNet.IdsMng.RegistriIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.EUTaxJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVACEE' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.ForecastTaxSummaryJournal'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.RiepiloghiIVAPrevisionali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.SupplierAccountStatements'
WHERE( NameSpace = 'MagoNet.Partite.EstrattiContoFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrder-TimesDeviation'
WHERE( NameSpace = 'MagoNet.Produzione.ScostamentiTempiOrdiniDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentMonthly-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseContiCentriProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.ItemSheetByPostingDate'
WHERE( NameSpace = 'MagoNet.MultiDeposito.SchedaArticoloDiMagazzinoPerDeposito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchasesJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVAAcquisti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentMonthly-Det-Grp-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriDettGrpProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.WHoldingTaxPaymentCertific'
WHERE( NameSpace = 'MagoNet.Percipienti.AttestatiRitenute' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.CalendarSheet'
WHERE( NameSpace = 'MagoNet.Cicli.SchedaCalendario' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Non-RegJobsInEntries'
WHERE( NameSpace = 'MagoNet.Analitica.TestCommesseMovimenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Det'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FinancialSimulation'
WHERE( NameSpace = 'MagoNet.Cespiti.SimulazioneFinanziario' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.FiscalData'
WHERE( NameSpace = 'MagoNet.Magazzino.DatiFiscaliEsercizio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.TangibleFixedAssetsCtg'
WHERE( NameSpace = 'MagoNet.Cespiti.CategorieMateriali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.SubcontratorAnalysis'
WHERE( NameSpace = 'MagoNet.ContoLavoro.AnalisiTerzisti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Non-RegCostCentersInTpl'
WHERE( NameSpace = 'MagoNet.Analitica.TestCentriModelli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PickingListNumberSheet'
WHERE( NameSpace = 'MagoNet.Produzione.StampaNumeroBuonoDiPrelievo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.ChildMOList'
WHERE( NameSpace = 'MagoNet.MRP.ElencoOrdiniProduzioneFiglio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.CurrenciesSheet'
WHERE( NameSpace = 'MagoNet.Divise.SchedaDivise' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Non-RegAccountsInEntries'
WHERE( NameSpace = 'MagoNet.Contabilita.ContiMovimentatiNonCensiti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.ItemsToOrder'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ProspettoArticoliDaOrdinare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.PaidWHoldingTax'
WHERE( NameSpace = 'MagoNet.Percipienti.RitenuteVersate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.IntraArrivalsCoverSheet'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiFrontespizio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.OperationsList'
WHERE( NameSpace = 'MagoNet.Cicli.ElencoOperazioni' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersActual-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.consuntivocentrigrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsActual-Det'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoCommesseDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesByCustomer'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilePerCliFor' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Glad'
WHERE( NameSpace = 'MagoNet.Percipienti.Glad' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsSheets-Det'
WHERE( NameSpace = 'MagoNet.Analitica.CommesseSchede' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.PriceListsDescriptions'
WHERE( NameSpace = 'MagoNet.Lingue.DescrizioniListini' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentersJobsBal'
WHERE( NameSpace = 'MagoNet.Analitica.conticentricommessesaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.RetailSalesDailyJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporoGionaliero' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Transport'
WHERE( NameSpace = 'MagoNet.Spedizioni.ModiTrasporto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.TurnoverByItemAndCust-Det'
WHERE( NameSpace = 'MagoNet.Vendite.FatturatoArticoloClienteSuRighe' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Grp-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriRiepGrpSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.QuestionSheet'
WHERE( NameSpace = 'MagoNet.Configuratore.SchedaDomanda' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.ContactQuotations'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteAClienteContatti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.DDSlip'
WHERE( NameSpace = 'MagoNet.Partite.DistintaRID' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Items'
WHERE( NameSpace = 'MagoNet.Articoli.Articoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.VouchersToBePresentedByCust'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.ReversaliPresentabiliCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsSheets'
WHERE( NameSpace = 'MagoNet.Analitica.ContiCommesseSchedeRiep' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentersActual'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoContiCentri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PapersPrintSubcontactorOrderMaterials'
WHERE( NameSpace = 'MagoNet.Produzione.StampaDocMaterialiOrdFor' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsJournal-History'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroStorico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BankTransferSlip'
WHERE( NameSpace = 'MagoNet.Partite.DistintaBonifici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.PriceListsByCategories'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.ListiniPerCategorie' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.CreditNotesPosting'
WHERE( NameSpace = 'MagoNet.Acquisti.RegistraNotaCreditoRicevuta' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.ProRataCalculation'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.ProRataCalculation' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.ShortMediumTermRcvblsByCust'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.CreditiMedioTerminePerCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CorrectionInvoiceForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoFatturaACorrezione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.JobsMasterSheet'
WHERE( (NameSpace = 'MagoNet.Analitica.CommesseSchedaAnagrafica' OR NameSpace = 'MagoNet.Commesse.CommesseSchedaAnagrafica') AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Buffetti8916FForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoBuffettiFatturaRicevutaFiscale8916F' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsMonthly-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseContiCommesseSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsBalancing-Balance'
WHERE( NameSpace = 'MagoNet.Cespiti.QuadraturaCespitiBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TrialBalance-ByLedger'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioRiepilogativo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.ItemsSuppliers'
WHERE( NameSpace = 'MagoNet.Acquisti.ProspettoFornitoriArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxGeneralSummaryJournal-Detailed'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoGeneraleRegistriIVADettaglio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersActual-Det'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoCentriDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Det-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseDettProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PapersPrintQuantityToPickList'
WHERE( NameSpace = 'MagoNet.Produzione.StampaDocElencoQuantitaDaPrelevareSuBdP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Parameters'
WHERE( NameSpace = 'MagoNet.Percipienti.Parametri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.FeeTemplates'
WHERE( NameSpace = 'MagoNet.Percipienti.ModelliParcelle' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.WCLoad'
WHERE( NameSpace = 'MagoNet.CRP.CaricoCentriDiLavoro' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Det-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseDettGrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.EUBalSheet-Memorandum'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.BilancioCEEContiOrdine' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsJournal-Bal-Det'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.JobTicketSheet'
WHERE( NameSpace = 'MagoNet.Produzione.SchedaBolleDiLavorazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.LIFOData'
WHERE( NameSpace = 'MagoNet.Magazzino.DatiLIFO' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.CustQuotationsPortfolio'
WHERE( NameSpace = 'MagoNet.OfferteClienti.PortafoglioOfferte' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.ProposedMO'
WHERE( NameSpace = 'MagoNet.MRP.OrdiniProduzioneProposti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.KEPYO'
WHERE( NameSpace = 'MagoNet.Contabilita_gr.Kepyo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.SubstituteItems'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliEquivalenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.ForecastPurchasesJournal'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.RegistriIVARicevutiPrevisionali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.CustSuppBalancing'
WHERE( NameSpace = 'MagoNet.Contabilita.QuadraturaCliforPartite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Departments'
WHERE( NameSpace = 'MagoNet.Articoli.Reparti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.ByFixedAsset'
WHERE( NameSpace = 'MagoNet.Cespiti.FiscalePerCespite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.ItemsSuppliersLeadTime'
WHERE( NameSpace = 'MagoNet.Acquisti.LeadTimeFornitoriArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Non-RegJobsInTpl'
WHERE( NameSpace = 'MagoNet.Analitica.TestCommesseModelli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ProductionPlans'
WHERE( NameSpace = 'MagoNet.DistintaBase.PianiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.TaxCodeAssignment'
WHERE( NameSpace = 'MagoNet.Articoli.AssegnazioneCodIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByStorageAndSupp-Item'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerDepositoFornitoreEArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.WCList'
WHERE( NameSpace = 'MagoNet.Cicli.ElencoCentriDiLavoro' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrdersSchedule'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ScadenzarioTesteOrdiniAFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Slips'
WHERE( NameSpace = 'MagoNet.Partite.Distinte' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Non-CollectedReceipts'
WHERE( NameSpace = 'MagoNet.Vendite.RicevuteFiscaliNonIncassate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.QuarterlyIntraDispatches2A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniBisTrimestrale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.IntraDispatches2B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniTer' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.CustomersLedgerCards'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.LedgerCardsCustomers' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsByLocation'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiPerUbicazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Buffetti8904F3Form'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoBuffettiBollaFattura8904F3' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.BillOfLadingForm'
WHERE( NameSpace = 'MagoNet.Acquisti.FincatoBollaCarico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsBudget'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoCommesse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.OrderedToSupplierTot'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.TotaliOrdinatoFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByCustomer'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SaleJournalNumCheckByDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAVenditePerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.MultipleReclassifiedAccounts'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.ContiRiclassificatiPiuVolte' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxByDocumentData'
WHERE( NameSpace = 'MagoNet.Contabilita.IVAperDatiDocumento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.RetailSalesJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_PL.SalesJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_pl.SalesRegister' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.ValuationUpToDate'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.ValorizzazioneAllaData' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Non-RegCustSuppInEntries'
WHERE( NameSpace = 'MagoNet.Contabilita.CliForMovimentatiNonCensiti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ManufacturingSummarizedBOMExplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.EsplosioneDistintaBaseRiepilogativaProd' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchaseJournNumCheckByDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAAcquistiPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrderForm'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.FincatoOrdineCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.EntriesByMaterials'
WHERE( NameSpace = 'MagoNet.Conai.MovimentiPerMateriale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsBudget-Det'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoCommesseDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.QuarterlyIntraDispatches2B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniTerTrimestrale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.ENASARCOFIRRPaymentSlip'
WHERE( NameSpace = 'MagoNet.Agenti.DistintaEnasarcoFIRR' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.SmartCodeSegmentsSheet'
WHERE( NameSpace = 'MagoNet.CodiceParlante.SchedaSegmenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Receipts'
WHERE( NameSpace = 'MagoNet.Vendite.RicevuteFiscali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FinanacialByFixedAsset'
WHERE( NameSpace = 'MagoNet.Cespiti.FinanziarioPerCespite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.MonthlyIntraDispatches2B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniTerMensile' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentersSheets'
WHERE( NameSpace = 'MagoNet.Analitica.ContiCentriSchedeRiep' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheet-FinStat'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioDiEsercizioStatoPatrimoniale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.TurnoverByItemAndCust'
WHERE( NameSpace = 'MagoNet.Vendite.FatturatoArticoloCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.ENASARCOCertificate'
WHERE( NameSpace = 'MagoNet.Agenti.AttestatiEnasarco' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentersBal'
WHERE( NameSpace = 'MagoNet.Analitica.ContiCentriSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.ForecastJournalEntries'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.SpuntaPrevisionali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.DisabledCostCentersInTpl'
WHERE( NameSpace = 'MagoNet.Analitica.TestCentriDisModelli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DeliveryNotesReview'
WHERE( NameSpace = 'MagoNet.Vendite.SpuntaCompletaDDTBolleAccompagnatorie' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.JobsByCustomer'
WHERE( (NameSpace = 'MagoNet.Analitica.CommessePerCliente' OR NameSpace = 'MagoNet.Commesse.CommessePerCliente') AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.SalesPeopleAccountStatements'
WHERE( NameSpace = 'MagoNet.Agenti.EstrattiContoEnasarco' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersSheets-FullCost'
WHERE( NameSpace = 'MagoNet.Analitica.CentriSchedeFull' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.TeamsSheet'
WHERE( NameSpace = 'MagoNet.Cicli.SchedaSquadre' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccompanyingInvoices'
WHERE( NameSpace = 'MagoNet.Vendite.ListaFattureAccompagnatorie' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.TurnoverByCustomer'
WHERE( NameSpace = 'MagoNet.Vendite.FatturatoSinteticoCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DeliveryNotesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.EliminaDDT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.SalePrices'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PrezziDiVendita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.BanksBillAccounts'
WHERE( NameSpace = 'MagoNet.Banche.BancheAziendaCCEffetti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Payables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.QuarterlyIntraArrivals1B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiTerTrimestrale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.RoutingsSheet'
WHERE( NameSpace = 'MagoNet.DistintaBase.SchedaCicli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.BOMList'
WHERE( NameSpace = 'MagoNet.DistintaBase.ElencoDistinte' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JournalEntryCheck'
WHERE( NameSpace = 'MagoNet.Analitica.ControlloPrimeNote' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.GLJournalDraft'
WHERE( NameSpace = 'MagoNet.Contabilita.BrogliaccioLibroGiornale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.ENASARCOFromBalances'
WHERE( NameSpace = 'MagoNet.Agenti.EnasarcoDaSaldiAgenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.SuppQuotationsPortfolio'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.PortafoglioOfferteDaFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.DrawingsList'
WHERE( NameSpace = 'MagoNet.Cicli.ElencoDisegni' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.TangibleFA-PurchaseData'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiMaterialiDatiAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.QuarterlyIntraArrivals1A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiBisTrimestrale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.PLAnalysis-Detailed'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiEconomiciAnalitico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.FIFOProgress'
WHERE( NameSpace = 'MagoNet.Magazzino.formazionefifo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.InventoryValuationByType'
WHERE( NameSpace = 'MagoNet.Magazzino.ACostiConSelezioneSuTipo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.TaxReporting-Year2004'
WHERE( NameSpace = 'MagoNet.Contabilita_it.ComunicazioneIVA2004' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.DataForVTFrame'
WHERE( NameSpace = 'MagoNet.Contabilita_it.DatiQuadroVT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersByGroup'
WHERE( NameSpace = 'MagoNet.Analitica.CentriPerGruppo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.InvoicesPortfolioTot'
WHERE( NameSpace = 'MagoNet.Acquisti.TotaliPortafoglioFornitoriFattureAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AccountsCardsByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ContiPerRegistrazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.StorageGroups'
WHERE( NameSpace = 'MagoNet.MultiDeposito.RaggruppamentiDepositi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsSheet'
WHERE( NameSpace = 'MagoNet.Magazzino.SchedaArticoloDiMagazzino' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.CustSuppEntriesWrongAccount'
WHERE( NameSpace = 'MagoNet.Contabilita.CliForMovimentatiPdCDiverso' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.BillsOfLadingList'
WHERE( NameSpace = 'MagoNet.Acquisti.ListaBolleDiCarico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Storages'
WHERE( NameSpace = 'MagoNet.MultiDeposito.Depositi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReceiptsDeferredInvoicing'
WHERE( NameSpace = 'MagoNet.Vendite.FatturazioneDifferitaRicevutaFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.EUBalSheet-PL'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.BilancioCEEContoEconomico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MagicDocuments.ExpenseNoteList'
WHERE( NameSpace = 'MagoNet.MagicDocuments.ExpenseNoteList' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriRiep' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.GoodsMaster'
WHERE( NameSpace = 'MagoNet.Articoli.MerciDatiAnagrafici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PANSlip'
WHERE( NameSpace = 'MagoNet.Partite.DistintaMAV' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.PurchaseRequestList'
WHERE( NameSpace = 'MagoNet.MRP.ElencoRDA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SuppliersCardsByAccrualDate'
WHERE( NameSpace = 'MagoNet.Contabilita.FornitoriPerCompetenza' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.SuppQuotationsPortfolioTot'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.TotaliPortafoglioOfferteDaFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Parameters'
WHERE( NameSpace = 'MagoNet.Cespiti.Parametri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.ContactsAddressBook'
WHERE( NameSpace = 'MagoNet.Contatti.RubricaContatti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Exceptions'
WHERE( NameSpace = 'MagoNet.Acquisti.ElencoNote' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.ProspSuppQuotations'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.ListaOfferteDaFornitorePotenziale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.CustomerPymtSchedule'
WHERE( NameSpace = 'MagoNet.Partite.ScadenzarioClientiPerDataScadenza' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.DutyCodes'
WHERE( NameSpace = 'MagoNet.Percipienti.CodiciTributo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.SubstituteItemsSheet'
WHERE( NameSpace = 'MagoNet.Produzione.StampaMaterialiEquivalenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.ProspectiveSuppliersSheet'
WHERE( NameSpace = 'MagoNet.Contatti.SchedaFornitoriPotenziali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Parameters'
WHERE( NameSpace = 'MagoNet.Analitica.parametri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsByCategory'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiPerCategoria' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Company'
WHERE( NameSpace = 'MagoNet.Azienda.Azienda' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxDeclaration'
WHERE( NameSpace = 'MagoNet.Contabilita.DichiarazioneIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostAccTemplates'
WHERE( NameSpace = 'MagoNet.Analitica.ModelliAnalitici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FAJournal-Simulation'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroSimulatoRaggruppato' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.IntraArrivals1B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiTer' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.OwnedCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.Controllate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesBySuppAndStorage-Item'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerFornitoreDepositoEArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.InvoicesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.EliminaFatturaImmediata' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.AnnualIntraArrivals1A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiBisAnnuale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.VouchersSlips'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.DistinteReversali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.RoutingVariantSheet'
WHERE( NameSpace = 'MagoNet.Varianti.SchedaVariantiCiclo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PapersPrintPickingLists'
WHERE( NameSpace = 'MagoNet.Produzione.StampaDocElencoBuoniDiPrelievo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.SuppliersLabels'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.EtichetteFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrder-CostsDeviation'
WHERE( NameSpace = 'MagoNet.Produzione.ScostamentoCostiOrdiniDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.OrderedByItem-Detailed'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdinatoArticoloPerPeriodoAnalitico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.LedgerCards'
WHERE(NameSpace = 'MagoNet.Contabilita.SchedeContabili'  AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsActual-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoCommesseGrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.GLJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.Librogiornale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.InvoicesPortfolioTot'
WHERE( NameSpace = 'MagoNet.Vendite.TotaliPortafoglioClientiFattureImmediate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.IssuedPaymentOrders'
WHERE( NameSpace = 'MagoNet.Partite.MandatiEmessi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.JobsByCode'
WHERE( (NameSpace = 'MagoNet.Analitica.CommessePerCodice' OR NameSpace = 'MagoNet.Commesse.CommessePerCodice') AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.PlafondUse'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.ProspettoUtilizzoPlafond' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CreditNotesPortfolioTot'
WHERE( NameSpace = 'MagoNet.Vendite.TotaliPortafoglioClientiNoteDiCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Det-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseDettSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.CashFlow'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.CashFlowGenerale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.PaymentTerms'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.CustQuotationsPortfolioTot'
WHERE( NameSpace = 'MagoNet.OfferteClienti.TotaliPortafoglioOfferte' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsTemplates'
WHERE( NameSpace = 'MagoNet.Analitica.CommesseModelliAnalitici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CreditNotesPosting'
WHERE( NameSpace = 'MagoNet.Vendite.StampaRegistraNotaCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Maintenance'
WHERE( NameSpace = 'MagoNet.Cespiti.SpeseManutenzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsSheets'
WHERE( NameSpace = 'MagoNet.Analitica.commesseschederiep' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Entries'
WHERE( NameSpace = 'MagoNet.Cespiti.MovimentiAmmortamento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.ByFiscalYear'
WHERE( NameSpace = 'MagoNet.Cespiti.FiscalePerEsercizio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.CustSuppBalSheetByAccount'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioCliForPerRaggruppamentoContabile' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.CommisionCategories'
WHERE( NameSpace = 'MagoNet.Agenti.CategorieProvvigionali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AccountingTemplatesSheets'
WHERE( NameSpace = 'MagoNet.Contabilita.SchedeModelliContabili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.ItemsByStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.ArticoliPerDeposito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.TrialBalance'
WHERE( NameSpace = 'MagoNet.Contabilita_gr.TrialBalance' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriRiepProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsBalancing'
WHERE( NameSpace = 'MagoNet.Cespiti.QuadraturaCespitiFiscali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByItemAndStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerArticoloDepositoPeriodo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ItemsBasePrices'
WHERE( NameSpace = 'MagoNet.Articoli.PrezziBaseArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.SubcontractorDeliveryNote'
WHERE( NameSpace = 'MagoNet.ContoLavoro.DDTAlFornitorePerLavorazioneEsterna' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.GoodsHandlingData'
WHERE( NameSpace = 'MagoNet.Magazzino.MerciDatiMovimentazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Non-CollectedReceiptsReview'
WHERE( NameSpace = 'MagoNet.Vendite.SpuntaCompletaRicevuteFiscaliNonIncassate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ItemsLabels'
WHERE( NameSpace = 'MagoNet.Articoli.EtichetteArticolo2x8' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ProductionProgressAnalysis'
WHERE( NameSpace = 'MagoNet.Produzione.AnalisiStatoProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.InventoryReasonsSheet'
WHERE( NameSpace = 'MagoNet.Magazzino.CausaliDiMagazzino' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.IntangibleFixedAssetsCtg'
WHERE( NameSpace = 'MagoNet.Cespiti.CategorieImmateriali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsByClass'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiPerClasse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ReorderMaterialsToProduction'
WHERE( NameSpace = 'MagoNet.Produzione.RiordinoProduzioneMaterialiMancanti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsInABCAnalysis'
WHERE( NameSpace = 'MagoNet.Magazzino.ArticoliInclusiInAnalisiABC' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ItemsTypes'
WHERE( NameSpace = 'MagoNet.Articoli.TipiArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.InvoicesPosting'
WHERE( NameSpace = 'MagoNet.Vendite.StampaRegistraFatturaImmediata' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.BalanceScraps'
WHERE( NameSpace = 'MagoNet.Cespiti.EliminazioniBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.NCReceiptsDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.EliminaRicevutaFiscaleNI' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DeliveryNotesPortfolioTot'
WHERE( NameSpace = 'MagoNet.Vendite.TotaliPortafoglioClientiDDT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Tax'
WHERE( NameSpace = 'MagoNet.Azienda.IVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetSimulation'
WHERE( NameSpace = 'MagoNet.Cespiti.FixedAssetSimulation' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.ALAnalysis'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiPatrimonialiRiepilogativo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.BillOfMaterials'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBase' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.PurchaseRequestSheet'
WHERE( NameSpace = 'MagoNet.MRP.SchedaRDA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.EntriesByReason'
WHERE( NameSpace = 'MagoNet.Cespiti.MovimentiPerCausale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.InvoicesListByMaterials'
WHERE( NameSpace = 'MagoNet.Conai.ElencoFattureVendita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.MaterialSuppliedToSubcntList'
WHERE( NameSpace = 'MagoNet.ContoLavoro.SituazioneMaterialiPressoFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Non-ReclassifiedAccounts'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.ContiNonRiclassificati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.CommissionPolicies'
WHERE( NameSpace = 'MagoNet.Agenti.ListaDiSpuntaPoliticheProvvigionali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.TaxReporting-Year2002'
WHERE( NameSpace = 'MagoNet.Contabilita_it.ComunicazioneIVA2002' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Form770Review'
WHERE( NameSpace = 'MagoNet.Percipienti.Spunta770' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AnnualTaxDeclarationData'
WHERE( NameSpace = 'MagoNet.Contabilita.DatiPerDichiarazioneIVAAnnuale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.ProRataDetails'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.ProRataDetails' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.SummarizedBOMImplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.ImplosioneDistintaBaseRiepilogativa' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.TurnoverByArea'
WHERE( NameSpace = 'MagoNet.Agenti.FatturatoAgentePerArea' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrder-EstimatedDates'
WHERE( NameSpace = 'MagoNet.Produzione.PrevisioneDateOrdiniDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.AccountingSimulationsReview'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.SpuntaSimulazioniContabili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.LIFOReceipts'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.Carichi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Det-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriDettSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.CommissionsSettlementStatus'
WHERE( NameSpace = 'MagoNet.Agenti.ListaProvvigioniLiquidateDaLiquidare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.TurnoverByCustAndItem'
WHERE( NameSpace = 'MagoNet.Vendite.FatturatoClienteArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.CustomersCardsByAccrualDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ClientiPerCompetenza' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Non-RegCostCentersInEntries'
WHERE( NameSpace = 'MagoNet.Analitica.TestCentriMovimenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.NoticeOfPayment'
WHERE( NameSpace = 'MagoNet.Partite.AvvisoPagamento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.OrderedBySupplier'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ProspettoOrdinatoFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReceiptForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoRicevutaFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ABCAnalysis'
WHERE( NameSpace = 'MagoNet.Magazzino.AnalisiABC' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsMonthly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseContiCommesse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.OrderedByCustomer'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.TotaliOrdinatoClientePerAnno' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Carriers'
WHERE( NameSpace = 'MagoNet.Spedizioni.Vettori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.EUAnnotationJournalNumCheck'
WHERE( NameSpace = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVACEE' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DNDeferredInvoicing'
WHERE( NameSpace = 'MagoNet.Vendite.FatturazioneDifferitaDDT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.PresentedVouchers'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.ReversaliPresentate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.InvoiceForm'
WHERE( NameSpace = 'MagoNet.Acquisti.FincatoFatturaAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Titles'
WHERE( NameSpace = 'MagoNet.Azienda.Titoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.ItemsValuationAtSalePrices'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.APrezziDiVendita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.StoragesByItem'
WHERE( NameSpace = 'MagoNet.MultiDeposito.DepositiPerArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.CreditNotesPortfolio'
WHERE( NameSpace = 'MagoNet.Acquisti.PortafoglioFornitoriNoteCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.BOMStructure'
WHERE( NameSpace = 'MagoNet.DistintaBase.StrutturaDistintaBase' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BillOExchangeSlip'
WHERE( NameSpace = 'MagoNet.Partite.DistintaCambiali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.ItemsLots'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiDegliArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.SmartCodeRoots'
WHERE( NameSpace = 'MagoNet.CodiceParlante.RadiciPerCodiceParlante' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CreditNotesPortfolio'
WHERE( NameSpace = 'MagoNet.Vendite.PortafoglioClientiNoteDiCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxByCustSupp'
WHERE( NameSpace = 'MagoNet.Contabilita.IVAperClifor' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.CashJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.CashJournal' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.LIFOProgress'
WHERE( NameSpace = 'MagoNet.Magazzino.formazionelifo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Invoices'
WHERE( NameSpace = 'MagoNet.Vendite.ListaFattureImmediate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ProductionDevelopment-OrderComponents'
WHERE( NameSpace = 'MagoNet.Produzione.AvanzamentoProduzioneElencoMateriali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.SupplierQuotationForm'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.FincatoOffertaFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.SerialNosLabels'
WHERE( NameSpace = 'MagoNet.Vendite.EtichetteMatricole2x8' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByStorageAndCust'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerDepositoECliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersSheets'
WHERE( NameSpace = 'MagoNet.Analitica.CENTRISCHEDERIEP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Locations'
WHERE( NameSpace = 'MagoNet.Cespiti.Ubicazioni' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.WCProcessingList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoLavorazioniPerCdL' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.ExpiredSupplierQuotations'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.SpuntaOfferteDaFornitoreScadute' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TrialBalance-Grouped'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioDiVerificaSintetico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.InvoicesReview'
WHERE( NameSpace = 'MagoNet.Vendite.SpuntaCompletaFattureImmediate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FAInInventory'
WHERE( NameSpace = 'MagoNet.Cespiti.InventarioFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.EntriesBySalesperson'
WHERE( NameSpace = 'MagoNet.Agenti.ListaMovimenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.EntriesByDate'
WHERE( NameSpace = 'MagoNet.Magazzino.MovimentiPerData' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Non-RegItemsInEntries'
WHERE( NameSpace = 'MagoNet.Magazzino.ArticoliMovimentatiNonCensiti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.EmployedDismissedSalesPeople'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiAssuntiCessati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.ItemsInLanguage'
WHERE( NameSpace = 'MagoNet.Lingue.ArticoliInLingua' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.BanksAccounts'
WHERE( NameSpace = 'MagoNet.Banche.BancheAziendaCCBancari' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.CustPymtScheduleByName'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.ScadenzarioClientiPerRagioneSociale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.ExceptionsByDocumentType'
WHERE( NameSpace = 'MagoNet.Acquisti.ListaDelleEccezioniPerTipoDocumento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.AuthorizedNotInvoicedEntries'
WHERE( NameSpace = 'MagoNet.Agenti.MovimentiAutorizzatiENonFatturati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FiscalIncome'
WHERE( NameSpace = 'MagoNet.Cespiti.RedditoFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Areas'
WHERE( NameSpace = 'MagoNet.Agenti.AreeDiVendita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TrialBalance-Det-WoCustSupp'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioDiVerificaAnaliticoSenzaCliFor' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.DDSlip-OnPaper'
WHERE( NameSpace = 'MagoNet.Partite.DistintaRIDSuCarta' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.BalanceRatios'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.IndiciDiBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.FeesByDate'
WHERE( NameSpace = 'MagoNet.Percipienti.ParcellePerData' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DeliveryNotesPosting'
WHERE( NameSpace = 'MagoNet.Vendite.StampaRegistraDDT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsActual'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoContiCommesse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.IntraDispatchesCoverSheet'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniFrontespizio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.DisposalEntries'
WHERE( NameSpace = 'MagoNet.Cespiti.MovimentiDismissione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.SalesGoodsJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.SalesSummaryForDeliveryGoods' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccompanyingInvoiceForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoFatturaAccompagnatoria' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Heading'
WHERE( NameSpace = 'MagoNet.Contabilita.vidima' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.NCReceiptsPortfolio'
WHERE( NameSpace = 'MagoNet.Vendite.PortafoglioClientiRicevuteFiscaliNonIncassate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.ItemsSuppliers'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ProspettoFornitoriArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.BalanceSimulation'
WHERE( NameSpace = 'MagoNet.Cespiti.SimulazioneBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxForfaitSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoIVAForfait' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.CustomersByItem'
WHERE( NameSpace = 'MagoNet.Articoli.ClientiPerArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.WCPlan'
WHERE( NameSpace = 'MagoNet.CRP.PianoCentriDiLavoro' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrdersSchedule-Detailed'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.ScadenzarioOrdiniDettaglioRighe' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsOn-HandByMonth'
WHERE( NameSpace = 'MagoNet.Magazzino.DisponibilitaArticoliPerMese' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsYearly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetEsCommesse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BillsToBePresented'
WHERE( NameSpace = 'MagoNet.Partite.EffettiPresentabili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AccountsCardsByAccrualDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ContiPerCompetenza' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.LastSaleData'
WHERE( NameSpace = 'MagoNet.Articoli.DatiUltimaVendita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ReceiptLabelsWithBarCodes'
WHERE( NameSpace = 'MagoNet.Articoli.EtichetteDiCarico2x8ConCodiciABarre' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsJournal-Bal-Sim'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroSimulatoRaggruppatoBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ProductsComposition'
WHERE( NameSpace = 'MagoNet.Articoli.StrutturaComposizioneProdotto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.TurnoverByCustomer'
WHERE( NameSpace = 'MagoNet.Agenti.FatturatoAgentePerCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesPure'
WHERE( NameSpace = 'MagoNet.Contabilita.SpuntaPrimeNotePure' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccInvoicesPortfolio'
WHERE( NameSpace = 'MagoNet.Vendite.PortafoglioClientiFattureAccompagnatorie' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.ALAnalysis-Detailed'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiPatrimonialiAnalitico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrderImplosion'
WHERE( NameSpace = 'MagoNet.Produzione.ImplosioneOdP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersSheets-Det'
WHERE( NameSpace = 'MagoNet.Analitica.CentriSchede' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.InvoicesList'
WHERE( NameSpace = 'MagoNet.Acquisti.ListaFattureDiAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.ForecastSalesJournal'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.RegistriIVAEmessiPrevisionali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.CustomerAccountStatements'
WHERE( NameSpace = 'MagoNet.Partite.EstrattiContoClienti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheet'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioContrapposto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentersMonthly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseContiCentri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.SalesPeopleByAreaManager'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiPerCapoarea' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.PartiallyExemptDeclaration'
WHERE( NameSpace = 'MagoNet.Conai.DichiarazioneClientiEsenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.BalanceByFiscalYear'
WHERE( NameSpace = 'MagoNet.Cespiti.BilancioPerEsercizio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.ProspSuppTaxIdNoCheck'
WHERE( NameSpace = 'MagoNet.Contatti.ControlloPartitaIVAFornitoriPotenziali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.CustomersSheet'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.SchedaClienti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.IntangibleFixedAssetsLabels'
WHERE( NameSpace = 'MagoNet.Cespiti.EtichetteCespitiImmateriali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsMonthly-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseContiCommesseProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PaymentOrderSlips'
WHERE( NameSpace = 'MagoNet.Partite.DistinteMandati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PapersPrintManufacturingOrderSheet'
WHERE( NameSpace = 'MagoNet.Produzione.StampaDocSchedaOrdiniDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.TeamsPlan'
WHERE( NameSpace = 'MagoNet.CRP.PianoSquadre' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PickingLists'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoBuoniDiPrelievo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheet-PL'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioDiEsercizioContoEconomico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Buffetti8904M2Form'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoBuffettiFattura8904M2' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.AnnualIntraDispatches2A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniBisAnnuale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.ContactsSheet'
WHERE( NameSpace = 'MagoNet.Contatti.SchedaContatti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.EntriesByDepartment'
WHERE( NameSpace = 'MagoNet.Magazzino.MovimentiPerReparto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsMarkup-Detailed'
WHERE( NameSpace = 'MagoNet.Magazzino.RicarichiArticoloDettaglio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Buffetti8902FBForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoBuffettiBolla8902FB' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrdersSchedule-Items'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ScadenzarioOrdiniAFornitorePerArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByCustAndStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerClienteEDeposito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.CashFlowPayables'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.CashFlowPartiteFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.SmartCodeSegments'
WHERE( NameSpace = 'MagoNet.CodiceParlante.SegmentiPerCodiceParlante' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsYearly-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetEsCommesseGrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersYearly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetEsCentri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.CompanyGroups'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.GruppoAziende' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.BalanceDisposals'
WHERE( NameSpace = 'MagoNet.Cespiti.DismissioniBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.PurchasesJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.PurchasingJournal' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.DirectCosting'
WHERE( NameSpace = 'MagoNet.Analitica.DirectCosting' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrderForm'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.FincatoOrdineFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.TaxSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.GeneralVATJournalSummaries' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.OrderedByItem'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdinatoArticoloPerPeriodoSintetico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsJourn-Bal-Det-Sim'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroSimulatoBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReceiptsReview'
WHERE( NameSpace = 'MagoNet.Vendite.SpuntaCompletaRicevuteFiscali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TrialBalance-Detailed'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioDiVerificaAnalitico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.TrialBalance'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.TrialBalance' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.ComponentsVariantsList'
WHERE( NameSpace = 'MagoNet.Varianti.ElencoVariantiComponenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Labels'
WHERE( NameSpace = 'MagoNet.Articoli.Etichette2x8' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.CreditNotesList'
WHERE( NameSpace = 'MagoNet.Acquisti.ListaNoteDiCreditoRicevute' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PickingList-QtyToPickList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoQuantitaDaPrelevareSuBdP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ItemSheet'
WHERE( NameSpace = 'MagoNet.DistintaBase.SchedaArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheet-FinStat-Ledger'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioStatoPatrimonialeRiepilogativo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.NCReceiptsPortfolioTot'
WHERE( NameSpace = 'MagoNet.Vendite.TotaliPortafoglioClientiRicevuteFiscaliNonIncassate' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.SubcontractorOrderList'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ElencoOrdiniAlFornitorePerLavorazioneEsterna' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.VouchersSlip'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.DistintaReversali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrdersList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoOrdiniDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.AcquiredNotAccruedEntries'
WHERE( NameSpace = 'MagoNet.Agenti.MovimentiAcquisitiNonMaturati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesJournal-EUAnnotations'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVAVenditeAcquistiCEE' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.ProRata'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.ProRataCalcolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.PriceListsReview'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.SpuntaListini' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.TangibleFA-TechnicalData'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiMaterialiDatiTecnici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriRiepGrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.JobsGroups'
WHERE( (NameSpace = 'MagoNet.Analitica.CommesseGruppi' OR NameSpace = 'MagoNet.Commesse.CommesseGruppi') AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsByCostCenter'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiPerCentro' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.ALAnalysis-Grouped'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiPatrimonialiSintetico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentMonthly-Det-Grp-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriDettGrpSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.PrivacyStatement'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.consenso' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.EntriesByLot'
WHERE( NameSpace = 'MagoNet.LottiMatricole.MovimentiPerLotto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FinancialByFiscalYear'
WHERE( NameSpace = 'MagoNet.Cespiti.FinanziarioPerEsercizio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseRiep' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.InventoryReasonsSheet-SecondPage'
WHERE( NameSpace = 'MagoNet.Magazzino.CausaliDiMagazzino2' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchasesExigibleTax'
WHERE( NameSpace = 'MagoNet.Contabilita.IvaEsigibileAcquisti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReceiptsDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.EliminaRicevutaFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.EUBalSheet-FinStat-Casc'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimonialeScalare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ProducibilityAnalysis'
WHERE( NameSpace = 'MagoNet.DistintaBase.AnalisiProducibilita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.ProRata-Detailed'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.ProRataDettaglio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.DifferentiatedBOMImplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.ImplosioneDistintaBaseScalare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.CashOrderSlip'
WHERE( NameSpace = 'MagoNet.Partite.DistintaRIBA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccInvoiceForm-MoreBranches'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoFatturaAccompagnatoriaPiuSedi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.BreakdownReasonsList'
WHERE( NameSpace = 'MagoNet.Cicli.ElencoCausaliND' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.ComponentsVariantsSheet'
WHERE( NameSpace = 'MagoNet.Varianti.SchedaVariantiComponenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ItemsBySupplier'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliPerFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_PL.PurchasesJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_pl.PurchasesRegister' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.ItemsSuppliersDelivered'
WHERE( NameSpace = 'MagoNet.Acquisti.ConsegnatoFornitoriArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Buffetti8903FForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoBuffettiFatturaAccompagnatoria8903F' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.OrderedByItemAndCustomer'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdinatoPerArticoloECliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersActual-Det-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.consuntivocentrigrpdett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Grp-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseRiepGrpSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.EntriesByInvoice'
WHERE( NameSpace = 'MagoNet.Agenti.MovimentiPerFattura' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Producers'
WHERE( NameSpace = 'MagoNet.Articoli.Produttori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.ItemSheetByStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.SchedaArticoloPerDeposito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FAJournal-Detailed-Sim'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroSimulato' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxGeneralSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoGeneraleRegistriIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.ReceivablesDeleting'
WHERE( NameSpace = 'MagoNet.Partite.PartiteClienteDaEliminare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.SuppAccStatementsByPymtSched'
WHERE( NameSpace = 'MagoNet.Partite.EstrattiContoFornitoriPerPartita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.ItemsByPriceList'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.SpuntaArticoliPerListino' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsYearly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetEsContiCommesse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.VouchersToBePresented'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.ReversaliPresentabili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.ShortMediumTermReceivables'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.CreditiMedioTermine' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.EUBalSheet-AbbFinStat-Casc'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimonialeAbbreviatoScalare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.TaxPlafond'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.PlafondIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsInEntriesByCustomer'
WHERE( NameSpace = 'MagoNet.Magazzino.ArticoliMovimentatiPerCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.SuppliersByCommodityCtg'
WHERE( NameSpace = 'MagoNet.Articoli.FornitoriPerCategorieMerceologiche' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.KEPYOCover'
WHERE( NameSpace = 'MagoNet.Contabilita_gr.KepyoCover' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.PurchasedBySuppliersTot'
WHERE( NameSpace = 'MagoNet.Acquisti.TotaliAcquistatoFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.SuppliersAddressBook'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.RubricaFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.FIRRAccountStatements'
WHERE( NameSpace = 'MagoNet.Agenti.EstrattiContoFIRR' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.SuppliersCategories'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.CategorieFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsSheets-FullCosting'
WHERE( NameSpace = 'MagoNet.Analitica.CommesseSchedeFull' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ItemsPurchaseBarCodes'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliBarCodeAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.AnnualIntraDispatches2B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniTerAnnuale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.InvoiceForm-PL'
WHERE( NameSpace = 'MagoNet.Vendite.FakturaVAT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ManufacturingDifferentiatedBOMExplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.EsplosioneDistintaBaseScalareProd' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrders'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniDaCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByStorageAndCust-Item'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerDepositoClienteEArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsFullCosting'
WHERE( NameSpace = 'MagoNet.Analitica.CommesseFullCost' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.CategorySheet'
WHERE( NameSpace = 'MagoNet.Cespiti.SchedaFiscaleCategoria' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FAInBalanceSheetNotes-Bal'
WHERE( NameSpace = 'MagoNet.Cespiti.NotaIntegrativaBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.InvoicesPortfolio'
WHERE( NameSpace = 'MagoNet.Acquisti.PortafoglioFornitoriFattureAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheet-PL-ByLedger'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioContoEconomicoRiepilogativo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Buffetti8902D3Form'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoBuffettiDDT8902D3' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.ItemVariantsList'
WHERE( NameSpace = 'MagoNet.Varianti.ElencoVariantiperArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchJournalByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVAAcquistiPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.Voucher'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.Reversale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsOn-Hand'
WHERE( NameSpace = 'MagoNet.Magazzino.DisponibilitaArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.SupplierQuotations'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.ListaOfferteDaFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.JobsDeliveriesDelay'
WHERE( (NameSpace = 'MagoNet.Analitica.CommesseRitardi' OR NameSpace = 'MagoNet.Commesse.CommesseRitardi') AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrdersToFulfill'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.EvadibilitaPerCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.PackingProducersDeclarations'
WHERE( NameSpace = 'MagoNet.Conai.DichiarazioneProduttoriImballaggi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrdersReview'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.SpuntaOrdiniCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.MOCRPDatesEstimate'
WHERE( NameSpace = 'MagoNet.CRP.PrevisioneDateODPCRP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.CustSuppBalanceSheet'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioCliFor' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsNotInEntriesSince'
WHERE( NameSpace = 'MagoNet.Magazzino.ArticoliNonMovimentatiDal' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ReorderMaterialsToSupplier'
WHERE( NameSpace = 'MagoNet.DistintaBase.RiordinoAFornitoreMaterialiMancanti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsSheets'
WHERE( NameSpace = 'MagoNet.Analitica.ContiSchede' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.AllowanceAccountStatements'
WHERE( NameSpace = 'MagoNet.Agenti.EstrattiContoIndennita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsJournal-Detailed'
WHERE( NameSpace = 'MagoNet.Cespiti.Registro' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.MonthlyIntraArrivals1A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiBisMensile' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesBySupplier'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.RetSalesJournByPostDate'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporoPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersBudget-Det-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.preventivocentrigrpdett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.IncompatQuestionsAnswers'
WHERE( NameSpace = 'MagoNet.Configuratore.IncompDomRisp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.MonthlyIntraArrivals1B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiTerMensile' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Ports'
WHERE( NameSpace = 'MagoNet.Spedizioni.Porti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.IntangibleFA-TechnicalData'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiImmaterialiDatiTecnici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsDirectCosting'
WHERE( NameSpace = 'MagoNet.Analitica.CommesseDirectCost' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.InvoicesPosting'
WHERE( NameSpace = 'MagoNet.Acquisti.RegistraFatturaAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ProcessingMaterialsList'
WHERE( NameSpace = 'MagoNet.Produzione.MaterialiInLavorazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.InventoryReasons'
WHERE( NameSpace = 'MagoNet.Magazzino.ListaCausaliDiMagazzino' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersBal'
WHERE( NameSpace = 'MagoNet.Analitica.CentriSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsScrapQty'
WHERE( NameSpace = 'MagoNet.Magazzino.QuantitaArticoliScarti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.SuppliersItems'
WHERE( NameSpace = 'MagoNet.Acquisti.ProspettoArticoliFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheetComparative'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioContrappostoComparato' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.EUAnnJournalNumCheckByDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVACEEPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.StubBooks'
WHERE( NameSpace = 'MagoNet.IdsMng.Bollettari' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchasesNotExigibleTax'
WHERE( NameSpace = 'MagoNet.Contabilita.IvaInesigibileAcquisti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxData'
WHERE( NameSpace = 'MagoNet.Contabilita.DatiIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.JobTickets-InventoryEntries'
WHERE( NameSpace = 'MagoNet.Produzione.MovimentiDiMagazzinoDaBollaDiLavorazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.ReclassificationSchemas'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.SchemiDiRiclassificazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrdersReview'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.SpuntaCompletaOrdiniAFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Packages'
WHERE( NameSpace = 'MagoNet.Spedizioni.Imballi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.ParentMOList'
WHERE( NameSpace = 'MagoNet.MRP.ElencoOrdiniProduzionePadre' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.PLAnalysis-Grouped'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiEconomiciSintetico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.CustomersTaxIdNoCheck'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ControlloPartitaIVAClienti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersQtyBalances'
WHERE( NameSpace = 'MagoNet.Analitica.CentriSaldiQta' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.HomogeneousCategories'
WHERE( NameSpace = 'MagoNet.Articoli.CategorieOmogenee' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.SummarizedBOMExplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.EsplosioneDistintaBaseRiepilogativa' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.SerialNosLabels'
WHERE( NameSpace = 'MagoNet.Acquisti.EtichetteMatricole' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.EntriesByItem'
WHERE( NameSpace = 'MagoNet.Analitica.MovimentiPerArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.CompanyBanks'
WHERE( NameSpace = 'MagoNet.Banche.BancheAzienda' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.AnnualIntraArrivals1B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraAcquistiTerAnnuale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.EUBalSheet-FinStat'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimoniale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchasesSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoRegistroIVAAcquisti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.PurchasedByItemsTot'
WHERE( NameSpace = 'MagoNet.Acquisti.TotaliAcquistatoArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.PurchasedBySuppliers'
WHERE( NameSpace = 'MagoNet.Acquisti.ProspettoAcquistatoFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.AvailabilityForecast'
WHERE( NameSpace = 'MagoNet.MRP.PrevisioneAndamentoGiacenze' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.OrderComponentsToPickList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoMaterialiDaPrelevare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.TurnoverByCustAndItem-Det'
WHERE( NameSpace = 'MagoNet.Vendite.FatturatoClienteArticoloSuRighe' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccompanyingInvoicesPosting'
WHERE( NameSpace = 'MagoNet.Vendite.StampaRegistraFatturaAccompagnatoria' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.OutstandingBills'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.EffettiInsoluti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Currencies'
WHERE( NameSpace = 'MagoNet.Divise.Divise' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.EntriesByReason'
WHERE( NameSpace = 'MagoNet.Magazzino.MovimentiPerCausale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.InventoryValuation'
WHERE( NameSpace = 'MagoNet.Magazzino.ACosti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetSheet'
WHERE( NameSpace = 'MagoNet.Cespiti.SchedaFiscaleCespite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Non-CollectedReceiptsPosting'
WHERE( NameSpace = 'MagoNet.Vendite.StampaRegistraRicevutaFiscaleNI' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.SuppliersSheet'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.SchedaFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Simulation'
WHERE( NameSpace = 'MagoNet.Cespiti.SimulazioneFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SuppliersCardsByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.FornitoriPerRegistrazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.InventoryJournal'
WHERE( NameSpace = 'MagoNet.Magazzino.GiornaleDiMagazzino' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.INPSAccountStatements'
WHERE( NameSpace = 'MagoNet.Percipienti.EstrattiContoINPS' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DeferredInvoicing'
WHERE( NameSpace = 'MagoNet.Vendite.FatturazioneDifferita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.EntriesByDate'
WHERE( NameSpace = 'MagoNet.Cespiti.MovimentiPerData' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsQtyBal'
WHERE( NameSpace = 'MagoNet.Analitica.CommesseSaldiQta' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.SoldPurchasedItems'
WHERE( NameSpace = 'MagoNet.Magazzino.ProspettoVendutoAcquistatoArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.ItemsToOrderBySuppliers'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ListaArticoliDaOrdinare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.SalesPeopleMaster'
WHERE( NameSpace = 'MagoNet.Agenti.AnagraficaAgenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.IncompleteJournalEntries'
WHERE( NameSpace = 'MagoNet.Contabilita.PrimeNoteZoppe' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.SmartCodeRootsSheet'
WHERE( NameSpace = 'MagoNet.CodiceParlante.SchedaRadice' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.TurnoverByCustomer-Detailed'
WHERE( NameSpace = 'MagoNet.Agenti.FatturatoAgentePerClienteConDettaglioDocumenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.PriceListsByItems'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.SpuntaListiniPerArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.PurchaseTaxByLog'
WHERE( NameSpace = 'MagoNet.Contabilita.IVAAcquistiperProtocollo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrdToRequestDelivery'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.SollecitiOrdiniAFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AccountStatement'
WHERE( NameSpace = 'MagoNet.Contabilita.EstrattoConto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.TrialBalanceCover'
WHERE( NameSpace = 'MagoNet.Contabilita_gr.TrialCover' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.CommodityCategories'
WHERE( NameSpace = 'MagoNet.Articoli.CategorieMerceologiche' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.DisabledJobsInTpl'
WHERE( NameSpace = 'MagoNet.Analitica.TestCommesseDisModelli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Sales'
WHERE( NameSpace = 'MagoNet.Cespiti.Vendite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.ENASARCOPostalPaymentSlip'
WHERE( NameSpace = 'MagoNet.Agenti.BollettinoEnasarco' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.CustAccStatementsBySalespers'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.EstrattiContoClientiPerAgente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.PrivacyStatementLabels'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.EtiConsenso' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.EntriesSheet'
WHERE( NameSpace = 'MagoNet.Magazzino.SchedaMovimentiMagazzino' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesByLog'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabileAcquistiPerProtocollo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.ENASARCOPaymentSlip'
WHERE( NameSpace = 'MagoNet.Agenti.DistintaEnasarco' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Disposals'
WHERE( NameSpace = 'MagoNet.Cespiti.Dismissioni' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PymtsScheduleByBank'
WHERE( NameSpace = 'MagoNet.Partite.ScadenzarioPagamentiBanca' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.TeamsList'
WHERE( NameSpace = 'MagoNet.Cicli.ElencoSquadre' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.ContactsTaxIdNoCheck'
WHERE( NameSpace = 'MagoNet.Contatti.ControlloPartitaIVAContatti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.MRPWarningsMessages'
WHERE( NameSpace = 'MagoNet.MRP.SegnalazioniMRP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.TurnoverByCustomer-Detailed'
WHERE( NameSpace = 'MagoNet.Vendite.FatturatoSinteticoClienteSuRighe' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.ShortMediumTermPyblsBySupp'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.DebitiMedioTerminePerFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReceiptsPortfolioTot'
WHERE( NameSpace = 'MagoNet.Vendite.TotaliPortafoglioClientiRicevuteFiscali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostAccAccounts'
WHERE( NameSpace = 'MagoNet.Analitica.ContiAnalitici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrder'
WHERE( NameSpace = 'MagoNet.Produzione.OrdineDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesJournalByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVAVenditePerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.RoutingVariantsList'
WHERE( NameSpace = 'MagoNet.Varianti.ElencoVariantiCicli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccInvoicesPortfolioTot'
WHERE( NameSpace = 'MagoNet.Vendite.TotaliPortafoglioClientiFattureAccompagnatorie' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsActual'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoCommesse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FAInInventory-Balance'
WHERE( NameSpace = 'MagoNet.Cespiti.InventarioBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.DunnedCustomers'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.ClientiSollecitati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsWithNegBookInvValues'
WHERE( NameSpace = 'MagoNet.Magazzino.ArticoliConGiacenzeNegative' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PresentedBills'
WHERE( NameSpace = 'MagoNet.Partite.EffettiPresentati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersByCode'
WHERE( NameSpace = 'MagoNet.Analitica.CentriPerCodice' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ProductCategories'
WHERE( NameSpace = 'MagoNet.Articoli.CategorieProdotto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.ItemSheetWithStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.SchedaArticoloConVisioneDeposito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.BalanceSales'
WHERE( NameSpace = 'MagoNet.Cespiti.VenditeBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersGroups'
WHERE( NameSpace = 'MagoNet.Analitica.CentriGruppi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CreditNotes'
WHERE( NameSpace = 'MagoNet.Vendite.ListaNoteDiCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersBudget-Det'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoCentriDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.MOJobTicketDetailedSheet'
WHERE( NameSpace = 'MagoNet.Produzione.DettaglioAnagraficoBollaDiLavorazionePerOdP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.BillsOfLadingDeleting'
WHERE( NameSpace = 'MagoNet.Acquisti.EliminaBollaCarico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.ISOCountryCodes'
WHERE( NameSpace = 'MagoNet.Azienda.ISOStati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.JobsDeliveries'
WHERE( (NameSpace = 'MagoNet.Analitica.CommessePerConsegna' OR NameSpace = 'MagoNet.Commesse.CommessePerConsegna') AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.TurnoverByAreaAndCustomer'
WHERE( NameSpace = 'MagoNet.Agenti.FatturatoAgentePerAreaECliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Classes'
WHERE( NameSpace = 'MagoNet.Cespiti.Classi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BillsToBePresentedByCustomer'
WHERE( NameSpace = 'MagoNet.Partite.EffettiPresentabiliCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.VariantsList'
WHERE( NameSpace = 'MagoNet.Varianti.ElencoVarianti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.FeesSheet'
WHERE( NameSpace = 'MagoNet.Percipienti.SchedeParcelle' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrderExplosion'
WHERE( NameSpace = 'MagoNet.Produzione.EsplosioneOdP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Scraps'
WHERE( NameSpace = 'MagoNet.Cespiti.Eliminazioni' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.CustSuppBanks'
WHERE( NameSpace = 'MagoNet.Banche.BancheCliFor' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Grp-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseRiepGrpProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersYearly-Det'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetEsCentriDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccompanyingInvoicesReview'
WHERE( NameSpace = 'MagoNet.Vendite.SpuntaCompletaFattureAccompagnatorie' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.EUBalSheet-AbbFinStat'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimonialeAbbreviato' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.SuppliersByItem'
WHERE( NameSpace = 'MagoNet.Articoli.FornitoriPerArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Det-Grp-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseDettGrpSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.SuppliersTaxIdNoCheck'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ControlloPartitaIVAFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.ProspSuppAddressBook'
WHERE( NameSpace = 'MagoNet.Contatti.RubricaFornitoriPotenziali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsYearly-Det'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetEsCommesseDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.CustomerQuotationForm'
WHERE( NameSpace = 'MagoNet.OfferteClienti.FincatoOffertaCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrdersSchedule-Items'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.ScadenzarioOrdiniDettaglioArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrders'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ListaOrdiniAFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.AccInvoicesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.EliminaFatturaAccompagnatoria' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.CustomerQuotations'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteACliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.ItemsSuppliersLabel'
WHERE( NameSpace = 'MagoNet.Articoli.EtichetteFornitoriArticoli2x8' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.ProductionPlanGeneration'
WHERE( NameSpace = 'MagoNet.DistintaBase.GenerazionePianoProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.PriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.Listini' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.ChartOfAccounts'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoConti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentersBudget'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoContiCentri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PapersPrintDetailedStepJobTicketsList'
WHERE( NameSpace = 'MagoNet.Produzione.StampaDocElencoBolleDiLavorazioneConDettaglioFasi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BalanceSheet-PL-Detailed'
WHERE( NameSpace = 'MagoNet.Contabilita.BilancioContoEconomicoAnalitico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.LastPurchaseData'
WHERE( NameSpace = 'MagoNet.Articoli.DatiUltimoAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.SuppPymtScheduleByName'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.ScadenzarioFornitoriPerRagioneSociale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentersYearly'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetEsContiCentri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByStorageAndSupp'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerDepositoEFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.FeesToBePaid'
WHERE( NameSpace = 'MagoNet.Percipienti.ParcelleDaPagare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Languages'
WHERE( NameSpace = 'MagoNet.Lingue.Lingue' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.OrderedByItem'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ProspettoOrdinatoArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReceiptsPosting'
WHERE( NameSpace = 'MagoNet.Vendite.StampaRegistraRicevutaFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FAInBalanceSheetNotes'
WHERE( NameSpace = 'MagoNet.Cespiti.NotaIntegrativaFiscale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.InvoiceForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoFatturaImmediata' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.DetailedCosting'
WHERE( NameSpace = 'MagoNet.DistintaBase.CostificazioneAnalitica' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.IntangibleFA-PurchaseData'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiImmaterialiDatiAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DDTPackingListForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoDDTPackingList' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.EntriesByMaterialsAndPackage'
WHERE( NameSpace = 'MagoNet.Conai.MovimentiPerMaterialeTipologia' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Det'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PaymentOrdersToBeIssued'
WHERE( NameSpace = 'MagoNet.Partite.MandatiEmettibili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesSummaryJournal-EUAnnotations'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoRegistroIVACEE' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.MOFeasabilityAnalysis'
WHERE( NameSpace = 'MagoNet.MRP.AnalisiOrdiniProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PymtOrdersToBeIssuedToSupp'
WHERE( NameSpace = 'MagoNet.Partite.MandatiEmettibiliFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsSheets'
WHERE( NameSpace = 'MagoNet.Cespiti.SchedeCespiti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.UoMs'
WHERE( NameSpace = 'MagoNet.Articoli.UnitaDiMisura' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.AvailabilityAnalysis'
WHERE( NameSpace = 'MagoNet.Produzione.ProspettoDisponibilita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.DNForm'
WHERE( NameSpace = 'MagoNet.Vendite.FincatoDDT' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.CreditNotesDeleting'
WHERE( NameSpace = 'MagoNet.Vendite.EliminaNotaCredito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsCostCentMonthly-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseContiCentriSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersBudget'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoCentri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BillsPymtScheduleByBank'
WHERE( NameSpace = 'MagoNet.Partite.ScadenzarioEffettiBanca' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.WithholdingTaxesReview'
WHERE( NameSpace = 'MagoNet.Contabilita_it.SpuntaRitenute' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesTax'
WHERE( NameSpace = 'MagoNet.Contabilita.SpuntaPrimeNoteIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Entries'
WHERE( NameSpace = 'MagoNet.Analitica.MovimentiTeste' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.SuppliersItems'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ProspettoArticoliFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.SuppliersLedgerCards'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.LedgerCardsSuppliers' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.RetSalesDailyJournByPostDate'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporoGionalieroPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersActual'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoCentri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsBudget'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoContiCommesse' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.SimulationByLifePeriod'
WHERE( NameSpace = 'MagoNet.Cespiti.SimulazioneFiscaleDurata' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PickingList-PickedQtyList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoRigheDiPrelievoSuBdP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.SalesPeopleBalances'
WHERE( NameSpace = 'MagoNet.Agenti.SaldiAgenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.SaleOrdersPortfolioTot'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.TotaliPortafoglioOrdini' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.ENASARCOFromFees'
WHERE( NameSpace = 'MagoNet.Agenti.EnasarcoDaParcelle' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.INPSPymtSchedule'
WHERE( NameSpace = 'MagoNet.Percipienti.ScadenzarioINPS' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.PackageLabels'
WHERE( NameSpace = 'MagoNet.Vendite.EtichetteSovrapacco2x8' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Ledgers'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.Mastri' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReceiptsPortfolio'
WHERE( NameSpace = 'MagoNet.Vendite.PortafoglioClientiRicevuteFiscali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PayablesDeleting'
WHERE( NameSpace = 'MagoNet.Partite.PartiteFornitoreDaEliminare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.AccountsJobsBal'
WHERE( NameSpace = 'MagoNet.Analitica.ContiCommesseSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsMarkup'
WHERE( NameSpace = 'MagoNet.Magazzino.RicarichiArticoloSintesi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PapersPrintJobTicketSheet'
WHERE( NameSpace = 'MagoNet.Produzione.StampaDocSchedaBollaDiLavorazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Grp-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriRiepGrpProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.OrderedByCustomerAndItem'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdinatoPerClienteEArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.ConsolidationTemplates'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.ModelliConsolidamento' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrdersSchedule-Det'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.ScadenzarioRigheOrdiniAFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.BillsOfLadingPosting'
WHERE( NameSpace = 'MagoNet.Acquisti.RegistraBollaCarico' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesPureHeaders'
WHERE( NameSpace = 'MagoNet.Contabilita.TesteContabiliPure' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.ReceiptLabelsWithBarCodes'
WHERE( NameSpace = 'MagoNet.Acquisti.EtichetteDiCaricoConCodiciABarre' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.FIFOData'
WHERE( NameSpace = 'MagoNet.Magazzino.DatiFIFO' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.FeesBySupplier'
WHERE( NameSpace = 'MagoNet.Percipienti.ParcellePerFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.CashFlowReceivables'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.CashFlowPartiteCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.WCSheet'
WHERE( NameSpace = 'MagoNet.Cicli.SchedaCentriDiLavoro' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.RetailSalesSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoRegistroIVACorrispettiviScorporo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsBudget-Det-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoCommesseGrpDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Det-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriDettProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.CustomersAddressBook'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.RubricaClienti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.MonthlyIntraDispatches2A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniBisMensile' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Checks'
WHERE( NameSpace = 'MagoNet.Partite.AssegniBancari' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.EntriesByAccount'
WHERE( NameSpace = 'MagoNet.Analitica.MovimentiPerConto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MulticompanyBalances.MulticompanyBalances'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.SaleOrdersSchedule'
WHERE( NameSpace = 'MagoNet.Produzione.ScadenziarioRigheOrdiniDaCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepilogoRegistroIVAVendite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntries'
WHERE( NameSpace = 'MagoNet.Contabilita.SpuntaPrimeNote' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.DifferentiatedBOMExplosion'
WHERE( NameSpace = 'MagoNet.DistintaBase.EsplosioneDistintaBaseScalare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.EntriesByPostingDate'
WHERE( NameSpace = 'MagoNet.Analitica.MovimentiPerDataReg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PaymentOrdersSlip'
WHERE( NameSpace = 'MagoNet.Partite.DistintaMandati' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.CustomersCardsByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ClientiPerRegistrazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.PaymentOrder'
WHERE( NameSpace = 'MagoNet.Partite.Mandato' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_IT.F24Form'
WHERE( NameSpace = 'MagoNet.Contabilita_it.ModelloF242003' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseRiepSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.FullCosting'
WHERE( NameSpace = 'MagoNet.Analitica.FullCosting' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.CustAccStatementByPymtSched'
WHERE( NameSpace = 'MagoNet.Partite.EstrattiContoClientiPerPartita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsBudget-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.PreventivoCommesseGrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersTemplates'
WHERE( NameSpace = 'MagoNet.Analitica.CentriModelliAnalitici' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.PLAnalysis'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiEconomiciRiepilogativo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AccountingDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita.ContiDiDefault' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsJournal-Bal'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroRaggruppatoBilancio' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersBudget-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.preventivocentrigrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Receivables'
WHERE( NameSpace = 'MagoNet.Partite.PartiteCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.JournalEntriesByPostingDate'
WHERE( NameSpace = 'MagoNet.Contabilita.PrimeNotePerRegistrazione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersByType'
WHERE( NameSpace = 'MagoNet.Analitica.CentriPerTipo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Form770Form'
WHERE( NameSpace = 'MagoNet.Percipienti.Lista770' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.CustomersLabels'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.EtichetteClienti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.IntraDispatches2A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntraCessioniBis' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.PurchasedByItems'
WHERE( NameSpace = 'MagoNet.Acquisti.ProspettoAcquistatoArticoli' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.SupplierPymtSchedule'
WHERE( NameSpace = 'MagoNet.Partite.ScadenzarioFornitoriPerDataScadenza' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsActual-Det-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.ConsuntivoCommesseGrpDett' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.SalesJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.SalesJournal' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersMonthly-Bal'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCentriRiepSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.TangibleFixedAssetsLabels'
WHERE( NameSpace = 'MagoNet.Cespiti.EtichetteCespitiMateriali' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsBal'
WHERE( NameSpace = 'MagoNet.Analitica.CommesseSaldi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.WCJobTicketDetailedSheet'
WHERE( NameSpace = 'MagoNet.Produzione.DettaglioAnagraficoBollaDiLavorazionePerCdL' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.InvoicesDeleting'
WHERE( NameSpace = 'MagoNet.Acquisti.EliminaFatturaAcquisto' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-Grp'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseRiepGrp' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AccountingTemplates'
WHERE( NameSpace = 'MagoNet.Contabilita.SpuntaModelliContabili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.MOProductionProgress'
WHERE( NameSpace = 'MagoNet.CRP.StatoAvanzamentoODP' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesBySuppAndStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerFornitoreEDeposito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.DetailedStepJobTicketsList'
WHERE( NameSpace = 'MagoNet.Produzione.ElencoBolleDiLavorazioneConDettaglioFasi' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReturnsFromCustomer'
WHERE( NameSpace = 'MagoNet.Vendite.ListaResiDaCliente' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.BillsPymtSchedule'
WHERE( NameSpace = 'MagoNet.Partite.ScadenzarioEffetti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.FixedAssetsJournal'
WHERE( NameSpace = 'MagoNet.Cespiti.RegistroRaggruppato' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesNotExigibleTax'
WHERE( NameSpace = 'MagoNet.Contabilita.IvaInesigibileVendite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.ENASARCOFromBalances-Det'
WHERE( NameSpace = 'MagoNet.Agenti.EnasarcoDettDaSaldiAgenti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.PurchaseOrdersPortfolio'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.PortafoglioOrdiniAFornitori' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.AccountingSimulations'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.SimulazioniContabili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.TaxSummaryJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RiepiloghiRegistroIVA' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.CostCentersByNature'
WHERE( NameSpace = 'MagoNet.Analitica.CentriPerNatura' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.IntraItems'
WHERE( NameSpace = 'MagoNet.Intrastat.ArticoliIntra' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ManufacturingOrderSheet'
WHERE( NameSpace = 'MagoNet.Produzione.SchedaOrdiniDiProduzione' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.PickingListSheet'
WHERE( NameSpace = 'MagoNet.Produzione.StampaBuonoDiPrelievo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.JobCostAnalysis'
WHERE( NameSpace = 'MagoNet.Produzione.AnalisiCostiCommessa' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.LabelsWithBarCodes'
WHERE( NameSpace = 'MagoNet.Articoli.EtichetteConCodiciABarreDiAcquistoOVendita' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesExigibleTax'
WHERE( NameSpace = 'MagoNet.Contabilita.IvaEsigibileVendite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.EntriesByCustAndStorage-Item'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MovimentiPerClienteDepositoEArticolo' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Non-RegFAInEntries'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiMovimentatiNonCensiti' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.ItemsShortage'
WHERE( NameSpace = 'MagoNet.Magazzino.ArticoliSottoscorta' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.ReturnsToSupplier'
WHERE( NameSpace = 'MagoNet.Vendite.ListaResiAFornitore' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.SalesCollectionJournal'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.SalesCollectionSummaryJournal' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR_Plus.ShortMediumTermPayables'
WHERE( NameSpace = 'MagoNet.PartiteAvanzato.DebitiMedioTermine' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.ProductionDevelopment-UnifiedOrderComponents'
WHERE( NameSpace = 'MagoNet.Produzione.AvanzamentoProduzioneAccorpatoPerMateriale' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.JobsMonthly-YTD'
WHERE( NameSpace = 'MagoNet.Analitica.BudgetMeseCommesseRiepProg' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.SalesJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVAVendite' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.AccountsLedgerCards'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.LedgerCardsAccounts' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.EntriesByLotAndStorage'
WHERE( NameSpace = 'MagoNet.LottiMatricole.MovimentiPerLottoDeposito' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.AccountingReasons'
WHERE( NameSpace = 'MagoNet.Contabilita.CausaliContabili' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.WHoldingTaxToBePaid'
WHERE( NameSpace = 'MagoNet.Percipienti.RitenuteDaVersare' AND 
TypeId = @reportTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.RetailSalesToBeDistJournal'
WHERE( NameSpace = 'MagoNet.Contabilita.RegistroIVACorrispettiviVentilazione' AND 
TypeId = @reportTypeId
)
END
GO