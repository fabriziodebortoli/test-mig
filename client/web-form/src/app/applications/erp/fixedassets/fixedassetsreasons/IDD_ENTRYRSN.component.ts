import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ENTRYRSNService } from './IDD_ENTRYRSN.service';

@Component({
    selector: 'tb-IDD_ENTRYRSN',
    templateUrl: './IDD_ENTRYRSN.component.html',
    providers: [IDD_ENTRYRSNService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ENTRYRSNComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ENTRYRSN_QTY_itemSource: any;
public IDC_ENTRYRSN_DATAPURCHASE_itemSource: any;
public IDC_ENTRYRSN_PURCHCOST_itemSource: any;
public IDC_ENTRYRSN_DISPOSALDATA_itemSource: any;
public IDC_ENTRYRSN_DISPOSALVALUE_itemSource: any;
public IDC_ENTRYRSN_FISC_DEPRINTOT_itemSource: any;
public IDC_ENTRYRSN_FISC_INACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FISC_INITIALNOTDEPRECIABLE_itemSource: any;
public IDC_ENTRYRSN_FISC_INACCACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FISC_LOSTACCUMDEPRIN_itemSource: any;
public IDC_ENTRYRSN_FISC_INCREMENTALCHARGES_itemSource: any;
public IDC_ENTRYRSN_FISC_REVALUATION_itemSource: any;
public IDC_ENTRYRSN_FISC_ADDITDEPR_itemSource: any;
public IDC_ENTRYRSN_FISC_CHARGES_itemSource: any;
public IDC_ENTRYRSN_FISC_MAINTENANCECHARGES_itemSource: any;
public IDC_ENTRYRSN_FISC_REPAIRSCHARGES_itemSource: any;
public IDC_ENTRYRSN_FISC_DEPRCHARGES_itemSource: any;
public IDC_ENTRYRSN_FISC_TRANSFCHARGES_itemSource: any;
public IDC_ENTRYRSN_FISC_DEPRTOT_itemSource: any;
public IDC_ENTRYRSN_FISC_DEPRECIATION_itemSource: any;
public IDC_ENTRYRSN_FISC_ACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FISC_NOTDEPRECIABLE_itemSource: any;
public IDC_ENTRYRSN_FISC_LOSTDEPR_itemSource: any;
public IDC_ENTRYRSN_FISC_LOSTACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FISC_ACCELDEPR_itemSource: any;
public IDC_ENTRYRSN_FISC_ACCACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_CAPITALGAIN_itemSource: any;
public IDC_ENTRYRSN_CAPITALLOSS_itemSource: any;
public IDC_ENTRYRSN_WINDFLOSS_itemSource: any;
public IDC_ENTRYRSN_FIN_DEPRINTOT_itemSource: any;
public IDC_ENTRYRSN_FIN_INACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FIN_INRENEWALRESERVE_itemSource: any;
public IDC_ENTRYRSN_FIN_INRENEWALACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FIN_CHARGES_itemSource: any;
public IDC_ENTRYRSN_FIN_DEPRTOT_itemSource: any;
public IDC_ENTRYRSN_FIN_DEPRECIATION_itemSource: any;
public IDC_ENTRYRSN_FIN_ACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FIN_RENEWALDEPR_itemSource: any;
public IDC_ENTRYRSN_FIN_RENEWALACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_FIN_RENEWALRESERVE_itemSource: any;
public IDC_ENTRYRSN_FIN_ASSIGNORCONTRIBUTION_itemSource: any;
public IDC_ENTRYRSN_BALANCE_DEPRINTOT_itemSource: any;
public IDC_ENTRYRSN_BALANCE_INACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_BALANCE_INITIALLIQUIDATION_itemSource: any;
public IDC_ENTRYRSN_BALANCE_INCREMENTALCHARGES_itemSource: any;
public IDC_ENTRYRSN_BALANCE_REVALUATION_itemSource: any;
public IDC_ENTRYRSN_BALANCE_ADDITDEPR_itemSource: any;
public IDC_ENTRYRSN_BALANCE_CHARGES_itemSource: any;
public IDC_ENTRYRSN_BALANCE_DEPRTOT_itemSource: any;
public IDC_ENTRYRSN_BALANCE_DEPRECIATION_itemSource: any;
public IDC_ENTRYRSN_BALANCE_ACCUMDEPR_itemSource: any;
public IDC_ENTRYRSN_BALANCE_LIQUIDATION_itemSource: any;
public IDC_ENTRYRSN_BALANCE_CAPITALGAIN_itemSource: any;
public IDC_ENTRYRSN_BALANCE_CAPITALLOSS_itemSource: any;
public IDC_ENTRYRSN_BALANCE_WINDFLOSS_itemSource: any;

    constructor(document: IDD_ENTRYRSNService,
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
        this.IDC_ENTRYRSN_QTY_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_DATAPURCHASE_itemSource = {
  "name": "UpdateEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.UpdateEnumCombo"
}; 
this.IDC_ENTRYRSN_PURCHCOST_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_DISPOSALDATA_itemSource = {
  "name": "UpdateEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.UpdateEnumCombo"
}; 
this.IDC_ENTRYRSN_DISPOSALVALUE_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_DEPRINTOT_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_INACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_INITIALNOTDEPRECIABLE_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_INACCACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_LOSTACCUMDEPRIN_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_INCREMENTALCHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_REVALUATION_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_ADDITDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_CHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_MAINTENANCECHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_REPAIRSCHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_DEPRCHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_TRANSFCHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_DEPRTOT_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_DEPRECIATION_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_ACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_NOTDEPRECIABLE_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_LOSTDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_LOSTACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_ACCELDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FISC_ACCACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_CAPITALGAIN_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_CAPITALLOSS_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_WINDFLOSS_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_DEPRINTOT_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_INACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_INRENEWALRESERVE_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_INRENEWALACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_CHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_DEPRTOT_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_DEPRECIATION_itemSource = {
  "name": "UpdateEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.UpdateEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_ACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_RENEWALDEPR_itemSource = {
  "name": "UpdateEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.UpdateEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_RENEWALACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_RENEWALRESERVE_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_FIN_ASSIGNORCONTRIBUTION_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_DEPRINTOT_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_INACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_INITIALLIQUIDATION_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_INCREMENTALCHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_REVALUATION_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_ADDITDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_CHARGES_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_DEPRTOT_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_DEPRECIATION_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_ACCUMDEPR_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_LIQUIDATION_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_CAPITALGAIN_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_CAPITALLOSS_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 
this.IDC_ENTRYRSN_BALANCE_WINDFLOSS_itemSource = {
  "name": "SumSubtractEnumCombo",
  "namespace": "ERP.FixedAssets.Documents.SumSubtractEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'FixedAssetsReasons':['Reason','Disabled','Description','PostDocumentData','UserEditableQty','UserEditablePerc','PurchaseDisposalEnabled','DisposalType','FiscalEnabled','FinancialEnabled','BalanceEnabled'],'global':['PurchaseDisposal_Qty','PurchaseDisposal_DataPurchase','PurchaseDisposal_PurchCost','PurchaseDisposal_DisposalData','PurchaseDisposal_DisposalValue','FiscDepr_DeprInTot','FiscDepr_InAccumDepr','FiscDepr_InitialNotDepreciable','FiscDepr_InAccAccumDepr','FiscDepr_LostDeprInTot','FiscDepr_IncrementalCharges','FiscDepr_Revaluation','FiscDepr_AdditDepr','FiscDepr_Charges','FiscDepr_MaintenanceCharges','FiscDepr_RepairsCharges','FiscDepr_DeprCharges','FiscDepr_TransfCharges','FiscDepr_DeprTot','FiscDepr_Depreciation','FiscDepr_AccumDepr','FiscDepr_NotDepreciable','FiscDepr_LostDepr','FiscDepr_LostDeprTot','FiscDepr_AccelDepr','FiscDepr_AccAccumDepr','FiscDepr_CapitalGain','FiscDepr_CapitalLoss','FiscDepr_WindfLoss','FinDepr_DeprInTot','FinDepr_InAccumDepr','FinDepr_InRenewalReserve','FinDepr_InRenewalAccumDepr','FinDepr_Charges','FinDepr_DeprTot','FinDepr_Depreciation','FinDepr_AccumDepr','FinDepr_RenewalDepr','FinDepr_RenewalAccumDepr','FinDepr_RenewalReserve','FinDepr_AssignorContribution','BalDepr_DeprInTot','BalDepr_InAccumDepr','BalDepr_InitialLiquidation','BalDepr_IncrementalCharges','BalDepr_Revaluation','BalDepr_AdditDepr','BalDepr_Charges','BalDepr_DeprTot','BalDepr_Depreciation','BalDepr_AccumDepr','BalDepr_Liquidation','BalDepr_CapitalGain','BalDepr_CapitalLoss','BalDepr_WindfLoss','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ENTRYRSNFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ENTRYRSNComponent, resolver);
    }
} 