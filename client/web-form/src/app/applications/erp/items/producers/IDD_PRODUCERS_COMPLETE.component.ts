import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRODUCERS_COMPLETEService } from './IDD_PRODUCERS_COMPLETE.service';

@Component({
    selector: 'tb-IDD_PRODUCERS_COMPLETE',
    templateUrl: './IDD_PRODUCERS_COMPLETE.component.html',
    providers: [IDD_PRODUCERS_COMPLETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRODUCERS_COMPLETEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PRODUCERS_COUNTY_itemSource: any;
public IDC_PRODUCERS_STATUS_itemSource: any;

    constructor(document: IDD_PRODUCERS_COMPLETEService,
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
        this.IDC_PRODUCERS_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 
this.IDC_PRODUCERS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Producers':['Producer','Disabled','CompanyName','ISOCountryCode','Notes','Address','Address2','StreetNo','District','City','ZIPCode','FederalState','Country','Address','Address2','City','ZIPCode','County','Country','Telephone1','Telephone2','Fax','ContactPerson','WorkingTime','EMail','Internet'],'global':['ProductCategory','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ProductCategory':['Category'],'HKLProductCtg':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRODUCERS_COMPLETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRODUCERS_COMPLETEComponent, resolver);
    }
} 