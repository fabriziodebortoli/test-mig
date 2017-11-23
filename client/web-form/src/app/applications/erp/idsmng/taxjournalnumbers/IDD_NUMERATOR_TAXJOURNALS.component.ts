import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_NUMERATOR_TAXJOURNALSService } from './IDD_NUMERATOR_TAXJOURNALS.service';

@Component({
    selector: 'tb-IDD_NUMERATOR_TAXJOURNALS',
    templateUrl: './IDD_NUMERATOR_TAXJOURNALS.component.html',
    providers: [IDD_NUMERATOR_TAXJOURNALSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_NUMERATOR_TAXJOURNALSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_NUMERATOR_TAXJOURNALSService,
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
		boService.appendToModelStructure({'TaxJournalNumbers':['TaxJournal','LastDocDate','LastDocNo','Suffix','FirstNo','LastNo'],'HKLTaxJournals':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_NUMERATOR_TAXJOURNALSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_NUMERATOR_TAXJOURNALSComponent, resolver);
    }
} 