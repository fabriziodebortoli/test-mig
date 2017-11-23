import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WEEE_CALCULATIONService } from './IDD_WEEE_CALCULATION.service';

@Component({
    selector: 'tb-IDD_WEEE_CALCULATION',
    templateUrl: './IDD_WEEE_CALCULATION.component.html',
    providers: [IDD_WEEE_CALCULATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WEEE_CALCULATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WEEE_CALCULATIONService,
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
		boService.appendToModelStructure({'CalculationWEEE':['DocNo','CustSupp'],'global':['DetailWEEE'],'DetailWEEE':['Item','Description','Qty','UnitValue','TaxableAmount','TaxCode','Offset','WEEECategory'],'HKLItems':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WEEE_CALCULATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WEEE_CALCULATIONComponent, resolver);
    }
} 