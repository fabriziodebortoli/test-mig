import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PRODUCTLINESService } from './IDD_PRODUCTLINES.service';

@Component({
    selector: 'tb-IDD_PRODUCTLINES',
    templateUrl: './IDD_PRODUCTLINES.component.html',
    providers: [IDD_PRODUCTLINESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PRODUCTLINESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PRODUCTLINESService,
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
        
        		this.bo.appendToModelStructure({'ProductLines':['ProductLine','Disabled','Description','GroupCode','DepreciationPerc','Notes'],'HKLGroupCode':['Description'],'global':['DebitBudTot','CreditBudTot','BudgetDebitBalance','BudgetCreditBalance','DebitActTot','CreditActTot','ActualDebitBalance','ActualCreditBalance','DebitForTot','CreditForTot','ForecastDebitBalance','ForecastCreditBalance','DebitSimTot','CreditSimTot','SimDebitBalance','SimCreditBalance','ProductLinesBalances','QtyDebitBudTot','QtyCreditBudTot','QtyBudgetDebitBalance','QtyBudgetCreditBalance','QtyDebitActTot','QtyCreditActTot','QtyActualDebitBalance','QtyActualCreditBalance','QtyDebitForTot','QtyCreditFortTot','QtyForecastDebitBalance','QtyForecastCreditBalance','QtyDebitSimTot','QtyCreditSimTot','QtySimDebitBalance','QtySimCreditBalance','ProductLinesBalances','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ProductLinesBalances':['l_TProductLinesBalancesEnh_P01','l_TProductLinesBalancesEnh_P02','l_TProductLinesBalancesEnh_P03','l_TProductLinesBalancesEnh_P04','l_TProductLinesBalancesEnh_P05','l_TProductLinesBalancesEnh_P06','l_TProductLinesBalancesEnh_P07','l_TProductLinesBalancesEnh_P01','l_TProductLinesBalancesEnh_P08','l_TProductLinesBalancesEnh_P09','l_TProductLinesBalancesEnh_P10','l_TProductLinesBalancesEnh_P11','l_TProductLinesBalancesEnh_P12','l_TProductLinesBalancesEnh_P13']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PRODUCTLINESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PRODUCTLINESComponent, resolver);
    }
} 