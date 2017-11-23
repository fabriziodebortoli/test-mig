import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NOTA_FISCAL_MAINTENANCEService } from './IDD_NOTA_FISCAL_MAINTENANCE.service';

@Component({
    selector: 'tb-IDD_NOTA_FISCAL_MAINTENANCE',
    templateUrl: './IDD_NOTA_FISCAL_MAINTENANCE.component.html',
    providers: [IDD_NOTA_FISCAL_MAINTENANCEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_NOTA_FISCAL_MAINTENANCEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_NF_CUST_MAINT_NOTA_FISCAL_CODE_validators: any;
public IDC_NF_SUPP_MAINT_NOTA_FISCAL_CODE_validators: any;
public IDC_NF_CUST_MAINT_NOTA_FISCAL_STATUS_itemSource: any;
public IDC_NF_SUPP_MAINT_NOTA_FISCAL_STATUS_itemSource: any;

    constructor(document: IDD_NOTA_FISCAL_MAINTENANCEService,
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
        this.IDC_NF_CUST_MAINT_NOTA_FISCAL_CODE_validators = [
  {
    "name": "NotaFiscalTypeCombo",
    "namespace": "Validator.erp.Business_BR.Components.NotaFiscalTypeCombo"
  }
]; 
this.IDC_NF_SUPP_MAINT_NOTA_FISCAL_CODE_validators = [
  {
    "name": "NotaFiscalTypeCombo",
    "namespace": "Validator.erp.Business_BR.Components.NotaFiscalTypeCombo"
  }
]; 
this.IDC_NF_CUST_MAINT_NOTA_FISCAL_STATUS_itemSource = {
  "name": "NotaFiscalForCustStatusCombo",
  "namespace": "MDC.ElectronicInvoicing_BR.AddOnsSales.NotaFiscalForCustStatusCombo"
}; 
this.IDC_NF_SUPP_MAINT_NOTA_FISCAL_STATUS_itemSource = {
  "name": "NotaFiscalForSuppStatusCombo",
  "namespace": "MDC.ElectronicInvoicing_BR.AddOnsPurchases.NotaFiscalForSuppStatusCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTBRNFForCustSaleDocMaintenance':['DocNo','DocumentDate','PostingDate','CustSupp','EIStatus'],'DBTBRNotaFiscalForCustMaintenance':['ThirdParties','NotaFiscalCode','Model','Series','ChNFe','PostedToRomaneio','DocNoNFServices','DocDateNFServices'],'HKLBRNotaFiscalType':['Description','Description'],'HKLCustomer':['CompNameComplete'],'DBTBRNFForSuppPurchDocMaintenance':['DocNo','DocumentDate','PostingDate','Supplier','EIStatus'],'DBTBRNotaFiscalForSuppMaintenance':['ThirdParties','NotaFiscalCode','Model','Series','ChNFe','PostedToRomaneio','DocNoNFServices','DocDateNFServices'],'HKLSupplier':['CompNameComplete'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NOTA_FISCAL_MAINTENANCEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NOTA_FISCAL_MAINTENANCEComponent, resolver);
    }
} 