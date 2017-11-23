import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INTRA_DISPATCHESService } from './IDD_INTRA_DISPATCHES.service';

@Component({
    selector: 'tb-IDD_INTRA_DISPATCHES',
    templateUrl: './IDD_INTRA_DISPATCHES.component.html',
    providers: [IDD_INTRA_DISPATCHESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INTRA_DISPATCHESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_INTRAT_DISPATCHES_NATUREOFTRANSACTION_itemSource: any;

    constructor(document: IDD_INTRA_DISPATCHESService,
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
        this.IDC_INTRAT_DISPATCHES_NATUREOFTRANSACTION_itemSource = {
  "name": "NatureDispatchesTerEnumCombo",
  "namespace": "ERP.Intrastat.Documents.NatureDispatchesTerEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'IntraDispatches':['BalanceYear','AccrualDate','BalanceMonth','Quarter','CustSupp'],'HKLCustSupp':['CompanyName'],'global':['JEReferences','Details','Details','Details','Adjustments','Adjustments','ServicesDetails','ServicesAdjustments','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Details':['Operation'],'Adjustments':['Operation','NatureOfTransaction','CombinedNomenclature','TotalAmount','TotalAmount','DebitCreditSign','CorrectionYear','l_TEnhIntraDispatches2B_P1','StatisticalValue','StatisticalValue','Notes'],'ServicesDetails':['ProgNo','CPACode','TotalAmount','CountryOfPayment','DocNo','DocumentDate','IntrastatSupplyType','IntrastatCollectionType'],'ServicesAdjustments':['RefIntrastatId','BalanceYear','LogNo','ProgNo','CustomsSectionCode','CPACode','TotalAmount','TotalAmount','CountryOfPayment','DocNo','DocumentDate','IntrastatSupplyType','IntrastatCollectionType']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INTRA_DISPATCHESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INTRA_DISPATCHESComponent, resolver);
    }
} 