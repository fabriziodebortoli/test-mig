import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_POSTING_QUALITYINSPECTIONService } from './IDD_POSTING_QUALITYINSPECTION.service';

@Component({
    selector: 'tb-IDD_POSTING_QUALITYINSPECTION',
    templateUrl: './IDD_POSTING_QUALITYINSPECTION.component.html',
    providers: [IDD_POSTING_QUALITYINSPECTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_POSTING_QUALITYINSPECTIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_POSTING_QUALITYINSPECTIONService,
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
        
        		this.bo.appendToModelStructure({'global':['InspectionOrder','InspectionNotes','StartingDate','EndingDate','AllSupp','SuppsSel','SuppStart','SuppEnd','AllNo','NoSel','FromNo','ToNo','AllCancelled','NotCancelled','Cancelled','AllPrinted','NoPrinted','Printed','OrderedBySupplier','OrderedByNo','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_POSTING_QUALITYINSPECTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_POSTING_QUALITYINSPECTIONComponent, resolver);
    }
} 