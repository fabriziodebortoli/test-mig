import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCOUNTING_TYPEService } from './IDD_ACCOUNTING_TYPE.service';

@Component({
    selector: 'tb-IDD_ACCOUNTING_TYPE',
    templateUrl: './IDD_ACCOUNTING_TYPE.component.html',
    providers: [IDD_ACCOUNTING_TYPEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACCOUNTING_TYPEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCOUNTING_TYPEService,
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
		boService.appendToModelStructure({'DBTAccountingType':['AccountingType','Description','PurchaseOffset','SalesOffset','ConsumptionOffset'],'HKLPurchaseOffset':['Description'],'HKLSaleOffset':['Description'],'HKLConsumptionOffset':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCOUNTING_TYPEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCOUNTING_TYPEComponent, resolver);
    }
} 