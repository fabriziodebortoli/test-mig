﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CHECK_ITEMS_FISCAL_YEARService } from './IDD_CHECK_ITEMS_FISCAL_YEAR.service';

@Component({
    selector: 'tb-IDD_CHECK_ITEMS_FISCAL_YEAR',
    templateUrl: './IDD_CHECK_ITEMS_FISCAL_YEAR.component.html',
    providers: [IDD_CHECK_ITEMS_FISCAL_YEARService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CHECK_ITEMS_FISCAL_YEARComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CHECK_ITEMS_FISCAL_YEARService,
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
        
        		this.bo.appendToModelStructure({'global':['FiscalYear','bAllItems','bItemSel','FromItem','ToItem','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CHECK_ITEMS_FISCAL_YEARFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CHECK_ITEMS_FISCAL_YEARComponent, resolver);
    }
} 