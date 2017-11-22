import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CREDITLIMIT_REBUILDService } from './IDD_CREDITLIMIT_REBUILD.service';

@Component({
    selector: 'tb-IDD_CREDITLIMIT_REBUILD',
    templateUrl: './IDD_CREDITLIMIT_REBUILD.component.html',
    providers: [IDD_CREDITLIMIT_REBUILDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CREDITLIMIT_REBUILDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CREDITLIMIT_REBUILDService,
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
		boService.appendToModelStructure({'global':['bOrdersNotDelivered','bDeliveryDocuments','bInvoices','AllCustomer','CustomerSel','FromCustomer','ToCustomer','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CREDITLIMIT_REBUILDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CREDITLIMIT_REBUILDComponent, resolver);
    }
} 