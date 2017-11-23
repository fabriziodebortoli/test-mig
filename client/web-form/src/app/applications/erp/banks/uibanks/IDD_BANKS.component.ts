import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BANKSService } from './IDD_BANKS.service';

@Component({
    selector: 'tb-IDD_BANKS',
    templateUrl: './IDD_BANKS.component.html',
    providers: [IDD_BANKSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BANKSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BANKS_STATUS_itemSource: any;
public IDC_BANKS_COUNTY_itemSource: any;

    constructor(document: IDD_BANKSService,
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
        this.IDC_BANKS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 
this.IDC_BANKS_COUNTY_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.County"
}; 

        		this.bo.appendToModelStructure({'Banks':['Bank','Disabled','Description','IsForeign','ISOCountryCode','ABI','ABIPrefix','CAB','CABPrefix','Swift','SIACode','CBICode','Bank','Disabled','Description','IsForeign','ISOCountryCode','ABI','ABIPrefix','CAB','CABPrefix','Swift','SIACode','CBICode','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Counter','Agency','Branch','Address','Address2','City','ZIPCode','County','Country','Counter','Agency','Branch','Telephone1','Telephone2','Telex','Fax','ContactPerson','EMail','Internet','Identifier','Signature','Notes','BankDays','SenderCode','SenderReference'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BANKSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BANKSComponent, resolver);
    }
} 