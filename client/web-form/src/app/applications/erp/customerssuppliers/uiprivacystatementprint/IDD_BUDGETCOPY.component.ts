import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BUDGETCOPYService } from './IDD_BUDGETCOPY.service';

@Component({
    selector: 'tb-IDD_BUDGETCOPY',
    templateUrl: './IDD_BUDGETCOPY.component.html',
    providers: [IDD_BUDGETCOPYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BUDGETCOPYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BUDGETCOPYService,
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
		boService.appendToModelStructure({'global':['CustSupp','CustSuppAll','CustSuppSel','FromCode','FromCode','ToCode','Category','DescriCategory','CopyBudget','CopyActual','VarPerc','nCurrentElement','GaugeDescription'],'HKLFromCode':['CompanyName','CompanyName'],'HKLToCode':['CompanyName']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BUDGETCOPYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BUDGETCOPYComponent, resolver);
    }
} 