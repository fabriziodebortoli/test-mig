import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EXTACCTEMPLATE_COPYService } from './IDD_EXTACCTEMPLATE_COPY.service';

@Component({
    selector: 'tb-IDD_EXTACCTEMPLATE_COPY',
    templateUrl: './IDD_EXTACCTEMPLATE_COPY.component.html',
    providers: [IDD_EXTACCTEMPLATE_COPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_EXTACCTEMPLATE_COPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EXTACCTEMPLATE_COPYService,
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
        
        		this.bo.appendToModelStructure({'global':['ExtAccTemplate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EXTACCTEMPLATE_COPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EXTACCTEMPLATE_COPYComponent, resolver);
    }
} 