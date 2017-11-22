import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYROLLIMPORTService } from './IDD_PAYROLLIMPORT.service';

@Component({
    selector: 'tb-IDD_PAYROLLIMPORT',
    templateUrl: './IDD_PAYROLLIMPORT.component.html',
    providers: [IDD_PAYROLLIMPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PAYROLLIMPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PAYROLLIMPORTService,
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
		boService.appendToModelStructure({'global':['ImportMonth','ImportYear','JEDocDate','ImportPathFile','strFileLayout','bReasonImportButton','nCurrentElement','Status','nCurrentElement','GaugeDescription','ProgressViewer'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYROLLIMPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYROLLIMPORTComponent, resolver);
    }
} 