import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MARGIN_TAXService } from './IDD_MARGIN_TAX.service';

@Component({
    selector: 'tb-IDD_MARGIN_TAX',
    templateUrl: './IDD_MARGIN_TAX.component.html',
    providers: [IDD_MARGIN_TAXService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MARGIN_TAXComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MARGIN_TAXService,
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
        
        		this.bo.appendToModelStructure({'MarginTaxData':['BalanceYear','BalanceMonth','PeriodRevenue','PeriodCost','ActualPeriodCreditCost'],'global':['PreviousPeriodCost','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MARGIN_TAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MARGIN_TAXComponent, resolver);
    }
} 