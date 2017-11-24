import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALESDOC_DEL_WIZARDService } from './IDD_SALESDOC_DEL_WIZARD.service';

@Component({
    selector: 'tb-IDD_SALESDOC_DEL_WIZARD',
    templateUrl: './IDD_SALESDOC_DEL_WIZARD.component.html',
    providers: [IDD_SALESDOC_DEL_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SALESDOC_DEL_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SALESDOC_DEL_DOCTYPE_itemSource: any;

    constructor(document: IDD_SALESDOC_DEL_WIZARDService,
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
        this.IDC_SALESDOC_DEL_DOCTYPE_itemSource = {
  "name": "DocumentTypeCombo",
  "namespace": "ERP.Sales.Components.SaleDocTypeForSaleDocDeletingCombo"
}; 

        		this.bo.appendToModelStructure({'global':['DocumentType','DeleterStartingDate','DeleterEndingDate','DeleterAllDocNo','DeleterDocNoSel','DeleterFromDocNo','DeleterToDocNo','DeleterAllJournal','DeleterSelJournal','DeleterFromJournal','DeleterToJournal','DeleterAllIssue','DeleterNotIssued','DeleterIssued','DeleterAllInv','DeleterNotInInv','DeleterInInv','DeleterAllAccounting','DeleterNotInAccounting','DeleterInAccounting','DeleterSummarizedAll','DeleterNotSummarized','DeleterSummarized','DeleterAllPrinted','DeleterNoPrinted','DeleterPrinted','DeleterAllGenComm','DeleterNotGenComm','DeleterGenComm','nCurrentElement']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALESDOC_DEL_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALESDOC_DEL_WIZARDComponent, resolver);
    }
} 