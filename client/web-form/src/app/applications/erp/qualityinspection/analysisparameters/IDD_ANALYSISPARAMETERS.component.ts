import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ANALYSISPARAMETERSService } from './IDD_ANALYSISPARAMETERS.service';

@Component({
    selector: 'tb-IDD_ANALYSISPARAMETERS',
    templateUrl: './IDD_ANALYSISPARAMETERS.component.html',
    providers: [IDD_ANALYSISPARAMETERSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ANALYSISPARAMETERSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ANALYSISPARAMETERSService,
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
		boService.appendToModelStructure({'Parameters':['Parameter','Description','UoM','AnalysisArea','AnalysisMethod'],'HKLMethod':['Description'],'global':['ParametersResults','ParametersDesc','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'ParametersResults':['Result'],'HKLResult':['Description'],'ParametersDesc':['Language','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ANALYSISPARAMETERSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ANALYSISPARAMETERSComponent, resolver);
    }
} 