﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOGISTIC_CONTROLService } from './IDD_LOGISTIC_CONTROL.service';

@Component({
    selector: 'tb-IDD_LOGISTIC_CONTROL',
    templateUrl: './IDD_LOGISTIC_CONTROL.component.html',
    providers: [IDD_LOGISTIC_CONTROLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOGISTIC_CONTROLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_LOGISTICS_CONTROL_PANEL_DOCUMENT_TYPE_itemSource: any;

    constructor(document: IDD_LOGISTIC_CONTROLService,
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
        this.IDC_LOGISTICS_CONTROL_PANEL_DOCUMENT_TYPE_itemSource = {
  "name": "BrowsingDocumentTypeCombo",
  "namespace": "ERP.InvoiceMng.Documents.BrowsingDocumentTypeCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bCustomer','bSupplier','CustSupp','bAllDocuments','DocumentType','DocumentNo','StartingDate','EndingDate','bOrderByDocType','bOrderByDocDate','ControlPanelDetailSelection','DBTNodeDetail','ImageSuppQuota','ImageSuppQuotaProcessed','ImagePurchOrder','ImagePurchOrderProcessed','ImagePurchOrderCancelled','ImageBOL','ImageBOLProcessed','ImageInspOrder','ImageInspOrderProcessed','ImageInspNote','ImagePurchDoc','ImagePurchDocProcessed','ImageCustQuota','ImageSaleOrder','ImageSaleOrderAllocated','ImageSaleOrderProcessed','ImageSaleOrderBlocked','ImageSaleOrderCancelled','ImageDeliveryNote','ImageDeliveryNoteProcessed','ImagePickList','ImagePickListProcessed','ImageSaleDocument','ImageSaleDocumentProcessed','ImageAddCharges','ImageInvEntry','ImageAccounting','ImageReceivable'],'ControlPanelDetailSelection':['ModifiableLineBmp','SelectionType'],'DBTNodeDetail':['l_FieldValue','l_FieldName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOGISTIC_CONTROLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOGISTIC_CONTROLComponent, resolver);
    }
} 