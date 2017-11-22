import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_IMPORTService } from './IDD_IMPORT.service';

@Component({
    selector: 'tb-IDD_IMPORT',
    templateUrl: './IDD_IMPORT.component.html',
    providers: [IDD_IMPORTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_IMPORTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_IMPORTService,
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
		boService.appendToModelStructure({'global':['SlaveDataXBRL'],'SlaveDataXBRL':['l_TEnhPersonalDataXBRLImp_P3','l_TEnhPersonalDataXBRLImp_P1','l_TEnhPersonalDataXBRLImp_P2']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_IMPORTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_IMPORTComponent, resolver);
    }
} 