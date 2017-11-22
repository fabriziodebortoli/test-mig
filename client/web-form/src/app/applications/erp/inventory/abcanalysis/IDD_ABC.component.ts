import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ABCService } from './IDD_ABC.service';

@Component({
    selector: 'tb-IDD_ABC',
    templateUrl: './IDD_ABC.component.html',
    providers: [IDD_ABCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ABCComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_ABC_CODETYPE_COST_itemSource: any;

    constructor(document: IDD_ABCService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_ABC_CODETYPE_COST_itemSource = {
  "name": "CostTypeCombo",
  "namespace": "ERP.Inventory.Components.AbcAnalysisValuationTypeCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['FiscalYear','CostType','Limit','ABCCalc','UpdateItems','NotPurch','DisableItems','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ABCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ABCComponent, resolver);
    }
} 