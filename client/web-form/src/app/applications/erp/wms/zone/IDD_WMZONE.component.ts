import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WMZONEService } from './IDD_WMZONE.service';

@Component({
    selector: 'tb-IDD_WMZONE',
    templateUrl: './IDD_WMZONE.component.html',
    providers: [IDD_WMZONEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WMZONEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_WMZONE_BLOCK_REASON_itemSource: any;
public IDC_WMZONE_PICKINGSTRATEGY_itemSource: any;

    constructor(document: IDD_WMZONEService,
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
        this.IDC_WMZONE_BLOCK_REASON_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.WMS.BlockReason"
}; 
this.IDC_WMZONE_PICKINGSTRATEGY_itemSource = {
  "name": "PickingStrategyCombo",
  "namespace": "ERP.WMS.Documents.PickingStrategyWithoutLotsEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTWMZone':['Storage','Disabled','Zone','Description','StorageUnit','HazardousMaterial','ZoneBarcodePrefix','PathSequence','UseBinStructure','BinStructure','Blocked','BlockDate','BlockReason','WorkerDescription','UseSection','ForPutaway','AddToStock','PutawayStrategy','PutawayNearFixedBin','CapacityCheck','WeightCheck','StorageUnitNumberCheck','MixedItem','MixedLots','ForPicking','TotalRemoval','PickingStrategy','PickingStrategyItemWithLot','UniqueLot','ReplenishmentZone','Interim','GIZone','GRZone','RTSZone','ScrapsZone','CrossDocking','QualityInspectionZone','MIZone','MRZone','PeriodicInventory','ContinuousInventory','InventoryBinAssignment','LastInventory'],'HKLBinStructure':['Description'],'global':['DBTWMZoneSections','DBTZoneResources','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTWMZoneSections':['Section','Description','ValidForAllMAterials'],'DBTZoneResources':['Team','DefaultTeam'],'HKLTeams':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WMZONEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WMZONEComponent, resolver);
    }
} 