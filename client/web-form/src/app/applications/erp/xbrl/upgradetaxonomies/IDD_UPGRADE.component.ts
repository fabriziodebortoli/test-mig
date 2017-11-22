import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_UPGRADEService } from './IDD_UPGRADE.service';

@Component({
    selector: 'tb-IDD_UPGRADE',
    templateUrl: './IDD_UPGRADE.component.html',
    providers: [IDD_UPGRADEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_UPGRADEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_UPGRADEService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['TaxDetail'],'TaxDetail':['l_TEnhPersonalDataXBRL_P1','l_TEnhPersonalDataXBRL_P2','l_TEnhPersonalDataXBRL_P4','l_TEnhPersonalDataXBRL_P5','l_TEnhPersonalDataXBRL_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_UPGRADEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_UPGRADEComponent, resolver);
    }
} 