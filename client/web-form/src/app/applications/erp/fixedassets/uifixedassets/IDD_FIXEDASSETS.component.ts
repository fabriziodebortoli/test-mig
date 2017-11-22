import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FIXEDASSETSService } from './IDD_FIXEDASSETS.service';

@Component({
    selector: 'tb-IDD_FIXEDASSETS',
    templateUrl: './IDD_FIXEDASSETS.component.html',
    providers: [IDD_FIXEDASSETSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FIXEDASSETSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_FIXEDASSET_CTGMETHOD_itemSource: any;
public IDC_FIXEDASSET_METHOD_itemSource: any;

    constructor(document: IDD_FIXEDASSETSService,
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
        this.IDC_FIXEDASSET_CTGMETHOD_itemSource = {
  "name": "FiscalDepreciationEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.FiscalDepreciationEnumCombo"
}; 
this.IDC_FIXEDASSET_METHOD_itemSource = {
  "name": "FiscalDepreciationEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.FiscalDepreciationEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'FixedAssets':['FixedAsset','Disabled','Description','Category','Class','Location','ParentCodeType','ParentFixedAsset','Qty','DepreciationStart','DepreciationStartingDate','LastDepreciationDate','LastBalDepreciationDate','LastBalDepreciationDate','DepreciationStartingDate','DeprByDate','DepreciationEndingDate','PartDeprPercCustom','PartDeprPerc','PartDeprLimitCustom','PartDeprLimit','PartDeprLimitCustom','PartDeprLimit','ExtraDed','NoChargesCalculation','PurchaseType','PurchaseDate','PurchaseDocDate','PurchaseDocNo','LogNo','Supplier','PurchaseCost','PurchaseCostDocCurr','DisposalType','DisposalDate','DisposalDocDate','DisposalDocNo','Customer','DisposalAmount','DisposalAmountDocCurr','WriteOffDate','BalWriteOffDate','IdNumber','Notes','InstallationDate','InspectionDate','PlacedInServiceDate','ItemAdditionalCode','Job','CostCenter','ProductLine','Picture','Picture','FiscalCustomized','DepreciationMethod','LifePeriod','AcceleratedPerc','FiscalCustomized','FiscalPerc','FiscalPerc','AuthorizeInsufficient','AcceleratedCustomized','AcceleratedPerc','FiscalCustomized','DepreciationMethod','LifePeriod','AcceleratedPerc','FiscalCustomized','FiscalPerc','LifePeriod','FiscalPerc','LifePeriod','AuthorizationPeriod','AssignorContribution','AssignorContributionCurr','BalanceCustomized','DeprTemplate','BalanceDepreciationMethod','BalanceLifePeriod','BalLifePeriodChangeDate','BalMethodChangeDate','BalanceCustomized','BalancePerc','Aligned','AlignmentDate','BalanceStep','Activity'],'HKLParent':['Description'],'global':['LastFYearDescri','RadioDeprType','ParzAmmPercCat','ParzAmmTettoCat','FiscDeprCtg','CtgMethod','LifePeriodCtg','FiscDeprCtg','CtgFiscalPerc','DeprType','DeprType','AccelDeprCtg','DepreciationAccPerc','FiscDeprCtg','CtgMethod','LifePeriodCtg','FiscDeprCtg','CtgFiscalPerc','LifePeriodCtg','CtgFiscalPerc','LifePeriodCtg','FiscalDepr','FinancialDepreciation','FinancialDeprDomestic','BalDeprCtg','BalCtgMethod','BalLifePeriodCtg','BalDeprCtg','BalancePercCtg','BalanceDepreciation','Coeff','Period','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLSupp':['CompNameComplete'],'HKLCust':['CompNameComplete'],'HKLItemsAdditionalCodes':['Description'],'HKLJob':['Description'],'HKLCstCenter':['Description'],'HKLProductLine':['Description'],'BalanceDepreciation':['LifePeriod','DepreciationMethod'],'Coeff':['FromPeriod','ToPeriod','RegrCoeff','Perc','Activity'],'Period':['FiscalYear','FiscalPeriod','DeprDisabled','BalanceDeprDisabled']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FIXEDASSETSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FIXEDASSETSComponent, resolver);
    }
} 