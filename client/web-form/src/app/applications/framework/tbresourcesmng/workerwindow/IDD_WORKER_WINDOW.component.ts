import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WORKER_WINDOWService } from './IDD_WORKER_WINDOW.service';

@Component({
    selector: 'tb-IDD_WORKER_WINDOW',
    templateUrl: './IDD_WORKER_WINDOW.component.html',
    providers: [IDD_WORKER_WINDOWService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WORKER_WINDOWComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WORKER_WINDOWService,
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
        
        		this.bo.appendToModelStructure({'global':['CreatedWorkerDes','CreatedDate','CreatedWorkerEmail','CreatedWorkerOfficePhone','CreatedWorkerPicture','ModifiedWorkerDes','ModifiedDate','ModifiedWorkerEmail','ModifiedWorkerOfficePhone','ModifiedWorkerPicture']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WORKER_WINDOWFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WORKER_WINDOWComponent, resolver);
    }
} 