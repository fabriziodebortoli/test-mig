import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ANNTAXService } from './IDD_ANNTAX.service';

@Component({
    selector: 'tb-IDD_ANNTAX',
    templateUrl: './IDD_ANNTAX.component.html',
    providers: [IDD_ANNTAXService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ANNTAXComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TAXCOMMUNICATION_POSITIONCODE_itemSource: any;

    constructor(document: IDD_ANNTAXService,
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
        this.IDC_TAXCOMMUNICATION_POSITIONCODE_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.Payees.PositionCode"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','SaleOp','PurchOp','NotTaxSaleOp','NotTaxPurchOp','ExempSaleOp','ExempPurchOp','IntraSaleOp','IntraPurchOp','FixedAssetsSaleOp','FixedAssetsPurchOp','GoldTaxableAmount','GoldTax','ScrapTaxableAmount','ScrapTaxAmount','TaxExigibility','DeductableTax','DueTax','CreditTax','DifferentDecl','PositionCode_XML','SubscriberFiscalCode','SubscriberFiscalCode2','SeparateAccounting','CompanyGroupDecl','bExceptionalEvent','ConfirmFC','TaxPayerReserve','ConfirmingFlag','IntermediaryReserve','Signature','ReserveDate','UserField','FreeSpace','Transmit']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ANNTAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ANNTAXComponent, resolver);
    }
} 