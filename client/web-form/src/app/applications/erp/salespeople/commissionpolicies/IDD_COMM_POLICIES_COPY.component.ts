import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMM_POLICIES_COPYService } from './IDD_COMM_POLICIES_COPY.service';

@Component({
    selector: 'tb-IDD_COMM_POLICIES_COPY',
    templateUrl: './IDD_COMM_POLICIES_COPY.component.html',
    providers: [IDD_COMM_POLICIES_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COMM_POLICIES_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMM_POLICIES_COPYService,
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
		boService.appendToModelStructure({'global':['CommissionPolicy']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMM_POLICIES_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMM_POLICIES_COPYComponent, resolver);
    }
} 