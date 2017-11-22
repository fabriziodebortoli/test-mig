import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_TAX_COMMUNICATION_GROUPSService } from './IDD_TAX_COMMUNICATION_GROUPS.service';

@Component({
    selector: 'tb-IDD_TAX_COMMUNICATION_GROUPS',
    templateUrl: './IDD_TAX_COMMUNICATION_GROUPS.component.html',
    providers: [IDD_TAX_COMMUNICATION_GROUPSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_TAX_COMMUNICATION_GROUPSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_TAX_COMMUNICATION_GROUPSService,
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
		boService.appendToModelStructure({'TaxCommunicationGroup':['TaxCommunicationGroup','Description','Notes','TaxableAmount','CustSuppType','CustSupp'],'global':['ComboStr','CompName','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_TAX_COMMUNICATION_GROUPSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_TAX_COMMUNICATION_GROUPSComponent, resolver);
    }
} 