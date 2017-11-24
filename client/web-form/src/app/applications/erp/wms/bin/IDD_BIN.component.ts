import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BINService } from './IDD_BIN.service';

@Component({
    selector: 'tb-IDD_BIN',
    templateUrl: './IDD_BIN.component.html',
    providers: [IDD_BINService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BINComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BIN_BLOCK_REASON_itemSource: any;

    constructor(document: IDD_BINService,
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
        this.IDC_BIN_BLOCK_REASON_itemSource = {
  "name": "ItemSourceXml",
  "namespace": "ItemSource.Framework.TbGes.TbGes.ItemSourceXml",
  "parameter": "DataFile.ERP.WMS.BlockReason"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'WMBin':['Storage','Disabled','Zone','Section','Bin','ForPicking','ForPutaway','BinType','BarcodeSegment','PathSequence','MaxWeight','UsedWeight','ReservedWeight','TotalCapacity','UsedCapacity','ReservedCapacity','MaxStorageUnit','NumOfSU','ReservedNumOfSU','Blocked','BlockDate','BlockReason','BlockType','WorkerDescription','Suspect','SuspectDate','LastEntry','LastEmptying','LastInventory'],'HKLZone':['Description'],'HKLSection':['Description'],'HKLBinType':['Description'],'global':['nCurrentElement','WMBinStocks','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','LegendBinStock','LegendBinStock','LegendBinStockSU','LegendBinStockIsMultSU','dDateFrom','dDateTo'],'WMBinStocks':['BinStocksV_Status','StockNumber','IsMultilevelStorageUnit','Item','BinStocksV_Descri','Lot','InternalIdNo','LotValidTo','SpecialStock','SpecialStockCode','StorageUnit','StorageUnitType','ConsignmentPartner','UnitOfMeasure','Qty','QtyBaseUoM','QtyReserved','QtyIncoming','Weight','Capacity']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BINFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BINComponent, resolver);
    }
} 