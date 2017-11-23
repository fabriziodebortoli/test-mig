import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CREATEService } from './IDD_CREATE.service';

@Component({
    selector: 'tb-IDD_CREATE',
    templateUrl: './IDD_CREATE.component.html',
    providers: [IDD_CREATEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CREATEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CREATEService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['RadioNormal','RadioConsolidated','RadioAbbreviated','RadioMicro','RadioAbbreviatedS','SlaveDataXBRL'],'SlaveDataXBRL':['l_TEnhPersonalDataXBRLCre_P2','l_TEnhPersonalDataXBRLCre_P1','l_TEnhPersonalDataXBRLCre_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CREATEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CREATEComponent, resolver);
    }
} 