import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WHOLDINGPAY_FEESService } from './IDD_WHOLDINGPAY_FEES.service';

@Component({
    selector: 'tb-IDD_WHOLDINGPAY_FEES',
    templateUrl: './IDD_WHOLDINGPAY_FEES.component.html',
    providers: [IDD_WHOLDINGPAY_FEESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WHOLDINGPAY_FEESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WHOLDINGPAY_FEESService,
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
        
        		this.bo.appendToModelStructure({'global':['PaymentDate','EffectiveDate','Method','BigStateProc','TaxAmount']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WHOLDINGPAY_FEESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WHOLDINGPAY_FEESComponent, resolver);
    }
} 