import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACTUAL_COSTS_CALCService } from './IDD_ACTUAL_COSTS_CALC.service';

@Component({
    selector: 'tb-IDD_ACTUAL_COSTS_CALC',
    templateUrl: './IDD_ACTUAL_COSTS_CALC.component.html',
    providers: [IDD_ACTUAL_COSTS_CALCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACTUAL_COSTS_CALCComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACTUAL_COSTS_CALCService,
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
		boService.appendToModelStructure({'global':['MOSel','FromMO','ToMO','JobSel','FromSaleJob','ToSaleJob','DateSel','FromDate','ToDate','UseStepCosts','UseFastMethod','DisplayExtendedMess','MODateConfirmationSel','FromMODateConfirmation','ToMODateConfirmation','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACTUAL_COSTS_CALCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACTUAL_COSTS_CALCComponent, resolver);
    }
} 