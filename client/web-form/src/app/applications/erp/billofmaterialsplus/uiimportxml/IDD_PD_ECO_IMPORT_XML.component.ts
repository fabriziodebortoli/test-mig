﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_ECO_IMPORT_XMLService } from './IDD_PD_ECO_IMPORT_XML.service';

@Component({
    selector: 'tb-IDD_PD_ECO_IMPORT_XML',
    templateUrl: './IDD_PD_ECO_IMPORT_XML.component.html',
    providers: [IDD_PD_ECO_IMPORT_XMLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PD_ECO_IMPORT_XMLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PD_ECO_IMPORT_XMLService,
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
        
        		this.bo.appendToModelStructure({'global':['ImportFileName','LogFileName','bParam']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_ECO_IMPORT_XMLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_ECO_IMPORT_XMLComponent, resolver);
    }
} 