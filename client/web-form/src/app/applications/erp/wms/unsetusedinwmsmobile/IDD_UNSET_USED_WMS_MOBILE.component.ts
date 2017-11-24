import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_UNSET_USED_WMS_MOBILEService } from './IDD_UNSET_USED_WMS_MOBILE.service';

@Component({
    selector: 'tb-IDD_UNSET_USED_WMS_MOBILE',
    templateUrl: './IDD_UNSET_USED_WMS_MOBILE.component.html',
    providers: [IDD_UNSET_USED_WMS_MOBILEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_UNSET_USED_WMS_MOBILEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_UNSET_USED_WMS_MOBILEService,
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
        
        		this.bo.appendToModelStructure({'global':['bNotPosted','bPosted','FromDate','bWMSStorages','bWMSStoragesFrom','WMSStoragesFromDate','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','ItemsDeleting','LegendPicked','LegendNotPicked'],'ItemsDeleting':['ItemsDelet_Selected','ItemsDelet_InEntriesBmp','Disabled','IsGood','Item','Description','BaseUoM','ItemsDelet_LastEntryDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_UNSET_USED_WMS_MOBILEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_UNSET_USED_WMS_MOBILEComponent, resolver);
    }
} 