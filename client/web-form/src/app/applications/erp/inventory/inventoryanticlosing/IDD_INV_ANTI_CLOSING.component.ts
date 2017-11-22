﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INV_ANTI_CLOSINGService } from './IDD_INV_ANTI_CLOSING.service';

@Component({
    selector: 'tb-IDD_INV_ANTI_CLOSING',
    templateUrl: './IDD_INV_ANTI_CLOSING.component.html',
    providers: [IDD_INV_ANTI_CLOSINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INV_ANTI_CLOSINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_INV_ANTI_CLOSINGService,
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
		boService.appendToModelStructure({'global':['nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INV_ANTI_CLOSINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INV_ANTI_CLOSINGComponent, resolver);
    }
} 