import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DECHARGESPOLICIES_FULLService } from './IDD_DECHARGESPOLICIES_FULL.service';

@Component({
    selector: 'tb-IDD_DECHARGESPOLICIES_FULL',
    templateUrl: './IDD_DECHARGESPOLICIES_FULL.component.html',
    providers: [IDD_DECHARGESPOLICIES_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DECHARGESPOLICIES_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DECHARGESPOLICIES_FULLService,
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
		boService.appendToModelStructure({'global':['AllCustomers','Shipping','Package','Areas','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Formula':['Customer','ShippingFormula','ShippingRounding','ShippingRoundingType','PackageFormula','PackageRounding','PackageRoundingType'],'HKLCustSupp':['CompNameComplete']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DECHARGESPOLICIES_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DECHARGESPOLICIES_FULLComponent, resolver);
    }
} 