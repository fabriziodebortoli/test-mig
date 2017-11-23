import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FIXEDASSETS_REDUCEDService } from './IDD_FIXEDASSETS_REDUCED.service';

@Component({
    selector: 'tb-IDD_FIXEDASSETS_REDUCED',
    templateUrl: './IDD_FIXEDASSETS_REDUCED.component.html',
    providers: [IDD_FIXEDASSETS_REDUCEDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_FIXEDASSETS_REDUCEDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_FIXEDASSET_CTGMETHOD_itemSource: any;
public IDC_FIXEDASSET_METHOD_itemSource: any;

    constructor(document: IDD_FIXEDASSETS_REDUCEDService,
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
        this.IDC_FIXEDASSET_CTGMETHOD_itemSource = {
  "name": "FiscalDepreciationEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.FiscalDepreciationEnumCombo"
}; 
this.IDC_FIXEDASSET_METHOD_itemSource = {
  "name": "FiscalDepreciationEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.FiscalDepreciationEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'FixedAssets':['FixedAsset','Disabled','Description','Category','Class','Location','ParentCodeType','ParentFixedAsset','Qty','DepreciationStart','FiscalCustomized','FiscalPerc','LifePeriod','FiscalPerc','LifePeriod','AcceleratedCustomized','AcceleratedPerc','BalanceCustomized','BalancePerc','AuthorizationPeriod','DepreciationStartingDate','BalanceCustomized','DeprTemplate','BalanceDepreciationMethod','BalanceLifePeriod','DepreciationStartingDate','FiscalCustomized','DepreciationMethod','LifePeriod','AcceleratedPerc','DepreciationStartingDate','FiscalCustomized','FiscalPerc','BalanceCustomized','BalancePerc','PurchaseType','IdNumber','Notes','ItemAdditionalCode','PartDeprPercCustom','PartDeprPerc','PartDeprLimitCustom','PartDeprLimit','PartDeprLimitCustom','PartDeprLimit','ExtraDed','NoChargesCalculation','BalanceStep','Activity','Job','CostCenter','ProductLine'],'HKLParent':['Description'],'global':['FiscDeprCtg','CtgFiscalPerc','LifePeriodCtg','CtgFiscalPerc','LifePeriodCtg','AccelDeprCtg','DepreciationAccPerc','BalDeprCtg','BalancePercCtg','FiscDeprCtg','CtgMethod','LifePeriodCtg','BalDeprCtg','BalCtgMethod','BalLifePeriodCtg','FiscDeprCtg','CtgMethod','LifePeriodCtg','FiscDeprCtg','CtgFiscalPerc','BalDeprCtg','BalancePercCtg','RadioDeprType','ParzAmmPercCat','ParzAmmTettoCat','Coeff','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLItemsAdditionalCodes':['Description'],'Coeff':['FromPeriod','ToPeriod','RegrCoeff','Perc','Activity'],'HKLJob':['Description'],'HKLCstCenter':['Description'],'HKLProductLine':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FIXEDASSETS_REDUCEDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FIXEDASSETS_REDUCEDComponent, resolver);
    }
} 