import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUSTOMERS_COMPLETEService } from './IDD_CUSTOMERS_COMPLETE.service';

@Component({
    selector: 'tb-IDD_CUSTOMERS_COMPLETE',
    templateUrl: './IDD_CUSTOMERS_COMPLETE.component.html',
    providers: [IDD_CUSTOMERS_COMPLETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CUSTOMERS_COMPLETEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CS_STATUS_itemSource: any;
public IDC_CS_REGION_itemSource: any;
public IDC_CS_FP_COUNTYOFBIRTH_itemSource: any;
public IDC_CS_CHAMBOFCOMMCOUNTY_itemSource: any;
public IDC_BE_CS_REGION_itemSource: any;
public IDC_BE_CS_STATUS_itemSource: any;
public IDC_CS_BRANCHSENTDOC_itemSource: any;
public IDC_CS_BRANCHSENTGOODS_itemSource: any;
public IDC_CS_BRANCHSENTBILLS_itemSource: any;
public IDC_CS_CURRENCY_FOR_BALANCES_itemSource: any;
public IDC_CUSTSUPP_OFFSETS_SYMBOL_itemSource: any;

    constructor(document: IDD_CUSTOMERS_COMPLETEService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_CS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_CS_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 
this.IDC_CS_FP_COUNTYOFBIRTH_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_CS_CHAMBOFCOMMCOUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_BE_CS_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 
this.IDC_BE_CS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_CS_BRANCHSENTDOC_itemSource = {
  "name": "CustSuppBranchCombo",
  "namespace": "ERP.CustomersSuppliers.Documents.CustSuppBranchCombo"
}; 
this.IDC_CS_BRANCHSENTGOODS_itemSource = {
  "name": "CustSuppBranchCombo",
  "namespace": "ERP.CustomersSuppliers.Documents.CustSuppBranchCombo"
}; 
this.IDC_CS_BRANCHSENTBILLS_itemSource = {
  "name": "CustSuppBranchCombo",
  "namespace": "ERP.CustomersSuppliers.Documents.CustSuppBranchCombo"
}; 
this.IDC_CS_CURRENCY_FOR_BALANCES_itemSource = {
  "name": "CustSuppCurrenciesCombo",
  "namespace": "ERP.CustomersSuppliers.Documents.CustSuppCurrenciesCombo"
}; 
this.IDC_CUSTSUPP_OFFSETS_SYMBOL_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.InventoryAccounting.OffsetSymbols",
  "useProductLanguage": true
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'CustomersSuppliers':['CustSupp','Disabled','CompanyName','Draft','TitleCode','NaturalPerson','ISOCountryCode','CustSuppKind','FiscalCode','TaxIdNumber','FantasyName','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Latitude','Longitude','Address','Address2','City','ZIPCode','County','Region','Country','Latitude','Longitude','IsDummy','InsertionDate','LinkedCustSupp','ExternalCode','OldCustSupp','ActivityCode','Telephone1','Telephone2','Telex','Fax','ContactPerson','WorkingPosition','WorkingTime','EMail','CertifiedEMail','SkypeID','Internet','DocumentSendingType','MailSendingType','NoSendPostaLite','TaxpayerType','FedStateReg','MunicipalityReg','GenRegNo','GenRegEntity','SUFRAMA','TaxOffice','ChambOfCommCounty','CompanyRegistrNo','ChambOfCommRegistrNo','SendDocumentsTo','ShipToAddress','PaymentAddress','Payment','Currency','InCurrency','PaymentPeriShablesWithin60','PaymentPeriShablesOver60','CustSuppBank','CA','DDCustSupp','CustSuppBank','CA','IBANIsManual','CIN','CACheck','CIN','CACheck','IBAN','DDCustSupp','CBICode','SIACode','CompanyBank','CompanyCA','Presentation','FactoringCA','CompanyBank','CompanyCA','Presentation','FactoringCA','CustomerCompanyCA','Language','PriceList','DiscountFormula','PriceList','DiscountFormula','PymtAccount','Job','CostCenter','UsedForSummaryDocuments','Job','CostCenter','Account','FiscalCtg','OMNIASubAccount','NoTaxComm','NoBlackList','PrivacyStatement','PrivacyStatementPrintDate','Notes'],'Options':['Blocked','Category','CustomerClassification','CustomerSpecification','GroupBills','PenalityPerc','OpenedAdmCases','OpenedAdmCasesAmount','UseReqForPymt','ReqForPymtThreshold','NoOfMaxLevelReqForPymt','ReqForPymtLastLevel','ReqForPymtLastDate','OpenedAdmCases','OpenedAdmCasesAmount','UseReqForPymt','ReqForPymtThreshold','NoOfMaxLevelReqForPymt','ReqForPymtLastLevel','ReqForPymtLastDate','ReferencesPrintType','Priority','Variant','Contract','NoPrintDueDate','OneDNPerOrder','OneReturnFromCustomerPerCN','OneDocumentPerPL','LotSelection','LotOverbook','DirectAllocation','AllocationArea','DebitCollectionCharges','CashOrderCharges','ShowPricesOnDN','DebitCollectionCharges','DebitStampCharges','InvoicingCustomer','LastDocNo','LastDocDate','LastDocTotal','LastPaymentTerm','InvoicingCustomer','OneInvoicePerOrder','InvoicingGroup','OneInvoicePerDN','GroupItems','GroupOrders','GroupCostAccounting','ExcludedFromWEEE','LastDocNo','LastDocDate','LastDocTotal','LastPaymentTerm','Transport','Package','CashOnDeliveryLevel','Shipping','Transport','Package','Port','FreeOfChargeLevel','CashOnDeliveryLevel','NoCarrierCharges','ChargesPercOnTotAmt','PackCharges','ShippingCharges','Carrier1','Carrier2','Carrier3','Salesperson','AreaManager','CommissionCtg','Area','CrossDocking','ConsignmentPartner','Salesperson','AreaManager','CommissionCtg','Area','CrossDocking','ConsignmentPartner','TaxCode','ExemptFromTax','AdditionalTax','IsAPrivatePerson','SuspendedTax','PublicAuthority','PASplitPayment','DebitFreeSamplesTaxAmount','GoodsOffset','ServicesOffset','WithholdingTaxManagement','WithholdingTaxBasePerc','WithholdingTaxPerc','DeclarationOfIntentNo','DeclarationOfIntentDate','DeclarationOfIntentOurNo','DeclarationOfIntentDueDate'],'HKLTitles':['Description'],'global':['FiscalCodeCalculate','OtherBranches','DocBranchDescri','GoodBranchDescri','BillBranchDescri','People','Outstandings','Outstandings','PriceListForCommodityCtg','BudIOrderedTot','OrdTot','BudInvoicedTot','InvoicedTot','Budget','NrFiscalYear','BalanceCurrency','DebitActTotal','CreditActTotal','ActualDebitBalance','ActualCreditBalance','DebitForTotal','CreditForTotal','BudgetDebitBalance','DebitTotal','CreditTotal','DebitBalance','CreditBalance','Balances','Offsets','DeclarationOfIntents','Form','CustSuppCircularLetters','Notes','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable','CLViewerSingleOrderLimit','CLViewerSingleOrderExposure','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerSingleOrderExposure','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerOrderedLimit','CLViewerOrderedExposure','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerOrderedExposure','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerTurnoverLimit','CLViewerTurnoverExposure','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverExposure','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverBills','CLViewerTurnoverOtherPaymentTerms','CLViewerTurnoverInvWithoutRec','CLViewerDeliveredDocNotInvoiced','CLViewerTotalExposureLimit','CLViewerTotalExposureExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','CLViewerTotalExposureExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','Documents'],'NaturalPerson':['LastName','Name','Professional','Gender','DateOfBirth','ISOCountryCodeOfBirth','CityOfBirth','CountyOfBirth'],'HKLCustCtg':['Description'],'HKLCustomerClassification':['Description'],'HKLCustomerSpecification':['Description'],'HKLActivityCode':['Description'],'OtherBranches':['Branch','Disabled','CompanyName','ISOCountryCode','FiscalCode','TaxIdNumber','TaxOffice','SIACode','FederalState','ZIPCode','Address','StreetNo','Address2','District','City','City','ZIPCode','County','Region','Country','Latitude','Longitude','Telephone1','Telephone2','Telex','Fax','SkypeID','Internet','EMail','MailSendingType','WorkingTime','ContactPerson','Language','Salesperson','AreaManager','Notes'],'People':['TitleCode','ExternalCode','LastName','Name','Telephone1','Telephone2','Telex','Fax','Branch','WorkingPosition','Notes','LegalRepresentative','EMail','SkypeID','AllDocuments','CustomerQuotation','SaleOrder','PickingList','DeliveryNote','ReturnFromCustomer','ProformaInvoice','InvoiceForAdvance','Invoice','AccompanyingInvoices','CorrectionAccInvoice','CorrectionInvoice','CreditNote','DebitNote','NonCollectedReceipt','Receipt','CorrectionReceipt','Receipt','CorrectionReceipt && !ERP.MasterData_BR','NotaFiscal'],'HKLPaymentTerms':['Description'],'HKLCurrencies':['Description'],'HKLCustSuppBank':['Description','Description'],'HKLCompanyBank':['Description','Description'],'HKLBanksCAsCustomers':['IBAN'],'Outstandings':['FiscalYear','NoOfOutstandings','OutstandingsTotAmt','FiscalYear','NoOfOutstandings','OutstandingsTotAmt'],'CustomerCreditLimit':['CreditLimitManage','MaxOrderValue','MaxOrderValueDate','MaxOrderValueCheckType','MaximumCredit','MaximumCreditDate','MaximumCreditCheckType','MaximumCreditCheckTypeDelDoc','MaximumCreditCheckTypeDefInv','MaximumCreditCheckTypeImmInv','MaxOrderedValue','MaxOrderedValueDate','MaxOrderedValueCheckType','TotalExposure','TotalExposureDate','TotalExposureCheckType','TotalExposureCheckTypeDelDoc','TotalExposureCheckTypeDefInv','TotalExposureCheckTypeImmInv'],'HKLLanguages':['Description'],'HKLCustContractsByType':['Description'],'HKLAllocationArea':['Description'],'HKLPriceLists':['Description','Description'],'PriceListForCommodityCtg':['Category','PriceList'],'HKLCommodityCtgBE':['Description'],'HKLPriceListsBE':['Description'],'HKLCustomer':['CompanyName','CompanyName'],'HKLTransport':['Description','Description'],'HKLPackages':['Description','Description'],'HKLShippingBy':['Description'],'HKLPorts':['Description'],'HKLCarriers1':['CompanyName'],'HKLCarriers2':['CompanyName'],'HKLCarriers3':['CompanyName'],'HKLSalesPeople':['Name','Name'],'HKLAreaManager':['Name','Name'],'HKLCommissionCtg':['Description','Description'],'HKLSaleAreas':['Description','Description'],'HKLConsignmentPartner':['CompanyName','CompanyName'],'Budget':['BalanceYear','BalanceMonth','TurnoverBudget','ActualTurnover','ActualCreditNotesAmount','OrderedBudget','ActualOrdered'],'HKLPymtAccount':['Description'],'HKLJobs':['Description','Description'],'HKLCostCenters':['Description','Description'],'HKLTax':['Description'],'HKLChartOfAccounts':['Description'],'HKLChartOfAccountsGoods':['Description'],'HKLChartOfAccountsServices':['Description'],'Balances':['Nature','Period','Debit','Credit'],'HKLFiscalCtg':['Description'],'CustSuppBRTaxes':['ISSTaxRateCode','ISSWithHoldingTax','PISWithHoldingTax','IRWithHoldingTax','COFINSWithHoldingTax','CSWithHoldingTax','INSSWithHoldingTax'],'HKLBRISSTaxRateCode':['Description'],'Offsets':['OffsetSymbol','OffsetSymbolDescription','Offset','OffsetDescription'],'DeclarationOfIntents':['DeclYear','LogNo','DeclDate','CustomerNo','CustomerDate','DeclType','LimitAmount','LetterNotes','FromDate','ToDate','AnnulmentDate','Notes'],'Form':['DocumentNamespace','ReportNamespace','ReportDescription'],'CustSuppCircularLetters':['Template','Printed','PrintDate','Disabled'],'HKLCircularLetterTemplates':['Description'],'Notes':['Notes','CustQuota','SaleOrd','PickingList','DN','Proforma','InvoiceForAdv','Invoice','CorrInvoice','AccompanyingInvoices','CorrAccInvoice','Receipt','Receipt','CorrReceipt','CorrReceipt','NCReceipt','RetCust','CreditNote','DebitNote','ShowInSales','CopyInSales','ShowInAccounting','ShowInPureAccounting'],'DBTLinksTable':['Image','Description'],'Documents':['CreditType','DocumentType','DocumentNo','DocumentDate','Amount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUSTOMERS_COMPLETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUSTOMERS_COMPLETEComponent, resolver);
    }
} 