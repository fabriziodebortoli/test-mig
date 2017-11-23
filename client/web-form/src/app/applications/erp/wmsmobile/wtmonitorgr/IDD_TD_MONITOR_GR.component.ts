import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TD_MONITOR_GRService } from './IDD_TD_MONITOR_GR.service';

@Component({
    selector: 'tb-IDD_TD_MONITOR_GR',
    templateUrl: './IDD_TD_MONITOR_GR.component.html',
    providers: [IDD_TD_MONITOR_GRService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_TD_MONITOR_GRComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MONITORGR_BOLNO_itemSource: any;
public IDC_MONITORGR_BOLNO_validators: any;

    constructor(document: IDD_TD_MONITOR_GRService,
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
        this.IDC_MONITORGR_BOLNO_itemSource = {
  "name": "BoLNoCombo",
  "namespace": "itemsource.erp.WMSMobile.BatchDocuments.BoLNoCombo"
}; 
this.IDC_MONITORGR_BOLNO_validators = [
  {
    "name": "BoLNoComboValidator",
    "namespace": "Validator.erp.WMSMobile.BatchDocuments.BoLNoComboValidator"
  }
]; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BoLNumber','BoLDate','Supplier','Detail'],'HKLSupplier':['CompanyName'],'Detail':['SyncStatusBmp','DestStorageUnit','Item','ItemDescription','UoM','QtyMoved','Lot','SupplierLotNo','SupplierLotExpiryDate','InternalIdNo','ItemStatus']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TD_MONITOR_GRFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TD_MONITOR_GRComponent, resolver);
    }
} 