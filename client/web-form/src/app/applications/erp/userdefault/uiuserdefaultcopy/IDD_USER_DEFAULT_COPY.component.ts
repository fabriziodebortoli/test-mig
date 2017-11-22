import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_USER_DEFAULT_COPYService } from './IDD_USER_DEFAULT_COPY.service';

@Component({
    selector: 'tb-IDD_USER_DEFAULT_COPY',
    templateUrl: './IDD_USER_DEFAULT_COPY.component.html',
    providers: [IDD_USER_DEFAULT_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_USER_DEFAULT_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_USER_DEFAULT_COPYService,
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
		boService.appendToModelStructure({'global':['UsrDftCpyBranch','UsrDftCpyWorkerID']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_USER_DEFAULT_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_USER_DEFAULT_COPYComponent, resolver);
    }
} 