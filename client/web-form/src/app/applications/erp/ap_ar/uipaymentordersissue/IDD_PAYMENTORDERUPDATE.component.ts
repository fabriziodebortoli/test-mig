import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYMENTORDERUPDATEService } from './IDD_PAYMENTORDERUPDATE.service';

@Component({
    selector: 'tb-IDD_PAYMENTORDERUPDATE',
    templateUrl: './IDD_PAYMENTORDERUPDATE.component.html',
    providers: [IDD_PAYMENTORDERUPDATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PAYMENTORDERUPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PAYMENTORDERUPDATEService,
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
		boService.appendToModelStructure({'global':['PymtOrders','TotalAmount'],'PymtOrders':['l_TEnhPymtOrders_P01','l_TEnhPymtOrders_P08','l_TEnhPymtOrders_P13','Amount','l_TEnhPymtOrders_P04']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYMENTORDERUPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYMENTORDERUPDATEComponent, resolver);
    }
} 