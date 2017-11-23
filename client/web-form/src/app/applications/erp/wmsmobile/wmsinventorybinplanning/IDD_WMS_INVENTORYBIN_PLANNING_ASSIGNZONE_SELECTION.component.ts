import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONService } from './IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTION.service';

@Component({
    selector: 'tb-IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTION',
    templateUrl: './IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTION.component.html',
    providers: [IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllZone','bSelZone','FromZone','ToZone','bOnlyZoneNotInventoriedFrom','ZoneNotInventoriedFromDate','bAllBin','bSelBin','FromBin','ToBin','bOnlySuspectBin','bOnlyBinNotInventoriedFrom','BinNotInventoriedFromDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONComponent, resolver);
    }
} 