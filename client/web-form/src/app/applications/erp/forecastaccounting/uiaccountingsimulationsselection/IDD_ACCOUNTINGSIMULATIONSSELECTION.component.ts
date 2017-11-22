import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCOUNTINGSIMULATIONSSELECTIONService } from './IDD_ACCOUNTINGSIMULATIONSSELECTION.service';

@Component({
    selector: 'tb-IDD_ACCOUNTINGSIMULATIONSSELECTION',
    templateUrl: './IDD_ACCOUNTINGSIMULATIONSSELECTION.component.html',
    providers: [IDD_ACCOUNTINGSIMULATIONSSELECTIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_ACCOUNTINGSIMULATIONSSELECTIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCOUNTINGSIMULATIONSSELECTIONService,
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
		boService.appendToModelStructure({'global':['FromPostDate','ToPostDate','FromSimulation','ToSimulation','bIncludeEmpty','AccountingSimulationsSelection'],'AccountingSimulationsSelection':['l_Selected','Simulation','Description','PostingDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCOUNTINGSIMULATIONSSELECTIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCOUNTINGSIMULATIONSSELECTIONComponent, resolver);
    }
} 