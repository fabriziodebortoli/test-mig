import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_INSPECTION_ORDERService } from './IDD_LOAD_INSPECTION_ORDER.service';

@Component({
    selector: 'tb-IDD_LOAD_INSPECTION_ORDER',
    templateUrl: './IDD_LOAD_INSPECTION_ORDER.component.html',
    providers: [IDD_LOAD_INSPECTION_ORDERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOAD_INSPECTION_ORDERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_INSPECTION_ORDERService,
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
		boService.appendToModelStructure({'InspectionOrderLoading':['InspectionOrderNo','InspectionOrderDate','ExpectedInspectionDate','Supplier'],'HKLMasterSupplier':['CompNameComplete'],'global':['InspectionOrderDetailLoading'],'InspectionOrderDetailLoading':['Selected','Item','Description','UoM','ConformingQty','ToBeReturnedQty','ScrapQty','NoPrint','NoRiepOnInspNotes','Lot']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_INSPECTION_ORDERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_INSPECTION_ORDERComponent, resolver);
    }
} 