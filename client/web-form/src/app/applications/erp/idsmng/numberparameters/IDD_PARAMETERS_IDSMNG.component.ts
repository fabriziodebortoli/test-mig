﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PARAMETERS_IDSMNGService } from './IDD_PARAMETERS_IDSMNG.service';

@Component({
    selector: 'tb-IDD_PARAMETERS_IDSMNG',
    templateUrl: './IDD_PARAMETERS_IDSMNG.component.html',
    providers: [IDD_PARAMETERS_IDSMNGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PARAMETERS_IDSMNGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PARAMETERS_IDSMNGService,
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
export class IDD_PARAMETERS_IDSMNGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PARAMETERS_IDSMNGComponent, resolver);
    }
} 