
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
	DECLARE @tableTypeId as integer 
	SET @tableTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 10)
	DECLARE @viewTypeId as integer 
	SET @viewTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type = 13)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_OwnedCompaniesBalances'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ControllateBilanci' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_CommissionEntriesDetail'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_MovimentiAgentiRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_ItemsFIFO'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ArticoliFifo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_SmartCodeParameters'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_ParametriCodiceParlante' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SalesStatisticsDetailed'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_StatisticheVenditeDett' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_CommodityCtgSuppliers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_CatMerceologicheFornitori' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Dbl.MA_CRPSimulations'
WHERE( NameSpace = 'MagoNet.CRP.CRPDbl.MN_SimulazioniCRP' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.Dbl.MA_CreditCards'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDbl.MN_CartaCredito' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_BillOfMaterialsRouting'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_DistintaBaseCiclo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_SalesPeopleBalances'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_AgentiSaldi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixedAssetsFinancial'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_CespitiFinanziario' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SalesOrdsDefaults'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_DefaultOrdCli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_CommissionCtg'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_CatProvv' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocReferences'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquistoRiferimenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.MA_Storages'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDbl.MN_Depositi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_TmpProducibilityAnalysis'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_TmpAnalisiProducibile' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchasesDefaults'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DefaultAcquisti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.MA_NumberParameters'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.MN_ParametriNumeratori' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemTypes'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_TipoArticolo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrd'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_DutyCodes'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_CodiciTributo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Dbl.MA_SuppQuotasDetail'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDbl.MN_OffForRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Dbl.MA_SuppQuotasSummary'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDbl.MN_OffForCodaContabile' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SalesOrdParameters'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_ParametriOrdCli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Dbl.MA_ReceiptsInvEntry'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDbl.MA_ViewReceipts' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.MA_ConfigurationsQnA'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.MN_ConfiguratoreDomRisp' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_OwnerCompaniesSendings'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ControllantiInvii' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdReferences'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliRiferimenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.MA_StubBookNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.MN_NumeratoriBolle' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_F24Defaults'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_DefaultF24' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Dbl.MA_InventoryEntriesDetail'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDbl.MN_MovimentiMagazzinoRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Dbl.MA_LoadingDetailSim'
WHERE( NameSpace = 'MagoNet.CRP.CRPDbl.MN_SimComposizCarico' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppSupplierOptions'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForAccessorioFornitore' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_OwnerCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_Controllanti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.Dbl.MA_AccountingParameters'
WHERE( NameSpace = 'MagoNet.Contabilita_GR.Contabilita_GRDbl.MN_ParametriContabilita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixAssetsReasons'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_CausaliCespiti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdPymtSched'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdForScadenze' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_FeeTemplatesDetail'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ModelliParcelleRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_TmpPapersPrint'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_TmpStampaDocumenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Dbl.MA_SuppQuotas'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDbl.MN_OffFor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Dbl.MA_CustQuotasShipping'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDbl.MN_OffCliSpedizione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_UnitsOfMeasure'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_UnitaMisura' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.Dbl.MA_PaymentTermsPercInstall'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDbl.MN_CondizioniPagamentoPerc' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_TmpReorderingFromSuppRef'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_TmpRiordinoFornitoriRif' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccountingParametersPrint'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_ParametriContabilitaStampe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_BillOfMaterialsQnA'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_DistintaBaseDomRisp' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_AvailabilityAnalysis'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_ProspettoDisponibilita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChargePolicies.Dbl.MA_ChargePoliciesShippings'
WHERE( NameSpace = 'MagoNet.PoliticheSpese.PoliticheSpeseDbl.MN_PoliticheSpeseSpedizioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.MA_InventoryReasons'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.MN_CausaliMagazzino' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_InvoiceTypes'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_TipoFattura' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.MA_CustMaterialsExemption'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MN_EsenzioniClientiMateriali' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_Operations'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_Operazioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_Company'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_Azienda' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_PayeesParametersAllowance'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ParametriPercipIndennita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.MA_MRPParameters'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.MN_ParametriMRP' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_ConsBalSheetParametersAcc'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ParametriBilConContiDaEscl' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.MA_GoodLocationDefaults'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.MN_DefaultUbicazioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_SmartCodesStructure'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_CodiceParlanteStruttura' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_Mail'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_Mail' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_TeamsBreakdowns'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_SquadreNonDisponibilita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_CommodityCtgCustomers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_CatMerceologicheClienti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.MA_ItemsLocations'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.MN_ArticoliUbicazioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_SubcontractingDoc'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_DocumentiCLav' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.MA_Carriers'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.MN_Vettori' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_JournalEntriesTax'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_PrimaNotaIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocTaxSummary'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquistoRiepilogoIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.MA_InventoryEntries'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.MN_MovimentiMagazzino' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_ManufacturingParameters'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_ParametriProduzione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppOutstandings'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForInsoluti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_SalesPeopleBalanceFIRR'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_AgentiSaldiFIRR' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChargePolicies.Dbl.MA_ChargePolicies'
WHERE( NameSpace = 'MagoNet.PoliticheSpese.PoliticheSpeseDbl.MN_PoliticheSpese' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdNotes'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdForNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocNotes'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Dbl.MA_CustQuotasTaxSummary'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDbl.MN_OffCliRiepilogoIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixAssetLocations'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_UbicazioniCespiti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_Departments'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_Reparti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleParameters'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_ParametriVendite' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.Dbl.MA_PaymentTerms'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDbl.MN_CondizioniPagamento' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.MA_TmpAvailabilityForecast'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.MN_TmpPrevisioneAndamGiac' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ProductCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_CatProdotto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.MA_Ports'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.MN_Porti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Dbl.MA_CustQuotasSummary'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDbl.MN_OffCliCodaContabile' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_JournalEntries'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_PrimaNota' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemsSubstitute'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ArticoliEquivalenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.MA_AdditionalCharges'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.MN_OneriAccessori' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.MA_ExtAccTemplateDetail'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.MN_ExtAccountingTemplateDetail' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Dbl.MA_VariantsRouting'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDbl.MN_VariantiCiclo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.XGate.Dbl.MA_XGateParameters'
WHERE( NameSpace = 'MagoNet.XGate.XGateDbl.MN_ParametriXGate' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_TransportLang'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_ModiTrasportoLingue' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_TmpProdPlanGenerationRef'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_TmpGenPianoProdRif' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemCustomers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_CliArticolo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_PortsLang'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_PortiLingue' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierPricesAdj.Dbl.MA_SupplierPricesAdjConfig'
WHERE( NameSpace = 'MagoNet.AllineamentoPrezziFornitore.AllineamentoPrezziFornitoreDbl.MN_ConfigurazioneAPF' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemsGoodsData'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ArticoliDatiMerce' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_PayeesParametersFIRROne'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ParametriPercipFIRRMono' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocDetail'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquistoRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_OwnedCompaniesJE'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ControllateRegPNota' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_MOHierarchies'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_OdPGerarchie' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_CompanyYears'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_AziendaAnni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_TmpProdPlanGeneration'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_TmpGenPianoProd' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_GLJournalTotals'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_TotaliLibroGiornale' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_GR.Dbl.MA_CustSupp'
WHERE( NameSpace = 'MagoNet.Contabilita_GR.Contabilita_GRDbl.MN_CliFor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ProductLines.Dbl.MA_ProductLines'
WHERE( NameSpace = 'MagoNet.LineeDiProdotto.LineeDiProdottoDbl.MN_LineeProdotto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.MA_Intra'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.MN_Intra' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchasesParameters'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_ParametriAcquisti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ProductLines.Dbl.MA_ProductLinesBalances'
WHERE( NameSpace = 'MagoNet.LineeDiProdotto.LineeDiProdottoDbl.MN_SaldiLineeProdotto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ProductSubCtgDefaults'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_DefaultSottoCatProdotto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Dbl.MA_SuppQuotasNote'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDbl.MN_OffForNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_CompanyParameters'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_ParametriAzienda' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppParameters'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_ParametriCliFor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.MA_ConfigurationsAnswers'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.MN_ConfiguratoreRisposte' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemsPurchaseBarCode'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ArticoliBarCodeAcquisto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixAssetsParameters'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_ParametriCespiti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.MA_Items'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.MN_Articoli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.MA_ChartOfAccountsCostAccTpl'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.MN_PianoContiModelliAnalitici' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_ItemsLanguageDescri'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_ArticoliDescriLingua' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Dbl.MA_Jobs'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDbl.MN_Commesse' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_ConsBalSheetParameters'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ParametriBilCon' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_CompanyTaxDeclaration'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_AziendaDatiIVAPeriodica' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.MA_AdditionalChargesRef'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.MN_OneriAccessoriRiferimenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.MA_ChartOfAccountParameters'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.MN_ParametriPianoConti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_ItemSuppliersPriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_ForArticoloListini' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleParametersWithholdTax'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_ParametriVenditeRitAcc' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppNaturalPerson'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForDatiPersonaFisica' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Dbl.MA_BalanceReclassDetail'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioDbl.MN_RiclassPianoContiRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InvoiceMng.Dbl.MA_DocumentParameters'
WHERE( NameSpace = 'MagoNet.InvoiceMng.InvoiceMngDbl.MN_ParametriDocumenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_MOSteps'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_OdPFasi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_TmpMOExplosion'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_TmpEsplosioneOdP' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_BOMParameters'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_ParametriDistintaBase' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_CompanyGroups'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_GruppoAziende' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_Drawings'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_Disegni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Dbl.MA_BalanceReclass'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioDbl.MN_RiclassPianoConti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ItemCodes.Dbl.MA_ItemParameters'
WHERE( NameSpace = 'MagoNet.VTSZSZJ.VTSZSZJDbl.MN_ParametriArticoli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_OwnedCompaniesSendings'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ControllateInvii' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_ItemCustomersPriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_CliArticoloListini' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_ItemsMonthlyBalances'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ArticoliSaldiMese' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDoc'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVendita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_ConsolidTemplatesDetail'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ModelliConsolidamentoRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppBalances'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForSaldi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.MA_Currencies'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.MN_Divise' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_SalesPeople'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_Agenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdSummary'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdForCodaContabile' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemsManufacturingData'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ArticoliProduzione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_TmpBOMImplosions'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_TmpImplosioneDistinta' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_Languages'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_Lingue' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.MA_CollectionParameters'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.MN_ParametriIncassi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocTaxSummary'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaRiepilogoIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_PurchasesValuesDefaults'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_DefaultValUnitariAcquisto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocCorrTaxSummary'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MA_PurchaseDocCorrTaxSummary' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccountingReasons'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_CausaliContabili' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.MA_StubBooks'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.MN_Bollettari' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.MA_CostCenterGroups'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.MN_GruppiCentriCosto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_FeesDetails'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ParcelleRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrd'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdFor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_ProductionPlansReferences'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_PianoProdRiferimenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccTemplatesGLDetail'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_ModelliContabiliRigheLG' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_JournalEntriesForecast'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_PrimaNotaPrevisionale' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccountingParameters'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_ParametriContabilita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_ConsBalSheets'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_BilanciConsolidati' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_SegmentsComb'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_SegmentiPossib' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Dbl.MA_Contacts'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDbl.MN_Contatti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocShipping'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquistoSpedizione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Dbl.MA_LotSerialParameters'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDbl.MN_ParametriLottiMatricole' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Dbl.MA_ReceiptsBatch'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDbl.MN_Carichi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocPymtSched'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaScadenze' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdShipping'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdForSpedizione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Dbl.MA_Banks'
WHERE( NameSpace = 'MagoNet.Banche.BancheDbl.MN_Banche' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.MA_InvAccTransParameters'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.MN_InvAccTransParameters' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_CompanyBranches'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_AziendaSedi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccountingParametersTax'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_ParametriContabilitaIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_TaxCodes'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_IVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_Titles'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_Titoli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemSuppliersOperations'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ForArticoloOperazioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Dbl.MA_Variants'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDbl.MN_Varianti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Dbl.MA_WCHoursSim'
WHERE( NameSpace = 'MagoNet.CRP.CRPDbl.MN_SimOreCdl' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemsKit'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ArticoliKit' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.MA_ConfigurationsQuestions'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.MN_ConfiguratoreDomande' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppNotes'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_WorkCentersBreakdown'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_CdLNonDisponibilita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_MO'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_OdP' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.MA_ItemsMaterials'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MN_ArticoliMateriali' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.MA_Packages'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.MN_Imballi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_SalesPeoplePartners'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_AgentiSoci' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ProducersCategories'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ProduttoriCategorie' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.MA_TmpMRPProposals'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.MN_TmpSuggerimentiMRP' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_TmpSaleOrdFulfilment'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_TmpEvasioneOrdCli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.MA_GoodLocations'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.MN_UbicazioniMerce' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_JournalEntriesGLDetail'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_PrimaNotaRigheLG' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.Dbl.MA_SubcontratorAnalysis'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroDbl.MN_AnalisiTerzisti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_TmpReorderingFromSupp'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_TmpRiordinoFornitori' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_OwnerCompaniesBalances'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ControllantiBilanci' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_TmpFIFOProgress'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_TmpFormazioneFifo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_TaxDeclaration'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_DichiarazioneIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_Calendars'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_Calendari' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_MOStepsDetailedQty'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_OdPFasiDettaglioQta' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_WorkCenters'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_CdL' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_InventoryDefaults'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_DefaultMagazzino' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.MA_MultiStorageParameters'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDbl.MN_ParametriMultiDeposito' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.MA_Materials'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MN_Materiali' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ItemCodes.Dbl.MA_ItemCodes'
WHERE( NameSpace = 'MagoNet.VTSZSZJ.VTSZSZJDbl.MN_VTSZSZJCodes' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Dbl.MA_VariantsComponents'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDbl.MN_VariantiComponenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdComponents'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliComponenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_CommodityCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_CatMerceologiche' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.MA_ItemToCtgAssociations'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.MN_AssociazioniArticoliCat' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocNotes'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquistoNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.Dbl.MA_JournalEntriesForecast'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.ContabilitaPrevisionaleDbl.MN_PrimaNotaPrevisionale' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_UnitOfMeasureDetail'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_UnitaMisuraRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdPymtSched'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliScadenze' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_TaxDistributionSummaries'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_RiepiloghiVentRegistroIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.MA_Ledgers'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.MN_Mastri' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Dbl.MA_VariantsStorageQty'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDbl.MN_VariantiQtaDeposito' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_JournalEntriesTaxJoin'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_PrimaNotaIVARegistroUnif' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_JournalEntriesTaxDetail'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_PrimaNotaRigheIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemParameters'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ParametriArticoli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.MA_CostAccParameters'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.MN_ParametriAnalitica' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocCorrTaxSummary'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaRiepilogoIVACor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.MA_CurrenciesFixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.MN_DiviseFixing' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.MA_ConfigurationsIncompat'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.MN_ConfiguratoreIncompat' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppBranches'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForSedi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_CalendarsShift'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_CalendariTurni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.MA_CostAccEntries'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.MN_MovimentiAnalitici' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_TmpLIFOProgress'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_TmpFormazioneLifo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SalesStatistics'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_StatisticheVendite' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_ProductionPlans'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_PianoProd' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_Items'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_Articoli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Dbl.MA_CustQuotas'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDbl.MN_OffCli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_SalesDiscDefaults'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_DefaultScontiVendita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_TmpABC'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_TmpABC' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Dbl.MA_ReceiptsBatchDetail'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDbl.MA_ReceiptsBatchDetail' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_PriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_Listini' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocReferences'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaRiferimenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.MA_Transport'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.MN_ModiTrasporto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_InventoryParameters'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ParametriMagazzino' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdTaxSummary'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliRiepilogoIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.MA_CurrencyParameters'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.MN_ParametriDivise' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustomerCtg'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CatClienti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_TaxJournalTotals'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_TotaliPerRegistroIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.MA_IDNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.MN_NumeratoriID' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Dbl.MA_SuppQuotasTaxSummary'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDbl.MN_OffForRiepilogoIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdSummary'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliCodaContabile' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_CommissionEntries'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_MovimentiAgenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocShipping'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaSpedizione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_Teams'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_Squadre' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Dbl.MA_CustQuotasNote'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDbl.MN_OffCliNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_ItemsFiscalData'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ArticoliFiscale' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSupp'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliFor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_BillOfMaterialsDrawings'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_DistintaBaseDisegni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_ItemsFIFODomCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ArticoliFifoDivLoc' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Dbl.MA_BanksAccounts'
WHERE( NameSpace = 'MagoNet.Banche.BancheDbl.MN_BancheCCBancari' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocManufReasons'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaCauProduzione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.MA_OffsetsCustSupp'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.MN_OffsetsCustSupp' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_CompanyFiscalYears'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_AziendaEsercizi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixedAssetsFiscal'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_CespitiFiscale' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.MA_PurchaseReq'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.MN_RdA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_MOComponents'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_OdPMateriali' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.MA_IntraArrivals1A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.MN_IntraAcquistiBis' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemsComparableUoM'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ArticoliUnMisDerivate' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Dbl.MA_CustQuotasDetail'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDbl.MN_OffCliRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixedAssetsBalance'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_CespitiBilancio' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.MA_LocationsCoordinates'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.MN_CoordinateUbicazioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.MA_CostCenters'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.MN_CentriCosto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SingleStepLifoFifo.Dbl.MA_LIFOFIFOParameters'
WHERE( NameSpace = 'MagoNet.LifoFifoScattiContinui.LifoFifoScattiContinuiDbl.MN_ParametriLifoFifo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdDetails'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.MA_PaymentParameters'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.MN_ParametriPagamenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_ItemsFiscalDataDomCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ArticoliFiscaleDivLoc' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.MA_CONAIEntries'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MN_MovimentiConai' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_SalesIncompatDefaults'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_DefaultIncompatVendita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_TmpBOMExplosions'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_TmpEsplosioneDistinta' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_Segments'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_Segmenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixAssetEntriesDetail'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_MovimentiCespitiRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocComponents'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaComponenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_CompanyPeople'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_AziendaPersone' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_PurchaseIncompatDefaults'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_DefaultIncompatAcquisto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdReferences'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdForRiferimenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_PackagesLang'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_ImballiLingue' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CRP.Dbl.MA_PlanTeamsSim'
WHERE( NameSpace = 'MagoNet.CRP.CRPDbl.MN_SimPianoSquadre' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_OpeningClosingDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_DefaultChiusuraApertura' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_ConfirmationLevels'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_GradiConferma' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdDefaults'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_DefaultOrdFor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_BreakdownReasons'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_CausaliNonDisponibilita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.Dbl.MA_JournalEntries'
WHERE( NameSpace = 'MagoNet.Contabilita_RO.Contabilita_RODbl.MN_PrimaNota' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.MA_AdditionalChargesDetail'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.MN_OneriAccessoriRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_MasterFor770Form'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_AnagrafichePer770' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_FeeTemplates'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ModelliParcelle' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_PL.Dbl.MA_TaxDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita_PL.Contabilita_PLDbl.MN_DefaultIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_SalesPeopleParameters'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_ParametriAgenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_PaymentTermsLang'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_CondizioniPagamentoLingue' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_PayeesParametersFIRRMulti'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ParametriPercipFIRRPluri' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_Fees'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_Parcelle' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_CommissionPoliciesDetail'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_PoliticheProvvRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocSummary'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaCodaContabile' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_ConsBalSheetsBalance'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_BilanciConsolidatiSaldi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdDetails'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdForRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccountingDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_DefaultContabili' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_InventoryReasonsLang'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_CausaliMagazzinoLingue' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Dbl.MA_JobGroups'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDbl.MN_GruppiCommesse' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_SalesPeopleAllowance'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_AgentiIndennita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.MA_CostCentersBalances'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.MN_CentriCostoSaldi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_SmartCodesISOCode'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_CodiceParlanteISOStato' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChargePolicies.Dbl.MA_ChargePoliciesPackages'
WHERE( NameSpace = 'MagoNet.PoliticheSpese.PoliticheSpeseDbl.MN_PoliticheSpeseImballi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_BillOfMaterials'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_DistintaBase' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.MA_Configurations'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.MN_ConfiguratoreArticoli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.MA_ItemsStorageQty'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDbl.MN_ArticoliQtaDeposito' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.MA_ExtAccTemplate'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.MN_ExtAccountingTemplate' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccountingAdjDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_DefaultAssestamento' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.MA_CONAIParameters'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MN_ParametriConai' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.MA_DistribTemplatesDetail'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.MN_ModelliRipartizioneRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Core.Dbl.MA_TmpReport'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_TmpReport' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.Dbl.MA_AccountingSimulations'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.ContabilitaPrevisionaleDbl.MN_SimulazioniContabili' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.MA_DistribTemplates'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.MN_ModelliRipartizione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.MA_PurchaseReqRequirements'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.MN_RdAProvenienzaFabbisogni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocCorrSummary'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MA_PurchaseDocCorrSummary' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.MA_IntraDispatches2B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.MN_IntraCessioniTer' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_TaxJournalSummaries'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_RiepiloghiPerRegistroIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_SmartCodesCombinations'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_CombinazioniCodiceParlante' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixAssetsCtg'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_CatCespiti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.MA_ConfiguratorParameters'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.MN_ParametriConfiguratore' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.MA_MRPSimulations'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.MN_MRPSimulazioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.MA_IntraArrivals1B'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.MN_IntraAcquistiTer' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.MA_OffsetsItems'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.MN_OffsetsItems' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.MA_TaxJournalNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.MN_NumeratoriRegistriIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppCustomerOptions'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForAccessorioCliente' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SpecialTax.Dbl.MA_Plafond'
WHERE( NameSpace = 'MagoNet.RegimiIVASpeciali.RegimiIVASpecialiDbl.MN_PlafondIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.MA_CostAccEntriesDetail'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.MN_MovimentiAnaliticiRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDoc'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquisto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ProductCtgSubCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_CatProdottoSottoCat' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_CustSuppBudget'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CliForBudget' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_ENASARCOParameters'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_ParametriEnasarco' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.MA_ISOCountryCodes'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.MN_ISOStati' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_Areas'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_Aree' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_Producers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_Produttori' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChargePolicies.Dbl.MA_ChargePoliciesAreas'
WHERE( NameSpace = 'MagoNet.PoliticheSpese.PoliticheSpeseDbl.MN_PoliticheSpeseAree' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Dbl.MA_ProspectiveSuppliers'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDbl.MN_FornitoriPotenziali' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocSummary'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquistoCodaContabile' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.MA_TaxJournals'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.MN_RegistriIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ItemCodes.Dbl.MA_Items'
WHERE( NameSpace = 'MagoNet.VTSZSZJ.VTSZSZJDbl.MN_Articoli' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_PayeesParametersINPS'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ParametriPercipINPS' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.MA_PaymentTermsDefaults'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.MN_DefaultPerTipoPagamento' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_MOComponentsStepsQty'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_OdPQtaMaterialiFasi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.MA_CalendarsExcludedDay'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.MN_CalendariGGEsclusi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemSuppliers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ForArticolo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.MA_PurchaseOrdTaxSummay'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.MN_OrdForRiepilogoIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixAssetsClasses'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_ClassiCespiti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_BillOfMaterialsComp'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_DistintaBaseComponenti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_InventoryReasons'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_CausaliMagazzino' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.MA_PriceListsLang'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.MN_ListiniLingue' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_PurchasesDiscDefaults'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_DefaultScontiAcquisto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.MA_PyblsRcvblsParameters'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.MN_ParametriPartite' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_CompanyGroupNotes'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_GruppoAziendeNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_SalesValuesDefaults'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_DefaultValUnitariVendita' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocDetail'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.MA_PyblsRcvblsDetails'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.MN_PartiteRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_TaxExigibility'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_EsigibilitaIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_SegmentsCombISOCode'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_SegmentiPossibISOStato' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_InventoryEntriesDetail'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_MovimentiMagazzinoRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_ProdPlanReplacements'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_PianoProdSostituzioni' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.MA_ItemsConai'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MN_ArticoliConai' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_TmpMOImplosion'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_TmpImplosioneOdP' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_Pickings'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_BdP' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.MA_ProductionPlansDetail'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.MN_PianoProdRighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_ItemsLIFODomCurr'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ArticoliLifoDivLoc' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.MA_ProductionDevelopment'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.MN_AvanzamentoProduzione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_ItemsIntrastat'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_ArticoliIntrastat' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccTemplates'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_ModelliContabili' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.MA_PyblsRcvblsParamsReqPymt'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.MN_ParametriPartiteSolleciti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.MA_ChartOfAccountsBalances'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.MN_PianoContiSaldi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Dbl.MA_SerialNumbers'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDbl.MN_NumeratoriMatricole' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.MA_PayeesParameters'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.MN_ParametriPercip' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting_RO.Dbl.MA_TaxDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita_RO.Contabilita_RODbl.MN_DefaultIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Dbl.MA_BanksBillAccounts'
WHERE( NameSpace = 'MagoNet.Banche.BancheDbl.MN_BancheCCEffetti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.MA_NonFiscalNumbers'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.MN_NumeratoriNonFiscali' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.MA_PyblsRcvbls'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.MN_Partite' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.MA_HomogeneousCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.MN_CatOmogenee' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_JournalEntriesIntraTax'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_PrimaNotaIVAIntra' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.MA_CommissionPolicies'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.MN_PoliticheProvv' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_ItemsLIFO'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_ArticoliLifo' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdNotes'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliNote' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.MA_SmartCodes'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.MN_CodiceParlante' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.MA_PurchaseDocPymtSched'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.MN_DocAcquistoScadenze' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_TmpBalanceSheetValues'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_TmpBilancioContrapposto' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_CompanyGroupPeople'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_GruppoAziendePersone' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.MA_InventoryEntries'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_MovimentiMagazzino' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SalesDefaults'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DefaultVendite' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.MA_PurchaseReqDetail'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.MN_RdARighe' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.MA_SaleOrdShipping'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.MN_OrdCliSpedizione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_TaxDefaults'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_DefaultIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.MA_SaleDocCorrSummary'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.MN_DocVenditaCodaContabileCor' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Dbl.MA_JobsBalances'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDbl.MN_CommesseSaldi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.MA_StorageGroups'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDbl.MN_GruppiDepositi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_OwnedCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_Controllate' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixedAssets'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_Cespiti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.MA_ChartOfAccounts'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.MN_PianoConti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Dbl.MA_SuppQuotasShipping'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDbl.MN_OffForSpedizione' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.MA_PackageTypes'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MN_TipologieImballaggio' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Dbl.MA_Lots'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDbl.MN_Lotti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MA_ConsolidTemplates'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.MN_ModelliConsolidamento' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_PricePolicies'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_PolitichePrezzi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.MA_FixAssetEntries'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.MN_MovimentiCespiti' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.MA_InventoryEntriesPhases'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MN_MovimentiMagazzinoFasi' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Dbl.MA_LotsStoragesQty'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDbl.MN_LottiQtaDeposito' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.MA_ItemsPriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.MN_ArticoliListini' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.MA_SupplierCtg'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.MN_CatFornitori' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Intrastat.Dbl.MA_IntraDispatches2A'
WHERE( NameSpace = 'MagoNet.Intrastat.IntrastatDbl.MN_IntraCessioniBis' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.MA_AccTemplatesTaxDetail'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.MN_ModelliContabiliRigheIVA' AND 
(
TypeId = @tableTypeId OR 
TypeId = @viewTypeId
))
END
GO