import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MRP_DEFAULTASSIGNATIONService } from './IDD_MRP_DEFAULTASSIGNATION.service';

@Component({
    selector: 'tb-IDD_MRP_DEFAULTASSIGNATION',
    templateUrl: './IDD_MRP_DEFAULTASSIGNATION.component.html',
    providers: [IDD_MRP_DEFAULTASSIGNATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MRP_DEFAULTASSIGNATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MRP_DEFAULTASSIGNATIONService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','bStockLevelHorizon','nStockLevelHorizon','bMinimumQty','nMinimumQty','bMRPPolicy','eMRPPolicy','bEOQ','nEOQ','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MRP_DEFAULTASSIGNATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MRP_DEFAULTASSIGNATIONComponent, resolver);
    }
} 