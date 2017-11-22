import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SUPP_ITEMTYPE_FULLService } from './IDD_SUPP_ITEMTYPE_FULL.service';

@Component({
    selector: 'tb-IDD_SUPP_ITEMTYPE_FULL',
    templateUrl: './IDD_SUPP_ITEMTYPE_FULL.component.html',
    providers: [IDD_SUPP_ITEMTYPE_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SUPP_ITEMTYPE_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SUPP_ITEMTYPE_FULLService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'ItemTypeSuppliers':['ItemType','Supplier','DiscountFormula'],'HKLItemType':['Description','DiscountFormula'],'HKLSuppItemType':['CompanyName'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SUPP_ITEMTYPE_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SUPP_ITEMTYPE_FULLComponent, resolver);
    }
} 