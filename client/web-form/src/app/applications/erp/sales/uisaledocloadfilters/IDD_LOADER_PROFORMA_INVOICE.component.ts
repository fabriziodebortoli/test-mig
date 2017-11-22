import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOADER_PROFORMA_INVOICEService } from './IDD_LOADER_PROFORMA_INVOICE.service';

@Component({
    selector: 'tb-IDD_LOADER_PROFORMA_INVOICE',
    templateUrl: './IDD_LOADER_PROFORMA_INVOICE.component.html',
    providers: [IDD_LOADER_PROFORMA_INVOICEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOADER_PROFORMA_INVOICEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOADER_PROFORMA_INVOICEService,
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
		boService.appendToModelStructure({'global':['ProFormaInvoiceFilter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOADER_PROFORMA_INVOICEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOADER_PROFORMA_INVOICEComponent, resolver);
    }
} 