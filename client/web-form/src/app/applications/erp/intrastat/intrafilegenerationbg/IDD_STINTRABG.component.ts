import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STINTRABGService } from './IDD_STINTRABG.service';

@Component({
    selector: 'tb-IDD_STINTRABG',
    templateUrl: './IDD_STINTRABG.component.html',
    providers: [IDD_STINTRABGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_STINTRABGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STINTRABGService,
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
		boService.appendToModelStructure({'global':['SaleSummary','PurchSummary','Normal','Adjustment','Period','Year','PeriodAdj','YearAdj','DelegTaxIdNo','DelegFiscalCode','DelegCompanyName','DelegFamily','DelegTelephone','ShowEmptyFields','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STINTRABGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STINTRABGComponent, resolver);
    }
} 