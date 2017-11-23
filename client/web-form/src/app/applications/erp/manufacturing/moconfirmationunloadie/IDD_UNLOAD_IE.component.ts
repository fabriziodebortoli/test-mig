import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_UNLOAD_IEService } from './IDD_UNLOAD_IE.service';

@Component({
    selector: 'tb-IDD_UNLOAD_IE',
    templateUrl: './IDD_UNLOAD_IE.component.html',
    providers: [IDD_UNLOAD_IEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_UNLOAD_IEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_UNLOAD_IE_SPECIFICATOR_TYPE_BE_itemSource: any;

    constructor(document: IDD_UNLOAD_IEService,
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
        this.IDC_UNLOAD_IE_SPECIFICATOR_TYPE_BE_itemSource = {
  "name": "SpecificatorTypeIECombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['RefNumber','Comp','Variant','PickedQuantity','NeededQty','UnloadedQty','AdjustmentQty','FinalQty','AvailabilityQty','Storage','SpecificatorType','Specificator','Lot'],'HKLItem':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_UNLOAD_IEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_UNLOAD_IEComponent, resolver);
    }
} 