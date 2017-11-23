import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALE_DOC_MAINTENANCEService } from './IDD_SALE_DOC_MAINTENANCE.service';

@Component({
    selector: 'tb-IDD_SALE_DOC_MAINTENANCE',
    templateUrl: './IDD_SALE_DOC_MAINTENANCE.component.html',
    providers: [IDD_SALE_DOC_MAINTENANCEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SALE_DOC_MAINTENANCEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SALEDOC_MAINT_DOCTYPE_itemSource: any;

    constructor(document: IDD_SALE_DOC_MAINTENANCEService,
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
        this.IDC_SALEDOC_MAINT_DOCTYPE_itemSource = {
  "name": "DocumentTypeCombo",
  "namespace": "ERP.Sales.Services.SalesDocEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'SalseDocMaintenance':['DocumentType','DocNo','DocumentDate','PostingDate','StubBook','CustSuppType','CustSupp'],'HKLCustSupp':['CompNameComplete'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALE_DOC_MAINTENANCEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALE_DOC_MAINTENANCEComponent, resolver);
    }
} 