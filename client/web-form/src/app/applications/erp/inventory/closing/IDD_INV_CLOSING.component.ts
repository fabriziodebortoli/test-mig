import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_INV_CLOSINGService } from './IDD_INV_CLOSING.service';

@Component({
    selector: 'tb-IDD_INV_CLOSING',
    templateUrl: './IDD_INV_CLOSING.component.html',
    providers: [IDD_INV_CLOSINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_INV_CLOSINGComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CLOSEDINV_CODETYPE_COST_itemSource: any;

    constructor(document: IDD_INV_CLOSINGService,
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
        this.IDC_CLOSEDINV_CODETYPE_COST_itemSource = {
  "name": "ValueTypeCombo",
  "namespace": "ERP.Company.Components.ValuationInventoryCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['AlignItemCosts','InitialInventoryBalances','Valuate','UseItemDefaultValuation','ValueType','bEvaluateByLot','ValueType','bEvaluateByLot','BlockInv','bIgnoreDisabledItems','DBTSummaryDetail'],'DBTSummaryDetail':['l_LineSummaryDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_INV_CLOSINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_INV_CLOSINGComponent, resolver);
    }
} 