import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_UNPACKINGService } from './IDD_UNPACKING.service';

@Component({
    selector: 'tb-IDD_UNPACKING',
    templateUrl: './IDD_UNPACKING.component.html',
    providers: [IDD_UNPACKINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_UNPACKINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_UNPACKINGService,
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
		boService.appendToModelStructure({'global':['Storage','Zone','Bin','Item','StorageUnit','LegendStorage','LegendStockWithoutSU','LegendStockWithSU','LegendSU']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_UNPACKINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_UNPACKINGComponent, resolver);
    }
} 