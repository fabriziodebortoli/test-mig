import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CROSS_DOCKING_REBUILDINGService } from './IDD_CROSS_DOCKING_REBUILDING.service';

@Component({
    selector: 'tb-IDD_CROSS_DOCKING_REBUILDING',
    templateUrl: './IDD_CROSS_DOCKING_REBUILDING.component.html',
    providers: [IDD_CROSS_DOCKING_REBUILDINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CROSS_DOCKING_REBUILDINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CROSS_DOCKING_REBUILDINGService,
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
		boService.appendToModelStructure({'global':['nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CROSS_DOCKING_REBUILDINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CROSS_DOCKING_REBUILDINGComponent, resolver);
    }
} 