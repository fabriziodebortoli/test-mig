import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RECEIVABLESCLOSINGService } from './IDD_RECEIVABLESCLOSING.service';

@Component({
    selector: 'tb-IDD_RECEIVABLESCLOSING',
    templateUrl: './IDD_RECEIVABLESCLOSING.component.html',
    providers: [IDD_RECEIVABLESCLOSINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RECEIVABLESCLOSINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BILLS_PYMTTERM_itemSource: any;

    constructor(document: IDD_RECEIVABLESCLOSINGService,
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
        this.IDC_BILLS_PYMTTERM_itemSource = {
  "name": "DeclarationExportFileEnumCombo",
  "namespace": "ERP.AP_AR.Documents.VouchersManagementEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AllPymtTerms','TypeSel','BillsType','FilterBills','FromDate','ToDate','CreditNote','AllCustSupp','CustSel','FromCustomer','ToCustomer','FromDocDate','ToDocDate','FromDocNo','ToDocNo','FilterAmount','MinimumAmount','LimitAmount','SaleOrdSel','OrdDateSel','NormalClosing','AllowanceClosing','bNotSelectedPymtTerm','bSelectedPymtTerm','PymtTerm','bCashOpening','bSelectedCash','sPymtCash','NegAllowancePymtCash','NegAllowancePymtAccount','PosRoundingPymtAccount','BatchClosingInAccounting','NrDoc','PresentationDate','Account','Bills','ToBePresented','ProcessStatus','BillsPresentableTot'],'HKLAccount':['Description'],'Bills':['l_TEnhBills_P01','l_TEnhBills_P15','l_TEnhBills_P16','OpenedAdmCases','l_TEnhBills_P08','PaymentTerm','CustSupp','l_TEnhBills_P09','l_TEnhBills_P14','l_TEnhBills_P19','l_TEnhBills_P20','l_TEnhBills_P13','l_TEnhBills_P21','l_TEnhBills_P25','PymtCash','PymtAccount','CostCenter','Amount','Amount','l_TEnhBills_P26','l_TEnhBills_P22','l_TEnhBills_P23','l_TEnhBills_P27','l_TEnhBills_P24','BillNo','CustSuppBank','l_TEnhBills_P10','PresentationBank','l_TEnhBills_P11','CA','Presentation','l_TEnhBills_P04','PresentationNotes','l_TEnhBills_P05','Presented','BillNo','Slip','MandateCode','l_TEnhBills_P18','MandateSequenceType']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RECEIVABLESCLOSINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RECEIVABLESCLOSINGComponent, resolver);
    }
} 