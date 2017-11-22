import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CONTACTSService } from './IDD_CONTACTS.service';

@Component({
    selector: 'tb-IDD_CONTACTS',
    templateUrl: './IDD_CONTACTS.component.html',
    providers: [IDD_CONTACTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CONTACTSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CONTACTS_STATUS_itemSource: any;
public IDC_CONTACTS_REGION_itemSource: any;
public IDC_CONTACTS_BRANCHES_COUNTY_itemSource: any;
public IDC_CONTACTS_BRANCHES_REGION_itemSource: any;
public IDC_CONTACTS_BRANCHES_STATUS_itemSource: any;
public IDC_CONTACTS_DOCBRANCH_itemSource: any;
public IDC_CONTACTS_SHIPBRANCH_itemSource: any;
public IDC_CONTACTS_PYMTBRANCH_itemSource: any;

    constructor(document: IDD_CONTACTSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_CONTACTS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_CONTACTS_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 
this.IDC_CONTACTS_BRANCHES_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_CONTACTS_BRANCHES_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 
this.IDC_CONTACTS_BRANCHES_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_CONTACTS_DOCBRANCH_itemSource = {
  "name": "BranchContactsCombo",
  "namespace": "ERP.Contacts.Documents.BranchContactsCombo"
}; 
this.IDC_CONTACTS_SHIPBRANCH_itemSource = {
  "name": "BranchContactsCombo",
  "namespace": "ERP.Contacts.Documents.BranchContactsCombo"
}; 
this.IDC_CONTACTS_PYMTBRANCH_itemSource = {
  "name": "BranchContactsCombo",
  "namespace": "ERP.Contacts.Documents.BranchContactsCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Contacts':['Contact','Disabled','CompanyName','TitleCode','NaturalPerson','ISOCountryCode','ContactsKind','FiscalCode','TaxIdNumber','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Latitude','Longitude','Address','Address2','City','ZIPCode','County','Region','Country','Latitude','Longitude','ContactDate','IsACustSupp','CustSupp','ConversionDate','Category','ContactOrigin','ContactSpecification','Telephone1','Telephone2','Telex','Fax','ContactPerson','WorkingTime','EMail','SkypeID','Internet','DocumentSendingType','MailSendingType','NoSendPostaLite','TaxpayerType','FedStateReg','MunicipalityReg','GenRegNo','GenRegEntity','SUFRAMA','TaxOffice','CompanyRegistrNo','SendDocumentsTo','ShipToAddress','PaymentAddress','Payment','Bank','Currency','Language','Port','PriceList','DiscountFormula','Salesperson','AreaManager','CommissionCtg','Area','TaxCode','ExemptFromTax','AdditionalTax','SuspendedTax','Account','GoodsOffset','ServicesOffset','Notes'],'HKLTitles':['Description'],'HKLCustCtg':['Description'],'HKLContactOrigin':['Description'],'HKLContactSpecification':['Description'],'global':['Branches','SendDocumentsToDescri','ShipToAddressDescri','PaymentAddressDescri','People','Notes','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Branches':['Branch','Disabled','Companyname','ISOCountryCode','FiscalCode','TaxIdNumber','TaxOffice','FederalState','ZIPCode','Address','StreetNo','Address2','District','City','ZIPCode','County','Region','Country','Latitude','Longitude','Telephone1','Telephone2','Telex','Fax','SkypeID','Internet','EMail','MailSendingType','WorkingTime','ContactPerson','Language','Salesperson','AreaManager','Notes'],'People':['TitleCode','ExternalCode','LastName','Name','Telephone1','Telephone2','Telex','Fax','EMail','MailSendingType','SkypeID','Branch','WorkingPosition','Notes'],'HKLPymtTerm':['Description'],'HKLSupportBank':['Description'],'HKLCurrencies':['Description'],'HKLLanguages':['Description'],'HKLPorts':['Description'],'HKLPriceLists':['Description'],'HKLSalesperson':['Name'],'HKLAreaManager':['Name'],'HKLCtgCommissions':['Description'],'HKLArea':['Description'],'HKLTax':['Description'],'HKLChartOfAccounts':['Description'],'HKLChartOfAccountsGoods':['Description'],'HKLChartOfAccountsServices':['Description'],'Notes':['Notes','PrintCustQuota','ShowCustQuota','CopyCustQuota']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CONTACTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CONTACTSComponent, resolver);
    }
} 