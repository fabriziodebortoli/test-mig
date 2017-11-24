import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BIN_GENERATIONService } from './IDD_BIN_GENERATION.service';

@Component({
    selector: 'tb-IDD_BIN_GENERATION',
    templateUrl: './IDD_BIN_GENERATION.component.html',
    providers: [IDD_BIN_GENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BIN_GENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BIN_GENERATIONService,
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
        
        		this.bo.appendToModelStructure({'global':['Storage','Zone','Section','MaxWeight','TotalCapacity','MaxStorageUnit','bForPicking','bForPutaway','BinType','BinType','GenerationBinRange','BinGenerationDetail'],'HKLBinType':['Description','Description'],'GenerationBinRange':['Position','Description','RangeStart','RangeEnd','Increment'],'BinGenerationDetail':['Selection','Bin','BarcodeSegment','ForPicking','ForPutaway','BinType','MaxWeight','TotalCapacity','MaxStorageUnit']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BIN_GENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BIN_GENERATIONComponent, resolver);
    }
} 