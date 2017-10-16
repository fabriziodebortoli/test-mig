﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver } from '@angular/core';

import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';

import { IDD_TAXCODESUPDATEService } from './IDD_TAXCODESUPDATE.service';

@Component({
    selector: 'tb-IDD_TAXCODESUPDATE',
    templateUrl: './IDD_TAXCODESUPDATE.component.html',
    providers: [IDD_TAXCODESUPDATEService, ComponentInfoService]
})
export class IDD_TAXCODESUPDATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXCODESUPDATEService,
    eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        ciService: ComponentInfoService) {
        super(document, eventData, resolver, ciService);
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['TaxCodesUpdate','bUpdateDefaultCode','bUpdateAccTpl','bUpdateCust','bUpdateSupp','bUpdateContact','bUpdateProspSupp','bUpdateFeeTpl','bUpdateProformaFees','ProformaFeesDate','bUpdateSuppQuotas','SuppQuotaDate','bUpdateSuppOrders','SuppOrdDate','bUpdateSuppDocuments','SuppDocDate','bUpdateCustQuotas','CustQuotaDate','bUpdateCustOrders','CustOrdDate','bUpdateCustDN','CustDNDate','bUpdateCustDNCharge','CustDNChargeDate','bUpdateCustProforma','CustProformaDate','bUpdateCustPick','CustPickDate','bUpdateItems','bUpdateCustCtg','bUpdateSmartCode','bUpdateConai','bUpdateWEEE','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','nCurrentElement','GaugeDescription','ProgressViewer'],'TaxCodesUpdate':['TaxCode','TEnhTaxCodes_P1'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }
    ngOnDestroy() {
        super.ngOnDestroy();
    }

}

@Component({
    template: ''
})
export class IDD_TAXCODESUPDATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXCODESUPDATEComponent, resolver);
    }
} 