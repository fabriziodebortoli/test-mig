import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MO_COMP_REPLService } from './IDD_MO_COMP_REPL.service';

@Component({
    selector: 'tb-IDD_MO_COMP_REPL',
    templateUrl: './IDD_MO_COMP_REPL.component.html',
    providers: [IDD_MO_COMP_REPLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_MO_COMP_REPLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SOST_JOLLY_UNIT_OF_MISURE_itemSource: any;

    constructor(document: IDD_MO_COMP_REPLService,
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
        this.IDC_SOST_JOLLY_UNIT_OF_MISURE_itemSource = {
  "name": "UnitsOfMeasureJollyComboBox",
  "namespace": "ERP.Manufacturing.Documents.UnitsOfMeasureJollyComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllMO','bMOSel','FromMO','ToMO','bAllComponents','bMOComponentsSel','FromComponent','ToComponent','bAllDate','bDateSel','FromDate','ToDate','bMOComponentsOnly','ComponentsDetail'],'ComponentsDetail':['Selection','MONo','Component','ComponentsDes','UoM','NeededQty','DeliveryDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MO_COMP_REPLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MO_COMP_REPLComponent, resolver);
    }
} 