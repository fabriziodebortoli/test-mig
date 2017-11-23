import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EI_CUSTOMERService } from './IDD_EI_CUSTOMER.service';

@Component({
    selector: 'tb-IDD_EI_CUSTOMER',
    templateUrl: './IDD_EI_CUSTOMER.component.html',
    providers: [IDD_EI_CUSTOMERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EI_CUSTOMERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EI_CUSTOMERService,
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
		boService.appendToModelStructure({'global':['bEIAll','bEIPublicAuth','bEINoPublicAuth','bAllCustomer','bCustomerSel','FromCustomer','ToCustomer','bAllDocNo','bDocNoSel','FromDocNo','ToDocNo','DateFrom','DateTo','EIChecks'],'EIChecks':['TEIMDCDocDetail_P10','TEIMDCDocDetail_P11','TEIMDCDocDetail_P11','DocumentType','DocNo','DocumentDate','EIStatus','CustSupp','TEIMDCDocDetail_P04','TEIMDCDocDetail_P07','TaxJournal','TEIMDCDocDetail_P03','TEIMDCDocDetail_P05','TEIMDCDocDetail_P21','TEIMDCDocDetail_P22','TEIMDCDocDetail_P23','TEIMDCDocDetail_P17','TEIMDCDocDetail_P16']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EI_CUSTOMERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EI_CUSTOMERComponent, resolver);
    }
} 