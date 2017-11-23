import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXJOURNALSService } from './IDD_TAXJOURNALS.service';

@Component({
    selector: 'tb-IDD_TAXJOURNALS',
    templateUrl: './IDD_TAXJOURNALS.component.html',
    providers: [IDD_TAXJOURNALSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXJOURNALSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXJOURNALSService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Year','Month','MonthTo','bSelWithinDate','WithinDate','TaxJournalType','AllJournalRadio','JournalRadioSel','FromTaxJournal','FromTaxJournal','ToTaxJournal','DisabledFilter','MarginTAX','FromPostDate','bSelFromDate','FromDate','PrevYearCredit','LastTaxPymt','DefinitivelyPrinted','bPrepareForSOS','TotForDoc','DistributionPosting','ClosedPeriod','DotMatrixPrinter','DotMatrixPrinter80Col','ContextualHeading','NoPrefix','VideoPage'],'HKLFromTaxJournal':['Description','Description'],'HKLToTaxJournal':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXJOURNALSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXJOURNALSComponent, resolver);
    }
} 