import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WARMAN_INTERIM_ANALYSISService } from './IDD_WARMAN_INTERIM_ANALYSIS.service';

@Component({
    selector: 'tb-IDD_WARMAN_INTERIM_ANALYSIS',
    templateUrl: './IDD_WARMAN_INTERIM_ANALYSIS.component.html',
    providers: [IDD_WARMAN_INTERIM_ANALYSISService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WARMAN_INTERIM_ANALYSISComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WARMAN_INTERIM_ANALYSISService,
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
		boService.appendToModelStructure({'global':['bInterimIN','Storage','bInterimOUT','bItemAll','bItemSelection','sItemFrom','sItemTo','bTreeView']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WARMAN_INTERIM_ANALYSISFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WARMAN_INTERIM_ANALYSISComponent, resolver);
    }
} 