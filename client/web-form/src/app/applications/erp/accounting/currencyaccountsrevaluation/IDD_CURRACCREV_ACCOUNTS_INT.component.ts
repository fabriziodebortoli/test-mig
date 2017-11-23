﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CURRACCREV_ACCOUNTS_INTService } from './IDD_CURRACCREV_ACCOUNTS_INT.service';

@Component({
    selector: 'tb-IDD_CURRACCREV_ACCOUNTS_INT',
    templateUrl: './IDD_CURRACCREV_ACCOUNTS_INT.component.html',
    providers: [IDD_CURRACCREV_ACCOUNTS_INTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CURRACCREV_ACCOUNTS_INTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CURRACCREV_ACCOUNTS_INTService,
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
		boService.appendToModelStructure({'global':['Currency','FixingDate','Fixing','FixingDescri','bAll','bSelection','From','To','PostDate','AccrualDate','Nature','bOneJEForCustSupp','Profit','Loss','Detail'],'Detail':['l_TEnhCurrencyAccountsRev_P01','l_TEnhCurrencyAccountsRev_P10','CustSuppType','Account','l_TEnhCurrencyAccountsRev_P04','l_TEnhCurrencyAccountsRev_P05','l_TEnhCurrencyAccountsRev_P06','l_TEnhCurrencyAccountsRev_P09','DocCurrAmount','FixingDate','Amount','Fixing','l_TEnhCurrencyAccountsRev_P02','l_TEnhCurrencyAccountsRev_P03']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CURRACCREV_ACCOUNTS_INTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CURRACCREV_ACCOUNTS_INTComponent, resolver);
    }
} 