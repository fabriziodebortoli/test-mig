import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BILLSPRESENTATIONService } from './IDD_BILLSPRESENTATION.service';

@Component({
    selector: 'tb-IDD_BILLSPRESENTATION',
    templateUrl: './IDD_BILLSPRESENTATION.component.html',
    providers: [IDD_BILLSPRESENTATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BILLSPRESENTATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BILLSPRESENTATIONService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AllPymtTerms','TypeSel','BillsType','FilterBills','FromDate','ToDate','CreditNote','AllCustSupp','CustSel','FromCustomer','ToCustomer','FromDocDate','ToDocDate','FromDocNo','ToDocNo','AllCustBank','CustBankSel','CustomerBank','ToCustomerBank','MinimumAmount','LimitAmount','SaleOrdSel','OrdDateSel','GroupType','NrDays','BillAmount','SetLastDate','UseBranch','UseContract','ParameterBank','UseBank','AllPresBank','PresBankSel1','PresBankSel','PresentationDate','Bills','BillsPresentableTot','ProcessStatus','TaxSummary','bOverSummary','Banks','PresentationResult','ToBePresented','ProcessStatus','BillsPresentableTot'],'Bills':['l_TEnhBills_P01','l_TEnhBills_P15','l_TEnhBills_P16','OpenedAdmCases','l_TEnhBills_P08','PaymentTerm','CustSupp','l_TEnhBills_P09','l_TEnhBills_P14','l_TEnhBills_P19','l_TEnhBills_P20','l_TEnhBills_P13','l_TEnhBills_P21','l_TEnhBills_P25','PymtCash','PymtAccount','CostCenter','Amount','Amount','l_TEnhBills_P26','l_TEnhBills_P22','l_TEnhBills_P23','l_TEnhBills_P27','l_TEnhBills_P24','BillNo','CustSuppBank','l_TEnhBills_P10','PresentationBank','l_TEnhBills_P11','CA','Presentation','l_TEnhBills_P04','PresentationNotes','l_TEnhBills_P05','Presented','BillNo','Slip','MandateCode','l_TEnhBills_P18','MandateSequenceType'],'Banks':['l_TEnhBillsBanks_P1','Bank','l_TEnhBillsBanks_P2','CA','FactoringType','FactoringAdvance','BorrowingRate','Presentation','Currency','MaxCreditLimit','l_TEnhBillsBanks_P3','Account','l_TEnhBillsBanks_P4'],'PresentationResult':['Slip','PresentationBank','CA','Presentation','Currency','l_TEnhPresentationResult_P1','l_TEnhPresentationResult_P2','l_TEnhPresentationResult_P3','l_TEnhPresentationResult_P4']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BILLSPRESENTATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BILLSPRESENTATIONComponent, resolver);
    }
} 