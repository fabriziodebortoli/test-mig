﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOMGRAPHService } from './IDD_BOMGRAPH.service';

@Component({
    selector: 'tb-IDD_BOMGRAPH',
    templateUrl: './IDD_BOMGRAPH.component.html',
    providers: [IDD_BOMGRAPHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOMGRAPHComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOMGRAPHService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['BOM','Variant','NrLevels','DateValid','ECODate','ECORevision','DBTNodeDetail'],'DBTNodeDetail':['l_FieldName','l_FieldValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOMGRAPHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOMGRAPHComponent, resolver);
    }
} 