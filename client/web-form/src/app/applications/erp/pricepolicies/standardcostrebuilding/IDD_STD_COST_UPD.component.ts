﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STD_COST_UPDService } from './IDD_STD_COST_UPD.service';

@Component({
    selector: 'tb-IDD_STD_COST_UPD',
    templateUrl: './IDD_STD_COST_UPD.component.html',
    providers: [IDD_STD_COST_UPDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_STD_COST_UPDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STD_COST_UPDService,
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
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','UpdateStandardCost','UpdateBasePrice','ValidityDate','Standard','Average','Last','LIFO','FIFO','BasePrice','CostSel','DeltaCost','BasePriceIncludesTax','Rounding','RoundType','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STD_COST_UPDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STD_COST_UPDComponent, resolver);
    }
} 