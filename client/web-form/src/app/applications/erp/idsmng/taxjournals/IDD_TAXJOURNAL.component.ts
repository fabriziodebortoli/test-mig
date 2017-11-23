import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXJOURNALService } from './IDD_TAXJOURNAL.service';

@Component({
    selector: 'tb-IDD_TAXJOURNAL',
    templateUrl: './IDD_TAXJOURNAL.component.html',
    providers: [IDD_TAXJOURNALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TAXJOURNALComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_TAXJOURN_CODETYPETAXJOURNAL_itemSource: any;

    constructor(document: IDD_TAXJOURNALService,
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
        this.IDC_TAXJOURN_CODETYPETAXJOURNAL_itemSource = {
  "name": "TypeJournalEnumCombo",
  "namespace": "ERP.IdsMng.Documents.TypeJournalEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'TaxJournals':['TaxJournal','Disabled','Description','CodeType','Notes','ExcludedFromVAT','SpecialPrint','IsTravelAgencyJournal','InTravelAgencyCalculation','ExcludedFromProRata','EUAnnotation','MarginTAX','AutomaticNumbering','IsAProtocol','BranchNumber','D394Serie','AGOSection','OMNIASection'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXJOURNALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXJOURNALComponent, resolver);
    }
} 