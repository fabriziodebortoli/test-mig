import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_UPDATE_ACTUAL_PURCHASEService } from './IDD_UPDATE_ACTUAL_PURCHASE.service';

@Component({
    selector: 'tb-IDD_UPDATE_ACTUAL_PURCHASE',
    templateUrl: './IDD_UPDATE_ACTUAL_PURCHASE.component.html',
    providers: [IDD_UPDATE_ACTUAL_PURCHASEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_UPDATE_ACTUAL_PURCHASEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_UPDATE_ACTUAL_PURCHASEService,
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
        
        		this.bo.appendToModelStructure({'global':['SelectiveStartingDate','SelectiveEndingDate','SelectiveAllSupp','SelectiveSuppsSel','SelectiveSuppStart','SelectiveSuppEnd','ActualClear','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_UPDATE_ACTUAL_PURCHASEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_UPDATE_ACTUAL_PURCHASEComponent, resolver);
    }
} 