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
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Storage','Zone','Bin','Item','bOnlyStocks','StorageUnit','NewStorageUnit','NewSUTCode','NewZone','NewBin','DBTPackingDetail','LegendStorage','LegendStockWithoutSU','LegendStockWithSU','LegendSU'],'DBTPackingDetail':['PackingDet_FieldName','PackingDet_FieldKey','PackingDet_FieldDescription','PackingDet_FieldValue']});

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