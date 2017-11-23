import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEPRECIATIONINFORECASTACCOUNTINGService } from './IDD_DEPRECIATIONINFORECASTACCOUNTING.service';

@Component({
    selector: 'tb-IDD_DEPRECIATIONINFORECASTACCOUNTING',
    templateUrl: './IDD_DEPRECIATIONINFORECASTACCOUNTING.component.html',
    providers: [IDD_DEPRECIATIONINFORECASTACCOUNTINGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DEPRECIATIONINFORECASTACCOUNTINGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEPRECIATIONINFORECASTACCOUNTINGService,
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
        
        		this.bo.appendToModelStructure({'global':['Twelfth','SimulationDate','AllCtgs','CtgSel','FromCtg','ToCtg','TwelfthCalc','TwelfthCaption','DaysCalc','DaysCaption','AccPostingDate','AccrualDate','NrDoc','Values','SimulationCode','Process']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEPRECIATIONINFORECASTACCOUNTINGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEPRECIATIONINFORECASTACCOUNTINGComponent, resolver);
    }
} 