import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TRAVELAGENCY_TAXService } from './IDD_TRAVELAGENCY_TAX.service';

@Component({
    selector: 'tb-IDD_TRAVELAGENCY_TAX',
    templateUrl: './IDD_TRAVELAGENCY_TAX.component.html',
    providers: [IDD_TRAVELAGENCY_TAXService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TRAVELAGENCY_TAXComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TRAVELAGENCY_TAXService,
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
		boService.appendToModelStructure({'TravelAgencyTaxData':['BalanceYear','BalanceMonth','PeriodRevenue','PeriodCost','ActualPeriodCreditCost'],'global':['PreviousPeriodCost','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TRAVELAGENCY_TAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TRAVELAGENCY_TAXComponent, resolver);
    }
} 