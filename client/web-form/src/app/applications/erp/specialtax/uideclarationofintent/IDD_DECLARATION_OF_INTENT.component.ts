import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DECLARATION_OF_INTENTService } from './IDD_DECLARATION_OF_INTENT.service';

@Component({
    selector: 'tb-IDD_DECLARATION_OF_INTENT',
    templateUrl: './IDD_DECLARATION_OF_INTENT.component.html',
    providers: [IDD_DECLARATION_OF_INTENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DECLARATION_OF_INTENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DECLARATION_OF_INTENTService,
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
		boService.appendToModelStructure({'DeclarationOfIntent':['DeclDate','DeclYear','LogNo','CustSupp','CustomerDate','CustomerNo','DeclType','LimitAmount','LetterNotes','FromDate','ToDate','AnnulmentDate','Notes','Printed','PrintDate','PrintedOnFile','PrintFileDate','TelProtocol','DocProtocol','PrintedLetter','PrintLetterDate','PrintedAnnulment','PrintAnnulmentDate'],'HKLCustSupp':['CompNameCompleteWithTaxNumber'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DECLARATION_OF_INTENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DECLARATION_OF_INTENTComponent, resolver);
    }
} 