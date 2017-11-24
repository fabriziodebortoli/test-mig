import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DELETE_SIMBOMCOSTService } from './IDD_DELETE_SIMBOMCOST.service';

@Component({
    selector: 'tb-IDD_DELETE_SIMBOMCOST',
    templateUrl: './IDD_DELETE_SIMBOMCOST.component.html',
    providers: [IDD_DELETE_SIMBOMCOSTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DELETE_SIMBOMCOSTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DELETE_SIMBOMCOSTService,
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
        
        		this.bo.appendToModelStructure({'global':['bBOMSimCostSelAll','bBOMSimCostSel','FromBOMSimCost','ToBOMSimCost','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DELETE_SIMBOMCOSTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DELETE_SIMBOMCOSTComponent, resolver);
    }
} 