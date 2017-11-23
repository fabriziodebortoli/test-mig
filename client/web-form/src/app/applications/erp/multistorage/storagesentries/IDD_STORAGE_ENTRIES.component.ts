import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STORAGE_ENTRIESService } from './IDD_STORAGE_ENTRIES.service';

@Component({
    selector: 'tb-IDD_STORAGE_ENTRIES',
    templateUrl: './IDD_STORAGE_ENTRIES.component.html',
    providers: [IDD_STORAGE_ENTRIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_STORAGE_ENTRIESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_STORAGEENTRIES_CODETYPE_SPECIFICATOR_itemSource: any;
public IDC_STORAGEENTRIES_PROC_CODETYPE_SPECIFICATOR2_itemSource: any;

    constructor(document: IDD_STORAGE_ENTRIESService,
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
        this.IDC_STORAGEENTRIES_CODETYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_STORAGEENTRIES_PROC_CODETYPE_SPECIFICATOR2_itemSource = {
  "name": "Specificato2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        		this.bo.appendToModelStructure({'global':['Storage','SpecificatorType','Specificator','bAllItems','bItemSel','FromItem','ToItem','Reason','OperationDate','Storage2','Specificator2Type','Specificator2','StoragesEntries'],'StoragesEntries':['StoragesEn_Selected','Item','BaseUoM','StoragesEn_Existence','StoragesEn_Qty','StoragesEn_Lot','StoragesEn_Variant','ProposedValue','StoragesEn_Annotations'],'HKLItems':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STORAGE_ENTRIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STORAGE_ENTRIESComponent, resolver);
    }
} 