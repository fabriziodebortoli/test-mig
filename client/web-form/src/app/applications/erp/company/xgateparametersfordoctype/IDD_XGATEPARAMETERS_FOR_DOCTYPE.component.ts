import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_XGATEPARAMETERS_FOR_DOCTYPEService } from './IDD_XGATEPARAMETERS_FOR_DOCTYPE.service';

@Component({
    selector: 'tb-IDD_XGATEPARAMETERS_FOR_DOCTYPE',
    templateUrl: './IDD_XGATEPARAMETERS_FOR_DOCTYPE.component.html',
    providers: [IDD_XGATEPARAMETERS_FOR_DOCTYPEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_XGATEPARAMETERS_FOR_DOCTYPEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_XGATEPARAMETERS_FOR_DOCTYPEService,
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
export class IDD_XGATEPARAMETERS_FOR_DOCTYPEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_XGATEPARAMETERS_FOR_DOCTYPEComponent, resolver);
    }
} 