﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LIFOFIFOService } from './IDD_LIFOFIFO.service';

@Component({
    selector: 'tb-IDD_LIFOFIFO',
    templateUrl: './IDD_LIFOFIFO.component.html',
    providers: [IDD_LIFOFIFOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LIFOFIFOComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LIFOFIFOService,
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
		boService.appendToModelStructure({'LIFOFIFOItems':['Item','Description'],'global':['LIFO','FIFO','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'LIFO':['OpeningDate','ClosingDate'],'FIFO':['OpeningDate','ClosingDate','RevaluationDone']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LIFOFIFOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LIFOFIFOComponent, resolver);
    }
} 