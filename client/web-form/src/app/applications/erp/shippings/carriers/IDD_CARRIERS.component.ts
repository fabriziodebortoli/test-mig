import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CARRIERSService } from './IDD_CARRIERS.service';

@Component({
    selector: 'tb-IDD_CARRIERS',
    templateUrl: './IDD_CARRIERS.component.html',
    providers: [IDD_CARRIERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CARRIERSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CARRIERS_STATUS_itemSource: any;
public IDC_CARRIERS_COUNTY_itemSource: any;

    constructor(document: IDD_CARRIERSService,
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
        this.IDC_CARRIERS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_CARRIERS_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Carriers':['Carrier','Disabled','CompanyName','TitleCode','ISOCountryCode','FiscalCode','TaxIdNumber','Currency','EORICode','Notes','NaturalPerson','Name','LastName','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','FedStateReg','Address','Address2','City','ZIPCode','County','Telephone1','Telephone2','Telex','Fax','EMail','TaxOffice','CompanyRegistrNo','TransportationForm','RoadHaulageContractorRegister','PackCharges','ShippingCharges','ChargesPercOnTotAmt','Allowed','InsuredGood'],'HKLTitles':['Description'],'HKLCurrencies':['Description'],'HKLISOCountryCodes':['Description','Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CARRIERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CARRIERSComponent, resolver);
    }
} 