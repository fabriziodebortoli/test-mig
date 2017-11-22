﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAMETERS_WMS_TOS_TRANSFERService } from './IDD_PARAMETERS_WMS_TOS_TRANSFER.service';

@Component({
    selector: 'tb-IDD_PARAMETERS_WMS_TOS_TRANSFER',
    templateUrl: './IDD_PARAMETERS_WMS_TOS_TRANSFER.component.html',
    providers: [IDD_PARAMETERS_WMS_TOS_TRANSFERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PARAMETERS_WMS_TOS_TRANSFERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PARAMETERS_WMS_TOS_TRANSFERService,
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
		boService.appendToModelStructure({'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PARAMETERS_WMS_TOS_TRANSFERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERS_WMS_TOS_TRANSFERComponent, resolver);
    }
} 