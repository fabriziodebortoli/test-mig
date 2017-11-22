import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EDIT_VALUE_TYPEService } from './IDD_EDIT_VALUE_TYPE.service';

@Component({
    selector: 'tb-IDD_EDIT_VALUE_TYPE',
    templateUrl: './IDD_EDIT_VALUE_TYPE.component.html',
    providers: [IDD_EDIT_VALUE_TYPEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EDIT_VALUE_TYPEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MOD_VALUE_TYPE_NEW_VALUE_TYPE_itemSource: any;

    constructor(document: IDD_EDIT_VALUE_TYPEService,
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
        this.IDC_MOD_VALUE_TYPE_NEW_VALUE_TYPE_itemSource = {
  "name": "ValuationInventoryCombo",
  "namespace": "itemsource.erp.company.components.valuationinventorycombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['HFItems_All','HFItems_Range','HFItems_From','HFItems_To','ValueType','bEvaluateByLot','ForceValueTypeModified','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EDIT_VALUE_TYPEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EDIT_VALUE_TYPEComponent, resolver);
    }
} 