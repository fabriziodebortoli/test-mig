import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CALC_PROD_LTService } from './IDD_CALC_PROD_LT.service';

@Component({
    selector: 'tb-IDD_CALC_PROD_LT',
    templateUrl: './IDD_CALC_PROD_LT.component.html',
    providers: [IDD_CALC_PROD_LTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CALC_PROD_LTComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CALC_PROD_LT_DAYS_ROUNDING_TYPE_itemSource: any;

    constructor(document: IDD_CALC_PROD_LTService,
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
        this.IDC_CALC_PROD_LT_DAYS_ROUNDING_TYPE_itemSource = {
  "name": "eRoundingEnumCombo",
  "namespace": "ERP.BillOfMaterials.Components.EnumRoundingTypeItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['bAllMO','bOrdSel','FromOrd','ToOrd','bAllProd','bProdSel','FromProd','ToProd','bAllVariants','bVariantsSel','FromVariant','ToVariant','bAllJobs','bJobSel','FromJob','ToJob','bAllDeliveryDates','bSelData','FromMODate','ToMODate','eRounding','nDigits','nHoursInDay','DBTCalculateProductionLeadTime','InHouseCreated','OutsourcedCreated','InHouseReleased','OutsourcedReleased','InHouseProcessing','OutsourcedProcessing'],'DBTCalculateProductionLeadTime':['TMO_Selection','BOM','UoM','ProducedQtyForLT','SecondRateQuantityForLT','ScrapQuantityForLT','ActualProcessTimeForLT','ActualSetupTimeForLT','TotTimeForLeadTime','CalcTotQty','CalcLeadTime']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CALC_PROD_LTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CALC_PROD_LTComponent, resolver);
    }
} 