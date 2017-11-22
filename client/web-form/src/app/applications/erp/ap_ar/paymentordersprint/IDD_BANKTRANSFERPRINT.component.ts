import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BANKTRANSFERPRINTService } from './IDD_BANKTRANSFERPRINT.service';

@Component({
    selector: 'tb-IDD_BANKTRANSFERPRINT',
    templateUrl: './IDD_BANKTRANSFERPRINT.component.html',
    providers: [IDD_BANKTRANSFERPRINTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BANKTRANSFERPRINTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PYMTORD_BANKTRANSFERTYPE_itemSource: any;

    constructor(document: IDD_BANKTRANSFERPRINTService,
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
        this.IDC_PYMTORD_BANKTRANSFERTYPE_itemSource = {
  "name": "EnumComboBankTransfer",
  "namespace": "ERP.AP_AR.Documents.EnumComboBankTransfer"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BankTransferType','AllSel1','SlipSel','SlipNo','AllSel1','SlipSel','SlipNo','AllSel','NoSel','FromNo','ToNo','IgnorePrinted','IssueDate','IssueBank','DefPrint','bPrintIBAN','bUseOneDateForForeign','bNewRecordP9','SupporttName','Transmit'],'HKLBank':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BANKTRANSFERPRINTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BANKTRANSFERPRINTComponent, resolver);
    }
} 