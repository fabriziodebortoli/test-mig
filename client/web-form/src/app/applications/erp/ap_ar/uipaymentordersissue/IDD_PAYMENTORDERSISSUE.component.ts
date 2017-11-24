import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYMENTORDERSISSUEService } from './IDD_PAYMENTORDERSISSUE.service';

@Component({
    selector: 'tb-IDD_PAYMENTORDERSISSUE',
    templateUrl: './IDD_PAYMENTORDERSISSUE.component.html',
    providers: [IDD_PAYMENTORDERSISSUEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PAYMENTORDERSISSUEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PAYMENTORDERSISSUEService,
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
        
        		this.bo.appendToModelStructure({'global':['AllPymtTerms','TypeSel','PymtOrdType','FilterPymtOrders','FromDate','ToDate','CreditNote','AllSupp','SuppSel','FromSupplier','ToSupplier','AllCurrency','SelectedCurrency','RefCurrency','AllSuppBank','SuppBankSel','SupplierBank','ToSupplierBank','Group1','Group2','LimitAmount','PuchOrdSel','OrdDateSel','GroupType','NrDays','PymtOrdAmount','SetLastDate','UseBranch','UseContract','ParameterBank','UseBank','AllPresBank','PresBankSel1','PresBankSel','IssueDate','ValueDate','PymtOrders','PymtOrdPresentableTot','ProcessStatus','PymtOrdPresentableTotWithoutCC','TotalBalance','bOverBalance','Banks','IssueResult','ToBePresented','ProcessStatus','BlockedImage','LitigationImage'],'PymtOrders':['l_TEnhPymtOrders_P07','l_TEnhPymtOrders_P24','l_TEnhPymtOrders_P01','l_TEnhPymtOrders_P15','l_TEnhPymtOrders_P16','l_TEnhPymtOrders_P08','l_TEnhPymtOrders_P18','PaymentTerm','l_TEnhPymtOrders_P23','CustSupp','l_TEnhPymtOrders_P09','l_TEnhPymtOrders_P22','l_TEnhPymtOrders_P25','l_TEnhPymtOrders_P26','l_TEnhPymtOrders_P13','l_TEnhPymtOrders_P27','l_TEnhPymtOrders_P31','PymtCash','PymtAccount','CostCenter','ValueDate','l_TEnhPymtOrders_P20','l_TEnhPymtOrders_P21','Amount','Amount','l_TEnhPymtOrders_P32','l_TEnhPymtOrders_P28','l_TEnhPymtOrders_P29','l_TEnhPymtOrders_P33','l_TEnhPymtOrders_P30','l_TEnhPymtOrders_P17','CustSuppBank','l_TEnhPymtOrders_P10','CA','CIN','l_TEnhPymtOrders_P19','PresentationBank','l_TEnhPymtOrders_P11','DebitCA','l_TEnhPymtOrders_P04','PresentationNotes','ChargesType','StatisticType','StatisticReason','CustomTariff','ChargesType','SEPACategoryPurpose','l_TEnhPymtOrders_P05','Presented'],'Banks':['l_TEnhPymtOrdBanks_P1','Bank','l_TEnhPymtOrdBanks_P2','CA','IBAN','Currency','l_TEnhPymtOrdBanks_P4','Account','l_TEnhPymtOrdBanks_P3'],'IssueResult':['Slip','PresentationBank','CA','TEnhIssueResult_P5','l_TEnhIssueResult_P1','l_TEnhIssueResult_P2','Currency','l_TEnhIssueResult_P3','l_TEnhIssueResult_P4']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYMENTORDERSISSUEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYMENTORDERSISSUEComponent, resolver);
    }
} 