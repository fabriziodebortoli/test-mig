import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_PRODPLANService } from './IDD_LOAD_PRODPLAN.service';

@Component({
    selector: 'tb-IDD_LOAD_PRODPLAN',
    templateUrl: './IDD_LOAD_PRODPLAN.component.html',
    providers: [IDD_LOAD_PRODPLANService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LOAD_PRODPLANComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_PRODPLANService,
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
        
        		this.bo.appendToModelStructure({'LoadProdPlan':['ProductionPlanNo','Description','Notes'],'global':['LoadProdPlanDetails'],'LoadProdPlanDetails':['Selected','Line','BOM','Variant','BoMDes','LocalUoM','ProductionQty','ExpectedDeliveryDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_PRODPLANFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_PRODPLANComponent, resolver);
    }
} 