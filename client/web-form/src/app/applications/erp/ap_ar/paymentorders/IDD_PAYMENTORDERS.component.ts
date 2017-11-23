import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYMENTORDERSService } from './IDD_PAYMENTORDERS.service';

@Component({
    selector: 'tb-IDD_PAYMENTORDERS',
    templateUrl: './IDD_PAYMENTORDERS.component.html',
    providers: [IDD_PAYMENTORDERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PAYMENTORDERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PAYMENTORDERSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Bank','CA','BOM','IssueDate','LimitDate','AllCurrency','SelCurrency','RefCurrency','PostingDate','Group','DocDate','PyntOrdCharges','NrDoc','RefCurrency','RefFixingDate','RefFixing','PymtOrders','TotalAmount','BlockedImage','LitigationImage'],'HKLBank':['Description'],'PymtOrders':['l_TEnhPaymentOrders_P06','l_TEnhPaymentOrders_P01','BillNo','l_TEnhPaymentOrders_P11','CustSupp','l_TEnhPaymentOrders_P02','InstallmentDate','PaymentTerm','l_TEnhPaymentOrders_P08','l_TEnhPaymentOrders_P09','PayableAmountInBaseCurr','l_TEnhPaymentOrders_P10','PresentationAmountBaseCurr','l_TEnhPaymentOrders_P03','CustSuppBank']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYMENTORDERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYMENTORDERSComponent, resolver);
    }
} 