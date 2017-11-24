import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_BOMService } from './IDD_LOAD_BOM.service';

@Component({
    selector: 'tb-IDD_LOAD_BOM',
    templateUrl: './IDD_LOAD_BOM.component.html',
    providers: [IDD_LOAD_BOMService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOAD_BOMComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_BOMService,
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
        
        		this.bo.appendToModelStructure({'global':['BOMComponentsToCopy','BOMRoutingsToCopy'],'BOMComponentsToCopy':['Selected','ComponentType','Drawing','Component','Description','Variant','Qty','PercQty','UoM','ScrapQty','ScrapUM','ValidityStartingDate','ValidityEndingDate'],'BOMRoutingsToCopy':['Sel','RtgStep','Alternate','AltRtgStep','Operation','WC','IsWC','SetupTime','ProcessingTime','TotalTime','QueueTime','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_BOMFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_BOMComponent, resolver);
    }
} 