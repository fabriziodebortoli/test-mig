﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_NOTAFISCALTYPE_COPY_NFService } from './IDD_BR_NOTAFISCALTYPE_COPY_NF.service';

@Component({
    selector: 'tb-IDD_BR_NOTAFISCALTYPE_COPY_NF',
    templateUrl: './IDD_BR_NOTAFISCALTYPE_COPY_NF.component.html',
    providers: [IDD_BR_NOTAFISCALTYPE_COPY_NFService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_NOTAFISCALTYPE_COPY_NFComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_NOTAFISCALTYPE_COPY_NFService,
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
		boService.appendToModelStructure({'global':['NotaFiscalType']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_NOTAFISCALTYPE_COPY_NFFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_NOTAFISCALTYPE_COPY_NFComponent, resolver);
    }
} 