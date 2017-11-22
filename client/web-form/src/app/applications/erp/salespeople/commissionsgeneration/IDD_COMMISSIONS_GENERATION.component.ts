import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_COMMISSIONS_GENERATIONService } from './IDD_COMMISSIONS_GENERATION.service';

@Component({
    selector: 'tb-IDD_COMMISSIONS_GENERATION',
    templateUrl: './IDD_COMMISSIONS_GENERATION.component.html',
    providers: [IDD_COMMISSIONS_GENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_COMMISSIONS_GENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_COMMISSIONS_GENERATIONService,
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
		boService.appendToModelStructure({'global':['StartingDate','EndingDate','ToOutstandingDate','FromOutstandingDate','bAllSalesPeople','bSalesPeopleSel','FromSalesperson','ToSalesperson','bOutstandingProcess','bCommProcess','bCommRecalculation','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_COMMISSIONS_GENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_COMMISSIONS_GENERATIONComponent, resolver);
    }
} 