
											if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[MSD_Objects]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
 
	DECLARE @referenceObjectTypeId as integer 
	SET @referenceObjectTypeId = (SELECT TypeId FROM MSD_ObjectTypes WHERE Type =11)
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.CompanyGroups'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.GruppoAziende' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.SaleOrd'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.OrdCli' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.SegmentsComb'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.SegmentiPossibilita' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.InventoryEntriesByDocNo'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MovimentiMagazzinoPerNrFiscaleBolla' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.SaleOrdersDetails'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.OrdCliRighe' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.InvoiceType'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.TipoFattura' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PaymentOrderSlipsByTypeAndCurrency'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistinteMandatiPerTipoEDivisa' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.ComponentsOrders.SubcntOrd'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroComponentsOrdini.OrdForLavEst' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.Transport'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.ModiTrasporto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.ProductSubCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.SottoCatProdotto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.UnitsOfMeasure'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.UnitaMisura' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.ConsolidationTemplates'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.ModelliConsolidamento' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PaymentOrderSlips'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistinteMandati' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.SalesPeople'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.Agenti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.AccountingTemplatesByOperationType'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.ModelliContabiliPerTipoOperazione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.Drawings'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.Disegni' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.PostableItems'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.ArticoliMovimentabili' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.PickingList'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.BuonoPrelievo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.Areas'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.AreeVendita' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.CollectionSlips'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistinteIncassi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.HomogeneousCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.CatOmogenee' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.ItemSuppliersByDescription'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ForArtPerDescrizione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.FeeTemplates'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.ModelliParcelle' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Dbl.BanksBillAccounts'
WHERE( NameSpace = 'MagoNet.Banche.BancheDbl.BancheCCEffetti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.Questions'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.Domande' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.JournalEntriesTax'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.PrimaNotaIVA' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.PurchaseOrdersDetails'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.OrdForRighe' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.AssetsAndLiabilitiesLedger'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.MastriPatrimoniali' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.CustomersCategories'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.CategorieClienti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.Producers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.Produttori' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.Storages'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDbl.Depositi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.BillOfMaterials'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.DistintaBase' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.GoodLocations'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.UbicazioniMerce' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SupplierQuotations.Dbl.SuppQuota'
WHERE( NameSpace = 'MagoNet.OfferteFornitori.OfferteFornitoriDbl.OffFor' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.InHouseOutsourcedWC'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.CdLExtInt' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.AccountingTemplatesByTaxSign'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.ModelliContabiliSegnoIVA' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.ItemCustomersByDescription'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.CliArtPerDescrizione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.ItemsLanguageDescri'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ArticoliDescriLingua' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.ReceiptSerialNos'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.MatricoleCarico' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ForecastAccounting.Dbl.AccountingSimulations'
WHERE( NameSpace = 'MagoNet.ContabilitaPrevisionale.ContabilitaPrevisionaleDbl.SimulazioniContabili' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.AdditionalCharges'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.OneriAccessori' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.Materials'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.Materiali' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Dbl.JobGroups'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDbl.GruppiCommesse' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.PurchaseBarcodes'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.BarCodeAcquisto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.DutyCodes'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.CodiciTributo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomerQuotations.Dbl.CustQuota'
WHERE( NameSpace = 'MagoNet.OfferteClienti.OfferteClientiDbl.OffCli' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Components.Advance'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeComponents.AccontiVendite' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.Operations'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.Operazioni' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.Titles'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.Titoli' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.ItemType'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.TipoArticolo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.SaleOrdersProdDetails'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.OrdCliRigheProd' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.TaxJournalsByType'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.RegistriIVAPerTipoRegistro' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.CollectionSlipsByType'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistinteIncassiPerTipo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.Advance'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.Acconti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.CommonFixedAssetsCtg'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.CategorieComuni' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.SaleOrdersToInvoice'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.OrdCliDaFatturare' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.CostCenters'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.CentriCosto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.PurchaseOrdersByItem'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.OrdForPerArticolo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.ItemsMaterials'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.ArticoliMateriali' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.JournalEntriesTaxByDocNo'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.PrimaNotaIVAPerNrDoc' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.PackageTypes'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MaterialiTipologie' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.ProfitAndLossLedger'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.MastriEconomici' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.ChartOfAccounts'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.PianoConti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.SuppliersCategories'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.CategorieFornitori' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.Fixing'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.Fixing' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Conai.Dbl.ConaiEntries'
WHERE( NameSpace = 'MagoNet.Conai.ConaiDbl.MovimentiConai' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.Branches'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.Sedi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.ProductCtgByManufacturer'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.CatProdottoPerProduttore' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.SaleOrdersByCustoerAndProduct'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.OrdCliPerClienteProd' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Dbl.ItemsLots'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDbl.LottiPerArticolo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.StubBooks'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.Bollettari' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.AP_AR'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.Partite' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Dbl.BanksAccounts'
WHERE( NameSpace = 'MagoNet.Banche.BancheDbl.BancheCCBancari' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.PurchaseOrd'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.OrdFor' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.FixedAssets'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.Cespiti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.Items'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.Articoli' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.OwnerCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.Controllanti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.BreakdownReasons'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.CausaliNonDisp' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.CommodityCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.CatMerceologica' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.ISOCountryCodes'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.ISOStati' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.CostCenterGroups'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.GruppiCentriCosto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.Currencies'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.Divise' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Dbl.CompanyCurrencies'
WHERE( NameSpace = 'MagoNet.Divise.DiviseDbl.DiviseAzienda' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.AnswersToQuestion'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.RisposteDomanda' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.SupplierSerialNos'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.MatricoleFornitore' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.SaleDocument'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.DocVendita' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.IntraTaxJournals'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.RegistriIVAIntra' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PayablesReceivablesByDocNo'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.PartitePerNrDoc' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.Ledgers'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.Mastri' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Components.Receivables'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeComponents.PartiteVendite' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.Branches'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.Sedi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Sales.Dbl.SaleDocExternal'
WHERE( NameSpace = 'MagoNet.Vendite.VenditeDbl.DocVenditaEsterno' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.ItemsLocationsByLotAndFiscalYear'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.ArticoliUbicazioni' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.CommonCustSupp'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.CliForBase' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.ItemsLocation'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.ArticoliUbicazione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.WC'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.CdL' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Dbl.Contacts'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDbl.Contatti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.Slips'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.Distinte' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.ComparableUoM'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.UnMisDerivate' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.ItemsbyCompositionType'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.ArticoliTipoComposizione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.DistributionTemplatesDetail'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.CategorieOneri' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.SaleBarCode'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.BarCodeVendita' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.SmartCodeRootWithSegments'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.CodParlanteRadiceConSegmenti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.PurchaseDoc'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.DocAcquisto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ItemCodes.Dbl.ItemCodes'
WHERE( NameSpace = 'MagoNet.VTSZSZJ.VTSZSZJDbl.VTSZSZJCodes' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.CommissionCategories'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.CategorieProvv' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.IdsMng.Dbl.TaxJournals'
WHERE( NameSpace = 'MagoNet.IdsMng.IdsMngDbl.RegistriIVA' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PricePolicies.Dbl.PriceLists'
WHERE( NameSpace = 'MagoNet.PolitichePrezzi.PolitichePrezziDbl.Listini' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.ChartOfAccountsByAccountType'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.PianoContiPerTipoConto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.VouchersSlips'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistinteReversali' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.AccountingTemplates'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.ModelliContabili' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.InventoryReasons'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.CausaliMagazzino' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Purchases.Dbl.Payables'
WHERE( NameSpace = 'MagoNet.Acquisti.AcquistiDbl.RadarPartite' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.ProductCtg'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.CatProdotto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Currencies.Components.FixingForCurrencyDoc'
WHERE( NameSpace = 'MagoNet.Divise.DiviseComponents.FixingForCurrencyDoc' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.AccountingReasonsByTemplate'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.CausaliContabiliPerTipoModello' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.AreaManager'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.AgentiCapiArea' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.StorageGroups'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDbl.RaggruppamentiDepositi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.OwnedCompanies'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.Controllate' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.JobTicket'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.BollaLavorazione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.FixedAssetsByLifePeriod'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.CespitiPerDurata' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.SubstituteItems'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ArticoliEquivalenti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.CommonFixedAssets'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.CespitiComuni' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Routing.Dbl.Teams'
WHERE( NameSpace = 'MagoNet.Cicli.CicliDbl.Squadre' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.PurchaseOrdersExternal'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.OrdForEsterno' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SalesPeople.Dbl.CommissionPolicies'
WHERE( NameSpace = 'MagoNet.Agenti.AgentiDbl.PoliticheProvv' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.FixedAssetsReasons'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.CausaliCespiti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.PurchaseOrdersDetailsByItem'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.OrdForRighePerArticolo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.Categories'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.Categorie' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.Segments'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.Segmenti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.Departments'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.Reparti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Payees.Dbl.INPSParameters'
WHERE( NameSpace = 'MagoNet.Percipienti.PercipientiDbl.ParametriINPS' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.InventoryAccounting.Dbl.ExtAccountingTemplate'
WHERE( NameSpace = 'MagoNet.InventoryAccountingTransaction.InventoryAccountingTransactionDbl.ExtAccountingTemplate' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.Tax'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.IVA' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.SaleOrdersToFulfill'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.OrdCliDaEvadere' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.BillsSlip'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistinteEffetti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiCompanyBalances.Dbl.MultiCompanyBalances'
WHERE( NameSpace = 'MagoNet.BilanciConsolidati.BilanciConsolidatiDbl.BilanciConsolidati' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.PostableChartOfAccounts'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.PianoContiMovimentabile' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.ProductionPlan'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.PianoProduzione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.Classes'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.Classi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Documents.Account'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDocuments.ContoPdC' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Documents.ContactsByAccountType'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDocuments.ContattiConto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.PurchaseOrdersToInvoice'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.OrdForDaFatturare' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AP_AR.Dbl.PaymentOrderSlipsByType'
WHERE( NameSpace = 'MagoNet.Partite.PartiteDbl.DistinteMandatiPerTipo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Variants.Dbl.Variants'
WHERE( NameSpace = 'MagoNet.Varianti.VariantiDbl.Varianti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.CoordinatesDescriptions'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.DescriCoordinate' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CostAccounting.Dbl.ChartOfAccountsInCostAcc'
WHERE( NameSpace = 'MagoNet.Analitica.AnaliticaDbl.PianoContiAnalitico' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BillOfMaterials.Dbl.BillOfMaterialsForType'
WHERE( NameSpace = 'MagoNet.DistintaBase.DistintaBaseDbl.DistintaBasePerTipo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Components.ItemsPriceLists'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoComponents.ListiniArt' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.Ports'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.Porti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MultiStorage.Dbl.ItemsStorageQty'
WHERE( NameSpace = 'MagoNet.MultiDeposito.MultiDepositoDbl.ArticoliQtaDeposito' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ProductLines.Dbl.ProductLines'
WHERE( NameSpace = 'MagoNet.LineeDiProdotto.LineeDiProdottoDbl.LineeProdotto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.ItemsStorageQty'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.ArticoliQtaDeposito' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Manufacturing.Dbl.ManufacturingOrder'
WHERE( NameSpace = 'MagoNet.Produzione.ProduzioneDbl.OrdiniProduzione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Inventory.Dbl.InventoryEntries'
WHERE( NameSpace = 'MagoNet.Magazzino.MagazzinoDbl.MovimentiMagazzino' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Languages.Dbl.Languages'
WHERE( NameSpace = 'MagoNet.Lingue.LingueDbl.Lingue' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Locations.Dbl.ItemLocation'
WHERE( NameSpace = 'MagoNet.Ubicazioni.UbicazioniDbl.UbicazioniArticolo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SaleOrders.Dbl.ConfirmationLevels'
WHERE( NameSpace = 'MagoNet.OrdiniClienti.OrdiniClientiDbl.GradiConferma' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.Packages'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.Imballi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Company.Dbl.FiscalYears'
WHERE( NameSpace = 'MagoNet.Azienda.AziendaDbl.Esercizi' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.CustomersSuppliers.Dbl.CustSupp'
WHERE( NameSpace = 'MagoNet.ClientiFornitori.ClientiFornitoriDbl.CliFor' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Dbl.ProspectiveSuppliers'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDbl.FornitoriPotenziali' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Contacts.Documents.ProspectiveSuppliersByAccountType'
WHERE( NameSpace = 'MagoNet.Contatti.ContattiDocuments.FornitoriPotenzialiConto' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.ItemCustomers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.CliArticolo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Dbl.AccountingReasons'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDbl.CausaliContabili' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.Dbl.PaymentTerms'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDbl.CondizioniPagamento' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Jobs.Dbl.Jobs'
WHERE( NameSpace = 'MagoNet.Commesse.CommesseDbl.Commesse' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.ComponentsSales.SubcntDN'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroComponentsVendite.DDTForLavEst' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Subcontracting.ComponentsPurchases.SubcntBoL'
WHERE( NameSpace = 'MagoNet.ContoLavoro.ContoLavoroComponentsAcquisti.BdCForLavEst' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Configurator.Dbl.Configuration'
WHERE( NameSpace = 'MagoNet.Configuratore.ConfiguratoreDbl.Configurazioni' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PaymentTerms.Dbl.CreditCard'
WHERE( NameSpace = 'MagoNet.CondizioniPagamento.CondizioniPagamentoDbl.CartaCredito' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Banks.Dbl.CompanyBanks'
WHERE( NameSpace = 'MagoNet.Banche.BancheDbl.BancheDitta' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.FixedAssets.Dbl.Locations'
WHERE( NameSpace = 'MagoNet.Cespiti.CespitiDbl.Ubicazioni' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.PurchaseOrders.Dbl.PurchaseOrdersToFulfill'
WHERE( NameSpace = 'MagoNet.OrdiniFornitori.OrdiniFornitoriDbl.OrdForDaEvadere' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Accounting.Documents.DistributionTaxCode'
WHERE( NameSpace = 'MagoNet.Contabilita.ContabilitaDocuments.IVAVent' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Dbl.ItemsByGoodType'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliDbl.ArticoliPerTipoBene' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.SmartCode.Dbl.SmartCodeRoot'
WHERE( NameSpace = 'MagoNet.CodiceParlante.CodiceParlanteDbl.CodParlanteRadice' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.AdditionalCharges.Dbl.DistributionTemplates'
WHERE( NameSpace = 'MagoNet.OneriAccessori.OneriAccessoriDbl.ModelliRipartizione' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.ChartOfAccounts.Dbl.ChartOfAccountsByLedgerType'
WHERE( NameSpace = 'MagoNet.PianoDeiConti.PianoDeiContiDbl.PianoContiPerTipoMastro' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Items.Components.ItemSuppliers'
WHERE( NameSpace = 'MagoNet.Articoli.ArticoliComponents.ForArticolo' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.Shippings.Dbl.Carriers'
WHERE( NameSpace = 'MagoNet.Spedizioni.SpedizioniDbl.Vettori' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.LotsSerials.Dbl.Lots'
WHERE( NameSpace = 'MagoNet.LottiMatricole.LottiMatricoleDbl.Lotti' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.MRP.Dbl.PurchaseRequest'
WHERE( NameSpace = 'MagoNet.MRP.MRPDbl.RdA' AND 
TypeId = @referenceObjectTypeId
)
END
BEGIN 
UPDATE 
MSD_Objects SET NameSpace = 'ERP.BalanceAnalysis.Dbl.BalanceReclassifications'
WHERE( NameSpace = 'MagoNet.AnalisiBilancio.AnalisiBilancioDbl.RiclassificazioniPianoConti' AND 
TypeId = @referenceObjectTypeId
)
END
GO