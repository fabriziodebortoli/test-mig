import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EI_DOC_ELABORATION_STATUSService } from './IDD_EI_DOC_ELABORATION_STATUS.service';

@Component({
    selector: 'tb-IDD_EI_DOC_ELABORATION_STATUS',
    templateUrl: './IDD_EI_DOC_ELABORATION_STATUS.component.html',
    providers: [IDD_EI_DOC_ELABORATION_STATUSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EI_DOC_ELABORATION_STATUSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EI_DOC_ELABORATION_STATUSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EI_DOC_ELABORATION_STATUSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EI_DOC_ELABORATION_STATUSComponent, resolver);
    }
} 