import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SLIPSService } from './IDD_SLIPS.service';

@Component({
    selector: 'tb-IDD_SLIPS',
    templateUrl: './IDD_SLIPS.component.html',
    providers: [IDD_SLIPSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SLIPSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SLIPSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BOM','PresDate','Bank','CA','Presentation','Detail','BillsTypes','NrBills','Currency','TotalAmount','NrCollected','TotColl'],'HKLBank':['Description'],'Detail':['l_TEnhSlipsDetail_P01','BillNo','PaymentTerm','PaymentTerm','CustSupp','l_TEnhSlipsDetail_P02','InstallmentDate','ValueDate','l_TEnhSlipsDetail_P06','l_TEnhSlipsDetail_P07','Amount','Approved','Collected','l_TEnhSlipsDetail_P03','PresentationNotes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SLIPSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SLIPSComponent, resolver);
    }
} 