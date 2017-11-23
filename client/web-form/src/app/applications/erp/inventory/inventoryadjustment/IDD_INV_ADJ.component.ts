import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INV_ADJService } from './IDD_INV_ADJ.service';

@Component({
    selector: 'tb-IDD_INV_ADJ',
    templateUrl: './IDD_INV_ADJ.component.html',
    providers: [IDD_INV_ADJService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_INV_ADJComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_INVENTORYCORRECTIONENTRY_CODETYPE_SPECIFICATOR_itemSource: any;
public IDC_INVENTORYCORRECTIONENTRY_PROPOSED_UNIT_VALUE_itemSource: any;
public IDC_INVENTORYCORRECTIONENTRY_PROPOSED_UNIT_VALUE_POSITIVE_itemSource: any;

    constructor(document: IDD_INV_ADJService,
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
        this.IDC_INVENTORYCORRECTIONENTRY_CODETYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_INVENTORYCORRECTIONENTRY_PROPOSED_UNIT_VALUE_itemSource = {
  "name": "ProposedValueCombo",
  "namespace": "ERP.Inventory.Components.ProposedValueEnumCombo"
}; 
this.IDC_INVENTORYCORRECTIONENTRY_PROPOSED_UNIT_VALUE_POSITIVE_itemSource = {
  "name": "ProposedValuePositiveCombo",
  "namespace": "ERP.Inventory.Components.ProposedValuePositiveEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Restore','bSaveSettings','Integrate','Overwrites','LinesSaved','LinesSaved','bOverwriteItemsAlreadyPresent','bAllLines','FromLine','ToLine','bBookInvAnalysis','bOnhandAnalysis','bExistenceToDateCheck','ExistenceToDate','Storage','SpecificatorType','Specificator','bDisplayItemsAvailable','bAllLot','bSelLot','Lot','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','OperationDate','bStartReport','NegInvRsn','ProposedValue','PosInvRsn','ProposedValuePositive','InventoryAdjustment'],'InventoryAdjustment':['IsSelected','Line','InvAdj_NotExistingItemBmp','Item','Lot','Variant','BaseUoM','PreviousQty','ActualQty','Difference','InvAdj_DifferenceBmp','ProposedValue','PreviousValue','ActualValue'],'HKLItemsBE':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INV_ADJFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INV_ADJComponent, resolver);
    }
} 