import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALESPEOPLE_COMPLETEService } from './IDD_SALESPEOPLE_COMPLETE.service';

@Component({
    selector: 'tb-IDD_SALESPEOPLE_COMPLETE',
    templateUrl: './IDD_SALESPEOPLE_COMPLETE.component.html',
    providers: [IDD_SALESPEOPLE_COMPLETEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALESPEOPLE_COMPLETEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SALESPEOPLEADDONFLY_HEAD_AREA_PRIMARY_itemSource: any;

    constructor(document: IDD_SALESPEOPLE_COMPLETEService,
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
        this.IDC_SALESPEOPLEADDONFLY_HEAD_AREA_PRIMARY_itemSource = {
  "name": "AreaManagerCombo",
  "namespace": "ERP.SalesPeople.Documents.AreaCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'SalesPeople':['Salesperson','Supplier','Name','IsAnEmployee','Disabled','NoCommissionEdit','Policy','MonthlyFixedAmount','BaseCommission','IsAnAreaManager','AreaManager','BaseAreaMngCommission','Enasarco','HiringDate','FiringDate','IsACompany','IsACorporation','OneFirmOnly','AgencyChangeDate'],'HKLAreaManager':['Name'],'global':['Balances','ENASARCONo','bSalespersonMulti','FIRR','Allowance','SalesPeoplePartners','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'FIRR':['BalanceYear','CodeType','Base','AccruedAmount','PaymentDate','IsManual'],'Allowance':['BalanceYear','Base','Accrued','PaymentDate','IsManual']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALESPEOPLE_COMPLETEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALESPEOPLE_COMPLETEComponent, resolver);
    }
} 