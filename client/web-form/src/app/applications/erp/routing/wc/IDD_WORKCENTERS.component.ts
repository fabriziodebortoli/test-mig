import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WORKCENTERSService } from './IDD_WORKCENTERS.service';

@Component({
    selector: 'tb-IDD_WORKCENTERS',
    templateUrl: './IDD_WORKCENTERS.component.html',
    providers: [IDD_WORKCENTERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WORKCENTERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WORKCENTERSService,
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
		boService.appendToModelStructure({'WC':['WC','Description','AverageCapacity','ResourceNo','Calendar','Notes','WorkType','ManagerID','Outsourced','Supplier','HourlyCost','UnitCost','AdditionalCost','Template','Make','PurchaseDate','PlacedInServiceDate'],'HKLWorkersManager':['NameComplete'],'HKLSuppliers':['CompanyName'],'global':['BreakdownWC','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WORKCENTERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WORKCENTERSComponent, resolver);
    }
} 