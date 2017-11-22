import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASESDOC_DEL_WIZARDService } from './IDD_PURCHASESDOC_DEL_WIZARD.service';

@Component({
    selector: 'tb-IDD_PURCHASESDOC_DEL_WIZARD',
    templateUrl: './IDD_PURCHASESDOC_DEL_WIZARD.component.html',
    providers: [IDD_PURCHASESDOC_DEL_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHASESDOC_DEL_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_DELETE_PURCHASESDOC_DOCTYPE_itemSource: any;

    constructor(document: IDD_PURCHASESDOC_DEL_WIZARDService,
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
        this.IDC_DELETE_PURCHASESDOC_DOCTYPE_itemSource = {
  "name": "PurchaseDocTypeForPurchDocDeletingCombo",
  "namespace": "ERP.Purchases.Services.PurchaseDocTypeForPurchDocDeletingCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['DocumentType','DeleterStartingDate','DeleterEndingDate','DeleterAllDocNo','DeleterDocNoSel','DeleterStartNoDoc','DeleterEndNoDoc','DeleterAllSuppliers','DeleterSelectSupplier','DeleterSupplierFrom','DeleterSupplierTo','DeleterInvRsn','DeleterConformingStorage1','DeleterJournal','DeleterAllPrinted','DeleterNoPrinted','DeleterPrinted','DeleterAllInv','DeleterNotInInv','DeleterInInv','DeleterAllAccounting','DeleterNotInAccounting','DeleterInAccounting','DeleterAllIssue','DeleterNotIssued','DeleterIssued','DeleterSummarizedAll','DeleterNotSummarized','DeleterSummarized','nCurrentElement']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASESDOC_DEL_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASESDOC_DEL_WIZARDComponent, resolver);
    }
} 