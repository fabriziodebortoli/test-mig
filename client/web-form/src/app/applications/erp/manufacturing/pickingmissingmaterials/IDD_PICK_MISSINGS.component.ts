import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PICK_MISSINGSService } from './IDD_PICK_MISSINGS.service';

@Component({
    selector: 'tb-IDD_PICK_MISSINGS',
    templateUrl: './IDD_PICK_MISSINGS.component.html',
    providers: [IDD_PICK_MISSINGSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PICK_MISSINGSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PICK_MISSINGSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllMO','bManOrdSel','FromMONo','ToMONo','bAllJT','bJTSel','FromBoLNo','ToBoLNo','bAllDate','bSelData','DateFrom','DateTo','Storage','StorageSemifinished','PickingListDetails'],'PickingListDetails':['Selection','Component','ComponentsDes','Variant','UoM','NeededQty','EstimatedUseDate','MONo','JobTicketNo','NotFoundQty','Updated']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PICK_MISSINGSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PICK_MISSINGSComponent, resolver);
    }
} 