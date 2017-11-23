import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXJOURNALSSUMMARIESService } from './IDD_TAXJOURNALSSUMMARIES.service';

@Component({
    selector: 'tb-IDD_TAXJOURNALSSUMMARIES',
    templateUrl: './IDD_TAXJOURNALSSUMMARIES.component.html',
    providers: [IDD_TAXJOURNALSSUMMARIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXJOURNALSSUMMARIESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXJOURNALSSUMMARIESService,
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
        
        		this.bo.appendToModelStructure({'TaxSummaryJournal':['TaxJournal','TaxCode','IntrastatTax','BalanceYear','BalanceMonth','IsManual','TotalAmount','TaxableAmount','TaxAmount','UndeductibleAmount','AdditionalTaxAmount'],'HKLTaxJournals':['Description'],'HKLTAX':['Description'],'global':['Currency','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXJOURNALSSUMMARIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXJOURNALSSUMMARIESComponent, resolver);
    }
} 