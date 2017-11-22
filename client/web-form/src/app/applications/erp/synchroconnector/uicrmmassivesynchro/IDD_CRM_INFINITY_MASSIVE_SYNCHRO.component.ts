import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CRM_INFINITY_MASSIVE_SYNCHROService } from './IDD_CRM_INFINITY_MASSIVE_SYNCHRO.service';

@Component({
    selector: 'tb-IDD_CRM_INFINITY_MASSIVE_SYNCHRO',
    templateUrl: './IDD_CRM_INFINITY_MASSIVE_SYNCHRO.component.html',
    providers: [IDD_CRM_INFINITY_MASSIVE_SYNCHROService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CRM_INFINITY_MASSIVE_SYNCHROComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CRM_INFINITY_MASSIVE_SYNCHROService,
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
		boService.appendToModelStructure({'global':['bDelta','bItems','bCustomers','bSuppliers','bContacts','bProspSuppliers','bAttachments','bCurrencies','bLanguages','bISOCountryCode','bPaymentTerm','bBanks','bBanksCompany','bBanksCustSupp','bIntrastat','bIntrastatCPA','bIntrastatNomecl','bGeneralUoM','bItemCtg','bCommodityCtg','bItemType','bHomogeneousCtg','bProductCtg','bSalesPeople','bSalesPeopleMaster','bSalesPeopleAreas','bTransport','bCarriers','bTaxCodes','bPriceLists','bStorages','bTitles','bCustCategories','bCustClassifications','bProducers','bCustCommCtg','bSuppClassification','bSuppCategories','bInvoices','bCustorder','bSuppOrder','bAccountPayable','bAccountReceivable','bDDT','bBillLanding','bCreditNote','DBTLinksTable'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CRM_INFINITY_MASSIVE_SYNCHROFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CRM_INFINITY_MASSIVE_SYNCHROComponent, resolver);
    }
} 