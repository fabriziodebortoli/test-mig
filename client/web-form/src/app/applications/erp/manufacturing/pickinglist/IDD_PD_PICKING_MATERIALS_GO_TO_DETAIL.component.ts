import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_PICKING_MATERIALS_GO_TO_DETAILService } from './IDD_PD_PICKING_MATERIALS_GO_TO_DETAIL.service';

@Component({
    selector: 'tb-IDD_PD_PICKING_MATERIALS_GO_TO_DETAIL',
    templateUrl: './IDD_PD_PICKING_MATERIALS_GO_TO_DETAIL.component.html',
    providers: [IDD_PD_PICKING_MATERIALS_GO_TO_DETAILService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PD_PICKING_MATERIALS_GO_TO_DETAILComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PD_PICKING_MATERIALS_GO_TO_DETAILService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['LineSearch']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_PICKING_MATERIALS_GO_TO_DETAILFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_PICKING_MATERIALS_GO_TO_DETAILComponent, resolver);
    }
} 