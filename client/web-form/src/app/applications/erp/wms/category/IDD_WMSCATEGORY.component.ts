import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WMSCATEGORYService } from './IDD_WMSCATEGORY.service';

@Component({
    selector: 'tb-IDD_WMSCATEGORY',
    templateUrl: './IDD_WMSCATEGORY.component.html',
    providers: [IDD_WMSCATEGORYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WMSCATEGORYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WMSCATEGORYService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'DBTCategory':['Category','Description'],'global':['DBTCategoryDetails','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'DBTCategoryDetails':['Storage','SearchZoneStrategyPutaway','SearchZoneStrategyPicking','StockReturnStrategy'],'HKLStorage':['Description'],'HKLSearchZonePutaway':['Description'],'HKLSearchZonePicking':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WMSCATEGORYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WMSCATEGORYComponent, resolver);
    }
} 