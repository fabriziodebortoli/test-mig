import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FISCALYEARGENERATIONService } from './IDD_FISCALYEARGENERATION.service';

@Component({
    selector: 'tb-IDD_FISCALYEARGENERATION',
    templateUrl: './IDD_FISCALYEARGENERATION.component.html',
    providers: [IDD_FISCALYEARGENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FISCALYEARGENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FISCALYEARGENERATIONService,
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
		boService.appendToModelStructure({'global':['PrevYearDescri','OpeningDatePrev','ClosingDatePrev','NewFiscalYearDescri','OpeningDate','ClosingDate','Year','OldTaxRegulations','OldSalesTaxPerc','OldSalesTaxPerc2','NewYear','TaxRegulations','SalesTaxPerc','SalesTaxPerc2','Year','TaxDeclaration','QuarterlyOptionStatic','CashTaxStatic','TaxDistribution','FarmerTaxStatic','NewYear','QuarterlyDecl','QuarterlyOption','CashTax','UseTaxDistribution','FarmerTax','Year','IntrastatPurchases','IntrastatSales','PurchasesStatisticalValue','SalesStatisticalValue','NewYear','PurchIntraSummary','SaleIntraSummary','StatisticalValueArrivals','StatisticalValueDispatches','bUpdateCounterTaxJournal','bUpdateCounterDeclarationOfIntentNos','bUpdateCounterStubBook','bUpdateCounterSerialNos','bUpdateCounterNonFiscals','bUpdateCounterLotNos','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FISCALYEARGENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FISCALYEARGENERATIONComponent, resolver);
    }
} 