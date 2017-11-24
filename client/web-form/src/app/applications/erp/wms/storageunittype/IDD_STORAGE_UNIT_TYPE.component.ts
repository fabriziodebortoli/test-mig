import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_STORAGE_UNIT_TYPEService } from './IDD_STORAGE_UNIT_TYPE.service';

@Component({
    selector: 'tb-IDD_STORAGE_UNIT_TYPE',
    templateUrl: './IDD_STORAGE_UNIT_TYPE.component.html',
    providers: [IDD_STORAGE_UNIT_TYPEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_STORAGE_UNIT_TYPEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_STORAGE_UNIT_TYPEService,
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
        
        		this.bo.appendToModelStructure({'DBTStorageUnitType':['SUTCode','Disabled','Description','Category','MixedItems','MaximumWeight','MaximumVolume'],'HKLWMCategory':['Description'],'global':['DBTSUTPackaging','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_STORAGE_UNIT_TYPEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_STORAGE_UNIT_TYPEComponent, resolver);
    }
} 