import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_JOBSService } from './IDD_JOBS.service';

@Component({
    selector: 'tb-IDD_JOBS',
    templateUrl: './IDD_JOBS.component.html',
    providers: [IDD_JOBSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_JOBSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_JOBSService,
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
		boService.appendToModelStructure({'Jobs':['Job','Disabled','Description','GroupCode','JobType','ParentJob','MachineHours','DepreciationPerc','CreationDate','ExpectedStartingDate','StartingDate','ExpectedDeliveryDate','DeliveryDate','Inhouse','Notes','EIJobCode','Customer','ContractCode','ProjectCode','Contract','ContactPerson','Price','Collected','ExpectedCost'],'HKLGroupCode':['Description'],'HKLParent':['Description'],'HKLCustomer':['CompNameComplete'],'global':['NetBookValue','BudgetMargin','DebitBudTot','CreditBudTot','BudgetDebitBalance','BudgetCreditBalance','DebitActTot','CreditActTot','ActualDebitBalance','ActualCreditBalance','DebitForTot','ForecastCreditBalance','ForecastDebitBalance','ForecastCreditBalance','DebitSimTot','CreditSimTot','SimDebitBalance','SimCreditBalance','Balances','QtyDebitBudTot','QtyCreditBudTot','QtyBudgetDebitBalance','QtyBudgetCreditBalance','QtyDebitActTot','QtyCreditActTot','QtyActualDebitBalance','QtyActualCreditBalance','QtyDebitForTot','QtyCreditFortTot','QtyForecastDebitBalance','QtyForecastCreditBalance','QtyDebitSimTot','QtyCreditSimTot','QtySimDebitBalance','QtySimCreditBalance','Balances','__Languages','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Balances':['TJobsBalancesEnh_P01','TJobsBalancesEnh_P02','TJobsBalancesEnh_P03','TJobsBalancesEnh_P04','TJobsBalancesEnh_P05','TJobsBalancesEnh_P06','TJobsBalancesEnh_P07','TJobsBalancesEnh_P01','TJobsBalancesEnh_P08','TJobsBalancesEnh_P09','TJobsBalancesEnh_P10','TJobsBalancesEnh_P11','TJobsBalancesEnh_P12','TJobsBalancesEnh_P13'],'@Languages':['__Language','__Description','__Notes','__TextDescri','__TextDescri2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_JOBSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_JOBSComponent, resolver);
    }
} 