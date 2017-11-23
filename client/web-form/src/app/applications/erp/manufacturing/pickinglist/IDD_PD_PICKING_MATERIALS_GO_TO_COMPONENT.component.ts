import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTService } from './IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENT.service';

@Component({
    selector: 'tb-IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENT',
    templateUrl: './IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENT.component.html',
    providers: [IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTService,
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
		boService.appendToModelStructure({'global':['ComponentSearch']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTComponent, resolver);
    }
} 