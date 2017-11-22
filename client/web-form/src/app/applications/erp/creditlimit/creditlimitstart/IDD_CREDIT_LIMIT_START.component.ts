import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CREDIT_LIMIT_STARTService } from './IDD_CREDIT_LIMIT_START.service';

@Component({
    selector: 'tb-IDD_CREDIT_LIMIT_START',
    templateUrl: './IDD_CREDIT_LIMIT_START.component.html',
    providers: [IDD_CREDIT_LIMIT_STARTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CREDIT_LIMIT_STARTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CREDIT_LIMIT_STARTService,
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
		boService.appendToModelStructure({'global':['bAllCustomer','bCustomerSel','CustomerStart','CustomerEnd','bAllCustomerCategory','bCustomerCategorySel','CustomerCategoryStart','CustomerCategoryEnd','bFromParameters','bFromDataInGrid','bFromBuiltIn','MaxOrderValueCopy','MaxOrderValueCheckTypeCopy','MaximumCreditCopy','MaximumCreditCheckTypeDefInvCopy','MaximumCreditCheckTypeCopy','MaximumCreditCheckTypeImmInvCopy','MaximumCreditCheckTypeDelDocCopy','MaxOrderedValueCopy','MaxOrderedValueCheckTypeCopy','TotalExposureCopy','TotalExposureCheckTypeDefInvCopy','TotalExposureCheckTypeCopy','TotalExposureCheckTypeImmInvCopy','TotalExposureCheckTypeDelDocCopy','MaxOrderValue','MaxOrderValueCheckType','MaximumCredit','MaximumCreditCheckTypeDefInv','MaximumCreditCheckType','MaximumCreditCheckTypeImmInv','MaximumCreditCheckTypeDelDoc','MaxOrderedValue','MaxOrderedValueCheckType','TotalExposure','TotalExposureCheckTypeDefInv','TotalExposureCheckType','TotalExposureCheckTypeImmInv','TotalExposureCheckTypeDelDoc','nCurrentElement']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CREDIT_LIMIT_STARTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CREDIT_LIMIT_STARTComponent, resolver);
    }
} 