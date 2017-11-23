import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CS_BRANCHES_ADD_ON_FLYService } from './IDD_CS_BRANCHES_ADD_ON_FLY.service';

@Component({
    selector: 'tb-IDD_CS_BRANCHES_ADD_ON_FLY',
    templateUrl: './IDD_CS_BRANCHES_ADD_ON_FLY.component.html',
    providers: [IDD_CS_BRANCHES_ADD_ON_FLYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CS_BRANCHES_ADD_ON_FLYComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BE_CS_REGION_itemSource: any;
public IDC_BE_CS_STATUS_itemSource: any;

    constructor(document: IDD_CS_BRANCHES_ADD_ON_FLYService,
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

        const boService = this.document as BOService;
		boService.appendToModelStructure({'OtherBranches':['Branch','Disabled','CompanyName','ISOCountryCode','FiscalCode','TaxIdNumber','TaxOffice','SIACode','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','City','ZIPCode','County','Region','Country','Latitude','Longitude','Telephone1','Telephone2','Telex','Fax','ContactPerson','WorkingTime','EMail','MailSendingType','SkypeID','Internet','Notes','Language'],'HKLBranchLanguage':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CS_BRANCHES_ADD_ON_FLYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CS_BRANCHES_ADD_ON_FLYComponent, resolver);
    }
} 