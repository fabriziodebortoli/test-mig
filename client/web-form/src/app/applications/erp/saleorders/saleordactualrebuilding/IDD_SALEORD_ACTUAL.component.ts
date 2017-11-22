import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALEORD_ACTUALService } from './IDD_SALEORD_ACTUAL.service';

@Component({
    selector: 'tb-IDD_SALEORD_ACTUAL',
    templateUrl: './IDD_SALEORD_ACTUAL.component.html',
    providers: [IDD_SALEORD_ACTUALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALEORD_ACTUALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALEORD_ACTUALService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllCustomer','CustomerSel','FromCustomer','ToCustomer','ActualClear','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALEORD_ACTUALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALEORD_ACTUALComponent, resolver);
    }
} 