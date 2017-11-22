import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTCENTERSService } from './IDD_COSTCENTERS.service';

@Component({
    selector: 'tb-IDD_COSTCENTERS',
    templateUrl: './IDD_COSTCENTERS.component.html',
    providers: [IDD_COSTCENTERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COSTCENTERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COSTCENTERSService,
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
		boService.appendToModelStructure({'CostCenters':['CostCenter','Disabled','Description','GroupCode','CodeType','Nature','SqMtSurfaceArea','DirectEmployeesNo','IndirectEmployeesNo','DepreciationPerc','DummyCostCenter','CostCenterManager','Notes','BarcodeSegment','Account'],'HKLGroupCode':['Description'],'HKLAccount':['Description'],'global':['DebitBudTot','CreditBudTot','BudgetDebitBalance','BudgetCreditBalance','DebitActTot','CreditActTot','ActualDebitBalance','ActualCreditBalance','DebitForTot','CreditForTot','ForecastDebitBalance','ForecastCreditBalance','DebitSimTot','CreditSimTot','SimDebitBalance','SimCreditBalance','CostCentersBalances','QtyDebitBudTot','QtyCreditBudTot','QtyBudgetDebitBalance','QtyBudgetCreditBalance','QtyDebitActTot','QtyCreditActTot','QtyActualDebitBalance','QtyActualCreditBalance','QtyDebitForTot','QtyCreditFortTot','QtyForecastDebitBalance','QtyForecastCreditBalance','QtyDebitSimTot','QtyCreditSimTot','QtySimDebitBalance','QtySimCreditBalance','CostCentersBalances','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'CostCentersBalances':['l_TCostCentersBalancesEnh_P01','l_TCostCentersBalancesEnh_P02','l_TCostCentersBalancesEnh_P03','l_TCostCentersBalancesEnh_P04','l_TCostCentersBalancesEnh_P05','l_TCostCentersBalancesEnh_P06','l_TCostCentersBalancesEnh_P07','l_TCostCentersBalancesEnh_P01','l_TCostCentersBalancesEnh_P08','l_TCostCentersBalancesEnh_P09','l_TCostCentersBalancesEnh_P10','l_TCostCentersBalancesEnh_P11','l_TCostCentersBalancesEnh_P12','l_TCostCentersBalancesEnh_P13']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTCENTERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTCENTERSComponent, resolver);
    }
} 