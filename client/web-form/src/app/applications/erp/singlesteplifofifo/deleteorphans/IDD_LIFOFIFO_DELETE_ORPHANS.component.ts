import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LIFOFIFO_DELETE_ORPHANSService } from './IDD_LIFOFIFO_DELETE_ORPHANS.service';

@Component({
    selector: 'tb-IDD_LIFOFIFO_DELETE_ORPHANS',
    templateUrl: './IDD_LIFOFIFO_DELETE_ORPHANS.component.html',
    providers: [IDD_LIFOFIFO_DELETE_ORPHANSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LIFOFIFO_DELETE_ORPHANSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LIFOFIFO_DELETE_ORPHANSService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','bDeleteLoadsWithoutLinkedInvEntries','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LIFOFIFO_DELETE_ORPHANSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LIFOFIFO_DELETE_ORPHANSComponent, resolver);
    }
} 