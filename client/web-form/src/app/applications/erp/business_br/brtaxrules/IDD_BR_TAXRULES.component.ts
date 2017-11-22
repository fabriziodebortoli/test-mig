import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAXRULESService } from './IDD_BR_TAXRULES.service';

@Component({
    selector: 'tb-IDD_BR_TAXRULES',
    templateUrl: './IDD_BR_TAXRULES.component.html',
    providers: [IDD_BR_TAXRULESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_TAXRULESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_TAXRULES_OPERATION_TYPE_itemSource: any;
public IDC_BR_TAXRULES_MOV_TYPE_itemSource: any;
public IDC_BR_TAXRULES_GOODS_ORIGIN_itemSource: any;
public IDC_BR_TAXRULES_ICMS_MOD_itemSource: any;
public IDC_BR_TAXRULES_ICMS_NO_TAX_REASON_itemSource: any;
public IDC_BR_TAXRULES_ICMSST_MOD_itemSource: any;

    constructor(document: IDD_BR_TAXRULESService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_BR_TAXRULES_OPERATION_TYPE_itemSource = {
  "name": "OperationTypeComboBox",
  "namespace": "ERP.Business_BR.Components.OperationTypeComboBox"
}; 
this.IDC_BR_TAXRULES_MOV_TYPE_itemSource = {
  "name": "MovementTypeComboBox",
  "namespace": "ERP.Business_BR.Components.MovementTypeComboBox"
}; 
this.IDC_BR_TAXRULES_GOODS_ORIGIN_itemSource = {
  "name": "GoodsOriginComboBox",
  "namespace": "ERP.Business_BR.Components.GoodsOriginComboBox"
}; 
this.IDC_BR_TAXRULES_ICMS_MOD_itemSource = {
  "name": "ICMSModComboBox",
  "namespace": "ERP.Business_BR.Components.ICMSModComboBox"
}; 
this.IDC_BR_TAXRULES_ICMS_NO_TAX_REASON_itemSource = {
  "name": "BRNoTaxReasonComboBox",
  "namespace": "ERP.Business_BR.Components.BRNoTaxReasonComboBox"
}; 
this.IDC_BR_TAXRULES_ICMSST_MOD_itemSource = {
  "name": "ICMSSTModComboBox",
  "namespace": "ERP.Business_BR.Components.ICMSSTModComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRTaxRules':['TaxRuleCode','Description','Priority','ValidityStartingDate','ValidityEndingDate','AllItems','Item','NCM','ItemFiscalCtg','OperationType','OperationTypeDescri','NotaFiscalCode','DistributeCharges','CFOP','MovementType','MoveTypeDescri','EXTIPI','FederalState','GoodsOrigin','GoodsOriginDescri','CustSuppFiscalCtg','ICMSTaxCode','ICMSTaxRateCode','ICMSSTReducTaxRateCode','ICMSTaxFormulaCode','ICMS_Mod','ICMS_ModDescri','ICMS_NoTaxReason','ICMS_NoTaxReasonDescri','ICMSDefTaxRateCode','ICMSDefTaxFormulaCode','ICMSExTaxRateCode','ICMSExTaxFormulaCode','ExcludeICMSST','ICMSSTTaxRateCode','ICMSSTReducTaxRateCode','ICMSSTToBeCompTaxRateCode','ICMSSTTaxFormulaCode','ICMSST_Mod','ICMSST_ModDescri','MVATaxRateCode','ICMSDestTaxRateCode','ICMSDestTaxFormulaCode','ICMSDestTempRateCode','ICMSOrigTaxFormulaCode','ICMSFCPTaxRateCode','ICMSFCPTaxFormulaCode','ICMSInterTaxRateCode','IITaxRateCode','IITaxFormulaCode','PISCalcCode','COFINSCalcCode','IPICalcCode','SUFRAMACalcCode','SIMPLESCalcCode','IPILegalCode','TaxMessageCode1','TaxMessageCode2'],'HKLItems':['Description'],'HKLBRNCM':['Description'],'HKLItemFiscalCtg':['Description'],'HKLBRNotaFiscalType':['Description'],'HKLBRCFOP':['Description'],'HKLCustSuppFiscalCtg':['Description'],'HKLBRTaxCodeICMS':['Description'],'HKLBRTaxRateICMS':['Description'],'HKLBRTaxRateICMSREDUC':['Description'],'HKLBRTaxFormulaICMS':['Description'],'HKLBRTaxRateICMSDef':['Description'],'HKLBRTaxFormulaICMSDef':['Description'],'HKLBRTaxRateICMSEx':['Description'],'HKLBRTaxFormulaICMSEx':['Description'],'HKLBRTaxRateICMSST':['Description'],'HKLBRTaxRateICMSSTREDUC':['Description'],'HKLBRTaxRateICMSSTTOBECOMP':['Description'],'HKLBRTaxFormulaICMSST':['Description'],'HKLBRTaxRateMVA':['Description'],'HKLBRTaxRateICMSDest':['Description'],'HKLBRTaxFormulaICMSDest':['Description'],'HKLBRTaxRateICMSDestTemp':['Description'],'HKLBRTaxFormulaICMSOrig':['Description'],'HKLBRTaxRateICMS_FCP':['Description'],'HKLBRTaxFormulaICMS_FCP':['Description'],'HKLBRTaxRateICMSInter':['Description'],'HKLBRTaxRateII':['Description'],'HKLBRTaxFormulaII':['Description'],'HKLBRTaxCalcPIS':['Description'],'HKLBRTaxCalcCOFINS':['Description'],'HKLBRTaxCalcIPI':['Description'],'HKLBRTaxCalcSUFRAMA':['Description'],'HKLBRTaxCalcSIMPLES':['Description'],'HKLBRIPILegalCode':['Description'],'HKLBRTaxMessagesCode1':['Description1','Description2','Description3','LongDescription'],'HKLBRTaxMessagesCode2':['Description1','Description2','Description3','LongDescription'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAXRULESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAXRULESComponent, resolver);
    }
} 