import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALE_ORDERS_UNBLOCKService } from './IDD_SALE_ORDERS_UNBLOCK.service';

@Component({
    selector: 'tb-IDD_SALE_ORDERS_UNBLOCK',
    templateUrl: './IDD_SALE_ORDERS_UNBLOCK.component.html',
    providers: [IDD_SALE_ORDERS_UNBLOCKService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SALE_ORDERS_UNBLOCKComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALE_ORDERS_UNBLOCKService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FromDate','EndingDate','FromExpectedDeliveryDate','EndingExpectedDeliveryDate','bAllOrdNo','bOrdNoSel','FromOrdNo','ToOrdNo','bAllCustomer','bCustomerSel','CustomerStart','CustomerEnd','AllPriority','PrioritySel','FromPriority','ToPriority','bAllJob','bJobSel','JobStart','JobEnd','AllSalesperson','SalespersonSel','FromSalesperson','ToSalesperson','bAllAreaManager','bAreaManagerSel','AreaManagerStart','AreaManagerEnd','bAllArea','bAreaSel','AreaStart','AreaEnd','SaleOrdersUnblock','CLViewerSingleOrderLimit','CLViewerSingleOrderExposure','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerSingleOrderExposure','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerSingleOrderMargin','CLViewerImageStatusSingleOrder','CLViewerOrderedLimit','CLViewerOrderedExposure','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerOrderedExposure','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerOrderedMargin','CLViewerImageStatusOrdered','CLViewerTurnoverLimit','CLViewerTurnoverExposure','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverExposure','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverMargin','CLViewerImageStatusTurnover','CLViewerTurnoverBills','CLViewerTurnoverOtherPaymentTerms','CLViewerTurnoverInvWithoutRec','CLViewerDeliveredDocNotInvoiced','CLViewerTotalExposureLimit','CLViewerTotalExposureExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','CLViewerTotalExposureExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','CLViewerTotalExposureMargin','CLViewerImageStatusTotalExposure','Documents'],'SaleOrdersUnblock':['StatusBmp','SaleOrdersUnblock_Selected','InvoicingCustomer','CustomerDescri','InternalOrdNo','OrderDate','ExternalOrdNo','Priority','ConfirmedDeliveryDate','ExpectedDeliveryDate','Payment','Salesperson','AreaManager','Area','Job'],'Documents':['CreditType','DocumentType','DocumentNo','DocumentDate','Amount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALE_ORDERS_UNBLOCKFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALE_ORDERS_UNBLOCKComponent, resolver);
    }
} 