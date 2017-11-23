import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_UPDATE_SALESPEOPLEService } from './IDD_UPDATE_SALESPEOPLE.service';

@Component({
    selector: 'tb-IDD_UPDATE_SALESPEOPLE',
    templateUrl: './IDD_UPDATE_SALESPEOPLE.component.html',
    providers: [IDD_UPDATE_SALESPEOPLEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_UPDATE_SALESPEOPLEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_UPDATE_SALESPEOPLE_COUNTY_itemSource: any;

    constructor(document: IDD_UPDATE_SALESPEOPLEService,
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
        this.IDC_UPDATE_SALESPEOPLE_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllSaleAreas','bSelectSaleAreas','SaleAreas','bAllCounty','bSelectCounty','County','SalesPeopleOld','SalesPeopleNew','bSalesPerson','bAlsoBranches','bAreaManager','UpdateSalesPeople'],'HKLSalesPeopleOld':['Name'],'HKLSalesPeopleNew':['Name'],'UpdateSalesPeople':['Selection','CustSupp','CompanyName','Address','ZIPCode','City','Country','Area','Salesperson','AreaManager','NaturalPerson','TaxIdNumber','FiscalCode','Telephone1','Telephone2','EMail','SkypeID','Branches']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_UPDATE_SALESPEOPLEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_UPDATE_SALESPEOPLEComponent, resolver);
    }
} 