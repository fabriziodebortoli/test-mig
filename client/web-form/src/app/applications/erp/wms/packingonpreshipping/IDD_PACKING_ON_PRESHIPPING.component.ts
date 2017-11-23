import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PACKING_ON_PRESHIPPINGService } from './IDD_PACKING_ON_PRESHIPPING.service';

@Component({
    selector: 'tb-IDD_PACKING_ON_PRESHIPPING',
    templateUrl: './IDD_PACKING_ON_PRESHIPPING.component.html',
    providers: [IDD_PACKING_ON_PRESHIPPINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PACKING_ON_PRESHIPPINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PACKING_ON_PRESHIPPINGService,
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
        
        		this.bo.appendToModelStructure({'global':['NewStorageUnit','NewSUTCode','DBTPackingOnPreShippingDetail','LegendStorage','LegendStockWithoutSU','LegendStockWithSU','LegendSU'],'DBTPackingOnPreShippingDetail':['PackingOnP_FieldName','PackingOnP_FieldKey','PackingOnP_FieldDescription','PackingOnP_FieldValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PACKING_ON_PRESHIPPINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PACKING_ON_PRESHIPPINGComponent, resolver);
    }
} 