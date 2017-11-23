import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STORAGE_CONFIGURATION_WIZARDService } from './IDD_STORAGE_CONFIGURATION_WIZARD.service';

@Component({
    selector: 'tb-IDD_STORAGE_CONFIGURATION_WIZARD',
    templateUrl: './IDD_STORAGE_CONFIGURATION_WIZARD.component.html',
    providers: [IDD_STORAGE_CONFIGURATION_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_STORAGE_CONFIGURATION_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STORAGE_CONFIGURATION_WIZARDService,
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
		boService.appendToModelStructure({'global':['Storage','ActivationDate','TwoStepsPutaway','UniqueBinManagement','ConsignmentStock','Int_In','Int_In_Descri','Int_Out','Int_Out_Descri','Int_Diff','Int_Diff_Descri','StockZone','StockZoneDescri','GRZone','GRZoneDescri','GIZone','GIZoneDescri','ReturnZone','ReturnZoneDescri','ScrapZone','ScrapZoneDescri','InspectionZone','InspectionZoneDescri','CrossDocking','CrossDockingDescri','ManStorageConfigIntMan_In','ManStorageConfigIntMan_In_Descri','ManStorageConfigIntMan_Out','ManStorageConfigIntMan_Out_Descri','ManStorageConfigIntMan_Diff','ManStorageConfigIntMan_Diff_Descri','ManStorageConfigMIPickingZone','ManStorageConfigMIPickingZoneDescri','ManStorageConfigManActivationDate','ManStorageConfigManTwoStepsPutaway','ManStorageConfigMRZone','ManStorageConfigMRZoneDescri','ManStorageConfigMIZone','ManStorageConfigMIZoneDescri']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STORAGE_CONFIGURATION_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STORAGE_CONFIGURATION_WIZARDComponent, resolver);
    }
} 