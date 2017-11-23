import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ALLOWANCECALCULATIONService } from './IDD_ALLOWANCECALCULATION.service';

@Component({
    selector: 'tb-IDD_ALLOWANCECALCULATION',
    templateUrl: './IDD_ALLOWANCECALCULATION.component.html',
    providers: [IDD_ALLOWANCECALCULATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ALLOWANCECALCULATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ALLOWANCECALCULATIONService,
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
		boService.appendToModelStructure({'global':['bAllSalesPeople','bSalesPeopleSel','FromSalesperson','ToSalesperson','FromComm','FromAcquired','Process','SalespersonProcess']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ALLOWANCECALCULATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ALLOWANCECALCULATIONComponent, resolver);
    }
} 