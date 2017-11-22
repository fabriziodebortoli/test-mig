﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RICBALANCESService } from './IDD_RICBALANCES.service';

@Component({
    selector: 'tb-IDD_RICBALANCES',
    templateUrl: './IDD_RICBALANCES.component.html',
    providers: [IDD_RICBALANCESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RICBALANCESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RICBALANCESService,
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
		boService.appendToModelStructure({'global':['bAllMonths','bMonthSelection','FromMonth','bAllStorage','bSelStorage','Storage','bSelStorage','Storage','SpecificatorType','Specificator','bAllVariants','bVariantSelection','VariantSelected','bAllLots','bLotSelection','LotSelected','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','OpeningDate','ClosingDate','bAlsoInitFiscalData','bPreviousYearSetting','bOrderRebuild','bResProdRebuild','bAllocatedRebuild','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RICBALANCESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RICBALANCESComponent, resolver);
    }
} 