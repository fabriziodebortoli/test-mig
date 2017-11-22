import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_FATEL_ADDITIONALDATAService } from './IDD_FATEL_ADDITIONALDATA.service';

@Component({
    selector: 'tb-IDD_FATEL_ADDITIONALDATA',
    templateUrl: './IDD_FATEL_ADDITIONALDATA.component.html',
    providers: [IDD_FATEL_ADDITIONALDATAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_FATEL_ADDITIONALDATAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_FATEL_ADDITIONALDATAService,
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
		boService.appendToModelStructure({'global':['bCompanyData','bTranscodingData','nCurrentElement','GaugeDescription']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_FATEL_ADDITIONALDATAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_FATEL_ADDITIONALDATAComponent, resolver);
    }
} 