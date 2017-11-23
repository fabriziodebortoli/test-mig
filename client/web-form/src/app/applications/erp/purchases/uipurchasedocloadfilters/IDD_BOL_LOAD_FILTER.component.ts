import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOL_LOAD_FILTERService } from './IDD_BOL_LOAD_FILTER.service';

@Component({
    selector: 'tb-IDD_BOL_LOAD_FILTER',
    templateUrl: './IDD_BOL_LOAD_FILTER.component.html',
    providers: [IDD_BOL_LOAD_FILTERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BOL_LOAD_FILTERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOL_LOAD_FILTERService,
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
        
        		this.bo.appendToModelStructure({'global':['SuppDocNoFilter','BoLNoFilter','BoLLineFilter','ExcludeFulffilledLines','ExcludeNotes','ItemFilter','ShowOnlyItemLine']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOL_LOAD_FILTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOL_LOAD_FILTERComponent, resolver);
    }
} 