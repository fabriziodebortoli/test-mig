import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDISTRIBUTIONService } from './IDD_TAXDISTRIBUTION.service';

@Component({
    selector: 'tb-IDD_TAXDISTRIBUTION',
    templateUrl: './IDD_TAXDISTRIBUTION.component.html',
    providers: [IDD_TAXDISTRIBUTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXDISTRIBUTIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDISTRIBUTIONService,
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
		boService.appendToModelStructure({'global':['UseTaxDistribution','ReDistribution','FromPeriod','ToPeriod','AllPurchase','PurchaseSel','FromPurchaseTaxJournal','ToPurchaseTaxJournal','AllRetail','RetailSel','FromRetailSaleTaxJournal','ToRetailSaleTaxJournal','UpdateSummary','Preview','DotMatrixPrinter','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDISTRIBUTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDISTRIBUTIONComponent, resolver);
    }
} 