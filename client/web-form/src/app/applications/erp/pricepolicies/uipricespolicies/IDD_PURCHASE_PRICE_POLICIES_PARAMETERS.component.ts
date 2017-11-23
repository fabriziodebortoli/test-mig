import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHASE_PRICE_POLICIES_PARAMETERSService } from './IDD_PURCHASE_PRICE_POLICIES_PARAMETERS.service';

@Component({
    selector: 'tb-IDD_PURCHASE_PRICE_POLICIES_PARAMETERS',
    templateUrl: './IDD_PURCHASE_PRICE_POLICIES_PARAMETERS.component.html',
    providers: [IDD_PURCHASE_PRICE_POLICIES_PARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PURCHASE_PRICE_POLICIES_PARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHASE_PRICE_POLICIES_PARAMETERSService,
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
		boService.appendToModelStructure({'global':['PurchasePolicy','PurchaseDiscountPolicy','PurchaseIncompatibility','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'PurchasePolicy':['Priority','CodeType','DiscountType','NotPrompt','Notes'],'PurchaseDiscountPolicy':['Priority','CodeType','NotPrompt','Notes'],'PurchaseIncompatibility':['ValueType','DiscountType']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHASE_PRICE_POLICIES_PARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHASE_PRICE_POLICIES_PARAMETERSComponent, resolver);
    }
} 