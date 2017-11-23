import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYABLESCLOSINGService } from './IDD_PAYABLESCLOSING.service';

@Component({
    selector: 'tb-IDD_PAYABLESCLOSING',
    templateUrl: './IDD_PAYABLESCLOSING.component.html',
    providers: [IDD_PAYABLESCLOSINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PAYABLESCLOSINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PYMTORD_PYMTTERM_itemSource: any;

    constructor(document: IDD_PAYABLESCLOSINGService,
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
        this.IDC_PYMTORD_PYMTTERM_itemSource = {
  "name": "ClosingEnumCombo",
  "namespace": "ERP.AP_AR.Documents.ClosingEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AllPymtTerms','TypeSel','PymtOrdType','FilterPymtOrders','FromDate','ToDate','CreditNote','AllSupp','SuppSel','FromSupplier','ToSupplier','FromDocDate','ToDocDate','FromDocNo','ToDocNo','FilterAmount','MinimumAmount','LimitAmount','PuchOrdSel','OrdDateSel','NormalClosing','AllowanceClosing','bNotSelectedPymtTerm','bSelectedPymtTerm','PymtTerm','bCashOpening','bSelectedCash','sPymtCash','PosAllowancePymtCash','PosAllowancePymtAccount','NegRoundingPymtAccount','BatchClosingInAccounting','NrDoc','IssueDate','IssueDate','Account','PymtOrders','ToBePresented','ProcessStatus','BlockedImage','LitigationImage'],'HKLAccount':['Description'],'PymtOrders':['l_TEnhPymtOrders_P07','l_TEnhPymtOrders_P24','l_TEnhPymtOrders_P01','l_TEnhPymtOrders_P15','l_TEnhPymtOrders_P16','l_TEnhPymtOrders_P08','l_TEnhPymtOrders_P18','PaymentTerm','l_TEnhPymtOrders_P23','CustSupp','l_TEnhPymtOrders_P09','l_TEnhPymtOrders_P22','l_TEnhPymtOrders_P25','l_TEnhPymtOrders_P26','l_TEnhPymtOrders_P13','l_TEnhPymtOrders_P27','l_TEnhPymtOrders_P31','PymtCash','PymtAccount','CostCenter','ValueDate','l_TEnhPymtOrders_P20','l_TEnhPymtOrders_P21','Amount','Amount','l_TEnhPymtOrders_P32','l_TEnhPymtOrders_P28','l_TEnhPymtOrders_P29','l_TEnhPymtOrders_P33','l_TEnhPymtOrders_P30','l_TEnhPymtOrders_P17','CustSuppBank','l_TEnhPymtOrders_P10','CA','CIN','l_TEnhPymtOrders_P19','PresentationBank','l_TEnhPymtOrders_P11','DebitCA','l_TEnhPymtOrders_P04','PresentationNotes','ChargesType','StatisticType','StatisticReason','CustomTariff','ChargesType','SEPACategoryPurpose','l_TEnhPymtOrders_P05','Presented']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYABLESCLOSINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYABLESCLOSINGComponent, resolver);
    }
} 