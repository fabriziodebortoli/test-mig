import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BSP_THREADS_MANAGERService } from './IDD_BSP_THREADS_MANAGER.service';

@Component({
    selector: 'tb-IDD_BSP_THREADS_MANAGER',
    templateUrl: './IDD_BSP_THREADS_MANAGER.component.html',
    providers: [IDD_BSP_THREADS_MANAGERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BSP_THREADS_MANAGERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BSP_THREADS_MANAGERService,
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
export class IDD_BSP_THREADS_MANAGERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BSP_THREADS_MANAGERComponent, resolver);
    }
} 