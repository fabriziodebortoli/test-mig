import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OPENORDERS_CUSTOMERCONTRACTSService } from './IDD_OPENORDERS_CUSTOMERCONTRACTS.service';

@Component({
    selector: 'tb-IDD_OPENORDERS_CUSTOMERCONTRACTS',
    templateUrl: './IDD_OPENORDERS_CUSTOMERCONTRACTS.component.html',
    providers: [IDD_OPENORDERS_CUSTOMERCONTRACTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_OPENORDERS_CUSTOMERCONTRACTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_OPENORDERS_CUSTOMERCONTRACTSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'CustContracts':['StartValidityDate','ModificationsHistory','Disabled','ContractNo','Description','Customer','Validity','ContractType','Notes'],'HKLCustomer':['CompNameComplete'],'global':['CustContractsDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'CustContractsDetails':['Horizon','ConfirmationLevel','ConfirmationLevelDescr','PeriodType','ReferenceDay','DailySplit']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OPENORDERS_CUSTOMERCONTRACTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OPENORDERS_CUSTOMERCONTRACTSComponent, resolver);
    }
} 