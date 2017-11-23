import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WMS_INVENTORYBIN_PLANNING_FRAMEService } from './IDD_WMS_INVENTORYBIN_PLANNING_FRAME.service';

@Component({
    selector: 'tb-IDD_WMS_INVENTORYBIN_PLANNING_FRAME',
    templateUrl: './IDD_WMS_INVENTORYBIN_PLANNING_FRAME.component.html',
    providers: [IDD_WMS_INVENTORYBIN_PLANNING_FRAMEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WMS_INVENTORYBIN_PLANNING_FRAMEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WMS_INVENTORYBIN_PLANNING_FRAMEService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bNewInventory','bEditInventory','Number','InventoryDescription','Date','Storage','ExpandBmp','Selected','TreeDescri','Descri','BinZoneDescri','LastInventory','HandheldStatusBmp','HandheldStatusOtherCountBmp','Selected','TreeDescri','Descri','BinZoneDescri','Team','Worker','TeamDescription','FullName','BinZoneDescriOtherCount','TeamOtherCount','WorkerOtherCount','TeamOtherCountDescription','FullNameOtherCount'],'HKLStorage':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WMS_INVENTORYBIN_PLANNING_FRAMEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WMS_INVENTORYBIN_PLANNING_FRAMEComponent, resolver);
    }
} 