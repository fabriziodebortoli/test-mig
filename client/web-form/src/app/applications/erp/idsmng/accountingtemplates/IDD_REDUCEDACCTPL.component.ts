import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_REDUCEDACCTPLService } from './IDD_REDUCEDACCTPL.service';

@Component({
    selector: 'tb-IDD_REDUCEDACCTPL',
    templateUrl: './IDD_REDUCEDACCTPL.component.html',
    providers: [IDD_REDUCEDACCTPLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_REDUCEDACCTPLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ACCTPL_CODETYPE_OPERATION_itemSource: any;

    constructor(document: IDD_REDUCEDACCTPLService,
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
        this.IDC_ACCTPL_CODETYPE_OPERATION_itemSource = {
  "name": "OperationEnumCombo",
  "namespace": "ERP.IdsMng.Components.OperationEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'AccountingTemplates':['Template','Disabled','Description','Operation','DocDateIsMand','DocNoIsMand','CodeType','PaymentScheduleAction','PaymentScheduleCreditNote','AccrualDeferral','GroupCode','Currency','ExcludedFromSAC','TaxJournal','CustSupp','Suspension','IntrastatOperation','ReverseChargeType','EUTaxJournal','TypeOfTaxDocument','TypeOfTaxDocumentAnn','TypeOfReverseCharge'],'HKLTaxJournals':['Description'],'HKLCustSupp':['CompNameComplete'],'HKLJournalIntrastatTax':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_REDUCEDACCTPLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_REDUCEDACCTPLComponent, resolver);
    }
} 