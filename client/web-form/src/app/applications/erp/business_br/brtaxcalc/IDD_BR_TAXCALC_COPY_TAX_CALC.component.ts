import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAXCALC_COPY_TAX_CALCService } from './IDD_BR_TAXCALC_COPY_TAX_CALC.service';

@Component({
    selector: 'tb-IDD_BR_TAXCALC_COPY_TAX_CALC',
    templateUrl: './IDD_BR_TAXCALC_COPY_TAX_CALC.component.html',
    providers: [IDD_BR_TAXCALC_COPY_TAX_CALCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_TAXCALC_COPY_TAX_CALCComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_TAXCALC_COPY_TAX_CALCService,
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
		boService.appendToModelStructure({'global':['TaxCalc']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAXCALC_COPY_TAX_CALCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAXCALC_COPY_TAX_CALCComponent, resolver);
    }
} 