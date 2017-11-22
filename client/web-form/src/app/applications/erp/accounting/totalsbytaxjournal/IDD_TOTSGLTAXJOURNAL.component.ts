import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TOTSGLTAXJOURNALService } from './IDD_TOTSGLTAXJOURNAL.service';

@Component({
    selector: 'tb-IDD_TOTSGLTAXJOURNAL',
    templateUrl: './IDD_TOTSGLTAXJOURNAL.component.html',
    providers: [IDD_TOTSGLTAXJOURNALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TOTSGLTAXJOURNALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TOTSGLTAXJOURNALService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'TaxJournalTotals':['TaxJournal','BalanceYear','BalanceMonth','LastPrintingDate','DefinitivelyPrinted','Updated','NoOfPrintedLines','LastPage','TotalAmount','TaxAmount','UndeductibleAmount'],'HKLTaxJournals':['Description'],'global':['Currency','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TOTSGLTAXJOURNALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TOTSGLTAXJOURNALComponent, resolver);
    }
} 