import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAXDOCUMENTSSETUPService } from './IDD_TAXDOCUMENTSSETUP.service';

@Component({
    selector: 'tb-IDD_TAXDOCUMENTSSETUP',
    templateUrl: './IDD_TAXDOCUMENTSSETUP.component.html',
    providers: [IDD_TAXDOCUMENTSSETUPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAXDOCUMENTSSETUPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAXDOCUMENTSSETUPService,
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
		boService.appendToModelStructure({'global':['FromDate','ToDate','Type','bAllTaxJournal','bSelTaxJournal','FromTaxJournal','ToTaxJournal','bAllDocuments','bSelDocuments','FromDocuments','ToDocuments','bAllLogNo','bSelLogNo','FromLogNo','ToLogNo','ProcessStatus'],'HKLTaxJournalFrom':['Description'],'HKLTaxJournalTo':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAXDOCUMENTSSETUPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAXDOCUMENTSSETUPComponent, resolver);
    }
} 