import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DOC_SUBCONTRACTING_COMPService } from './IDD_DOC_SUBCONTRACTING_COMP.service';

@Component({
    selector: 'tb-IDD_DOC_SUBCONTRACTING_COMP',
    templateUrl: './IDD_DOC_SUBCONTRACTING_COMP.component.html',
    providers: [IDD_DOC_SUBCONTRACTING_COMPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DOC_SUBCONTRACTING_COMPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DOC_SUBCONTRACTING_COMPService,
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
		boService.appendToModelStructure({'global':['bMOOnly','bAll','bReturnedMaterialReceipt','SubcntBoLMOComponentsList'],'SubcntBoLMOComponentsList':['Selection','Component','ComponentsDes','UoM','CompQty','CompBoLQty','NeededQty','PickedQuantity']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DOC_SUBCONTRACTING_COMPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DOC_SUBCONTRACTING_COMPComponent, resolver);
    }
} 