import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STORAGE_UNITService } from './IDD_STORAGE_UNIT.service';

@Component({
    selector: 'tb-IDD_STORAGE_UNIT',
    templateUrl: './IDD_STORAGE_UNIT.component.html',
    providers: [IDD_STORAGE_UNITService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_STORAGE_UNITComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STORAGE_UNITService,
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
        
        		this.bo.appendToModelStructure({'DBTStorageUnit':['SUNumber','SUTCode','BarcodeSegment','UsedInWarehouse','UsedInPreShipping','MaximumWeight','MaximumVolume','FixedGrossVolume','GrossWeight','GrossVolume','Dimensions'],'HKLWMSUnitType':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STORAGE_UNITFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STORAGE_UNITComponent, resolver);
    }
} 