import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASEDOC_MAINTENANCEService } from './IDD_PURCHASEDOC_MAINTENANCE.service';

@Component({
    selector: 'tb-IDD_PURCHASEDOC_MAINTENANCE',
    templateUrl: './IDD_PURCHASEDOC_MAINTENANCE.component.html',
    providers: [IDD_PURCHASEDOC_MAINTENANCEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASEDOC_MAINTENANCEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PURCHASEDOC_MAINT_DOCTYPE_itemSource: any;

    constructor(document: IDD_PURCHASEDOC_MAINTENANCEService,
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
        this.IDC_PURCHASEDOC_MAINT_DOCTYPE_itemSource = {
  "name": "PurchasesDocEnumCombo",
  "namespace": "ERP.Purchases.Services.PurchasesDocEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'PurcDocMaintenance':['DocumentType','DocNo','DocumentDate','PostingDate','StubBook','Supplier'],'HKLCustSupp':['CompNameComplete'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASEDOC_MAINTENANCEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASEDOC_MAINTENANCEComponent, resolver);
    }
} 