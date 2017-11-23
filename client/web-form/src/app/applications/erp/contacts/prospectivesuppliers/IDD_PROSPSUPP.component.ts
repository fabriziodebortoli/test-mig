import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PROSPSUPPService } from './IDD_PROSPSUPP.service';

@Component({
    selector: 'tb-IDD_PROSPSUPP',
    templateUrl: './IDD_PROSPSUPP.component.html',
    providers: [IDD_PROSPSUPPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PROSPSUPPComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PROSPSUPP_STATUS_itemSource: any;
public IDC_PROSPSUPP_REGION_itemSource: any;
public IDC_PROSPSUPP_BRANCHES_COUNTY_itemSource: any;
public IDC_PROSPSUPP_BRANCHES_REGION_itemSource: any;
public IDC_PROSPSUPP_BRANCHES_STATUS_itemSource: any;
public IDC_PROSPSUPP_PYMTBRANCH_itemSource: any;

    constructor(document: IDD_PROSPSUPPService,
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
        this.IDC_PROSPSUPP_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_PROSPSUPP_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 
this.IDC_PROSPSUPP_BRANCHES_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_PROSPSUPP_BRANCHES_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 
this.IDC_PROSPSUPP_BRANCHES_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_PROSPSUPP_PYMTBRANCH_itemSource = {
  "name": "BranchProspectiveSupplierCombo",
  "namespace": "ERP.Contacts.Documents.BranchProspectiveSupplierCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'ProspectiveSuppliers':['ProspectiveSupplier','Disabled','CompanyName','TitleCode','NaturalPerson','ISOCountryCode','ProspSuppKind','FiscalCode','TaxIdNumber','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Latitude','Longitude','Address','Address2','City','ZIPCode','County','Region','Country','Latitude','Longitude','FromDate','IsACustSupp','CustSupp','ConversionDate','Category','ContactOrigin','ContactSpecification','Telephone1','Telephone2','Telex','Fax','ContactPerson','WorkingTime','EMail','SkypeID','Internet','DocumentSendingType','MailSendingType','NoSendPostaLite','TaxpayerType','FedStateReg','MunicipalityReg','GenRegNo','GenRegEntity','SUFRAMA','TaxOffice','CompanyRegistrNo','PaymentAddress','Payment','CustSuppBank','Currency','Language','Port','DiscountFormula','TaxCode','ExemptFromTax','SuspendedTax','Account','GoodsOffset','ServicesOffset','Notes'],'HKLTitles':['Description'],'HKLSupplierCtg':['Description'],'HKLContactOrigin':['Description'],'HKLContactSpecification':['Description'],'global':['Branches','PaymentAddressDescri','People','Notes','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Branches':['Branch','Disabled','Companyname','ISOCountryCode','FiscalCode','TaxIdNumber','TaxOffice','FederalState','ZIPCode','Address','StreetNo','Address2','District','City','ZIPCode','County','Region','Country','Latitude','Longitude','Telephone1','Telephone2','Telex','Fax','SkypeID','Internet','Email','MailSendingType','WorkingTime','ContactPerson','Language','Notes'],'People':['TitleCode','ExternalCode','LastName','Name','Telephone1','Telephone2','Telex','Fax','Email','MailSendingType','SkypeID','Branch','WorkingPosition','Notes'],'HKLPymtTerm':['Description'],'HKLSupportBank':['Description'],'HKLCurrencies':['Description'],'HKLLanguages':['Description'],'HKLPorts':['Description'],'HKLTax':['Description'],'HKLChartOfAccounts':['Description'],'HKLChartOfAccountsGoods':['Description'],'HKLChartOfAccountsServices':['Description'],'Notes':['Notes','PrintSuppQuota','ShowSuppQuota','CopySuppQuota']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PROSPSUPPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PROSPSUPPComponent, resolver);
    }
} 