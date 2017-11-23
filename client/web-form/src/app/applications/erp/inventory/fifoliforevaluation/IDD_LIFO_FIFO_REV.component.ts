import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LIFO_FIFO_REVService } from './IDD_LIFO_FIFO_REV.service';

@Component({
    selector: 'tb-IDD_LIFO_FIFO_REV',
    templateUrl: './IDD_LIFO_FIFO_REV.component.html',
    providers: [IDD_LIFO_FIFO_REVService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LIFO_FIFO_REVComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LIFO_FIFO_REVService,
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
        
        		this.bo.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','bFIFORevaluate','bLIFORevaluate','OperationDate','RevaluateInvRsn','FIFOLIFORevaluation'],'FIFOLIFORevaluation':['IsSelected','Item','Description','BaseUoM','FinalBookInv','BookInvValue','UnitValue','RevaluateBookInvValue','Note']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LIFO_FIFO_REVFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LIFO_FIFO_REVComponent, resolver);
    }
} 