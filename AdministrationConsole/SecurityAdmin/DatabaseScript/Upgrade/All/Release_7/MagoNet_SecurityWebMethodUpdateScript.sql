
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 
	DECLARE @webMethodTypeId as integer 
	SET @webMethodTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 3)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.StepSearch_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CCercaFase_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrdersPrint_Run'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.StampaMandati_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_Reference'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaTestoRiferimento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotation_TaxSummaryGroupedLine'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor_RitornaRigheAccorpateRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.AddOnsAccounting.CDGLJournal_UpdateJE'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.Contabilita_ROAddOnsContabilita.CDGeneralLedger_UpdateAccounting' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_TaxSummaryGroupedLine'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaRigheAccorpateRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.F24Data'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.GetDatiPerF24' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.CurrentClosingDate'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.DataChiusuraEsercizio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotation_TaxSummaryLine'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli_RitornaRigaRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.JobsBalances_MonthlyBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCommesse_SaldiMensiliCommessa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.CostCentersBalances_ProgressBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCentriCosto_SaldoProgressivoCentro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseeOrd_NotesLine'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_RitornaRigaNoteCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_Create'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_Dispose'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.InventoryValuation_ReceiptIssueReason'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CMovimentiMagazzinoFunction_CausaleMovimentoCaricoScarico' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_FIFODomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreFifoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_BalanceFromEntries'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_SaldoDaRegistrazioni' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.JobsBalances_QtyBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCommesse_SaldoQtaCommessa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Components.MultiStorageValuation_SoldToDateByStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoComponents.CMultiDepositoWoormFunction_VendutoAllaDataPerDeposito' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.FormatLiteral'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.FormatLiteral' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_TaxSummaryInDocCurrGroupedLine'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaRigheAccorpateRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.FixedAssetsNumberingManager_Create'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.NumeratoriCespitiManager_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_Invoiced'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_GetBudgetFatturato' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.IsYearBeforeEuro'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.IsAnnoBeforeEuro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotation_Dispose'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_ConvertWithInvRsnFixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiTraDiviseConFixingInBaseAllaCausale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_FeeLineGroupedByReason'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_RitornaRigaAccorpataPerCausale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_Create'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_PrepareQueryOnFees'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_PreparaTestaParcelleRaINPS' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LastCost'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreUltimoCosto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.StepSearch_Create'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CCercaFase_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.SlipWithDifferentPymtTerm'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistintaPagamentiDiversi' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MO_Create'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.DOdP_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.Form770Print_Values'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloRitenuteParcelleMng_RitornaImportiPer770' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd_TaxSummaryInDocCurrGroupedLine'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_RitornaRigheAccorpateRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.Form770Print_Create'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloRitenuteParcelleMng_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_ConvertWithJEFixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiTraDiviseConFixingInBaseAlMovPN' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Components.MultiStorageValuation_OnHandToDateByStorage'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoComponents.CMultiDepositoWoormFunction_DisponibilitaAllaDataPerDeposito' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.EUAnnotationTaxJournalNumberingCheck'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.ControllaNumerazioneAnnotCEE' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.CostCentersBalances_Dispose'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCentriCosto_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_Dispose'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.LastTaxPaymentDate'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.DataUltVersIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.JobsBalances_Create'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCommesse_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.TaxIdNoCheck'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.ControlloPartitaIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_NotesLine'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaRigaNoteCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LIFODomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreLifoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_AverageCostDomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreCostoMedioDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.IssueDate'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DataEmissione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionDevelopment_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AvanzamentoProduzione_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_ActualOrdered'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_GetConsuntivoOrdinato' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_SetVerbose'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_SetVerbose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_AverageUncertainCost'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreCostoMedioIncerto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_OpenBalanceWithNature'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_SaldoAperturaPerNatura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_BalanceInCurr'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_SaldoInDivisa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.CDocReferences_GetDocReferences'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.CRiferimentiDocumento_LeggiRiferimentiDoc' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Components.ContactsNumberingManager_Dispose'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiComponents.ContattiCounterManagerObj_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.DDSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaRID' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotation_TaxSummaryInDocCurrLine'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli_RitornaRigaRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.IntraRoundingNoDecimal'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.ArrLireIntra' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Components.CountersManager_Dispose'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngComponents.NumNonFiscManager_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Documents.LIFOFIFOValuationToDate_Type'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.DValorizzazioneAllaData_GetTipoValorizzazione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProductionPlanGeneration_FillTableProdPlanGeneration'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.GenerazionePianoProduzione_RiempiTableGenerazionePianoProduzione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_SoldToDate'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_VendutoAllaData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_Exist'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_Exist' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.AcceleratedPerc'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.AliquotaAnticipata' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_Dispose'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.PrivacyStatementLetters_Run'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.StampaLettereConsenso_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.CustSuppDetailedClosing'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.DettaglioChiusureCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_FIFOUncertainDomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreFifoIncertoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Components.CalculateItemAvailability_CalculateItemAvailabilityByType'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseComponents.CCalcolaDisponibilitaArticolo_CalcolaDisponibilitaArticoloPerTipo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_Create'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Components.CalculateItemAvailability_CalculateItemAvailability'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseComponents.CCalcolaDisponibilitaArticolo_CalcolaDisponibilitaArticolo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.InventoryValuation_SaleReason'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CMovimentiMagazzinoFunction_CausaleMovimentoVendita' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxSummaryLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigaRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.TaxJournalDefinitivePrint'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.RegistroStampatoSuBollato' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.SegmentLength'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.LunghezzaSegmento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd_Dispose'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd_TaxSummaryInDocCurrLine'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_RitornaRigaRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_MonthlyBalance'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldiMensili' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.PrivacyStatementLetters_CustSuppUpdate'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.StampaLettereConsenso_AggiornaConsensoCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.PreviousClosingDate'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.DataChiusuraEsercizioPrecedente' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.GetNativeBarCodeType'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.GetNativeBarCodeType' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.TaxJournalProgr'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.GetPrimoNr' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_OpenBalance'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoApertura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_ActualCreditNote'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_GetConsuntivoNoteCredito' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.FiscalCodeCheck'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.ControlloCodiceFiscale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.F24PaymentDay'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.GiornoVersamentoF24' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Components.MultiStorageValuation_Dispose'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoComponents.CMultiDepositoWoormFunction_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_TaxPymtScheduleLine'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaRigaScadenze' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_RoundingCurr'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ArrImportoDivisa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_AcceleratedDepr'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_QuotaAnticipata' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_Balance'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_Saldo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ReorderMaterialsToProduction_FillMORequirementTable'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.RiordinoProduzioneMancanti_RiempiTableODPMancanti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.ENASARCOManager_Dispose'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloEnasarcoMng_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_BalanceToDate'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_SaldoAllaData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_Dispose'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_Create'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_BalanceToMonth'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_SaldoMese' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_Dispose'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.DistributionTaxCode'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.GetCodiceIVAVentDefault' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotation_TaxSummaryGroupedLine'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli_RitornaRigheAccorpateRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.JobsBalances_Balance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCommesse_SaldoCommessa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_Create'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProductionPlanGeneration_Dispose'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.GenerazionePianoProduzione_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.ClosedInstallmentPresentable'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.ConsideraRateChiuse' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_Create'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.ContributionsTransferAtPymt'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.GcContributiAlPagamento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.GLJournalUpdate'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.AggiornaDatiLibroGiornale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.FixingLireEuro'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.FixingLireEuro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Components.CalculateItemAvailability_Dispose'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseComponents.CCalcolaDisponibilitaArticolo_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd_Reference'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_RitornaTestoRiferimento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxPymtScheduleGroupedLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigheAccorpateScadenze' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrdersPrint_UpdateSlips'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.StampaMandati_AggiornaStampeDistinte' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_TaxPymtScheduleGroupedLine'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaRigheAccorpateScadenze' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_NARounding'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ArrNAImportoDivisaBase' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.MOEsplosion_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CEsplosioneOdP_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.MOEsplosion_Create'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CEsplosioneOdP_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_Balance'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_Saldo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_ActualInvoiced'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_GetConsuntivoFatturato' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotation_Dispose'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.Offset_Dispose'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.Contropartita_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_FinancialDepr'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_QuotaFinanziario' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.MOEsplosion_JobExplosion'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CEsplosioneOdP_EsplosioneCommesse' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_FiscalDepr'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_QuotaFiscale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_TaxSummaryInDocCurrLine'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaRigaRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotation_Create'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_Update'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_AggiornaBudget' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_OnHandToDate'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_DisponibilitaAllaData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PrintedUpdate'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.AggiornaStampato' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_ADDays'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_GetNrGiorniGodimento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.FiscalYearDescription'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.DescrizioneEsercizio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_ConvertWithSaleFixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiTraDiviseConFixingVendite' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.FiscalYearsCheck'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.ConfrontaEsercizi' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionDevelopment_GetMOComponents'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AvanzamentoProduzione_GetDatiListaMater' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.FixedAssetsNumberingManager_NextNo'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.NumeratoriCespitiManager_OttieniProssimoNumero' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.AuthorizationPeriod'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.DurataFinanziario' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotation_TaxSummaryLine'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor_RitornaRigaRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.AllPymtsPresentation'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.PresentaTuttiPagamenti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.Form770Print_Dispose'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloRitenuteParcelleMng_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BankTransferSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaBonifici' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.UoMConversion_Convert'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ConversioniUnMis_ConvertiUnMis' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_BookInvToDate'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_GiacenzaAllaData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LastUncertainCostDomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreUltimoCostoIncertoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_CreditInitial'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_AvereInizioPeriodo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_Create'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_WittholdingTaxPeriod'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_RitornaPeriodoImportoDaVersareRitenute' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_TaxSummaryInDocCurrLine'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaRigaRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxPymtScheduleLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigaScadenze' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_OtherYearsDays'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_GetNrGiorniAltriEsercizi' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotation_TaxSummaryInDocCurrLine'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor_RitornaRigaRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_OpenBalanceWithNature'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoAperturaPerNatura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.GLJournalDefinitivePrint'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.StampatoSuBollato' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.GetTaxExigForVTFrame'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.RitornaIvaEsigibileQuadroVT' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Components.CountersManager_NextNo'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngComponents.NumNonFiscManager_OttieniProssimoNumero' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.AddOnsAccounting.SalesJournalTotals'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.Contabilita_ROAddOnsContabilita.GetSalesRegisterTotals' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_SecondLastCostDomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValorePenultimoCostoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.ClosingDateOfFiscalYear'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.DataChiusuraNrEsercizio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_SetNature'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_ImpostaNatura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.TaxJournalDescription'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.GetDescriRegistroIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_UpdatePymtDataOnGroupedFees'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_AggiornaDatiPagamentoSuParcelleAccorpate' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProducibilityAnalysis_GetToExplodeQty'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.AnalisiProducibilita_GetQtaDaEsplodere' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_ConvertDomesticCurrencies'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiTraDiviseLocali' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Components.ConaiUnitByItem'
