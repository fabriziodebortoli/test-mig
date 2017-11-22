import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RUN_QTY_TO_PRODService } from './IDD_RUN_QTY_TO_PROD.service';

@Component({
    selector: 'tb-IDD_RUN_QTY_TO_PROD',
    templateUrl: './IDD_RUN_QTY_TO_PROD.component.html',
    providers: [IDD_RUN_QTY_TO_PRODService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RUN_QTY_TO_PRODComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RUN_QTY_TO_PRODService,
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
		boService.appendToModelStructure({'global':['DlgMO','DlgBOM','DlgSaleOrder','DlgCustomer','DlgJob','DlgQtyToProd']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RUN_QTY_TO_PRODFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RUN_QTY_TO_PRODComponent, resolver);
    }
} 