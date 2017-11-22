import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PICKING_MATERIALS_QTY_CLOSUREService } from './IDD_PICKING_MATERIALS_QTY_CLOSURE.service';

@Component({
    selector: 'tb-IDD_PICKING_MATERIALS_QTY_CLOSURE',
    templateUrl: './IDD_PICKING_MATERIALS_QTY_CLOSURE.component.html',
    providers: [IDD_PICKING_MATERIALS_QTY_CLOSUREService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PICKING_MATERIALS_QTY_CLOSUREComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PICKING_MATERIALS_QTY_CLOSUREService,
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
		boService.appendToModelStructure({'global':['ProductionQty','bAssignLotStorageOnPicking','bPLPickAlsoMissingMOComponents','bProductionQtyAllSteps','bProductionQtyUntilStep','bProductionQtyOnlyStep','ProductionQtyStep']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PICKING_MATERIALS_QTY_CLOSUREFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PICKING_MATERIALS_QTY_CLOSUREComponent, resolver);
    }
} 