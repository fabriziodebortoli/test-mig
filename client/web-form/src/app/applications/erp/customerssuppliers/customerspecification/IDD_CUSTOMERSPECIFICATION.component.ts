import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUSTOMERSPECIFICATIONService } from './IDD_CUSTOMERSPECIFICATION.service';

@Component({
    selector: 'tb-IDD_CUSTOMERSPECIFICATION',
    templateUrl: './IDD_CUSTOMERSPECIFICATION.component.html',
    providers: [IDD_CUSTOMERSPECIFICATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CUSTOMERSPECIFICATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CUSTOMERSPECIFICATIONService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'CustomerSpecification':['CustomerSpecification','Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUSTOMERSPECIFICATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUSTOMERSPECIFICATIONComponent, resolver);
    }
} 