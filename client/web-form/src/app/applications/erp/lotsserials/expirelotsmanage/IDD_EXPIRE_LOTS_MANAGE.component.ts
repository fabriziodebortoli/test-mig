import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EXPIRE_LOTS_MANAGEService } from './IDD_EXPIRE_LOTS_MANAGE.service';

@Component({
    selector: 'tb-IDD_EXPIRE_LOTS_MANAGE',
    templateUrl: './IDD_EXPIRE_LOTS_MANAGE.component.html',
    providers: [IDD_EXPIRE_LOTS_MANAGEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_EXPIRE_LOTS_MANAGEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_EXPIRE_LOTS_MANAGE_TYPE_SPECIFICATOR_itemSource: any;
public IDC_EXPIRE_LOTS_MANAGE_TYPE_SPECIFICATOR_FINAL_itemSource: any;
public IDC_EXPIRE_LOTS_MANAGE_TYPE_SPECIFICATOR_PHASE2_itemSource: any;

    constructor(document: IDD_EXPIRE_LOTS_MANAGEService,
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
        this.IDC_EXPIRE_LOTS_MANAGE_TYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_EXPIRE_LOTS_MANAGE_TYPE_SPECIFICATOR_FINAL_itemSource = {
  "name": "Specificator1TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 
this.IDC_EXPIRE_LOTS_MANAGE_TYPE_SPECIFICATOR_PHASE2_itemSource = {
  "name": "Specificator2TypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        		this.bo.appendToModelStructure({'global':['Storage','SpecificatorType','Specificator','RefDate','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','InvRsn','OperationDate','Storage1','Specificator1Type','Specificator1','Storage2','Specificator2Type','Specificator2','ExpireLotsManage'],'ExpireLotsManage':['ExpireLots_IsSelected','Storage','SpecificatorType','Specificator','Item','Lot','DueDate','BookInv'],'HKLItem':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EXPIRE_LOTS_MANAGEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EXPIRE_LOTS_MANAGEComponent, resolver);
    }
} 