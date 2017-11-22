import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INVENTRY_LOAD_UNLOADService } from './IDD_INVENTRY_LOAD_UNLOAD.service';

@Component({
    selector: 'tb-IDD_INVENTRY_LOAD_UNLOAD',
    templateUrl: './IDD_INVENTRY_LOAD_UNLOAD.component.html',
    providers: [IDD_INVENTRY_LOAD_UNLOADService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INVENTRY_LOAD_UNLOADComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_INVENTRYFROMLOADUNLOADTO_LOAD_SPEC_TYPE_itemSource: any;
public IDC_INVENTRYFROMLOADUNLOADTO_PROPOSED_LOAD_UNIT_VALUE_itemSource: any;
public IDC_INVENTRYFROMLOADUNLOADTO_UNLOAD_SPEC_TYPE_itemSource: any;
public IDC_INVENTRYFROMLOADUNLOADTO_PROPOSED_UNLOAD_UNIT_VALUE_itemSource: any;

    constructor(document: IDD_INVENTRY_LOAD_UNLOADService,
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
        this.IDC_INVENTRYFROMLOADUNLOADTO_LOAD_SPEC_TYPE_itemSource = {
  "name": "LoadSpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_INVENTRYFROMLOADUNLOADTO_PROPOSED_LOAD_UNIT_VALUE_itemSource = {
  "name": "ProposedValueLoadCombo",
  "namespace": "ERP.Inventory.Components.ProposedValueEnumCombo"
}; 
this.IDC_INVENTRYFROMLOADUNLOADTO_UNLOAD_SPEC_TYPE_itemSource = {
  "name": "UnloadSpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_INVENTRYFROMLOADUNLOADTO_PROPOSED_UNLOAD_UNIT_VALUE_itemSource = {
  "name": "ProposedValueUnloadCombo",
  "namespace": "ERP.Inventory.Components.ProposedValueEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Storage','bAll','bLoad','bUnload','ResourceFilter_All','ResourceFilter_Sel','ResourceFilter','bAllDate','bSelDate','FromDate','ToDate','LoadInvRsn','LoadStorage','LoadSpecificatorType','LoadSpecificator','ProposedValueLoad','UnloadInvRsn','UnloadStorage','UnloadSpecificatorType','UnloadSpecificator','ProposedValueUnload','OperationDate','InvEntryFromLoadUnloadTO'],'HKLWorkersFilter':['NameComplete'],'InvEntryFromLoadUnloadTO':['InvEntryFr_IsSelected','MovementType','ID','ToNumber','Item','ItemDescription','Lot','Storage','UoM','QtyMoved','DifferenceBmp','InvEntryFr_ProposedValue','InvEntryFr_Zone','InvEntryFr_Bin','InvEntryFr_StorageUnit','PackingUnit','ConfirmationDate','SourceSpecialStock','SourceSpecialStockCode','DestSpecialStock','DestSpecialStockCode','ToResource','WorkerDescription','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INVENTRY_LOAD_UNLOADFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INVENTRY_LOAD_UNLOADComponent, resolver);
    }
} 