
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_ScheduledTasks]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.InvoicesPortfolio'
WHERE( Command = 'MagoNet.Vendite.PortafoglioClientiFattureImmediate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.RetailSalesToBeDistSummJourn'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoRegistroIVACorrispettiviVentilazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.SummarizedCosting'
WHERE( Command = 'MagoNet.DistintaBase.CostificazioneSintetica' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.OrderedByItemTot'
WHERE( Command = 'MagoNet.OrdiniFornitori.TotaliOrdinatoArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.EUTaxJournalByPostingDate'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVACEEPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Det-Grp'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriDettGrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BankDraftsRequest'
WHERE( Command = 'MagoNet.Partite.RichiestaAssegniCircolari' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CreditNotesReview'
WHERE( Command = 'MagoNet.Vendite.SpuntaCompletaNoteDiCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.DistributionSpread'
WHERE( Command = 'MagoNet.Contabilita.RipartoVentilazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Non-DistribAccountsInEntries'
WHERE( Command = 'MagoNet.Analitica.TestContiNonRipartiti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.CustomersByCommodityCtg'
WHERE( Command = 'MagoNet.Articoli.ClientiPerCategorieMerceologiche' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.MOToProcessList'
WHERE( Command = 'MagoNet.Produzione.ElencoOrdiniDiProduzioneDaProdurre' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.OrderedByCustomer-Detailed'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdinatoClientePerAnno' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.BillsOfLadingPortfolioTot'
WHERE( Command = 'MagoNet.Acquisti.TotaliPortafoglioFornitoriBolleCarico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.OwnerCompanies'
WHERE( Command = 'MagoNet.BilanciConsolidati.Controllanti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.PrivacyStatementLetter'
WHERE( Command = 'MagoNet.ClientiFornitori.LetteraConsenso' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrdersPortfolioTot'
WHERE( Command = 'MagoNet.OrdiniFornitori.TotaliPortafoglioOrdiniAFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Reasons'
WHERE( Command = 'MagoNet.Cespiti.CausaliMovimento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.CashOrderSlip-OnPaper'
WHERE( Command = 'MagoNet.Partite.DistintaRIBASuCarta' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.IntraArrivals1A'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiBis' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesJournal-EUAnnByPostDate'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVAVenditeAcquistiCEEPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.JournalEntryCheck'
WHERE( Command = 'MagoNet.Percipienti.ControlloParcelle' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsByJob'
WHERE( Command = 'MagoNet.Cespiti.CespitiPerCommessa' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesTaxByLog'
WHERE( Command = 'MagoNet.Contabilita.SpuntaPrimeNoteIVAPerProtocollo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersYearly-Grp'
WHERE( Command = 'MagoNet.Analitica.budgetescentrigrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BillOfExchange'
WHERE( Command = 'MagoNet.Partite.Cambiale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BankTransferRequest'
WHERE( Command = 'MagoNet.Partite.RichiestaBonifici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchaseJournalNumCheck'
WHERE( Command = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAAcquisti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.CustomersCategories'
WHERE( Command = 'MagoNet.ClientiFornitori.CategorieClienti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.ManufacturingItemSheet'
WHERE( Command = 'MagoNet.DistintaBase.SchedaArticoloProd' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrdersSchedule'
WHERE( Command = 'MagoNet.OrdiniClienti.ScadenzarioOrdiniDettaglioTeste' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxExigibility'
WHERE( Command = 'MagoNet.Contabilita.EsigibilitaIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsByGroup'
WHERE( Command = 'MagoNet.Analitica.CommessePerGruppo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ProductionProgrAnalysis-Base'
WHERE( Command = 'MagoNet.Produzione.AnalisiStatoProduzioneBase' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersFullCosting'
WHERE( Command = 'MagoNet.Analitica.CentriFullCost' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.JobTicketsList'
WHERE( Command = 'MagoNet.Produzione.ElencoBolleDiLavorazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.CashOrder'
WHERE( Command = 'MagoNet.Partite.RIBA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.GLJournal'
WHERE( Command = 'MagoNet.Contabilita_ro.GeneralLedger' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.CreditNotesPortfolioTot'
WHERE( Command = 'MagoNet.Acquisti.TotaliPortafoglioFornitoriNoteCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrdersPortfolio'
WHERE( Command = 'MagoNet.OrdiniClienti.PortafoglioOrdini' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SaleJournalNumCheck'
WHERE( Command = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAVendite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.BillsOfLadingPortfolio'
WHERE( Command = 'MagoNet.Acquisti.PortafoglioFornitoriBolleCarico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DeliveryNotesPortfolio'
WHERE( Command = 'MagoNet.Vendite.PortafoglioClientiDDT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Non-RegAccountsInEntries'
WHERE( Command = 'MagoNet.Analitica.TestContiMovimenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AccrualsDeferrals.EntriesToAccrualDeferral'
WHERE( Command = 'MagoNet.RateiRisconti.OperazioniSoggetteARateoRisconto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.ProductionPlanSheet'
WHERE( Command = 'MagoNet.DistintaBase.SchedaPianiDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.SerialNosLabels'
WHERE( Command = 'MagoNet.Magazzino.EtichetteMatricole' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheet-FinStat-Det'
WHERE( Command = 'MagoNet.Contabilita.BilancioStatoPatrimonialeAnalitico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.InterestsOnArrearCalculation'
WHERE( Command = 'MagoNet.Partite.CalcoloInteressiMora' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DeliveryNotes'
WHERE( Command = 'MagoNet.Vendite.ListaDDTBolleAccompagnatorie' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Det-Grp-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseDettGrpProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.SubcntDNMaterialsForm'
WHERE( Command = 'MagoNet.Produzione.StampaDocMaterialiDDT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.CreditNotesDeleting'
WHERE( Command = 'MagoNet.Acquisti.EliminaNotaCreditoRicevuta' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CreditNoteNegSignForm'
WHERE( Command = 'MagoNet.Vendite.FincatoNotaCreditoSegniNegativi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.BalanceByFixedAsset'
WHERE( Command = 'MagoNet.Cespiti.BilancioPerCespite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersDirectCosting'
WHERE( Command = 'MagoNet.Analitica.CentriDirectCost' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Currencies.CurrenciesFixing'
WHERE( Command = 'MagoNet.Divise.DiviseFixing' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesByDocumentData'
WHERE( Command = 'MagoNet.Contabilita.ContabileperDatiDocumento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsSheetByReason'
WHERE( Command = 'MagoNet.Magazzino.SchedaArticoloPerCausale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.TaxJournals'
WHERE( Command = 'MagoNet.IdsMng.RegistriIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.EUTaxJournal'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVACEE' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ForecastAccounting.ForecastTaxSummaryJournal'
WHERE( Command = 'MagoNet.ContabilitaPrevisionale.RiepiloghiIVAPrevisionali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.SupplierAccountStatements'
WHERE( Command = 'MagoNet.Partite.EstrattiContoFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.MO-TimesDeviation'
WHERE( Command = 'MagoNet.Produzione.ScostamentiTempiOrdiniDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentMonthly-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseContiCentriProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.ItemSheetByPostingDate'
WHERE( Command = 'MagoNet.MultiDeposito.SchedaArticoloDiMagazzinoPerDeposito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchasesJournal'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVAAcquisti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentMonthly-Det-Grp-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriDettGrpProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.WHoldingTaxPaymentCertific'
WHERE( Command = 'MagoNet.Percipienti.AttestatiRitenute' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.CalendarScheet'
WHERE( Command = 'MagoNet.Cicli.SchedaCalendario' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Non-RegJobsInEntries'
WHERE( Command = 'MagoNet.Analitica.TestCommesseMovimenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Det'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FinancialSimulation'
WHERE( Command = 'MagoNet.Cespiti.SimulazioneFinanziario' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.FiscalData'
WHERE( Command = 'MagoNet.Magazzino.DatiFiscaliEsercizio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.TangibleFixedAssetsCtg'
WHERE( Command = 'MagoNet.Cespiti.CategorieMateriali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.SubcontratorAnalysis'
WHERE( Command = 'MagoNet.ContoLavoro.AnalisiTerzisti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Non-RegCostCentersInTpl'
WHERE( Command = 'MagoNet.Analitica.TestCentriModelli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.PickingListNumberSheet'
WHERE( Command = 'MagoNet.Produzione.StampaNumeroBuonoDiPrelievo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.ChildMOList'
WHERE( Command = 'MagoNet.MRP.ElencoOrdiniProduzioneFiglio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Currencies.CurrenciesSheet'
WHERE( Command = 'MagoNet.Divise.SchedaDivise' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Non-RegAccountsInEntries'
WHERE( Command = 'MagoNet.Contabilita.ContiMovimentatiNonCensiti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.ItemsToOrder'
WHERE( Command = 'MagoNet.OrdiniFornitori.ProspettoArticoliDaOrdinare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.PaidWHoldingTax'
WHERE( Command = 'MagoNet.Percipienti.RitenuteVersate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.IntraArrivalsCoverSheet'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiFrontespizio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.OperationsList'
WHERE( Command = 'MagoNet.Cicli.ElencoOperazioni' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersActual-Grp'
WHERE( Command = 'MagoNet.Analitica.consuntivocentrigrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsActual-Det'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoCommesseDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesByCustomer'
WHERE( Command = 'MagoNet.Contabilita.ContabilePerCliFor' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Glad'
WHERE( Command = 'MagoNet.Percipienti.Glad' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsSheets-Det'
WHERE( Command = 'MagoNet.Analitica.CommesseSchede' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Languages.PriceListsDescriptions'
WHERE( Command = 'MagoNet.Lingue.DescrizioniListini' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentersJobsBal'
WHERE( Command = 'MagoNet.Analitica.conticentricommessesaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.RetailSalesDailyJournal'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporoGionaliero' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Transport'
WHERE( Command = 'MagoNet.Spedizioni.ModiTrasporto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.TurnoverByItemAndCust-Det'
WHERE( Command = 'MagoNet.Vendite.FatturatoArticoloClienteSuRighe' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Grp-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriRiepGrpSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Configurator.QuestionSheet'
WHERE( Command = 'MagoNet.Configuratore.SchedaDomanda' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.ContactQuotations'
WHERE( Command = 'MagoNet.OfferteClienti.OfferteAClienteContatti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.DDSlip'
WHERE( Command = 'MagoNet.Partite.DistintaRID' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Items'
WHERE( Command = 'MagoNet.Articoli.Articoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.VouchersToBePresentedByCust'
WHERE( Command = 'MagoNet.PartiteAvanzato.ReversaliPresentabiliCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsSheets'
WHERE( Command = 'MagoNet.Analitica.ContiCommesseSchedeRiep' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentersActual'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoContiCentri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.SubcntOrderMaterialsForm'
WHERE( Command = 'MagoNet.Produzione.StampaDocMaterialiOrdFor' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsJournal-History'
WHERE( Command = 'MagoNet.Cespiti.RegistroStorico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BankTransferSlip'
WHERE( Command = 'MagoNet.Partite.DistintaBonifici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.PriceListsByCategories'
WHERE( Command = 'MagoNet.PolitichePrezzi.ListiniPerCategorie' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.CreditNotesPosting'
WHERE( Command = 'MagoNet.Acquisti.RegistraNotaCreditoRicevuta' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.ProRataCalculation'
WHERE( Command = 'MagoNet.Contabilita_ro.ProRataCalculation' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.ShortMediumTermRcvblsByCust'
WHERE( Command = 'MagoNet.PartiteAvanzato.CreditiMedioTerminePerCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CorrectionInvoiceForm'
WHERE( Command = 'MagoNet.Vendite.FincatoFatturaACorrezione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMasterSheet'
WHERE( Command = 'MagoNet.Analitica.CommesseSchedaAnagrafica' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Buffetti8916FForm'
WHERE( Command = 'MagoNet.Vendite.FincatoBuffettiFatturaRicevutaFiscale8916F' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsMonthly-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseContiCommesseSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsBalancing-Balance'
WHERE( Command = 'MagoNet.Cespiti.QuadraturaCespitiBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TrialBalance-ByLedger'
WHERE( Command = 'MagoNet.Contabilita.BilancioRiepilogativo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.ItemsSuppliers'
WHERE( Command = 'MagoNet.Acquisti.ProspettoFornitoriArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxGeneralSummaryJournal-Det'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoGeneraleRegistriIVADettaglio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersActual-Det'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoCentriDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Det-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseDettProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.QuantityToPickForm'
WHERE( Command = 'MagoNet.Produzione.StampaDocElencoQuantitaDaPrelevareSuBdP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Parameters'
WHERE( Command = 'MagoNet.Percipienti.Parametri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.FeeTemplates'
WHERE( Command = 'MagoNet.Percipienti.ModelliParcelle' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.WCLoad'
WHERE( Command = 'MagoNet.CRP.CaricoCentriDiLavoro' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Det-Grp'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseDettGrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.EUBalSheet-Memorandum'
WHERE( Command = 'MagoNet.AnalisiBilancio.BilancioCEEContiOrdine' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsJournal-Bal-Det'
WHERE( Command = 'MagoNet.Cespiti.RegistroBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.JobTicketSheet'
WHERE( Command = 'MagoNet.Produzione.SchedaBolleDiLavorazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.LIFOData'
WHERE( Command = 'MagoNet.Magazzino.DatiLIFO' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.CustQuotationsPortfolio'
WHERE( Command = 'MagoNet.OfferteClienti.PortafoglioOfferte' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.ProposedMO'
WHERE( Command = 'MagoNet.MRP.OrdiniProduzioneProposti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_GR.KEPYO'
WHERE( Command = 'MagoNet.Contabilita_gr.Kepyo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.SubstituteItems'
WHERE( Command = 'MagoNet.Articoli.ArticoliEquivalenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ForecastAccounting.ForecastPurchasesJournal'
WHERE( Command = 'MagoNet.ContabilitaPrevisionale.RegistriIVARicevutiPrevisionali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.CustSuppBalancing'
WHERE( Command = 'MagoNet.Contabilita.QuadraturaCliforPartite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Departments'
WHERE( Command = 'MagoNet.Articoli.Reparti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.ByFixedAsset'
WHERE( Command = 'MagoNet.Cespiti.FiscalePerCespite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.ItemsSuppliersLeadTime'
WHERE( Command = 'MagoNet.Acquisti.LeadTimeFornitoriArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Non-RegJobsInTpl'
WHERE( Command = 'MagoNet.Analitica.TestCommesseModelli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.ProductionPlans'
WHERE( Command = 'MagoNet.DistintaBase.PianiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.TaxCodeAssignment'
WHERE( Command = 'MagoNet.Articoli.AssegnazioneCodIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByStorageAndSupp-Item'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerDepositoFornitoreEArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.WCList'
WHERE( Command = 'MagoNet.Cicli.ElencoCentriDiLavoro' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrdersSchedule'
WHERE( Command = 'MagoNet.OrdiniFornitori.ScadenzarioTesteOrdiniAFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Slips'
WHERE( Command = 'MagoNet.Partite.Distinte' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Non-CollectedReceipts'
WHERE( Command = 'MagoNet.Vendite.RicevuteFiscaliNonIncassate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.QuarterlyIntraDispatches2A'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniBisTrimestrale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.IntraDispatches2B'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniTer' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.CustomersLedgerCards'
WHERE( Command = 'MagoNet.Contabilita_ro.LedgerCardsCustomers' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsByLocation'
WHERE( Command = 'MagoNet.Cespiti.CespitiPerUbicazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Buffetti8904F3Form'
WHERE( Command = 'MagoNet.Vendite.FincatoBuffettiBollaFattura8904F3' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.BillOfLadingForm'
WHERE( Command = 'MagoNet.Acquisti.FincatoBollaCarico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsBudget'
WHERE( Command = 'MagoNet.Analitica.PreventivoCommesse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.OrderedToSupplierTot'
WHERE( Command = 'MagoNet.OrdiniFornitori.TotaliOrdinatoFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByCustomer'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SaleJournalNumCheckByDate'
WHERE( Command = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAVenditePerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.MultipleReclassifiedAccounts'
WHERE( Command = 'MagoNet.AnalisiBilancio.ContiRiclassificatiPiuVolte' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxByDocumentData'
WHERE( Command = 'MagoNet.Contabilita.IVAperDatiDocumento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.RetailSalesJournal'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_PL.SalesJournal'
WHERE( Command = 'MagoNet.Contabilita_pl.SalesRegister' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.ValuationUpToDate'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.ValorizzazioneAllaData' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Non-RegCustSuppInEntries'
WHERE( Command = 'MagoNet.Contabilita.CliForMovimentatiNonCensiti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.SummarizedBOMExplosion-Manuf'
WHERE( Command = 'MagoNet.DistintaBase.EsplosioneDistintaBaseRiepilogativaProd' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchaseJournNumCheckByDate'
WHERE( Command = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVAAcquistiPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrderForm'
WHERE( Command = 'MagoNet.OrdiniClienti.FincatoOrdineCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.EntriesByMaterials'
WHERE( Command = 'MagoNet.Conai.MovimentiPerMateriale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsBudget-Det'
WHERE( Command = 'MagoNet.Analitica.PreventivoCommesseDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.QuarterlyIntraDispatches2B'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniTerTrimestrale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.ENASARCOFIRRPaymentSlip'
WHERE( Command = 'MagoNet.Agenti.DistintaEnasarcoFIRR' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.SmartCodeSegmentsSheet'
WHERE( Command = 'MagoNet.CodiceParlante.SchedaSegmenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Receipts'
WHERE( Command = 'MagoNet.Vendite.RicevuteFiscali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FinanacialByFixedAsset'
WHERE( Command = 'MagoNet.Cespiti.FinanziarioPerCespite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.MonthlyIntraDispatches2B'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniTerMensile' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentersSheets'
WHERE( Command = 'MagoNet.Analitica.ContiCentriSchedeRiep' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheet-FinStat'
WHERE( Command = 'MagoNet.Contabilita.BilancioDiEsercizioStatoPatrimoniale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.TurnoverByItemAndCust'
WHERE( Command = 'MagoNet.Vendite.FatturatoArticoloCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.ENASARCOCertificate'
WHERE( Command = 'MagoNet.Agenti.AttestatiEnasarco' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentersBal'
WHERE( Command = 'MagoNet.Analitica.ContiCentriSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ForecastAccounting.ForecastJournalEntries'
WHERE( Command = 'MagoNet.ContabilitaPrevisionale.SpuntaPrevisionali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.DisabledCostCentersInTpl'
WHERE( Command = 'MagoNet.Analitica.TestCentriDisModelli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DeliveryNotesReview'
WHERE( Command = 'MagoNet.Vendite.SpuntaCompletaDDTBolleAccompagnatorie' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsByCustomer'
WHERE( Command = 'MagoNet.Analitica.CommessePerCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.SalesPeopleAccountStatements'
WHERE( Command = 'MagoNet.Agenti.EstrattiContoEnasarco' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersSheets-FullCost'
WHERE( Command = 'MagoNet.Analitica.CentriSchedeFull' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.TeamsScheet'
WHERE( Command = 'MagoNet.Cicli.SchedaSquadre' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccompanyingInvoices'
WHERE( Command = 'MagoNet.Vendite.ListaFattureAccompagnatorie' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.TurnoverByCustomer'
WHERE( Command = 'MagoNet.Vendite.FatturatoSinteticoCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DeliveryNotesDeleting'
WHERE( Command = 'MagoNet.Vendite.EliminaDDT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.SalePrices'
WHERE( Command = 'MagoNet.PolitichePrezzi.PrezziDiVendita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Banks.BanksBillAccounts'
WHERE( Command = 'MagoNet.Banche.BancheAziendaCCEffetti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Payables'
WHERE( Command = 'MagoNet.Partite.PartiteFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.QuarterlyIntraArrivals1B'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiTerTrimestrale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.RoutingsSheet'
WHERE( Command = 'MagoNet.DistintaBase.SchedaCicli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.BOMList'
WHERE( Command = 'MagoNet.DistintaBase.ElencoDistinte' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JournalEntryCheck'
WHERE( Command = 'MagoNet.Analitica.ControlloPrimeNote' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.GLJournalDraft'
WHERE( Command = 'MagoNet.Contabilita.BrogliaccioLibroGiornale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.ENASARCOFromBalances'
WHERE( Command = 'MagoNet.Agenti.EnasarcoDaSaldiAgenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.SuppQuotationsPortfolio'
WHERE( Command = 'MagoNet.OfferteFornitori.PortafoglioOfferteDaFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.DrawingsList'
WHERE( Command = 'MagoNet.Cicli.ElencoDisegni' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.TangibleFA-PurchaseData'
WHERE( Command = 'MagoNet.Cespiti.CespitiMaterialiDatiAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.QuarterlyIntraArrivals1A'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiBisTrimestrale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.PLAnalysis-Detailed'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiEconomiciAnalitico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.FIFOProgress'
WHERE( Command = 'MagoNet.Magazzino.formazionefifo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.InventoryValuationByType'
WHERE( Command = 'MagoNet.Magazzino.ACostiConSelezioneSuTipo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_IT.TaxReporting-Year2004'
WHERE( Command = 'MagoNet.Contabilita_it.ComunicazioneIVA2004' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersByGroup'
WHERE( Command = 'MagoNet.Analitica.CentriPerGruppo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.InvoicesPortfolioTot'
WHERE( Command = 'MagoNet.Acquisti.TotaliPortafoglioFornitoriFattureAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AccountsCardsByPostingDate'
WHERE( Command = 'MagoNet.Contabilita.ContiPerRegistrazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.StorageGroups'
WHERE( Command = 'MagoNet.MultiDeposito.RaggruppamentiDepositi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsSheet'
WHERE( Command = 'MagoNet.Magazzino.SchedaArticoloDiMagazzino' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.CustSuppEntriesWrongAccount'
WHERE( Command = 'MagoNet.Contabilita.CliForMovimentatiPdCDiverso' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.BillsOfLadingList'
WHERE( Command = 'MagoNet.Acquisti.ListaBolleDiCarico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.Storages'
WHERE( Command = 'MagoNet.MultiDeposito.Depositi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReceiptsDeferredInvoicing'
WHERE( Command = 'MagoNet.Vendite.FatturazioneDifferitaRicevutaFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.EUBalSheet-PL'
WHERE( Command = 'MagoNet.AnalisiBilancio.BilancioCEEContoEconomico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriRiep' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.GoodsMaster'
WHERE( Command = 'MagoNet.Articoli.MerciDatiAnagrafici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PANSlip'
WHERE( Command = 'MagoNet.Partite.DistintaMAV' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.PurchaseRequestList'
WHERE( Command = 'MagoNet.MRP.ElencoRDA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SuppliersCardsByAccrualDate'
WHERE( Command = 'MagoNet.Contabilita.FornitoriPerCompetenza' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.SuppQuotationsPortfolioTot'
WHERE( Command = 'MagoNet.OfferteFornitori.TotaliPortafoglioOfferteDaFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Parameters'
WHERE( Command = 'MagoNet.Cespiti.Parametri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.ContactsAddressBook'
WHERE( Command = 'MagoNet.Contatti.RubricaContatti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.Exceptions'
WHERE( Command = 'MagoNet.Acquisti.ElencoNote' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.ProspSuppQuotations'
WHERE( Command = 'MagoNet.OfferteFornitori.ListaOfferteDaFornitorePotenziale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.CustomerPymtSchedule'
WHERE( Command = 'MagoNet.Partite.ScadenzarioClientiPerDataScadenza' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.DutyCodes'
WHERE( Command = 'MagoNet.Percipienti.CodiciTributo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.SubstituteItemsSheet'
WHERE( Command = 'MagoNet.Produzione.StampaMaterialiEquivalenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.ProspectiveSuppliersSheet'
WHERE( Command = 'MagoNet.Contatti.SchedaFornitoriPotenziali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Parameters'
WHERE( Command = 'MagoNet.Analitica.parametri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsByCategory'
WHERE( Command = 'MagoNet.Cespiti.CespitiPerCategoria' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Company'
WHERE( Command = 'MagoNet.Azienda.Azienda' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxDeclaration'
WHERE( Command = 'MagoNet.Contabilita.DichiarazioneIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostAccTemplates'
WHERE( Command = 'MagoNet.Analitica.ModelliAnalitici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FAJournal-Simulation'
WHERE( Command = 'MagoNet.Cespiti.RegistroSimulatoRaggruppato' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.IntraArrivals1B'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiTer' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.OwnedCompanies'
WHERE( Command = 'MagoNet.BilanciConsolidati.Controllate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesBySuppAndStorage-Item'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerFornitoreDepositoEArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.InvoicesDeleting'
WHERE( Command = 'MagoNet.Vendite.EliminaFatturaImmediata' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.AnnualIntraArrivals1A'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiBisAnnuale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.VouchersSlips'
WHERE( Command = 'MagoNet.PartiteAvanzato.DistinteReversali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.RoutingVariantSheet'
WHERE( Command = 'MagoNet.Varianti.SchedaVariantiCiclo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.PickingListsForm'
WHERE( Command = 'MagoNet.Produzione.StampaDocElencoBuoniDiPrelievo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.SuppliersLabels'
WHERE( Command = 'MagoNet.ClientiFornitori.EtichetteFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.MO-CostsDeviation'
WHERE( Command = 'MagoNet.Produzione.ScostamentoCostiOrdiniDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.OrderedByItem-Detailed'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdinatoArticoloPerPeriodoAnalitico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.LedgerCards'
WHERE( Command = 'MagoNet.Contabilita.SchedeContabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsActual-Grp'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoCommesseGrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.GLJournal'
WHERE( Command = 'MagoNet.Contabilita.Librogiornale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.InvoicesPortfolioTot'
WHERE( Command = 'MagoNet.Vendite.TotaliPortafoglioClientiFattureImmediate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.IssuedPaymentOrders'
WHERE( Command = 'MagoNet.Partite.MandatiEmessi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsByCode'
WHERE( Command = 'MagoNet.Analitica.CommessePerCodice' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SpecialTax.PlafondUse'
WHERE( Command = 'MagoNet.RegimiIvaSpeciali.ProspettoUtilizzoPlafond' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CreditNotesPortfolioTot'
WHERE( Command = 'MagoNet.Vendite.TotaliPortafoglioClientiNoteDiCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Det-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseDettSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.CashFlow'
WHERE( Command = 'MagoNet.PartiteAvanzato.CashFlowGenerale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PaymentTerms.PaymentTerms'
WHERE( Command = 'MagoNet.CondizioniPagamento.CondizioniPagamento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.CustQuotationsPortfolioTot'
WHERE( Command = 'MagoNet.OfferteClienti.TotaliPortafoglioOfferte' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsTemplates'
WHERE( Command = 'MagoNet.Analitica.CommesseModelliAnalitici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CreditNotesPosting'
WHERE( Command = 'MagoNet.Vendite.StampaRegistraNotaCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Maintenance'
WHERE( Command = 'MagoNet.Cespiti.SpeseManutenzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsSheets'
WHERE( Command = 'MagoNet.Analitica.commesseschederiep' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Entries'
WHERE( Command = 'MagoNet.Cespiti.MovimentiAmmortamento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.ByFiscalYear'
WHERE( Command = 'MagoNet.Cespiti.FiscalePerEsercizio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.CustSuppBalSheetByAccount'
WHERE( Command = 'MagoNet.Contabilita.BilancioCliForPerRaggruppamentoContabile' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.CommisionCategories'
WHERE( Command = 'MagoNet.Agenti.CategorieProvvigionali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AccountingTemplatesSheets'
WHERE( Command = 'MagoNet.Contabilita.SchedeModelliContabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.ItemsByStorage'
WHERE( Command = 'MagoNet.MultiDeposito.ArticoliPerDeposito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_GR.TrialBalance'
WHERE( Command = 'MagoNet.Contabilita_gr.TrialBalance' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriRiepProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsBalancing'
WHERE( Command = 'MagoNet.Cespiti.QuadraturaCespitiFiscali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByItemAndStorage'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerArticoloDepositoPeriodo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ItemsBasePrices'
WHERE( Command = 'MagoNet.Articoli.PrezziBaseArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.SubcontractorDeliveryNote'
WHERE( Command = 'MagoNet.ContoLavoro.DDTAlFornitorePerLavorazioneEsterna' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.GoodsHandlingData'
WHERE( Command = 'MagoNet.Magazzino.MerciDatiMovimentazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Non-CollectedReceiptsReview'
WHERE( Command = 'MagoNet.Vendite.SpuntaCompletaRicevuteFiscaliNonIncassate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ItemsLabels'
WHERE( Command = 'MagoNet.Articoli.EtichetteArticolo2x8' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ProductionProgressAnalysis'
WHERE( Command = 'MagoNet.Produzione.AnalisiStatoProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.InventoryReasonsSheet'
WHERE( Command = 'MagoNet.Magazzino.CausaliDiMagazzino' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.IntangibleFixedAssetsCtg'
WHERE( Command = 'MagoNet.Cespiti.CategorieImmateriali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsByClass'
WHERE( Command = 'MagoNet.Cespiti.CespitiPerClasse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ReorderMaterialsToProduction'
WHERE( Command = 'MagoNet.Produzione.RiordinoProduzioneMaterialiMancanti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsInABCAnalysis'
WHERE( Command = 'MagoNet.Magazzino.ArticoliInclusiInAnalisiABC' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ItemsTypes'
WHERE( Command = 'MagoNet.Articoli.TipiArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.InvoicesPosting'
WHERE( Command = 'MagoNet.Vendite.StampaRegistraFatturaImmediata' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.BalanceScraps'
WHERE( Command = 'MagoNet.Cespiti.EliminazioniBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.NCReceiptsDeleting'
WHERE( Command = 'MagoNet.Vendite.EliminaRicevutaFiscaleNI' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DeliveryNotesPortfolioTot'
WHERE( Command = 'MagoNet.Vendite.TotaliPortafoglioClientiDDT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Tax'
WHERE( Command = 'MagoNet.Azienda.IVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetSimulation'
WHERE( Command = 'MagoNet.Cespiti.FixedAssetSimulation' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.ALAnalysis'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiPatrimonialiRiepilogativo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.BillOfMaterials'
WHERE( Command = 'MagoNet.DistintaBase.DistintaBase' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.PurchaseRequestSheet'
WHERE( Command = 'MagoNet.MRP.SchedaRDA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.EntriesByReason'
WHERE( Command = 'MagoNet.Cespiti.MovimentiPerCausale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.InvoicesListByMaterials'
WHERE( Command = 'MagoNet.Conai.ElencoFattureVendita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.MaterialSuppliedToSubcntList'
WHERE( Command = 'MagoNet.ContoLavoro.SituazioneMaterialiPressoFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.Non-ReclassifiedAccounts'
WHERE( Command = 'MagoNet.AnalisiBilancio.ContiNonRiclassificati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.CommissionPolicies'
WHERE( Command = 'MagoNet.Agenti.ListaDiSpuntaPoliticheProvvigionali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_IT.TaxReporting-Year2002'
WHERE( Command = 'MagoNet.Contabilita_it.ComunicazioneIVA2002' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Form770Review'
WHERE( Command = 'MagoNet.Percipienti.Spunta770' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AnnualTaxDeclarationData'
WHERE( Command = 'MagoNet.Contabilita.DatiPerDichiarazioneIVAAnnuale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.ProRataDetails'
WHERE( Command = 'MagoNet.Contabilita_ro.ProRataDetails' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.SummarizedBOMImplosion'
WHERE( Command = 'MagoNet.DistintaBase.ImplosioneDistintaBaseRiepilogativa' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.TurnoverByArea'
WHERE( Command = 'MagoNet.Agenti.FatturatoAgentePerArea' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.MO-EstimatedDates'
WHERE( Command = 'MagoNet.Produzione.PrevisioneDateOrdiniDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ForecastAccounting.AccountingSimulationsReview'
WHERE( Command = 'MagoNet.ContabilitaPrevisionale.SpuntaSimulazioniContabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SingleStepLifoFifo.Receipts'
WHERE( Command = 'MagoNet.LifoFifoScattiContinui.Carichi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Det-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriDettSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.CommissionsSettlementStatus'
WHERE( Command = 'MagoNet.Agenti.ListaProvvigioniLiquidateDaLiquidare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.TurnoverByCustAndItem'
WHERE( Command = 'MagoNet.Vendite.FatturatoClienteArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.CustomersCardsByAccrualDate'
WHERE( Command = 'MagoNet.Contabilita.ClientiPerCompetenza' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Non-RegCostCentersInEntries'
WHERE( Command = 'MagoNet.Analitica.TestCentriMovimenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.NoticeOfPayment'
WHERE( Command = 'MagoNet.Partite.AvvisoPagamento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.OrderedBySupplier'
WHERE( Command = 'MagoNet.OrdiniFornitori.ProspettoOrdinatoFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReceiptForm'
WHERE( Command = 'MagoNet.Vendite.FincatoRicevutaFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ABCAnalysis'
WHERE( Command = 'MagoNet.Magazzino.AnalisiABC' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsMonthly'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseContiCommesse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.OrderedByCustomer'
WHERE( Command = 'MagoNet.OrdiniClienti.TotaliOrdinatoClientePerAnno' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Carriers'
WHERE( Command = 'MagoNet.Spedizioni.Vettori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.EUAnnotationJournalNumCheck'
WHERE( Command = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVACEE' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DNDeferredInvoicing'
WHERE( Command = 'MagoNet.Vendite.FatturazioneDifferitaDDT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.PresentedVouchers'
WHERE( Command = 'MagoNet.PartiteAvanzato.ReversaliPresentate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.InvoiceForm'
WHERE( Command = 'MagoNet.Acquisti.FincatoFatturaAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.Titles'
WHERE( Command = 'MagoNet.Azienda.Titoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.ItemsValuationAtSalePrices'
WHERE( Command = 'MagoNet.PolitichePrezzi.APrezziDiVendita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.StoragesByItem'
WHERE( Command = 'MagoNet.MultiDeposito.DepositiPerArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.CreditNotesPortfolio'
WHERE( Command = 'MagoNet.Acquisti.PortafoglioFornitoriNoteCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.BOMStructure'
WHERE( Command = 'MagoNet.DistintaBase.StrutturaDistintaBase' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BillOExchangeSlip'
WHERE( Command = 'MagoNet.Partite.DistintaCambiali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.ItemsLots'
WHERE( Command = 'MagoNet.LottiMatricole.LottiDegliArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.SmartCodeRoots'
WHERE( Command = 'MagoNet.CodiceParlante.RadiciPerCodiceParlante' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CreditNotesPortfolio'
WHERE( Command = 'MagoNet.Vendite.PortafoglioClientiNoteDiCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxByCustSupp'
WHERE( Command = 'MagoNet.Contabilita.IVAperClifor' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.CashJournal'
WHERE( Command = 'MagoNet.Contabilita_ro.CashJournal' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.LIFOProgress'
WHERE( Command = 'MagoNet.Magazzino.formazionelifo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Invoices'
WHERE( Command = 'MagoNet.Vendite.ListaFattureImmediate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ProductionDevelopment-Comp'
WHERE( Command = 'MagoNet.Produzione.AvanzamentoProduzioneElencoMateriali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.SupplierQuotationForm'
WHERE( Command = 'MagoNet.OfferteFornitori.FincatoOffertaFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.SerialNosLabels'
WHERE( Command = 'MagoNet.Vendite.EtichetteMatricole2x8' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByStorageAndCust'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerDepositoECliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersSheets'
WHERE( Command = 'MagoNet.Analitica.CENTRISCHEDERIEP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Locations'
WHERE( Command = 'MagoNet.Cespiti.Ubicazioni' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.WCProcessingList'
WHERE( Command = 'MagoNet.Produzione.ElencoLavorazioniPerCdL' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.ExpiredSupplierQuotations'
WHERE( Command = 'MagoNet.OfferteFornitori.SpuntaOfferteDaFornitoreScadute' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TrialBalance-Grouped'
WHERE( Command = 'MagoNet.Contabilita.BilancioDiVerificaSintetico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.InvoicesReview'
WHERE( Command = 'MagoNet.Vendite.SpuntaCompletaFattureImmediate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FAInInventory'
WHERE( Command = 'MagoNet.Cespiti.InventarioFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.EntriesBySalesperson'
WHERE( Command = 'MagoNet.Agenti.ListaMovimenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.EntriesByDate'
WHERE( Command = 'MagoNet.Magazzino.MovimentiPerData' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.Non-RegItemsInEntries'
WHERE( Command = 'MagoNet.Magazzino.ArticoliMovimentatiNonCensiti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.EmployedDismissedSalesPeople'
WHERE( Command = 'MagoNet.Agenti.AgentiAssuntiCessati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Languages.ItemsInLanguage'
WHERE( Command = 'MagoNet.Lingue.ArticoliInLingua' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Banks.BanksAccounts'
WHERE( Command = 'MagoNet.Banche.BancheAziendaCCBancari' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.CustPymtScheduleByName'
WHERE( Command = 'MagoNet.PartiteAvanzato.ScadenzarioClientiPerRagioneSociale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.ExceptionsByDocumentType'
WHERE( Command = 'MagoNet.Acquisti.ListaDelleEccezioniPerTipoDocumento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.AuthorizedNotInvoicedEntries'
WHERE( Command = 'MagoNet.Agenti.MovimentiAutorizzatiENonFatturati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FiscalIncome'
WHERE( Command = 'MagoNet.Cespiti.RedditoFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.Areas'
WHERE( Command = 'MagoNet.Agenti.AreeDiVendita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TrialBalance-Det-WoCustSupp'
WHERE( Command = 'MagoNet.Contabilita.BilancioDiVerificaAnaliticoSenzaCliFor' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.DDSlip-OnPaper'
WHERE( Command = 'MagoNet.Partite.DistintaRIDSuCarta' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.BalanceRatios'
WHERE( Command = 'MagoNet.AnalisiBilancio.IndiciDiBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.FeesByDate'
WHERE( Command = 'MagoNet.Percipienti.ParcellePerData' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DeliveryNotesPosting'
WHERE( Command = 'MagoNet.Vendite.StampaRegistraDDT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsActual'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoContiCommesse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.IntraDispatchesCoverSheet'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniFrontespizio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.DisposalEntries'
WHERE( Command = 'MagoNet.Cespiti.MovimentiDismissione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.SalesGoodsJournal'
WHERE( Command = 'MagoNet.Contabilita_ro.SalesSummaryForDeliveryGoods' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccompanyingInvoiceForm'
WHERE( Command = 'MagoNet.Vendite.FincatoFatturaAccompagnatoria' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.Heading'
WHERE( Command = 'MagoNet.Contabilita.vidima' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.NCReceiptsPortfolio'
WHERE( Command = 'MagoNet.Vendite.PortafoglioClientiRicevuteFiscaliNonIncassate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.ItemsSuppliers'
WHERE( Command = 'MagoNet.OrdiniFornitori.ProspettoFornitoriArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.BalanceSimulation'
WHERE( Command = 'MagoNet.Cespiti.SimulazioneBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxForfaitSummaryJournal'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoIVAForfait' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.CustomersByItem'
WHERE( Command = 'MagoNet.Articoli.ClientiPerArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.WCPlan'
WHERE( Command = 'MagoNet.CRP.PianoCentriDiLavoro' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrdersSchedule-Detailed'
WHERE( Command = 'MagoNet.OrdiniClienti.ScadenzarioOrdiniDettaglioRighe' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsOn-HandByMonth'
WHERE( Command = 'MagoNet.Magazzino.DisponibilitaArticoliPerMese' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsYearly'
WHERE( Command = 'MagoNet.Analitica.BudgetEsCommesse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BillsToBePresented'
WHERE( Command = 'MagoNet.Partite.EffettiPresentabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AccountsCardsByAccrualDate'
WHERE( Command = 'MagoNet.Contabilita.ContiPerCompetenza' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.LastSaleData'
WHERE( Command = 'MagoNet.Articoli.DatiUltimaVendita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ReceiptLabelsWithBarCodes'
WHERE( Command = 'MagoNet.Articoli.EtichetteDiCarico2x8ConCodiciABarre' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsJournal-Bal-Sim'
WHERE( Command = 'MagoNet.Cespiti.RegistroSimulatoRaggruppatoBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ProductsComposition'
WHERE( Command = 'MagoNet.Articoli.StrutturaComposizioneProdotto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.TurnoverByCustomer'
WHERE( Command = 'MagoNet.Agenti.FatturatoAgentePerCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesPure'
WHERE( Command = 'MagoNet.Contabilita.SpuntaPrimeNotePure' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccInvoicesPortfolio'
WHERE( Command = 'MagoNet.Vendite.PortafoglioClientiFattureAccompagnatorie' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.ALAnalysis-Detailed'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiPatrimonialiAnalitico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ManufacturingOrderImplosion'
WHERE( Command = 'MagoNet.Produzione.ImplosioneOdP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersSheets-Det'
WHERE( Command = 'MagoNet.Analitica.CentriSchede' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.InvoicesList'
WHERE( Command = 'MagoNet.Acquisti.ListaFattureDiAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ForecastAccounting.ForecastSalesJournal'
WHERE( Command = 'MagoNet.ContabilitaPrevisionale.RegistriIVAEmessiPrevisionali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.CustomerAccountStatements'
WHERE( Command = 'MagoNet.Partite.EstrattiContoClienti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheet'
WHERE( Command = 'MagoNet.Contabilita.BilancioContrapposto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentersMonthly'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseContiCentri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.SalesPeopleByAreaManager'
WHERE( Command = 'MagoNet.Agenti.AgentiPerCapoarea' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.PartiallyExemptDeclaration'
WHERE( Command = 'MagoNet.Conai.DichiarazioneClientiEsenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.BalanceByFiscalYear'
WHERE( Command = 'MagoNet.Cespiti.BilancioPerEsercizio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.ProspSuppTaxIdNoCheck'
WHERE( Command = 'MagoNet.Contatti.ControlloPartitaIVAFornitoriPotenziali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.CustomersSheet'
WHERE( Command = 'MagoNet.ClientiFornitori.SchedaClienti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.IntangibleFixedAssetsLabels'
WHERE( Command = 'MagoNet.Cespiti.EtichetteCespitiImmateriali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsMonthly-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseContiCommesseProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PaymentOrderSlips'
WHERE( Command = 'MagoNet.Partite.DistinteMandati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ManufacturingOrderSheetForm'
WHERE( Command = 'MagoNet.Produzione.StampaDocSchedaOrdiniDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.TeamsPlan'
WHERE( Command = 'MagoNet.CRP.PianoSquadre' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.PickingLists'
WHERE( Command = 'MagoNet.Produzione.ElencoBuoniDiPrelievo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheet-PL'
WHERE( Command = 'MagoNet.Contabilita.BilancioDiEsercizioContoEconomico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Buffetti8904M2Form'
WHERE( Command = 'MagoNet.Vendite.FincatoBuffettiFattura8904M2' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.AnnualIntraDispatches2A'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniBisAnnuale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.ContactsSheet'
WHERE( Command = 'MagoNet.Contatti.SchedaContatti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.EntriesByDepartment'
WHERE( Command = 'MagoNet.Magazzino.MovimentiPerReparto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsMarkup-Detailed'
WHERE( Command = 'MagoNet.Magazzino.RicarichiArticoloDettaglio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Buffetti8902FBForm'
WHERE( Command = 'MagoNet.Vendite.FincatoBuffettiBolla8902FB' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrdersSchedule-Items'
WHERE( Command = 'MagoNet.OrdiniFornitori.ScadenzarioOrdiniAFornitorePerArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByCustAndStorage'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerClienteEDeposito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.CashFlowPayables'
WHERE( Command = 'MagoNet.PartiteAvanzato.CashFlowPartiteFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.SmartCodeSegments'
WHERE( Command = 'MagoNet.CodiceParlante.SegmentiPerCodiceParlante' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsYearly-Grp'
WHERE( Command = 'MagoNet.Analitica.BudgetEsCommesseGrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersYearly'
WHERE( Command = 'MagoNet.Analitica.BudgetEsCentri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.CompanyGroups'
WHERE( Command = 'MagoNet.BilanciConsolidati.GruppoAziende' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.BalanceDisposals'
WHERE( Command = 'MagoNet.Cespiti.DismissioniBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.PurchasesJournal'
WHERE( Command = 'MagoNet.Contabilita_ro.PurchasingJournal' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.DirectCosting'
WHERE( Command = 'MagoNet.Analitica.DirectCosting' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrderForm'
WHERE( Command = 'MagoNet.OrdiniFornitori.FincatoOrdineFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.TaxSummaryJournal'
WHERE( Command = 'MagoNet.Contabilita_ro.GeneralVATJournalSummaries' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.OrderedByItem'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdinatoArticoloPerPeriodoSintetico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsJourn-Bal-Det-Sim'
WHERE( Command = 'MagoNet.Cespiti.RegistroSimulatoBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReceiptsReview'
WHERE( Command = 'MagoNet.Vendite.SpuntaCompletaRicevuteFiscali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TrialBalance-Detailed'
WHERE( Command = 'MagoNet.Contabilita.BilancioDiVerificaAnalitico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.TrialBalance'
WHERE( Command = 'MagoNet.Contabilita_ro.TrialBalance' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.ComponentsVariantsList'
WHERE( Command = 'MagoNet.Varianti.ElencoVariantiComponenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Labels'
WHERE( Command = 'MagoNet.Articoli.Etichette2x8' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.CreditNotesList'
WHERE( Command = 'MagoNet.Acquisti.ListaNoteDiCreditoRicevute' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.PickingList-QtyToPickList'
WHERE( Command = 'MagoNet.Produzione.ElencoQuantitaDaPrelevareSuBdP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.ItemSheet'
WHERE( Command = 'MagoNet.DistintaBase.SchedaArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheet-FinStat-Ledger'
WHERE( Command = 'MagoNet.Contabilita.BilancioStatoPatrimonialeRiepilogativo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.NCReceiptsPortfolioTot'
WHERE( Command = 'MagoNet.Vendite.TotaliPortafoglioClientiRicevuteFiscaliNonIncassate' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Subcontracting.SubcontractorOrderList'
WHERE( Command = 'MagoNet.ContoLavoro.ElencoOrdiniAlFornitorePerLavorazioneEsterna' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.VouchersSlip'
WHERE( Command = 'MagoNet.PartiteAvanzato.DistintaReversali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ManufacturingOrdersList'
WHERE( Command = 'MagoNet.Produzione.ElencoOrdiniDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.AcquiredNotAccruedEntries'
WHERE( Command = 'MagoNet.Agenti.MovimentiAcquisitiNonMaturati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesJournal-EUAnnotations'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVAVenditeAcquistiCEE' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SpecialTax.ProRata'
WHERE( Command = 'MagoNet.RegimiIvaSpeciali.ProRataCalcolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.PriceListsReview'
WHERE( Command = 'MagoNet.PolitichePrezzi.SpuntaListini' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.TangibleFA-TechnicalData'
WHERE( Command = 'MagoNet.Cespiti.CespitiMaterialiDatiTecnici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Grp'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriRiepGrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsGroups'
WHERE( Command = 'MagoNet.Analitica.CommesseGruppi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsByCostCenter'
WHERE( Command = 'MagoNet.Cespiti.CespitiPerCentro' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.ALAnalysis-Grouped'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiPatrimonialiSintetico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentMonthly-Det-Grp-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriDettGrpSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.PrivacyStatement'
WHERE( Command = 'MagoNet.ClientiFornitori.consenso' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.EntriesByLot'
WHERE( Command = 'MagoNet.LottiMatricole.MovimentiPerLotto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FinancialByFiscalYear'
WHERE( Command = 'MagoNet.Cespiti.FinanziarioPerEsercizio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseRiep' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.InventoryReasonsSheet-Page2'
WHERE( Command = 'MagoNet.Magazzino.CausaliDiMagazzino2' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchasesExigibleTax'
WHERE( Command = 'MagoNet.Contabilita.IvaEsigibileAcquisti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReceiptsDeleting'
WHERE( Command = 'MagoNet.Vendite.EliminaRicevutaFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.EUBalSheet-FinStat-Casc'
WHERE( Command = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimonialeScalare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.ProducibilityAnalysis'
WHERE( Command = 'MagoNet.DistintaBase.AnalisiProducibilita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SpecialTax.ProRata-Detailed'
WHERE( Command = 'MagoNet.RegimiIvaSpeciali.ProRataDettaglio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.CascadeBOMImplosion'
WHERE( Command = 'MagoNet.DistintaBase.ImplosioneDistintaBaseScalare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.CashOrderSlip'
WHERE( Command = 'MagoNet.Partite.DistintaRIBA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccInvoiceForm-MoreBranches'
WHERE( Command = 'MagoNet.Vendite.FincatoFatturaAccompagnatoriaPiuSedi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.BreakdownReasonsList'
WHERE( Command = 'MagoNet.Cicli.ElencoCausaliND' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.ComponentsVariantsSheet'
WHERE( Command = 'MagoNet.Varianti.SchedaVariantiComponenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ItemsBySupplier'
WHERE( Command = 'MagoNet.Articoli.ArticoliPerFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_PL.PurchasesJournal'
WHERE( Command = 'MagoNet.Contabilita_pl.PurchasesRegister' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.ItemsSuppliersDelivered'
WHERE( Command = 'MagoNet.Acquisti.ConsegnatoFornitoriArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Buffetti8903FForm'
WHERE( Command = 'MagoNet.Vendite.FincatoBuffettiFatturaAccompagnatoria8903F' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.OrderedByItemAndCustomer'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdinatoPerArticoloECliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersActual-Det-Grp'
WHERE( Command = 'MagoNet.Analitica.consuntivocentrigrpdett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Grp-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseRiepGrpSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.EntriesByInvoice'
WHERE( Command = 'MagoNet.Agenti.MovimentiPerFattura' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.Producers'
WHERE( Command = 'MagoNet.Articoli.Produttori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.ItemSheetByStorage'
WHERE( Command = 'MagoNet.MultiDeposito.SchedaArticoloPerDeposito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FAJournal-Detailed-Sim'
WHERE( Command = 'MagoNet.Cespiti.RegistroSimulato' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxGeneralSummaryJournal'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoGeneraleRegistriIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.ReceivablesDeleting'
WHERE( Command = 'MagoNet.Partite.PartiteClienteDaEliminare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.SuppAccStatementsByPymtSched'
WHERE( Command = 'MagoNet.Partite.EstrattiContoFornitoriPerPartita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.ItemsByPriceList'
WHERE( Command = 'MagoNet.PolitichePrezzi.SpuntaArticoliPerListino' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsYearly'
WHERE( Command = 'MagoNet.Analitica.BudgetEsContiCommesse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.VouchersToBePresented'
WHERE( Command = 'MagoNet.PartiteAvanzato.ReversaliPresentabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.ShortMediumTermReceivables'
WHERE( Command = 'MagoNet.PartiteAvanzato.CreditiMedioTermine' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.EUBalSheet-AbbFinStat-Casc'
WHERE( Command = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimonialeAbbreviatoScalare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SpecialTax.TaxPlafond'
WHERE( Command = 'MagoNet.RegimiIvaSpeciali.PlafondIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsInEntriesByCustomer'
WHERE( Command = 'MagoNet.Magazzino.ArticoliMovimentatiPerCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.SuppliersByCommodityCtg'
WHERE( Command = 'MagoNet.Articoli.FornitoriPerCategorieMerceologiche' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_GR.KEPYOCover'
WHERE( Command = 'MagoNet.Contabilita_gr.KepyoCover' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.PurchasedBySuppliersTot'
WHERE( Command = 'MagoNet.Acquisti.TotaliAcquistatoFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.SuppliersAddressBook'
WHERE( Command = 'MagoNet.ClientiFornitori.RubricaFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.FIRRAccountStatements'
WHERE( Command = 'MagoNet.Agenti.EstrattiContoFIRR' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.SuppliersCategories'
WHERE( Command = 'MagoNet.ClientiFornitori.CategorieFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsSheets-FullCosting'
WHERE( Command = 'MagoNet.Analitica.CommesseSchedeFull' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ItemsPurchaseBarCodes'
WHERE( Command = 'MagoNet.Articoli.ArticoliBarCodeAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.AnnualIntraDispatches2B'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniTerAnnuale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.InvoiceForm-PL'
WHERE( Command = 'MagoNet.Vendite.FakturaVAT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.CascadeBOMExplosion-Manuf'
WHERE( Command = 'MagoNet.DistintaBase.EsplosioneDistintaBaseScalareProd' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrders'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdiniDaCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByStorageAndCust-Item'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerDepositoClienteEArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsFullCosting'
WHERE( Command = 'MagoNet.Analitica.CommesseFullCost' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.CategorySheet'
WHERE( Command = 'MagoNet.Cespiti.SchedaFiscaleCategoria' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FAInBalanceSheetNotes-Bal'
WHERE( Command = 'MagoNet.Cespiti.NotaIntegrativaBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.InvoicesPortfolio'
WHERE( Command = 'MagoNet.Acquisti.PortafoglioFornitoriFattureAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheet-PL-ByLedger'
WHERE( Command = 'MagoNet.Contabilita.BilancioContoEconomicoRiepilogativo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Buffetti8902D3Form'
WHERE( Command = 'MagoNet.Vendite.FincatoBuffettiDDT8902D3' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.ItemVariantsList'
WHERE( Command = 'MagoNet.Varianti.ElencoVariantiperArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchJournalByPostingDate'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVAAcquistiPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.Voucher'
WHERE( Command = 'MagoNet.PartiteAvanzato.Reversale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsOn-Hand'
WHERE( Command = 'MagoNet.Magazzino.DisponibilitaArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SupplierQuotations.SupplierQuotations'
WHERE( Command = 'MagoNet.OfferteFornitori.ListaOfferteDaFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsDeliveriesDelay'
WHERE( Command = 'MagoNet.Analitica.CommesseRitardi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrdersToFulfill'
WHERE( Command = 'MagoNet.OrdiniClienti.EvadibilitaPerCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.PackingProducersDeclarations'
WHERE( Command = 'MagoNet.Conai.DichiarazioneProduttoriImballaggi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrdersReview'
WHERE( Command = 'MagoNet.OrdiniClienti.SpuntaOrdiniCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.MOCRPDatesEstimate'
WHERE( Command = 'MagoNet.CRP.PrevisioneDateODPCRP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.CustSuppBalanceSheet'
WHERE( Command = 'MagoNet.Contabilita.BilancioCliFor' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsNotInEntriesSince'
WHERE( Command = 'MagoNet.Magazzino.ArticoliNonMovimentatiDal' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.ReorderMaterialsToSupplier'
WHERE( Command = 'MagoNet.DistintaBase.RiordinoAFornitoreMaterialiMancanti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsSheets'
WHERE( Command = 'MagoNet.Analitica.ContiSchede' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.AllowanceAccountStatements'
WHERE( Command = 'MagoNet.Agenti.EstrattiContoIndennita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsJournal-Detailed'
WHERE( Command = 'MagoNet.Cespiti.Registro' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.MonthlyIntraArrivals1A'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiBisMensile' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesBySupplier'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.RetSalesJournByPostDate'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporoPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersBudget-Det-Grp'
WHERE( Command = 'MagoNet.Analitica.preventivocentrigrpdett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Configurator.IncompatQuestionsAnswers'
WHERE( Command = 'MagoNet.Configuratore.IncompDomRisp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.MonthlyIntraArrivals1B'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiTerMensile' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Ports'
WHERE( Command = 'MagoNet.Spedizioni.Porti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.IntangibleFA-TechnicalData'
WHERE( Command = 'MagoNet.Cespiti.CespitiImmaterialiDatiTecnici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsDirectCosting'
WHERE( Command = 'MagoNet.Analitica.CommesseDirectCost' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.InvoicesPosting'
WHERE( Command = 'MagoNet.Acquisti.RegistraFatturaAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ProcessingMaterialsList'
WHERE( Command = 'MagoNet.Produzione.MaterialiInLavorazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.InventoryReasons'
WHERE( Command = 'MagoNet.Magazzino.ListaCausaliDiMagazzino' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersBal'
WHERE( Command = 'MagoNet.Analitica.CentriSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsScrapQty'
WHERE( Command = 'MagoNet.Magazzino.QuantitaArticoliScarti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.SuppliersItems'
WHERE( Command = 'MagoNet.Acquisti.ProspettoArticoliFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheetComparative'
WHERE( Command = 'MagoNet.Contabilita.BilancioContrappostoComparato' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.EUAnnJournalNumCheckByDate'
WHERE( Command = 'MagoNet.Contabilita.ControlloNumerazioniRegistriIVACEEPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.IdsMng.StubBooks'
WHERE( Command = 'MagoNet.IdsMng.Bollettari' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchasesNotExigibleTax'
WHERE( Command = 'MagoNet.Contabilita.IvaInesigibileAcquisti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxData'
WHERE( Command = 'MagoNet.Contabilita.DatiIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.JobTickets-InventoryEntries'
WHERE( Command = 'MagoNet.Produzione.MovimentiDiMagazzinoDaBollaDiLavorazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.ReclassificationSchemas'
WHERE( Command = 'MagoNet.AnalisiBilancio.SchemiDiRiclassificazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrdersReview'
WHERE( Command = 'MagoNet.OrdiniFornitori.SpuntaCompletaOrdiniAFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Shippings.Packages'
WHERE( Command = 'MagoNet.Spedizioni.Imballi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.ParentMOList'
WHERE( Command = 'MagoNet.MRP.ElencoOrdiniProduzionePadre' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.PLAnalysis-Grouped'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiEconomiciSintetico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.CustomersTaxIdNoCheck'
WHERE( Command = 'MagoNet.ClientiFornitori.ControlloPartitaIVAClienti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersQtyBalances'
WHERE( Command = 'MagoNet.Analitica.CentriSaldiQta' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.HomogeneousCategories'
WHERE( Command = 'MagoNet.Articoli.CategorieOmogenee' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.SummarizedBOMExplosion'
WHERE( Command = 'MagoNet.DistintaBase.EsplosioneDistintaBaseRiepilogativa' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.SerialNosLabels'
WHERE( Command = 'MagoNet.Acquisti.EtichetteMatricole' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.EntriesByItem'
WHERE( Command = 'MagoNet.Analitica.MovimentiPerArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Banks.CompanyBanks'
WHERE( Command = 'MagoNet.Banche.BancheAzienda' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.AnnualIntraArrivals1B'
WHERE( Command = 'MagoNet.Intrastat.IntraAcquistiTerAnnuale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.EUBalSheet-FinStat'
WHERE( Command = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimoniale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchasesSummaryJournal'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoRegistroIVAAcquisti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.PurchasedByItemsTot'
WHERE( Command = 'MagoNet.Acquisti.TotaliAcquistatoArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.PurchasedBySuppliers'
WHERE( Command = 'MagoNet.Acquisti.ProspettoAcquistatoFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.AvailabilityForecast'
WHERE( Command = 'MagoNet.MRP.PrevisioneAndamentoGiacenze' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.OrderComponentsToPickList'
WHERE( Command = 'MagoNet.Produzione.ElencoMaterialiDaPrelevare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.TurnoverByCustAndItem-Det'
WHERE( Command = 'MagoNet.Vendite.FatturatoClienteArticoloSuRighe' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccompanyingInvoicesPosting'
WHERE( Command = 'MagoNet.Vendite.StampaRegistraFatturaAccompagnatoria' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.OutstandingBills'
WHERE( Command = 'MagoNet.PartiteAvanzato.EffettiInsoluti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Currencies.Currencies'
WHERE( Command = 'MagoNet.Divise.Divise' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.EntriesByReason'
WHERE( Command = 'MagoNet.Magazzino.MovimentiPerCausale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.InventoryValuation'
WHERE( Command = 'MagoNet.Magazzino.ACosti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetSheet'
WHERE( Command = 'MagoNet.Cespiti.SchedaFiscaleCespite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.Non-CollectedReceiptsPosting'
WHERE( Command = 'MagoNet.Vendite.StampaRegistraRicevutaFiscaleNI' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.SuppliersSheet'
WHERE( Command = 'MagoNet.ClientiFornitori.SchedaFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Simulation'
WHERE( Command = 'MagoNet.Cespiti.SimulazioneFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SuppliersCardsByPostingDate'
WHERE( Command = 'MagoNet.Contabilita.FornitoriPerRegistrazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.InventoryJournal'
WHERE( Command = 'MagoNet.Magazzino.GiornaleDiMagazzino' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.INPSAccountStatements'
WHERE( Command = 'MagoNet.Percipienti.EstrattiContoINPS' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DeferredInvoicing'
WHERE( Command = 'MagoNet.Vendite.FatturazioneDifferita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.EntriesByDate'
WHERE( Command = 'MagoNet.Cespiti.MovimentiPerData' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsQtyBal'
WHERE( Command = 'MagoNet.Analitica.CommesseSaldiQta' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.SoldPurchasedItems'
WHERE( Command = 'MagoNet.Magazzino.ProspettoVendutoAcquistatoArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.ItemsToOrderBySuppliers'
WHERE( Command = 'MagoNet.OrdiniFornitori.ListaArticoliDaOrdinare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.SalesPeopleMaster'
WHERE( Command = 'MagoNet.Agenti.AnagraficaAgenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.IncompleteJournalEntries'
WHERE( Command = 'MagoNet.Contabilita.PrimeNoteZoppe' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SmartCode.SmartCodeRootsSheet'
WHERE( Command = 'MagoNet.CodiceParlante.SchedaRadice' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.TurnoverByCustomer-Detailed'
WHERE( Command = 'MagoNet.Agenti.FatturatoAgentePerClienteConDettaglioDocumenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.PriceListsByItems'
WHERE( Command = 'MagoNet.PolitichePrezzi.SpuntaListiniPerArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.PurchaseTaxByLog'
WHERE( Command = 'MagoNet.Contabilita.IVAAcquistiperProtocollo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrdToRequestDelivery'
WHERE( Command = 'MagoNet.OrdiniFornitori.SollecitiOrdiniAFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AccountStatement'
WHERE( Command = 'MagoNet.Contabilita.EstrattoConto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_GR.TrialBalanceCover'
WHERE( Command = 'MagoNet.Contabilita_gr.TrialCover' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.CommodityCategories'
WHERE( Command = 'MagoNet.Articoli.CategorieMerceologiche' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.DisabledJobsInTpl'
WHERE( Command = 'MagoNet.Analitica.TestCommesseDisModelli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Sales'
WHERE( Command = 'MagoNet.Cespiti.Vendite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.ENASARCOPostalPaymentSlip'
WHERE( Command = 'MagoNet.Agenti.BollettinoEnasarco' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.CustAccStatementsBySalespers'
WHERE( Command = 'MagoNet.PartiteAvanzato.EstrattiContoClientiPerAgente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.PrivacyStatementLabels'
WHERE( Command = 'MagoNet.ClientiFornitori.EtiConsenso' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.EntriesSheet'
WHERE( Command = 'MagoNet.Magazzino.SchedaMovimentiMagazzino' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesByLog'
WHERE( Command = 'MagoNet.Contabilita.ContabileAcquistiPerProtocollo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.ENASARCOPaymentSlip'
WHERE( Command = 'MagoNet.Agenti.DistintaEnasarco' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Disposals'
WHERE( Command = 'MagoNet.Cespiti.Dismissioni' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PymtsScheduleByBank'
WHERE( Command = 'MagoNet.Partite.ScadenzarioPagamentiBanca' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.TeamsList'
WHERE( Command = 'MagoNet.Cicli.ElencoSquadre' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.ContactsTaxIdNoCheck'
WHERE( Command = 'MagoNet.Contatti.ControlloPartitaIVAContatti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.MRPWarningsMessages'
WHERE( Command = 'MagoNet.MRP.SegnalazioniMRP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.TurnoverByCustomer-Detailed'
WHERE( Command = 'MagoNet.Vendite.FatturatoSinteticoClienteSuRighe' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.ShortMediumTermPyblsBySupp'
WHERE( Command = 'MagoNet.PartiteAvanzato.DebitiMedioTerminePerFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReceiptsPortfolioTot'
WHERE( Command = 'MagoNet.Vendite.TotaliPortafoglioClientiRicevuteFiscali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostAccAccounts'
WHERE( Command = 'MagoNet.Analitica.ContiAnalitici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ManufacturingOrder'
WHERE( Command = 'MagoNet.Produzione.OrdineDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesJournalByPostingDate'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVAVenditePerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.RoutingVariantsList'
WHERE( Command = 'MagoNet.Varianti.ElencoVariantiCicli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccInvoicesPortfolioTot'
WHERE( Command = 'MagoNet.Vendite.TotaliPortafoglioClientiFattureAccompagnatorie' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsActual'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoCommesse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FAInInventory-Balance'
WHERE( Command = 'MagoNet.Cespiti.InventarioBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.DunnedCustomers'
WHERE( Command = 'MagoNet.PartiteAvanzato.ClientiSollecitati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsWithNegBookInvValues'
WHERE( Command = 'MagoNet.Magazzino.ArticoliConGiacenzeNegative' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PresentedBills'
WHERE( Command = 'MagoNet.Partite.EffettiPresentati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersByCode'
WHERE( Command = 'MagoNet.Analitica.CentriPerCodice' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ProductCategories'
WHERE( Command = 'MagoNet.Articoli.CategorieProdotto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.ItemSheetWithStorage'
WHERE( Command = 'MagoNet.MultiDeposito.SchedaArticoloConVisioneDeposito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.BalanceSales'
WHERE( Command = 'MagoNet.Cespiti.VenditeBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersGroups'
WHERE( Command = 'MagoNet.Analitica.CentriGruppi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CreditNotes'
WHERE( Command = 'MagoNet.Vendite.ListaNoteDiCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersBudget-Det'
WHERE( Command = 'MagoNet.Analitica.PreventivoCentriDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.MOJobTicketDetailedSheet'
WHERE( Command = 'MagoNet.Produzione.DettaglioAnagraficoBollaDiLavorazionePerOdP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.BillsOfLadingDeleting'
WHERE( Command = 'MagoNet.Acquisti.EliminaBollaCarico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Company.ISOCountryCodes'
WHERE( Command = 'MagoNet.Azienda.ISOStati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsDeliveries'
WHERE( Command = 'MagoNet.Analitica.CommessePerConsegna' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.TurnoverByAreaAndCustomer'
WHERE( Command = 'MagoNet.Agenti.FatturatoAgentePerAreaECliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Classes'
WHERE( Command = 'MagoNet.Cespiti.Classi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BillsToBePresentedByCustomer'
WHERE( Command = 'MagoNet.Partite.EffettiPresentabiliCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Variants.VariantsList'
WHERE( Command = 'MagoNet.Varianti.ElencoVarianti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.FeesSheet'
WHERE( Command = 'MagoNet.Percipienti.SchedeParcelle' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ManufacturingOrderExplosion'
WHERE( Command = 'MagoNet.Produzione.EsplosioneOdP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Scraps'
WHERE( Command = 'MagoNet.Cespiti.Eliminazioni' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Banks.CustSuppBanks'
WHERE( Command = 'MagoNet.Banche.BancheCliFor' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Grp-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseRiepGrpProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersYearly-Det'
WHERE( Command = 'MagoNet.Analitica.BudgetEsCentriDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccompanyingInvoicesReview'
WHERE( Command = 'MagoNet.Vendite.SpuntaCompletaFattureAccompagnatorie' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.EUBalSheet-AbbFinStat'
WHERE( Command = 'MagoNet.AnalisiBilancio.BilancioCEEStatoPatrimonialeAbbreviato' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.SuppliersByItem'
WHERE( Command = 'MagoNet.Articoli.FornitoriPerArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Det-Grp-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseDettGrpSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.SuppliersTaxIdNoCheck'
WHERE( Command = 'MagoNet.ClientiFornitori.ControlloPartitaIVAFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Contacts.ProspSuppAddressBook'
WHERE( Command = 'MagoNet.Contatti.RubricaFornitoriPotenziali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsYearly-Det'
WHERE( Command = 'MagoNet.Analitica.BudgetEsCommesseDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.CustomerQuotationForm'
WHERE( Command = 'MagoNet.OfferteClienti.FincatoOffertaCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrdersSchedule-Items'
WHERE( Command = 'MagoNet.OrdiniClienti.ScadenzarioOrdiniDettaglioArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrders'
WHERE( Command = 'MagoNet.OrdiniFornitori.ListaOrdiniAFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.AccInvoicesDeleting'
WHERE( Command = 'MagoNet.Vendite.EliminaFatturaAccompagnatoria' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomerQuotations.CustomerQuotations'
WHERE( Command = 'MagoNet.OfferteClienti.OfferteACliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.ItemsSuppliersLabel'
WHERE( Command = 'MagoNet.Articoli.EtichetteFornitoriArticoli2x8' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.ProductionPlanGeneration'
WHERE( Command = 'MagoNet.DistintaBase.GenerazionePianoProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PricePolicies.PriceLists'
WHERE( Command = 'MagoNet.PolitichePrezzi.Listini' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ChartOfAccounts.ChartOfAccounts'
WHERE( Command = 'MagoNet.PianoDeiConti.PianoConti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentersBudget'
WHERE( Command = 'MagoNet.Analitica.PreventivoContiCentri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.JobTicketsForm-DetailedStep'
WHERE( Command = 'MagoNet.Produzione.StampaDocElencoBolleDiLavorazioneConDettaglioFasi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.BalanceSheet-PL-Detailed'
WHERE( Command = 'MagoNet.Contabilita.BilancioContoEconomicoAnalitico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.LastPurchaseData'
WHERE( Command = 'MagoNet.Articoli.DatiUltimoAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.SuppPymtScheduleByName'
WHERE( Command = 'MagoNet.PartiteAvanzato.ScadenzarioFornitoriPerRagioneSociale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentersYearly'
WHERE( Command = 'MagoNet.Analitica.BudgetEsContiCentri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByStorageAndSupp'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerDepositoEFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.FeesToBePaid'
WHERE( Command = 'MagoNet.Percipienti.ParcelleDaPagare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Languages.Languages'
WHERE( Command = 'MagoNet.Lingue.Lingue' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.OrderedByItem'
WHERE( Command = 'MagoNet.OrdiniFornitori.ProspettoOrdinatoArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReceiptsPosting'
WHERE( Command = 'MagoNet.Vendite.StampaRegistraRicevutaFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FAInBalanceSheetNotes'
WHERE( Command = 'MagoNet.Cespiti.NotaIntegrativaFiscale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.InvoiceForm'
WHERE( Command = 'MagoNet.Vendite.FincatoFatturaImmediata' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.DetailedCosting'
WHERE( Command = 'MagoNet.DistintaBase.CostificazioneAnalitica' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.IntangibleFA-PurchaseData'
WHERE( Command = 'MagoNet.Cespiti.CespitiImmaterialiDatiAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DDTPackingListForm'
WHERE( Command = 'MagoNet.Vendite.FincatoDDTPackingList' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Conai.EntriesByMaterialsAndPackage'
WHERE( Command = 'MagoNet.Conai.MovimentiPerMaterialeTipologia' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Det'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PaymentOrdersToBeIssued'
WHERE( Command = 'MagoNet.Partite.MandatiEmettibili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesSummaryJournal-EUAnn'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoRegistroIVACEE' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MRP.MOFeasabilityAnalysis'
WHERE( Command = 'MagoNet.MRP.AnalisiOrdiniProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PymtOrdersToBeIssuedToSupp'
WHERE( Command = 'MagoNet.Partite.MandatiEmettibiliFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsSheets'
WHERE( Command = 'MagoNet.Cespiti.SchedeCespiti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.UoMs'
WHERE( Command = 'MagoNet.Articoli.UnitaDiMisura' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.AvailabilityAnalysis'
WHERE( Command = 'MagoNet.Produzione.ProspettoDisponibilita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.DNForm'
WHERE( Command = 'MagoNet.Vendite.FincatoDDT' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.CreditNotesDeleting'
WHERE( Command = 'MagoNet.Vendite.EliminaNotaCredito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsCostCentMonthly-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseContiCentriSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersBudget'
WHERE( Command = 'MagoNet.Analitica.PreventivoCentri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BillsPymtScheduleByBank'
WHERE( Command = 'MagoNet.Partite.ScadenzarioEffettiBanca' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_IT.WithholdingTaxesReview'
WHERE( Command = 'MagoNet.Contabilita_it.SpuntaRitenute' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesTax'
WHERE( Command = 'MagoNet.Contabilita.SpuntaPrimeNoteIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.Entries'
WHERE( Command = 'MagoNet.Analitica.MovimentiTeste' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.SuppliersItems'
WHERE( Command = 'MagoNet.OrdiniFornitori.ProspettoArticoliFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.SuppliersLedgerCards'
WHERE( Command = 'MagoNet.Contabilita_ro.LedgerCardsSuppliers' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.RetSalesDailyJournByPostDate'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVACorrispettiviScorporoGionalieroPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersActual'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoCentri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsBudget'
WHERE( Command = 'MagoNet.Analitica.PreventivoContiCommesse' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.SimulationByLifePeriod'
WHERE( Command = 'MagoNet.Cespiti.SimulazioneFiscaleDurata' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.PickingList-PickedQtyList'
WHERE( Command = 'MagoNet.Produzione.ElencoRigheDiPrelievoSuBdP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.SalesPeopleBalances'
WHERE( Command = 'MagoNet.Agenti.SaldiAgenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.SaleOrdersPortfolioTot'
WHERE( Command = 'MagoNet.OrdiniClienti.TotaliPortafoglioOrdini' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.ENASARCOFromFees'
WHERE( Command = 'MagoNet.Agenti.EnasarcoDaParcelle' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.INPSPymtSchedule'
WHERE( Command = 'MagoNet.Percipienti.ScadenzarioINPS' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.PackageLabels'
WHERE( Command = 'MagoNet.Vendite.EtichetteSovrapacco2x8' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ChartOfAccounts.Ledgers'
WHERE( Command = 'MagoNet.PianoDeiConti.Mastri' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReceiptsPortfolio'
WHERE( Command = 'MagoNet.Vendite.PortafoglioClientiRicevuteFiscali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PayablesDeleting'
WHERE( Command = 'MagoNet.Partite.PartiteFornitoreDaEliminare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.AccountsJobsBal'
WHERE( Command = 'MagoNet.Analitica.ContiCommesseSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsMarkup'
WHERE( Command = 'MagoNet.Magazzino.RicarichiArticoloSintesi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.JobTicketSheetForm'
WHERE( Command = 'MagoNet.Produzione.StampaDocSchedaBollaDiLavorazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Grp-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriRiepGrpProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SaleOrders.OrderedByCustomerAndItem'
WHERE( Command = 'MagoNet.OrdiniClienti.OrdinatoPerClienteEArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.ConsolidationTemplates'
WHERE( Command = 'MagoNet.BilanciConsolidati.ModelliConsolidamento' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrdersSchedule-Det'
WHERE( Command = 'MagoNet.OrdiniFornitori.ScadenzarioRigheOrdiniAFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.BillsOfLadingPosting'
WHERE( Command = 'MagoNet.Acquisti.RegistraBollaCarico' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesPureHeaders'
WHERE( Command = 'MagoNet.Contabilita.TesteContabiliPure' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.ReceiptLabelsWithBarCodes'
WHERE( Command = 'MagoNet.Acquisti.EtichetteDiCaricoConCodiciABarre' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.FIFOData'
WHERE( Command = 'MagoNet.Magazzino.DatiFIFO' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.FeesBySupplier'
WHERE( Command = 'MagoNet.Percipienti.ParcellePerFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.CashFlowReceivables'
WHERE( Command = 'MagoNet.PartiteAvanzato.CashFlowPartiteCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Routing.WCScheet'
WHERE( Command = 'MagoNet.Cicli.SchedaCentriDiLavoro' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.RetailSalesSummaryJournal'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoRegistroIVACorrispettiviScorporo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsBudget-Det-Grp'
WHERE( Command = 'MagoNet.Analitica.PreventivoCommesseGrpDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Det-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriDettProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.CustomersAddressBook'
WHERE( Command = 'MagoNet.ClientiFornitori.RubricaClienti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.MonthlyIntraDispatches2A'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniBisMensile' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Checks'
WHERE( Command = 'MagoNet.Partite.AssegniBancari' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.EntriesByAccount'
WHERE( Command = 'MagoNet.Analitica.MovimentiPerConto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiCompanyBalances.MultiCompanyBalances'
WHERE( Command = 'MagoNet.BilanciConsolidati.BilanciConsolidati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.SaleOrdersSchedule'
WHERE( Command = 'MagoNet.Produzione.ScadenziarioRigheOrdiniDaCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesSummaryJournal'
WHERE( Command = 'MagoNet.Contabilita.RiepilogoRegistroIVAVendite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntries'
WHERE( Command = 'MagoNet.Contabilita.SpuntaPrimeNote' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BillOfMaterials.CascadeBOMExplosion'
WHERE( Command = 'MagoNet.DistintaBase.EsplosioneDistintaBaseScalare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.EntriesByPostingDate'
WHERE( Command = 'MagoNet.Analitica.MovimentiPerDataReg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PaymentOrdersSlip'
WHERE( Command = 'MagoNet.Partite.DistintaMandati' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.CustomersCardsByPostingDate'
WHERE( Command = 'MagoNet.Contabilita.ClientiPerRegistrazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.PaymentOrder'
WHERE( Command = 'MagoNet.Partite.Mandato' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_IT.F24Form'
WHERE( Command = 'MagoNet.Contabilita_it.ModelloF242003' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseRiepSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.FullCosting'
WHERE( Command = 'MagoNet.Analitica.FullCosting' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.CustAccStatementByPymtSched'
WHERE( Command = 'MagoNet.Partite.EstrattiContoClientiPerPartita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsBudget-Grp'
WHERE( Command = 'MagoNet.Analitica.PreventivoCommesseGrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersTemplates'
WHERE( Command = 'MagoNet.Analitica.CentriModelliAnalitici' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.BalanceAnalysis.PLAnalysis'
WHERE( Command = 'MagoNet.AnalisiBilancio.AnalisiEconomiciRiepilogativo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AccountingDefaults'
WHERE( Command = 'MagoNet.Contabilita.ContiDiDefault' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsJournal-Bal'
WHERE( Command = 'MagoNet.Cespiti.RegistroRaggruppatoBilancio' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersBudget-Grp'
WHERE( Command = 'MagoNet.Analitica.preventivocentrigrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.Receivables'
WHERE( Command = 'MagoNet.Partite.PartiteCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.JournalEntriesByPostingDate'
WHERE( Command = 'MagoNet.Contabilita.PrimeNotePerRegistrazione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersByType'
WHERE( Command = 'MagoNet.Analitica.CentriPerTipo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.Form770Form'
WHERE( Command = 'MagoNet.Percipienti.Lista770' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CustomersSuppliers.CustomersLabels'
WHERE( Command = 'MagoNet.ClientiFornitori.EtichetteClienti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.IntraDispatches2A'
WHERE( Command = 'MagoNet.Intrastat.IntraCessioniBis' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.PurchasedByItems'
WHERE( Command = 'MagoNet.Acquisti.ProspettoAcquistatoArticoli' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.SupplierPymtSchedule'
WHERE( Command = 'MagoNet.Partite.ScadenzarioFornitoriPerDataScadenza' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsActual-Det-Grp'
WHERE( Command = 'MagoNet.Analitica.ConsuntivoCommesseGrpDett' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.SalesJournal'
WHERE( Command = 'MagoNet.Contabilita_ro.SalesJournal' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersMonthly-Bal'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCentriRiepSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.TangibleFixedAssetsLabels'
WHERE( Command = 'MagoNet.Cespiti.EtichetteCespitiMateriali' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsBal'
WHERE( Command = 'MagoNet.Analitica.CommesseSaldi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.WCJobTicketDetailedSheet'
WHERE( Command = 'MagoNet.Produzione.DettaglioAnagraficoBollaDiLavorazionePerCdL' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Purchases.InvoicesDeleting'
WHERE( Command = 'MagoNet.Acquisti.EliminaFatturaAcquisto' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-Grp'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseRiepGrp' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AccountingTemplates'
WHERE( Command = 'MagoNet.Contabilita.SpuntaModelliContabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CRP.MOProductionProgress'
WHERE( Command = 'MagoNet.CRP.StatoAvanzamentoODP' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesBySuppAndStorage'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerFornitoreEDeposito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.DetailedStepJobTicketsList'
WHERE( Command = 'MagoNet.Produzione.ElencoBolleDiLavorazioneConDettaglioFasi' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReturnsFromCustomer'
WHERE( Command = 'MagoNet.Vendite.ListaResiDaCliente' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR.BillsPymtSchedule'
WHERE( Command = 'MagoNet.Partite.ScadenzarioEffetti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.FixedAssetsJournal'
WHERE( Command = 'MagoNet.Cespiti.RegistroRaggruppato' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesNotExigibleTax'
WHERE( Command = 'MagoNet.Contabilita.IvaInesigibileVendite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.SalesPeople.ENASARCOFromBalances-Det'
WHERE( Command = 'MagoNet.Agenti.EnasarcoDettDaSaldiAgenti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.PurchaseOrders.PurchaseOrdersPortfolio'
WHERE( Command = 'MagoNet.OrdiniFornitori.PortafoglioOrdiniAFornitori' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.ForecastAccounting.AccountingSimulations'
WHERE( Command = 'MagoNet.ContabilitaPrevisionale.SimulazioniContabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.TaxSummaryJournal'
WHERE( Command = 'MagoNet.Contabilita.RiepiloghiRegistroIVA' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.CostCentersByNature'
WHERE( Command = 'MagoNet.Analitica.CentriPerNatura' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Intrastat.IntraItems'
WHERE( Command = 'MagoNet.Intrastat.ArticoliIntra' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ManufacturingOrderSheet'
WHERE( Command = 'MagoNet.Produzione.SchedaOrdiniDiProduzione' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.PickingListSheet'
WHERE( Command = 'MagoNet.Produzione.StampaBuonoDiPrelievo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.JobCostAnalysis'
WHERE( Command = 'MagoNet.Produzione.AnalisiCostiCommessa' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Items.LabelsWithBarCodes'
WHERE( Command = 'MagoNet.Articoli.EtichetteConCodiciABarreDiAcquistoOVendita' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesExigibleTax'
WHERE( Command = 'MagoNet.Contabilita.IvaEsigibileVendite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.MultiStorage.EntriesByCustAndStorage-Item'
WHERE( Command = 'MagoNet.MultiDeposito.MovimentiPerClienteDepositoEArticolo' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.FixedAssets.Non-RegFAInEntries'
WHERE( Command = 'MagoNet.Cespiti.CespitiMovimentatiNonCensiti' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Inventory.ItemsShortage'
WHERE( Command = 'MagoNet.Magazzino.ArticoliSottoscorta' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Sales.ReturnsToSupplier'
WHERE( Command = 'MagoNet.Vendite.ListaResiAFornitore' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.SalesCollectionJournal'
WHERE( Command = 'MagoNet.Contabilita_ro.SalesCollectionSummaryJournal' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.AP_AR_Plus.ShortMediumTermPayables'
WHERE( Command = 'MagoNet.PartiteAvanzato.DebitiMedioTermine' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Manufacturing.ProductionDevelopment-Grpd'
WHERE( Command = 'MagoNet.Produzione.AvanzamentoProduzioneAccorpatoPerMateriale' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.CostAccounting.JobsMonthly-YTD'
WHERE( Command = 'MagoNet.Analitica.BudgetMeseCommesseRiepProg' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.SalesJournal'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVAVendite' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting_RO.AccountsLedgerCards'
WHERE( Command = 'MagoNet.Contabilita_ro.LedgerCardsAccounts' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.LotsSerials.EntriesByLotAndStorage'
WHERE( Command = 'MagoNet.LottiMatricole.MovimentiPerLottoDeposito' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.AccountingReasons'
WHERE( Command = 'MagoNet.Contabilita.CausaliContabili' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Payees.WHoldingTaxToBePaid'
WHERE( Command = 'MagoNet.Percipienti.RitenuteDaVersare' AND 
Type = 2
)
END
BEGIN 
UPDATE 
MSD_ScheduledTasks SET Command = 'ERP.Accounting.RetailSalesToBeDistJournal'
WHERE( Command = 'MagoNet.Contabilita.RegistroIVACorrispettiviVentilazione' AND 
Type = 2
)
END
GO