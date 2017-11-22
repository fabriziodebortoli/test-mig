import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INROMANEIOLOADINGService } from './IDD_INROMANEIOLOADING.service';

@Component({
    selector: 'tb-IDD_INROMANEIOLOADING',
    templateUrl: './IDD_INROMANEIOLOADING.component.html',
    providers: [IDD_INROMANEIOLOADINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INROMANEIOLOADINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INROMANEIOLOADINGService,
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
		boService.appendToModelStructure({'global':['bAllDocumentDate','bSelDocumentDate','FromDocumentDate','ToDocumentDate','bAllDocType','bDocTypeNFCust','bDocTypeNFSupp','bAllCustSupp','bSelCustSupp','FromCustSupp','ToCustSupp','bAllDocumentNo','bSelDocumentNo','FromDocumentNo','ToDocumentNo','bAllVehicle','bSelVehicle','SelectedVehicleCode','SelectedVehicleLicensePlate','NFInRomaneioLoading'],'NFInRomaneioLoading':['TEnhNFInRo_Select','TEnhNFInRo_Series','TEnhNFInRo_Model','TEnhNFInRo_DocNo','TEnhNFInRo_DocDate','TEnhNFInRo_CustSuppType','TEnhNFInRo_CustSupp','TEnhNFInRo_CustSuppCompanyName','TEnhNFInRo_CustSuppFederalState','TEnhNFInRo_CustSuppCity','TEnhNFInRo_DeliveryToCompanyName','TEnhNFInRo_DeliveryToFederalState','TEnhNFInRo_DeliveryToCity','TEnhNFInRo_Currency']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INROMANEIOLOADINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INROMANEIOLOADINGComponent, resolver);
    }
} 