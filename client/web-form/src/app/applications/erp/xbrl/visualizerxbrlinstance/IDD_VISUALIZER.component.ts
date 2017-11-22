import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_VISUALIZERService } from './IDD_VISUALIZER.service';

@Component({
    selector: 'tb-IDD_VISUALIZER',
    templateUrl: './IDD_VISUALIZER.component.html',
    providers: [IDD_VISUALIZERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_VISUALIZERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_VISUALIZERService,
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
		boService.appendToModelStructure({'global':['Path','SlaveDataXBRL'],'SlaveDataXBRL':['l_TEnhPersonalDataXBRLVis_P3','l_TEnhPersonalDataXBRLVis_P2','l_TEnhPersonalDataXBRLVis_P1','l_TEnhPersonalDataXBRLVis_P4']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_VISUALIZERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_VISUALIZERComponent, resolver);
    }
} 