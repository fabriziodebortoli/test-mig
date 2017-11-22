import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMPANYGROUPSService } from './IDD_COMPANYGROUPS.service';

@Component({
    selector: 'tb-IDD_COMPANYGROUPS',
    templateUrl: './IDD_COMPANYGROUPS.component.html',
    providers: [IDD_COMPANYGROUPSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COMPANYGROUPSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_COMPANYGROUPS_STATUS_itemSource: any;

    constructor(document: IDD_COMPANYGROUPSService,
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
        this.IDC_COMPANYGROUPS_STATUS_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Company.State"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'CompanyGroups':['Company','CompanyName','ISOCountryCode','TaxIdNumber','TaxOffice','BusinessKind','Currency','Language','StatedCapital','ZIPCode','Address','StreetNo','Address2','District','FederalState','City','Country','Address','Address2','City','ZIPCode','County','Country','Telephone1','Telephone2','Telex','Fax','EMailAddress','InternetAddress'],'HKLCurrencies':['Description'],'HKLLanguages':['Description'],'global':['People','Notes','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Notes':['Notes','PublicCompany','UseInBalanceSheetNotes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMPANYGROUPSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMPANYGROUPSComponent, resolver);
    }
} 