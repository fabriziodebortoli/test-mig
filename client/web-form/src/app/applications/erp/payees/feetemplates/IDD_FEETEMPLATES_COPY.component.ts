import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FEETEMPLATES_COPYService } from './IDD_FEETEMPLATES_COPY.service';

@Component({
    selector: 'tb-IDD_FEETEMPLATES_COPY',
    templateUrl: './IDD_FEETEMPLATES_COPY.component.html',
    providers: [IDD_FEETEMPLATES_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_FEETEMPLATES_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FEETEMPLATES_COPYService,
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
        
        		this.bo.appendToModelStructure({'global':['Template']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FEETEMPLATES_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FEETEMPLATES_COPYComponent, resolver);
    }
} 