import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_REBUILD_ORDEREDService } from './IDD_REBUILD_ORDERED.service';

@Component({
    selector: 'tb-IDD_REBUILD_ORDERED',
    templateUrl: './IDD_REBUILD_ORDERED.component.html',
    providers: [IDD_REBUILD_ORDEREDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_REBUILD_ORDEREDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_REBUILD_ORDEREDService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_REBUILD_ORDEREDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_REBUILD_ORDEREDComponent, resolver);
    }
} 