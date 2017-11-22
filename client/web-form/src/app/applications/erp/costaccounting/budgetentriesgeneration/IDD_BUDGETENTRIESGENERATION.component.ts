import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BUDGETENTRIESGENERATIONService } from './IDD_BUDGETENTRIESGENERATION.service';

@Component({
    selector: 'tb-IDD_BUDGETENTRIESGENERATION',
    templateUrl: './IDD_BUDGETENTRIESGENERATION.component.html',
    providers: [IDD_BUDGETENTRIESGENERATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BUDGETENTRIESGENERATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BUDGETENTRIESGENERATIONService,
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
		boService.appendToModelStructure({'global':['nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BUDGETENTRIESGENERATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BUDGETENTRIESGENERATIONComponent, resolver);
    }
} 