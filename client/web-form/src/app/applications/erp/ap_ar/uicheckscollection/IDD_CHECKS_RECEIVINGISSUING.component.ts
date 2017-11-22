import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CHECKS_RECEIVINGISSUINGService } from './IDD_CHECKS_RECEIVINGISSUING.service';

@Component({
    selector: 'tb-IDD_CHECKS_RECEIVINGISSUING',
    templateUrl: './IDD_CHECKS_RECEIVINGISSUING.component.html',
    providers: [IDD_CHECKS_RECEIVINGISSUINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CHECKS_RECEIVINGISSUINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CHECKS_RECEIVINGISSUINGService,
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
		boService.appendToModelStructure({'global':['Customer','Date','DocNoFilter','PostingDate','DocDate','NrDoc','Checks','AdvanceAmount','AdvanceNrDoc','Bills','TotalChecks','TotalAmount'],'HKLCustSupp':['CompanyName'],'Checks':['l_TEnhChecks_P06','l_TEnhChecks_P01','IssuerBank','IssuerBank','IssuerBankCA','IssuerBankCA','FiscalNo','BillType','Amount','DueDate','IssuePlace','IssueDate','IssuerName','IssuerTaxIdNumber','IssuerFiscalCode','Description','BillCode'],'Bills':['l_TEnhBillsCollection_P01','BillNo','CustSupp','l_TEnhBillsCollection_P02','InstallmentDate','PaymentTerm','Currency','PayableAmountInBaseCurr','PresentationAmountBaseCurr','l_TEnhBillsCollection_P18','l_TEnhBillsCollection_P19','l_TEnhBillsCollection_P20','PayableAmountInBaseCurr','l_TEnhBillsCollection_P21','l_TEnhBillsCollection_P22','Closed','PresentationAmountBaseCurr','PayableAmountInBaseCurr','Closed','PayableAmountInBaseCurr','PayableAmountInBaseCurr','l_TEnhBillsCollection_P19','l_TEnhBillsCollection_P20','PresentationAmountBaseCurr','l_TEnhBillsCollection_P17','l_TEnhBillsCollection_P03','l_TEnhBillsCollection_P07','CustSupp','l_TEnhBillsCollection_P02','ValueDate','l_TEnhBillsCollection_P28','l_TEnhBillsCollection_P23','l_TEnhBillsCollection_P24','CollectionDate','l_TEnhBillsCollection_P25','l_TEnhBillsCollection_P26','l_TEnhBillsCollection_P27','CustSuppBank']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CHECKS_RECEIVINGISSUINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CHECKS_RECEIVINGISSUINGComponent, resolver);
    }
} 