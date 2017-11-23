import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALE_ORD_EDITService } from './IDD_SALE_ORD_EDIT.service';

@Component({
    selector: 'tb-IDD_SALE_ORD_EDIT',
    templateUrl: './IDD_SALE_ORD_EDIT.component.html',
    providers: [IDD_SALE_ORD_EDITService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SALE_ORD_EDITComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALE_ORD_EDITService,
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
		boService.appendToModelStructure({'SaleOrdFulfilmentEditing':['InternalOrdNo','Customer'],'global':['SaleOrdFulfilmentDetailEditing'],'SaleOrdFulfilmentDetailEditing':['l_Sel','Position','CloseSaleOrdPos','LineType','ExpectedDeliveryDate','Item','Description','UoM','Qty','UnitValue','TaxCode','DiscountFormula','DiscountAmount','TaxableAmount','TotalAmount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALE_ORD_EDITFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALE_ORD_EDITComponent, resolver);
    }
} 