import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCService } from './IDD_COSTACC.service';

@Component({
    selector: 'tb-IDD_COSTACC',
    templateUrl: './IDD_COSTACC.component.html',
    providers: [IDD_COSTACCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_COSTACCComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_COSTACC_ENTRYTYPE_itemSource: any;

    constructor(document: IDD_COSTACCService,
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
        this.IDC_COSTACC_ENTRYTYPE_itemSource = {
  "name": "EntryTypeForecastEnumCombo",
  "namespace": "ERP.CostAccounting.Components.EntryTypeForecastEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Header':['Account','PostingDate','AccrualDate','RefNo','Reversing','CodeType','DebitCreditSign','TotalAmount','Notes','DocumentDate','DocNo','LogNo','DocNo','LogNo','RefDocNo','CustSuppType','CustSupp'],'HKLAccount':['Description'],'HKLCustSupp':['CompNameComplete'],'global':['Detail','DebitCCTot','CostCentersDebitVar','DebitJobTot','JobsDebitVar','DebitLineTot','LinesDebitVar','CreditCCTot','CostCentersCreditVar','CreditJobTot','JobsCreditVar','CreditLineTot','LinesCreditVar','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['AccrualDate','CostCenter','Job','ProductLine','DebitCreditSign','Perc','Amount','Item','Qty','Notes'],'HKLDetailCstCenter':['Description'],'HKLDetailJob':['Description'],'HKLDetailProductLine':['Description'],'HKLDetailItem':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCComponent, resolver);
    }
} 