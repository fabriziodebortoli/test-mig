import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_TAXMESSAGESService } from './IDD_BR_TAXMESSAGES.service';

@Component({
    selector: 'tb-IDD_BR_TAXMESSAGES',
    templateUrl: './IDD_BR_TAXMESSAGES.component.html',
    providers: [IDD_BR_TAXMESSAGESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BR_TAXMESSAGESComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_TAXMESSAGESService,
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
		boService.appendToModelStructure({'DBTBRTaxMessages':['TaxMessageCode','Disabled','Description1','Description2','Description3','LongDescription'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_TAXMESSAGESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_TAXMESSAGESComponent, resolver);
    }
} 