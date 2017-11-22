import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INTRA_PURCHASEService } from './IDD_INTRA_PURCHASE.service';

@Component({
    selector: 'tb-IDD_INTRA_PURCHASE',
    templateUrl: './IDD_INTRA_PURCHASE.component.html',
    providers: [IDD_INTRA_PURCHASEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INTRA_PURCHASEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_INTRAT_PURCHASE_NATUREOFTRANSACTION_itemSource: any;

    constructor(document: IDD_INTRA_PURCHASEService,
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
        this.IDC_INTRAT_PURCHASE_NATUREOFTRANSACTION_itemSource = {
  "name": "NaturePurchaseTerEnumCombo",
  "namespace": "ERP.Intrastat.Documents.NaturePurchaseTerEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'IntraArrivals':['BalanceYear','AccrualDate','BalanceMonth','Quarter','CustSupp','Currency','FixingDate','Fixing'],'HKLCustSupp':['CompanyName'],'global':['JEReferences','Details','Details','Details','Adjustments','Adjustments','ServicesDetails','ServicesAdjustments','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLCurrenciesCurrObj':['Description'],'Details':['NetMass','SuppUnit'],'Adjustments':['Operation','NatureOfTransaction','CombinedNomenclature','TotalAmountDocCurr','TotalAmountDocCurr','TotalAmount','TotalAmount','DebitCreditSign','CorrectionYear','l_TEnhIntraArrivals1B_P1','StatisticalValue','StatisticalValue','Notes'],'ServicesDetails':['ProgNo','CPACode','TotalAmountDocCurr','TotalAmount','CountryOfPayment','DocNo','DocumentDate','IntrastatSupplyType','IntrastatCollectionType'],'ServicesAdjustments':['RefIntrastatId','BalanceYear','LogNo','ProgNo','CustomsSectionCode','CPACode','TotalAmountDocCurr','TotalAmountDocCurr','TotalAmount','TotalAmount','CountryOfPayment','DocNo','DocumentDate','IntrastatSupplyType','IntrastatCollectionType']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INTRA_PURCHASEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INTRA_PURCHASEComponent, resolver);
    }
} 