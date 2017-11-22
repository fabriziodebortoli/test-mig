import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GEN_NOPICKINGLISTSService } from './IDD_GEN_NOPICKINGLISTS.service';

@Component({
    selector: 'tb-IDD_GEN_NOPICKINGLISTS',
    templateUrl: './IDD_GEN_NOPICKINGLISTS.component.html',
    providers: [IDD_GEN_NOPICKINGLISTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_GEN_NOPICKINGLISTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GEN_NOPICKINGLISTSService,
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
		boService.appendToModelStructure({'global':['bAllMO','bManOrdSel','FromMONo','ToMONo','bAllJT','bJTSel','FromBoLNo','ToBoLNo','bJOBALL','bJOBSel','FromJOB','ToJOB','bAllDate','bSelData','DateFrom','DateTo','Storage','StorageSemifinished','PickingListDetails'],'PickingListDetails':['Selection','Component','ComponentsDes','Variant','UoM','NeededQty','EstimatedUseDate','MONo','JobTicketNo','NotFoundQty','Updated']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_GEN_NOPICKINGLISTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GEN_NOPICKINGLISTSComponent, resolver);
    }
} 