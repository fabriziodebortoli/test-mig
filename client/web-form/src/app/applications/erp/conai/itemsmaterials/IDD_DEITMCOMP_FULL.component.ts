﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEITMCOMP_FULLService } from './IDD_DEITMCOMP_FULL.service';

@Component({
    selector: 'tb-IDD_DEITMCOMP_FULL',
    templateUrl: './IDD_DEITMCOMP_FULL.component.html',
    providers: [IDD_DEITMCOMP_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DEITMCOMP_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEITMCOMP_FULLService,
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
		boService.appendToModelStructure({'Items':['Item','TaxCode','UseDocumentWeight','ImportedMaterial'],'HKLItems':['Description'],'HKLTAX':['Description'],'global':['ItemsMaterials','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEITMCOMP_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEITMCOMP_FULLComponent, resolver);
    }
} 