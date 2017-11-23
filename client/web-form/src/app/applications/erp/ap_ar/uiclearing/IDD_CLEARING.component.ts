import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CLEARINGService } from './IDD_CLEARING.service';

@Component({
    selector: 'tb-IDD_CLEARING',
    templateUrl: './IDD_CLEARING.component.html',
    providers: [IDD_CLEARINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CLEARINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CLEARINGService,
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
        
        		this.bo.appendToModelStructure({'global':['CustSupp','SelectionDate','Currency','Filter','FromDocDate','ToDocDate','AllNo','SelNo','FromNo','ToNo','AllSuppNo','SelSuppNo','FromSuppNo','ToSuppNo','ContractCode','ProjectCode','Amount','EquivalentAmount','bNotSelectedPymtTerm','bSelectedPymtTerm','PymtTerm','LastFixingDate','LastFixing','JEClosing','PostDateDoc','JEDocDate','JEDocNo','Clearing','TotInBaseCurr','TotalAmount','TotInDocCurr','TotalInCurr','BlockedImage','LitigationImage'],'Clearing':['l_TEnhClearing_P03','l_TEnhClearing_P28','l_TEnhClearing_P01','l_TEnhClearing_P02','l_TEnhClearing_P15','Advance','l_TEnhClearing_P22','l_TEnhClearing_P23','l_TEnhClearing_P24','l_TEnhClearing_P21','InstallmentNo','InstallmentDate','PaymentTerm','l_TEnhClearing_P08','l_TEnhClearing_P09','Amount','l_TEnhClearing_P31','Closed','Salesperson','Currency','FixingIsManual','FixingDate','Fixing','PayableAmountInBaseCurr','Notes','l_TEnhClearing_P13','l_TEnhClearing_P14','l_TEnhClearing_P29','l_TEnhClearing_P30']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CLEARINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CLEARINGComponent, resolver);
    }
} 