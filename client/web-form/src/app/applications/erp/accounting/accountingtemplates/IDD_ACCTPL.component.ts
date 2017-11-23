import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCTPLService } from './IDD_ACCTPL.service';

@Component({
    selector: 'tb-IDD_ACCTPL',
    templateUrl: './IDD_ACCTPL.component.html',
    providers: [IDD_ACCTPLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ACCTPLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ACCTPL_CODETYPE_OPERATION_itemSource: any;

    constructor(document: IDD_ACCTPLService,
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
        this.IDC_ACCTPL_CODETYPE_OPERATION_itemSource = {
  "name": "OperationEnumCombo",
  "namespace": "ERP.IdsMng.Components.OperationEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'AccountingTemplates':['Template','Disabled','Description','Operation','DocDateIsMand','DocNoIsMand','CodeType','PaymentScheduleAction','PaymentScheduleCreditNote','AccrualDeferral','GroupCode','Currency','ExcludedFromSAC','AGOAccReason','TaxJournal','CustSupp','Suspension','IntrastatOperation','ReverseChargeType','EUTaxJournal','TypeOfTaxDocument','TypeOfTaxDocumentAnn','TypeOfReverseCharge'],'global':['GLDetail','TaxDetail','RetailDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'GLDetail':['TEnhAccTemplatesGLDetail_P4'],'HKLAGOAccReasons':['Description'],'HKLTaxJournals':['Description'],'HKLCustSupp':['CompNameComplete'],'HKLJournalIntrastatTax':['Description'],'TaxDetail':['TaxCode','TaxableAmount','TaxAmount','TotalAmount','UndeductibleAmount','Notes'],'HKLTaxCode':['Description'],'RetailDetail':['TaxCode','DebitAccount','CreditAccount'],'HKLRtlTaxCode':['Description'],'HKLDebitAccount':['Description'],'HKLCreditAccount':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCTPLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCTPLComponent, resolver);
    }
} 