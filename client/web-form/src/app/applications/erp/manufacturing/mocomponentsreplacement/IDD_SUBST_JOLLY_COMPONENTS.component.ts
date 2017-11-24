import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SUBST_JOLLY_COMPONENTSService } from './IDD_SUBST_JOLLY_COMPONENTS.service';

@Component({
    selector: 'tb-IDD_SUBST_JOLLY_COMPONENTS',
    templateUrl: './IDD_SUBST_JOLLY_COMPONENTS.component.html',
    providers: [IDD_SUBST_JOLLY_COMPONENTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SUBST_JOLLY_COMPONENTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SUBST_JOLLY_COMPONENTSService,
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
        
        		this.bo.appendToModelStructure({'global':['BOMComponentsReplacement']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SUBST_JOLLY_COMPONENTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SUBST_JOLLY_COMPONENTSComponent, resolver);
    }
} 