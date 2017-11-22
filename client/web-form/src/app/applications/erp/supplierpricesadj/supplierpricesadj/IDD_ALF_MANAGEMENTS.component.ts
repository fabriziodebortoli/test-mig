import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ALF_MANAGEMENTSService } from './IDD_ALF_MANAGEMENTS.service';

@Component({
    selector: 'tb-IDD_ALF_MANAGEMENTS',
    templateUrl: './IDD_ALF_MANAGEMENTS.component.html',
    providers: [IDD_ALF_MANAGEMENTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ALF_MANAGEMENTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ALF_MANAGEMENTSService,
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
		boService.appendToModelStructure({'global':['SupplierPricesAdj'],'SupplierPricesAdj':['Select','Item','Description','SupplierPr_ValueType','ExpectedPrice','UnitValue','SupplierPr_DiscountType','ExpectedDiscount','DiscountFormula','DocumentDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ALF_MANAGEMENTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ALF_MANAGEMENTSComponent, resolver);
    }
} 