﻿export const appRoutes = [
    { path: 'framework/tbresourcesmng', loadChildren: 'app/applications/framework/tbresourcesmng/tbresourcesmng.module#TbResourcesMngModule' },
    { path: 'framework/tbges', loadChildren: 'app/applications/framework/tbges/tbges.module#TbGesModule' },
    { path: 'framework/tbgenlibui', loadChildren: 'app/applications/framework/tbgenlibui/tbgenlibui.module#TbGenlibUIModule' },
    { path: 'extensions/tbdatasynchroclient', loadChildren: 'app/applications/extensions/tbdatasynchroclient/tbdatasynchroclient.module#TbDataSynchroClientModule' },
    { path: 'extensions/easyattachment', loadChildren: 'app/applications/extensions/easyattachment/easyattachment.module#EasyAttachmentModule' },
    { path: 'mdc/taxsettlement_it', loadChildren: 'app/applications/mdc/taxsettlement_it/taxsettlement_it.module#TaxSettlement_ITModule' },
    { path: 'mdc/taxdocuments_it', loadChildren: 'app/applications/mdc/taxdocuments_it/taxdocuments_it.module#TaxDocuments_ITModule' },
    { path: 'mdc/fatel', loadChildren: 'app/applications/mdc/fatel/fatel.module#FATELModule' },
    { path: 'mdc/electronicinvoicing_it', loadChildren: 'app/applications/mdc/electronicinvoicing_it/electronicinvoicing_it.module#ElectronicInvoicing_ITModule' },
    { path: 'mdc/electronicinvoicing_hu', loadChildren: 'app/applications/mdc/electronicinvoicing_hu/electronicinvoicing_hu.module#ElectronicInvoicing_HUModule' },
    { path: 'mdc/electronicinvoicing_br', loadChildren: 'app/applications/mdc/electronicinvoicing_br/electronicinvoicing_br.module#ElectronicInvoicing_BRModule' },
    { path: 'mdc/core', loadChildren: 'app/applications/mdc/core/core.module#CoreModule' },
    { path: 'erp/xbrl', loadChildren: 'app/applications/erp/xbrl/xbrl.module#XBRLModule' },
    { path: 'erp/wmsmobile', loadChildren: 'app/applications/erp/wmsmobile/wmsmobile.module#WMSMobileModule' },
    { path: 'erp/wms', loadChildren: 'app/applications/erp/wms/wms.module#WMSModule' },
    { path: 'erp/weee', loadChildren: 'app/applications/erp/weee/weee.module#WEEEModule' },
    { path: 'erp/warman', loadChildren: 'app/applications/erp/warman/warman.module#WarManModule' },
    { path: 'erp/variants', loadChildren: 'app/applications/erp/variants/variants.module#VariantsModule' },
    { path: 'erp/userdefault', loadChildren: 'app/applications/erp/userdefault/userdefault.module#UserDefaultModule' },
    { path: 'erp/toolsmanagement', loadChildren: 'app/applications/erp/toolsmanagement/toolsmanagement.module#ToolsManagementModule' },
    { path: 'erp/tesanconnector', loadChildren: 'app/applications/erp/tesanconnector/tesanconnector.module#TESANConnectorModule' },
    { path: 'erp/synchroconnector', loadChildren: 'app/applications/erp/synchroconnector/synchroconnector.module#SynchroConnectorModule' },
    { path: 'erp/supplierquotations', loadChildren: 'app/applications/erp/supplierquotations/supplierquotations.module#SupplierQuotationsModule' },
    { path: 'erp/supplierpricesadj', loadChildren: 'app/applications/erp/supplierpricesadj/supplierpricesadj.module#SupplierPricesAdjModule' },
    { path: 'erp/subcontracting', loadChildren: 'app/applications/erp/subcontracting/subcontracting.module#SubcontractingModule' },
    { path: 'erp/specialtax', loadChildren: 'app/applications/erp/specialtax/specialtax.module#SpecialTaxModule' },
    { path: 'erp/smartcode', loadChildren: 'app/applications/erp/smartcode/smartcode.module#SmartCodeModule' },
    { path: 'erp/singlesteplifofifo', loadChildren: 'app/applications/erp/singlesteplifofifo/singlesteplifofifo.module#SingleStepLifoFifoModule' },
    { path: 'erp/simplifiedaccounting', loadChildren: 'app/applications/erp/simplifiedaccounting/simplifiedaccounting.module#SimplifiedAccountingModule' },
    { path: 'erp/shippings', loadChildren: 'app/applications/erp/shippings/shippings.module#ShippingsModule' },
    { path: 'erp/salespeople', loadChildren: 'app/applications/erp/salespeople/salespeople.module#SalesPeopleModule' },
    { path: 'erp/sales', loadChildren: 'app/applications/erp/sales/sales.module#SalesModule' },
    { path: 'erp/saleorders', loadChildren: 'app/applications/erp/saleorders/saleorders.module#SaleOrdersModule' },
    { path: 'erp/routing', loadChildren: 'app/applications/erp/routing/routing.module#RoutingModule' },
    { path: 'erp/qualityinspection', loadChildren: 'app/applications/erp/qualityinspection/qualityinspection.module#QualityInspectionModule' },
    { path: 'erp/purchases', loadChildren: 'app/applications/erp/purchases/purchases.module#PurchasesModule' },
    { path: 'erp/purchaseorders', loadChildren: 'app/applications/erp/purchaseorders/purchaseorders.module#PurchaseOrdersModule' },
    { path: 'erp/productlines', loadChildren: 'app/applications/erp/productlines/productlines.module#ProductLinesModule' },
    { path: 'erp/pricepolicies', loadChildren: 'app/applications/erp/pricepolicies/pricepolicies.module#PricePoliciesModule' },
    { path: 'erp/payrollimport', loadChildren: 'app/applications/erp/payrollimport/payrollimport.module#PayrollImportModule' },
    { path: 'erp/paymentterms', loadChildren: 'app/applications/erp/paymentterms/paymentterms.module#PaymentTermsModule' },
    { path: 'erp/payees', loadChildren: 'app/applications/erp/payees/payees.module#PayeesModule' },
    { path: 'erp/openorders', loadChildren: 'app/applications/erp/openorders/openorders.module#OpenOrdersModule' },
    { path: 'erp/omniaconnector', loadChildren: 'app/applications/erp/omniaconnector/omniaconnector.module#OMNIAConnectorModule' },
    { path: 'erp/multistorage', loadChildren: 'app/applications/erp/multistorage/multistorage.module#MultiStorageModule' },
    { path: 'erp/multicompanybalances', loadChildren: 'app/applications/erp/multicompanybalances/multicompanybalances.module#MultiCompanyBalancesModule' },
    { path: 'erp/mrp', loadChildren: 'app/applications/erp/mrp/mrp.module#MRPModule' },
    { path: 'erp/manufacturingplus', loadChildren: 'app/applications/erp/manufacturingplus/manufacturingplus.module#ManufacturingPlusModule' },
    { path: 'erp/manufacturingmobile', loadChildren: 'app/applications/erp/manufacturingmobile/manufacturingmobile.module#ManufacturingMobileModule' },
    { path: 'erp/manufacturing', loadChildren: 'app/applications/erp/manufacturing/manufacturing.module#ManufacturingModule' },
    { path: 'erp/lotsserials', loadChildren: 'app/applications/erp/lotsserials/lotsserials.module#LotsSerialsModule' },
    { path: 'erp/languages', loadChildren: 'app/applications/erp/languages/languages.module#LanguagesModule' },
    { path: 'erp/jobs', loadChildren: 'app/applications/erp/jobs/jobs.module#JobsModule' },
    { path: 'erp/items', loadChildren: 'app/applications/erp/items/items.module#ItemsModule' },
    { path: 'erp/itemcodes', loadChildren: 'app/applications/erp/itemcodes/itemcodes.module#ItemCodesModule' },
    { path: 'erp/invoicemng', loadChildren: 'app/applications/erp/invoicemng/invoicemng.module#InvoiceMngModule' },
    { path: 'erp/inventoryaccounting', loadChildren: 'app/applications/erp/inventoryaccounting/inventoryaccounting.module#InventoryAccountingModule' },
    { path: 'erp/inventory', loadChildren: 'app/applications/erp/inventory/inventory.module#InventoryModule' },
    { path: 'erp/intrastat', loadChildren: 'app/applications/erp/intrastat/intrastat.module#IntrastatModule' },
    { path: 'erp/imago', loadChildren: 'app/applications/erp/imago/imago.module#IMagoModule' },
    { path: 'erp/idsmng', loadChildren: 'app/applications/erp/idsmng/idsmng.module#IdsMngModule' },
    { path: 'erp/ibconnector', loadChildren: 'app/applications/erp/ibconnector/ibconnector.module#IBConnectorModule' },
    { path: 'erp/forecastaccounting', loadChildren: 'app/applications/erp/forecastaccounting/forecastaccounting.module#ForecastAccountingModule' },
    { path: 'erp/fixedassets', loadChildren: 'app/applications/erp/fixedassets/fixedassets.module#FixedAssetsModule' },
    { path: 'erp/customerssuppliers', loadChildren: 'app/applications/erp/customerssuppliers/customerssuppliers.module#CustomersSuppliersModule' },
    { path: 'erp/customerquotations', loadChildren: 'app/applications/erp/customerquotations/customerquotations.module#CustomerQuotationsModule' },
    { path: 'erp/currencies', loadChildren: 'app/applications/erp/currencies/currencies.module#CurrenciesModule' },
    { path: 'erp/crp', loadChildren: 'app/applications/erp/crp/crp.module#CRPModule' },
    { path: 'erp/creditlimit', loadChildren: 'app/applications/erp/creditlimit/creditlimit.module#CreditLimitModule' },
    { path: 'erp/costaccounting', loadChildren: 'app/applications/erp/costaccounting/costaccounting.module#CostAccountingModule' },
    { path: 'erp/core', loadChildren: 'app/applications/erp/core/core.module#CoreModule' },
    { path: 'erp/contacts', loadChildren: 'app/applications/erp/contacts/contacts.module#ContactsModule' },
    { path: 'erp/configurator', loadChildren: 'app/applications/erp/configurator/configurator.module#ConfiguratorModule' },
    { path: 'erp/conai', loadChildren: 'app/applications/erp/conai/conai.module#ConaiModule' },
    { path: 'erp/company', loadChildren: 'app/applications/erp/company/company.module#CompanyModule' },
    { path: 'erp/chartofaccounts', loadChildren: 'app/applications/erp/chartofaccounts/chartofaccounts.module#ChartOfAccountsModule' },
    { path: 'erp/chargepolicies', loadChildren: 'app/applications/erp/chargepolicies/chargepolicies.module#ChargePoliciesModule' },
    { path: 'erp/cash', loadChildren: 'app/applications/erp/cash/cash.module#CashModule' },
    { path: 'erp/business_br', loadChildren: 'app/applications/erp/business_br/business_br.module#Business_BRModule' },
    { path: 'erp/billofmaterialsplus', loadChildren: 'app/applications/erp/billofmaterialsplus/billofmaterialsplus.module#BillOfMaterialsPlusModule' },
    { path: 'erp/billofmaterials', loadChildren: 'app/applications/erp/billofmaterials/billofmaterials.module#BillOfMaterialsModule' },
    { path: 'erp/basel_ii', loadChildren: 'app/applications/erp/basel_ii/basel_ii.module#Basel_IIModule' },
    { path: 'erp/barcode', loadChildren: 'app/applications/erp/barcode/barcode.module#BarcodeModule' },
    { path: 'erp/banks', loadChildren: 'app/applications/erp/banks/banks.module#BanksModule' },
    { path: 'erp/balanceanalysis', loadChildren: 'app/applications/erp/balanceanalysis/balanceanalysis.module#BalanceAnalysisModule' },
    { path: 'erp/ap_ar_plus', loadChildren: 'app/applications/erp/ap_ar_plus/ap_ar_plus.module#AP_AR_PlusModule' },
    { path: 'erp/ap_ar', loadChildren: 'app/applications/erp/ap_ar/ap_ar.module#AP_ARModule' },
    { path: 'erp/agoconnector', loadChildren: 'app/applications/erp/agoconnector/agoconnector.module#AGOConnectorModule' },
    { path: 'erp/additionalcharges', loadChildren: 'app/applications/erp/additionalcharges/additionalcharges.module#AdditionalChargesModule' },
    { path: 'erp/accrualsdeferrals', loadChildren: 'app/applications/erp/accrualsdeferrals/accrualsdeferrals.module#AccrualsDeferralsModule' },
    { path: 'erp/accounting_ro', loadChildren: 'app/applications/erp/accounting_ro/accounting_ro.module#Accounting_ROModule' },
    { path: 'erp/accounting_it', loadChildren: 'app/applications/erp/accounting_it/accounting_it.module#Accounting_ITModule' },
    { path: 'erp/accounting_ch', loadChildren: 'app/applications/erp/accounting_ch/accounting_ch.module#Accounting_CHModule' },
    { path: 'erp/accounting_bg', loadChildren: 'app/applications/erp/accounting_bg/accounting_bg.module#Accounting_BGModule' },
    { path: 'erp/accounting', loadChildren: 'app/applications/erp/accounting/accounting.module#AccountingModule' },];
