import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_RECALCULATIONS_EXIGIBILIETAXService } from './IDD_RECALCULATIONS_EXIGIBILIETAX.service';

@Component({
    selector: 'tb-IDD_RECALCULATIONS_EXIGIBILIETAX',
    templateUrl: './IDD_RECALCULATIONS_EXIGIBILIETAX.component.html',
    providers: [IDD_RECALCULATIONS_EXIGIBILIETAXService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_RECALCULATIONS_EXIGIBILIETAXComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_RECALCULATIONS_EXIGIBILIETAXService,
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
		boService.appendToModelStructure({'global':['StartMonth','EndMonth','StartMonth','FromYear','EndMonth','ToYear','FromYear','EndMonth','ToYear','EndMonth','ToYear','Process']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_RECALCULATIONS_EXIGIBILIETAXFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_RECALCULATIONS_EXIGIBILIETAXComponent, resolver);
    }
} 