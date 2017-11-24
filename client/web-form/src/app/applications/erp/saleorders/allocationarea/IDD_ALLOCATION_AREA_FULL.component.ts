import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ALLOCATION_AREA_FULLService } from './IDD_ALLOCATION_AREA_FULL.service';

@Component({
    selector: 'tb-IDD_ALLOCATION_AREA_FULL',
    templateUrl: './IDD_ALLOCATION_AREA_FULL.component.html',
    providers: [IDD_ALLOCATION_AREA_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ALLOCATION_AREA_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ALLOCATION_AREA_FULLService,
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
        
        		this.bo.appendToModelStructure({'AllocationArea':['AllocationArea','Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ALLOCATION_AREA_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ALLOCATION_AREA_FULLComponent, resolver);
    }
} 