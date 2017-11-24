import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_JEService } from './IDD_JE.service';

@Component({
    selector: 'tb-IDD_JE',
    templateUrl: './IDD_JE.component.html',
    providers: [IDD_JEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_JEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_JEService,
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
        
        		this.bo.appendToModelStructure({'JournalEntries':['AccTpl','PostingDate','AccrualDate','GroupCode','RefNo','DocumentDate','DocNo','ValueDate'],'HKLAccTpl':['Description','Description'],'JESlave':['AccTpl','PostingDate','AccrualDate','DocumentDate','DocNo','DocumentDate','DocNo','TotalAmount','GroupCode','RefNo','ValueDate','PostingDate'],'JournalEntriesTax':['DepartureDate','CustSupp','CustSupp','TaxJournal','LogNo','NotExigible','ExigibilityDate','ExigibilityDate','TaxAccrualDate','PlafondAccrualDate','Notes','BlackListCustSupp','CreditNotePreviousPeriod','TaxCommunicationGroup','TaxCommunicationOperation','D394ReceiptNo'],'global':['CustSuppDescription','CustSuppDescription','FixingDescri','FixingDescri','SimulationReview','Review','GLJournal','FixingDescri','FixingDescri','TaxSummary','TaxSummary','GLJournal','SimulationReview','Review','JournalTaxButton','JournalTaxButton','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable','TotalsNoPostedProgressive','TotalsDebitTotProgressive','TotalsCreditTotProgressive','DataJEPrevStrStatusOperation','DataJEPrevTemplate','DataJEPrevPostDate','DataJEPrevAccrualDate','DataJEPrevAccrualDateTAX','DataJEPrevNoReference','DataJEPrevDocDate','DataJEPrevDocNo','DataJEPrevDocTot','DataJEPrevLogNo'],'__JE':['TotalAmount','Currency','FixingDate','Fixing','Fixing','TotalAmountDocCurr','Currency','FixingDate','Fixing','Fixing','TotalAmountDocCurr'],'HKLCurrenciesCurrObj':['Description','Description'],'ForecastData':['Automatic','Simulation','SimulationDate','Notes','FinalPosting','FinalExpectedDate','FinalPosted','FinalPostingDate','Automatic','Simulation','SimulationDate','Notes','FinalPosting','FinalExpectedDate','FinalPosted','FinalPostingDate'],'GLJournal':['l_TEnhJEGLDetail_P13','l_AccountType','l_TEnhJEGLDetail_P04','l_DebitDocCurrAmount','l_CrediDocCurrAmount','l_TEnhJEGLDetail_P10','FiscalAmount','l_TEnhJEGLDetail_P13','l_AccountType','l_TEnhJEGLDetail_P04','l_DebitDocCurrAmount','l_CrediDocCurrAmount','l_TEnhJEGLDetail_P10','FiscalAmount'],'HKLAdjAccount':['Description','Description'],'HKLTaxJournal':['Description'],'TaxSummary':['TaxCode','TaxableAmountDocCurr','TaxAmountDocCurr','TotalAmountDocCurr','UndeductibleAmountDocCurr','Notes','TaxCode','TaxableAmountDocCurr','TaxableAmount','TaxAmount','AdditionalTaxAmount','TotalAmount','UndeductibleAmount','NotInReverseCharge','Notes','FiscalTaxableAmt','FiscalTaxAmt','FiscalTotalAmt','FiscalUndeductible','Notes','FiscalTaxableAmt','FiscalTaxAmt','FiscalTotalAmt'],'HKLTaxCode':['Description','Description'],'HKLBlackListCustSupp':['CompanyName'],'EUAnnotations':['TaxJournal','DocNo','Currency','Currency','Fixing','TotalAmountDocCurr'],'HKLJournalIntrastatTax':['Description','Description'],'HKLCurrDescriptionIntra':['Description'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_JEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_JEComponent, resolver);
    }
} 