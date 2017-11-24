﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_ITEM_FISCALCTGService } from './IDD_BR_ITEM_FISCALCTG.service';

@Component({
    selector: 'tb-IDD_BR_ITEM_FISCALCTG',
    templateUrl: './IDD_BR_ITEM_FISCALCTG.component.html',
    providers: [IDD_BR_ITEM_FISCALCTGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_ITEM_FISCALCTGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_ITEM_FISCALCTGService,
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
        
        		this.bo.appendToModelStructure({'DBTBRItemFiscalCtg':['ItemFiscalCtg','Description','Disabled'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_ITEM_FISCALCTGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_ITEM_FISCALCTGComponent, resolver);
    }
} 