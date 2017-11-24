﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_UPDATE_ITEMS_FISCAL_VALUESService } from './IDD_BR_UPDATE_ITEMS_FISCAL_VALUES.service';

@Component({
    selector: 'tb-IDD_BR_UPDATE_ITEMS_FISCAL_VALUES',
    templateUrl: './IDD_BR_UPDATE_ITEMS_FISCAL_VALUES.component.html',
    providers: [IDD_BR_UPDATE_ITEMS_FISCAL_VALUESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_UPDATE_ITEMS_FISCAL_VALUESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_UPDATE_ITEMS_FISCAL_VALUESService,
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
        
        		this.bo.appendToModelStructure({'global':['nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_UPDATE_ITEMS_FISCAL_VALUESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_UPDATE_ITEMS_FISCAL_VALUESComponent, resolver);
    }
} 