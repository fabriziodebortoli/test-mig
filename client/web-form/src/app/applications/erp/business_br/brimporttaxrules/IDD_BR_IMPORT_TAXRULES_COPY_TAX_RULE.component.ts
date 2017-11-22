import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULEService } from './IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULE.service';

@Component({
    selector: 'tb-IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULE',
    templateUrl: './IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULE.component.html',
    providers: [IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULEService,
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
		boService.appendToModelStructure({'global':['TaxRule']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_IMPORT_TAXRULES_COPY_TAX_RULEComponent, resolver);
    }
} 