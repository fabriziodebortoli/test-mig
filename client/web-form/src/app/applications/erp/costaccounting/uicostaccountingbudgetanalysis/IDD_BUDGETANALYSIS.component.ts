import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BUDGETANALYSISService } from './IDD_BUDGETANALYSIS.service';

@Component({
    selector: 'tb-IDD_BUDGETANALYSIS',
    templateUrl: './IDD_BUDGETANALYSIS.component.html',
    providers: [IDD_BUDGETANALYSISService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BUDGETANALYSISComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BUDGETANALYSISService,
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
		boService.appendToModelStructure({'global':['strBalanceType','strType','AllRadio','RadioSel','FromCode','ToCode','strBalanceBy','AllRadio','RadioSel','FromCode','ToCode','AllNatureRadio','NatureRadioSel','CostCenterNature','OnDate','bFromCreationDate','bEveryone','PeriodNature','FiscalYear','OnDate','bEveryone','PeriodNature','strReportLayout','strScale']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BUDGETANALYSISFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BUDGETANALYSISComponent, resolver);
    }
} 