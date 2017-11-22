import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DIRECTDEBIT_SEPAService } from './IDD_DIRECTDEBIT_SEPA.service';

@Component({
    selector: 'tb-IDD_DIRECTDEBIT_SEPA',
    templateUrl: './IDD_DIRECTDEBIT_SEPA.component.html',
    providers: [IDD_DIRECTDEBIT_SEPAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DIRECTDEBIT_SEPAComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SEPA_DD_SDD_TYPE_itemSource: any;

    constructor(document: IDD_DIRECTDEBIT_SEPAService,
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
        this.IDC_SEPA_DD_SDD_TYPE_itemSource = {
  "name": "TypeEnumComboSDD",
  "namespace": "ERP.AP_AR.Components.TypeEnumComboSDD"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BillsType','AllSel1','SlipSel','SlipNo','AllSel','NoSel','FromNo','ToNo','IgnorePrinted','PresDate','PresBank','DefPrint','ExecutionDate','GenerateSlipByMandTypeDueDate','nCurrentElement','GaugeDescription'],'HKLBank':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DIRECTDEBIT_SEPAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DIRECTDEBIT_SEPAComponent, resolver);
    }
} 