import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SUPPLIERS_COMPLETEService } from './IDD_SUPPLIERS_COMPLETE.service';

@Component({
    selector: 'tb-IDD_SUPPLIERS_COMPLETE',
    templateUrl: './IDD_SUPPLIERS_COMPLETE.component.html',
    providers: [IDD_SUPPLIERS_COMPLETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SUPPLIERS_COMPLETEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CS_STATUS_itemSource: any;
public IDC_CS_REGION_itemSource: any;
public IDC_CS_FP_COUNTYOFBIRTH_itemSource: any;
public IDC_CS_CHAMBOFCOMMCOUNTY_itemSource: any;
public IDC_BE_CS_REGION_itemSource: any;
public IDC_BE_CS_STATUS_itemSource: any;
public IDC_CS_BRANCHSENTDOC_itemSource: any;
public IDC_CS_BRANCHSENTGOODS_itemSource: any;
public IDC_CS_BRANCHSENTBILLS_itemSource: any;
public IDC_CS_770LETTER_itemSource: any;
public IDC_CS_CURRENCY_FOR_BALANCES_itemSource: any;
public IDC_CUSTSUPP_OFFSETS_SYMBOL_itemSource: any;

    constructor(document: IDD_SUPPLIERS_COMPLETEService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
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
this.IDC_CS_770LETTER_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.ReasonCU"
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
		boService.appendToModelStructure({'CustomersSuppliers':['CustSupp','Disabled','CompanyName','Draft','IsCustoms','IsCustoms','TitleCode','NaturalPerson','ISOCountryCode','CustSuppKind','FiscalCode','TaxIdNumber','FantasyName','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Latitude','Longitude','Address','Address2','City','ZIPCode','County','Region','Country','Latitude','Longitude','InsertionDate','LinkedCustSupp','ExternalCode','OldCustSupp','ActivityCode','Telephone1','Telephone2','Telex','Fax','ContactPerson','WorkingPosition','WorkingTime','EMail','CertifiedEMail','SkypeID','Internet','DocumentSendingType','MailSendingType','NoSendPostaLite','TaxpayerType','FedStateReg','MunicipalityReg','GenRegNo','GenRegEntity','SUFRAMA','TaxOffice','ChambOfCommCounty','CompanyRegistrNo','ChambOfCommRegistrNo','SendDocumentsTo','ShipToAddress','PaymentAddress','Payment','Currency','InCurrency','CustSuppBank','CA','CustSuppBank','CA','IBANIsManual','CIN','CACheck','CIN','CACheck','IBAN','CBICode','SIACode','CompanyBank','CompanyCA','Language','DiscountFormula','DiscountFormula','Storage','Storage','PymtAccount','Job','CostCenter','UsedForSummaryDocuments','InvoiceAccTpl','CreditNoteAccTpl','Job','CostCenter','Account','FiscalCtg','OMNIASubAccount','NoTaxComm','NoBlackList','PrivacyStatement','PrivacyStatementPrintDate','Notes'],'Options':['Blocked','Blocked','Category','SupplierClassification','SupplierSpecification','GroupPymtOrders','BlockPayments','ChargesType','CustomTariff','CashOnDeliveryLevel','CashOnDeliveryLevelDate','ReferencesPrintType','LastDocNo','LastDocDate','LastDocTotal','LastPaymentTerm','GroupCollectionCharges','ShowPricesOnDN','GroupCollectionCharges','GroupStampCharges','Transport','Package','Shipping','Transport','Package','Port','Carrier1','Carrier2','Carrier3','Salesperson','Area','NoDNGeneration','Salesperson','Area','NoDNGeneration','TaxCode','ExemptFromTax','SuspendedTax','CreditFreeSamplesTaxAmount','GoodsOffset','ServicesOffset'],'HKLTitles':['Description'],'global':['FiscalCodeCalculate','OtherBranches','DocBranchDescri','GoodBranchDescri','BillBranchDescri','People','BudIOrderedTot','OrdTot','BudInvoicedTot','InvoicedTot','Budget','NrFiscalYear','BalanceCurrency','DebitActTotal','CreditActTotal','ActualDebitBalance','ActualCreditBalance','DebitForTotal','CreditForTotal','BudgetDebitBalance','DebitTotal','CreditTotal','DebitBalance','CreditBalance','Balances','Offsets','DeclarationOfIntents','Form','CustSuppCircularLetters','Notes','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'NaturalPerson':['LastName','Name','Professional','Gender','DateOfBirth','ISOCountryCodeOfBirth','CityOfBirth','CountyOfBirth','FeeTpl','Form770Letter','INPSAccount'],'HKLSuppCtg':['Description'],'HKLSupplierClassification':['Description'],'HKLSupplierSpecification':['Description'],'HKLActivityCode':['Description'],'OtherBranches':['Branch','Disabled','CompanyName','ISOCountryCode','FiscalCode','TaxIdNumber','TaxOffice','SIACode','FederalState','ZIPCode','Address','StreetNo','Address2','District','City','City','ZIPCode','County','Region','Country','Latitude','Longitude','Telephone1','Telephone2','Telex','Fax','SkypeID','Internet','EMail','MailSendingType','WorkingTime','ContactPerson','Language','Salesperson','AreaManager','Notes'],'People':['TitleCode','ExternalCode','LastName','Name','Telephone1','Telephone2','Telex','Fax','Branch','WorkingPosition','Notes','LegalRepresentative','EMail','SkypeID','AllDocuments','SupplierQuotation','PurchaseOrder','ReturnToSupplier','SubcontrDeliveryNote','NotaFiscal'],'HKLPaymentTerms':['Description'],'HKLCurrencies':['Description'],'HKLCustSuppBank':['Description','Description'],'HKLCompanyBank':['Description'],'HKLLanguages':['Description'],'HKLTransport':['Description','Description'],'HKLPackages':['Description','Description'],'HKLShippingBy':['Description'],'HKLPorts':['Description'],'HKLCarriers1':['CompanyName'],'HKLCarriers2':['CompanyName'],'HKLCarriers3':['CompanyName'],'HKLSalesPeople':['Name','Name'],'HKLSaleAreas':['Description','Description'],'Budget':['BalanceYear','BalanceMonth','TurnoverBudget','ActualTurnover','ActualCreditNotesAmount','OrderedBudget','ActualOrdered'],'HKLPymtAccount':['Description'],'HKLJobs':['Description','Description'],'HKLCostCenters':['Description','Description'],'HKLTax':['Description'],'HKLChartOfAccounts':['Description'],'HKLChartOfAccountsGoods':['Description'],'HKLChartOfAccountsServices':['Description'],'HKLFeeTemplates':['Description'],'HKLChartOfAccountsINPS':['Description'],'Balances':['Nature','Period','Debit','Credit'],'HKLFiscalCtg':['Description'],'CustSuppBRTaxes':['ISSTaxRateCode','ISSWithHoldingTax','PISWithHoldingTax','IRWithHoldingTax','COFINSWithHoldingTax','CSWithHoldingTax','INSSWithHoldingTax'],'HKLBRISSTaxRateCode':['Description'],'Offsets':['OffsetSymbol','OffsetSymbolDescription','Offset','OffsetDescription'],'DeclarationOfIntents':['DeclYear','LogNo','DeclDate','CustomerNo','CustomerDate','DeclType','LimitAmount','LetterNotes','FromDate','ToDate','AnnulmentDate','Notes'],'Form':['DocumentNamespace','ReportNamespace','ReportDescription'],'CustSuppCircularLetters':['Template','Printed','PrintDate','Disabled'],'HKLCircularLetterTemplates':['Description'],'Notes':['Notes','SuppQuota','PurchaseOrd','BillOfLading','PurchInvForAdv','PurchaseDoc','PurchCorrInv','RetSupp','CreditNote','DebitNote','SubcontrPurchOrder','SubcontrDeliveryNote','SubcontrBillOfLading','ShowInPurchases','CopyInPurchases','ShowInAccounting','ShowInPureAccounting'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SUPPLIERS_COMPLETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SUPPLIERS_COMPLETEComponent, resolver);
    }
} 