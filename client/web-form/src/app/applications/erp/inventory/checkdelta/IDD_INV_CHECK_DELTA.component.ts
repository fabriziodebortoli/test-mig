import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INV_CHECK_DELTAService } from './IDD_INV_CHECK_DELTA.service';

@Component({
    selector: 'tb-IDD_INV_CHECK_DELTA',
    templateUrl: './IDD_INV_CHECK_DELTA.component.html',
    providers: [IDD_INV_CHECK_DELTAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INV_CHECK_DELTAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INV_CHECK_DELTAService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','RefDecimali','bCheckFinalData','bCheckInitialData','RefDecimali','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INV_CHECK_DELTAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INV_CHECK_DELTAComponent, resolver);
    }
} 