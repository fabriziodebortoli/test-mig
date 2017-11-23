import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_ITEMSService } from './IDD_DELETE_ITEMS.service';

@Component({
    selector: 'tb-IDD_DELETE_ITEMS',
    templateUrl: './IDD_DELETE_ITEMS.component.html',
    providers: [IDD_DELETE_ITEMSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DELETE_ITEMSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_ITEMSService,
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
        
        		this.bo.appendToModelStructure({'global':['bNotPosted','bPosted','FromDate','bTypeAll','bTypeGood','bTypeService','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','bDisabled','bDeleteComponent','bLoadGridForManSel','ItemsDeleting','LegendPicked','LegendNotPicked'],'ItemsDeleting':['ItemsDelet_Selected','ItemsDelet_InEntriesBmp','Disabled','IsGood','Item','Description','BaseUoM','ItemsDelet_LastEntryDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_ITEMSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_ITEMSComponent, resolver);
    }
} 