import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INPROC_RAW_MATService } from './IDD_INPROC_RAW_MAT.service';

@Component({
    selector: 'tb-IDD_INPROC_RAW_MAT',
    templateUrl: './IDD_INPROC_RAW_MAT.component.html',
    providers: [IDD_INPROC_RAW_MATService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_INPROC_RAW_MATComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INPROC_RAW_MATService,
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
        
        		this.bo.appendToModelStructure({'global':['bAllItem','bSelItem','FromItem','ToItem','bAllWC','bSelWC','FromWC','ToWC','bInHouseWC','bOutsrcWC','bShowZeroRows','bShowZeroColumns','bAllSupp','bSelSupp','FromSupp','ToSupp','bShowZeroRows','bShowZeroColumns','DBTInProcessingRawMaterials'],'DBTInProcessingRawMaterials':['Comp','UoM'],'HKLItem':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INPROC_RAW_MATFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INPROC_RAW_MATComponent, resolver);
    }
} 