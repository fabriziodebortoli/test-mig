import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DWMSECTIONCATEGORYService } from './IDD_DWMSECTIONCATEGORY.service';

@Component({
    selector: 'tb-IDD_DWMSECTIONCATEGORY',
    templateUrl: './IDD_DWMSECTIONCATEGORY.component.html',
    providers: [IDD_DWMSECTIONCATEGORYService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_DWMSECTIONCATEGORYComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DWMSECTIONCATEGORYService,
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
		boService.appendToModelStructure({'DBTWMSectionCategory':['SectionCategory','Description','HazardousMaterial'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DWMSECTIONCATEGORYFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DWMSECTIONCATEGORYComponent, resolver);
    }
} 