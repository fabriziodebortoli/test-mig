import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMMISSIONS_SETTLEMENTService } from './IDD_COMMISSIONS_SETTLEMENT.service';

@Component({
    selector: 'tb-IDD_COMMISSIONS_SETTLEMENT',
    templateUrl: './IDD_COMMISSIONS_SETTLEMENT.component.html',
    providers: [IDD_COMMISSIONS_SETTLEMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COMMISSIONS_SETTLEMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMMISSIONS_SETTLEMENTService,
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
        
        		this.bo.appendToModelStructure({'global':['StartingDate','EndingDate','bAllSalesPeople','bSalesPeopleSel','FromSalesperson','ToSalesperson','CommissionsSettlement'],'CommissionsSettlement':['Commission_Selected','Commission_AreaManager','Salesperson','Commission_SalespersonDesc','Commission_CustCode','Commission_CustDescr','Commission_DocNo','DocumentDate','DocAmount','TaxableAmountDocTot','InstallmentNo','Base','Comm','ExpectedAccrualDate','ActualAccrualDate','Cancel']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMMISSIONS_SETTLEMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMMISSIONS_SETTLEMENTComponent, resolver);
    }
} 