import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ADDITIONAL_CHARGES_LOADService } from './IDD_ADDITIONAL_CHARGES_LOAD.service';

@Component({
    selector: 'tb-IDD_ADDITIONAL_CHARGES_LOAD',
    templateUrl: './IDD_ADDITIONAL_CHARGES_LOAD.component.html',
    providers: [IDD_ADDITIONAL_CHARGES_LOADService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ADDITIONAL_CHARGES_LOADComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ADDITIONAL_CHARGES_LOADService,
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
		boService.appendToModelStructure({'global':['SuppDocNoFilter','SuppFilter']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ADDITIONAL_CHARGES_LOADFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ADDITIONAL_CHARGES_LOADComponent, resolver);
    }
} 