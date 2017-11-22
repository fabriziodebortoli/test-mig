import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCBOOKATTACHMENTSService } from './IDD_ACCBOOKATTACHMENTS.service';

@Component({
    selector: 'tb-IDD_ACCBOOKATTACHMENTS',
    templateUrl: './IDD_ACCBOOKATTACHMENTS.component.html',
    providers: [IDD_ACCBOOKATTACHMENTSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACCBOOKATTACHMENTSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCBOOKATTACHMENTSService,
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
		boService.appendToModelStructure({'AccBookAttachments':['AttachCode','Disabled','Description','PrintOrder','IsAReport','ReportNamespace','TableTitle','PrintOnlyColDescri','PrintSignColumn','PrintTotal','PrintAccountColumns'],'global':['AccBookAttachmentsDetail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'AccBookAttachmentsDetail':['Description','LineType','Amount','Account'],'HKLAccount':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCBOOKATTACHMENTSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCBOOKATTACHMENTSComponent, resolver);
    }
} 