import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LEAD_TIME_CALCService } from './IDD_LEAD_TIME_CALC.service';

@Component({
    selector: 'tb-IDD_LEAD_TIME_CALC',
    templateUrl: './IDD_LEAD_TIME_CALC.component.html',
    providers: [IDD_LEAD_TIME_CALCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LEAD_TIME_CALCComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LEAD_TIME_CALCService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllSupp','SuppsSel','SuppStart','SuppEnd','AllItems','ItemsSel','FromItem','ToItem','AverageDelivery','AverageQtyDeliverd','LastDelivery','MinUsed','MaxUsed','LeadTimeCalculation'],'LeadTimeCalculation':['l_Selected','Supplier','Item','DaysForDelivery','l_RealLeadTime','l_Updated']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LEAD_TIME_CALCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LEAD_TIME_CALCComponent, resolver);
    }
} 