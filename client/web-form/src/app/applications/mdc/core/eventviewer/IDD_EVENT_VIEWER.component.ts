import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EVENT_VIEWERService } from './IDD_EVENT_VIEWER.service';

@Component({
    selector: 'tb-IDD_EVENT_VIEWER',
    templateUrl: './IDD_EVENT_VIEWER.component.html',
    providers: [IDD_EVENT_VIEWERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_EVENT_VIEWERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_EVENT_VIEWERService,
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
		boService.appendToModelStructure({'global':['DBTEventViewer'],'DBTEventViewer':['EventDate','Event_Type','Event_Description','Event_XML','Event_String1','Event_String2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EVENT_VIEWERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EVENT_VIEWERComponent, resolver);
    }
} 