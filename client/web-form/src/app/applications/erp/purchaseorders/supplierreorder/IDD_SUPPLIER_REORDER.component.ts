import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SUPPLIER_REORDERService } from './IDD_SUPPLIER_REORDER.service';

@Component({
    selector: 'tb-IDD_SUPPLIER_REORDER',
    templateUrl: './IDD_SUPPLIER_REORDER.component.html',
    providers: [IDD_SUPPLIER_REORDERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SUPPLIER_REORDERComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_GENPURCORD_CODETYPE_SPECIFICATOR_itemSource: any;

    constructor(document: IDD_SUPPLIER_REORDERService,
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
        this.IDC_GENPURCORD_CODETYPE_SPECIFICATOR_itemSource = {
  "name": "SpecificatorTypeCombo",
  "namespace": "ERP.Inventory.Components.SpecTypeNoIgnoreCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Restore','Integrate','Overwrites','bSaveSettings','AllPriority','PrioritySel','FromPriority','ToPriority','Shortage','Insufficiency','StandardPrice','LastPrice','DefaultPrice','EndingDate','AllOrdNo','OrdNoSel','FromOrdNo','ToOrdNo','AllItems','ItemsSel','FromItem','ToItem','AllSupp','SuppsSel','SuppStart','SuppEnd','AllProductionJob','ProductionJobSel','FromProducitonJob','ToProductionJob','GenInvSel','StorageSel','Storage','SpecificatorType','Specificator','ItemsWithoutSupp','IgnoreStock','IgnorePurchaseOrd','GroupOrders','bOnlyItemsWithMinimumStock','ItemsShortage','bBlockedOrders','ItemsBoM','SupplierSelection','Suppliers'],'SupplierSelection':['SupplierSe_Sel','Supplier','SupplierDes','Storage','SpecificatorType','Specificator','Item','SupplierCode','Description','ToOrder','UoM','BaseUoM','ReservationType','ReferenceDocNo','Position'],'Suppliers':['Suppliers_Sel','Supplier','CompanyName','Storage','SpecificatorType','Specificator','Address','City']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SUPPLIER_REORDERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SUPPLIER_REORDERComponent, resolver);
    }
} 