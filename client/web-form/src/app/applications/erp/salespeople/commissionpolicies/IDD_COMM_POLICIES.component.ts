import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMM_POLICIESService } from './IDD_COMM_POLICIES.service';

@Component({
    selector: 'tb-IDD_COMM_POLICIES',
    templateUrl: './IDD_COMM_POLICIES.component.html',
    providers: [IDD_COMM_POLICIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COMM_POLICIESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DESALEPERSONPOLICIES_DETAIL_BE_POLICYCOMMTYPE_itemSource: any;
public IDC_DESALEPERSONPOLICIES_DETAIL_BE_CROSSINGCODETYPE_itemSource: any;
public IDC_DESALEPERSONPOLICIES_DETAIL_BE_CROSSINGCODETYPE2_itemSource: any;

    constructor(document: IDD_COMM_POLICIESService,
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
        this.IDC_DESALEPERSONPOLICIES_DETAIL_BE_POLICYCOMMTYPE_itemSource = {
  "name": "TypeCommCombo",
  "namespace": "ERP.SalesPeople.Documents.TypeCommCombo"
}; 
this.IDC_DESALEPERSONPOLICIES_DETAIL_BE_CROSSINGCODETYPE_itemSource = {
  "name": "CrossingCodeTypeCombo",
  "namespace": "ERP.SalesPeople.Documents.CrossingCodeTypeCombo"
}; 
this.IDC_DESALEPERSONPOLICIES_DETAIL_BE_CROSSINGCODETYPE2_itemSource = {
  "name": "CrossingCodeTypeCombo",
  "namespace": "ERP.SalesPeople.Documents.CrossingCodeTypeCombo"
}; 

        		this.bo.appendToModelStructure({'CommissionPolicy':['Policy','CommissionOnLines','Description','CommissionFormula','DiscountsDetail','AccrualType','AccrualPercAtInvoiceDate','CommissionType','FinalDiscountIncluded'],'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['AllCodes','_PolicyCommType','_CrossingCodeType','CrossingCode','_CrossingCodeType2','CrossingCode2','CrossingValue','SalespersonCommPerc','AreaManagerCommPerc','StartingFrom','CommissionType','FinalDiscountIncluded','Disabled']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMM_POLICIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMM_POLICIESComponent, resolver);
    }
} 