import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PROD_DEV_ANALYSISService } from './IDD_PROD_DEV_ANALYSIS.service';

@Component({
    selector: 'tb-IDD_PROD_DEV_ANALYSIS',
    templateUrl: './IDD_PROD_DEV_ANALYSIS.component.html',
    providers: [IDD_PROD_DEV_ANALYSISService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PROD_DEV_ANALYSISComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PROD_DEV_ANALYSISService,
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
        
        		this.bo.appendToModelStructure({'global':['bCreateSubcntOrd','bSeparateOrderForMO','bCreateSubcntDN']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PROD_DEV_ANALYSISFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PROD_DEV_ANALYSISComponent, resolver);
    }
} 