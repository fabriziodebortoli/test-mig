import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHORD_ACTUALService } from './IDD_PURCHORD_ACTUAL.service';

@Component({
    selector: 'tb-IDD_PURCHORD_ACTUAL',
    templateUrl: './IDD_PURCHORD_ACTUAL.component.html',
    providers: [IDD_PURCHORD_ACTUALService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCHORD_ACTUALComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHORD_ACTUALService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','AllSupplier','SupplierSel','FromSupplier','ToSupplier','ActualClear','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHORD_ACTUALFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHORD_ACTUALComponent, resolver);
    }
} 