import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TRANSCOD_EUService } from './IDD_TRANSCOD_EU.service';

@Component({
    selector: 'tb-IDD_TRANSCOD_EU',
    templateUrl: './IDD_TRANSCOD_EU.component.html',
    providers: [IDD_TRANSCOD_EUService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TRANSCOD_EUComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TRANSCOD_EUService,
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
        
        		this.bo.appendToModelStructure({'ReclassificationSchema':['SchemaCode','Disabled','Description','Predefined','IsBeO','IsXbrl','Currency','PositiveRoundingCode','NegativeRoundingCode','XbrlBalanceType','XbrlMap','XbrlBalanceType','XbrlMap','XbrlRoundingFinStat','XbrlRoundingProfit','XbrlRoundingLoss'],'HKLCurrencies':['Description'],'global':['Detail','RoundXbrlRoundingFinStat','RoundXbrlRoundingProfit','RoundXbrlRoundingLoss','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['Code','Line','BalanceSection','ReferenceSchemaCode','ReferenceCode','Description','LineNoCol','LineType','LineType','BalanceSign','BalanceType','l_TEnhBalanceReclassDetail_P2','l_TEnhBalanceReclassDetail_P3','l_TEnhBalanceReclassDetail_P1','OffsetAccount','OffsetCashType','OffsetCustSupp','IgnoreDifferentSign','AllKind','Nature']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TRANSCOD_EUFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TRANSCOD_EUComponent, resolver);
    }
} 