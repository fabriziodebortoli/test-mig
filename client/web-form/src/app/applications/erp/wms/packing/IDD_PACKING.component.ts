import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PACKINGService } from './IDD_PACKING.service';

@Component({
    selector: 'tb-IDD_PACKING',
    templateUrl: './IDD_PACKING.component.html',
    providers: [IDD_PACKINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PACKINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PACKINGService,
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
        
        		this.bo.appendToModelStructure({'global':['Storage','Zone','Bin','Item','bOnlyStocks','StorageUnit','NewStorageUnit','NewSUTCode','NewZone','NewBin','DBTPackingDetail','LegendStorage','LegendStockWithoutSU','LegendStockWithSU','LegendSU'],'DBTPackingDetail':['PackingDet_FieldName','PackingDet_FieldKey','PackingDet_FieldDescription','PackingDet_FieldValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PACKINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PACKINGComponent, resolver);
    }
} 