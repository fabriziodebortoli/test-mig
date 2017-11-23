import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TD_BR_IMPORTNFEService } from './IDD_TD_BR_IMPORTNFE.service';

@Component({
    selector: 'tb-IDD_TD_BR_IMPORTNFE',
    templateUrl: './IDD_TD_BR_IMPORTNFE.component.html',
    providers: [IDD_TD_BR_IMPORTNFEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TD_BR_IMPORTNFEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BRIMPORTNFE_DETAIL_UOMOFQTYSEND_CONS_itemSource: any;

    constructor(document: IDD_TD_BR_IMPORTNFEService,
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
        this.IDC_BRIMPORTNFE_DETAIL_UOMOFQTYSEND_CONS_itemSource = {
  "name": "UnitsOfMeasureInvoiceComboBox",
  "namespace": "ERP.InvoiceMng.Components.UnitsOfMeasureInvoiceComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllCustSupp','bCustSuppSel','StartingCustSupp','EndingCustSupp','bAllDate','bDateSel','StartingDate','EndingDate','bDefaultPath','DirXMLToBeElabPath','ImportNFeHead','BRImportNFePymtSched','ImportNFedetail','ImgHead_NFeAlreadyGenerated','ImgHead_NFeNotCompleted','ImgHead_NFeCompleted','ImgHead_NFeCompletedWithWarnings','ImgHead_LockedByOtherUser','ImgDetail_RowNotCompleted','ImgDetail_RowCompleted','ImgDetail_RowCompletedWithWarnings'],'ImportNFeHead':['TEnhBRImp_SyncStatusBmp','OperationDescription_Send','NotaFiscalCode_Cons','Series_Send','DocNo_Send','TotalAmount_Send','DocumentDate_Send','Payment_Cons','CustSupp_Cons','CompanyName_Send','TaxIdNumber_Send','FiscalCode_Send','FederalState_Send','CityName_Send','Telephone_Send','PaymentType_Send','Transport_Cons','PymtCash_Cons','PymtAccount_Cons','CostCenter_Cons','ChNFe_Send','CompanyName_Cons','TaxIdNumber_Cons','FiscalCode_Cons','FederalState_Cons','CityName_Cons','ReceiptDate_Cons','ConsultDate_Cons','ProcessDate_Cons','ImportDate_Cons','HeadErrors_Cons'],'HKLPaymentTerms':['Description'],'HKLTransportMode':['Description'],'BRImportNFePymtSched':['TEnhBRImpPymt_SyncStatusBmp','InstallmentAmount_Send','DueDate_Send'],'ImportNFedetail':['TEnhBRImpDet_SyncStatusBmp','Item_Send','Item_Cons','Description_Send','UoM_Send','Qty_Send','UoMOfQtySend_Cons','BaseUoM_Cons','QtyInBaseUoM_Cons','UnitValue_Send','ProductValue_VProd_Send','DiscountValue_VDesc_Send','Barcode_Send','TaxRuleCodeCompany_Cons','CFOP_Send','NCM_Send','ICMS_Taxable_Send','ICMS_Perc_Send','ICMS_Value_Send','DetailErrors_Cons','FiscalQty_Send','FiscalValue_Send','ProductAddInfo_Send'],'HKLBRTaxRulesCompany':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TD_BR_IMPORTNFEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TD_BR_IMPORTNFEComponent, resolver);
    }
} 