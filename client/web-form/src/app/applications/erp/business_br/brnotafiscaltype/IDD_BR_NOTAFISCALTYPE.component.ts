import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_NOTAFISCALTYPEService } from './IDD_BR_NOTAFISCALTYPE.service';

@Component({
    selector: 'tb-IDD_BR_NOTAFISCALTYPE',
    templateUrl: './IDD_BR_NOTAFISCALTYPE.component.html',
    providers: [IDD_BR_NOTAFISCALTYPEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_NOTAFISCALTYPEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BR_NOTA_FISCAL_TYPE_MOV_TYPE_itemSource: any;
public IDC_BR_NOTA_FISCAL_TYPE_CUST_SUPP_TYPE_itemSource: any;
public IDC_BR_NOTA_FISCAL_TYPE_OPERATION_TYPE_itemSource: any;
public IDC_BR_NOTA_FISCAL_TYPE_TRANS_DATA_DANFE_itemSource: any;

    constructor(document: IDD_BR_NOTAFISCALTYPEService,
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
        this.IDC_BR_NOTA_FISCAL_TYPE_MOV_TYPE_itemSource = {
  "name": "MovementTypeComboBox",
  "namespace": "ERP.Business_BR.Components.MovementTypeComboBox"
}; 
this.IDC_BR_NOTA_FISCAL_TYPE_CUST_SUPP_TYPE_itemSource = {
  "name": "AskCustSuppTypeComboBox",
  "namespace": "ERP.Business_BR.Components.AskCustSuppTypeComboBox"
}; 
this.IDC_BR_NOTA_FISCAL_TYPE_OPERATION_TYPE_itemSource = {
  "name": "OperationTypeComboBox",
  "namespace": "ERP.Business_BR.Components.OperationTypeComboBox"
}; 
this.IDC_BR_NOTA_FISCAL_TYPE_TRANS_DATA_DANFE_itemSource = {
  "name": "DANFETypePrintComboBox",
  "namespace": "ERP.Business_BR.Components.DANFETypePrintComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRNotaFiscalType':['NotaFiscalCode','ForGoods','Description','MovementType','MoveTypeDescri','CustSuppType','CustSuppDescri','NFeIssuingPurpose','OperationType','OperationTypeDescri','CustPresenceIndicator','ISSTaxRateCode','InventoryReason','InventoryReasonAdjust','CFOPGroup','ExcludeElectrTransm','DANFETypePrint','DANFETypePrintDescri','IncludedInTurnover','ExcludedFromTot','InCreditLimit','GenerateCommissions','NotaFiscalAccTpl','NotaFiscalAccRsn','AdvanceAccRsn','FreeSamplesAccRsn','GoodsAccount','ServicesAccount','FreeSamplesAmount','ShippingCharges','AdditionalCharges','InsuranceCharges','Advance','SimplesMsg','SimplesZeroMsg','EnableNFeRef','EnableOrigDest','Message1','Message2','EnableApproxTaxesMsg','ApproxTaxesMsg','AutoNumbering','Model','Series'],'HKLBRTaxRateCode':['Description'],'HKLInvReason':['Description'],'HKLInvReasonAdjust':['Description'],'HKLBRCFOPGroup':['Description'],'HKLBRSeries':['Description'],'global':['DBTBRNotaFiscalTypeDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTBRNotaFiscalTypeDetail':['Storage','Priority','Model','Series'],'HKLStorage':['Description'],'HKLBRSeriesBE':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_NOTAFISCALTYPEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_NOTAFISCALTYPEComponent, resolver);
    }
} 