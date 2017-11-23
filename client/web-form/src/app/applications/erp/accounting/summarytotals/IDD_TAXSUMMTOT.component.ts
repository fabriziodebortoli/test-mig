import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXSUMMTOTService } from './IDD_TAXSUMMTOT.service';

@Component({
    selector: 'tb-IDD_TAXSUMMTOT',
    templateUrl: './IDD_TAXSUMMTOT.component.html',
    providers: [IDD_TAXSUMMTOTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXSUMMTOTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXSUMMTOTService,
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
		boService.appendToModelStructure({'TaxSummaryTotals':['BalanceYear','Period','LastPage','PaymentDetails','DefinitivelyPrinted','ExigibleTax','DeductibleTax','DebitTax','CreditTax','PreviousDebitTax','PreviousCreditTax','PreviousYearCreditTax','ExcludedCreditTax','IncludedCreditTax','ImportedCarsTaxPaid','SpecialCreditTax','Interests','DebitTaxPeriod','CreditTaxPeriod'],'global':['Amount','Credit','VP9','Advance','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXSUMMTOTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXSUMMTOTComponent, resolver);
    }
} 