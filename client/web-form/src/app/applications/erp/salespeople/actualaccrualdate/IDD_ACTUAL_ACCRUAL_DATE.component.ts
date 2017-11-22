import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACTUAL_ACCRUAL_DATEService } from './IDD_ACTUAL_ACCRUAL_DATE.service';

@Component({
    selector: 'tb-IDD_ACTUAL_ACCRUAL_DATE',
    templateUrl: './IDD_ACTUAL_ACCRUAL_DATE.component.html',
    providers: [IDD_ACTUAL_ACCRUAL_DATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACTUAL_ACCRUAL_DATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACTUAL_ACCRUAL_DATEService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','bAllSalesPeople','bSalesPeopleSel','FromSalesperson','ToSalesperson','ActualAccr_DocNo','ActualAccr_Selected','AdvanceBalance','ActualAccr_AreaManager','Salesperson','ActualAccr_SalespersonDesc','ActualAccr_CustCode','ActualAccr_CustomerDesc','InstallmentAmount','CollAmount','RemainingAmount','CommissionTot','Comm','RemainingComm','CommissionAllowance','Allowance','ExpectedAccrualDate','AccrEffDate','Base','Suspended','LegendPymtSched','LegendPymtSchedInstNotRec','LegendPymtSchedInstRec','LegendSalespEntryNotLinked','LegendAllowance']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACTUAL_ACCRUAL_DATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACTUAL_ACCRUAL_DATEComponent, resolver);
    }
} 