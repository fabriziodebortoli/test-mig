import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EIPAYMENTTYPEService } from './IDD_EIPAYMENTTYPE.service';

@Component({
    selector: 'tb-IDD_EIPAYMENTTYPE',
    templateUrl: './IDD_EIPAYMENTTYPE.component.html',
    providers: [IDD_EIPAYMENTTYPEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EIPAYMENTTYPEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EIPAYMENTTYPEService,
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
		boService.appendToModelStructure({'EIPaymentType':['PaymentType','EICode'],'HKLEICoding':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EIPAYMENTTYPEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EIPAYMENTTYPEComponent, resolver);
    }
} 