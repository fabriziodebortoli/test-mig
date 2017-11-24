import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALE_PRICE_POLICIES_PARAMETERSService } from './IDD_SALE_PRICE_POLICIES_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_SALE_PRICE_POLICIES_PARAMETERS',
    templateUrl: './IDD_SALE_PRICE_POLICIES_PARAMETERS.component.html',
    providers: [IDD_SALE_PRICE_POLICIES_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SALE_PRICE_POLICIES_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALE_PRICE_POLICIES_PARAMETERSService,
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
        
        		this.bo.appendToModelStructure({'global':['SalePolicy','SaleDiscountPolicy','SaleIncompatibility','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'SalePolicy':['Priority','CodeType','DiscountType','NotPrompt','Notes'],'SaleDiscountPolicy':['Priority','CodeType','NotPrompt','Notes'],'SaleIncompatibility':['ValueType','DiscountType']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALE_PRICE_POLICIES_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALE_PRICE_POLICIES_PARAMETERSComponent, resolver);
    }
} 