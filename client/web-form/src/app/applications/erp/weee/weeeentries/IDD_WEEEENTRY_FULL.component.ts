import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WEEEENTRY_FULLService } from './IDD_WEEEENTRY_FULL.service';

@Component({
    selector: 'tb-IDD_WEEEENTRY_FULL',
    templateUrl: './IDD_WEEEENTRY_FULL.component.html',
    providers: [IDD_WEEEENTRY_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WEEEENTRY_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_WEEEENTRY_DOCTYPE_itemSource: any;

    constructor(document: IDD_WEEEENTRY_FULLService,
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
        this.IDC_WEEEENTRY_DOCTYPE_itemSource = {
  "name": "DocumentTypeCombo",
  "namespace": "ERP.WEEE.Documents.DocTypeWEEEEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'WEEEEntries':['EntryDate','Customer','Item','Category','CombinedNomenclature','PRODCOM','Qty','TotalContributionAmount','DocumentType','DocumentLine','DocumentNumber','DocumentDate'],'HKLCustomer':['CompNameComplete'],'HKLItem':['Description'],'HKLWEEECtg':['Description'],'global':['UnitaryContributionAmount','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WEEEENTRY_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WEEEENTRY_FULLComponent, resolver);
    }
} 