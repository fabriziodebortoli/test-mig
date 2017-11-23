import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MO_MAINTENANCEService } from './IDD_MO_MAINTENANCE.service';

@Component({
    selector: 'tb-IDD_MO_MAINTENANCE',
    templateUrl: './IDD_MO_MAINTENANCE.component.html',
    providers: [IDD_MO_MAINTENANCEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MO_MAINTENANCEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MO_MAINTENANCE_OPERATION_itemSource: any;
public IDC_MO_MAINTENANCE_BE_STATUS_itemSource: any;

    constructor(document: IDD_MO_MAINTENANCEService,
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
        this.IDC_MO_MAINTENANCE_OPERATION_itemSource = {
  "name": "OperationSelectedCombo",
  "namespace": "ERP.Manufacturing.Documents.MOMaintenanceItemSource"
}; 
this.IDC_MO_MAINTENANCE_BE_STATUS_itemSource = {
  "name": "ToolStatusEnumCombo",
  "namespace": "ERP.Manufacturing.Documents.MOMaintenanceStateItemSource"
}; 

        		this.bo.appendToModelStructure({'global':['OperationSelected','bMOAll','bMOSel','MOFrom','MOTo','bItemAll','bItemSel','bItemFrom','bItemTo','bVariantAll','bVariantSel','VariantFrom','VariantTo','bJobAll','bJobSel','JobFrom','JobTo','bDeleteConfirmedMO','bDeleteInventoryEntries','bCorrectInventoryEntries','bRestoreProdPlanLine','bDeleteDocProcessExt','MOMaintenance','InHouseRel','OutRel','InHouseProc','OutProc','InHouseConf','OutConf'],'MOMaintenance':['TMO_StateBmp','TMO_Selection','MONo','MOStatus','BOM','DeliveryDate','UoM','ProductionQty','Job'],'HKLItemDetail':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MO_MAINTENANCEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MO_MAINTENANCEComponent, resolver);
    }
} 