import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MANUAL_ASSIGNMENTService } from './IDD_MANUAL_ASSIGNMENT.service';

@Component({
    selector: 'tb-IDD_MANUAL_ASSIGNMENT',
    templateUrl: './IDD_MANUAL_ASSIGNMENT.component.html',
    providers: [IDD_MANUAL_ASSIGNMENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MANUAL_ASSIGNMENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MANUAL_ASSIGNMENTService,
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
		boService.appendToModelStructure({'global':['DBTBinStocksManualAssign'],'DBTBinStocksManualAssign':['BinStocksM_Selected','Zone','BinStocksM_ZoneDesctiption','Bin','StockNumber','Item','BinStocksM_ItemDescription','Lot','InternalIdNo','UnitOfMeasure','BinStocksM_QtyOnHand','StorageUnit','BinStocksM_SUTypeDescr','BinStocksM_NewStorage','BinStocksM_NewZone','BinStocksM_NewZoneDescri','BinStocksM_NewBin','ConsignmentPartner']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MANUAL_ASSIGNMENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MANUAL_ASSIGNMENTComponent, resolver);
    }
} 