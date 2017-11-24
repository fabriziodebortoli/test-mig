import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXSUMMARYService } from './IDD_TAXSUMMARY.service';

@Component({
    selector: 'tb-IDD_TAXSUMMARY',
    templateUrl: './IDD_TAXSUMMARY.component.html',
    providers: [IDD_TAXSUMMARYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXSUMMARYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXSUMMARYService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        
        		this.bo.appendToModelStructure({'global':['Year','FromPeriod','Interests','ProRata','ToPeriod','bSelWithinDate','WithinDate','ProRataDeductible','Interests','ProRata','bSelWithinDate','WithinDate','ProRataDeductible','WithinDate','ProRataDeductible','PreviousDebitCredit','Amount','LastTaxPymt','TaxAdvance','TaxAdvanceDescri','PurchaseExigAmount','PurchaseNonExigAmount','SaleExigAmount','SaleNonExigAmount','SaleSplitPaymentAmount','DebitCredit1','Amount1','Description1','Amount1','Description1','DebitCredit2','Amount2','Description2','Amount2','Description2','DebitCredit3','Amount3','Description3','Amount3','Description3','DebitCredit4','Amount4','Description4','Amount4','Description4','ExcludedCreditTax','IncludedCreditTax','ImportedCarsTaxPaid','SpecialCreditTax','TAXTRANSFER','PostDate','NrDoc','FreeDescri','DefinitivelyPrinted','bPrepareForSOS','DotMatrixPrinter','ContextualHeading','NoPrefix','VideoPage','SummbTransfer','SummPubPostDate','SummDocNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXSUMMARYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXSUMMARYComponent, resolver);
    }
} 