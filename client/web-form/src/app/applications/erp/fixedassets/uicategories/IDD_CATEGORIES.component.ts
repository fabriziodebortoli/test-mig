import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CATEGORIESService } from './IDD_CATEGORIES.service';

@Component({
    selector: 'tb-IDD_CATEGORIES',
    templateUrl: './IDD_CATEGORIES.component.html',
    providers: [IDD_CATEGORIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CATEGORIESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CATEGORY_METHOD_itemSource: any;

    constructor(document: IDD_CATEGORIESService,
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
        this.IDC_CATEGORY_METHOD_itemSource = {
  "name": "CtgFiscalDepreciationEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.CtgFiscalDepreciationEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Categories':['Category','Description','PartDeprPerc','PartDeprLimit','ChargesNoOfyears','ChargesPerc','LifePeriod','DepreciationMethod','MinLifePeriod','MaxLifePeriod','OfficialPerc','FirstFiscalYearPerc','MinimumPerc','AcceleratedPerc','AcceleratedDisabled','AcceleratedNoOfYears','OfficialPerc','DepreciateByLifePeriod','LifePeriod','BalancePerc','CategoryAccount','DeprAccount','AccumDeprAccount','AcceleratedDeprAccount','AcceleratedAccumDeprAccount'],'global':['CategoryType','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLCoAAccountCategoryAccount':['Description'],'HKLCoAAccountAccountDepr':['Description'],'HKLCoAAccountAccountFDepr':['Description'],'HKLCoAAccountAccountDeprAnt':['Description'],'HKLCoAAccountAccountFDeprAnt':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CATEGORIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CATEGORIESComponent, resolver);
    }
} 