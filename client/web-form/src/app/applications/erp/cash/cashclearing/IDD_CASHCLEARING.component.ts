import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CASHCLEARINGService } from './IDD_CASHCLEARING.service';

@Component({
    selector: 'tb-IDD_CASHCLEARING',
    templateUrl: './IDD_CASHCLEARING.component.html',
    providers: [IDD_CASHCLEARINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CASHCLEARINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CASHCLEARINGService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CustSupp','SelectionDate','FromDocDate','ToDocDate','AllNo','SelNo','FromNo','ToNo','AllSuppNo','SelSuppNo','FromSuppNo','ToSuppNo','bNotSelectedPymtTerm','bSelectedPymtTerm','PymtTerm','Amount','CashClearing','TotInBaseCurr','TotalAmount','TotInDocCurr','TotalInCurr'],'CashClearing':['l_TEnhCashClearing_P03','l_TEnhCashClearing_P23','l_TEnhCashClearing_P01','l_TEnhCashClearing_P02','l_TEnhCashClearing_P20','l_TEnhCashClearing_P21','l_TEnhCashClearing_P22','l_TEnhCashClearing_P19','InstallmentNo','InstallmentDate','PaymentTerm','l_TEnhCashClearing_P08','l_TEnhCashClearing_P08','l_TEnhCashClearing_P09','l_TEnhCashClearing_P09','Amount','Closed','Currency','FixingIsManual','FixingDate','Fixing','PayableAmountInBaseCurr','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CASHCLEARINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CASHCLEARINGComponent, resolver);
    }
} 