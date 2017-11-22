import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MODIFY_LOTSService } from './IDD_MODIFY_LOTS.service';

@Component({
    selector: 'tb-IDD_MODIFY_LOTS',
    templateUrl: './IDD_MODIFY_LOTS.component.html',
    providers: [IDD_MODIFY_LOTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MODIFY_LOTSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_MODIFY_LOTS_CODETYPE_SPEC_itemSource: any;

    constructor(document: IDD_MODIFY_LOTSService,
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
        this.IDC_MODIFY_LOTS_CODETYPE_SPEC_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ProductionLotEditDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ProductionLotEditDetail':['BmpProperty','Component','Variant','ComponentNature','UoM','NeededQty','PickedQuantity','Storage','SpecificatorType','Specificator','Lot'],'HKLItem':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MODIFY_LOTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MODIFY_LOTSComponent, resolver);
    }
} 