WHERE( NameSpace = 'MagoNet.Conai.ConaiComponents.ConaiUnitarioPerArticolo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_Dispose'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_Currency'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_Divisa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ReorderMaterialsToSupplier_GetLineMO'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.RiordinoMaterialiMancanti_GetRigaOrdMancanti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxSummaryLineWithPerc'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RigaIVAConAliquota' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotation_NotesLine'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli_RitornaRigaNoteCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.PrivacyStatementLetters_Dispose'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.StampaLettereConsenso_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.CDocReferences_Create'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.CRiferimentiDocumento_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Components.ItemPriceFromPriceListToDate'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziComponents.PrezzoListinoArticoloPerData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.TaxJournalPrint_Dispose'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRegistriIVA_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.CurrentFiscalYear'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.EsercizioCorrente' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.ReorderingFromSupp_Dispose'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.RiordinoFornitori_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PANSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaMAV' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Services.PurchasesSettings'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiServices.PostazioneAcquisti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_Total'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_Totale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.IntraTotalInCurrencyRounding'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.ArrTotValutaIntra' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotation_NotesLine'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor_RitornaRigaNoteCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.ENASARCOManager_Minimum'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloEnasarcoMng_DeterminaMinimoDaVersare' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Services.AccountingSettings'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaServices.PostazioneContabilita' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LIFO'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreLifo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_FIFO'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreFifo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_TaxSummaryInDocCurrGroupedLine'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaRigheAccorpateRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_Dispose'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxSummaryLineBuffetti'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RigheAccorpateRiepIVABuffetti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.BaseCurrency'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.RitornaDivisaBase' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.PreviousOpeningDate'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.DataAperturaEsercizioPrecedente' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_NotesLine'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaRigaNoteCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd_Create'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Components.InventoryByStorage_Dispose'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoComponents.CMovimentiMagazzinoPerDepositoFunction_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_ComponentsLine'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaRigheComponenti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Documents.ProRataPrint_Dispose'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.RegimiIVASpecialiDocuments.StampaProRata_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ReorderMaterialsToSupplier_Run'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.RiordinoMaterialiMancanti_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.MOEsplosion_MOExplosion'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CEsplosioneOdP_EsplosioneOdP' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Services.ConaiSettings'
WHERE( NameSpace = 'MagoNet.Conai.ConaiServices.PostazioneConai' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LastEntryNotOverDate'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_UltimoMovimentoNonSuperioreData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_ConvertWithActualFixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiTraDiviseConFixingReale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.TaxJournalPrint_UpdateTotals'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRegistriIVA_AggiornaTotSingoliRegIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.InventoryValuation_BookInvFromEntries'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CMovimentiMagazzinoFunction_GiacenzaArticoloInBaseAiMovimenti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PrintedSlipUpdate'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.AggiornaStampatoDistinte' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.StepSearch_PreviousStepSearch'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CercaFase_CercaFasePrecedente' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ReorderMaterialsToProduction_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.RiordinoProduzioneMancanti_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.AddOnsAccounting.TaxParameters'
WHERE( NameSpace = 'MagoNet.Contabilita_ro.Contabilita_ROAddOnsContabilita.GetVATParameters' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.CostCentersBalances_Balance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCentriCosto_SaldoCentro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.ENASARCOManager_Sum'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloEnasarcoMng_SommaValoreEnasarco' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Services.SalesSettings'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeServices.PostazioneVendite' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_ActualDebitNote'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_GetConsuntivoNoteDebito' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_BalanceInBaseCurr'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_SaldoInDivisaBase' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Documents.CustomerQuotation_TaxSummaryInDocCurrGroupedLine'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDocuments.OffCli_RitornaRigheAccorpateRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.JobsBalances_Dispose'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCommesse_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.SlipProg'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.ProgressivoDistinta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_ComponentsLineUpperBound'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaLimiteSupRigheComponenti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_Reference'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaTestoRiferimento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_Convert'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiTraDivise' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.CDocReferences_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.CRiferimentiDocumento_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotation_TaxSummaryInDocCurrGroupedLine'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor_RitornaRigheAccorpateRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.ReorderingFromSupp_Supplier'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.RiordinoFornitori_GetNomeFornitore' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_PeriodCalculation'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_CalcolaBudgetDiPeriodo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Components.ReqForPymtMaxLevel'
WHERE( NameSpace = 'MagoNet.Partite.PartiteComponents.GetNrMaxLivelliSolleciti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.IsPeriodBeforeEuro'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.IsPeriodoBeforeEuro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.InventoryValuation_Dispose'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CMovimentiMagazzinoFunction_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MO_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.DOdP_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionDevelopment_Run'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AvanzamentoProduzione_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_Calculation'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_CalcolaRateoRisconto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_TaxSummaryGroupedLine'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaRigheAccorpateRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Components.ContactsNumberingManager_Create'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiComponents.ContattiCounterManagerObj_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_AverageCost'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreCostoMedio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.ENASARCOManager_Create'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloEnasarcoMng_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_Create'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_SetSimulation'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_ImpostaSimulazione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.CostCentersBalances_Create'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCentriCosto_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ReorderMaterialsToSupplier_FillTableMO'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.RiordinoMaterialiMancanti_RiempiTableODPMancanti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.QuarterlyTaxSettlement'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.IVATrimestrale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.InventoryValuation_Action'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CMovimentiMagazzinoFunction_GetAzioneMagazzino' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.ReorderingFromSupp_Run'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.RiordinoFornitori_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ReorderMaterialsToProduction_GetMORequirementLine'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.RiordinoProduzioneMancanti_GetRigaOrdProdMancanti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxSummaryInDocCurrGroupedLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigheAccorpateRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.CostCentersBalances_QtyBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCentriCosto_SaldoQtaCentro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_LostDepr'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_QuotaPersa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_StandardUncertainCost'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreCostoStandardIncerto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Components.RequestForPaymentCustUpdate'
WHERE( NameSpace = 'MagoNet.Partite.PartiteComponents.AggiornaSollecitiCliente' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.GLJournalPreviousDebitCredit'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.GetDareAverePrecedente' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ReorderMaterialsToProduction_Run'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.RiordinoProduzioneMancanti_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Documents.LIFOFIFOValuationToDate_EndDate'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDocuments.DValorizzazioneAllaData_GetDataFine' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.Offset_Create'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.Contropartita_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.BalancePerc'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.AliquotaBilancio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.CashOrderSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaRIBA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Documents.ProRataPrint_UpdatePerc'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.RegimiIVASpecialiDocuments.StampaProRata_AggiornaPercDefinitiva' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Services.PymtScheduleSettings'
WHERE( NameSpace = 'MagoNet.Partite.PartiteServices.PostazionePartite' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_OtherYearsRate'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_GetQuotaAltriEsercizi' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_Nature'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_GetNaturaRateoRisconto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_ConvertToDomesticCurrency'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiDaDivisaADivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_NetBookValueDepr'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_QuotaSulResiduo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_StandardCost'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreCostoStandard' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.MOImplosion_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CImplosioneOdP_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.IssueBank'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.BancaEmissione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_TaxSummaryLineWithPerc'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RigaIVAConAliquota' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Components.CalculateItemAvailability_Create'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseComponents.CCalcolaDisponibilitaArticolo_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.FiscalYearsOfDate'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.EsercizioDellaData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.TaxPeriod'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.PeriodoIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Documents.SupplierQuotation_Create'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDocuments.OffFor_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.CurrentOpeningDate'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.DataAperturaEsercizio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_Create1'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_Create1' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.TaxPerc'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.PercInteresseIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Services.InventorySettings'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoServices.PostazioneMagazzino' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.F24TaxDebit'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.DebitoIVAF24' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.IntraRounding'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.ArrotondamentoIntra' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.CostCentersBalances_MonthlyBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCentriCosto_SaldiMensiliCentro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.TaxJournalPrint_UpdateArray'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRegistriIVA_AggiornaArrayDatiRegistriStampati' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LastUncertainCost'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreUltimoCostoIncerto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_DebitToDate'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_DareAllaData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_BalanceWithNature'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoPerNatura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_OpenDebitCreditWithNature'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoDareAverePerNatura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_BalanceByAccrualDate'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_SaldoPerCompetenza' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Services.ItemsSettings'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliServices.PostazioneArticoli' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.UoMConversion_Dispose'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ConversioniUnMis_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_TaxRounding'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ArrImpostaDivisaBase' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProductionPlanGeneration_Run'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.GenerazionePianoProduzione_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_ComonentsLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigheComponenti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_UpdateINPSPymtDataOnFees'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_AggiornaDatiPagamentoINPSSuParcelle' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.UoMConversion_Create'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ConversioniUnMis_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_BalanceWithNature'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_SaldoAllaDataPerNatura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Components.InventoryByStorage_Create'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoComponents.CMovimentiMagazzinoPerDepositoFunction_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionLotEdit_Dispose'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.DModificaLotti_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_Clear'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_AzzeraBudget' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Components.ContactsNumberingManager_NextNo'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiComponents.ContattiCounterManagerObj_OttieniProssimoNumero' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_SetRounding'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_ImpostaArrotonda' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PresentationDate'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DataPresentazione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.JobsBalances_ProgressBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCommesse_SaldoProgressivoCommessa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_BalanceInBaseCurr'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoInDivisaBase' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Components.RequestForPaymentFile'
WHERE( NameSpace = 'MagoNet.Partite.PartiteComponents.GetTestiDiSollecito' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.FiscalPerc'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.AliquotaFiscale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_SecondLastCost'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValorePenultimoCosto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_Create'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_Dispose'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PresentationBank'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.BancaPresentazione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Core.Services.MastersAddOnFly'
WHERE( NameSpace = 'MagoNet.Core.CoreServices.AnagraficheRidotte' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.CostCentersBalances_FiscalYearBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCentriCosto_SaldiEsercizioCentro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.MO_MOImplosion'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.DOdP_ImplosioneOdP' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionProgressAnalysis'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AnalisiStatoProduzione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_OpenBalance'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_SaldoApertura' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionDevelopment_MOComponentToPrint'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AvanzamentoProduzione_ComponenteDaStampare' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_NotesLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigaNoteCliFor' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AccrualsDeferrals.Components.AccrualsDeferrals_Days'
WHERE( NameSpace = 'MagoNet.RateiRisconti.RateiRiscontiComponents.CRateiRisconti_GetNrGiorni' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.IsCompanyInEuro'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.IsAziendaConvertitaEuro' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Components.InventoryByStorage_ReceiptIssueAction'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoComponents.CMovimentiMagazzinoPerDepositoFunction_AzioneDepositoCaricoScarico' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_AverageUncertainCostDomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreCostoMedioIncertoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionLotEdit_Create'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.DModificaLotti_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd_TaxSummaryLine'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_RitornaRigaRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_TaxSummaryLine'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaRigaRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProducibilityAnalysis_Run'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.AnalisiProducibilita_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Components.IDsManager_Dispose'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngComponents.IDManager_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_Create'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_INPSPeriod'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_RitornaPeriodoImportoInpsProfessionisti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Documents.SaleOrd_Reference'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDocuments.OrdCli_RitornaTestoRiferimento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_TaxRoundingCurr'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ArrImpostaDivisa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxSummaryGroupedLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigheAccorpateRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Components.ReclassifiedAccountsBalances_SetCurrency'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioComponents.SintesiPianoContiRiclassificato_ImpostaDivisa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LastCostDomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreUltimoCostoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBalances_CreditToDate'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiClientiFornitori_AvereAllaData' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.FixedAssetsNumberingManager_Dispose'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.NumeratoriCespitiManager_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_Ordered'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_GetBudgetOrdinato' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.IntraSuppUnitRounding'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.ArrUnSupplIntra' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_ComponentsLineUpperBound'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaLimiteSupRigheComponenti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Components.JobsBalances_FiscalYearBalance'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaComponents.SintesiCommesse_SaldiEsercizioCommessa' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Documents.PurchaseOrd_TaxSummaryGroupedLine'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDocuments.OrdFor_RitornaRigheAccorpateRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.BillOExchangeSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.DistintaCambiali' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_ReadEntriesForValuation'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_LeggiMovimentiPerValorizzazione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LIFOUncertainDomesticCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreLifoIncertoDivisaLocale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.BatchDocuments.TaxJournalPrint_Run'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaBatchDocuments.StampaRegistriIVA_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Components.CountersManager_Create'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngComponents.NumNonFiscManager_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.AccountName'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.NomeConto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_DebitCredit'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoDareAvere' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Documents.ProRataPrint_Run'
WHERE( NameSpace = 'MagoNet.RegimiIvaSpeciali.RegimiIVASpecialiDocuments.StampaProRata_Run' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Documents.ProductionDevelopment_GetMOFeasible'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDocuments.AvanzamentoProduzione_GetOdpFattibile' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxSummaryInDocCurrLine'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigaRiepIVAValuta' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_BalanceFromEntries'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoDaRegistrazioni' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.ENASARCOManager_FIRRSinglePartner'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloEnasarcoMng_CalcoloFIRRSingoloSocio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.OpeningDateOfFiscalYear'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.DataAperturaNrEsercizio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Documents.PurchaseDoc_TaxSummaryLine'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDocuments.DDocAcquisto_RitornaRigaRiepIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.ENASARCOManager_Update'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloEnasarcoMng_AggiornaVersatoEnasarco' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_ConvertWithPurchaseFixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ConvertiTraDiviseConFixingAcquisti' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Documents.PaymentOrdersPrint_Dispose'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDocuments.StampaMandati_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.Offset_Data'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.Contropartita_DatiContropartita' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_FIFOUncertain'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreFifoIncerto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.LifePeriod'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.DurataFiscale' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Components.Depreciation_BalanceDepr'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiComponents.Ammortamento_QuotaBilancio' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_ClearQueryOnFees'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_PulisciTestaParcelleRaINPS' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProducibilityAnalysis_GetInducedRequirementQty'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.AnalisiProducibilita_GetFabbisognoIndotto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Components.IDsManager_Create'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngComponents.IDManager_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Components.F24Print_Dispose'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiComponents.CalcoloDatiPerF24Mng_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProductionPlanGeneration_GetDBTProductionPlanGeneration'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.GenerazionePianoProduzione_GetDatiDBTGenerazionePianoProduzione' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.CurrencyManager_Rounding'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.CCurrencyManager_ArrImportoDivisaBase' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.TaxCode'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.CodiceTributoIVA' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.GLJournalProgr'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.GetNrProgr' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_BalanceByAccrualDate'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_SaldoPerCompetenza' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProducibilityAnalysis_GetProdPlanQty'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.AnalisiProducibilita_GetQtaSchema' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.TaxIdNoSecondCheck'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.ControlloPartitaIVASecondario' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Components.TaxJournalNumberingCheck'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaComponents.ControllaNumerazioneRegistri' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Components.MultiStorageValuation_Create'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoComponents.CMultiDepositoWoormFunction_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.IntraNetMassRounding'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.ArrMassaNettaIntra' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Documents.SaleDoc_TaxPymtScheduleLineWithPymtTerm'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDocuments.DDocVendita_RitornaRigaScadenzeConTipoPagamento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.SegmentName'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.NomeSegmento' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Documents.ProducibilityAnalysis_Dispose'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDocuments.AnalisiProducibilita_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.InventoryValuation_Create'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CMovimentiMagazzinoFunction_Create' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Components.ChartOfAccountsBalances_DebitInitial'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiComponents.SintesiPianoConti_DareInizioPeriodo' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsValuation_LIFOUncertain'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.CArticoliWoormFunction_ValoreLifoIncerto' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Components.IDsManager_NextId'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngComponents.IDManager_GetNextID' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Components.CustSuppBudget_Dispose'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriComponents.SintesiBudgetCliFor_Dispose' AND 
TypeId = @webMethodTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Components.MOImplosion_Create'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneComponents.CImplosioneOdP_Create' AND 
TypeId = @webMethodTypeId
)
END
GO