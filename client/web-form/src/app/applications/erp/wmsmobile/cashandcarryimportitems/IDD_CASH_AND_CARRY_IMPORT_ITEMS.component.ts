import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CASH_AND_CARRY_IMPORT_ITEMSService } from './IDD_CASH_AND_CARRY_IMPORT_ITEMS.service';

@Component({
    selector: 'tb-IDD_CASH_AND_CARRY_IMPORT_ITEMS',
    templateUrl: './IDD_CASH_AND_CARRY_IMPORT_ITEMS.component.html',
    providers: [IDD_CASH_AND_CARRY_IMPORT_ITEMSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CASH_AND_CARRY_IMPORT_ITEMSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CASH_AND_CARRY_IMPORT_ITEMS_DEVICE_itemSource: any;

    constructor(document: IDD_CASH_AND_CARRY_IMPORT_ITEMSService,
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
        this.IDC_CASH_AND_CARRY_IMPORT_ITEMS_DEVICE_itemSource = {
  "name": "WorkerCombo",
  "namespace": "ERP.WMSMobile.Components.WorkerItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CashAndCarryImportItems_sWorker']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CASH_AND_CARRY_IMPORT_ITEMSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CASH_AND_CARRY_IMPORT_ITEMSComponent, resolver);
    }
} 