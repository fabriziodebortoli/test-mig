import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_MO_STEPS_OUTSOURCEDService } from './IDD_MO_STEPS_OUTSOURCED.service';

@Component({
    selector: 'tb-IDD_MO_STEPS_OUTSOURCED',
    templateUrl: './IDD_MO_STEPS_OUTSOURCED.component.html',
    providers: [IDD_MO_STEPS_OUTSOURCEDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_MO_STEPS_OUTSOURCEDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_MO_STEPS_OUTSOURCEDService,
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
		boService.appendToModelStructure({'global':['bAllSteps','Supplier','SupplierCompanyName','SubcontactorOrdMOSteps'],'SubcontactorOrdMOSteps':['StateBmp','Selection','MONo','RtgStep','Alternate','AltRtgStep','BOM','UoM','ProductionQty','SubcontractorOrderQuantity','QtyToOrder','Supplier','Operation','SupplierCompanyName','BOMDescri','MOStatus','Job','Customer','SaleOrdNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_MO_STEPS_OUTSOURCEDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_MO_STEPS_OUTSOURCEDComponent, resolver);
    }
} 