﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASE_INVOICE_LOAD_FILTERService } from './IDD_PURCHASE_INVOICE_LOAD_FILTER.service';

@Component({
    selector: 'tb-IDD_PURCHASE_INVOICE_LOAD_FILTER',
    templateUrl: './IDD_PURCHASE_INVOICE_LOAD_FILTER.component.html',
    providers: [IDD_PURCHASE_INVOICE_LOAD_FILTERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASE_INVOICE_LOAD_FILTERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHASE_INVOICE_LOAD_FILTERService,
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
		boService.appendToModelStructure({'global':['PurchaseInvoice','CorrectionPurchaseInvoice','SuppInvoiceFilter','InvoiceFilter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASE_INVOICE_LOAD_FILTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASE_INVOICE_LOAD_FILTERComponent, resolver);
    }
} 