import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ADDITIONAL_CHARGES_PARAMSService } from './IDD_ADDITIONAL_CHARGES_PARAMS.service';

@Component({
    selector: 'tb-IDD_ADDITIONAL_CHARGES_PARAMS',
    templateUrl: './IDD_ADDITIONAL_CHARGES_PARAMS.component.html',
    providers: [IDD_ADDITIONAL_CHARGES_PARAMSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ADDITIONAL_CHARGES_PARAMSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ADDITIONAL_CHARGES_PARAMSService,
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
		boService.appendToModelStructure({'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ADDITIONAL_CHARGES_PARAMSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ADDITIONAL_CHARGES_PARAMSComponent, resolver);
    }
} 