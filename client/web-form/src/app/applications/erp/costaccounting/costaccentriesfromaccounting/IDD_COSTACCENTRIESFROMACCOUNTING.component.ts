﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COSTACCENTRIESFROMACCOUNTINGService } from './IDD_COSTACCENTRIESFROMACCOUNTING.service';

@Component({
    selector: 'tb-IDD_COSTACCENTRIESFROMACCOUNTING',
    templateUrl: './IDD_COSTACCENTRIESFROMACCOUNTING.component.html',
    providers: [IDD_COSTACCENTRIESFROMACCOUNTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COSTACCENTRIESFROMACCOUNTINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COSTACCENTRIESFROMACCOUNTINGService,
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
		boService.appendToModelStructure({'global':['AccFromPostDate','AccToPostDate','AllJE','JESel','PureJE','SaleJE','PurchaseJE','AllKind','Forecast','AllTemplates','SelTemplates','FromTemplate','ToTemplate','AccDateEqualDocDate','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COSTACCENTRIESFROMACCOUNTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COSTACCENTRIESFROMACCOUNTINGComponent, resolver);
    }
} 