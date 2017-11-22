import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCOUNTINGREBUILDINGService } from './IDD_COSTACCOUNTINGREBUILDING.service';

@Component({
    selector: 'tb-IDD_COSTACCOUNTINGREBUILDING',
    templateUrl: './IDD_COSTACCOUNTINGREBUILDING.component.html',
    providers: [IDD_COSTACCOUNTINGREBUILDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COSTACCOUNTINGREBUILDINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_RICALCOSTACC_COSTCENTERS_CODETYPE_itemSource: any;
public IDC_RICALCOSTACC_JOBS_CODETYPE_itemSource: any;
public IDC_RICALCOSTACC_PRODUCTLINE_CODETYPE_itemSource: any;

    constructor(document: IDD_COSTACCOUNTINGREBUILDINGService,
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
        this.IDC_RICALCOSTACC_COSTCENTERS_CODETYPE_itemSource = {
  "name": "EntryTypeForecastEnumCombo",
  "namespace": "ERP.CostAccounting.Components.EntryTypeForecastEnumCombo"
}; 
this.IDC_RICALCOSTACC_JOBS_CODETYPE_itemSource = {
  "name": "EntryTypeForecastEnumCombo",
  "namespace": "ERP.CostAccounting.Components.EntryTypeForecastEnumCombo"
}; 
this.IDC_RICALCOSTACC_PRODUCTLINE_CODETYPE_itemSource = {
  "name": "EntryTypeForecastEnumCombo",
  "namespace": "ERP.CostAccounting.Components.EntryTypeForecastEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CostCenters','CostCentersFromMonth','FromYearCostCenters','CostCentersToMonth','ToYearCostCenters','AllCostCenters','CostCentersSel','CostCentersType','Jobs','JobsFromMonth','FromYearJobs','JobsToMonth','ToYearJobs','AllJob','JobSell','JobsType','ProductLine','LineFromMonth','FromYearLines','AllLine','FromYearLines','AllLine','LineToMonth','ToYearLines','LineSel','LineType','ToYearLines','LineSel','LineType','LineSel','LineType','CostCentersProcess','JobsProcess','LineProcess']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCOUNTINGREBUILDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCOUNTINGREBUILDINGComponent, resolver);
    }
} 