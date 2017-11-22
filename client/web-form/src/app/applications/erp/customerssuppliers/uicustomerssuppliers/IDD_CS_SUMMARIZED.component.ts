import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CS_SUMMARIZEDService } from './IDD_CS_SUMMARIZED.service';

@Component({
    selector: 'tb-IDD_CS_SUMMARIZED',
    templateUrl: './IDD_CS_SUMMARIZED.component.html',
    providers: [IDD_CS_SUMMARIZEDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CS_SUMMARIZEDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CS_REGION_itemSource: any;
public IDC_CS_STATUS_itemSource: any;
public IDC_CS_FP_COUNTYOFBIRTH_itemSource: any;

    constructor(document: IDD_CS_SUMMARIZEDService,
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
        this.IDC_CS_REGION_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.Region"
}; 
this.IDC_CS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_CS_FP_COUNTYOFBIRTH_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'CustomersSuppliers':['CustSupp','CompanyName','Draft','IsCustoms','TitleCode','NaturalPerson','ISOCountryCode','CustSuppKind','FiscalCode','TaxIdNumber','InsertionDate','ActivityCode','FantasyName','Address','Address2','City','ZIPCode','County','Region','Country','Latitude','Longitude','Telephone1','Telephone2','Telex','Fax','ContactPerson','WorkingPosition','WorkingTime','EMail','CertifiedEMail','SkypeID','Internet','Payment','CustSuppBank','Currency','InCurrency','UsedForSummaryDocuments','Account','ChambOfCommRegistrNo','UsedForSummaryDocuments','Account','ChambOfCommRegistrNo','InvoiceAccTpl','CreditNoteAccTpl','CustSupp','CompanyName','Draft','IsCustoms','TitleCode','NaturalPerson','ISOCountryCode','CustSuppKind','FiscalCode','TaxIdNumber','InsertionDate','ActivityCode','FantasyName','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Latitude','Longitude','TaxpayerType','FedStateReg','MunicipalityReg','GenRegNo','GenRegEntity','SUFRAMA','Telephone1','Telephone2','Telex','Fax','ContactPerson','WorkingPosition','WorkingTime','EMail','CertifiedEMail','SkypeID','Internet','Payment','CustSuppBank','Currency','InCurrency'],'HKLTitles':['Description','Description'],'global':['FiscalCodeCalculate','FiscalCodeCalculate','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLActivityCode':['Description','Description'],'NaturalPerson':['LastName','Name','Professional','Gender','DateOfBirth','ISOCountryCodeOfBirth','CityOfBirth','CountyOfBirth','LastName','Name','Professional','Gender','DateOfBirth','ISOCountryCodeOfBirth','CityOfBirth','CountyOfBirth'],'HKLPaymentTerms':['Description','Description'],'HKLCustSuppBank':['Description','Description'],'HKLCurrencies':['Description','Description'],'Options':['TaxCode','ExemptFromTax','AdditionalTax','IsAPrivatePerson','SuspendedTax','PublicAuthority','PASplitPayment','PASplitPayment','WithholdingTaxManagement','WithholdingTaxBasePerc','WithholdingTaxPerc','TaxCode','ExemptFromTax','SuspendedTax'],'HKLTax':['Description','Description'],'HKLChartOfAccounts':['Description','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CS_SUMMARIZEDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CS_SUMMARIZEDComponent, resolver);
    }
} 