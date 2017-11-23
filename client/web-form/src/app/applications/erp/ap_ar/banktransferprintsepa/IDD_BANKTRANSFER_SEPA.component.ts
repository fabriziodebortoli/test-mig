import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BANKTRANSFER_SEPAService } from './IDD_BANKTRANSFER_SEPA.service';

@Component({
    selector: 'tb-IDD_BANKTRANSFER_SEPA',
    templateUrl: './IDD_BANKTRANSFER_SEPA.component.html',
    providers: [IDD_BANKTRANSFER_SEPAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BANKTRANSFER_SEPAComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SEPA_BANKTRANSFERTYPE_itemSource: any;

    constructor(document: IDD_BANKTRANSFER_SEPAService,
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
        this.IDC_SEPA_BANKTRANSFERTYPE_itemSource = {
  "name": "EnumComboCH",
  "namespace": "ERP.AP_AR.Documents.EnumComboCH"
}; 
this.IDC_SEPA_BANKTRANSFERTYPE_itemSource = {
  "name": "EnumComboForeignSCT",
  "namespace": "ERP.AP_AR.Documents.EnumComboForeignSCT"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AllType','OneType','BankTransferType','AllSel1','SlipSel','SlipNo','BankTransferType','AllSel1','SlipSel','SlipNo','AllSel','NoSel','FromNo','ToNo','IgnorePrinted','IssueDate','IssueBank','DefPrint','ExecutionDate','GenerateSlipByDueDate','Urgency','Success','Transmit'],'HKLBank':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BANKTRANSFER_SEPAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BANKTRANSFER_SEPAComponent, resolver);
    }
} 