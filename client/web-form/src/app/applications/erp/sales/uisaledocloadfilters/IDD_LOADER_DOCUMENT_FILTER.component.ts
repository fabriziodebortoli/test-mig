import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOADER_DOCUMENT_FILTERService } from './IDD_LOADER_DOCUMENT_FILTER.service';

@Component({
    selector: 'tb-IDD_LOADER_DOCUMENT_FILTER',
    templateUrl: './IDD_LOADER_DOCUMENT_FILTER.component.html',
    providers: [IDD_LOADER_DOCUMENT_FILTERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOADER_DOCUMENT_FILTERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOADER_DOCUMENT_FILTERService,
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
		boService.appendToModelStructure({'global':['RFCFilter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOADER_DOCUMENT_FILTERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOADER_DOCUMENT_FILTERComponent, resolver);
    }
} 