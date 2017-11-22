import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BOMGRAPH_FIND_COMPService } from './IDD_BOMGRAPH_FIND_COMP.service';

@Component({
    selector: 'tb-IDD_BOMGRAPH_FIND_COMP',
    templateUrl: './IDD_BOMGRAPH_FIND_COMP.component.html',
    providers: [IDD_BOMGRAPH_FIND_COMPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BOMGRAPH_FIND_COMPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BOMGRAPH_FIND_COMPService,
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
		boService.appendToModelStructure({'global':['FindComponent']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BOMGRAPH_FIND_COMPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BOMGRAPH_FIND_COMPComponent, resolver);
    }
} 