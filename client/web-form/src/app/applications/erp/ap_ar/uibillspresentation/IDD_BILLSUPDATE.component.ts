import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BILLSUPDATEService } from './IDD_BILLSUPDATE.service';

@Component({
    selector: 'tb-IDD_BILLSUPDATE',
    templateUrl: './IDD_BILLSUPDATE.component.html',
    providers: [IDD_BILLSUPDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BILLSUPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BILLSUPDATEService,
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
		boService.appendToModelStructure({'global':['Bills','TotalAmount'],'Bills':['l_TEnhBills_P01','l_TEnhBills_P08','l_TEnhBills_P13','Amount','l_TEnhBills_P04']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BILLSUPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BILLSUPDATEComponent, resolver);
    }
} 