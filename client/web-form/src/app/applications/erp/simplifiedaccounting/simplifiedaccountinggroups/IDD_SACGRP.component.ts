import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SACGRPService } from './IDD_SACGRP.service';

@Component({
    selector: 'tb-IDD_SACGRP',
    templateUrl: './IDD_SACGRP.component.html',
    providers: [IDD_SACGRPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SACGRPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SACGRPService,
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
		boService.appendToModelStructure({'global':['SimplifiedAccountingGroups','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SimplifiedAccountingGroups':['ColumnCode','l_TEnhSimplifiedAccGroups_P2','l_TEnhSimplifiedAccGroups_P3','l_TEnhSimplifiedAccGroups_P1','IgnoreDifferentSign']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SACGRPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SACGRPComponent, resolver);
    }
} 